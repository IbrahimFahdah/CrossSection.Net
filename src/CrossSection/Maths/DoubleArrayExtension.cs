using CrossSection.Maths2;
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
                for (int j = 0; j < A.ColumnCount(); j++)
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

            var R = new double[A.RowCount(), B.ColumnCount()];
            for (int i = 0; i < R.RowCount(); i++)
                for (int j = 0; j < R.ColumnCount(); j++)
                    for (int k = 0; k < A.ColumnCount(); k++)
                        R[i, j] += A[i, k] * B[k, j];
            return R;

        }

        public static double[,] MultiplyBy(this double[,] A, double x)
        {
            for (int i = 0; i < A.RowCount(); i++)
                for (int j = 0; j < A.ColumnCount(); j++)
                    A[i, j] = A[i, j] * x;
            return A;

        }

        //public static double[,] MultiplyBy(this double x, double[,] A)
        //{
        //    for (int i = 0; i < A.RowCount(); i++)
        //        for (int j = 0; j < A.ColumnCount(); j++)
        //            A[i, j] = A[i, j] * x;
        //    return A;

        //}

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
                for (int j = 0; j < A.ColumnCount(); j++)
                    A[i, j] += B[i, j];
        }

        public static double[] Append(this double[] A, double[] B)
        {

            for (int i = 0; i < A.Length; i++)
                A[i] += B[i];
            return A;

        }

        public static double[] Append(this double[] A, LMatrix B)
        {

            for (int i = 0; i < A.Length; i++)
                A[i] += B.mat[i];

            return A;

        }

        public static double[,] Transpose(this double[,] arr)
        {
            int rowCount = arr.RowCount();
            int columnCount = arr.ColumnCount();
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

        public static double[,] Dot(this double[,] A, double x)
        {
            var R = new double[A.RowCount(), A.ColumnCount()];
            for (int i = 0; i < A.RowCount(); i++)
                for (int j = 0; j < A.ColumnCount(); j++)
                    R[i, j] = A[i, j]*x;

         
            return R;

        }

        public static double[,] Dot(this double x, double[,] A)
        {
            var R = new double[A.RowCount(), A.ColumnCount()];
            for (int i = 0; i < A.RowCount(); i++)
                for (int j = 0; j < A.ColumnCount(); j++)
                    R[i, j] = A[i, j] * x;


            return R;

        }
        public static double[] Dot(this  double x, double[] A)
        {
            var R = new double[A.Length];
            for (int i = 0; i < A.Length; i++)
                R[i] = A[i] * x;
            return R;

        }
        public static double[] Dot(this double[] A, double x)
        {
            var R = new double[A.Length];
            for (int i = 0; i < A.Length; i++)
                R[i] = A[i] * x;
            return R;

        }


        public static double[][] Dot(this double[][] A, double[,] B)
        {

            var R = new double[A.RowCount()][];
            for (int i = 0; i < A.RowCount(); i++)
            {
                R[i] = new double[B.ColumnCount()];
                for (int j = 0; j < B.ColumnCount(); j++)
                {
                    for (int k = 0; k < A.ColumnCount(); k++)
                    {
                        R[i][j] += A[i][k] * B[k,j];
                    }
                }
            }

            return R;

        }

        public static double[][] Dot(this double[][] A, double[][] B)
        {

            var R = new double[A.RowCount()][];
            for (int i = 0; i < A.RowCount(); i++)
            {
                R[i] = new double[B.ColumnCount()];
                for (int j = 0; j < B.ColumnCount(); j++)
                {
                    for (int k = 0; k < A.ColumnCount(); k++)
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
                for (int j = 0; j < A.ColumnCount(); j++)
                    R[i] += A[i, j] * V[j];

            return R;

        }

        public static double[] Dot(this double[] V, double[,] A)
        {
            var R = new double[V.Length];

            for (int i = 0; i < V.Length; i++)
                for (int j = 0; j < A.ColumnCount(); j++)
                    R[i] += V[j] * A[j, i];

            return R;

        }

        public static double[] Row(this double[,] A, int i)
        {
            var R = new double[A.ColumnCount()];
            for (int j = 0; j < A.ColumnCount(); j++)
            {
                R[j] = A[i, j];
            }

            return R;
        }
        public static double[] Row(this double[][] A, int i)
        {
            return A[i];
        }

        public static int RowCount(this double[][] A)
        {
            return A.Length;
        }

        public static int ColumnCount(this double[][] A)
        {
            return A[0].Length;
        }

        public static int RowCount(this double[,] A)
        {
            return A.GetLength(0);
        }

        public static int ColumnCount(this double[,] A)
        {
            return A.GetLength(1);
        }

    }
}
