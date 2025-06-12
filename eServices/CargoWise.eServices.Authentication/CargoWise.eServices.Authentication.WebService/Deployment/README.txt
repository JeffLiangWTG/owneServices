There are a few prerequisites that need to be fulfilled before deploying webservice. 
	1.	Make sure that Web Deploy (MSDeploy) has been installed on the target server.
	2.	Make sure that relevant features have been installed. To install the features:
		a.	Open Add Roles and Features Wizard
		b.	Go to Server Roles > Web Server (IIS) > Web Server > Application Development
		c.	Check the sub-objects under Application Development and install.
	3.	Make sure that appropriate SSL Certificate has been installed.
	4.	Give IIS Worker Process Accounts write permission to the webservice folder:
		a.	Open C:\interpub\wwwroot
		b.	Right-click on the webservice folder that you just created, and click on Properties
		c.	Go to Security tab and click on Edit.
		d.	Find IIS_IUSRS(servername\IIS_IUSRS).
		e.	check Write under permissions and save the change.