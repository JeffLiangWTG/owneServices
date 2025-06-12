# How to Setup eAdaptorSampleWebService WCF Service

We are using the same C# web service for the Java client.

1. Run IIS Manager

2. Create a Self-Signed Certificate. See step under "Obtain a Certificate", "Create Self Signed Certificate" from here: http://learn.iis.net/page.aspx/144/how-to-set-up-ssl-on-iis-7/

3. Right click on "Default Web Site" site and click "Add Application". Alias: eAdaptorService, Physical Path: Path to eAdaptorSampleWebService project

4. Create an SSL Binding for the new site as shown here: http://learn.iis.net/page.aspx/144/how-to-set-up-ssl-on-iis-7/

5. Navigate to new eAdaptorService application under "Default Web Site", select it. Switch to Content View.

6. In the Web.config file, change serviceCertificate "findValue" attribute (<serviceCertificate findValue="my-iis-server.name") to your machine / certificate name.

7. Right click on eAdaptorSampleWebService.svc and select "Browse".

8. Copy the URL from the browser. Replace http with https. Make sure it works in your browser with no errors.

9. Test the https address using the eAdaptorSampleWebClient Application as shown below.


# How to use the eAdaptorSampleWebClient Java Application

1. Ensure you have [JDK 8](https://www.oracle.com/java/technologies/javase/javase-jdk8-downloads.html) and [Eclipse](https://www.eclipse.org/downloads/) installed.

2. Open the project folder in Eclipse: File => Import => General => Existing Projects into Workspace => select your project folder e.g. C:\SOAP.JAVA\eAdaptorSampleWebClient

3. This project targets Java 8. In Eclipse, right click the project => Properties => Java Compiler => set Compiler compliance level to 1.8. More info here: https://waynebeaton.wordpress.com/2017/03/10/run-eclipse-ide-on-one-version-of-java-but-target-another/

4. Click Run (Ctrl + F5) to start the application. If you need to modify this app, you will need to install the `WindowsBuilder` and `WindowsBuilder XWT Support` Eclipse plugins.

5. Service Address can be either the URL from your eAdaptorSampleWebService setup above or the URL for your CW1 eAdaptor Web Service, e.g. https://my-iis-server.name/eAdaptorService/eAdaptorSampleWebService.svc, click Ping to confirm the web service is running.

6. Select a Message File from the SampleXML folder or any XML that conforms to the standard.

7. Enter Recipient Id, a 9-character code that matches your taget system. If target is eAdaptorSampleWebService, it can be anything. For CW1 systems, see Universal and Native XML Reference Guide for how to obtain it.

8. Enter Sender Id and Password. If target is eAdaptorSampleWebService, can be anything. For CW1 systems, see Universal and Native XML Reference Guide for how to obtain it.

9. Press Send button.

10. Check the folder you configured in eAdaptorSampleWebService as the "destinationFolder" (default is C:\eAdaptorSampleService) for delivered messages. File name format example: Message_956617b8-15fd-4637-b236-c0a507d77cb3.xml.

