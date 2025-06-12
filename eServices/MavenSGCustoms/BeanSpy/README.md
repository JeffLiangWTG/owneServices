This project lets us to make the BeanSpy deployment zip package.

**Please run "mvn clean install" in "C:\eServices\MavenCommon\Common" first to install common-pom.xml if you haven't done this in the past.**

And then make package in current BeanSpy folder: `mvn clean verify package -X`

Note: using clean will first do some cleaning, it's like Rebuild in VS. You can also do `mvn clean`, etc.

## How to run tomcat webservice locally
You have to install Tomcat on port 8080. Refer to "SetUp Tomcat Server" in `$\eServices\MavenCommon\README.md`

## How to deploy
Click the deploy link on the http://crikey.wtg.zone/
Project=$(BINPATH)\BeanSpy\BeanSpy.zip;Command=mvn install:install-file deploy ...

For manually deploy
cd `$\eServices\MavenSGCustoms\BeanSpy\deploy` and follow its README.md

- deploy - `mvn install:install-file deploy -Dtomcat_hostname=localhost`
- remove - `mvn install:install-file deploy -Dremove=true -Dtomcat_hostname=localhost`

## After deployment verification
After deployment, you can go to Tomcat manager page to see the BeanSpy is deployed.
http://localhost:8080/manager/html/
or http://au2sp-shub-401.sand.wtg.zone:8080/manager/html/

And go to the BeanSpy Stats page to see the xml data, such as Thread, Memory, etc.
http://localhost:8080/BeanSpy/Stats
or http://au2sp-shub-401.sand.wtg.zone:8080/BeanSpy/Stats

(Please replace the above server address to which the BeanSpy deployed.)

## For reference
http://www.buchatech.com/2013/04/monitor-tomcat-running-as-windows-services-with-scom/