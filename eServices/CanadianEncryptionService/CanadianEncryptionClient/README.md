When you want to update Client locally:

1/ Update or host new CanadianEncryptionService
	- Using CanadianEncryptionService/"mvn tomcat:run" or "mvn tomcat:redeploy"(Ref: http://tomcat.apache.org/maven-plugin-2.2/tomcat7-maven-plugin/plugin-info.html.
	- Or manual deploy the updated CanadianEncryptionService to tomcat server.

2/ Run CanadianEncryptionClient/"mvn clean jaxws:wsimport -Dtomcat_hostname={tomcat_hostname} -Dtomcat_port={tomcat_port} -Dtomcat_deployment_path={tomcat_deployment_path} -Dtomcat_protocol={tomcat_protocol}" in cmd to update client from localhost.

Note: You can create another profile in CanadianEncryption/properties-pom.xml to do this.
