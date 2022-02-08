using NetTopologySuite.IO.UdtBase;
using Oracle.ManagedDataAccess.Types;

namespace NetTopologySuite.IO.Sdo
{
    [OracleCustomTypeMapping("MDSYS.SDO_POINT_TYPE")]
    public class SdoPoint : OracleCustomTypeBase<SdoPoint>
    {
        [OracleObjectMapping("X")]
        public double? X { get; set; }

        [OracleObjectMapping("Y")]
        public double? Y { get; set; }

        [OracleObjectMapping("Z")]
        public double? Z { get; set; }

        public override void MapFromCustomObject()
        {
            SetValue("X", X);
            SetValue("Y", Y);
            SetValue("Z", Z);
        }

        public override void MapToCustomObject()
        {
            X = GetValue<double?>("X");
            Y = GetValue<double?>("Y");
            Z = GetValue<double?>("Z");
        }
    }
}
