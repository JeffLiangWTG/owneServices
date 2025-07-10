using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWiseOne.WebInfrastructure;
using CargoWiseOne.WebInfrastructure.ErrorReporting;
using CargoWiseOne.WebInfrastructure.TestFramework;
using Enterprise.Registry.Business.eServices;
using Enterprise.Upgrades;
using Microsoft.Web.Administration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	[UseSnapshotProtection]
	class ServiceHostUpgradeTest : TestCase
	{
		[DeveloperOnlyTest]
		public void TestSiteUpgraded_AfterNextTimerCallback_OnCurrentVersionChange()
		{
			AssertEquals(siteManager.GetWebPage(testSite, "ServerPath.aspx")?.TrimEnd('\\'), Path.Combine(sitePath, TestApplicationName));
			AssertEquals(siteManager.GetWebPageWithBasicAuthorization(testSite, "eAdaptor", eAdaptorUserName, eAdaptorPassword), "<html><body><h1>Welcome to the eAdaptor HTTP+XML Service</h1></body></html>");

			// Arrange
			UploadPackage(versionNumber: 2, status: "CUR");
			var connection = ((IDbConnectionInternals)Db.Connection).ADOConnection;
			var currentVersion = new SqlUpgradeManager(new UpgradeSqlContext(connection, connection.DataSource, connection.Database)).QueryCurrentVersion();
			Assert("Just uploaded version 2 as current.", currentVersion.Version.Equals(new Version(2, 0, 0, 0)));

			// Act
			var timeOutCancellation = new CancellationTokenSource(UpgradeTimerInterval);
			var upgradedSiteServerPath = string.Empty;
			while (!timeOutCancellation.IsCancellationRequested)
			{
				var serverPath = GetServerPath(testSite.Name);
				if (!string.IsNullOrEmpty(serverPath) && !string.Equals(serverPath, initialServerPath, StringComparison.OrdinalIgnoreCase))
				{
					upgradedSiteServerPath = serverPath;
					break;
				}

				Thread.Sleep(100);
			}

			// Assert
			Assert(!string.IsNullOrEmpty(upgradedSiteServerPath));
			Assert("Upgraded path must contain the new version number", upgradedSiteServerPath.Contains("2.0.0.0"));
			AssertNotEquals(initialServerPath, upgradedSiteServerPath);
		}

		[DeveloperOnlyTest]
		public void TestSiteUpgraded_OnDatabaseUpgradedException()
		{
			AssertEquals(siteManager.GetWebPage(testSite, "ServerPath.aspx")?.TrimEnd('\\'), Path.Combine(sitePath, TestApplicationName));
			AssertEquals(siteManager.GetWebPageWithBasicAuthorization(testSite, "eAdaptor", eAdaptorUserName, eAdaptorPassword), "<html><body><h1>Welcome to the eAdaptor HTTP+XML Service</h1></body></html>");

			// Arrange
			UploadPackage(versionNumber: 2, status: "CUR");
			BumpUpSchemaVersion();

			// Act
			var response = PostEAdaptorWithUniversalShipmentRequest();
			Assert("Expecting UniversalResponse message", !response.Contains("UniversalResponse"));
			Thread.Sleep(TimeSpan.FromSeconds(10));

			// Assert
			var upgradedSiteServerPath = GetServerPath(testSite.Name);
			Assert(!string.IsNullOrEmpty(upgradedSiteServerPath));
			Assert("Upgraded path must contain the new version number", upgradedSiteServerPath.Contains("2.0.0.0"));
			AssertNotEquals(initialServerPath, upgradedSiteServerPath);
		}

		#region Implementations

		string PostEAdaptorWithUniversalShipmentRequest()
		{
			return siteManager.GetWebPageWithBasicAuthorization(testSite, "eAdaptor", eAdaptorUserName, eAdaptorPassword, (httpWebRequest) =>
			{
				httpWebRequest.Method = "POST";
				httpWebRequest.ContentType = "application/xml";
				const string testXml = @"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
<ShipmentRequest>
<DataContext>
<DataTargetCollection>
<DataTarget>
<Type>CustomDeclaration</Type>
<Key>B000010000</Key>
</DataTarget>
</DataTargetCollection>
<Company>
<Code>EDI</Code>
</Company>
<EnterpriseID>EDI</EnterpriseID>
<ServerID>DAT</ServerID>
</DataContext>
</ShipmentRequest>
</UniversalShipmentRequest>";

				using (var requestStream = httpWebRequest.GetRequestStream())
				{
					var requestData = Encoding.UTF8.GetBytes(testXml);
					requestStream.Write(requestData, 0, requestData.Length);
				}
			});
		}

		string GetServerPath(string siteName)
		{
			using (var newSeverManager = new ServerManager())
			{
				return newSeverManager.Sites[siteName] == null
					? string.Empty
					: WebSiteTestManager.GetPhysicalPath(newSeverManager.Sites[siteName]);
			}
		}

		void BumpUpSchemaVersion()
		{
			const string sql = @"
UPDATE dbo.StmData SET
	SD_BinaryValue = CONVERT(varbinary(max), CONVERT(nvarchar(max), (SELECT CONVERT(int, CONVERT(nvarchar(max), SD_BinaryValue))) + 1))
WHERE 1 = 1
	AND SD_Name = 'DATABASE_SCHEMA_VERSION'
	AND SD_Owner is NULL
	AND SD_DepartmentGuid is NULL
";
			Db.Connection.ExecuteNonQuery(sql);
		}

		protected override void TearDown()
		{
			try
			{
				siteManager.Dispose();
				base.TearDown();
			}
			finally
			{
				using (var newSeverManager = new ServerManager())
				{
					while (newSeverManager.Sites.Any()
					&& newSeverManager.Sites.Select(x => x.Name).Intersect(siteNames).Any())
					{
						Thread.Sleep(100);
					}
				}

				var retry = 0;
				while (localPaths.Any(Directory.Exists) && retry++ < 3)
				{
					try
					{
						Thread.Sleep(100);
						localPaths.ToList().ForEach(x =>
						{
							if (Directory.Exists(x))
							{
								Directory.Delete(x, true);
							}
						});
					}
					catch
					{
						// ignored because some site been notified to clean up old version
					}
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			siteManager = new WebSiteTestManager(HtmlFail);

			EmptyUpgradePackages();
			SetupNewTestWebSite(UploadPackage());
			ConfigEAdaptorInboundAuthentications();

			if (string.IsNullOrEmpty(initialServerPath))
			{
				Fail($"Test site: {testSite.Name} appears not running.");
			}
		}

		void EmptyUpgradePackages()
		{
			Db.Connection.ExecuteNonQuery("DELETE dbo.StmUpgrade;");
		}

		UpgradeInfoExtended UploadPackage(int versionNumber = 1, string status = "APL")
		{
			var packageMaker = new PackageMaker(((IDbConnectionInternals)Db.Connection).ADOConnection);
			var zipFileName = PackageMaker.EnterpriseWebDeployZipName;

			packageMaker.AddWebFile(zipFileName, TestApplicationName, "Global.asax", GlobalAsax);
			packageMaker.AddWebFile(zipFileName, TestApplicationName, "Web.config", GetWebConfig());
			packageMaker.AddWebFile(zipFileName, TestApplicationName, "ServerPath.aspx", ServerPath);

			var version = new Version(versionNumber, 0, 0, 0);
			return packageMaker.UploadPackage(version, status);
		}

		void SetupNewTestWebSite(UpgradeInfoExtended packageInfo)
		{
			var testConfiguration = new InstallationConfigurationForTest(Guid.NewGuid());
			using var sqlContext = new WebUpgradeSqlContext(Db.ServerName, Db.DatabaseName, () => ((IDbConnectionInternals)Db.Connection).ADOConnection);
			using var upgradeManager = new WebUpgradeManagerForTest(testConfiguration, new Version(1, 2, 3, 4), sqlContext, new Mock<IErrorReporter>().Object, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
			var localPath = upgradeManager.InstallWebFilesForTest(packageInfo);
			var sharedBin = Path.Combine(localPath, "shared-bin");
			CopyLocalBuildAssemblies(sharedBin);

			sitePath = localPath;
			localPaths.Add(Directory.GetParent(sitePath).FullName);
			localPaths.Add(sitePath);
			localPaths.Add(sharedBin);

			testSite = siteManager.AddWebSite((string name) => Path.Combine(localPath, TestApplicationName));
			siteNames.Add(testSite.Name);

			siteManager.AppPool.ManagedPipelineMode = ManagedPipelineMode.Classic;
			siteManager.AppPool.ProcessModel.IdleTimeout = TimeSpan.FromHours(48);

			siteManager.ServerManager.CommitChanges();
			WebDbConfiguration.SaveConfiguration(
				new WebDbConfigurationInfo()
				{
					ApplicationPath = WebAppPath.For(testSite),
					ServerName = Db.ServerName,
					DatabaseName = Db.DatabaseName
				});

			initialServerPath = GetServerPath(testSite.Name);
			using (var timeoutCancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
			{
				while (string.IsNullOrEmpty(initialServerPath) && !timeoutCancellation.IsCancellationRequested)
				{
					Thread.Sleep(100);
					initialServerPath = GetServerPath(testSite.Name);

					if (string.IsNullOrEmpty(initialServerPath))
					{
						break;
					}
				}
			}
		}

		void ConfigEAdaptorInboundAuthentications()
		{
			eAdaptorRegistry.Instance.eAdaptorInboundAuthentications.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, $"{eAdaptorUserName}|{eAdaptorPassword}");
		}

		void CopyLocalBuildAssemblies(string targetBinDirectory)
		{
			var assemblies = new HashSet<string>(Directory.GetFiles(ExecutableDirectory, "*.dll", SearchOption.TopDirectoryOnly)
					.Select(filePath => Path.GetFileName(filePath))
					.Where(fileName =>
						MustIncludeEndsWithNames.Any(x => fileName.EndsWith(x))
						|| (!ExcludedStartsWithNames.Any(x => fileName.StartsWith(x))
							&& !ExcludedEndsWithNames.Any(x => fileName.EndsWith(x))
							&& !Regex.Match(fileName, ExcludedFileNamesPattern).Success))
				).ToList();

			assemblies.ForEach(assemblyFileName =>
			{
				if (!File.Exists(Path.Combine(targetBinDirectory, assemblyFileName)))
				{
					File.Copy(
						Path.Combine(ExecutableDirectory, assemblyFileName),
						Path.Combine(targetBinDirectory, assemblyFileName));
				}
			});
		}

		static string ExcludedFileNamesPattern => @"^Enterprise\.Customs\.[\w]{2}\..*";

		static List<string> ExcludedStartsWithNames => new List<string>
		{
			"Enterprise.AuditDataServices.",
			"Enterprise.ArchiveManager.",
			"Enterprise.BarcodeParsing.",
			"Enterprise.BehaviourManagement.",
			"Enterprise.Billing.StlCollector",
			"Enterprise.BufferManagement.GUI",
			"Enterprise.BufferManagement.NetworkVisualisation.",
			"Enterprise.Edifact",
			"Enterprise.Customs._CustomsTemplate_",
			"Enterprise.Customs._EUCustomsTemplate_",
			"Enterprise.Customs.US.ACEManifest.",
			"Enterprise.Customs.ASYCUDA.",
			"Enterprise.Customs.AsycudaCustoms.",
			"Enterprise.Customs.ASYCUDAManifest.",
			"Enterprise.Customs.BLNSCustoms.Business",
			"Enterprise.Customs.CustomsWare.",
			"Enterprise.CodeAnalysis.",
			"Enterprise.CodeSniffer.",
			"Enterprise.DataTransfer.",
			"Enterprise.DbHealth.",
			"Enterprise.DbBackup",
			"Enterprise.DocumentScanning.",
			"Enterprise.DocumentVisualizer.",
			"Enterprise.eTail",
			"Enterprise.Faxing.",
			"Enterprise.FaxRouter.",
			"Enterprise.Freight.Agency",
			"Enterprise.Freight.CFS",
			"Enterprise.Freight.ContainerYard.",
			"Enterprise.Freight.DistanceCalculation.",
			"Enterprise.Freight.Forwarding.AWB.Business'",
			"Enterprise.Freight.Forwarding.Documents.",
			"Enterprise.Freight.Forwarding.PortMessaging.",
			"Enterprise.Freight.Forwarding.Routing.",
			"Enterprise.Freight.Forwarding.ServiceTasks",
			"Enterprise.Freight.LocalCartage",
			"Enterprise.Freight.OnlineSailingSchedules",
			"Enterprise.Freight.PortHubs.",
			"Enterprise.Freight.QuotedBookings.",
			"Enterprise.GPS.",
			"Enterprise.LandedCosting.",
			"Enterprise.LocalTransport.Mobile",
			"Enterprise.MarketingManager.",
			"Enterprise.Mobile.",
			"Enterprise.Packing.",
			"Enterprise.PAVE.",
			"Enterprise.PrintProcessing",
			"Enterprise.ProcessManagement.",
			"Enterprise.Rating",
			"Enterprise.Recruiter.",
			"Enterprise.Recruitment.",
			"Enterprise.RemoteDesktopServices",
			"Enterprise.Security.ActiveDirectory",
			"Enterprise.Telematics.",
			"Enterprise.TFSCheckInPolicy.",
			"Enterprise.TimeEngineScheduler.",
			"Enterprise.VisualBoards.",
			"Enterprise.Warehouse.",
			"CWNUnit.",
			"CargoWise.Bi.",
			"CargoWise.CalendarArithmetic",
			"CargoWise.Customs.",
			"CargoWise.Loader.",
			"CargoWise.Macros",
			"CargoWise.NGenRoot",
			"CargoWise.PdfiumWrapper",
			"CargoWise.RefDataRepo.",
			"CargoWise.RefDbRepo.",
			"CargoWise.Tools.",
			"CargoWise.VisualStudioCustomTools.",
			"CargoWise.WPF",
			"AWSSDK",
			"BuildTools",
			"CargoWise.NetworkVisualisation",
			"Microsoft.TeamFoundation.",
			"Microsoft.VisualStudio",
			"nunit",
			"Office.dll",
			"Outlook.dll",
			"Pinyin4net.dll",
			"RemotePrinting.",
			"WTG.Telematics.",
			"ZClient",
			"BorderWise",
			"BaseCodeGeneratorWithSite",
			"WiseRates.",
			"XmlDiffPatch.",
			"ChartFX.Lite.",
			"ClearImageNet.",
			"DocumentScanning.",
			"DocumentWrappers.",
			"ExcelTemplates.",
			"FirebirdSql.",
			"FlexCel.",
			"IronPython.",
			"LibPhoneNumber.",
			"Microsoft.CodeAnalysis.",
			"Microsoft.SqlServer.Management.SqlParser.",
			"Microsoft.Toolkit.Forms.UI.Controls.WebView.",
			"PdfToTextNet.",
			"WinFormHtmlEditor.",
		};

		static List<string> ExcludedEndsWithNames => new List<string>
		{
			"Test.dll",
			"Tests.dll",
			"Testing.dll",
			"TestFramework.dll",
			"TestHelper.dll",
		};

		static List<string> MustIncludeEndsWithNames => new List<string>
		{
			".Business.dll",
			".Integration.dll",
			".Shared.dll",
			".Web.dll",
		};

		string GetWebConfig()
		{
			var configFile = XDocument.Load(Path.Combine(ExecutableDirectory, "Enterprise.Services.ServiceHost.dll.config"));
			configFile.XPathSelectElement(@"//appSettings/add[@key='ServerName']").Attribute("value").Value = Db.ServerName;
			configFile.XPathSelectElement(@"//appSettings/add[@key='DatabaseName']").Attribute("value").Value = Db.DatabaseName;
			var webConfig = configFile.ToString();

			return webConfig.Replace(@"httpGetEnabled=""false""", @"httpGetEnabled=""true""");
		}

		string GlobalAsax => @"<%@ Application Codebehind=""Global.asax.cs"" Inherits=""Enterprise.Services.ServiceHost.Global"" Language=""C#"" %>";

		string ServerPath => @"<%=System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath%>";

		readonly List<string> siteNames = new List<string>();
		readonly HashSet<string> localPaths = new HashSet<string>();

		TimeSpan UpgradeTimerInterval => TimeSpan.FromMinutes(5);
		string TestApplicationName => "Services";
		string eAdaptorUserName => "test";
		string eAdaptorPassword => "test";

		WebSiteTestManager siteManager;
		Site testSite;
		string sitePath;
		string initialServerPath;

		#endregion
	}
}
