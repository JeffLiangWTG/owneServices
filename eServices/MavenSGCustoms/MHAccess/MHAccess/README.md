This project is the parent project which lets us build the all children projects and build the deployment package.

**Please run "mvn clean install" in "C:\eServices\MavenCommon\Common" first to install common-pom.xml if you haven't done this in the past.**

And then do a full build in current MHAccess folder: `mvn clean install`

Note: using clean will first do some cleaning, it's like Rebuild in VS. You can also do `mvn clean verify`, etc.

## How to run tomcat webservice locally
You have to install Tomcat on port 8080. Refer to "SetUp Tomcat Server" in `$\eServices\MavenCommon\README.md`

## How to deploy
cd `$\eServices\MavenSGCustoms\MHAccess\MHAccess\target\deploy` and follow its README.md

## HOW TO UPDATE A PROVIDED MHACCESS API FROM SGCUSTOM
1. Uninstall old version and install new MHAccess.exe.
2. Deploy the latest MHAccessInstallationFolder/snsjar/mhx.jar|mhxUtil.jar|soapws-client.jar to https://proget.wtg.zone/feeds/Maven
3. Check the version of MHAccessInstallationFolder/snsjar/FastInfoset.jar
4. Update version number of these jars in MHAccessBusiness
5. Update the latest files under MHAccessGatewayWebService/src/java/webapp/WEB-INF/MHAccess (includes snsjar folder)
6. Update MHAccess/deploy/profile/*/web.xml if ServerIP is changed.