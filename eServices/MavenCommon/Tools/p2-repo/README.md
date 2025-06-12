This project merges defined p2 repositories into one internal usage repo for eServices.  

Run `mvn deploy` to deploy to an existing tomcat server
For instance:
- `mvn deploy`
- `mvn deploy -P test`
- `mvn deploy -Dtomcat_hostname=localhost`