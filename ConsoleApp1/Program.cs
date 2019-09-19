using CrossSection;
using CrossSection.Analysis;
using CrossSection.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Solver _solver = new Solver();
        
            SectionMaterial defaultMat = new SectionMaterial("dummy", 1, 1.0, 0.0, 1.0);

            SectionDefinition sec = new SectionDefinition();
            sec.SolutionSettings = new SolutionSettings(0.001);

            List<Point2D> boundary = new List<Point2D>();
            boundary.Add(new Point2D(0, 0));
            boundary.Add(new Point2D(0, 300));
            boundary.Add(new Point2D(1300, 300));
            boundary.Add(new Point2D(990, 0));
            sec.Contours.Add(new SectionContour(boundary, false, defaultMat));

            List<Point2D> hole = null;
            for (int i = 0; i < 4; i++)
            {
                var x0 = 50 + i * 230;
                hole = new List<Point2D>();
                hole.Add(new Point2D(x0, 50));
                hole.Add(new Point2D(x0 + 180, 50));
                hole.Add(new Point2D(x0 + 180, 250));
                hole.Add(new Point2D(x0, 250));
                sec.Contours.Add(new SectionContour(hole, true, null));
            }

            hole = new List<Point2D>();
            hole.Add(new Point2D(970, 50));
            hole.Add(new Point2D(970, 250));
            hole.Add(new Point2D(1175, 250));
            sec.Contours.Add(new SectionContour(hole, true, null));


            _solver.Solve(sec);

        }
    }
}
