## Usage

### Reading
```C#
// SdoGeometry from somewhere, e.g. OracleDataReader or OracleCommand.ExecuteScalar()
SdoGeometry oracleUdtGeom;

// Instantiate OracleGeometryReader
var ogr = new NetTopologySuite.IO.OracleGeometryReader(NetTopologySuite.NtsGeometryServices.Instance);

// Transform geometry
var ntsGeom = ogr.Read(oracleUdtGeom);
```

### Writing
```C#
// NTS geometry from somewhere
Geometry ntsGeom;

// Instantiate OracleGeometryWriter
var ogw = new NetTopologySuite.IO.OracleGeometryWriter();

// Transform geometry
var oracleUdtGeom = ogw.Write(ntsGeom);

// Create an OracleParameter for the geometry
var oracleParam = new OracleParameter() {
    // Name of parameter used in SQL command (:geom)
    ParameterName = "geom",                  
    DbType = DbType.Object,                  
    Direction = ParameterDirection.Input,
    // Name of the udt type as in Oracle database
    UdtTypeName = "MDSYS.SDO_GEOMETRY"       
};

// Assign the geometry value
oracleParam.Value = oracleUdtGeom;
```

## Integration tests
### Requirements
In order to perform the tests, a Docker container with an Oracle XE database has to be available.
One that worked for us is [oci-oracle-free](https://hub.docker.com/r/gvenzl/oracle-free):   
```
docker pull docker pull gvenzl/oracle-free:latest
```

Make sure you bind the correct port when running the image:
```
docker run -d -p 1521:1521 -e ORACLE_PASSWORD=secret gvenzl/oracle-free
```

You should be able to connect using the following values:

Property | Value
--- | ---
hostname | localhost
port | 1521
sid | free
username | system
password | oracle

Make sure you change the connectionstring in the test project's [App.config](test/NetTopologySuite.IO.Oracle.Test/App.config) to match with whatever test database you want to run the tests against. 
The default settings assume the above docker image is running on localhost.

### Performing the tests
- Open the solution 'NetTopologySuite.IO.Oracle.sln'
- Go to Test > Test Explorer 
- Run the tests (should be all green)

The integration test is fairly simple: it creates a table with a geometry column, writes a row, reads it back and compares the two 
versions for equality. After that it drops the table again.
You should be able to run this against any database, the table name created and dropped is defined at the tope of the test class  (="NTS_TEST_GEO_DATA").
