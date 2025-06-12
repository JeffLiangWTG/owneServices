Run "mvn install:install-file deploy -P profile_name" to deploy.
Note: execute install:install-file to install the webservice assembly offline into local repo before the build lifecycle starts.

You also could modify the properties by using -Dproperty-name=PropertyValue
For instance: mvn install:install-file deploy -P production -Dtomcat_port=8888
or: mvn install:install-file deploy -Dtomcat_hostname=sydsp-wddb-1.sand.wtg.zone -Dtomcat_port=8888