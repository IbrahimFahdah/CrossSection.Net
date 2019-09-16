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
using System.Collections.Generic;
using System.Linq;
using CrossSection.DataModel;
using TriangleNet;
using TriangleNet.Geometry;

namespace CrossSection.Triangulation
{
    internal static class PolygonHelper
    {
        public static TriangleNet.Geometry.Point FindPointInPolygon(SectionContour contour, List<SectionContour> otherContours,
            int limit, double eps = 2e-5)
        {
            List<Vertex> poly = contour.Points.Select(p => new Vertex(p.X, p.Y)).ToList();
            var bounds = new TriangleNet.Geometry.Rectangle();
            bounds.Expand(poly);

            int length = poly.Count;

            var test = new TriangleNet.Geometry.Point();

            TriangleNet.Geometry.Point a, b, c; // Current corner points.

            double bx, by;
            double dx, dy;
            double h;

            var predicates = new RobustPredicates();

            a = poly[0];
            b = poly[1];

            for (int i = 0; i < length; i++)
            {
                c = poly[(i + 2) % length];

                // Corner point.
                bx = b.X;
                by = b.Y;

                // NOTE: if we knew the contour points were in counterclockwise order, we
                // could skip concave corners and search only in one direction.

                h = predicates.CounterClockwise(a, b, c);

                if (Math.Abs(h) < eps)
                {
                    // Points are nearly co-linear. Use perpendicular direction.
                    dx = (c.Y - a.Y) / 2;
                    dy = (a.X - c.X) / 2;
                }
                else
                {
                    // Direction [midpoint(a-c) -> corner point]
                    dx = (a.X + c.X) / 2 - bx;
                    dy = (a.Y + c.Y) / 2 - by;
                }

                // Move around the contour.
                a = b;
                b = c;

                h = 1.0;

                for (int j = 0; j < limit; j++)
                {
                    // Search in direction.
                    test.X = bx + dx * h;
                    test.Y = by + dy * h;

                    if (bounds.Contains(test) && IsPointInPolygon(test, contour, otherContours))
                    {
                        return test;
                    }

                    // Search in opposite direction (see NOTE above).
                    test.X = bx - dx * h;
                    test.Y = by - dy * h;

                    if (bounds.Contains(test) && IsPointInPolygon(test, contour, otherContours))
                    {
                        return test;
                    }

                    h = h / 2;
                }
            }

            throw new Exception();
        }

        /// <summary>
        /// Return true if the given point is inside the polygon, or false if it is not.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <param name="poly">The polygon (list of contour points).</param>
        /// <returns></returns>
        /// <remarks>
        /// WARNING: If the point is exactly on the edge of the polygon, then the function
        /// may return true or false.
        /// 
        /// See http://alienryderflex.com/polygon/
        /// </remarks>
        public static bool IsPointInPolygon(TriangleNet.Geometry.Point point, SectionContour contour,
            List<SectionContour> otherContours)
        {
            bool inside = false;

            double x = point.X;
            double y = point.Y;

            List<Vertex> poly = contour.Points.Select(p => new Vertex(p.X, p.Y)).ToList();

            inside = IsPointInPolygon(point, poly);
            if (inside)
            {
                foreach (var item in otherContours)
                {
                    if (IsPointInPolygon(point, item.Points.Select(p => new Vertex(p.X, p.Y)).ToList()))
                    {
                        inside = false;
                        break;
                    }
                }
            }
            return inside;
        }

        public static bool IsPointInPolygon(TriangleNet.Geometry.Point point, List<Vertex> poly)
        {
            bool inside = false;

            double x = point.X;
            double y = point.Y;

            int count = poly.Count;

            for (int i = 0, j = count - 1; i < count; i++)
            {
                if (((poly[i].Y < y && poly[j].Y >= y) || (poly[j].Y < y && poly[i].Y >= y))
                    && (poly[i].X <= x || poly[j].X <= x))
                {
                    inside ^= (poly[i].X + (y - poly[i].Y) / (poly[j].Y - poly[i].Y) * (poly[j].X - poly[i].X) < x);
                }

                j = i;
            }


            return inside;
        }

    }

}