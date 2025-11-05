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

        /// <summary>
        /// Property for spatial reference system
        /// </summary>
        /// <remarks>Only used when set to a value other than <c>-1</c>. Otherwise the SRID property of the geometry to write is used.</remarks>
        public int SRID { get; set; } = SridNull;

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
            int gtype = GType(point, out int dimension);
            var elemInfoList = new List<double>(dimension);
            var ordinateList = new List<double>();

            ProcessPoint(point, dimension, elemInfoList, ordinateList, 1);

            return new SdoGeometry()
            {
                SdoGtype = gtype,
                Sdo_Srid = SRID != SridNull ? SRID : point.SRID,
                ElemArray = elemInfoList.ToArray(),
                OrdinatesArray = ordinateList.ToArray(),
            };
        }

        private SdoGeometry Write(LineString lineString)
        {
            int gtype = GType(lineString, out int dimension);
            var elemInfoList = new List<double>(dimension * lineString.NumPoints);
            var ordinateList = new List<double>();

            ProcessLinear(lineString, dimension, elemInfoList, ordinateList, 1);

            return new SdoGeometry()
            {
                SdoGtype = gtype,
                Sdo_Srid = SRID != SridNull ? SRID : lineString.SRID,
                ElemArray = elemInfoList.ToArray(),
                OrdinatesArray = ordinateList.ToArray(),
            };
        }

        private SdoGeometry Write(Polygon polygon)
        {
            int gtype = GType(polygon, out int dimension);
            var elemInfoList = new List<double>(dimension * polygon.NumPoints);
            var ordinateList = new List<double>();

            ProcessPolygon(polygon, dimension, elemInfoList, ordinateList, 1);

            return new SdoGeometry
            {
                SdoGtype = gtype,
                Sdo_Srid = SRID != SridNull ? SRID : polygon.SRID,
                ElemArray = elemInfoList.ToArray(),
                OrdinatesArray = ordinateList.ToArray(),
            };
        }

        private SdoGeometry Write(MultiPoint multiPoint)
        {
            int gtype = GType(multiPoint, out int dimension);
            var elemInfoList = new List<double>(dimension * multiPoint.NumPoints);
            var ordinateList = new List<double>();

            ProcessMultiPoint(multiPoint, dimension, elemInfoList, ordinateList, 1);

            return new SdoGeometry
            {
                SdoGtype = gtype,
                Sdo_Srid = SRID != SridNull ? SRID : multiPoint.SRID,
                ElemArray = elemInfoList.ToArray(),
                OrdinatesArray = ordinateList.ToArray(),
            };
        }

        private SdoGeometry Write(MultiLineString multiLineString)
        {
            int gtype = GType(multiLineString, out int dimension);
            var elemInfoList = new List<double>(dimension * multiLineString.NumPoints);
            var ordinateList = new List<double>();

            ProcessMultiLineString(multiLineString, dimension, elemInfoList, ordinateList, 1);

            return new SdoGeometry
            {
                SdoGtype = gtype,
                Sdo_Srid = SRID != SridNull ? SRID : multiLineString.SRID,
                ElemArray = elemInfoList.ToArray(),
                OrdinatesArray = ordinateList.ToArray(),
            };
        }

        private SdoGeometry Write(MultiPolygon multiPolygon)
        {
            int gtype = GType(multiPolygon, out int dimension);
            var elemInfoList = new List<double>(dimension * multiPolygon.NumPoints);
            var ordinateList = new List<double>();
            
            ProcessMultiPolygon(multiPolygon, dimension, elemInfoList, ordinateList, 1);

            return new SdoGeometry
            {
                SdoGtype = gtype,
                Sdo_Srid = SRID != SridNull ? SRID : multiPolygon.SRID,
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
            int gtype = GType(geometryCollection, out int dimension);

            for (int i = 0; i < cnt; i++)
            {
                var geom = geometryCollection.GetGeometryN(i);
                switch (geom.OgcGeometryType)
                {
                    case OgcGeometryType.Point:
                        pos = ProcessPoint((Point)geom, dimension, elemInfoList, ordinateList, pos);
                        break;

                    case OgcGeometryType.LineString:
                        pos = ProcessLinear((LineString)geom, dimension, elemInfoList, ordinateList, pos);
                        break;

                    case OgcGeometryType.Polygon:
                        pos = ProcessPolygon((Polygon)geom, dimension, elemInfoList, ordinateList, pos);
                        break;

                    case OgcGeometryType.MultiPoint:
                        pos = ProcessMultiPoint((MultiPoint)geom, dimension, elemInfoList, ordinateList, pos);
                        break;

                    case OgcGeometryType.MultiLineString:
                        pos = ProcessMultiLineString((MultiLineString)geom, dimension, elemInfoList, ordinateList, pos);
                        break;

                    case OgcGeometryType.MultiPolygon:
                        pos = ProcessMultiPolygon((MultiPolygon)geom, dimension, elemInfoList, ordinateList, pos);
                        break;

                    default:
                        throw new ArgumentException("Geometry not supported in GeometryCollection: " + geom);
                }
            }

            return new SdoGeometry
            {
                SdoGtype = gtype,
                Sdo_Srid = SRID != SridNull ? SRID : geometryCollection.SRID,
                ElemArray = elemInfoList.ToArray(),
                OrdinatesArray = ordinateList.ToArray(),
            };
        }

        private static int ProcessPoint(Point point, int dimension, List<double> elemInfoList, List<double> ordinateList, int pos)
        {
            elemInfoList.Add(pos);
            elemInfoList.Add((int)SdoEType.Coordinate);
            elemInfoList.Add(1);
            return pos + AddOrdinates(point.CoordinateSequence, dimension, ordinateList);
        }

        private static int ProcessLinear(LineString line, int dimension, List<double> elemInfoList, List<double> ordinateList, int pos)
        {
            elemInfoList.Add(pos);
            elemInfoList.Add((int)SdoEType.Line);
            elemInfoList.Add(1);
            return pos + AddOrdinates(line.CoordinateSequence, dimension, ordinateList);
        }

        private static int ProcessPolygon(Polygon polygon, int dimension, List<double> elemInfoList, List<double> ordinateList, int pos)
        {
            elemInfoList.Add(pos);
            elemInfoList.Add((int)SdoEType.PolygonExterior);
            elemInfoList.Add(1);

            var exteriorRingCoords = polygon.ExteriorRing.CoordinateSequence;
            pos += AddOrdinates(exteriorRingCoords, dimension, ordinateList, LinearRingOrientation.CounterClockwise);
                    /* Algorithm.Orientation.IsCCW(exteriorRingCoords)
                ? AddOrdinates(exteriorRingCoords, dimension, ordinateList)
                : AddOrdinatesInReverse(exteriorRingCoords, dimension, ordinateList);*/

            int interiorRingCount = polygon.NumInteriorRings;
            for (int i = 0; i < interiorRingCount; i++)
            {
                elemInfoList.Add(pos);
                elemInfoList.Add((int)SdoEType.PolygonInterior);
                elemInfoList.Add(1);

                var interiorRingCoords = polygon.GetInteriorRingN(i).CoordinateSequence;
                pos += AddOrdinates(interiorRingCoords, dimension, ordinateList, LinearRingOrientation.Clockwise);
                    /*Algorithm.Orientation.IsCCW(interiorRingCoords)
                    ? AddOrdinatesInReverse(interiorRingCoords, dimension, ordinateList)
                    : AddOrdinates(interiorRingCoords, dimension, ordinateList);*/
            }

            return pos;
        }

        private static int ProcessMultiPoint(MultiPoint multiPoint, int dimension, List<double> elemInfoList, List<double> ordinateList, int pos)
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
                pos += AddOrdinates(point.CoordinateSequence, dimension, ordinateList);
            }

            return pos;
        }

        private static int ProcessMultiLineString(MultiLineString multiLineString, int dimension, List<double> elemInfoList, List<double> ordinateList, int pos)
        {
            int cnt = multiLineString.NumGeometries;
            for (int i = 0; i < cnt; i++)
            {
                var line = (LineString)multiLineString.GetGeometryN(i);
                pos = ProcessLinear(line, dimension, elemInfoList, ordinateList, pos);
            }

            return pos;
        }

        private static int ProcessMultiPolygon(MultiPolygon multiPolygon, int dimension, List<double> elemInfoList, List<double> ordinateList, int pos)
        {
            int cnt = multiPolygon.NumGeometries;
            for (int i = 0; i < cnt; i++)
            {
                var poly = (Polygon)multiPolygon.GetGeometryN(i);
                pos = ProcessPolygon(poly, dimension, elemInfoList, ordinateList, pos);
            }

            return pos;
        }

        //private enum RingOrientation 

        private static int AddOrdinates(CoordinateSequence sequence, int dimension, List<double> ords,
            LinearRingOrientation orientation = LinearRingOrientation.DontCare)
        {
            switch (orientation)
            {
                case LinearRingOrientation.CounterClockwise:
                    if (!Algorithm.Orientation.IsCCW(sequence)) sequence = sequence.Reversed();
                    break;
                case LinearRingOrientation.Clockwise:
                    if (Algorithm.Orientation.IsCCW(sequence)) sequence = sequence.Reversed();
                    break;
            }

            for (int i = 0; i < sequence.Count; i++)
            {
                for(int j = 0; j < sequence.Dimension; j++)
                    ords.Add(sequence.GetOrdinate(i, j));
            }

            return sequence.Count * dimension;
        }

        private static int AddOrdinatesInReverse(CoordinateSequence sequence, int dimension, List<double> ords)
        {            
            int numOfPoints = sequence.Count;

            for (int i = numOfPoints - 1; i >= 0; i--)
            {
                for (int j = 0; j < sequence.Dimension; j++)
                    ords.Add(sequence.GetOrdinate(i, j));
            }

            return numOfPoints * dimension;
        }

        private static int GType(Geometry geom, out int dimension)
        {
            DimensionAndMeasure(geom, out dimension, out int measure);
            return dimension * 1000 + measure * 100 + (int)Template(geom);
        }

        private static void DimensionAndMeasure(Geometry geom, out int dimension, out int measure)
        {
            var sdd = new DimensionAndMeasureDeterminator();
            geom.Apply(sdd);
            dimension = sdd.Dimension;
            measure = sdd.Measure;
        }

        private static SdoGTemplate Template(Geometry geom)
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

        private class DimensionAndMeasureDeterminator : IEntireCoordinateSequenceFilter
        {
            public void Filter(CoordinateSequence sequence)
            {
                bool hasMeasure = sequence.Measures > 0;
                Dimension = Math.Min(sequence.Dimension, 4);
                if (hasMeasure)
                    Measure = Dimension;

                Done = true;
            }

            public int Dimension { get; private set; } = 2;

            public int Measure { get; private set; } = 0;

            public bool Done { get; private set; }
            public bool GeometryChanged => false;
        }
    }
}
