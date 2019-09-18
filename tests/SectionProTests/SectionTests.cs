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

using System.Collections.Generic;
using CrossSection;
using CrossSection.Analysis;
using CrossSection.DataModel;
using CrossSection.IO;
using CrossSection.Triangulation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SectionProTests
{
    /// <summary>
    /// Output from CrossSection.Net are compared with the output from the python package written by python package written by Robbie van Leeuwen 
    /// and the output from other software
    /// </summary>
    [TestClass]
    public class SectionTests
    {
        Solver _solver = new Solver();
        ShapeGeneratorHelper helper = new ShapeGeneratorHelper();
        SectionMaterial defaultMat = new SectionMaterial("dummy", 1, 1.0, 0.0, 1.0);

        /// <summary>
        /// A flag used to choose if you want to write the mesh data to files.
        /// This is used to check if section triangulation looks OK. 
        /// Mesh files can be opened using the mesh viewer which comes with Triangle.Net https://archive.codeplex.com/?p=triangle 
        /// </summary>
        bool _writefiles = false;

        /// <summary>
        /// The folder where the mesh files will be written.
        /// Must not be empty if _writefiles = true.
        /// </summary>
        string _meshFileFolder = @"";

        [TestMethod]
        public void CircleSec_Test()
        {
            SectionDefinition sec = new SectionDefinition(nameof(CircleSec_Test));
            sec.SolutionSettings = new SolutionSettings(0.01);
            sec.Contours.Add(new SectionContour(helper.CreateCircle(25, 64), false, defaultMat));
            _solver.Solve(sec);
            Compare(nameof(CircleSec_Test), sec);
        }

        [TestMethod]
        public void HollowCircleSec_Test()
        {
            SectionDefinition sec = new SectionDefinition(nameof(HollowCircleSec_Test));
            sec.SolutionSettings = new SolutionSettings(0.01);
            sec.Contours.Add(new SectionContour(helper.CreateCircle(25, 64), false, defaultMat));
            sec.Contours.Add(new SectionContour(helper.CreateCircle(10, 64), true, defaultMat));
            _solver.Solve(sec);
            Compare(nameof(HollowCircleSec_Test), sec);
        }

        [TestMethod]
        public void CircleSecWithShift_Test()
        {
            SectionDefinition sec = new SectionDefinition(nameof(CircleSecWithShift_Test));
            sec.SolutionSettings = new SolutionSettings(0.01);
            sec.Contours.Add(new SectionContour(helper.CreateCircle(25, 64), false, defaultMat));
            sec.ShiftPoints(100, 50);
            _solver.Solve(sec);
            Compare(nameof(CircleSecWithShift_Test), sec);

        }

        [TestMethod]
        public void Chs_Test()
        {
            SectionDefinition sec = new SectionDefinition(nameof(Chs_Test));
            sec.SolutionSettings = new SolutionSettings(0.01);
            sec.Contours.Add(new SectionContour(helper.CreateCircle(21.3 / 2.0, 50), false, defaultMat));
            sec.Contours.Add(new SectionContour(helper.CreateCircle(21.3 / 2.0 - 2.6, 50), true, defaultMat));
            _solver.Solve(sec);
            Compare(nameof(Chs_Test), sec);
        }

        [TestMethod]
        public void CruciformSec_Test()
        {
            SectionDefinition sec = new SectionDefinition(nameof(CruciformSec_Test));
            sec.SolutionSettings = new SolutionSettings(0.01);
            sec.Contours.Add(new SectionContour(helper.CreateCruciformShape(200, 100, 10, 10, 5), false, defaultMat));
            _solver.Solve(sec);
            Compare(nameof(CruciformSec_Test), sec);
        }

        [TestMethod]
        public void Ehs_Test()
        {
            SectionDefinition sec = new SectionDefinition(nameof(Ehs_Test));
            sec.SolutionSettings = new SolutionSettings(0.01);
            sec.Contours.Add(new SectionContour(helper.CreateEllipse(1, 150.0 / 2.0, 75.0 / 2.0, 100), false, defaultMat));
            sec.Contours.Add(new SectionContour(helper.CreateEllipse(1, 150.0 / 2.0 - 4.0, 75.0 / 2.0 - 4.0, 100), true, defaultMat));
            _solver.Solve(sec);
            Compare(nameof(Ehs_Test), sec);
        }

        [TestMethod]
        public void ZedSection_Test()
        {
            SectionDefinition sec = new SectionDefinition(nameof(ZedSection_Test));
            sec.SolutionSettings = new SolutionSettings(0.01);
            sec.Contours.Add(new SectionContour(helper.CreateZedShape(200, 100, 75, 20, 5, 10, 10), false, defaultMat));
            _solver.Solve(sec);
            Compare(nameof(ZedSection_Test), sec);
        }

        [TestMethod]
        public void CeeSection_Test()
        {
            SectionDefinition sec = new SectionDefinition(nameof(CeeSection_Test));
            sec.SolutionSettings = new SolutionSettings(0.01);
            sec.Contours.Add(new SectionContour(helper.CreateCeeShape(200, 100, 75, 5, 10, 10), false, defaultMat));
            _solver.Solve(sec);
            Compare(nameof(CeeSection_Test), sec);

        }

        [TestMethod]
        public void AngleSec_Test()
        {
            SectionDefinition sec = new SectionDefinition(nameof(AngleSec_Test));
            sec.SolutionSettings = new SolutionSettings(0.01);
            sec.Contours.Add(new SectionContour(helper.CreateAngleShape(100, 100, 6, 8, 5, 8), false, defaultMat));
            sec.ShiftPoints(100, 25);
            _solver.Solve(sec);
            Compare(nameof(AngleSec_Test), sec);
        }

        [TestMethod]
        public void ISec_Test()
        {
            SectionDefinition sec = new SectionDefinition(nameof(ISec_Test));
            sec.SolutionSettings = new SolutionSettings(0.01);
            SectionMaterial steel = new SectionMaterial("Steel", 1, 200e3, 0.3, 500);
            sec.Contours.Add(new SectionContour(helper.CreateIShape(304, 165, 10.2, 6.1, 11.4, 8), false, steel));
            _solver.Solve(sec);
            Compare(nameof(ISec_Test), sec);
        }

        [TestMethod]
        public void MonoISection_Test()
        {
            SectionDefinition sec = new SectionDefinition(nameof(MonoISection_Test));
            sec.SolutionSettings = new SolutionSettings(0.01);
            sec.Contours.Add(new SectionContour(helper.CreateMonoISection(150, 100, 20, 30, 15, 300, 250, 50, 20, 25, 10), false, defaultMat));
            _solver.Solve(sec);
            Compare(nameof(MonoISection_Test), sec);
        }

        [TestMethod]
        public void CompositeSec_Test()
        {
            //Note that because of using more than one material, some output need to be mapped to eq. material (e.g. Sx output represents Mpx and Sx= Mpx/fy) 

            SectionMaterial steel = new SectionMaterial("Steel", 1, 200e3, 0.3, 500);
            var timber = new SectionMaterial("Timber", 2, 8e3, 0.35, 20);

            SectionDefinition sec = new SectionDefinition(nameof(CompositeSec_Test));
            sec.SolutionSettings = new SolutionSettings(0.01);
            sec.Contours.Add(new SectionContour(helper.CreateIShape(304, 165, 10.2, 6.1, 11.4, 8), false, steel));
            var panel = new SectionContour(helper.CreateRectangle(50, 600), false, timber);
            panel.ShiftPoints(-217.5, 304);
            sec.Contours.Add(panel);

            _solver.Solve(sec);
            Compare(nameof(CompositeSec_Test), sec);

           
        }

        [TestMethod]
        public void CompundSec_Test()
        {
            SectionDefinition sec = new SectionDefinition(nameof(CompundSec_Test));
            sec.SolutionSettings = new SolutionSettings(0.01);

            (List<Point2D> outer, List<Point2D> inner) = helper.CreateRhsShape(150, 100, 6, 15, 8);
            sec.Contours.Add(new SectionContour(outer, false, defaultMat));
            sec.Contours.Add(new SectionContour(inner, true, defaultMat));

            List<Point2D> points = new List<Point2D>();
            points.Add(new Point2D(0, 0));
            points.Add(new Point2D(50, 0));
            points.Add(new Point2D(25, 50));
            var triangle = new SectionContour(points,false, defaultMat);
            triangle.ShiftPoints(25, 150);
            sec.Contours.Add(triangle);
          
            var angle = new SectionContour(helper.CreateAngleShape(100, 100, 6, 8, 5, 8), false, defaultMat);
            angle.ShiftPoints(100, 25);
            sec.Contours.Add(angle);

            _solver.Solve(sec);
            Compare(nameof(CompundSec_Test), sec);

        }

        [TestMethod]
        public void CustomSec1_Test()
        {
            SectionDefinition sec = new SectionDefinition(nameof(CustomSec1_Test));
            sec.SolutionSettings = new SolutionSettings(0.002);

            List<Point2D> boundary = new List<Point2D>();
            boundary.Add(new Point2D(0,0));
            boundary.Add(new Point2D(180, 0));
            boundary.Add(new Point2D(180, 120));
            boundary.Add(new Point2D(120, 120));
            boundary.Add(new Point2D(180, 300));
            boundary.Add(new Point2D(0, 300));

            List<Point2D> hole = new List<Point2D>();
            hole.Add(new Point2D(60, 60));
            hole.Add(new Point2D(120, 60));
            hole.Add(new Point2D(120, 120));
            hole.Add(new Point2D(60, 120));

            sec.Contours.Add(new SectionContour(boundary, false, defaultMat));
            sec.Contours.Add(new SectionContour(hole, true, null));

            _solver.Solve(sec);
            Compare(nameof(CustomSec1_Test), sec);

        }

        [TestMethod]
        public void CustomSec2_Test()
        {
            SectionDefinition sec = new SectionDefinition(nameof(CustomSec2_Test));
            sec.SolutionSettings = new SolutionSettings(0.002);

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
                hole.Add(new Point2D(x0+180, 50));
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
            Compare(nameof(CustomSec2_Test), sec);

        }


        private void Write(SectionDefinition sec, string folder)
        {
            var secName = !string.IsNullOrWhiteSpace(sec.Name) ? sec.Name : "unnamed";
            string fileName = System.IO.Path.Combine(folder, secName);
            sec.WritePoly(fileName+"_poly.poly");
            sec.WriteMesh(fileName + "_mesh");
            sec.Write(fileName+".txt");
        }


        private void Compare(string testName, SectionDefinition sec)
        {
            if (_writefiles)
            {
                Write(sec, _meshFileFolder);
            }

            JObject actual = BuildTestData(sec, testName);

            var expected = System.IO.File.ReadAllText($@"..\..\ExpectedData\{testName}.txt");
            JObject expectedJson = JsonConvert.DeserializeObject<JObject>(expected);

            Compare(expectedJson, actual);



        }

        private JObject BuildTestData(SectionDefinition sec, string testName, bool replaceExpected = false)
        {
            string data = JsonConvert.SerializeObject(sec.Output.SectionProperties);
            JObject actual = JsonConvert.DeserializeObject<JObject>(data);
            if (replaceExpected)
            {
                System.IO.File.WriteAllText($@"..\..\ExpectedData\{testName}.txt", JsonConvert.SerializeObject(actual, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        FloatParseHandling = FloatParseHandling.Double
                    }));
            }
            return actual;
        }
        private void Compare(JObject expectedJObject, JObject actualJObject)
        {
            if (!JToken.DeepEquals(expectedJObject, actualJObject))
            {
                foreach (KeyValuePair<string, JToken> actualProperty in actualJObject)
                {
                    JProperty expctedProp = expectedJObject.Property(actualProperty.Key);

                    if (!JToken.DeepEquals(expctedProp.Value, actualProperty.Value))
                    {
                        Assert.AreEqual((double)expctedProp.Value, (double)actualProperty.Value, 0.00001, "{0} has changed.", actualProperty.Key);
                    }
                }
            }

        }
    }
}
