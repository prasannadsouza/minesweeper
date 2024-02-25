# C# console Minesweeper 
Console minesweeper game built with c#,  with option for size and complexity.<br>
Run the c# console project to start the game or
Run unit tests to explode a mine or uncover all mines

## Create any sized minefield with a optional mine density
1. No Options (Just Press enter on prompt), will create a 4 by 4 minefield with 25% of the positions having  mines
2. Example 1: 5 5 50 (will create a 5 by 5 with 50% of the minefield occupied by mines)
3. Example 2: 6 8 75 (will create a 6 by 8 with 75% of the minefield occupied by mines)

A 4 by 4 mine will look like this, where X is the mine and the numbers are the number of mines adjacent to the cell 

### A minefield with hidden mines
&ensp;&ensp;0 1 2 3<br>
  ---------<br>
3| ? ? ? ?<br>
2| ? ? ? ?<br>
1| ? ? ? ?<br>
0| ? ? ? ?<br>

 
### An uncovered 4 by 4 minefield  with 25% mines
 &ensp;&ensp;0 1 2 3<br>
 ------------<br>
 3| X 1 0 0<br>
 2| 2 2 1 0<br>
 1| 2 X 3 1<br>
 0| 2 X 3 X<br>

## After the minefield is created
1. Provide coordinates to uncover a position
2. Example: 0 0  will uncover the 0th row and 0th column and the surrounding positions without the mines
   
&ensp;&ensp;0 1 2 3<br>
-----------<br>
3| ? ? ? ?<br>
2| ? ? ? ?<br>
1| 2 ? ? ?<br>
0| 2 ? ? ?<br>

### Keep playing till a mine is stepped on or till all the mines are discovered