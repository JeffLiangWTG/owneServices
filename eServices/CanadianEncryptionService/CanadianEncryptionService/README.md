Run "mvn tomcat:run" in cmd to run this webapp as standalone appplication.
Run "mvn tomcat:redeploy" in cmd to deploy this webapp to defined tomcat server(in pom.xml).
Run profile by "mvn COMMAND -Pprofile_name"

Reference: http://tomcat.apache.org/maven-plugin-2.2/tomcat7-maven-plugin/plugin-info.html

your settings.xml under C:\Users\your.hostname\.m2\ must have this proxy setup:

<settings xmlns="http://maven.apache.org/SETTINGS/1.0.0" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://maven.apache.org/SETTINGS/1.0.0                           https://maven.apache.org/xsd/settings-1.0.0.xsd">
	<proxies>
		<proxy>
			<active>true</active>
			<protocol>http</protocol>
			<host>sydco-spxy-1.wtg.zone</host>
			<port>8080</port>
			<nonProxyHosts>localhost|*.wtg.zone</nonProxyHosts>
		</proxy>
	</proxies>
</settings>

Example with custom server: mvn tomcat:redeploy -Dtomcat_hostname=sydsp-wddb-1.sand.wtg.zone -Dtomcat_port=8888