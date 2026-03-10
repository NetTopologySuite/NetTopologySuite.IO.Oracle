using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.IO.Oracle.Test
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class OracleTest
    {

        private static readonly OracleGeometryReader or = new OracleGeometryReader(NtsGeometryServices.Instance);
        private static readonly WKTReader wr = new WKTReader { IsOldNtsCoordinateSyntaxAllowed = false,  };
        private static readonly WKTWriter ww = new WKTWriter(4);
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void CCWTestsOnPolygon()
        {
            string wrongCCW = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10),(5 5,6 5,6 6,5 6,5 5))";
            string correctCCW = "POLYGON((10 10, 20 10, 20 20, 10 20, 10 10),(5 5,5 6,6 6,6 5,5 5))";

            var geom1 = wr.Read(wrongCCW);
            var geom2 = wr.Read(correctCCW);

            var t = new OracleGeometryWriter().Write(geom1);
            var geom3 = or.Read(t);
            Assert.That(geom2.EqualsExact(geom3));
        }

        /// <summary>
        /// Tests all geometry types by transforming from wkt to oracle and back
        /// </summary>
        /// <param name="wkt"></param>
        /// <param name="srid"></param>
        [TestCase("POINT(10 10)", -1)]
        [TestCase("POINT(10 10)", 4326)]
        [TestCase("POINT Z(10 10 0)", -1)]
        [TestCase("POINT Z(10 10 0)", 4326)]
        [TestCase("POINT Z(10 10 20)", -1)]
        [TestCase("POINT Z(10 10 20)", 4326)]
        [TestCase("POINT M(10 10 30)", -1)]
        [TestCase("POINT M(10 10 30)", 4326)]
        [TestCase("POINT ZM(10 10 20 30)", -1)]
        [TestCase("POINT ZM(10 10 20 30)", 4326)]
        [TestCase("MULTIPOINT(11 12)", -1)]
        [TestCase("MULTIPOINT(11 12, 20 20)", -1)]
        [TestCase("MULTIPOINT Z(11 12 12, 20 20 20)", -1)]
        [TestCase("LINESTRING(10 10,20 20,50 50,34 34)", -1)]
        [TestCase("LINESTRING Z(10 10 20,20 20 20,50 50 50,34 34 34)", -1)]
        [TestCase("LINESTRING ZM(10 10 20 30,20 20 20 30,50 50 50 30,34 34 34 30)", -1)]
        [TestCase("POLYGON((10 10,20 10,20 20,10 20,10 10))", -1)]
        [TestCase("POLYGON((10 10,20 10,20 20,10 20,10 10),(5 5,5 6,6 6,6 5,5 5))", -1)]
        [TestCase("POLYGON Z((10 10 0,20 10 0,20 20 0,10 20 0,10 10 0),(5 5 0,5 6 0,6 6 0,6 5 0,5 5 0))", -1)]
        [TestCase("POLYGON ZM((10 10 0 30,20 10 0 31,20 20 0 32,10 20 0 33,10 10 0 30),(5 5 0 30,5 6 0 31,6 6 0 32,6 5 0 33,5 5 0 30))", -1)]
        [TestCase("MULTIPOLYGON(((10 10,20 10,20 20,20 10,10 10)))", -1)]
        [TestCase("MULTIPOLYGON(((10 10,20 10,20 20,20 10,10 10)),((10 10,20 10,20 20,20 10,10 10)))", -1)]
        [TestCase("MULTIPOLYGON(((10 10,20 10,20 20,10 20,10 10),(5 5,5 6,6 6,6 5,5 5)),((10 10,20 10,20 20,20 10,10 10)))", -1)]
        [TestCase("MULTIPOLYGON(((10 10,20 10,20 20,10 20,10 10),(5 5,5 6,6 6,6 5,5 5)),((10 10,20 10,20 20,20 10,10 10),(5 5,5 6,6 6,6 5,5 5)))", -1)]
        [TestCase("MULTIPOLYGON Z(((10 10 0,20 10 0,20 20 0,10 20 0,10 10 0),(5 5 0,5 6 0,6 6 0,6 5 0,5 5 0)),((10 10 0,20 10 0,20 20 0,20 10 0,10 10 0),(5 5 0,5 6 0,6 6 0,6 5 0,5 5 0)))", -1)]
        [TestCase("MULTILINESTRING((10 10,20 10,20 20,20 10))", -1)]
        [TestCase("MULTILINESTRING((1 1, 2 1, 3 1), (1 2, 2 2, 3 2, 4 2), (1 3, 1 3, 3 3, 4 3))", -1)]
        [TestCase("MULTILINESTRING((1 1, 2 1, 3 1), (1 2, 2 2, 3 2, 4 2), (1 3, 1 3, 3 3, 4 3),(1 5, 2 5, 3 5),(1 6, 2 6, 3 6, 4 6))", -1)]
        [TestCase("MULTILINESTRING Z((10 10 5,20 10 5,20 20 0,20 10 0,10 10 0),(5 5 0,5 6 0,6 6 0,6 5 0,5 5 0))", -1)]
        [TestCase("GEOMETRYCOLLECTION(POLYGON((10 10,20 10,20 20,10 20,10 10)),POLYGON((30 10,40 10,40 20,30 20,30 10)))", -1)]
        [TestCase("GEOMETRYCOLLECTION(POLYGON((10 10,20 10,20 20,10 20,10 10),(5 5,5 6,6 6,6 5,5 5)))", -1)]
        [TestCase("GEOMETRYCOLLECTION(POLYGON((10 10,20 10,20 20,10 20,10 10),(5 5,5 6,6 6,6 5,5 5)),LINESTRING(10 10,20 20,50 50,34 34))", -1)]
        [TestCase("GEOMETRYCOLLECTION(POINT(10 10),LINESTRING(10 10,20 20,50 50,34 34))", -1)]
        [TestCase("GEOMETRYCOLLECTION(POINT(10 10),MULTIPOINT(11 12, 20 20))", -1)]
        public void BasicConversion(string wkt, int srid)
        {
            wr.DefaultSRID = srid;
            var geom = wr.Read(wkt);
            Assert.That(geom.SRID, Is.EqualTo(srid));

            string parsed = ww.Write(geom);

            var t = new OracleGeometryWriter().Write(geom);
            var geomRead = or.Read(t);

            Assert.That(geomRead.EqualsExact(geom), Is.True);
            Assert.That(geomRead.SRID, Is.EqualTo(geom.SRID));
            Assert.That(ww.Write(geomRead), Is.EqualTo(parsed));
        }

        /// <summary>
        /// Tests geometry collection with multitypes 
        /// </summary>
        /// <param name="wkt"></param>
        /// <param name="wktresult"></param>
        /// <param name="srid"></param>
        [TestCase("GEOMETRYCOLLECTION(MULTIPOINT(11 12, 20 20))", "GEOMETRYCOLLECTION(MULTIPOINT(11 12, 20 20))", - 1)]
        [TestCase("GEOMETRYCOLLECTION(MULTIPOLYGON(((10 10,20 10,20 20,10 20,10 10),(5 5,5 6,6 6,6 5,5 5)),((10 10,20 10,20 20,20 10,10 10),(5 5,5 6,6 6,6 5,5 5))))", "GEOMETRYCOLLECTION(POLYGON((10 10,20 10,20 20,10 20,10 10),(5 5,5 6,6 6,6 5,5 5)),POLYGON((10 10,20 10,20 20,20 10,10 10),(5 5,5 6,6 6,6 5,5 5)))", - 1)]
        [TestCase("GEOMETRYCOLLECTION(MULTILINESTRING((10 10,20 10,20 20,10 20,10 10),(5 5,5 6,6 6,6 5,5 5)))", "GEOMETRYCOLLECTION(LINESTRING(10 10,20 10,20 20,10 20,10 10),LINESTRING(5 5,5 6,6 6,6 5,5 5))", -1)]
        public void CollectionConversion(string wkt, string wktresult, int srid)
        {
            var geom = wr.Read(wkt);
            var result = wr.Read(wktresult);

            geom.SRID = srid;
            result.SRID = srid;

            var t = new OracleGeometryWriter().Write(geom);
            var regeom = or.Read(t);
            Assert.That(result.EqualsExact(regeom));
        }

        [Test]
        public void TestIssue23()
        {
            var rdr = new WKTReader();
            rdr.DefaultSRID = 4326;
            var geom = rdr.Read("GEOMETRYCOLLECTION(POINT(1 1), LINESTRING(2 2, 3 3), POLYGON((4 4, 4 6, 6 6, 6 4, 4 4)))");
            var t = new OracleGeometryWriter().Write(geom);
            Assert.That(true);
        }

        [Test]
        public void TestIssue25()
        {
            // Create a reader
            var reader = new OracleGeometryReader(NtsGeometryServices.Instance);

            // MultiPoint 19c
            var sdo19c = new Sdo.SdoGeometry
            {
                SdoGtype = 2005,
                Sdo_Srid = 1,
                Point = null,
                ElemArray = new double[] { 1, 1, 1, 3, 1, 1, 5, 1, 1 },
                OrdinatesArray = new double[] { 0, 0, 1, 1, 1, 0 }
            };
            var nts19c = reader.Read(sdo19c);

            // MultiPoint 19c
            var sdo23ai = new Sdo.SdoGeometry
            {
                SdoGtype = 2005,
                Sdo_Srid = 1,
                Point = null,
                ElemArray = new double[] { 1, 1, 3 },
                OrdinatesArray = new double[] { 0, 0, 1, 1, 1, 0 }
            };
            var nts23ai = reader.Read(sdo23ai);

            Assert.That(nts19c.EqualsExact(nts23ai));
        }

        [TestCase(666, null, null, null, 666)]
        [TestCase(666, null, 333, null, 666)]
        [TestCase(666, 333, null, null, 333)]
        [TestCase(666, null, 333, 333, 333)]
        [TestCase(666, null, 333, 111, 111)]
        [TestCase(666, 9, 333, 111, 9)]
        public void TestIssue27(int sridServices, int? sridWriter, int? sridNull, int? sridGeometry, int expectedSrid)
        {
            var services = new NtsGeometryServices(CoordinateArraySequenceFactory.Instance, new Geometries.PrecisionModel(), sridServices);

            // Create the writer
            var writer = new OracleGeometryWriter();
            if (sridWriter.HasValue) writer.SRID = sridWriter.Value;

            // Set SRID value to be interpreted as NULL
            OracleGeometrySettings.SRIDNullValue = sridNull;

            // Create the geometry factory
            var factory = sridGeometry.HasValue
                ? services.CreateGeometryFactory(sridGeometry.Value)
                : services.CreateGeometryFactory();

            // Create and write point
            var geomS = factory.CreatePoint(new Coordinate(10, 10));
            var sdoGeom = writer.Write(geomS);

            // Create reader and read point
            var reader = new OracleGeometryReader(services);
            var geomD = reader.Read(sdoGeom);

            // Assert SRID has expected value
            Assert.That(geomD.SRID, Is.EqualTo(expectedSrid));
        }
    }
}
