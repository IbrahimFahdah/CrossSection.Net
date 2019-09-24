using System;
using System.Collections.Generic;
using System.Text;

namespace CrossSection.Maths
{
    internal static class DoubleArrayExtension
    {

        public static double[,] Dot(this double[,] A, double[,] B)
        {

            var R = new double[A.RowCounts(), B.ColumnCounts()];
            for (int i = 0; i < R.RowCounts(); i++)
                for (int j = 0; j < R.ColumnCounts(); j++)
                    for (int k = 0; k < A.ColumnCounts(); k++)
                        R[i, j] += A[i, k] * B[k, j];
            return R;

        }

        public static double[][] Transpose(this double[][] arr)
        {
            int rowCount = arr.Length;
            int columnCount = arr[0].Length;
            double[][] transposed = new double[columnCount][];

            for (int column = 0; column < columnCount; column++)
            {
                transposed[column] = new double[rowCount];
                for (int row = 0; row < rowCount; row++)
                {
                    transposed[column][row] = arr[row][column];
                }
            }
            return transposed;

        }


        public static double[][] Dot(this double[][] A, double[][] B)
        {

            var R = new double[A.RowCounts()][];
            for (int i = 0; i < A.RowCounts(); i++)
            {
                R[i] = new double[B.ColumnCounts()];
                for (int j = 0; j < B.ColumnCounts(); j++)
                {
                    for (int k = 0; k < A.ColumnCounts(); k++)
                    {
                        R[i][j] += A[i][k] * B[k][j];
                    }
                }
            }

            return R;

        }

        public static double Dot(this double[] A, double[] B)
        {

            var R = 0.0;
            for (int i = 0; i < A.Length; i++)
            {
                R += A[i] * B[i];
            }

            return R;

        }


        public static double[] Row(this double[][] A, int i)
        {
            return A[i];
        }

        public static int RowCounts(this double[][] A)
        {
            return A.Length;
        }

        public static int ColumnCounts(this double[][] A)
        {
            return A[0].Length;
        }

        public static int RowCounts(this double[,] A)
        {
            return A.GetLength(0);
        }

        public static int ColumnCounts(this double[,] A)
        {
            return A.GetLength(1);
        }

    }
}
