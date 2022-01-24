using NUnit.Framework;
using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using NetTopologySuite.IO;

namespace NetTopologySuite.IO.Oracle.Connection.Test
{

    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class IntegrationTest
    {
        private const string testTableName = "NTS_TEST_GEO_DATA";

        /// <summary>
        /// Connect to the database
        /// </summary>
        [Test]
        public void TestConnection()
        {
            OracleHelper.OpenConnection();
        }

        [Test]
        public void TestCreateGeometryTable()
        {

            using var connection = OracleHelper.OpenConnection();

            string res = OracleHelper.CreateGeometryTable(connection, testTableName);
            OracleHelper.DropGeometryTable(connection, testTableName);

            // TODO this is pretty dumb, need to check exact output
            Assert.IsTrue(!string.IsNullOrWhiteSpace(res));
        }

        [Test]
        // Assumption GEO_DATA table exists.
        // Write a new created Geometry object to the database.
        public void WriteGeometryTable()
        {
            // Read WKT into geometry object.
            var wr = new WKTReader { IsOldNtsCoordinateSyntaxAllowed = false };
            var correctCCW = "POLYGON((10 10, 20 10, 20 20, 10 20, 10 10),(5 5,5 6,6 6,6 5,5 5))";
            var geom = wr.Read(correctCCW);
            Console.WriteLine("Geometry: {0}", geom);

            // Write geometry object into UDT object.
            var oracleWriter = new OracleGeometryWriter();
            var udt = oracleWriter.Write(geom);

            // Open connection
            using var connection = OracleHelper.OpenConnection();

            // Drop & Create Geometry table.
            OracleHelper.CreateGeometryTable(connection, testTableName);

            var queryString = $"INSERT INTO {testTableName} (data) VALUES (:geo)";

            using OracleCommand command = new OracleCommand(queryString, connection);
            var geometryParam = new OracleParameter()
            {
                ParameterName= "geo",
                DbType = DbType.Object,
                Value = udt,
                Direction = ParameterDirection.Input,
                UdtTypeName = "MDSYS.SDO_GEOMETRY"
            };
            command.Parameters.Add(geometryParam);
            command.ExecuteNonQuery();

            // Clean up: drop table again
            OracleHelper.DropGeometryTable(connection, testTableName);
        }


    }



}
