namespace NetTopologySuite.IO
{
    /// <summary>
    /// Utility class for common settings around <see cref="OracleGeometryReader"/>
    /// and <see cref="OracleGeometryWriter"/>.
    /// </summary>
    public class OracleGeometrySettings
    {
        /// <summary>
        /// Gets or sets a value indicating the SRID value that indicates <c>NULL</c> for <c>SDO_GEOMETRY.SDO_SRID</c>.
        /// </summary>
        /// <remarks>
        /// If this value is not set, every SDO_GEOMETRY written will have an 
        /// </remarks>
        public static int? SRIDNullValue {  get; set; }
    }
}
