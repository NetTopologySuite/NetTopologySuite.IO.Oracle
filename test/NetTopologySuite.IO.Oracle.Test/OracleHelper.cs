using Oracle.ManagedDataAccess.Client;
using System.Configuration;

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
            var conStringUser = ConfigurationManager.AppSettings["TestDBConnectionString"];
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
    }
}
