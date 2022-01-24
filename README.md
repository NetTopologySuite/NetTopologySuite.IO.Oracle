README
In order to perform the tests, a Docker container with an Oracle image has to be activated.
To get an overview on how to create a Docker image with Oracle and activate its container, 
go to: https://dev.azure.com/merkatornv/Marlin/_wiki/wikis/Marlin.wiki/203/Creating-a-Docker-image-with-Oracle-then-running-the-container


An Oracle image has been built and can be accessed on Docker Hub at: https://hub.docker.com/r/arthurchome/oracle_docker_image

Once you got an Oracle image, follow these steps:

STEP 1: run the container with the image
-type '$ docker images' to see which images are currently in your docker private repository
-You should have one called 'oracle/database'
-type following command to run the container: 
 '$ docker run --name oracle -d -p 1521:1521 -p 5500:5500 -e ORACLE_PWD=mysecurepassword -e ORACLE_SID=ORALSID -e SERSID -e ORACLE_PDB=ORALPDB oracle/database:19.3.0-ee'
-See whether it's running with '$ docker ps'

Step 2: test the connection with Oracle SQL Developer
-Install Oracel SQL Developer
-Add new connection and check Oracle Docker:
 * Username: SYS
 * Password: mysecurepassword
 * Hostname: localhost
 * Port: 1521
 * Service name: ORALSID

Step 3: perform the tests
-Open the solution 'NetTopologySuite.IO.Oracle.sln'
-Go to test > test explorer 
-Run the tests (should be all green)
