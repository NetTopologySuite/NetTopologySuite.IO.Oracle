using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Data;
using NUnit.Framework;
using NetTopologySuite.IO.Sdo;

namespace NetTopologySuite.IO.Oracle.Connection.Test
{
    public static class OracleHelper
    {

        

        /// <summary>
        /// Opens a connection to the test database
        /// </summary>
        /// <returns></returns>
        public static OracleConnection OpenConnection()
        {
            var user = "SYS";
            var pwd = "mysecurepassword";
            var privilege = "SYSDBA";
            var db = " (DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = localhost)(PORT = 1521)) (CONNECT_DATA =(SERVER = DEDICATED)(SERVICE_NAME = ORALSID)))";
            var conStringUser = "User Id=" + user + ";Password=" + pwd + ";Data Source=" + db + ";DBA Privilege=" + privilege + ";";
            
            OracleConnection con = new OracleConnection(conStringUser);
            con.Open();
            return con;
        }

        /// <summary>
        /// Drops (if it exists) and recreates the given table.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tableName"> </param>
        /// <returns>The name of the table as returned from sys.all_tables</returns>
        public static string CreateGeometryTable(OracleConnection connection, string tableName)
        {
            OracleHelper.DropGeometryTable(connection, tableName);

            var queryString = $"CREATE TABLE {tableName} (data MSYS.SDO_GEOMETRY)";
            using OracleCommand command = new OracleCommand(queryString, connection);
            command.ExecuteNonQuery();

            var queryString2 = $"SELECT TABLE_NAME FROM sys.all_tables WHERE TABLE_NAME = '{tableName}'";
            using OracleCommand command2 = new OracleCommand(queryString2, connection);
            return (string)command2.ExecuteScalar();
        }

        /// <summary>
        /// Drops (if it exists) the given table.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tableName"></param>
        public static void DropGeometryTable(OracleConnection connection, string tableName)
        {
            // Remove 'GEO_DATA' table
            var queryString = $@"BEGIN
                EXECUTE IMMEDIATE 'DROP TABLE {tableName}';
                EXCEPTION
                    WHEN OTHERS THEN
                        IF SQLCODE != -942 THEN
                            RAISE;
                        END IF;
                END;";
            using OracleCommand command = new OracleCommand(queryString, connection);
            command.ExecuteNonQuery();
        }


        /// <summary>
        /// Assumption GEO_DATA table exists.
        /// Write a new created Geometry object to the database.
        /// </summary>
        public static Geometries.Geometry WriteGeometryToTable(string wkt, string testTableName)
        {
            var geom = ConvertWKTToGeometry(wkt);
            SdoGeometry udt = ConvertWKTToOracleUDT(geom);

            // Open connection
            using var connection = OpenConnection();

            // Drop & Create Geometry table.
            CreateGeometryTable(connection, testTableName);

            var queryString = $"INSERT INTO {testTableName} (data) VALUES (:geo)";

            using OracleCommand command = new OracleCommand(queryString, connection);
            var geometryParam = new OracleParameter()
            {
                ParameterName = "geo",
                DbType = DbType.Object,
                Value = udt,
                Direction = ParameterDirection.Input,
                UdtTypeName = "MDSYS.SDO_GEOMETRY"
            };
            command.Parameters.Add(geometryParam);
            command.ExecuteNonQuery();

            return geom;
        }

        private static Sdo.SdoGeometry ConvertWKTToOracleUDT(Geometries.Geometry geom)
        {
            // Write geometry object into UDT object.
            var oracleWriter = new OracleGeometryWriter();
            var udt = oracleWriter.Write(geom);
            return udt;
        }

        private static Geometries.Geometry ConvertWKTToGeometry(string wkt)
        {
            // Read WKT into geometry object.
            var wr = new WKTReader { IsOldNtsCoordinateSyntaxAllowed = false };
            var correctCCW = wkt;
            var geom = wr.Read(correctCCW);
            return geom;
        }

        /// <summary>
        /// Read a newly created Geometry object from the database.
        /// Assumption GEO_DATA table exists.
        /// </summary>
        public static Geometries.Geometry ReadGeometryFromTable(string testTableName)
        {
            // Open connection
            using var connection = OpenConnection();

            // Write query string & command
            var queryString = $"SELECT * FROM {testTableName}";
            using OracleCommand command = new OracleCommand(queryString, connection);

            var geometryParam = new OracleParameter();
            command.Parameters.Add(geometryParam);
            var res = (SdoGeometry) command.ExecuteScalar();

            var oracleReader = new OracleGeometryReader();
            var geom2 = oracleReader.Read(res);

            return geom2;
        }


    }
}
