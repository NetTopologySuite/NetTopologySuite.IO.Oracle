using GeoAPI.IO;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTopologySuite.IO.Oracle.Test
{
    [TestFixture]
    public class OracleTest
    {
        // Our set of geometries to test.
        public static string[] testSet = new string[]
        {
                "POINT(10 10)",
                "POINT(10 10 0)",
                "POINT(10 10 20)",
                "MULTIPOINT(11 12, 20 20)",
                "MULTIPOINT(11 12 12, 20 20 20)",
                "LINESTRING(10 10,20 20,50 50,34 34)",
                "LINESTRING(10 10 20,20 20 20,50 50 50,34 34 34)",
                "POLYGON((10 10,20 10,20 20,10 20,10 10))",
                "POLYGON((10 10,20 10,20 20,10 20,10 10),(5 5,5 6,6 6,6 5,5 5))",
                "POLYGON((10 10 0,20 10 0,20 20 0,10 20 0,10 10 0),(5 5 0,5 6 0,6 6 0,6 5 0,5 5 0))",
                "MULTIPOLYGON(((10 10,20 10,20 20,20 10,10 10)),((10 10,20 10,20 20,20 10,10 10)))",
                "MULTIPOLYGON(((10 10,20 10,20 20,10 20,10 10),(5 5,5 6,6 6,6 5,5 5)),((10 10,20 10,20 20,20 10,10 10)))",
                "MULTIPOLYGON(((10 10,20 10,20 20,10 20,10 10),(5 5,5 6,6 6,6 5,5 5)),((10 10,20 10,20 20,20 10,10 10),(5 5,5 6,6 6,6 5,5 5)))",
                "MULTIPOLYGON(((10 10 0,20 10 0,20 20 0,10 20 0,10 10 0),(5 5 0,5 6 0,6 6 0,6 5 0,5 5 0)),((10 10 0,20 10 0,20 20 0,20 10 0,10 10 0),(5 5 0,5 6 0,6 6 0,6 5 0,5 5 0)))",
                "MULTILINESTRING((10 10,20 10,20 20,20 10),(5 5,5 6,6 6,6 5))",
                "MULTILINESTRING((10 10 5,20 10 5,20 20 0,20 10 0,10 10 0),(5 5 0,5 6 0,6 6 0,6 5 0,5 5 0))",
        };

        public static int SRID = 4326;

        private static readonly OracleGeometryReader or = new OracleGeometryReader();
        private static readonly WKTReader wr = new WKTReader();

        [Test]
        public void BasicConversion()
        {
            for (var i = 0; i < testSet.Length; i++)
            {
                General(testSet[i], -1);
                General(testSet[i], SRID);
            }
        }

        [Test]
        public void CCWTestsOnPolygon()
        {
            var wrongCCW = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10),(5 5,6 5,6 6,5 6,5 5))";
            var correctCCW = "POLYGON((10 10, 20 10, 20 20, 10 20, 10 10),(5 5,5 6,6 6,6 5,5 5))";

            var geom1 = wr.Read(wrongCCW);
            var geom2 = wr.Read(correctCCW);

            var t = new OracleGeometryWriter().Write(geom1);
            var geom3 = or.Read(t);
            Assert.IsTrue(geom2.EqualsExact(geom3));
        }

        private static void General(string wkt, int srid)
        {
            var geom = wr.Read(wkt);
            var parsed = geom.AsText();
            var regeom = wr.Read(parsed);
            var reparsed = regeom.AsText();

            geom.SRID = srid;
            regeom.SRID = srid;

            Assert.IsTrue(geom.EqualsExact(regeom));
            Assert.AreEqual(parsed, reparsed);

            var t = new OracleGeometryWriter().Write(regeom);
            var regeom3 = or.Read(t);
            Assert.IsTrue(geom.EqualsExact(regeom3));

        }
    }
}
