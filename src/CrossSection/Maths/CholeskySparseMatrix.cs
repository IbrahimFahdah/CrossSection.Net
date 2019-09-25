using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrossSection.Maths
{
    internal class CholeskySparseMatrix
    {
        internal class SparseVector
        {
            public int[] Indices;
            public double[] Values;
            public int ValueCount;
            public readonly int Length;
            internal SparseVector(int n)
            {
                Length = n;
                Indices = new int[0];
                Values = new double[0];
                ValueCount = 0;
            }

            public double this[int index]
            {
                get
                {
                    return At(index);
                }
                set
                {
                    At(index, value);
                }
            }

            /// <summary>
            /// Retrieves the requested element without range checking.
            /// </summary>
            public double At(int index)
            {
                // Search if item index exists in NonZeroIndices array in range "0 - nonzero values count"
                var itemIndex = Array.BinarySearch(Indices, 0, ValueCount, index);
                return itemIndex >= 0 ? Values[itemIndex] : 0.0;
            }


            /// <summary>
            /// Sets the element without range checking.
            /// </summary>
            public void At(int index, double value)
            {
                // Search if "index" already exists in range "0 - nonzero values count"
                var itemIndex = Array.BinarySearch(Indices, 0, ValueCount, index);
                if (itemIndex >= 0)
                {
                    // Non-zero item found in matrix
                    if (value == 0.0)
                    {
                        // Delete existing item
                        RemoveAtIndexUnchecked(itemIndex);
                    }
                    else
                    {
                        // Update item
                        Values[itemIndex] = value;
                    }
                }
                else
                {
                    // Item not found. Add new value
                    if (value != 0)
                    {
                        InsertAtIndexUnchecked(~itemIndex, index, value);
                    }
                }

            }

            internal void RemoveAtIndexUnchecked(int itemIndex)
            {
                // Value is zero. Let's delete it from Values and Indices array
                Array.Copy(Values, itemIndex + 1, Values, itemIndex, ValueCount - itemIndex - 1);
                Array.Copy(Indices, itemIndex + 1, Indices, itemIndex, ValueCount - itemIndex - 1);

                ValueCount -= 1;

                // Check whether we need to shrink the arrays. This is reasonable to do if
                // there are a lot of non-zero elements and storage is two times bigger
                if ((ValueCount > 1024) && (ValueCount < Indices.Length / 2))
                {
                    Array.Resize(ref Values, ValueCount);
                    Array.Resize(ref Indices, ValueCount);
                }
            }

            internal void InsertAtIndexUnchecked(int itemIndex, int index, double value)
            {
                // Check if the storage needs to be increased
                if ((ValueCount == Values.Length) && (ValueCount < Length))
                {
                    // Value and Indices arrays are completely full so we increase the size
                    var size = Math.Min(Values.Length + GrowthSize(), Length);
                    Array.Resize(ref Values, size);
                    Array.Resize(ref Indices, size);
                }

                // Move all values (with a position larger than index) in the value array to the next position
                // Move all values (with a position larger than index) in the columIndices array to the next position
                Array.Copy(Values, itemIndex, Values, itemIndex + 1, ValueCount - itemIndex);
                Array.Copy(Indices, itemIndex, Indices, itemIndex + 1, ValueCount - itemIndex);

                // Add the value and the column index
                Values[itemIndex] = value;
                Indices[itemIndex] = index;

                // increase the number of non-zero numbers by one
                ValueCount += 1;
            }

            /// <summary>
            /// Calculates the amount with which to grow the storage array's if they need to be
            /// increased in size.
            /// </summary>
            /// <returns>The amount grown.</returns>
            int GrowthSize()
            {
                int delta;
                if (Values.Length > 1024)
                {
                    delta = Values.Length / 4;
                }
                else
                {
                    if (Values.Length > 256)
                    {
                        delta = 512;
                    }
                    else
                    {
                        delta = Values.Length > 64 ? 128 : 32;
                    }
                }

                return delta;
            }

        }
        // Master dictionary hold rows of column dictionary
        protected SparseVector[] _rows;

        /// <summary>
        /// Constructs a SparseMatrix instance.
        /// </summary>
        public CholeskySparseMatrix(double[,] array)
        {
            int n = array.GetLength(0);

            _rows = new SparseVector[n];
            for (int row = 0; row < n; row++)
            {
                var columnIndices = new List<int>();
                var values = new List<double>();

                //read upper half of the array
                for (int col = row; col < n; col++)
                {
                    if (array[row, col] != 0)
                    {
                        values.Add(array[row, col]);
                        columnIndices.Add(col);
                    }
                }

                _rows[row] = new SparseVector(n);
                _rows[row].Values = values.ToArray();
                _rows[row].Indices = columnIndices.ToArray();
                _rows[row].ValueCount = columnIndices.Count;
            }
        }

        public SparseVector this[int row]
        {
            get
            {
                return _rows[row];
            }

        }

        /// <summary>
        /// Gets or sets the value at the specified matrix position.
        /// </summary>
        /// <param name="row">Matrix row</param>
        /// <param name="col">Matrix column</param>
        public double this[int row, int col]
        {
            get
            {
                return _rows[row].At(col);
            }
            set
            {
                _rows[row].At(col, value);
            }
        }


        public SparseVector GetRow(int row)
        {
            return _rows[row];
        }

    }

}
