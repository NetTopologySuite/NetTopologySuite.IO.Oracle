README

In order to perform the tests, a Docker container with an Oracle XE database has to be available.
One that worked for us is:
```docker pull gvenzl/oracle-xe:latest```

Make sure you bind the correct port when running the image:
docker run -d -p 1521:1521 -e ORACLE_PASSWORD=secret gvenzl/oracle-xe

You should be able to connect using:
hostname: localhost
port: 49161
sid: xe
username: system
password: secret

Make sure you change the connectionstring in the test project's App.config to match with whatever test database you want to run the tests against. 
The default settings in the App.config assume the above docker image is run using that exact command.

To run the tests:
- Open the solution 'NetTopologySuite.IO.Oracle.sln'
- Go to test > test explorer 
- Run the tests (should be all green)

The integration test is fairly simple: it creates a table with a geometry column, writes a row, reads it back and compares the two 
versions for equality. After that it drops the table again.
You should be able to run this against any database, the table name created and dropped is defined at the tope of the test class  (="NTS_TEST_GEO_DATA").
