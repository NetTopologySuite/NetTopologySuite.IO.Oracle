using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.IO.Sdo;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Translates a NTS Geometry into an Oracle UDT.
    /// </summary>
    public class OracleGeometryWriter
    {
        private const int SridNull = -1;

        /// <summary>
        /// Property for spatial reference system
        /// </summary>
        public int SRID { get; set; } = SridNull;

        private int Dimension(IGeometry geom)
        {
            return double.IsNaN(geom.Coordinate.Z) ? 2 : 3;
        }

        private int GType(IGeometry geom)
        {
            return Dimension(geom) * 1000 + (int)Template(geom);
        }

        /// <summary>
        /// Converts an IGeometry to the corresponding Oracle UDT of type SdoGeometry
        /// it returns null, if conversion fails
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns>SdoGeometry</returns>
        public SdoGeometry Write(IGeometry geometry)
        {
            if (geometry?.IsEmpty != false)
            {
                return null;
            }

            switch (geometry)
            {
                case IPoint point:
                    return Write(point);

                case ILineString line:
                    return Write(line);

                case IPolygon polygon:
                    return Write(polygon);

                case IMultiPoint multiPoint:
                    return Write(multiPoint);

                case IMultiLineString multiLineString:
                    return Write(multiLineString);

                case IMultiPolygon multiPolygon:
                    return Write(multiPolygon);

                case IGeometryCollection collection:
                    return Write(collection);

                default:
                    throw new ArgumentException("Geometry not supported: " + geometry);
            }
        }

        private SdoGeometry Write(IPoint point)
        {
            var elemInfoList = new List<decimal>();
            var ordinateList = new List<decimal>();

            ProcessPoint(point, elemInfoList, ordinateList, 1);

            return new SdoGeometry()
            {
                SdoGtype = GType(point),
                Sdo_Srid = point.SRID,
                ElemArray = elemInfoList.ToArray(),
                OrdinatesArray = ordinateList.ToArray(),
            };
        }

        private SdoGeometry Write(ILineString line)
        {
            var elemInfoList = new List<decimal>();
            var ordinateList = new List<decimal>();

            ProcessLinear(line, elemInfoList, ordinateList, 1);

            return new SdoGeometry()
            {
                SdoGtype = GType(line),
                Sdo_Srid = line.SRID,
                ElemArray = elemInfoList.ToArray(),
                OrdinatesArray = ordinateList.ToArray(),
            };
        }

        private SdoGeometry Write(IPolygon polygon)
        {
            var elemInfoList = new List<decimal>();
            var ordinateList = new List<decimal>();

            ProcessPolygon(polygon, elemInfoList, ordinateList, 1);

            return new SdoGeometry
            {
                SdoGtype = GType(polygon),
                Sdo_Srid = polygon.SRID,
                ElemArray = elemInfoList.ToArray(),
                OrdinatesArray = ordinateList.ToArray(),
            };
        }

        private SdoGeometry Write(IMultiPoint multiPoint)
        {
            var elemInfoList = new List<decimal>();
            var ordinateList = new List<decimal>();

            ProcessMultiPoint(multiPoint, elemInfoList, ordinateList, 1);

            return new SdoGeometry
            {
                SdoGtype = GType(multiPoint),
                Sdo_Srid = multiPoint.SRID,
                ElemArray = elemInfoList.ToArray(),
                OrdinatesArray = ordinateList.ToArray(),
            };
        }

        private SdoGeometry Write(IMultiLineString multiLineString)
        {
            var elemInfoList = new List<decimal>();
            var ordinateList = new List<decimal>();

            ProcessMultiLineString(multiLineString, elemInfoList, ordinateList, 1);

            return new SdoGeometry
            {
                SdoGtype = GType(multiLineString),
                Sdo_Srid = multiLineString.SRID,
                ElemArray = elemInfoList.ToArray(),
                OrdinatesArray = ordinateList.ToArray(),
            };
        }

        private SdoGeometry Write(IMultiPolygon multiPolygon)
        {
            var elemInfoList = new List<decimal>();
            var ordinateList = new List<decimal>();

            ProcessMultiPolygon(multiPolygon, elemInfoList, ordinateList, 1);

            return new SdoGeometry
            {
                SdoGtype = GType(multiPolygon),
                Sdo_Srid = multiPolygon.SRID,
                ElemArray = elemInfoList.ToArray(),
                OrdinatesArray = ordinateList.ToArray(),
            };
        }

        private SdoGeometry Write(IGeometryCollection geometryCollection)
        {
            var elemInfoList = new List<decimal>();
            var ordinateList = new List<decimal>();
            int pos = 1;

            int cnt = geometryCollection.NumGeometries;
            for (int i = 0; i < cnt; i++)
            {
                var geom = geometryCollection.GetGeometryN(i);
                switch (geom.OgcGeometryType)
                {
                    case OgcGeometryType.Point:
                        pos = ProcessPoint((IPoint)geom, elemInfoList, ordinateList, pos);
                        break;

                    case OgcGeometryType.LineString:
                        pos = ProcessLinear((ILineString)geom, elemInfoList, ordinateList, pos);
                        break;

                    case OgcGeometryType.Polygon:
                        pos = ProcessPolygon((IPolygon)geom, elemInfoList, ordinateList, pos);
                        break;

                    case OgcGeometryType.MultiPoint:
                        pos = ProcessMultiPoint((IMultiPoint)geom, elemInfoList, ordinateList, pos);
                        break;

                    case OgcGeometryType.MultiLineString:
                        pos = ProcessMultiLineString((IMultiLineString)geom, elemInfoList, ordinateList, pos);
                        break;

                    case OgcGeometryType.MultiPolygon:
                        pos = ProcessMultiPolygon((IMultiPolygon)geom, elemInfoList, ordinateList, pos);
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

        private int ProcessPoint(IPoint point, List<decimal> elemInfoList, List<decimal> ordinateList, int pos)
        {
            elemInfoList.Add(pos);
            elemInfoList.Add((int)SdoEType.Coordinate);
            elemInfoList.Add(1);
            return pos + AddOrdinates(point.CoordinateSequence, ordinateList);
        }

        private int ProcessLinear(ILineString line, List<decimal> elemInfoList, List<decimal> ordinateList, int pos)
        {
            elemInfoList.Add(pos);
            elemInfoList.Add((int)SdoEType.Line);
            elemInfoList.Add(1);
            return pos + AddOrdinates(line.CoordinateSequence, ordinateList);
        }

        private int ProcessPolygon(IPolygon polygon, List<decimal> elemInfoList, List<decimal> ordinateList, int pos)
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

        private int ProcessMultiPoint(IMultiPoint multiPoint, List<decimal> elemInfoList, List<decimal> ordinateList, int pos)
        {
            int cnt = multiPoint.NumGeometries;

            // (airbreather 2019-01-29) for some reason, IMultiPoint seems to be special: it's not
            // just ProcessPoint for each point, since that would append to elemInfoList multiple
            // times.  instead, elemInfoList gets incremented just once.  *shrugs*.
            elemInfoList.Add(pos);
            elemInfoList.Add((int)SdoEType.Coordinate);
            elemInfoList.Add(cnt);

            for (int i = 0; i < cnt; i++)
            {
                var point = (IPoint)multiPoint.GetGeometryN(i);
                pos += AddOrdinates(point.CoordinateSequence, ordinateList);
            }

            return pos;
        }

        private int ProcessMultiLineString(IMultiLineString multiLineString, List<decimal> elemInfoList, List<decimal> ordinateList, int pos)
        {
            int cnt = multiLineString.NumGeometries;
            for (int i = 0; i < cnt; i++)
            {
                var line = (ILineString)multiLineString.GetGeometryN(i);
                pos += ProcessLinear(line, elemInfoList, ordinateList, pos);
            }

            return pos;
        }

        private int ProcessMultiPolygon(IMultiPolygon multiPolygon, List<decimal> elemInfoList, List<decimal> ordinateList, int pos)
        {
            int cnt = multiPolygon.NumGeometries;
            for (int i = 0; i < cnt; i++)
            {
                var poly = (IPolygon)multiPolygon.GetGeometryN(i);
                pos = ProcessPolygon(poly, elemInfoList, ordinateList, pos);
            }

            return pos;
        }

        private int AddOrdinates(ICoordinateSequence sequence, List<decimal> ords)
        {
            int dimension = sequence.Dimension;
            int numOfPoints = sequence.Count;
            for (int i = 0; i < numOfPoints; i++)
            {
                ords.Add((decimal)sequence.GetX(i));
                ords.Add((decimal)sequence.GetY(i));
                if (dimension == 3)
                {
                    ords.Add((decimal)sequence.GetOrdinate(i, Ordinate.Z));
                }
            }

            return numOfPoints * dimension;
        }

        private int AddOrdinatesInReverse(ICoordinateSequence sequence, List<decimal> ords)
        {
            int dimension = sequence.Dimension;
            int numOfPoints = sequence.Count;

            for (int i = numOfPoints - 1; i >= 0; i--)
            {
                ords.Add((decimal)sequence.GetX(i));
                ords.Add((decimal)sequence.GetY(i));
                if (dimension == 3)
                {
                    ords.Add((decimal)sequence.GetOrdinate(i, Ordinate.Z));
                }
            }

            return numOfPoints * dimension;
        }

        private SdoGTemplate Template(IGeometry geom)
        {
            switch (geom)
            {
                case null:
                    return SdoGTemplate.Unknown;

                case IPoint _:
                    return SdoGTemplate.Coordinate;

                case ILineString _:
                    return SdoGTemplate.Line;

                case IPolygon _:
                    return SdoGTemplate.Polygon;

                case IMultiPoint _:
                    return SdoGTemplate.MultiPoint;

                case IMultiLineString _:
                    return SdoGTemplate.MultiLine;

                case IMultiPolygon _:
                    return SdoGTemplate.MultiPolygon;

                case IGeometryCollection _:
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
