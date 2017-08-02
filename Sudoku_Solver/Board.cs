using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Sudoku_Solver
{
    class Board
    {
        private byte[,] grid;


        public Board()
        {
            this.grid = new byte[9, 9];
        }


        public Board(byte[,] grid)
        {
            if (grid.GetLength(0) != 9 || grid.GetLength(1) != 9)
            {
                throw new System.ArgumentException();
            }
            this.grid = new byte[9, 9];
            Array.Copy(grid, this.grid, 81);
        }


        public Board(Board board)
        {
            this.grid = new byte[9, 9];
            Array.Copy(board.grid, this.grid, 81);
        }


        public static implicit operator Board(byte[,] grid)
        {
            return new Board(grid);
        }


        // Code to make board[y, x] work
        public byte this[int y, int x]
        {
            get
            {
                return grid[y, x];
            }
            set
            {
                grid[y, x] = (byte)value;
            }
        }


        // Return a copy of the board
        public Board Clone()
        {
            return new Board(this.grid);
        }


        // Check whether the board can contain a valid solution
        public bool Check()
        {
            // Check rows (and valid numbers)
            for (int y = 0; y < 9; y++)
            {
                bool[] found = new bool[10];
                for (int x = 0; x < 9; x++)
                {
                    int num = grid[y, x];
                    if (num < 0 || num > 9)
                    {
                        return false;
                    }

                    if (num != 0)
                    {
                        if (!found[num])
                        {
                            found[num] = true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            // Check columns
            for (int x = 0; x < 9; x++)
            {
                bool[] found = new bool[10];
                for (int y = 0; y < 9; y++)
                {
                    int num = grid[y, x];
                    if (num == 0)
                    {
                        continue;
                    }

                    if (num != 0)
                    {
                        if (!found[num])
                        {
                            found[num] = true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            // Check squares
            for (int m = 0; m < 9; m += 3)
            {
                for (int n = 0; n < 9; n += 3)
                {
                    bool[] found = new bool[10];
                    for (int i = 0; i < 9; i++)
                    {
                        int num = grid[m + i / 3, n + i % 3];
                        if (num == 0)
                        {
                            continue;
                        }

                        if (num != 0)
                        {
                            if (!found[num])
                            {
                                found[num] = true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }


        // Return whether num is a valid entry at [y, x]
        public bool Valid(int y, int x, int num)
        {
            if (num < 1 || num > 9)
            {
                return false;
            }

            int ySquare = y / 3 * 3;
            int xSquare = x / 3 * 3;
            for (int i = 0; i < 9; i++)
            {
                // Check row
                if (i != x && grid[y, i] == num)
                {
                    return false;
                }

                // Check column
                if (i != y && grid[i, x] == num)
                {
                    return false;
                }

                // Check square
                int m = ySquare + i / 3;
                int n = xSquare + i % 3;
                if (m != y && n != x && grid[m, n] == num)
                {
                    return false;
                }
            }

            return true;
        }


        // Return taken candidates as bitfield for position [y, x]
        public uint Taken(int y, int x)
        {
            uint found = 0;

            int ySquare = y / 3 * 3;
            int xSquare = x / 3 * 3;
            for (int i = 0; i < 9; i++)
            {
                // Check row
                found |= ((uint)1 << grid[y, i]);

                // Check column
                found |= ((uint)1 << grid[i, x]);

                // Check square
                int m = ySquare + i / 3;
                int n = xSquare + i % 3;
                found |= ((uint)1 << grid[m, n]);
            }

            return found;
        }


        // Return viable candidates for position [y, x]
        public List<byte> Viable(int y, int x)
        {
            uint found = 0;

            int ySquare = y / 3 * 3;
            int xSquare = x / 3 * 3;
            for (int i = 0; i < 9; i++)
            {
                // Check row
                found |= ((uint)1 << grid[y, i]);

                // Check column
                found |= ((uint)1 << grid[i, x]);

                // Check square
                int m = ySquare + i / 3;
                int n = xSquare + i % 3;
                found |= ((uint)1 << grid[m, n]);
            }

            List<byte> result = new List<byte>();
            for (byte i = 1; i <= 9; i++)
            {
                if (((found >> i) & 1) == 0)
                {
                    result.Add(i);
                }
            }

            return result;
        }


        // Return a nicely formatted string of the contents on the board
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            for (int y = 0; y < grid.GetLength(0); y++)
            {
                for (int x = 0; x < grid.GetLength(1); x++)
                {
                    output.AppendFormat("{0} ", grid[y, x]);
                }
                output.Append("\n");
            }

            return output.ToString();
        }

    }
}
