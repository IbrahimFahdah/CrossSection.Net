// <copyright>
//https://github.com/IbrahimFahdah/CrossSection.Net

//Copyright(c) 2019 Ibrahim Fahdah

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.
//</copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CrossSection.Maths;
using CrossSection.Triangulation;
using TriangleNet.Topology;

namespace CrossSection.Analysis
{

    internal class ExtendedTri
    {
        internal class ShapeFunData
        {
            internal double[,] B = null;
            internal double[] N;
            internal double j;
        }




        private Triangle _tri;
        private TriNode[] _nodes;

        internal ShapeFunData[] ShapeInfo;
        internal ExtendedTri(Triangle tri, TriNode[] nodes)
        {
            _tri = tri;
            _nodes = nodes;
            BuildShapeFunData();
        }

        private void BuildShapeFunData()
        {
            ShapeInfo = new ShapeFunData[6];

            //Gauss points for 6 point Gaussian integration
            var gps = ShapeFunctionHelper.gauss_points(6);

            for (int i = 0; i < gps.RowCount(); i++)
            {
                var gp = gps.Row(i);
                double[,] B = null;
                double[] N;
                double j;
                ShapeFunctionHelper.shape_function(coords, gp, out N, out B, out j);
                ShapeInfo[i] = new ShapeFunData() { N = N, B = B, j = j };
            }
        }

        public int Label => _tri.Label;

        public double[,] coords => _tri.GetTriCoords();

        internal void TriangleData(out int n0, out int n1, out int n2, out int n3, out int n4, out int n5)
        {

            n0 = _nodes[0].Index;
            n1 = _nodes[1].Index;
            n2 = _nodes[2].Index;
            n3 = _nodes[3].Index;
            n4 = _nodes[4].Index;
            n5 = _nodes[5].Index;
        }



    }

}
