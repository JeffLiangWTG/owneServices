using System.Linq;
using CargoWise.eHub.BizTalkAdapters.Common;
using NUnit.Framework;

namespace CargoWise.eHub.BizTalkAdapters.IntegrationTests.FtpTransferers
{
	/// <summary>
	/// The following tests will let you debugging FtpTransferer functions. Click the highlighted "Output" text in Test Explorer's result panel for the logging details.
	/// You will need to setup your local FTP server if you want to test again your localhost url or change DefaultReceiveConfig/Config/Server to an existing FTP server I.E. eHub-test.db.sand.wtg.zone
	/// Setup FTP in local instruction:
	///		Window Server: Server Manager > Manage > Add Roles and Features > Keep hitting Next until it hits Server Roles section. In right pane, tick FTP Service and FTP Extensibility in Web Server (IIS) > FTP Server.
	///		Window 10: Control Panel > Programs > Turn Windows Features on or off > FTP Server > tick FTP Service and FTP Extensibility.
	///		Next to Install.
	///		In IIS, right click on server > Add FTP Site
	///		In Add FTP popup
	///			Site Information:        Sepcify FTP site name and Physical path > Next
	///			Binding and SSL Setting: Choose option no SSL > Next
	///			Authentication:          Basic Authentication
	///			Authorization:           Choose Specified users and input DevAdmin user
	///			Permission:				 Read and Write > Finish
	///		You can try to connect to your local FTP Server using \\sydco-scfs-1.wtg.zone\globaldata\SoftwareStore\FileZilla(clean) FileZilla_3.35.2_win64-setup.exe: Host=localhost, username=DevAdmin, password=3hubRock$, port=21.
	/// </summary>
	[TestFixture]
	[Ignore("Setup DefaultReceiveConfig pointing an existing FTP Server I.E. eHub-test.db.sand.wtg.zone or setup your local FTP Server then commenting this line.")]
	class TestFtpTransferer : FtpTestBase
	{
		// Getting your XML Config from C:\eServices\Operation\Bindings\ReceivePorts or export from your local binding. Remember to retype the password.
		const string DefaultReceiveConfig = @"<Config xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
			<uri>ftpex://localhost:21/</uri>
			<Server>localhost</Server>
			<Port>21</Port>
			<User>DevAdmin</User>
			<Password>3hubRock$</Password>
			<Timeout>90000</Timeout>
			<Mode>Passive</Mode>
			<MultipleLocations />
			<Folder></Folder>
			<FileMask>*.*</FileMask>
			<ServerSideFiltering>true</ServerSideFiltering>
			<FlagFile />
			<UseNLST>false</UseNLST>
			<SortOrder>None</SortOrder>
			<EmptyFileOption>Ignore</EmptyFileOption>
			<RenameBeforeDownload />
			<MoveBeforeDownload />
			<RenameAfterDownload />
			<MoveAfterDownload />
			<MoveWorkingDirectory>false</MoveWorkingDirectory>
			<PollingInterval>1</PollingInterval>
			<PollingUnit>Minutes</PollingUnit>
			<Logging>true</Logging>
			<LogDir>C:\Logs\BizTalk\Interfaces\{prefix,3}</LogDir>
			<LogMaxSize>2</LogMaxSize>
			<LogMaxCount>10</LogMaxCount>
			<LogLevel>Debug</LogLevel>
			<FtpsMode>None</FtpsMode>
			<ValidateServerCert>true</ValidateServerCert>
			<ThumbprintClientCert />
			<DataEncryption>true</DataEncryption>
		</Config>";

		[Test]
		public void TestFtpTransferer_Receive_Listing()
		{
			using(var ftpTransferer = new FtpTransferrerFactory().CreateTransferrer())
			{
				var receiveProperties = InitReceiveConfiguration(ftpTransferer, DefaultReceiveConfig);
				ftpTransferer.Open();
				var files = ftpTransferer.ListFiles(receiveProperties.SingleLocation, receiveProperties, this.linkedCancelTokenSource);
				Assert.AreEqual(0, files.Count());
			}
		}
	}
}
