using System;
using System.Collections.Generic;
using UnityEngine;

public class Bitboard
{
    private ulong bitboard;
    private bool isWhite;

    public Bitboard()
    {
        bitboard = 0;
        isWhite = false;
    }

    public void SetWhitePiece(bool isWhitePiece)
    {
        isWhite = isWhitePiece;
    }

    public bool IsWhitePiece()
    {
        return isWhite;
    }

    public List<int> GetIndicesOfPieces()
    {
        List<int> indices = new List<int>();
        ulong num = bitboard;
        int index = 0;

        while (num != 0)
        {
            if ((num & 1) != 0)
            {
                indices.Add(index);
            }
            num >>= 1;
            ++index;
        }

        return indices;
    }

    public void SetBitboard(ulong num)
    {
        bitboard = num;
    }


    public void AddAtIndex(int index)
    {
        ulong targetMoveMask = 1UL << index;
        bitboard |= targetMoveMask;
    }


    public void RemoveAtIndex(int index)
    {
        ulong pieceToMoveMask = 1UL << index;
        bitboard &= ~pieceToMoveMask;
    }
    public void PrintBinary()
    {
        Console.WriteLine($"Bitboard in binary: {Convert.ToString((long)bitboard, 2).PadLeft(64, '0')}");
    }

    public ulong GetBitboard()
    {
        return bitboard;
    }

    public void PrintBitboardInRowsAndColumns()
    {
        for (int rank = 7; rank >= 0; --rank)
        {
            string row = "";

            for (int file = 0; file < 8; ++file)
            {
                int squareIndex = rank * 8 + file;
                if ((bitboard & (1UL << squareIndex)) != 0)
                {
                    row += "1 ";
                }
                else
                {
                    row += "0 ";
                }
            }

            Debug.Log(row);
        }
    }
}
