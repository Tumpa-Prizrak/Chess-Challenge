using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ChessChallenge.API;

public class FirstBot : IChessBot
{
    public struct EvalMove
    {
        public int Evaluation;
        public Move Move;

        public EvalMove(int evaluation, Move move)
        {
            Evaluation = evaluation;
            Move = move;
        }
    }

    public int GetValue(PieceType pieceType)
    {
        switch (pieceType)
        {
            case PieceType.None:
                return 0;
            case PieceType.Pawn:
                return 100;
            case PieceType.Knight:
                return 300;
            case PieceType.Bishop:
                return 305;
            case PieceType.Rook:
                return 500;
            case PieceType.Queen:
                return 900;
            case PieceType.King:
                return 0;
        }

        return 0;
    }
    
    public Move Think(Board board, Timer timer)
    {
        return Search(board, 3).Move;
    }

    public EvalMove Search(Board board, int depth)
    {
        EvalMove bestMove;
        bestMove = new EvalMove(!board.IsWhiteToMove ? int.MaxValue : int.MinValue, Move.NullMove);

        foreach (Move move in board.GetLegalMoves())
        {
            EvalMove egg;
            board.MakeMove(move);
            egg = depth == 1 ? new EvalMove(Evaluate(board), move) : Search(board, depth - 1);
            egg.Move = move;
            board.UndoMove(move);

            if (!board.IsWhiteToMove)
            {
                if (egg.Evaluation <= bestMove.Evaluation)
                {
                    bestMove = egg;
                }
            }
            else
            {
                if (egg.Evaluation >= bestMove.Evaluation)
                {
                    bestMove = egg;
                }
            }
        }

        return bestMove;
    }

    [SuppressMessage("ReSharper.DPA", "DPA0001: Memory allocation issues")]
    public int Evaluate(Board board)
    {
        int value = 0;

        if (board.IsInCheckmate())
        {
            return board.IsWhiteToMove ? int.MinValue : int.MaxValue;
        }

        if (board.IsDraw())
        {
            return (board.IsWhiteToMove ? int.MinValue + 1 : int.MaxValue - 1);
        }

        if (board.IsInCheck())
        {
            value += 50 * (board.IsWhiteToMove ? -1 : 1);
        }

        foreach (var variablePieceType in new List<PieceType>() { PieceType.Pawn, PieceType.Knight, PieceType.Bishop, PieceType.Bishop, PieceType.Rook, PieceType.Queen})
        {
            foreach (Piece piece in board.GetPieceList(variablePieceType, true))
            {
                value += GetValue(piece.PieceType);
            }
            
            foreach (Piece piece in board.GetPieceList(variablePieceType, false))
            {
                value -= GetValue(piece.PieceType);
            }
        }
        
        return value;
    }
}