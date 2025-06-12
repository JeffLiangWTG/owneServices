If you want to remove the existing application, run

`mvn install:install-file deploy -Dremove=true [-P profile_name]`

(install-file to make sure maven dependency problem does not fail the build). For instance:
- `mvn install:install-file deploy -Dremove=true`
- `mvn install:install-file deploy -Dremove=true -P test`
- `mvn install:install-file deploy -Dremove=true -P production1`
- `mvn install:install-file deploy -Dremove=true -P production2`
- `mvn install:install-file deploy -Dremove=true -Dtomcat_hostname=localhost`

Run `mvn install:install-file deploy [-P profile_name]` to deploy on your local.

Note: execute install:install-file to install the webservice assembly offline into local repo before the build lifecycle starts.

Also Note: You don't have to specify a profile when deploying locally i.e. `mvn install:install-file deploy -Dremove=true -Dtomcat_hostname=localhost`.

The tomcat_password in profiles in pom.xml are encrypted using [Dat.Integration.Encrypt](http://tfs.wtg.zone:8080/tfs/CargoWise/eServices/_wiki/wikis/eServices.wiki?wikiVersion=GBwikiMaster&pagePath=%2FProducts%2FeHub%2FDAT%20Deployments&pageId=96)

If your password is different with the default password in the pom.xml, you can specify it as `-Dtomcat_password=decryptedPassword`.

You also could modify the properties by using -Dproperty-name=PropertyValue. For instance:
- `mvn install:install-file clean deploy`
- `mvn install:install-file clean deploy -P test`
- `mvn install:install-file clean deploy -P production1`
- `mvn install:install-file clean deploy -P production2`
- `mvn install:install-file clean deploy -Dtomcat_hostname=localhost`