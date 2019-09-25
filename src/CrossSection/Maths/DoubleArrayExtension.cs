using System;
using System.Collections.Generic;
using System.Text;

namespace CrossSection.Maths
{
    internal static class DoubleArrayExtension
    {

        public static bool istheSameAs(this double[,] A, double[,] B)
        {
            for (int i = 0; i < A.RowCount(); i++)
            {
                for (int j = 0; j < A.ColumnCounts(); j++)
                {
                    if (A[i, j] != B[i, j])
                    {
                        return false;
                    }
                }
            }

            return true;

        }

        public static double[,] Dot(this double[,] A, double[,] B)
        {

            var R = new double[A.RowCount(), B.ColumnCounts()];
            for (int i = 0; i < R.RowCount(); i++)
                for (int j = 0; j < R.ColumnCounts(); j++)
                    for (int k = 0; k < A.ColumnCounts(); k++)
                        R[i, j] += A[i, k] * B[k, j];
            return R;

        }

        public static double[,] MultiplyBy(this double[,] A, double x)
        {
            for (int i = 0; i < A.RowCount(); i++)
                for (int j = 0; j < A.ColumnCounts(); j++)
                    A[i, j] = A[i, j] * x;
            return A;

        }

        public static double[,] MultiplyBy(this double x, double[,] A)
        {
            for (int i = 0; i < A.RowCount(); i++)
                for (int j = 0; j < A.ColumnCounts(); j++)
                    A[i, j] = A[i, j] * x;
            return A;

        }

        public static double[] MultiplyBy(this double x, double[] A)
        {
            for (int i = 0; i < A.Length; i++)
                A[i] = A[i] * x;
            return A;

        }

        public static double[] MultiplyBy(this double[] A, double x)
        {
            for (int i = 0; i < A.Length; i++)
                A[i] = A[i] * x;
            return A;

        }

        public static void Append(this double[,] A, double[,] B)
        {
            for (int i = 0; i < A.RowCount(); i++)
                for (int j = 0; j < A.ColumnCounts(); j++)
                    A[i, j] += B[i, j];
        }

        public static double[] Append(this double[] A, double[] B)
        {

            for (int i = 0; i < A.Length; i++)
                A[i] += B[i];
            return A;

        }

        public static double[,] Transpose(this double[,] arr)
        {
            int rowCount = arr.RowCount();
            int columnCount = arr.ColumnCounts();
            double[,] transposed = new double[columnCount, rowCount];

            for (int column = 0; column < columnCount; column++)
            {
                for (int row = 0; row < rowCount; row++)
                {
                    transposed[column, row] = arr[row, column];
                }
            }
            return transposed;

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

        public static double[] Dot(this double[,] A, double[] V)
        {
            var R = new double[A.RowCount()];

            for (int i = 0; i < A.RowCount(); i++)
                for (int j = 0; j < A.ColumnCounts(); j++)
                    R[i] += A[i, j] * V[j];

            return R;

        }

        public static double[] Dot(this double[] V, double[,] A)
        {
            var R = new double[V.Length];

            for (int i = 0; i < V.Length; i++)
                for (int j = 0; j < A.ColumnCounts(); j++)
                    R[i] += V[j] * A[j, i];

            return R;

        }

        public static double[] Row(this double[,] A, int i)
        {
            var R = new double[A.ColumnCounts()];
            for (int j = 0; j < A.ColumnCounts(); j++)
            {
                R[j] = A[i, j];
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

        public static int RowCount(this double[,] A)
        {
            return A.GetLength(0);
        }

        public static int ColumnCounts(this double[,] A)
        {
            return A.GetLength(1);
        }

    }
}
