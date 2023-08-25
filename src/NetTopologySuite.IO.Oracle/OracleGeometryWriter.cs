using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Sdo;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Translates a NTS Geometry into an Oracle UDT.
    /// </summary>
    public class OracleGeometryWriter
    {
        private const int SridNull = -1;
        private int dimension = 0;

        /// <summary>
        /// Property for spatial reference system
        /// </summary>
        public int SRID { get; set; } = SridNull;

        private int Dimension(Geometry geom)
        {
            return double.IsNaN(geom.Coordinate.Z) ? 2 : 3;
        }

        private int GType(Geometry geom)
        {
            return Dimension(geom) * 1000 + (int)Template(geom);
        }

        /// <summary>
        /// Converts an Geometry to the corresponding Oracle UDT of type SdoGeometry
        /// it returns null, if conversion fails
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns>SdoGeometry</returns>
        public SdoGeometry Write(Geometry geometry)
        {
            if (geometry?.IsEmpty != false)
            {
                return null;
            }

            this.dimension = Dimension(geometry);

            switch (geometry)
            {
                case Point point:
                    return Write(point);

                case LineString line:
                    return Write(line);

                case Polygon polygon:
                    return Write(polygon);

                case MultiPoint multiPoint:
                    return Write(multiPoint);

                case MultiLineString multiLineString:
                    return Write(multiLineString);

                case MultiPolygon multiPolygon:
                    return Write(multiPolygon);

                case GeometryCollection collection:
                    return Write(collection);

                default:
                    throw new ArgumentException("Geometry not supported: " + geometry);
            }
        }

        private SdoGeometry Write(Point point)
        {
            var elemInfoList = new List<double>();
            var ordinateList = new List<double>();

            ProcessPoint(point, elemInfoList, ordinateList, 1);

            return new SdoGeometry()
            {
                SdoGtype = GType(point),
                Sdo_Srid = point.SRID,
                ElemArray = elemInfoList.ToArray(),
                OrdinatesArray = ordinateList.ToArray(),
            };
        }

        private SdoGeometry Write(LineString line)
        {
            var elemInfoList = new List<double>();
            var ordinateList = new List<double>();

            ProcessLinear(line, elemInfoList, ordinateList, 1);

            return new SdoGeometry()
            {
                SdoGtype = GType(line),
                Sdo_Srid = line.SRID,
                ElemArray = elemInfoList.ToArray(),
                OrdinatesArray = ordinateList.ToArray(),
            };
        }

        private SdoGeometry Write(Polygon polygon)
        {
            var elemInfoList = new List<double>();
            var ordinateList = new List<double>();

            ProcessPolygon(polygon, elemInfoList, ordinateList, 1);

            return new SdoGeometry
            {
                SdoGtype = GType(polygon),
                Sdo_Srid = polygon.SRID,
                ElemArray = elemInfoList.ToArray(),
                OrdinatesArray = ordinateList.ToArray(),
            };
        }

        private SdoGeometry Write(MultiPoint multiPoint)
        {
            var elemInfoList = new List<double>();
            var ordinateList = new List<double>();

            ProcessMultiPoint(multiPoint, elemInfoList, ordinateList, 1);

            return new SdoGeometry
            {
                SdoGtype = GType(multiPoint),
                Sdo_Srid = multiPoint.SRID,
                ElemArray = elemInfoList.ToArray(),
                OrdinatesArray = ordinateList.ToArray(),
            };
        }

        private SdoGeometry Write(MultiLineString multiLineString)
        {
            var elemInfoList = new List<double>();
            var ordinateList = new List<double>();

            ProcessMultiLineString(multiLineString, elemInfoList, ordinateList, 1);

            return new SdoGeometry
            {
                SdoGtype = GType(multiLineString),
                Sdo_Srid = multiLineString.SRID,
                ElemArray = elemInfoList.ToArray(),
                OrdinatesArray = ordinateList.ToArray(),
            };
        }

        private SdoGeometry Write(MultiPolygon multiPolygon)
        {
            var elemInfoList = new List<double>();
            var ordinateList = new List<double>();

            ProcessMultiPolygon(multiPolygon, elemInfoList, ordinateList, 1);

            return new SdoGeometry
            {
                SdoGtype = GType(multiPolygon),
                Sdo_Srid = multiPolygon.SRID,
                ElemArray = elemInfoList.ToArray(),
                OrdinatesArray = ordinateList.ToArray(),
            };
        }

        private SdoGeometry Write(GeometryCollection geometryCollection)
        {
            var elemInfoList = new List<double>();
            var ordinateList = new List<double>();
            int pos = 1;

            int cnt = geometryCollection.NumGeometries;
            for (int i = 0; i < cnt; i++)
            {
                var geom = geometryCollection.GetGeometryN(i);
                switch (geom.OgcGeometryType)
                {
                    case OgcGeometryType.Point:
                        pos = ProcessPoint((Point)geom, elemInfoList, ordinateList, pos);
                        break;

                    case OgcGeometryType.LineString:
                        pos = ProcessLinear((LineString)geom, elemInfoList, ordinateList, pos);
                        break;

                    case OgcGeometryType.Polygon:
                        pos = ProcessPolygon((Polygon)geom, elemInfoList, ordinateList, pos);
                        break;

                    case OgcGeometryType.MultiPoint:
                        pos = ProcessMultiPoint((MultiPoint)geom, elemInfoList, ordinateList, pos);
                        break;

                    case OgcGeometryType.MultiLineString:
                        pos = ProcessMultiLineString((MultiLineString)geom, elemInfoList, ordinateList, pos);
                        break;

                    case OgcGeometryType.MultiPolygon:
                        pos = ProcessMultiPolygon((MultiPolygon)geom, elemInfoList, ordinateList, pos);
                        break;

                    default:
                        throw new ArgumentException("Geometry not supported in GeometryCollection: " + geom);
                }
            }

            return new SdoGeometry
            {
                SdoGtype = GType(geometryCollection),
                Sdo_Srid = geometryCollection.SRID,
                ElemArray = elemInfoList.ToArray(),
                OrdinatesArray = ordinateList.ToArray(),
            };
        }

        private int ProcessPoint(Point point, List<double> elemInfoList, List<double> ordinateList, int pos)
        {
            elemInfoList.Add(pos);
            elemInfoList.Add((int)SdoEType.Coordinate);
            elemInfoList.Add(1);
            return pos + AddOrdinates(point.CoordinateSequence, ordinateList);
        }

        private int ProcessLinear(LineString line, List<double> elemInfoList, List<double> ordinateList, int pos)
        {
            elemInfoList.Add(pos);
            elemInfoList.Add((int)SdoEType.Line);
            elemInfoList.Add(1);
            return pos + AddOrdinates(line.CoordinateSequence, ordinateList);
        }

        private int ProcessPolygon(Polygon polygon, List<double> elemInfoList, List<double> ordinateList, int pos)
        {
            elemInfoList.Add(pos);
            elemInfoList.Add((int)SdoEType.PolygonExterior);
            elemInfoList.Add(1);

            var exteriorRingCoords = polygon.ExteriorRing.CoordinateSequence;
            pos += Algorithm.Orientation.IsCCW(exteriorRingCoords)
                ? AddOrdinates(exteriorRingCoords, ordinateList)
                : AddOrdinatesInReverse(exteriorRingCoords, ordinateList);

            int interiorRingCount = polygon.NumInteriorRings;
            for (int i = 0; i < interiorRingCount; i++)
            {
                elemInfoList.Add(pos);
                elemInfoList.Add((int)SdoEType.PolygonInterior);
                elemInfoList.Add(1);

                var interiorRingCoords = polygon.GetInteriorRingN(i).CoordinateSequence;
                pos += Algorithm.Orientation.IsCCW(interiorRingCoords)
                    ? AddOrdinatesInReverse(interiorRingCoords, ordinateList)
                    : AddOrdinates(interiorRingCoords, ordinateList);
            }

            return pos;
        }

        private int ProcessMultiPoint(MultiPoint multiPoint, List<double> elemInfoList, List<double> ordinateList, int pos)
        {
            int cnt = multiPoint.NumGeometries;

            // (airbreather 2019-01-29) for some reason, MultiPoint seems to be special: it's not
            // just ProcessPoint for each point, since that would append to elemInfoList multiple
            // times.  instead, elemInfoList gets incremented just once.  *shrugs*.
            elemInfoList.Add(pos);
            elemInfoList.Add((int)SdoEType.Coordinate);
            elemInfoList.Add(cnt);

            for (int i = 0; i < cnt; i++)
            {
                var point = (Point)multiPoint.GetGeometryN(i);
                pos += AddOrdinates(point.CoordinateSequence, ordinateList);
            }

            return pos;
        }

        private int ProcessMultiLineString(MultiLineString multiLineString, List<double> elemInfoList, List<double> ordinateList, int pos)
        {
            int cnt = multiLineString.NumGeometries;
            for (int i = 0; i < cnt; i++)
            {
                var line = (LineString)multiLineString.GetGeometryN(i);
                pos += ProcessLinear(line, elemInfoList, ordinateList, pos);
            }

            return pos;
        }

        private int ProcessMultiPolygon(MultiPolygon multiPolygon, List<double> elemInfoList, List<double> ordinateList, int pos)
        {
            int cnt = multiPolygon.NumGeometries;
            for (int i = 0; i < cnt; i++)
            {
                var poly = (Polygon)multiPolygon.GetGeometryN(i);
                pos = ProcessPolygon(poly, elemInfoList, ordinateList, pos);
            }

            return pos;
        }

        private int AddOrdinates(CoordinateSequence sequence, List<double> ords)
        {            
            int numOfPoints = sequence.Count;
            for (int i = 0; i < numOfPoints; i++)
            {
                ords.Add((double)sequence.GetX(i));
                ords.Add((double)sequence.GetY(i));
                if (dimension == 3)
                {
                    ords.Add((double)sequence.GetZ(i));
                }
            }

            return numOfPoints * dimension;
        }

        private int AddOrdinatesInReverse(CoordinateSequence sequence, List<double> ords)
        {            
            int numOfPoints = sequence.Count;

            for (int i = numOfPoints - 1; i >= 0; i--)
            {
                ords.Add((double)sequence.GetX(i));
                ords.Add((double)sequence.GetY(i));
                if (dimension == 3)
                {
                    ords.Add((double)sequence.GetZ(i));
                }
            }

            return numOfPoints * dimension;
        }

        private SdoGTemplate Template(Geometry geom)
        {
            switch (geom)
            {
                case null:
                    return SdoGTemplate.Unknown;

                case Point _:
                    return SdoGTemplate.Coordinate;

                case LineString _:
                    return SdoGTemplate.Line;

                case Polygon _:
                    return SdoGTemplate.Polygon;

                case MultiPoint _:
                    return SdoGTemplate.MultiPoint;

                case MultiLineString _:
                    return SdoGTemplate.MultiLine;

                case MultiPolygon _:
                    return SdoGTemplate.MultiPolygon;

                case GeometryCollection _:
                    return SdoGTemplate.Collection;

                default:
                    throw new ArgumentException("Cannot encode JTS "
                        + geom.GeometryType + " as SDO_GTEMPLATE "
                        + "(Limitied to Point, Line, Polygon, GeometryCollection, MultiPoint,"
                        + " MultiLineString and MultiPolygon)");
            }
        }
    }
}
