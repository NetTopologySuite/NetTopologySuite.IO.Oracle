## Usage

### Reading
```C#
// SdoGeometry from somewhere, e.g. OracleDataReader or OracleCommand.ExecuteScalar()
SdoGeometry oracleGeom;

// Instantiate OracleGeometryReader
var ogr = new NetTopologySuite.IO.OracleGeometryReader(NetTopologySuite.NtsGeometryServices.Instance);

// Transform geometry
var ntsGeom = ogr.Read(oracleGeom);
```

### Writing
```C#
// NTS geometry from somewhere
Geometry ntsGeom;

// Instantiate OracleGeometryWriter
var ogw = new NetTopologySuite.IO.OracleGeometryWriter();

// Transform geometry
var oracleGeom = ogw.Write(ntsGeom);
```

## Integration tests
### Requirements
In order to perform the tests, a Docker container with an Oracle XE database has to be available.
One that worked for us is [oci-oracle-xe](https://hub.docker.com/r/gvenzl/oracle-xe):   
```
docker pull docker pull gvenzl/oracle-xe:latest
```

Make sure you bind the correct port when running the image:
```
docker run -d -p 1521:1521 -e ORACLE_PASSWORD=secret gvenzl/oracle-xe
```

You should be able to connect using the following values:
Property | Value
--- | ---
hostname | localhost
port | 1521
sid | xe
username | system
password | oracle

Make sure you change the connectionstring in the test project's App.config to match with whatever test database you want to run the tests against. 
The default settings assume the above docker image is running on localhost.

### Performing the tests
- Open the solution 'NetTopologySuite.IO.Oracle.sln'
- Go to Test > Test Explorer 
- Run the tests (should be all green)

The integration test is fairly simple: it creates a table with a geometry column, writes a row, reads it back and compares the two 
versions for equality. After that it drops the table again.
You should be able to run this against any database, the table name created and dropped is defined at the tope of the test class  (="NTS_TEST_GEO_DATA").
