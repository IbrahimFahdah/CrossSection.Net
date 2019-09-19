//// <copyright>
////https://github.com/IbrahimFahdah/CrossSection.Net

////Copyright(c) 2019 Ibrahim Fahdah

////Permission is hereby granted, free of charge, to any person obtaining a copy
////of this software and associated documentation files (the "Software"), to deal
////in the Software without restriction, including without limitation the rights
////to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
////copies of the Software, and to permit persons to whom the Software is
////furnished to do so, subject to the following conditions:

////The above copyright notice and this permission notice shall be included in all
////copies or substantial portions of the Software.

////THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
////IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
////FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
////AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
////LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
////OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
////SOFTWARE.
////</copyright>

//using MathNet.Numerics.LinearAlgebra;
//using System.Collections.Generic;

//namespace CrossSection.Triangulation
//{
//    public static class MatrixCache2
//    {
//        private static readonly object balanceLock = new object();
//        private static List<Matrix<double>> storage = new List<Matrix<double>>();

//        public static Matrix Create(int rows, int columns)
//        {
//            var store= Get ($"{rows},{columns}");
//            if (store!=null)
//                return new Matrix(store);

//            return new Matrix(rows, columns);
//        }

//        public static Matrix Create(double[,] array)
//        {
//            var store = Get(array.Code());
//            if (store != null)
//            {
//                for (int i = 0; i < store.RowCount; i++)
//                {
//                    for (int j = 0; j < store.ColumnCount; j++)
//                    {
//                        store[i,j]= array[i,j];
//                    }
//                }
              
//                return new Matrix(store);
//            }
               

//            return new Matrix(array);
//        }

//        public static Matrix Create(double[][] array)
//        {
//            var store = Get(array.Code());
//            if (store != null)
//            {
//                for (int i = 0; i < store.RowCount; i++)
//                {
//                    for (int j = 0; j < store.ColumnCount; j++)
//                    {
//                        store[i, j] = array[i][j];
//                    }
//                }

//                return new Matrix(store);
//            }

//            return new Matrix(array);
//        }

//        public static void Add(Matrix<double> m)
//        {
//           // lock (balanceLock)
//            {
//                storage.Add(m);//, $"{m.RowCount},{m.ColumnCount}");
//                //foreach (Matrix m in lst)
//                //{
//                //    m.Clear();
//                //    storage.Add(m, $"{m.RowCount},{m.ColumnCount}");
//                //}
//            }
            
//        }

//        private static Matrix<double> Get(string code)
//        {
//           // lock (balanceLock)
//            {
//                var store = storage.FirstOrDefault(m => m.Code() == code);
//                if (store!=null)//.Equals(default(KeyValuePair<Matrix<double>, string>)))
//                {
//                    var m = store;
//                    storage.Remove(m);
//                    return m;
//                }
//                return null;
//            }
//        }

//        private static string Code(this Matrix<double> matrix)
//        {
//            return $"{matrix.RowCount},{matrix.ColumnCount}";
//        }

//        private static string Code(this double[,] array)
//        {
//            return $"{array.GetLength(0)},{array.GetLength(1)}";
//        }

//        private static string Code(this double[][] array)
//        {
//            return $"{array.GetLength(0)},{array.GetLength(1)}";
//        }
//    }
//}
