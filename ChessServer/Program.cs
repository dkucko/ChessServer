using System;

namespace ChessServer
{
    class Program
    {
        static int port = 32168;
        static string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        static Board board;
        static Server server;

        static void Main(string[] args)
        {
            ParseArgs(args);

            board = new Board(fen);
            server = new Server(port);
            server.StartListening();
            server.SendMessage(Player.WHITE, MessageType.COLOR_ASSIGNMENT, "white");
            server.SendMessage(Player.BLACK, MessageType.COLOR_ASSIGNMENT, "black");
            server.BroadcastMessage(MessageType.INITIAL_FEN, fen);

            ServerLoop();

            server.BroadcastMessage(MessageType.GAME_END, "");
        }

        static void ServerLoop()
        {
            Player currentPlayer = board.playerToMove;
            bool gameOver = false;
            do
            {
                server.SendMessage(currentPlayer, MessageType.MOVE_REQUEST, "");
                string move = server.WaitForMessage(currentPlayer, 0, out MessageType mType);
                if (mType != MessageType.CLIENT_MOVE)
                {
                    // Wrong message was sent, handle somehow
                }
                if (!board.ValidateMove(move))
                {
                    server.SendMessage(currentPlayer, MessageType.INVALID_MOVE, "");
                    // End game?
                }
                else
                {
                    gameOver = board.DoMove(move);
                    server.BroadcastMessage(MessageType.MOVE_INFO, move);
                }

                board.Print();
            } while (!gameOver);
        }

        static void ParseArgs(string[] args)
        {
            // Parse command line options
            for (int i = 0; i < args.Length; i += 2)
            {
                switch (args[i])
                {
                    case "-h":
                    case "--help":
                        PrintHelp();
                        return;
                    case "-p":
                    case "--port":
                        try
                        {
                            port = Convert.ToInt32(args[i + 1]);
                            if (port < 1 || port > 65535)
                            {
                                Console.WriteLine("ERROR: Specified port is not within the valid port range (1-65535)");
                                return;
                            }
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.WriteLine("ERROR: No port was specified");
                            return;
                        }
                        catch
                        {
                            Console.WriteLine("ERROR: Port must be an integer");
                            return;
                        }
                        break;
                    case "-f":
                    case "--fen":
                        try
                        {
                            fen = args[i + 1];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.WriteLine("ERROR: No fen was specified");
                            return;
                        }
                        break;
                    case "-t":
                    case "--time":
                        try
                        {
                            string time = args[i + 1];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.WriteLine("ERROR: No time was specified");
                            return;
                        }
                        break;
                    default:
                        Console.WriteLine($"ERROR: Unkown command option: {args[i]}");
                        return;
                }
            }
        }

        static void PrintHelp()
        {
            Console.WriteLine(@"
This is a simple Chess Server for two clients communicating via Sockets.

Available Command Line Options:
-h, --help: Print this help text and exit the program
-p, --port: The Port on which the server should listen, default: 32168
-f, --fen : The initial board position encoded in FEN Notation (MUST be surronded by double Quotes)
-t, --time: The Time Control to use (all numbers will be interpreted as seconds)
    examples:
    -t 3  : White and black have a total of 3 seconds for play with no additional time per turn
    -t 10+2 :  White and black have a total of 10 secondes for play with an addional two seconds per turn
    -t 360:10+2 : White will have a total of 6 minutes with no bonus time, black will have a total of 10 seconds with two second bonus per turn
            ");
        }
    }
}
