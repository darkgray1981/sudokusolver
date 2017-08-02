using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace Sudoku_Solver
{
    public partial class Form1 : Form
    {
        private Pen thinPen, thickPen;
        private Font boardFont;
        private StringFormat fontFormat;
        private Brush boardFontColour;
        private Graphics pb1Gfx;
        private int length;
        private int step;
        private int padding = 10;
        private Board sudoku;
        private bool solving = false;
        
        public Form1()
        {
            InitializeComponent();

            pb1Gfx = this.pictureBox1.CreateGraphics();

            this.pictureBox1.MouseUp += this.pictureBox1_MouseUp;
            this.pictureBox1.Paint += this.pictureBox1_Paint;

            this.length = (this.pictureBox1.Size.Width - 2 * this.padding) / 9 * 9;
            this.step = this.length / 9;

            this.thinPen = new Pen(Color.Black);
            this.thinPen.Width = 1;
            this.thickPen = new Pen(Color.Black);
            this.thickPen.Width = 2;

            this.boardFont = new Font("Arial", 24);
            this.fontFormat = new StringFormat(StringFormatFlags.NoClip);
            this.fontFormat.LineAlignment = StringAlignment.Center;
            this.fontFormat.Alignment = StringAlignment.Center;
            this.boardFontColour = Brushes.Black;

            this.sudoku = new Board();
            this.checkColour();

        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            // Get the graphics object
            Graphics gfx = e.Graphics;
            // Loop and create grid
            for (int i = 0; i <= 9; i++)
            {
                if (i % 3 == 0)
                {
                    // Draw thick horizontal lines
                    gfx.DrawLine(thickPen, padding - 1, padding + i * step, padding + length + 1, padding + i * step);
                    // Draw thick vertical lines
                    gfx.DrawLine(thickPen, padding + i * step, padding - 1, padding + i * step, padding + length + 1);
                }
                else
                {
                    // Draw horizontal lines
                    gfx.DrawLine(thinPen, padding, padding + i * step, padding + length, padding + i * step);
                    // Draw vertical lines
                    gfx.DrawLine(thinPen, padding + i * step, padding, padding + i * step, padding + length);
                }
            }

            // Fill grid with numbers from Board
            for (int i = 0; i < 81; i++)
            {
                int x = i % 9;
                int y = i / 9;

                // Construct a new Rectangle
                Rectangle displayRectangle =
                    new Rectangle(padding + x * step + 2, padding + y * step + 2, step - 4, step - 4);

                gfx.FillRectangle(Brushes.White, displayRectangle);

                string text = "";
                if (sudoku[y, x] != 0)
                {
                    text = sudoku[y, x].ToString();
                }
                gfx.DrawString(text, this.boardFont, this.boardFontColour, (RectangleF)displayRectangle, this.fontFormat);
            }

        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            // Only react if click is within grid
            if (!this.solving && e.X > padding && e.X < length + padding && e.Y > padding && e.Y < length + padding)
            {
                int xBox = 9 * (e.X - padding) / length;
                int yBox = 9 * (e.Y - padding) / length;

                // Construct a new Rectangle
                Rectangle displayRectangle =
                    new Rectangle(padding + xBox * step + 2, padding + yBox * step + 2, step - 4, step - 4);

                pb1Gfx.FillRectangle(Brushes.White, displayRectangle);

                string text = "";

                if (e.Button == MouseButtons.Left)
                {
                    this.sudoku[yBox, xBox] = (byte)((this.sudoku[yBox, xBox] + 1) % 10);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    // Decrease clicked number by one
                    this.sudoku[yBox, xBox] = (byte)((this.sudoku[yBox, xBox] + 9) % 10);
                }
                if (this.sudoku[yBox, xBox] != 0) {
                    text = this.sudoku[yBox, xBox].ToString();
                }
                pb1Gfx.DrawString(text, this.boardFont, this.boardFontColour, (RectangleF)displayRectangle, this.fontFormat);
                this.checkColour();
            }
        }


        // Clear button
        private void button2_Click(object sender, EventArgs e)
        {
            // Check if we should abort current solving process
            if (this.solving)
            {
                this.solving = false;

                // Give the solver thread a moment to return
                Thread.Sleep(10);
            }
            
            // Make the board a blank slate
            this.sudoku = new Board();

            this.checkColour();
        }


        // Solve button
        private void button1_Click(object sender, EventArgs e)
        {
            // Check if we need to abort current solving process
            if (this.solving)
            {
                this.solving = false;
                this.button1.Text = "Solve";
                return;
            }

            // Start solving, update button text to mirror state
            this.solving = true;
            this.button1.Text = "Abort";
            this.Refresh();

            // Start solver in another thread to keep  UI responsive
            backgroundWorker1.RunWorkerAsync();
        }


        // Identify proper colour for board (black == correct, red == incorrect)
        private void checkColour()
        {
            if (this.sudoku.Check())
            {
                this.button1.Enabled = true;
                this.boardFontColour = Brushes.Black;
            }
            else
            {
                this.button1.Enabled = false;
                this.boardFontColour = Brushes.Red;
            }

            this.Refresh();
        }


        // Board specific solver functions

        // Solve function setup and launch
        private Board Solve(Board board)
        {
            // Make sure the board is solvable before we bother
            if (board.Check() == true)
            {
                return _solve(board);
            }

            return null;
        }


        // Recursive solving function with bitfields
        private Board _solve(Board board)
        {
            //  Abort code
            if (this.solving == false)
            {
                return null;
            }

            
            int least_index = -1;
            uint least_candidates = 0;
            uint least_count = 0;
            for (int i = 0; i < 81; i++)
            {
                if (board[i / 9, i % 9] == 0)
                {
                    var candidates = board.Taken(i / 9, i % 9);
                    uint count = 9;
                    for (int p = 1; p <= 9; p++)
                    {
                        count -= (candidates >> p) & 1;
                    }
                    if (least_index == -1 || least_count > count)
                    {
                        least_index = i;
                        least_candidates = candidates;
                        least_count = count;
                        if (least_count <= 1)
                        {
                            break;
                        }
                    }
                }
            }

            // If the entire board is filled, we're done here!
            if (least_index == -1)
            {
                return board;
            }

            // If we find a spot with no candidates, we can't do anything
            if (least_count == 0)
            {
                return null;
            }

            for (byte i = 0; i <= 9; i++)
            {
                if (((least_candidates >> i) & 1) == 1)
                {
                    continue;
                }
                Board trial = board.Clone();
                trial[least_index / 9, least_index % 9] = i;
                trial = _solve(trial);
                if (trial != null)
                {
                    return trial;
                }
            }

            return null;
        }


        // Dispatch the solver in the background and handle results
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Board solved = null;
            Stopwatch t = Stopwatch.StartNew();
            solved = Solve(sudoku);
            long end = t.ElapsedMilliseconds;
            Debug.WriteLine("({0} ms)\n\n", end);

            if (solved != null)
            {
                Debug.Write(solved.ToString());
                Debug.WriteLine("Correct: " + solved.Check());
                this.sudoku = solved;
            }
            else
            {
                Debug.WriteLine("No solution found.");
            }

        }


        // Update UI with solver results
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Turn button back to normal now that we're done solving
            this.solving = false;
            this.button1.Text = "Solve";

            this.checkColour();
        }
    }
}
