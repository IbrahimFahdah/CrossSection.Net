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

using CrossSection.DataModel;
using CrossSection.Triangulation;
using System.Collections.Generic;
using System.Linq;
using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Topology;
namespace CrossSection
{

    public static class TriExtensions
    {
        

        public static (double x3, double y3, double x4 , double y4, double x5, double y5)
            GetMidVertex(this Triangle item)
        {
            //Node 3 between 0,1
            //Node 4 between 1,2
            //Node 5 between 2,0

            var x0 = item.GetVertex(0).X;
            var x1 = item.GetVertex(1).X;
            var x2 = item.GetVertex(2).X;
            var y0 = item.GetVertex(0).Y;
            var y1 = item.GetVertex(1).Y;
            var y2 = item.GetVertex(2).Y;

            var x3 = (x0 + x1) * 0.5;
            var x4 = (x1 + x2) * 0.5;
            var x5 = (x2 + x0) * 0.5;
            var y3 = (y0 + y1) * 0.5;
            var y4 = (y1 + y2) * 0.5;
            var y5 = (y2 + y0) * 0.5;

            return ( x3,  y3,  x4,  y4,  x5,  y5);
        }


        public static Polygon BuildPolygon(this SectionDefinition sec)
        {
            Polygon _polygon = new Polygon();
            foreach (var contour in sec.Contours)
            {
                _polygon.Add(new Contour(contour.Points.Select(p => new Vertex(p.X, p.Y))));
            }

            CreatedRegions(sec,_polygon);

            return _polygon;
        }

        public static Mesh Triangulate(this SectionDefinition sec,Polygon polygon)
        {
            var options = new ConstraintOptions();
            options.ConformingDelaunay = true;
            var quality = new QualityOptions()
            {
                MinimumAngle = sec.SolutionSettings.MinimumAngle,
                MaximumArea = sec.SolutionSettings.MaximumArea
            };
            var mesh = (Mesh)polygon.Triangulate(options, quality);

            return mesh;
        }

        private static void CreatedRegions(this SectionDefinition sec,Polygon polygon)
        {
            polygon.Regions.Clear();
            polygon.Holes.Clear();
            foreach (var contour in sec.Contours)
            {
                AddRegionPoint(polygon, contour, sec.Contours.Where(c => c != contour).ToList());

                //GetRegionPointer(contour) is not used because doesn't consider contour inside another such a hole
                //_polygon.Regions.Add(GetRegionPointer(contour));
            }
        }

        private static void AddRegionPoint(Polygon polygon, SectionContour c, List<SectionContour> otherContours)
        {
            TriangleNet.Geometry.Point p = null;
            if (c.IsHole)
            {
                p = PolygonHelper.FindPointInPolygon(c, new List<SectionContour>(), 100);
            }
            else
            {
                //find a point inside the counter but not in any other.
                p = PolygonHelper.FindPointInPolygon(c, otherContours, 100);

            }
            polygon.Regions.Add(new RegionPointer(p.X, p.Y, c.Material?.Id ?? -1));
            if (c.IsHole)
                polygon.Holes.Add(p);
        }
    }

}

