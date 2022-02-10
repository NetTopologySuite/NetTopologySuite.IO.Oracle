using NUnit.Framework;
using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using NetTopologySuite.IO;
using System.Configuration;

namespace NetTopologySuite.IO.Oracle.Connection.Test
{

    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class IntegrationTest
    {
        private const string testTableName = "NTS_TEST_GEO_DATA";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            try
            {
                string cns = ConfigurationManager.AppSettings.Get("TestDBConnectionString");
                TestContext.WriteLine("Trying to connect with '{0}'", cns);
                var cnsb = new OracleConnectionStringBuilder(cns);
                //cnsb.DataSource
                using var conn = new OracleConnection(cnsb.ConnectionString);
                conn.Open();
            } catch (Exception ex)
            {
                TestContext.WriteLine(ex.Message);
                TestContext.WriteLine(ex.StackTrace);
                Assert.Ignore("Connection to Oracle database server failed");
            }
        }

        [Test]
        /// <summary>
        /// Connect to database, make a table, write to database, read to database, drop table.
        /// Assumption GEO_DATA table exists.
        /// </summary>
        /// <param name="wkt"></param>
        [TestCase("POINT(10 10)")]
        [TestCase("POINT(10 10)")]
        [TestCase("POINT Z(10 10 0)")]
        [TestCase("POINT Z(10 10 20)")]
        [TestCase("MULTIPOINT(11 12, 20 20)")]
        [TestCase("MULTIPOINT Z(11 12 12, 20 20 20)")]
        [TestCase("LINESTRING(10 10,20 20,50 50,34 34)")]
        [TestCase("LINESTRING(3383.69840913624 0.860795155102757,20 0.860795155102757,0.860795155102757 0.222096232800789,0.222096232800789 0.222096232800789)")]
        [TestCase("LINESTRING Z(10 10 20,20 20 20,50 50 50,34 34 34)")]
        [TestCase("POLYGON((10 10,20 10,20 20,10 20,10 10))")]
        [TestCase("POLYGON((10 10,20 10,20 20,10 20,10 10),(5 5,5 6,6 6,6 5,5 5))")]
        [TestCase("POLYGON Z((10 10 0,20 10 0,20 20 0,10 20 0,10 10 0),(5 5 0,5 6 0,6 6 0,6 5 0,5 5 0))")]
        [TestCase("MULTIPOLYGON(((10 10,20 10,20 20,20 10,10 10)),((10 10,20 10,20 20,20 10,10 10)))")]
        [TestCase("MULTIPOLYGON(((10 10,20 10,20 20,10 20,10 10),(5 5,5 6,6 6,6 5,5 5)),((10 10,20 10,20 20,20 10,10 10)))")]
        [TestCase("MULTIPOLYGON(((10 10,20 10,20 20,10 20,10 10),(5 5,5 6,6 6,6 5,5 5)),((10 10,20 10,20 20,20 10,10 10),(5 5,5 6,6 6,6 5,5 5)))")]
        [TestCase("MULTIPOLYGON Z(((10 10 0,20 10 0,20 20 0,10 20 0,10 10 0),(5 5 0,5 6 0,6 6 0,6 5 0,5 5 0)),((10 10 0,20 10 0,20 20 0,20 10 0,10 10 0),(5 5 0,5 6 0,6 6 0,6 5 0,5 5 0)))")]
        [TestCase("MULTILINESTRING((10 10,20 10,20 20,20 10),(5 5,5 6,6 6,6 5))")]
        [TestCase("MULTILINESTRING Z((10 10 5,20 10 5,20 20 0,20 10 0,10 10 0),(5 5 0,5 6 0,6 6 0,6 5 0,5 5 0))")]
        [TestCase("GEOMETRYCOLLECTION(POLYGON((10 10,20 10,20 20,10 20,10 10)),POLYGON((30 10,40 10,40 20,30 20,30 10)))")]
        [TestCase("GEOMETRYCOLLECTION(POLYGON((10 10,20 10,20 20,10 20,10 10),(5 5,5 6,6 6,6 5,5 5)))")]
        [TestCase("GEOMETRYCOLLECTION(POLYGON((10 10,20 10,20 20,10 20,10 10),(5 5,5 6,6 6,6 5,5 5)),LINESTRING(10 10,20 20,50 50,34 34))")]
        [TestCase("GEOMETRYCOLLECTION(POINT(10 10),LINESTRING(10 10,20 20,50 50,34 34))")]
        [TestCase("GEOMETRYCOLLECTION(POINT(10 10),MULTIPOINT(11 12, 20 20))")]

        public void TestWritingAndReadingBackFromGeometryTable(string wkt)
        {
            // Open a connection.
            var connection = OracleHelper.OpenConnection();

            // Make a new table.v
            string res = OracleHelper.CreateGeometryTable(connection, testTableName);
            // TODO this is pretty dumb, need to check exact output
            Assert.IsTrue(!string.IsNullOrWhiteSpace(res));

            // Write current geometry to table.
            var geom = OracleHelper.WriteGeometryToTable(wkt, testTableName);

            // Read current geometry from table.
            var geom2 = OracleHelper.ReadGeometryFromTable(testTableName);

            Assert.IsTrue(geom.EqualsExact(geom2));

            // Drop Geometry table
            OracleHelper.DropGeometryTable(connection, testTableName);
        }


    }



}
