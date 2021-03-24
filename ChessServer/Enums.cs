namespace ChessServer
{
    enum Player
    {
        WHITE = 1,
        BLACK
    }

    enum MessageType : byte
    {
        COLOR_ASSIGNMENT = 1,
        INITIAL_FEN,
        CLIENT_MOVE,
        INVALID_MOVE,
        MOVE_INFO,
        MOVE_REQUEST,
        GAME_END
    }
}