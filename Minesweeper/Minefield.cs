namespace Minesweeper;

public class ErrorCode
{ 
    public const int InvalidRows = 1;
    public const int InvalidColumns = 2;
    public const int InvalidMineDensity = 3;
}
public class MessageCodes
{
    public const int Ok = 0;
    public const int SteppedOnAMine = 1;
    public const int CellAlreadyRevealed = 2;
    public const int RowPositionIsInvalid = 3;
    public const int ColumnPositionIsInvalid = 4;
    public const int AllMinesDiscovered = 5;
}

class MineFieldException: Exception
{ 
    public int ErrorCode {  get; private set; }

    public MineFieldException(int errorCode, string message): base(message)
    {
        ErrorCode = errorCode;
    }
}

class MineCell
{
    public bool IsRevealed { get; private set; }
    public bool HasBomb { get; private set; }
    public bool IsPlaced { get; private set; }

    public void PlaceBomb()
    {
        HasBomb = true;
        IsPlaced = true;
    }

    public void RevealCell() => IsRevealed = true;
    public void SetPlaced() => IsPlaced = true;
}

public struct Position
{
    public int Row;
    public int Column;
}

public class Minefield
{
    private const int AVERAGE_ROWS = 4;
    private const int AVERAGE_COLUMNS = 4;
    private const int AVERAGE_MINE_DENSITY = 25;
   
    private MineCell[,] board = new MineCell[0, 0];
    public int MineDensity { get; private set; }
    public int Rows => board.GetLength(0);
    public int Columns => board.GetLength(0);
    public int TotalMines { get; private set; }
    public int TotalRevealedCells { get; private set; }

    private Minefield(int rows, int columns, int mineDensity)
    {
        board = new MineCell[rows, columns];
        MineDensity = getCorrectMineDensity(mineDensity);
    }

    public static Minefield CreateMine(int rows = AVERAGE_ROWS, int columns = AVERAGE_COLUMNS, int mineDensity = AVERAGE_MINE_DENSITY)
    {
        if (rows < 2 || rows > 50) throw new MineFieldException(ErrorCode.InvalidRows, "Rows should be between 2 and 50");
        if (columns < 2 || columns > 50) throw new MineFieldException(ErrorCode.InvalidColumns, "Columns should be between 2 and 50");
        if (mineDensity < 1 || columns > mineDensity) throw new MineFieldException(ErrorCode.InvalidColumns, "MineDensity should be between 1 and 99");

        var mineField = new Minefield(rows, columns, mineDensity);
        mineField.placeMines();

        return mineField;
    }
    public int StepIntoPosition(Position position)
    {
        var actualRow = board.GetLength(0) - 1 - position.Row;

        if (actualRow < 0 || actualRow > (board.GetLength(0) - 1)) return MessageCodes.RowPositionIsInvalid;
        if (position.Column < 0 || position.Column > (board.GetLength(1) - 1)) return MessageCodes.ColumnPositionIsInvalid;

        var cell = board[actualRow, position.Column];
        if (cell.IsRevealed)
        {
            return MessageCodes.CellAlreadyRevealed;
        }
        
        if (cell.HasBomb)
        {
            revealCell(cell);
            return MessageCodes.SteppedOnAMine;
        }

        revealCell(cell);

        if (AreAllMinesDiscovered())
        {
            return MessageCodes.AllMinesDiscovered;
        }

        var adjacentPositions = getAdjacentPositions(new Position {Row = actualRow, Column = position.Column });
        adjacentPositions.ForEach(position =>
        {
            var cell = board[position.Row, position.Column];
            if (cell.HasBomb) return;

            //var adjacentBombCount =  getAdjacentBombCount(new Position { Row = position.Row, Column = position.Column});
            //if (adjacentBombCount > 0) revealCell(cell);
            revealCell(cell);
        });

        if (AreAllMinesDiscovered())
        {
            return MessageCodes.AllMinesDiscovered;
        }
        
        return MessageCodes.Ok;
    }
    public List<Position> GetMineCellPositions()
    { 
        var positions = new List<Position>();
        int rows = board.GetLength(0);
        int columns = board.GetLength(1);
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                var cell = board[row, column];
                if (cell.HasBomb) positions.Add(new Position { Row = row, Column = column });
            }
        }

        return positions;
    }
    public bool AreAllMinesDiscovered()
    {
        int rows = board.GetLength(0);
        int columns = board.GetLength(1);
        return TotalMines + TotalRevealedCells == rows * columns;
    }

    public bool IsCellRevealed(int row, int col) => board[row, col].IsRevealed;
    public bool CellHasBomb(int row, int col) => board[row, col].HasBomb;

    public int GetAdjacentBombCount(Position position)
    {
        return getAdjacentPositions(position).Where(e => board[e.Row, e.Column].HasBomb).Count();
    }

    private int getCorrectMineDensity(int mineDensity)
    {
        if (mineDensity < 1) return 1;
        if (mineDensity > 99) return 99;
        return mineDensity;
    }
    private void placeMines()
    {
        var random = new Random();
        var rows = board.GetLength(0);
        var columns = board.GetLength(1);
        int totalCells = rows * columns;
        TotalMines = totalCells * MineDensity / 100;
        if (TotalMines == 0) TotalMines = 1;
        int totalMinesRequired = 0;

        while (totalMinesRequired < TotalMines)
        {
            int row = random.Next(0, rows);
            int col = random.Next(0, columns);

            if (board[row, col] == null) board[row, col] = new MineCell();
            if (!board[row, col].IsPlaced)
            {
                board[row, col].PlaceBomb();
                totalMinesRequired++;
            }
        }

        for (int row = 0; row < rows; row++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (board[row, j] == null) board[row, j] = new MineCell();
                if (!board[row, j].IsPlaced) board[row, j].SetPlaced();
            }
        }
    }
   
    private List<Position> getAdjacentPositions(Position position)
    {
        var adjacentCells = new List<Position>();
        int[] dr = { -1, -1, -1, 0, 0, 1, 1, 1 };
        int[] dc = { -1, 0, 1, -1, 1, -1, 0, 1 };

        int rows = board.GetLength(0);
        int columns = board.GetLength(1);

        for (int i = 0; i < 8; i++)
        {
            int row = position.Row + dr[i];
            int col = position.Column + dc[i];

            if (row < 0 || row > rows - 1) continue;
            if (col < 0 || col > columns - 1) continue;

            adjacentCells.Add( new Position { Row = row, Column = col });
        }

        return adjacentCells;
    }
    private void revealCell(MineCell cell)
    {
        if (cell.IsRevealed) return;
        cell.RevealCell();
        TotalRevealedCells++;
    }
   
   
}
