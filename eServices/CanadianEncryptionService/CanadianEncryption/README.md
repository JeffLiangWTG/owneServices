This project is the parent project which lets us build the all children projects and build the deployment package.

**Please run "mvn clean install" in "C:\eServices\MavenCommon" first to install common-pom.xml** 

Run the following maven command and fix the errors relate about tomcat server and maven properties on properties-pom.xml

mvn clean install

If you want to execute a demo, you could run:

mvn clean install -Dtomcat_hostname=sydsp-wddb-1.sand.wtg.zone -Dtomcat_port=8888

If you want to run code quality check, you have to pass the intergration tests first:

mvn clean verify
mvn clean verify -Dtomcat_hostname=sydsp-wddb-1.sand.wtg.zone -Dtomcat_port=8888

Validation tools:

** Note: you could download those following plugins via: http://sydsp-wddb-1.sand.wtg.zone:8888/eServiceP2Repo/ **
	Or build its local in $/eServices/Java/p2-repo

Eclipse checkstyle:
	- http://eclipse-cs.sourceforge.net/#!/
	- http://eclipse-cs.sourceforge.net/#!/custom-config

Eclipse findbugs:
	- https://github.com/findbugsproject/findbugs/releases: git download
	- http://www.vogella.com/tutorials/Findbugs/article.html: tutorial
	- Run "mvn findbugs:gui" to view the target/findbugsXml.xml report after execute verify phase.
	
Eclipse PMD:
	- https://sourceforge.net/projects/pmd/files/pmd-eclipse/update-site-latest/plugins/: download the latest
	- http://pmd.sourceforge.net/snapshot/pmd-java/rules/index.html: Rule set wiki
	- https://github.com/pmd/pmd/blob/master/pmd-java/src/main/resources/rulesets/java/: Rule set xml
	
Tech hack:
	- You could download a jar file by converting it into zip: http://archive.online-convert.com/convert-to-zip