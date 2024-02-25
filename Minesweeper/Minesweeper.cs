namespace Minesweeper;

class Minesweeper
{
    static void Main()
    {
        int messageLine = 0;
        int boardHeight = 0;
        Console.ResetColor();
        while (true)
        {
            try
            {
                if (messageLine > 0) Console.SetCursorPosition(0, messageLine + 2);
                writeMessage(Console.CursorTop, "Enter E to Exit");
                writeMessage(Console.CursorTop, "Enter Rows Columns BombDensity for Mine Field for eg. 4 5 25 (4 Rows 5 Columns 25%  BombDensity) or press enter for default");

                var lineInput = Console.ReadLine();
                if (lineInput?.Trim().ToLower() == "e")
                {
                    Console.Clear();
                    break;
                }

                var mineFieldData = GetMineField(lineInput);
                if (mineFieldData.isValid == false) continue;

                if (mineFieldData.mineField == null)
                {
                    writeMessage(Console.CursorTop, "Unable to create Minefield", isWarning: true);
                    continue;
                }

                Minefield field = mineFieldData.mineField;
                Console.Clear();
                printBoard(field);
                printBoard(field,false,true);
                boardHeight = Console.CursorTop;
                bool hasStepError = false;

                var linePositions = getCursorPositions();
                int instructionLine = linePositions.instructionLine;
                messageLine = linePositions.messageLine;

                while (true)
                {
                    writeMessage(instructionLine, "Enter E to Exit");
                    writeMessage(instructionLine + 1, "");
                    writeMessage(instructionLine + 1, "Enter Row Column  to step into position for eg. 0 1 (0th Row 1st Column)");
                    var cursorPosition = Console.GetCursorPosition();
                    Console.WriteLine(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, cursorPosition.Top);
                    lineInput = Console.ReadLine();
                    if (lineInput?.Trim().ToLower() == "e")
                    {
                        messageLine = 0;
                        Console.Clear();
                        break;
                    }

                    Position? position = GetPositionToStepInto(field, lineInput);
                    if (position == null) continue;

                    var stepOutput = validateStepIntoPosition(field, field.StepIntoPosition((Position)position!));
                    hasStepError = stepOutput.isValid;
                    if (stepOutput.isGameOver) break;
                }

                //the mine field should look like this now:
                //  01234
                //4|1X1
                //3|11111
                //2|2211X
                //1|XX111
                //0|X31

                // Game code...
            }
            catch (Exception ex)
            {
                writeMessage(Console.CursorTop, $"An error occurred creating a minefield, error:{ex.Message}", isWarning: true);
                continue;
            }
        }

        void addWaitingMessage()
        {
            var messageLine = getCursorPositions().messageLine;
            writeMessage(messageLine, new string('*', Console.WindowWidth));
            Thread.Sleep(250);
            writeMessage(messageLine, new string(' ', Console.WindowWidth));
        }

        (bool isValid, bool isGameOver) validateStepIntoPosition(Minefield field, int stepOutput)
        {
            printBoard(field, true, stepOutput == MessageCodes.AllMinesDiscovered || stepOutput == MessageCodes.SteppedOnAMine);

            var messageLine = getCursorPositions().messageLine;
            addWaitingMessage();
            if (stepOutput == MessageCodes.RowPositionIsInvalid)
            {
                writeMessage(messageLine, $"Please enter row position between 0 and {field.Rows - 1}", isWarning: true);
                return (false, false);
            }

            if (stepOutput == MessageCodes.ColumnPositionIsInvalid)
            {
                writeMessage(messageLine, $"Please enter column position between 0 and {field.Columns - 1}", isWarning: true);
                return (false, false);
            }

            if (stepOutput == MessageCodes.CellAlreadyRevealed)
            {
                writeMessage(messageLine, $"Position already checked, please select another position", isWarning: true);
                return (false, false);
            }

            if (stepOutput == MessageCodes.SteppedOnAMine)
            {
                writeMessage(messageLine, $"You stepped on a mine",isWarning:true);
                return (false, true);
            }

            if (stepOutput == MessageCodes.AllMinesDiscovered)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                writeMessage(messageLine, $"You discovered all mines",isSuccess:true);
                Console.ResetColor();
                return (true, true);
            }

            return (true, false);
        }

        (int instructionLine, int messageLine) getCursorPositions()
        {
            return (boardHeight, boardHeight + 5);
        }

        Position? GetPositionToStepInto(Minefield field, string? lineInput)
        {
            var messageLine = getCursorPositions().messageLine;
            addWaitingMessage();
            var inputs = lineInput?.Split(" ");
            if (inputs?.Length == 2 == false)
            {

                writeMessage(messageLine, "Please enter two numbers for mine positions for e.g.  2 2");
                return null;
            }

            int row = 0;
            if (int.TryParse(inputs[0], out row) == false)
            {
                writeMessage(messageLine, $"Please provide a valid row position between 0 and {field.Rows - 1}");
                return null;
            }

            int column = 0;
            if (int.TryParse(inputs[1], out column) == false)
            {
                writeMessage(messageLine, $"Please provide a valid column position between 0  and {field.Columns - 1}");
                return null;
            }

            return new Position { Row = row, Column = column };
        }
    }

    private static void writeMessage(int cursorPositionTop, string message, bool isWarning = false, bool isSuccess = false)
    {
        Console.SetCursorPosition(0, cursorPositionTop);
        var cursorPosition = Console.GetCursorPosition();
        Console.WriteLine(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, cursorPositionTop);

        if (isWarning) Console.ForegroundColor = ConsoleColor.Red;
        if (isSuccess) Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"{Console.GetCursorPosition().Top}: {message}");
        Console.ResetColor();
    }

    private static (Minefield? mineField, bool isValid) GetMineField(string? lineInput)
    {
        (Minefield? minefield, bool isValid) retValue = (null, false);

        var inputs = lineInput?.Split(" ");

        if (string.IsNullOrWhiteSpace(lineInput) || inputs?.Length > 0 == false)
        {
            retValue.minefield = Minefield.CreateMine();
            retValue.isValid = true;
            return retValue;
        }

        if (inputs.Length > 3)
        {
            writeMessage(Console.CursorTop,"Invalid Number of Inputs",isWarning:true);
            return retValue;
        }

        int rows = 0;
        if (inputs.Length > 0) int.TryParse(inputs[0], out rows);
        int columns = 0;
        if (inputs.Length > 1) int.TryParse(inputs[1], out columns);
        int bombDensity = 0;
        if (inputs.Length > 2)
        {
            int.TryParse(inputs[2], out bombDensity);
        }
        else
        {
            bombDensity = 25;
        }
        try
        {
            retValue.minefield = Minefield.CreateMine(rows, columns, bombDensity);
            retValue.isValid = true;
            return retValue;
        }
        catch (MineFieldException mfEx)
        {
            writeMessage(Console.CursorTop, $"An error occurred creating a minefield, code:{mfEx.ErrorCode} error:{mfEx.Message}", isWarning:true);
        }
        catch (Exception ex)
        {
            writeMessage(Console.CursorTop, $"An error occurred creating a minefield, error:{ex.Message}",isWarning:true);
        }

        return retValue;
    }

    private static void printBoard(Minefield field, bool atPosition0 = true, bool revealSecret = false)
    {
        const char BOMB = 'X';
        const char NOTREVEALED = '?';
        int rows = field.Rows;
        int columns = field.Columns;


        if (atPosition0)
        {
            Console.SetCursorPosition(0, 0);
        }
        else
        {
            Console.WriteLine();
        }

        void writeWithColor(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write($"{message}");
            Console.ResetColor();
        }

        //Console.WriteLine("     " + string.Join(" ", Enumerable.Range(0, columns)));
        Console.WriteLine("   " + string.Join(" ", Enumerable.Range(0, columns)));
        Console.WriteLine("  " + new string('-', columns * 2 + 1));
        for (int row = 0; row < rows; row++)
        {
            //Console.Write($"{row} {rows -1 - row}|");
            Console.Write($"{rows - 1 - row}|");

            for (int column = 0; column < columns; column++)
            {
                var isCellRevealed = field.IsCellRevealed(row, column);
                var cellHasBomb = field.CellHasBomb(row, column);

                if (!isCellRevealed)
                {
                    if (cellHasBomb)
                    {
                        if (revealSecret)
                        {
                            writeWithColor($" {BOMB}", ConsoleColor.Red);
                        }
                        else
                        {
                            Console.Write($" {NOTREVEALED}");
                        }
                        continue;
                    }

                    if (revealSecret)
                    {
                        writeWithColor($" {field.GetAdjacentBombCount(new Position { Row = row, Column = column })}", ConsoleColor.Green);
                    }
                    else
                    {
                        Console.Write($" {NOTREVEALED}");
                    }
                    continue;
                }

                if (cellHasBomb)
                {
                    writeWithColor($" {BOMB}", ConsoleColor.Red);
                    continue;
                }

                writeWithColor($" {field.GetAdjacentBombCount(new Position { Row = row, Column = column })}", ConsoleColor.Green);
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }
}