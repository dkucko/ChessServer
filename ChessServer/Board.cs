using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessServer
{
    class Board
    {
        private char[] squares = new char[64];
        public Player playerToMove { get; private set; }
        private string castlingRights;
        private string enPassantMove;
        private int numHalfMoves;
        private int numTurns;
        private Stack<string> fenHistory = new Stack<string>();

        public Board(string fen)
        {
            this.ParseFen(fen);
            this.fenHistory.Push(fen);
        }

        public bool ValidateMove(string move)
        {

            return true;
        }

        public bool DoMove(string move)
        {
            // First do the move

            // Then save the resulting fen to the history
            this.fenHistory.Push(this.GenerateFen());
            // Todo: Determine game over and return
            return false;
        }

        public void UndoMove()
        {
            // Pop the last fen from the history and parse it, thereby restoring the state before the last move
            this.ParseFen(this.fenHistory.Pop());
        }

        public void Print()
        {
            Console.WriteLine("  a b c d e f g h  ");
            Console.WriteLine(" ----------------- ");
            for (int row = 0; row < 8; row++)
            {
                Console.WriteLine(8 - row + "|" + String.Join(' ', this.squares.Skip(row * 8).Take(8).ToArray()) + "|" + (8 - row));
            }
            Console.WriteLine(" ----------------- ");
            Console.WriteLine("  a b c d e f g h  ");
            Console.WriteLine("FEN: " + this.GenerateFen());
        }

        private void ParseFen(string fen)
        {
            string[] groups = fen.Split(' ');

            // First Group: Board Position
            int currentRow = 0;
            foreach (string row in groups[0].Split('/'))
            {
                for (int i = 0; i < row.Length; i++)
                {
                    if ('1' <= row[i] && row[i] <= '8')
                    {
                        for (int j = 0; j < Convert.ToInt32(row[i].ToString()); j++)
                        {
                            this.squares[currentRow * 8 + i + j] = ' ';
                        }
                    }
                    else
                    {
                        this.squares[currentRow * 8 + i] = row[i];
                    }
                }
                currentRow += 1;
            }

            // Second Group: Player to move
            if (groups[1] == "w")
            {
                this.playerToMove = Player.WHITE;
            }
            else if (groups[1] == "b")
            {
                this.playerToMove = Player.BLACK;
            }

            // Third Group: Castling rights
            this.castlingRights = groups[2];

            // Fourth Group: En Passant move
            this.enPassantMove = groups[3];

            // Fifth Group: Number of half moves
            this.numHalfMoves = Convert.ToInt32(groups[4]);

            // Sixth Group: Number of the current turn
            this.numTurns = Convert.ToInt32(groups[5]);
        }

        private string GenerateFen()
        {
            string fen = "";
            int skippedFields = 0;
            string row = "";
            for (int i = 1; i <= 64; i++)
            {
                if (this.squares[i - 1] == ' ')
                {
                    skippedFields += 1;
                }
                else
                {
                    if (skippedFields > 0)
                    {
                        row += skippedFields;
                        skippedFields = 0;
                    }
                    row += this.squares[i - 1];
                }

                if (i % 8 == 0)
                {
                    if (skippedFields > 0)
                    {
                        row += skippedFields;
                        skippedFields = 0;
                    }
                    fen += row;
                    if (i != 64)
                    {
                        fen += '/';
                    }
                    row = "";
                }
            }

            return fen + " " + this.playerToMove + " " + this.castlingRights + " " + this.enPassantMove + " " + this.numHalfMoves + " " + this.numTurns;
        }
    }
}
