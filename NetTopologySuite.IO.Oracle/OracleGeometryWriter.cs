using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Sdo;
using Oracle.DataAccess.Client;

namespace NetTopologySuite.IO
{
    /**
     * 
     * Translates a NTS Geometry into an Oracle UDT. 
     * 
     */
    public class OracleGeometryWriter
    {
        private const int SridNull = -1;
        private int _srid = SridNull;

        /// <summary>
        /// Explicit paramter less constructor
        /// </summary>
        public OracleGeometryWriter()
        {
            SRID = SridNull;
        }

        /// <summary>
        /// Property for spatial reference system
        /// </summary>
        public int SRID
        {
            get { return _srid; }
            set { _srid = value; }
        }

        private int Dimension(IGeometry geom)
        {
            var d = Double.IsNaN(geom.Coordinate.Z) ? 2 : 3;
            return d;
        }

        private int GType(IGeometry geom)
        {
            int d = Dimension(geom) * 1000;
            const int l = 0;
            var tt = (int)Template(geom);

            return (d + l + tt);
        }

        /// <summary>
        /// Converts an IGeometry to the corresponding Oracle UDT of type SdoGeometry
        /// it returns null, if conversion fails
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns>SdoGeometry</returns>
        public SdoGeometry Write(IGeometry geometry)
        {
            if (geometry.IsEmpty)
                return null;

            if (geometry is IPoint)
                return Write(geometry as IPoint);
            if (geometry is ILinearRing)
                return Write(geometry as ILinearRing);
            if (geometry is ILineString)
                return Write(geometry as ILineString);
            if (geometry is IPolygon)
                return Write(geometry as IPolygon);
            if (geometry is IMultiPoint)
                return Write(geometry as IMultiPoint);
            if (geometry is IMultiLineString)
                return Write(geometry as IMultiLineString);
            if (geometry is IMultiPolygon)
                return Write(geometry as IMultiPolygon);
            if (geometry is IGeometryCollection)
                return Write(geometry as IGeometryCollection);

            throw new ArgumentException("Geometry not supported: " + geometry);
        }

        private SdoGeometry Write(IPoint point)
        {
            var sdoGeometry = new SdoGeometry
            {
                Point = new SdoPoint
                {
                    X = (decimal)point.Coordinate.X,
                    Y = (decimal)point.Coordinate.Y
                },
                Sdo_Srid = point.SRID,
                SdoGtype = GType(point)
            };

            if (Dimension(point) == 3)
                sdoGeometry.Point.Z = (decimal)point.Coordinate.Z;

            return sdoGeometry;
        }

        private SdoGeometry Write(ILinearRing ring)
        {
            return new SdoGeometry()
            {
                SdoGtype = GType(ring),
                Sdo_Srid = ring.SRID,
                ElemArray = new decimal[] { 1, 1003, 1 },
                OrdinatesArray = GetOrdinates(ring).ToArray()
            };
        }

        private SdoGeometry Write(ILineString ring)
        {
            return new SdoGeometry()
            {
                SdoGtype = GType(ring),
                Sdo_Srid = ring.SRID,
                ElemArray = new decimal[] { 1, 2, 1 },
                OrdinatesArray = GetOrdinates(ring).ToArray()
            };
        }

        private SdoGeometry Write(IPolygon polygon)
        {
            var sdoGeometry = new SdoGeometry
            {
                SdoGtype = GType(polygon),
                Sdo_Srid = polygon.SRID,
            };

            var elemInfoList = new List<decimal>();
            var ordinateList = new List<decimal>();
            var pos = 1;

            pos = ProcessPolygon(polygon, elemInfoList, ordinateList, pos);

            sdoGeometry.ElemArray = elemInfoList.ToArray();
            sdoGeometry.OrdinatesArray = ordinateList.ToArray();

            return sdoGeometry;
        }

        private int ProcessPolygon(IPolygon polygon, List<decimal> elemInfoList, List<decimal> ordinateList, int pos)
        {
            elemInfoList.AddRange(new List<decimal>() { pos, 1003, 1 });
            var exteriorOrdinates = GetOrdinates(polygon.ExteriorRing);
            ordinateList.AddRange(exteriorOrdinates);
            pos += exteriorOrdinates.Count;
            foreach (var ring in polygon.InteriorRings)
            {
                elemInfoList.AddRange(new List<decimal>() { pos, 2003, 1 });
                var interiorOrdinates = GetOrdinates(ring);
                ordinateList.AddRange(interiorOrdinates);
                pos += interiorOrdinates.Count;
            }

            return pos;
        }

        private SdoGeometry Write(IMultiPoint multiPoint)
        {
            var sdoGeometry = new SdoGeometry { SdoGtype = GType(multiPoint), Sdo_Srid = multiPoint.SRID };

            var elemInfoList = new List<decimal>();
            var ordinateList = new List<decimal>();
            var pos = 1;
            elemInfoList.AddRange(new List<decimal>() { pos, 1, multiPoint.NumGeometries });
            foreach (var point in multiPoint.Geometries)
            {
                var p = point as IPoint;
                var ordinates = new List<decimal> { (decimal)p.X, (decimal)p.Y };
                if (Dimension(point) == 3)
                    ordinates.Add((decimal)p.Z);
                ordinateList.AddRange(ordinates);
            }

            sdoGeometry.ElemArray = elemInfoList.ToArray();
            sdoGeometry.OrdinatesArray = ordinateList.ToArray();

            return sdoGeometry;
        }

        private SdoGeometry Write(IMultiLineString multiLineString)
        {
            var sdoGeometry = new SdoGeometry { SdoGtype = GType(multiLineString), Sdo_Srid = multiLineString.SRID };

            var elemInfoList = new List<decimal>();
            var ordinateList = new List<decimal>();
            var pos = 1;
            foreach (var line in multiLineString.Geometries)
            {
                elemInfoList.AddRange(new List<decimal>() { pos, 2, 1 });
                var ordinates = GetOrdinates(line as ILineString);
                ordinateList.AddRange(ordinates);
                pos += ordinates.Count;
            }

            sdoGeometry.ElemArray = elemInfoList.ToArray();
            sdoGeometry.OrdinatesArray = ordinateList.ToArray();

            return sdoGeometry;
        }

        SdoGeometry Write(IMultiPolygon multiPolygon)
        {
            var sdoGeometry = new SdoGeometry { SdoGtype = GType(multiPolygon), Sdo_Srid = multiPolygon.SRID };


            var elemInfoList = new List<decimal>();
            var ordinateList = new List<decimal>();
            var pos = 1;

            foreach (var poly in multiPolygon.Geometries)
            {
                pos = ProcessPolygon(poly as IPolygon, elemInfoList, ordinateList, pos);
            }

            sdoGeometry.ElemArray = elemInfoList.ToArray();
            sdoGeometry.OrdinatesArray = ordinateList.ToArray();

            return sdoGeometry;
        }

        private SdoGeometry Write(IGeometryCollection geometryCollection)
        {
            return new SdoGeometry();
        }

        private List<decimal> GetOrdinates(ILineString lineString)
        {
            var ords = new List<decimal>();
            var numOfPoints = lineString.NumPoints;
            for (var i = 0; i < numOfPoints; i++)
            {
                ords.Add((decimal)lineString.GetCoordinateN(i).X);
                ords.Add((decimal)lineString.GetCoordinateN(i).Y);
                if (Dimension(lineString) == 3)
                    ords.Add((decimal)lineString.GetCoordinateN(i).Z);
            }

            return ords;
        }


        private SdoGTemplate Template(IGeometry geom)
        {
            if (geom == null)
            {
                return SdoGTemplate.Unknown;
            }
            if (geom is IPoint)
            {
                return SdoGTemplate.Coordinate;
            }
            if (geom is ILineString)
            {
                return SdoGTemplate.Line;
            }
            if (geom is IPolygon)
            {
                return SdoGTemplate.Polygon;
            }
            if (geom is IMultiPoint)
            {
                return SdoGTemplate.MultiPoint;
            }
            if (geom is IMultiLineString)
            {
                return SdoGTemplate.MultiLine;
            }
            if (geom is IMultiPolygon)
            {
                return SdoGTemplate.MultiPolygon;
            }
            if (geom is IGeometryCollection)
            {
                return SdoGTemplate.Collection;
            }

            throw new ArgumentException("Cannot encode JTS "
                + geom.GeometryType + " as SDO_GTEMPLATE "
                + "(Limitied to Point, Line, Polygon, GeometryCollection, MultiPoint,"
                + " MultiLineString and MultiPolygon)");
        }       
    }
}


