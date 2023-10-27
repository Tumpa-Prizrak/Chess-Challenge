using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ChessChallenge.API;
using ChessChallenge.Application;

public class PreviousBot : IChessBot
{
    // --------- Settings ---------

    public int MaxDepth = 20;
    
    // --------- EndSettings ---------
    
    public struct EvalMove
    {
        public int Evaluation;
        public Move Move;

        public EvalMove(int evaluation, Move move)
        {
            Evaluation = evaluation;
            Move = move;
        }

        public EvalMove()
        {
            Evaluation = 0;
            Move = Move.NullMove;
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
        Random rnd = new Random();
        Move bestMove = Move.NullMove;
        int maxThinkingTime;
        if (board.PlyCount <= 6)
        {
            maxThinkingTime = 1000;
        }
        else
        {
            maxThinkingTime = Math.Min(timer.MillisecondsRemaining / 5, timer.GameStartTimeMilliseconds / 50);
        }
        

        for (int i = 1; i <= MaxDepth; i++)
        {
            EvalMove evalMove = Search(board, i, timer, maxThinkingTime);
            if (evalMove.Move.IsNull)
            {
                break;
            }
            else
            {
                ConsoleHelper.Log($"depth: {i} | best move: {evalMove.Move} | eval: {evalMove.Evaluation}");
                bestMove = evalMove.Move;
                if (evalMove.Evaluation == int.MaxValue || evalMove.Evaluation == int.MinValue)
                {
                    break;
                }
            }
        }
        
        if (bestMove.IsNull)
        {
            Move[] legalMoves = board.GetLegalMoves();
            Move move = legalMoves[rnd.Next(0, legalMoves.Length)];
            ConsoleHelper.Log($"random move: {bestMove}");
            return move;
        }
        else
        {
            ConsoleHelper.Log($"best move: {bestMove}");
            return bestMove;
        }
    }

    public EvalMove Search(Board board, int depth, Timer timer, int maxTime)
    {
        EvalMove bestMove;
        bestMove = new EvalMove();
        bestMove.Evaluation = !board.IsWhiteToMove ? int.MaxValue : int.MinValue;
        Move[] moves = board.GetLegalMoves();
        
        foreach (Move move in moves)
        {
            if (timer.MillisecondsElapsedThisTurn >= maxTime)
            {
                return new EvalMove();
            }
            
            EvalMove egg;
            board.MakeMove(move);
            egg = depth == 1 ? new EvalMove(Evaluate(board), move) : Search(board, depth - 1, timer, maxTime);
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