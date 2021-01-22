# Bejeweled

Classic Bejeweled prototype in Unity.

Board size can be defined in GameController (rows, columns) and generated with random pieces, 
or by attaching an instance of BoardData scriptable object.

Fill BoardData Board property with a representation of the initial board.

Example:

```
0,1,2
-1,2,3
3,0,-1
```
Numbers represent piece type. -1 is random piece.

Game ends when there are no more available plays.
