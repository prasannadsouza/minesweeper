using Minesweeper;
using System.Data.Common;

namespace MinesweeperTest;

[TestClass]
public class Tests
{
    [TestMethod]
    public void TestAnExplosion()
    {
        var field = Minefield.CreateMine();
        var minePositions = field.GetMineCellPositions();
        Random random = new Random();
        var index = random.Next(0, minePositions.Count - 1);
        var actualRow = field.Rows - 1 - minePositions[index].Row;
        var stepOutput = field.StepIntoPosition(new Position { Row = actualRow, Column = minePositions[index].Column });
        Assert.AreEqual(MessageCodes.SteppedOnAMine, stepOutput);
    }

    [TestMethod]
    public void TestEscapeFromMineField()
    {
        var field = Minefield.CreateMine();
        for (int row = 0; row < field.Rows; row++)
        {
            for (int column = 0; column < field.Columns; column++)
            {
                if (field.IsCellRevealed(row, column)) continue;
                if (field.CellHasBomb(row, column)) continue;
                var actualRow = field.Rows - 1 - row;
                var stepOutput = field.StepIntoPosition(new Position {Row = actualRow, Column = column });
                Assert.AreNotEqual(stepOutput, MessageCodes.SteppedOnAMine);
            }
        }

        Assert.AreEqual(field.AreAllMinesDiscovered(), true);
    }
}
