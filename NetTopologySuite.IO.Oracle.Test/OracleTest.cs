using GeoAPI.IO;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTopologySuite.IO.Oracle.Test
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class OracleTest
    {

        private static readonly OracleGeometryReader or = new OracleGeometryReader();
        private static readonly WKTReader wr = new WKTReader();

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wkt"></param>
        /// <param name="srid"></param>
        [TestCase("POINT(10 10)", -1)]
        [TestCase("POINT(10 10)", 4326)]
        [TestCase("POINT(10 10 0)", -1)]
        [TestCase("POINT(10 10 20)", -1)]
        [TestCase("MULTIPOINT(11 12, 20 20)", -1)]
        [TestCase("MULTIPOINT(11 12 12, 20 20 20)", -1)]
        [TestCase("LINESTRING(10 10,20 20,50 50,34 34)", -1)]
        [TestCase("LINESTRING(10 10 20,20 20 20,50 50 50,34 34 34)", -1)]
        [TestCase("POLYGON((10 10,20 10,20 20,10 20,10 10))", -1)]
        [TestCase("POLYGON((10 10,20 10,20 20,10 20,10 10),(5 5,5 6,6 6,6 5,5 5))", -1)]
        [TestCase("POLYGON((10 10 0,20 10 0,20 20 0,10 20 0,10 10 0),(5 5 0,5 6 0,6 6 0,6 5 0,5 5 0))", -1)]
        [TestCase("MULTIPOLYGON(((10 10,20 10,20 20,20 10,10 10)),((10 10,20 10,20 20,20 10,10 10)))", -1)]
        [TestCase("MULTIPOLYGON(((10 10,20 10,20 20,10 20,10 10),(5 5,5 6,6 6,6 5,5 5)),((10 10,20 10,20 20,20 10,10 10)))", -1)]
        [TestCase("MULTIPOLYGON(((10 10,20 10,20 20,10 20,10 10),(5 5,5 6,6 6,6 5,5 5)),((10 10,20 10,20 20,20 10,10 10),(5 5,5 6,6 6,6 5,5 5)))", -1)]
        [TestCase("MULTIPOLYGON(((10 10 0,20 10 0,20 20 0,10 20 0,10 10 0),(5 5 0,5 6 0,6 6 0,6 5 0,5 5 0)),((10 10 0,20 10 0,20 20 0,20 10 0,10 10 0),(5 5 0,5 6 0,6 6 0,6 5 0,5 5 0)))", -1)]
        [TestCase("MULTILINESTRING((10 10,20 10,20 20,20 10),(5 5,5 6,6 6,6 5))", -1)]
        [TestCase("MULTILINESTRING((10 10 5,20 10 5,20 20 0,20 10 0,10 10 0),(5 5 0,5 6 0,6 6 0,6 5 0,5 5 0))", -1)]
        public void BasicConversion(string wkt, int srid)
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
