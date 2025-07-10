using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using System.Web.Http.Routing;
using CargoWise.Bi.Deployment.ReportingServices;
using CargoWise.Bi.Registration.PowerBi;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Services.ServiceHost.Tests;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence.Test
{
	class BiControllerTest : TransactionedTestCase
	{
		public void TestEmptyBiReportUserCredential()
		{
			var mockService = new Mock<IBiReportsService>();
			var controller = new BIControllerForTest(mockService.Object);
			var controllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), new HttpRequestMessage());
			controller.ControllerContext = controllerContext;
			using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SomeDataWarehouseServer"))
			using (SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BiReportCredential { Domain = "test", UserName = "test", Password = "test" }))
			using (SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BiReportCredential()))
			{
				var actionResult = controller.GetPowerBiReportsList("logistics");
				AssertEquals(((BadRequestErrorMessageResult)actionResult).Message, "Please provide a valid value for the \"BI Report Credential\" registry item in the \"System > BI\" path to access Power BI Server.");
			}
		}

		public void TestGetPowerBiReportsList()
		{
			using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SomeDataWarehouseServer"))
			using (SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BiReportCredential { Domain = "test", UserName = "test", Password = "test" }))
			using (SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SomeAnalysisServer"))
			using (SystemDataRegistry.Instance.BiAuditServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SomeAuditServer"))
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://test/someService/"))
			using (SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SomePowerBiServer/pbirs"))
			{
				var category = BiReportCategory.All;
				var mockService = new Mock<IBiReportsService>();
				mockService.Setup(s => s.GetPowerBiReportsList(It.IsAny<IPowerBiReportLinkBuilderFactory>(), It.IsAny<BiReportCategory>()))
					.Returns(Array.Empty<PowerBiReport>).Callback<IPowerBiReportLinkBuilderFactory, BiReportCategory>((p1, p2) =>
					{
						category = p2;
					}).Verifiable();
				var controller = new BIControllerForTest(mockService.Object);

				controller.GetPowerBiReportsList(category.ToString());

				mockService.Verify(s => s.GetPowerBiReportsList(It.IsAny<IPowerBiReportLinkBuilderFactory>(), It.IsAny<BiReportCategory>()), Times.Once);
				AssertEquals(BiReportCategory.All, category);
			}
		}

		static void AssertJsonResult(object expected, IHttpActionResult actionResult)
		{
			var expectedJson = JsonConvert.SerializeObject(expected);

			var response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			var actualJson = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

			AssertEquals(expectedJson, actualJson);
		}

		[UseSnapshotProtection]
		public void TestEmptyPowerBiPortalUrl()
		{
			using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SomeDataWarehouseServer"))
			using (SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BiReportCredential { Domain = "test", UserName = "test", Password = "test" }))
			using (SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SomeAnalysisServer"))
			using (SystemDataRegistry.Instance.BiAuditServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SomeAuditServer"))
			using (SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				var mockService = new Mock<IBiReportsService>();
				var controller = new BIControllerForTest(mockService.Object);
				var controllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), new HttpRequestMessage());
				controller.ControllerContext = controllerContext;

				var actionResult = controller.GetPowerBiReportsList("logistics");
				AssertEquals("Please enter a valid value for the \"Power BI Web Portal URL\" registry item in the \"System > BI\" path.", ((BadRequestErrorMessageResult)actionResult).Message);
			}
		}

		[UseSnapshotProtection]
		public void TestGetCompanyLogo()
		{
			var controller = new BiController();
			controller.Request = new HttpRequestMessage(HttpMethod.Get, "http://www.goofygoober/cw1api/powerbi/getCompanyLogo");

			var biControllerHelpers = new BiControllerHelper();
			var newCompany = biControllerHelpers.CreateANewCompany();
			var newBranch = biControllerHelpers.CreateANewBranch(newCompany);
			var newStaff = biControllerHelpers.CreateNewStaff("Tester", newBranch);

			using (EnvProxy.Instance.SetTemporaryUserContext("Tester", newBranch.PK.ToGuid(), Guid.Empty))
			{
				AssertEquals(EnvProxy.Instance.CurrentUser.PK, newStaff.PK.ToGuid());
				AssertEquals(EnvProxy.Instance.CurrentCompany.PK, newCompany.PK.ToGuid());

				CreateImageInRegistryAndValidateGetCompanyLogoFromRegistry(controller, 30, 40, newCompany.PK.ToGuid());
			}

			ClearImageInRegistryAndValidateGetCompanyLogoFromRegistry(controller);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestGetReportConfigurations()
		{
			var factory = new BusinessObjectFactory();

			var homeCompany = factory.NewWithValidTestData<GlbCompany>();
			homeCompany.CompanyName = "homeCompany";
			homeCompany.GC_Code = "HMC";

			var homeBranch = factory.NewWithValidTestData<GlbBranch>();
			homeBranch.GB_BranchName = "homeBranch";
			homeBranch.GB_Code = "HMB";
			homeBranch.GB_GC = homeCompany.PK;

			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "testStaff";
			staff.GS_Code = "STF";
			staff.GS_GB_HomeBranch = homeBranch.PK;

			factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), homeBranch.PK.ToGuid(), Guid.Empty))
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				var companyCode = (EnvProxy.Instance.CurrentUser as GlbStaff)?.HomeBranch?.Company.GC_Code.ToString();
				var sqlText = @"
INSERT INTO [biadmin].[ReportConfiguration] (ReportConfigurationID, CompanyCode, ReportName, VisualName, ConfigurationName, ColumnConfiguration, IsDefault, LastModifiedDateUTC)
VALUES
('ec2ab390-6ce5-4bd8-bab0-20f09c93a739', @CompanyCode, 'Shipment Profile Report', 'VisualC', 'ConfigA', '{""reportConfigurationID"":""ec2ab390-6ce5-4bd8-bab0-20f09c93a739"",""columnNames"":{""Category"":""Cat"",""Demographic"":""Demo""},""name"":""ConfigA"",""hidden"":false,""isDefault"":true,""columnsDisplayed"":[""Category"",""Demographic"",""Channels""],""LastModifiedDateUTC"":""1/01/1900 12:00:00 AM""}', 0, '1/01/1900 12:00:00 AM'),
('f464ea64-f4e8-4cf4-9b6a-70f9553af565', @CompanyCode, 'Shipment Profile Report', 'VisualC', 'ConfigB', '{""reportConfigurationID"":""f464ea64-f4e8-4cf4-9b6a-70f9553af565"",""columnNames"":{""Quantity"":""Quant""},""name"":""ConfigB"",""hidden"":false,""isDefault"":false,""columnsDisplayed"":[""Category"",""Quantity"",""Demographic""],""LastModifiedDateUTC"":""1/01/1900 12:00:00 AM""}', 0, '1/01/1900 12:00:00 AM'),
('92a57429-8768-47dc-a8af-ffbaec717f57', @CompanyCode, 'Shipment Profile Report', 'VisualC', 'ConfigC', '{""reportConfigurationID"":""92a57429-8768-47dc-a8af-ffbaec717f57"",""columnNames"":{""Demographic"":""Values""},""name"":""ConfigC"",""hidden"":false,""isDefault"":false,""columnsDisplayed"":[""Quantity"",""Demographic""],""LastModifiedDateUTC"":""1/01/1900 12:00:00 AM""}', 0, '1/01/1900 12:00:00 AM');
";

				using (var command = connection.Command(sqlText))
				{
					command.AddParameter("@CompanyCode", SqlDbType.VarChar, companyCode);
					command.ExecuteNonQuery();
				}

				var controller = new BiController();
				controller.Request = new HttpRequestMessage(HttpMethod.Get, "http://www.goofygoober/cw1api/powerbi/getReportConfigurations/WTG");

				var response = controller.GetReportConfigurations();
				var expectedResponse =
					"[{\"reportConfigurationID\":\"ec2ab390-6ce5-4bd8-bab0-20f09c93a739\",\"columnNames\":{\"Category\":\"Cat\",\"Demographic\":\"Demo\"},\"name\":\"ConfigA\",\"hidden\":false,\"isDefault\":true,\"columnsDisplayed\":[\"Category\",\"Demographic\",\"Channels\"],\"LastModifiedDateUTC\":\"1/01/1900 12:00:00 AM\"}," +
					"{\"reportConfigurationID\":\"f464ea64-f4e8-4cf4-9b6a-70f9553af565\",\"columnNames\":{\"Quantity\":\"Quant\"},\"name\":\"ConfigB\",\"hidden\":false,\"isDefault\":false,\"columnsDisplayed\":[\"Category\",\"Quantity\",\"Demographic\"],\"LastModifiedDateUTC\":\"1/01/1900 12:00:00 AM\"}," +
					"{\"reportConfigurationID\":\"92a57429-8768-47dc-a8af-ffbaec717f57\",\"columnNames\":{\"Demographic\":\"Values\"},\"name\":\"ConfigC\",\"hidden\":false,\"isDefault\":false,\"columnsDisplayed\":[\"Quantity\",\"Demographic\"],\"LastModifiedDateUTC\":\"1/01/1900 12:00:00 AM\"}]";
				AssertJsonResult(expectedResponse, response);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.EDW })]
		public void TestAddReportConfigurations()
		{
			var controller = new BiController();
			var currentDate = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);

			var body = new ConfigurationBody { ReportConfigurationID = "ec2ab390-6ce5-4bd8-bab0-20f09c93a739", ConfigurationName = "ConfigA", ColumnConfiguration = "{\"reportConfigurationID\":\"ec2ab390-6ce5-4bd8-bab0-20f09c93a739\",\"columnNames\":{\"Category\":\"Cat\",\"Demographic\":\"Demo\"},\"name\":\"ConfigA\",\"hidden\":false,\"isDefault\":true,\"columnsDisplayed\":[\"Category\",\"Demographic\",\"Channels\"],\"LastModifiedDateUTC\":\"" + currentDate + "\"}", IsDefault = true, LastModifiedDateUTC = currentDate };
			controller.AddReportConfigurations(body);

			var sqlText = $"select @IsDefault = IsDefault, @ColumnConfiguration = ColumnConfiguration from [biadmin].[ReportConfiguration] where ReportConfigurationID = @ReportConfigurationID";
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddOutputParameter("@IsDefault", SqlDbType.Bit, 0, 0, 0, 0);
				cmd.AddOutputParameter("@ColumnConfiguration", SqlDbType.VarChar, -1, 0, 0, 0);
				cmd.AddParameter("@ReportConfigurationID", SqlDbType.VarChar, "ec2ab390-6ce5-4bd8-bab0-20f09c93a739");

				cmd.ExecuteNonQuery();

				var actualIsDefault = Convert.ToBoolean(cmd.GetParameterValue("@IsDefault"));
				var actualColumnConfiguration = cmd.GetParameterValue("@ColumnConfiguration").ToString();
				var expectedResponse = "{\"reportConfigurationID\":\"ec2ab390-6ce5-4bd8-bab0-20f09c93a739\",\"columnNames\":{\"Category\":\"Cat\",\"Demographic\":\"Demo\"},\"name\":\"ConfigA\",\"hidden\":false,\"isDefault\":true,\"columnsDisplayed\":[\"Category\",\"Demographic\",\"Channels\"],\"LastModifiedDateUTC\":\"" + currentDate + "\"}";
				AssertEquals("New Configuration should be added to EDW", expectedResponse, actualColumnConfiguration);
				AssertEquals("Added Configuration should be the default configuration", actualIsDefault, true);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.EDW })]
		public void TestDeleteReportConfigurations()
		{
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				var sqlCommand = @"
INSERT INTO [biadmin].[ReportConfiguration] (ReportConfigurationID, CompanyCode, ReportName, VisualName, ConfigurationName, ColumnConfiguration, IsDefault, LastModifiedDateUTC)
VALUES
('ec2ab390-6ce5-4bd8-bab0-20f09c93a739', 'WTG', 'Shipment Profile Report', 'VisualC', 'ConfigA', '{""reportConfigurationID"":""ec2ab390-6ce5-4bd8-bab0-20f09c93a739"", ""columnNames"":{""Category"":""Cat"",""Demographic"":""Demo""},""name"":""ConfigA"",""hidden"":false,""isDefault"":true,""columnsDisplayed"":[""Category"",""Demographic"",""Channels""],""LastModifiedDateUTC"":""1/01/1900 12:00:00 AM""}', 0, SYSDATETIME()),
('ec2ab390-6ce5-4bd8-bab0-20f09c93a740', 'EDI', 'Shipment Profile Report', 'VisualC', 'ConfigA', '{""reportConfigurationID"":""ec2ab390-6ce5-4bd8-bab0-20f09c93a740"",""columnNames"":{""Category"":""Cat"",""Demographic"":""Demo""},""name"":""ConfigA"",""hidden"":false,""isDefault"":true,""columnsDisplayed"":[""Category"",""Demographic"",""Channels""],""LastModifiedDateUTC"":""1/01/1900 12:00:00 AM""}', 0, SYSDATETIME());";
				connection.ExecuteNonQuery(sqlCommand);

				var controller = new BiController();
				controller.DeleteReportConfigurations("ec2ab390-6ce5-4bd8-bab0-20f09c93a739");

				var sqlText = $"if exists (select null from [biadmin].[ReportConfiguration] where ReportConfigurationID = @ReportConfigurationID) select 1 else select 0";

				using (var cmd = connection.Command(sqlText))
				{
					cmd.AddParameter("@ReportConfigurationID", SqlDbType.VarChar, "ec2ab390-6ce5-4bd8-bab0-20f09c93a739");
					var exists = Convert.ToBoolean(cmd.ExecuteScalar());
					Assert("Configuration should have been deleted", !exists);
				}

				using (var cmd = connection.Command(sqlText))
				{
					cmd.AddParameter("@ReportConfigurationID", SqlDbType.VarChar, "ec2ab390-6ce5-4bd8-bab0-20f09c93a740");
					var exists = Convert.ToBoolean(cmd.ExecuteScalar());
					Assert("Configuration should not have been deleted", exists);
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.EDW })]
		public void TestUpdateReportConfigurations()
		{
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				var sqlCommand = @"
INSERT INTO [biadmin].[ReportConfiguration] (ReportConfigurationID, CompanyCode, ReportName, VisualName, ConfigurationName, ColumnConfiguration, IsDefault, LastModifiedDateUTC)
VALUES
('ec2ab390-6ce5-4bd8-bab0-20f09c93a739', 'WTG', 'Shipment Profile Report', 'VisualC', 'ConfigA', '{""reportConfigurationID"":""ec2ab390-6ce5-4bd8-bab0-20f09c93a739"",""columnNames"":{""Category"":""Cat"",""Demographic"":""Demo""},""name"":""ConfigA"",""hidden"":false,""isDefault"":true,""columnsDisplayed"":[""Category"",""Demographic"",""Channels""],""LastModifiedDateUTC"":""01/01/1900 00:00:00""}', 0, 01/01/1900);";
				connection.ExecuteNonQuery(sqlCommand);

				var controller = new BiController();
				var body = new ConfigurationBody { ReportConfigurationID = "ec2ab390-6ce5-4bd8-bab0-20f09c93a739", ConfigurationName = "ConfigA", ColumnConfiguration = "{\"reportConfigurationID\":\"ec2ab390-6ce5-4bd8-bab0-20f09c93a739\",\"columnNames\":{\"Category\":\"Category\",\"Demographic\":\"Demo\"},\"name\":\"ConfigA\",\"hidden\":false,\"isDefault\":false,\"columnsDisplayed\":[\"Category\",\"Demographic\",\"Channels\"],\"LastModifiedDateUTC\":\"01/01/1900 00:00:00\"}", IsDefault = false, LastModifiedDateUTC = "01/01/1900 00:00:00", NewModifiedDateUTC = "02/02/1900 00:00:00" }; // copying SQL date format
				controller.UpdateReportConfigurations(body);

				var sqlText = $"select @IsDefault = IsDefault, @ColumnConfiguration = ColumnConfiguration from [biadmin].[ReportConfiguration] where ReportConfigurationID = @ReportConfigurationID";
				using (var cmd = connection.Command(sqlText))
				{
					cmd.AddOutputParameter("@IsDefault", SqlDbType.Bit, 0, 0, 0, 0);
					cmd.AddOutputParameter("@ColumnConfiguration", SqlDbType.VarChar, -1, 0, 0, 0);

					cmd.AddParameter("@ReportConfigurationID", SqlDbType.VarChar, "ec2ab390-6ce5-4bd8-bab0-20f09c93a739");
					cmd.ExecuteNonQuery();

					var actualIsDefault = Convert.ToBoolean(cmd.GetParameterValue("@IsDefault"));
					var actualColumnConfiguration = cmd.GetParameterValue("@ColumnConfiguration").ToString();
					var expectedResponse = "{\"reportConfigurationID\":\"ec2ab390-6ce5-4bd8-bab0-20f09c93a739\",\"columnNames\":{\"Category\":\"Category\",\"Demographic\":\"Demo\"},\"name\":\"ConfigA\",\"hidden\":false,\"isDefault\":false,\"columnsDisplayed\":[\"Category\",\"Demographic\",\"Channels\"],\"LastModifiedDateUTC\":\"02/02/1900 00:00:00\"}";
					AssertEquals("Configuration should be updated to the new values", expectedResponse, actualColumnConfiguration);
					AssertEquals("Configuration should not longer be the default", actualIsDefault, false);
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.EDW })]
		public void TestUpdateReportConfigurationsOnRecentlyModifiedConfiguration()
		{
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				var sqlCommand = @"
INSERT INTO [biadmin].[ReportConfiguration] (ReportConfigurationID, CompanyCode, ReportName, VisualName, ConfigurationName, ColumnConfiguration, IsDefault, LastModifiedDateUTC)
VALUES
('ec2ab390-6ce5-4bd8-bab0-20f09c93a739', 'WTG', 'Shipment Profile Report', 'VisualC', 'ConfigA', '{""reportConfigurationID"":""ec2ab390-6ce5-4bd8-bab0-20f09c93a739"",""columnNames"":{""Category"":""Cat"",""Demographic"":""Demo""},""name"":""ConfigA"",""hidden"":false,""isDefault"":true,""columnsDisplayed"":[""Category"",""Demographic"",""Channels""],""LastModifiedDateUTC"":""1/01/1900 12:00:00 AM""}', 0, 01/01/1900);";
				connection.ExecuteNonQuery(sqlCommand);

				var controller = new BiController();
				var body = new ConfigurationBody { ReportConfigurationID = "ec2ab390-6ce5-4bd8-bab0-20f09c93a739", ConfigurationName = "ConfigA", ColumnConfiguration = "{\"reportConfigurationID\":\"ec2ab390-6ce5-4bd8-bab0-20f09c93a739\",\"columnNames\":{\"Category\":\"Category\",\"Demographic\":\"Demo\"},\"name\":\"ConfigA\",\"hidden\":false,\"isDefault\":false,\"columnsDisplayed\":[\"Category\",\"Demographic\",\"Channels\"],\"LastModifiedDateUTC\":\"2020-01-03 11:47:40.417\"}", IsDefault = false, LastModifiedDateUTC = "2020-01-01 11:00:00", NewModifiedDateUTC = "2020-01-01 12:00:00" };

				try
				{
					controller.UpdateReportConfigurations(body);
					Assert(false);
				}
				catch (NewReportConfigurationConcurrencyException ex)
				{
					AssertEquals(string.Format(CultureInfo.InvariantCulture, "This configuration has been modified recently, please refresh your configurations"), ex.Message);
				}
			}
		}

		void CreateImageInRegistryAndValidateGetCompanyLogoFromRegistry(BiController controller, int width, int height, Guid companyPk)
		{
			using (var imageToStore = CreateATestImage(width, height))
			{
				using (SystemDataRegistry.Instance.BiReportCompanyLogo.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, imageToStore))
				{
					var imageInRegistry = SystemDataRegistry.Instance.BiReportCompanyLogo.GetValueWithoutFallback(companyPk, Guid.Empty, Guid.Empty);
					var expectedBase64Image = ConvertImageToBase64String(imageInRegistry);
					var actualJson = controller.GetCompanyLogo();
					AssertJsonResult(expectedBase64Image, actualJson);
				}
			}
		}

		void ClearImageInRegistryAndValidateGetCompanyLogoFromRegistry(BiController controller)
		{
			var expectedBase64Image = string.Empty;

			using (SystemDataRegistry.Instance.BiReportCompanyLogo.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, null))
			{
				var actualJson = controller.GetCompanyLogo();
				AssertJsonResult(expectedBase64Image, actualJson);
			}
		}

		string ConvertImageToBase64String(Image image)
		{
			var imageBytes = new ImageRegistryDataType().Serialise(image);
			return Convert.ToBase64String(imageBytes);
		}

		Image CreateATestImage(int width, int height)
		{
			var bitmap = new Bitmap(width, height);
			byte[] bytes;

			using (var tempFile = TempFile.New())
			{
				bitmap.Save(tempFile.Filename);
				bytes = File.ReadAllBytes(tempFile.Filename);
			}

			var stream = new MemoryStream(bytes);
			return Image.FromStream(stream);
		}

		[UseSnapshotProtection]
		public void TestAnalyticsAPIFailsIfSecurityNotAllowed()
		{
			Task.Run(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var branch1 = CreateANewBranch("XYZ", "XYZ");
					var staff = Factory.NewWithValidTestData<GlbStaff>();
					staff.GS_GB_HomeBranch = branch1.PK;
					Factory.Save();

					var cancellationToken = new CancellationToken(false);
					var bc = new BiController();
					var glowAuthenticationTicketIdentity = GlowTicketTestHelper.CreateStaffIdentity(staff, branch1.PK.ToGuid(), Env.CurrentDepartmentPK);

					var httpActionContext = new HttpActionContext()
					{
						ControllerContext = new HttpControllerContext()
						{
							Controller = new DummyController(glowAuthenticationTicketIdentity),
							Configuration = new HttpConfiguration()
						}
					};
					bc.ControllerContext = httpActionContext.ControllerContext;
					bc.Request = new HttpRequestMessage();
					bc.Request.RequestUri = new Uri("https://myReportServer.com/PBIRS");

					using (EnvProxy.Instance.SetTemporaryUserContext(staff.GS_LoginName, branch1.PK.ToGuid(), Guid.Empty))
					using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SomeDataWarehouseServer"))
					using (SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BiReportCredential { Domain = "test", UserName = "test", Password = "test" }))
					using (SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SomeAnalysisServer"))
					using (SystemDataRegistry.Instance.BiAuditServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SomeAuditServer"))
					using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://test/someService/"))
					using (SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SomePowerBiServer/pbirs"))
					{
						Env.Security.AnalysisServices.IsAllowed = false;
						var actionResult = bc.GetShipmentProfileReportData(BIAPIServiceConstants.CSV, "S0000000");
						var response = actionResult.ExecuteAsync(new CancellationToken(false));
						var byteStream = response.Result.Content.ReadAsStreamAsync();
						var expectedError = "{\"Message\":\"Incorrect API Request: You do not have the appropriate security rights to run this function.\\r\\n\\r\\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:\\r\\n\\r\\nManage -> Business Intelligence & Analytics -> API -> Analytics Data\"}";
						AssertEquals(expectedError, new StreamReader(byteStream.Result).ReadToEnd());
					}
				}
				Factory.RelinquishThreadOwnership();
			}).Wait();
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW }, skipTransaction: true)]
		public void TestLoadReportsShowsErrorOnHttpRequestException()
		{
			var biControllerHelpers = new BiControllerHelper();
			var controller = new BiController();
			controller.Request = new HttpRequestMessage(HttpMethod.Get, "http://www.goofygoober/cw1api/analytics/loadReport/woozer");
			var newDepartment = biControllerHelpers.CreateANewDepartment();
			var newBranch = biControllerHelpers.CreateANewBranch("ABZ", "ABZ");

			var staff = biControllerHelpers.Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = newBranch.PK;
			biControllerHelpers.Factory.Save();
			using (EnvProxy.Instance.SetTemporaryUserContext(staff.GS_LoginName, newBranch.PK.ToGuid(), newDepartment.PK.ToGuid()))
			using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
			using (SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, biControllerHelpers.GetBiReportImpersonatedUserCredential()))
			using (SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "AnalysisServer"))
			using (SystemDataRegistry.Instance.BiAuditServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "AuditServer"))
			using (SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://myreportserver/pbirs"))
			{
				var response = controller.GetPowerBiReportData("testPath").Result;
				AssertEquals("<html><body>The Report server can't be reached. An error occurred while sending the request.</body></html>", response.Content.ReadAsStringAsync().Result);

				response = controller.GetPowerBiReport("testPath").Result;
				AssertEquals("<html><body>The Report server can't be reached. An error occurred while sending the request.</body></html>", response.Content.ReadAsStringAsync().Result);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW }, skipTransaction: true)]
		public void TestGetPowerBiReportPreventsCompanyFilterSpoofing()
		{
			var biControllerHelpers = new BiControllerHelper();

			using (Db.DisposableActionForDbConnection())
			{
				Env.Security.AuditServices.IsAllowed = true;
			}

			Task.Factory.StartNew(() =>
			{
				var controller = biControllerHelpers.GenerateBiControllerWithContext("XYZ");

				using (Db.DisposableActionForDbConnection())
				{
					var staff = biControllerHelpers.Factory.NewWithValidTestData<GlbStaff>();
					biControllerHelpers.Factory.Save();

					using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "DataWarehouseServer"))
					using (SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, biControllerHelpers.GetBiReportImpersonatedUserCredential()))
					using (SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "AnalysisServer"))
					using (SystemDataRegistry.Instance.BiAuditServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "AuditServer"))
					using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://test/someService/"))
					using (SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SomePowerBiServer/pbirs"))
					{
						var path = "MyReport?filter=Company_x0020_Branch%2FCompany_x0020_Code eq 'XXX' and Company_x0020_Branch_x0020_Helper%2FCompany_x0020_Code eq 'XXX' and Company_x0020_Country/Country_x0020_Code eq 'YYY'&test=shouldStillAppear";
						var responseMessage = controller.GetPowerBiReport(path, spoofingTest: true);
						AssertContains($"?test=shouldStillAppear", controller.LastPowerBiReportPath);
					}
				}
				biControllerHelpers.Factory.RelinquishThreadOwnership();
			}).Wait();

			Task.Factory.StartNew(() =>
			{
				biControllerHelpers.Factory.TakeThreadOwnership();
				var controller = biControllerHelpers.GenerateBiControllerWithContext("ABZ");

				using (Db.DisposableActionForDbConnection())
				{
					var staff = biControllerHelpers.Factory.NewWithValidTestData<GlbStaff>();
					biControllerHelpers.Factory.Save();

					using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "DataWarehouseServer"))
					using (SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, biControllerHelpers.GetBiReportImpersonatedUserCredential()))
					using (SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "AnalysisServer"))
					using (SystemDataRegistry.Instance.BiAuditServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "AuditServer"))
					using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://test/someService/"))
					using (SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SomePowerBiServer/pbirs"))
					{
						var path = "MyReport?SomeParameter=shouldbefirst&filter=Company_x0020_Branch%2FCompany_x0020_Code eq 'XXX' and Company_x0020_Branch_x0020_Helper%2FCompany_x0020_Code eq 'XXX' and Company_x0020_Country/Country_x0020_Code eq 'YYY'&test=shouldStillAppear";
						var responseMessage = controller.GetPowerBiReport(path, spoofingTest: true);
						AssertContains($"?SomeParameter=shouldbefirst&test=shouldStillAppear", controller.LastPowerBiReportPath);
					}
				}
				biControllerHelpers.Factory.RelinquishThreadOwnership();
			}).Wait();
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW }, skipTransaction: true)]
		public void TestGetPowerBiReportHttpClientHandlerVariableNotNull()
		{
			var biControllerHelpers = new BiControllerHelper();
			var controller = new BiController();
			controller.Request = new HttpRequestMessage(HttpMethod.Get, "http://www.goofygoober/cw1api/analytics/loadReport/woozer");
			var newDepartment = biControllerHelpers.CreateANewDepartment();
			var newBranch = biControllerHelpers.CreateANewBranch("ABZ", "ABZ");

			var staff = biControllerHelpers.Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = newBranch.PK;
			biControllerHelpers.Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(staff.GS_LoginName, newBranch.PK.ToGuid(), newDepartment.PK.ToGuid()))
			using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
			using (SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, biControllerHelpers.GetBiReportImpersonatedUserCredential()))
			using (SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "AnalysisServer"))
			using (SystemDataRegistry.Instance.BiAuditServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "AuditServer"))
			using (SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://myreportserver/pbirs"))
			{
				AssertNull(controller.httpClientHandler);
				var response = controller.GetPowerBiReport("testPath").Result;
				var result = response.Content.ReadAsStringAsync().Result;
				AssertNotNull(controller.httpClientHandler);
				AssertEquals(true, controller.httpClientHandler.UseDefaultCredentials);
			}
		}

		BusinessObjectFactory Factory
		{
			get
			{
				using (Db.DisposableActionForDbConnection())
				{
					return factory = factory ?? new BusinessObjectFactory();
				}
			}
		}

		BusinessObjectFactory factory;

		GlbBranch CreateANewBranch(string companyCode, string branchCode)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var branch = Factory.NewWithValidTestData<GlbBranch>();

				branch.FillWithValidTestData();
				branch.Company.GC_Code = companyCode;
				branch.GB_Code = branchCode;
				factory.Save();
				return branch;
			}
		}

		public class TestData
		{
			public static string expected1 = "[{\"__$row_number\":201,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AMkAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":0,\"AA_Description\":\"Test: 0\"}]";
			public static string expected2 = "[{\"__$row_number\":1,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AAEAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":200,\"AA_Description\":\"Test: 200\"}," +
				"{\"__$row_number\":2,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AAIAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":199,\"AA_Description\":\"Test: 199\"}," +
				"{\"__$row_number\":3,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AAMAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":198,\"AA_Description\":\"Test: 198\"}," +
				"{\"__$row_number\":4,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AAQAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":197,\"AA_Description\":\"Test: 197\"}," +
				"{\"__$row_number\":5,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AAUAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":196,\"AA_Description\":\"Test: 196\"}," +
				"{\"__$row_number\":6,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AAYAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":195,\"AA_Description\":\"Test: 195\"}," +
				"{\"__$row_number\":7,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AAcAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":194,\"AA_Description\":\"Test: 194\"}," +
				"{\"__$row_number\":8,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AAgAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":193,\"AA_Description\":\"Test: 193\"}," +
				"{\"__$row_number\":9,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AAkAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":192,\"AA_Description\":\"Test: 192\"}," +
				"{\"__$row_number\":10,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AAoAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":191,\"AA_Description\":\"Test: 191\"}," +
				"{\"__$row_number\":11,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AAsAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":190,\"AA_Description\":\"Test: 190\"}," +
				"{\"__$row_number\":12,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AAwAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":189,\"AA_Description\":\"Test: 189\"}," +
				"{\"__$row_number\":13,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AA0AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":188,\"AA_Description\":\"Test: 188\"}," +
				"{\"__$row_number\":14,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AA4AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":187,\"AA_Description\":\"Test: 187\"}," +
				"{\"__$row_number\":15,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AA8AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":186,\"AA_Description\":\"Test: 186\"}," +
				"{\"__$row_number\":16,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"ABAAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":185,\"AA_Description\":\"Test: 185\"}," +
				"{\"__$row_number\":17,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"ABEAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":184,\"AA_Description\":\"Test: 184\"}," +
				"{\"__$row_number\":18,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"ABIAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":183,\"AA_Description\":\"Test: 183\"}," +
				"{\"__$row_number\":19,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"ABMAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":182,\"AA_Description\":\"Test: 182\"}," +
				"{\"__$row_number\":20,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"ABQAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":181,\"AA_Description\":\"Test: 181\"}," +
				"{\"__$row_number\":21,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"ABUAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":180,\"AA_Description\":\"Test: 180\"}," +
				"{\"__$row_number\":22,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"ABYAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":179,\"AA_Description\":\"Test: 179\"}," +
				"{\"__$row_number\":23,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"ABcAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":178,\"AA_Description\":\"Test: 178\"}," +
				"{\"__$row_number\":24,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"ABgAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":177,\"AA_Description\":\"Test: 177\"}," +
				"{\"__$row_number\":25,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"ABkAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":176,\"AA_Description\":\"Test: 176\"}," +
				"{\"__$row_number\":26,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"ABoAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":175,\"AA_Description\":\"Test: 175\"}," +
				"{\"__$row_number\":27,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"ABsAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":174,\"AA_Description\":\"Test: 174\"}," +
				"{\"__$row_number\":28,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"ABwAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":173,\"AA_Description\":\"Test: 173\"}," +
				"{\"__$row_number\":29,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AB0AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":172,\"AA_Description\":\"Test: 172\"}," +
				"{\"__$row_number\":30,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AB4AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":171,\"AA_Description\":\"Test: 171\"}," +
				"{\"__$row_number\":31,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AB8AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":170,\"AA_Description\":\"Test: 170\"}," +
				"{\"__$row_number\":32,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"ACAAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":169,\"AA_Description\":\"Test: 169\"}," +
				"{\"__$row_number\":33,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"ACEAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":168,\"AA_Description\":\"Test: 168\"}," +
				"{\"__$row_number\":34,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"ACIAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":167,\"AA_Description\":\"Test: 167\"}," +
				"{\"__$row_number\":35,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"ACMAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":166,\"AA_Description\":\"Test: 166\"}," +
				"{\"__$row_number\":36,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"ACQAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":165,\"AA_Description\":\"Test: 165\"}," +
				"{\"__$row_number\":37,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"ACUAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":164,\"AA_Description\":\"Test: 164\"}," +
				"{\"__$row_number\":38,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"ACYAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":163,\"AA_Description\":\"Test: 163\"}," +
				"{\"__$row_number\":39,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"ACcAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":162,\"AA_Description\":\"Test: 162\"}," +
				"{\"__$row_number\":40,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"ACgAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":161,\"AA_Description\":\"Test: 161\"}]";
			public static string expected3 = "[{\"__$row_number\":81,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AFEAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":120,\"AA_Description\":\"Test: 120\"}," +
				"{\"__$row_number\":82,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AFIAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":119,\"AA_Description\":\"Test: 119\"}," +
				"{\"__$row_number\":83,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AFMAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":118,\"AA_Description\":\"Test: 118\"}," +
				"{\"__$row_number\":84,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AFQAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":117,\"AA_Description\":\"Test: 117\"}," +
				"{\"__$row_number\":85,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AFUAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":116,\"AA_Description\":\"Test: 116\"}," +
				"{\"__$row_number\":86,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AFYAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":115,\"AA_Description\":\"Test: 115\"}," +
				"{\"__$row_number\":87,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AFcAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":114,\"AA_Description\":\"Test: 114\"}," +
				"{\"__$row_number\":88,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AFgAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":113,\"AA_Description\":\"Test: 113\"}," +
				"{\"__$row_number\":89,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AFkAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":112,\"AA_Description\":\"Test: 112\"}," +
				"{\"__$row_number\":90,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AFoAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":111,\"AA_Description\":\"Test: 111\"}," +
				"{\"__$row_number\":91,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AFsAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":110,\"AA_Description\":\"Test: 110\"}," +
				"{\"__$row_number\":92,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AFwAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":109,\"AA_Description\":\"Test: 109\"}," +
				"{\"__$row_number\":93,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AF0AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":108,\"AA_Description\":\"Test: 108\"}," +
				"{\"__$row_number\":94,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AF4AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":107,\"AA_Description\":\"Test: 107\"}," +
				"{\"__$row_number\":95,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AF8AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":106,\"AA_Description\":\"Test: 106\"}," +
				"{\"__$row_number\":96,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AGAAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":105,\"AA_Description\":\"Test: 105\"}," +
				"{\"__$row_number\":97,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AGEAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":104,\"AA_Description\":\"Test: 104\"}," +
				"{\"__$row_number\":98,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AGIAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":103,\"AA_Description\":\"Test: 103\"}," +
				"{\"__$row_number\":99,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AGMAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":102,\"AA_Description\":\"Test: 102\"}," +
				"{\"__$row_number\":100,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AGQAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":101,\"AA_Description\":\"Test: 101\"}," +
				"{\"__$row_number\":101,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AGUAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":100,\"AA_Description\":\"Test: 100\"}," +
				"{\"__$row_number\":102,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AGYAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":99,\"AA_Description\":\"Test: 99\"}," +
				"{\"__$row_number\":103,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AGcAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":98,\"AA_Description\":\"Test: 98\"}," +
				"{\"__$row_number\":104,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AGgAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":97,\"AA_Description\":\"Test: 97\"}," +
				"{\"__$row_number\":105,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AGkAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":96,\"AA_Description\":\"Test: 96\"}," +
				"{\"__$row_number\":106,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AGoAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":95,\"AA_Description\":\"Test: 95\"}," +
				"{\"__$row_number\":107,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AGsAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":94,\"AA_Description\":\"Test: 94\"}," +
				"{\"__$row_number\":108,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AGwAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":93,\"AA_Description\":\"Test: 93\"}," +
				"{\"__$row_number\":109,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AG0AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":92,\"AA_Description\":\"Test: 92\"}," +
				"{\"__$row_number\":110,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AG4AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":91,\"AA_Description\":\"Test: 91\"}," +
				"{\"__$row_number\":111,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AG8AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":90,\"AA_Description\":\"Test: 90\"}," +
				"{\"__$row_number\":112,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AHAAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":89,\"AA_Description\":\"Test: 89\"}," +
				"{\"__$row_number\":113,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AHEAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":88,\"AA_Description\":\"Test: 88\"}," +
				"{\"__$row_number\":114,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AHIAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":87,\"AA_Description\":\"Test: 87\"}," +
				"{\"__$row_number\":115,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AHMAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":86,\"AA_Description\":\"Test: 86\"}," +
				"{\"__$row_number\":116,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AHQAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":85,\"AA_Description\":\"Test: 85\"}," +
				"{\"__$row_number\":117,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AHUAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":84,\"AA_Description\":\"Test: 84\"}," +
				"{\"__$row_number\":118,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AHYAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":83,\"AA_Description\":\"Test: 83\"}," +
				"{\"__$row_number\":119,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AHcAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":82,\"AA_Description\":\"Test: 82\"}," +
				"{\"__$row_number\":120,\"__$start_lsn\":\"AADlnAACGugAAQ==\",\"__$seqval\":\"AHgAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1907,\"__$command_id\":1,\"AA_Code\":81,\"AA_Description\":\"Test: 81\"}]";
			public static string expected4 = "[{\"__$row_number\":121,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AHkAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":11,\"BB_Description\":\"Test: 11\"}," +
				"{\"__$row_number\":122,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AHoAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":10,\"BB_Description\":\"Test: 10\"}," +
				"{\"__$row_number\":123,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AHsAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":9,\"BB_Description\":\"Test: 9\"}," +
				"{\"__$row_number\":124,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AHwAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":8,\"BB_Description\":\"Test: 8\"}," +
				"{\"__$row_number\":125,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AH0AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":7,\"BB_Description\":\"Test: 7\"}," +
				"{\"__$row_number\":126,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AH4AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":6,\"BB_Description\":\"Test: 6\"}," +
				"{\"__$row_number\":127,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AH8AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":5,\"BB_Description\":\"Test: 5\"}," +
				"{\"__$row_number\":128,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AIAAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":4,\"BB_Description\":\"Test: 4\"}," +
				"{\"__$row_number\":129,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AIEAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":3,\"BB_Description\":\"Test: 3\"}," +
				"{\"__$row_number\":130,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AIIAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":2,\"BB_Description\":\"Test: 2\"}," +
				"{\"__$row_number\":131,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AIMAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":1,\"BB_Description\":\"Test: 1\"}," +
				"{\"__$row_number\":132,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AIQAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":0,\"BB_Description\":\"Test: 0\"}]";
			public static string expected5 = "[{\"__$row_number\":1,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AAEAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":131,\"BB_Description\":\"Test: 131\"}," +
				"{\"__$row_number\":2,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AAIAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":130,\"BB_Description\":\"Test: 130\"}," +
				"{\"__$row_number\":3,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AAMAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":129,\"BB_Description\":\"Test: 129\"}," +
				"{\"__$row_number\":4,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AAQAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":128,\"BB_Description\":\"Test: 128\"}," +
				"{\"__$row_number\":5,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AAUAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":127,\"BB_Description\":\"Test: 127\"}," +
				"{\"__$row_number\":6,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AAYAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":126,\"BB_Description\":\"Test: 126\"}," +
				"{\"__$row_number\":7,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AAcAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":125,\"BB_Description\":\"Test: 125\"}," +
				"{\"__$row_number\":8,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AAgAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":124,\"BB_Description\":\"Test: 124\"}," +
				"{\"__$row_number\":9,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AAkAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":123,\"BB_Description\":\"Test: 123\"}," +
				"{\"__$row_number\":10,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AAoAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":122,\"BB_Description\":\"Test: 122\"}," +
				"{\"__$row_number\":11,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AAsAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":121,\"BB_Description\":\"Test: 121\"}," +
				"{\"__$row_number\":12,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AAwAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":120,\"BB_Description\":\"Test: 120\"}," +
				"{\"__$row_number\":13,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AA0AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":119,\"BB_Description\":\"Test: 119\"}," +
				"{\"__$row_number\":14,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AA4AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":118,\"BB_Description\":\"Test: 118\"}," +
				"{\"__$row_number\":15,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AA8AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":117,\"BB_Description\":\"Test: 117\"}," +
				"{\"__$row_number\":16,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"ABAAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":116,\"BB_Description\":\"Test: 116\"}," +
				"{\"__$row_number\":17,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"ABEAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":115,\"BB_Description\":\"Test: 115\"}," +
				"{\"__$row_number\":18,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"ABIAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":114,\"BB_Description\":\"Test: 114\"}," +
				"{\"__$row_number\":19,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"ABMAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":113,\"BB_Description\":\"Test: 113\"}," +
				"{\"__$row_number\":20,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"ABQAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":112,\"BB_Description\":\"Test: 112\"}," +
				"{\"__$row_number\":21,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"ABUAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":111,\"BB_Description\":\"Test: 111\"}," +
				"{\"__$row_number\":22,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"ABYAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":110,\"BB_Description\":\"Test: 110\"}," +
				"{\"__$row_number\":23,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"ABcAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":109,\"BB_Description\":\"Test: 109\"}," +
				"{\"__$row_number\":24,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"ABgAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":108,\"BB_Description\":\"Test: 108\"}," +
				"{\"__$row_number\":25,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"ABkAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":107,\"BB_Description\":\"Test: 107\"}," +
				"{\"__$row_number\":26,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"ABoAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":106,\"BB_Description\":\"Test: 106\"}," +
				"{\"__$row_number\":27,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"ABsAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":105,\"BB_Description\":\"Test: 105\"}," +
				"{\"__$row_number\":28,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"ABwAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":104,\"BB_Description\":\"Test: 104\"}," +
				"{\"__$row_number\":29,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AB0AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":103,\"BB_Description\":\"Test: 103\"}," +
				"{\"__$row_number\":30,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AB4AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":102,\"BB_Description\":\"Test: 102\"}," +
				"{\"__$row_number\":31,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AB8AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":101,\"BB_Description\":\"Test: 101\"}," +
				"{\"__$row_number\":32,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"ACAAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":100,\"BB_Description\":\"Test: 100\"}," +
				"{\"__$row_number\":33,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"ACEAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":99,\"BB_Description\":\"Test: 99\"}," +
				"{\"__$row_number\":34,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"ACIAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":98,\"BB_Description\":\"Test: 98\"}," +
				"{\"__$row_number\":35,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"ACMAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":97,\"BB_Description\":\"Test: 97\"}," +
				"{\"__$row_number\":36,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"ACQAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":96,\"BB_Description\":\"Test: 96\"}," +
				"{\"__$row_number\":37,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"ACUAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":95,\"BB_Description\":\"Test: 95\"}," +
				"{\"__$row_number\":38,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"ACYAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":94,\"BB_Description\":\"Test: 94\"}," +
				"{\"__$row_number\":39,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"ACcAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":93,\"BB_Description\":\"Test: 93\"}," +
				"{\"__$row_number\":40,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"ACgAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":92,\"BB_Description\":\"Test: 92\"}]";

			public static string expected6 = "[{\"__$row_number\":81,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AFEAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":51,\"BB_Description\":\"Test: 51\"}," +
				"{\"__$row_number\":82,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AFIAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":50,\"BB_Description\":\"Test: 50\"}," +
				"{\"__$row_number\":83,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AFMAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":49,\"BB_Description\":\"Test: 49\"}," +
				"{\"__$row_number\":84,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AFQAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":48,\"BB_Description\":\"Test: 48\"}," +
				"{\"__$row_number\":85,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AFUAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":47,\"BB_Description\":\"Test: 47\"}," +
				"{\"__$row_number\":86,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AFYAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":46,\"BB_Description\":\"Test: 46\"}," +
				"{\"__$row_number\":87,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AFcAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":45,\"BB_Description\":\"Test: 45\"}," +
				"{\"__$row_number\":88,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AFgAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":44,\"BB_Description\":\"Test: 44\"}," +
				"{\"__$row_number\":89,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AFkAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":43,\"BB_Description\":\"Test: 43\"}," +
				"{\"__$row_number\":90,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AFoAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":42,\"BB_Description\":\"Test: 42\"}," +
				"{\"__$row_number\":91,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AFsAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":41,\"BB_Description\":\"Test: 41\"}," +
				"{\"__$row_number\":92,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AFwAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":40,\"BB_Description\":\"Test: 40\"}," +
				"{\"__$row_number\":93,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AF0AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":39,\"BB_Description\":\"Test: 39\"}," +
				"{\"__$row_number\":94,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AF4AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":38,\"BB_Description\":\"Test: 38\"}," +
				"{\"__$row_number\":95,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AF8AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":37,\"BB_Description\":\"Test: 37\"}," +
				"{\"__$row_number\":96,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AGAAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":36,\"BB_Description\":\"Test: 36\"}," +
				"{\"__$row_number\":97,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AGEAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":35,\"BB_Description\":\"Test: 35\"}," +
				"{\"__$row_number\":98,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AGIAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":34,\"BB_Description\":\"Test: 34\"}," +
				"{\"__$row_number\":99,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AGMAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":33,\"BB_Description\":\"Test: 33\"}," +
				"{\"__$row_number\":100,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AGQAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":32,\"BB_Description\":\"Test: 32\"}," +
				"{\"__$row_number\":101,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AGUAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":31,\"BB_Description\":\"Test: 31\"}," +
				"{\"__$row_number\":102,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AGYAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":30,\"BB_Description\":\"Test: 30\"}," +
				"{\"__$row_number\":103,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AGcAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":29,\"BB_Description\":\"Test: 29\"}," +
				"{\"__$row_number\":104,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AGgAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":28,\"BB_Description\":\"Test: 28\"}," +
				"{\"__$row_number\":105,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AGkAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":27,\"BB_Description\":\"Test: 27\"}," +
				"{\"__$row_number\":106,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AGoAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":26,\"BB_Description\":\"Test: 26\"}," +
				"{\"__$row_number\":107,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AGsAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":25,\"BB_Description\":\"Test: 25\"}," +
				"{\"__$row_number\":108,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AGwAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":24,\"BB_Description\":\"Test: 24\"}," +
				"{\"__$row_number\":109,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AG0AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":23,\"BB_Description\":\"Test: 23\"}," +
				"{\"__$row_number\":110,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AG4AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":22,\"BB_Description\":\"Test: 22\"}," +
				"{\"__$row_number\":111,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AG8AAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":21,\"BB_Description\":\"Test: 21\"}," +
				"{\"__$row_number\":112,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AHAAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":20,\"BB_Description\":\"Test: 20\"}," +
				"{\"__$row_number\":113,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AHEAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":19,\"BB_Description\":\"Test: 19\"}," +
				"{\"__$row_number\":114,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AHIAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":18,\"BB_Description\":\"Test: 18\"}," +
				"{\"__$row_number\":115,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AHMAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":17,\"BB_Description\":\"Test: 17\"}," +
				"{\"__$row_number\":116,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AHQAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":16,\"BB_Description\":\"Test: 16\"}," +
				"{\"__$row_number\":117,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AHUAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":15,\"BB_Description\":\"Test: 15\"}," +
				"{\"__$row_number\":118,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AHYAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":14,\"BB_Description\":\"Test: 14\"}," +
				"{\"__$row_number\":119,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AHcAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":13,\"BB_Description\":\"Test: 13\"}," +
				"{\"__$row_number\":120,\"__$start_lsn\":\"AADlnAACGugAAg==\",\"__$seqval\":\"AHgAAAAAAAAAAA==\",\"__$operation\":2,\"__$update_mask\":\"AA==\",\"__$lsn_period\":1909,\"__$command_id\":1,\"BB_Code\":12,\"BB_Description\":\"Test: 12\"}]";
			public static string csvExpected1 = "__$row_number,__$start_lsn,__$seqval,__$operation,__$update_mask,__$lsn_period,__$command_id,AA_Code,AA_Description\r\n\"201\",\"0x0000E59C00021AE80001\",\"0x00C90000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"0\",\"Test: 0\"\r\n";
			public static string csvExpected2 = "__$row_number,__$start_lsn,__$seqval,__$operation,__$update_mask,__$lsn_period,__$command_id,AA_Code,AA_Description\r\n\"1\",\"0x0000E59C00021AE80001\",\"0x00010000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"200\",\"Test: 200\"\r\n\"" +
				"2\",\"0x0000E59C00021AE80001\",\"0x00020000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"199\",\"Test: 199\"\r\n\"" +
				"3\",\"0x0000E59C00021AE80001\",\"0x00030000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"198\",\"Test: 198\"\r\n\"" +
				"4\",\"0x0000E59C00021AE80001\",\"0x00040000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"197\",\"Test: 197\"\r\n\"" +
				"5\",\"0x0000E59C00021AE80001\",\"0x00050000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"196\",\"Test: 196\"\r\n\"" +
				"6\",\"0x0000E59C00021AE80001\",\"0x00060000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"195\",\"Test: 195\"\r\n\"" +
				"7\",\"0x0000E59C00021AE80001\",\"0x00070000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"194\",\"Test: 194\"\r\n\"" +
				"8\",\"0x0000E59C00021AE80001\",\"0x00080000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"193\",\"Test: 193\"\r\n\"" +
				"9\",\"0x0000E59C00021AE80001\",\"0x00090000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"192\",\"Test: 192\"\r\n\"" +
				"10\",\"0x0000E59C00021AE80001\",\"0x000A0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"191\",\"Test: 191\"\r\n\"" +
				"11\",\"0x0000E59C00021AE80001\",\"0x000B0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"190\",\"Test: 190\"\r\n\"" +
				"12\",\"0x0000E59C00021AE80001\",\"0x000C0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"189\",\"Test: 189\"\r\n\"" +
				"13\",\"0x0000E59C00021AE80001\",\"0x000D0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"188\",\"Test: 188\"\r\n\"" +
				"14\",\"0x0000E59C00021AE80001\",\"0x000E0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"187\",\"Test: 187\"\r\n\"" +
				"15\",\"0x0000E59C00021AE80001\",\"0x000F0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"186\",\"Test: 186\"\r\n\"" +
				"16\",\"0x0000E59C00021AE80001\",\"0x00100000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"185\",\"Test: 185\"\r\n\"" +
				"17\",\"0x0000E59C00021AE80001\",\"0x00110000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"184\",\"Test: 184\"\r\n\"" +
				"18\",\"0x0000E59C00021AE80001\",\"0x00120000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"183\",\"Test: 183\"\r\n\"" +
				"19\",\"0x0000E59C00021AE80001\",\"0x00130000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"182\",\"Test: 182\"\r\n\"" +
				"20\",\"0x0000E59C00021AE80001\",\"0x00140000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"181\",\"Test: 181\"\r\n\"" +
				"21\",\"0x0000E59C00021AE80001\",\"0x00150000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"180\",\"Test: 180\"\r\n\"" +
				"22\",\"0x0000E59C00021AE80001\",\"0x00160000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"179\",\"Test: 179\"\r\n\"" +
				"23\",\"0x0000E59C00021AE80001\",\"0x00170000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"178\",\"Test: 178\"\r\n\"" +
				"24\",\"0x0000E59C00021AE80001\",\"0x00180000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"177\",\"Test: 177\"\r\n\"" +
				"25\",\"0x0000E59C00021AE80001\",\"0x00190000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"176\",\"Test: 176\"\r\n\"" +
				"26\",\"0x0000E59C00021AE80001\",\"0x001A0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"175\",\"Test: 175\"\r\n\"" +
				"27\",\"0x0000E59C00021AE80001\",\"0x001B0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"174\",\"Test: 174\"\r\n\"" +
				"28\",\"0x0000E59C00021AE80001\",\"0x001C0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"173\",\"Test: 173\"\r\n\"" +
				"29\",\"0x0000E59C00021AE80001\",\"0x001D0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"172\",\"Test: 172\"\r\n\"" +
				"30\",\"0x0000E59C00021AE80001\",\"0x001E0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"171\",\"Test: 171\"\r\n\"" +
				"31\",\"0x0000E59C00021AE80001\",\"0x001F0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"170\",\"Test: 170\"\r\n\"" +
				"32\",\"0x0000E59C00021AE80001\",\"0x00200000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"169\",\"Test: 169\"\r\n\"" +
				"33\",\"0x0000E59C00021AE80001\",\"0x00210000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"168\",\"Test: 168\"\r\n\"" +
				"34\",\"0x0000E59C00021AE80001\",\"0x00220000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"167\",\"Test: 167\"\r\n\"" +
				"35\",\"0x0000E59C00021AE80001\",\"0x00230000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"166\",\"Test: 166\"\r\n\"" +
				"36\",\"0x0000E59C00021AE80001\",\"0x00240000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"165\",\"Test: 165\"\r\n\"" +
				"37\",\"0x0000E59C00021AE80001\",\"0x00250000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"164\",\"Test: 164\"\r\n\"" +
				"38\",\"0x0000E59C00021AE80001\",\"0x00260000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"163\",\"Test: 163\"\r\n\"" +
				"39\",\"0x0000E59C00021AE80001\",\"0x00270000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"162\",\"Test: 162\"\r\n\"" +
				"40\",\"0x0000E59C00021AE80001\",\"0x00280000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"161\",\"Test: 161\"\r\n";
			public static string csvExpected3 = "__$row_number,__$start_lsn,__$seqval,__$operation,__$update_mask,__$lsn_period,__$command_id,AA_Code,AA_Description\r\n\"" +
				"81\",\"0x0000E59C00021AE80001\",\"0x00510000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"120\",\"Test: 120\"\r\n\"" +
				"82\",\"0x0000E59C00021AE80001\",\"0x00520000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"119\",\"Test: 119\"\r\n\"" +
				"83\",\"0x0000E59C00021AE80001\",\"0x00530000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"118\",\"Test: 118\"\r\n\"" +
				"84\",\"0x0000E59C00021AE80001\",\"0x00540000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"117\",\"Test: 117\"\r\n\"" +
				"85\",\"0x0000E59C00021AE80001\",\"0x00550000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"116\",\"Test: 116\"\r\n\"" +
				"86\",\"0x0000E59C00021AE80001\",\"0x00560000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"115\",\"Test: 115\"\r\n\"" +
				"87\",\"0x0000E59C00021AE80001\",\"0x00570000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"114\",\"Test: 114\"\r\n\"" +
				"88\",\"0x0000E59C00021AE80001\",\"0x00580000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"113\",\"Test: 113\"\r\n\"" +
				"89\",\"0x0000E59C00021AE80001\",\"0x00590000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"112\",\"Test: 112\"\r\n\"" +
				"90\",\"0x0000E59C00021AE80001\",\"0x005A0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"111\",\"Test: 111\"\r\n\"" +
				"91\",\"0x0000E59C00021AE80001\",\"0x005B0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"110\",\"Test: 110\"\r\n\"" +
				"92\",\"0x0000E59C00021AE80001\",\"0x005C0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"109\",\"Test: 109\"\r\n\"" +
				"93\",\"0x0000E59C00021AE80001\",\"0x005D0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"108\",\"Test: 108\"\r\n\"" +
				"94\",\"0x0000E59C00021AE80001\",\"0x005E0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"107\",\"Test: 107\"\r\n\"" +
				"95\",\"0x0000E59C00021AE80001\",\"0x005F0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"106\",\"Test: 106\"\r\n\"" +
				"96\",\"0x0000E59C00021AE80001\",\"0x00600000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"105\",\"Test: 105\"\r\n\"" +
				"97\",\"0x0000E59C00021AE80001\",\"0x00610000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"104\",\"Test: 104\"\r\n\"" +
				"98\",\"0x0000E59C00021AE80001\",\"0x00620000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"103\",\"Test: 103\"\r\n\"" +
				"99\",\"0x0000E59C00021AE80001\",\"0x00630000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"102\",\"Test: 102\"\r\n\"" +
				"100\",\"0x0000E59C00021AE80001\",\"0x00640000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"101\",\"Test: 101\"\r\n\"" +
				"101\",\"0x0000E59C00021AE80001\",\"0x00650000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"100\",\"Test: 100\"\r\n\"" +
				"102\",\"0x0000E59C00021AE80001\",\"0x00660000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"99\",\"Test: 99\"\r\n\"" +
				"103\",\"0x0000E59C00021AE80001\",\"0x00670000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"98\",\"Test: 98\"\r\n\"" +
				"104\",\"0x0000E59C00021AE80001\",\"0x00680000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"97\",\"Test: 97\"\r\n\"" +
				"105\",\"0x0000E59C00021AE80001\",\"0x00690000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"96\",\"Test: 96\"\r\n\"" +
				"106\",\"0x0000E59C00021AE80001\",\"0x006A0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"95\",\"Test: 95\"\r\n\"" +
				"107\",\"0x0000E59C00021AE80001\",\"0x006B0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"94\",\"Test: 94\"\r\n\"" +
				"108\",\"0x0000E59C00021AE80001\",\"0x006C0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"93\",\"Test: 93\"\r\n\"" +
				"109\",\"0x0000E59C00021AE80001\",\"0x006D0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"92\",\"Test: 92\"\r\n\"" +
				"110\",\"0x0000E59C00021AE80001\",\"0x006E0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"91\",\"Test: 91\"\r\n\"" +
				"111\",\"0x0000E59C00021AE80001\",\"0x006F0000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"90\",\"Test: 90\"\r\n\"" +
				"112\",\"0x0000E59C00021AE80001\",\"0x00700000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"89\",\"Test: 89\"\r\n\"" +
				"113\",\"0x0000E59C00021AE80001\",\"0x00710000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"88\",\"Test: 88\"\r\n\"" +
				"114\",\"0x0000E59C00021AE80001\",\"0x00720000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"87\",\"Test: 87\"\r\n\"" +
				"115\",\"0x0000E59C00021AE80001\",\"0x00730000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"86\",\"Test: 86\"\r\n\"" +
				"116\",\"0x0000E59C00021AE80001\",\"0x00740000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"85\",\"Test: 85\"\r\n\"" +
				"117\",\"0x0000E59C00021AE80001\",\"0x00750000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"84\",\"Test: 84\"\r\n\"" +
				"118\",\"0x0000E59C00021AE80001\",\"0x00760000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"83\",\"Test: 83\"\r\n\"" +
				"119\",\"0x0000E59C00021AE80001\",\"0x00770000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"82\",\"Test: 82\"\r\n\"" +
				"120\",\"0x0000E59C00021AE80001\",\"0x00780000000000000000\",\"2\",\"0x00\",\"1907\",\"1\",\"81\",\"Test: 81\"\r\n";
			public static string csvExpected4 = "__$row_number,__$start_lsn,__$seqval,__$operation,__$update_mask,__$lsn_period,__$command_id,BB_Code,BB_Description\r\n\"" +
				"121\",\"0x0000E59C00021AE80002\",\"0x00790000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"11\",\"Test: 11\"\r\n\"" +
				"122\",\"0x0000E59C00021AE80002\",\"0x007A0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"10\",\"Test: 10\"\r\n\"" +
				"123\",\"0x0000E59C00021AE80002\",\"0x007B0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"9\",\"Test: 9\"\r\n\"" +
				"124\",\"0x0000E59C00021AE80002\",\"0x007C0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"8\",\"Test: 8\"\r\n\"" +
				"125\",\"0x0000E59C00021AE80002\",\"0x007D0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"7\",\"Test: 7\"\r\n\"" +
				"126\",\"0x0000E59C00021AE80002\",\"0x007E0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"6\",\"Test: 6\"\r\n\"" +
				"127\",\"0x0000E59C00021AE80002\",\"0x007F0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"5\",\"Test: 5\"\r\n\"" +
				"128\",\"0x0000E59C00021AE80002\",\"0x00800000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"4\",\"Test: 4\"\r\n\"" +
				"129\",\"0x0000E59C00021AE80002\",\"0x00810000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"3\",\"Test: 3\"\r\n\"" +
				"130\",\"0x0000E59C00021AE80002\",\"0x00820000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"2\",\"Test: 2\"\r\n\"" +
				"131\",\"0x0000E59C00021AE80002\",\"0x00830000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"1\",\"Test: 1\"\r\n\"" +
				"132\",\"0x0000E59C00021AE80002\",\"0x00840000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"0\",\"Test: 0\"\r\n";
			public static string csvExpected5 = "__$row_number,__$start_lsn,__$seqval,__$operation,__$update_mask,__$lsn_period,__$command_id,BB_Code,BB_Description\r\n\"" +
				"1\",\"0x0000E59C00021AE80002\",\"0x00010000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"131\",\"Test: 131\"\r\n\"" +
				"2\",\"0x0000E59C00021AE80002\",\"0x00020000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"130\",\"Test: 130\"\r\n\"" +
				"3\",\"0x0000E59C00021AE80002\",\"0x00030000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"129\",\"Test: 129\"\r\n\"" +
				"4\",\"0x0000E59C00021AE80002\",\"0x00040000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"128\",\"Test: 128\"\r\n\"" +
				"5\",\"0x0000E59C00021AE80002\",\"0x00050000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"127\",\"Test: 127\"\r\n\"" +
				"6\",\"0x0000E59C00021AE80002\",\"0x00060000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"126\",\"Test: 126\"\r\n\"" +
				"7\",\"0x0000E59C00021AE80002\",\"0x00070000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"125\",\"Test: 125\"\r\n\"" +
				"8\",\"0x0000E59C00021AE80002\",\"0x00080000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"124\",\"Test: 124\"\r\n\"" +
				"9\",\"0x0000E59C00021AE80002\",\"0x00090000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"123\",\"Test: 123\"\r\n\"" +
				"10\",\"0x0000E59C00021AE80002\",\"0x000A0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"122\",\"Test: 122\"\r\n\"" +
				"11\",\"0x0000E59C00021AE80002\",\"0x000B0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"121\",\"Test: 121\"\r\n\"" +
				"12\",\"0x0000E59C00021AE80002\",\"0x000C0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"120\",\"Test: 120\"\r\n\"" +
				"13\",\"0x0000E59C00021AE80002\",\"0x000D0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"119\",\"Test: 119\"\r\n\"" +
				"14\",\"0x0000E59C00021AE80002\",\"0x000E0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"118\",\"Test: 118\"\r\n\"" +
				"15\",\"0x0000E59C00021AE80002\",\"0x000F0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"117\",\"Test: 117\"\r\n\"" +
				"16\",\"0x0000E59C00021AE80002\",\"0x00100000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"116\",\"Test: 116\"\r\n\"" +
				"17\",\"0x0000E59C00021AE80002\",\"0x00110000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"115\",\"Test: 115\"\r\n\"" +
				"18\",\"0x0000E59C00021AE80002\",\"0x00120000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"114\",\"Test: 114\"\r\n\"" +
				"19\",\"0x0000E59C00021AE80002\",\"0x00130000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"113\",\"Test: 113\"\r\n\"" +
				"20\",\"0x0000E59C00021AE80002\",\"0x00140000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"112\",\"Test: 112\"\r\n\"" +
				"21\",\"0x0000E59C00021AE80002\",\"0x00150000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"111\",\"Test: 111\"\r\n\"" +
				"22\",\"0x0000E59C00021AE80002\",\"0x00160000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"110\",\"Test: 110\"\r\n\"" +
				"23\",\"0x0000E59C00021AE80002\",\"0x00170000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"109\",\"Test: 109\"\r\n\"" +
				"24\",\"0x0000E59C00021AE80002\",\"0x00180000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"108\",\"Test: 108\"\r\n\"" +
				"25\",\"0x0000E59C00021AE80002\",\"0x00190000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"107\",\"Test: 107\"\r\n\"" +
				"26\",\"0x0000E59C00021AE80002\",\"0x001A0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"106\",\"Test: 106\"\r\n\"" +
				"27\",\"0x0000E59C00021AE80002\",\"0x001B0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"105\",\"Test: 105\"\r\n\"" +
				"28\",\"0x0000E59C00021AE80002\",\"0x001C0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"104\",\"Test: 104\"\r\n\"" +
				"29\",\"0x0000E59C00021AE80002\",\"0x001D0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"103\",\"Test: 103\"\r\n\"" +
				"30\",\"0x0000E59C00021AE80002\",\"0x001E0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"102\",\"Test: 102\"\r\n\"" +
				"31\",\"0x0000E59C00021AE80002\",\"0x001F0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"101\",\"Test: 101\"\r\n\"" +
				"32\",\"0x0000E59C00021AE80002\",\"0x00200000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"100\",\"Test: 100\"\r\n\"" +
				"33\",\"0x0000E59C00021AE80002\",\"0x00210000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"99\",\"Test: 99\"\r\n\"" +
				"34\",\"0x0000E59C00021AE80002\",\"0x00220000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"98\",\"Test: 98\"\r\n\"" +
				"35\",\"0x0000E59C00021AE80002\",\"0x00230000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"97\",\"Test: 97\"\r\n\"" +
				"36\",\"0x0000E59C00021AE80002\",\"0x00240000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"96\",\"Test: 96\"\r\n\"" +
				"37\",\"0x0000E59C00021AE80002\",\"0x00250000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"95\",\"Test: 95\"\r\n\"" +
				"38\",\"0x0000E59C00021AE80002\",\"0x00260000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"94\",\"Test: 94\"\r\n\"" +
				"39\",\"0x0000E59C00021AE80002\",\"0x00270000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"93\",\"Test: 93\"\r\n\"" +
				"40\",\"0x0000E59C00021AE80002\",\"0x00280000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"92\",\"Test: 92\"\r\n";
			public static string csvExpected6 = "__$row_number,__$start_lsn,__$seqval,__$operation,__$update_mask,__$lsn_period,__$command_id,BB_Code,BB_Description\r\n\"" +
				"81\",\"0x0000E59C00021AE80002\",\"0x00510000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"51\",\"Test: 51\"\r\n\"" +
				"82\",\"0x0000E59C00021AE80002\",\"0x00520000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"50\",\"Test: 50\"\r\n\"" +
				"83\",\"0x0000E59C00021AE80002\",\"0x00530000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"49\",\"Test: 49\"\r\n\"" +
				"84\",\"0x0000E59C00021AE80002\",\"0x00540000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"48\",\"Test: 48\"\r\n\"" +
				"85\",\"0x0000E59C00021AE80002\",\"0x00550000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"47\",\"Test: 47\"\r\n\"" +
				"86\",\"0x0000E59C00021AE80002\",\"0x00560000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"46\",\"Test: 46\"\r\n\"" +
				"87\",\"0x0000E59C00021AE80002\",\"0x00570000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"45\",\"Test: 45\"\r\n\"" +
				"88\",\"0x0000E59C00021AE80002\",\"0x00580000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"44\",\"Test: 44\"\r\n\"" +
				"89\",\"0x0000E59C00021AE80002\",\"0x00590000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"43\",\"Test: 43\"\r\n\"" +
				"90\",\"0x0000E59C00021AE80002\",\"0x005A0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"42\",\"Test: 42\"\r\n\"" +
				"91\",\"0x0000E59C00021AE80002\",\"0x005B0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"41\",\"Test: 41\"\r\n\"" +
				"92\",\"0x0000E59C00021AE80002\",\"0x005C0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"40\",\"Test: 40\"\r\n\"" +
				"93\",\"0x0000E59C00021AE80002\",\"0x005D0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"39\",\"Test: 39\"\r\n\"" +
				"94\",\"0x0000E59C00021AE80002\",\"0x005E0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"38\",\"Test: 38\"\r\n\"" +
				"95\",\"0x0000E59C00021AE80002\",\"0x005F0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"37\",\"Test: 37\"\r\n\"" +
				"96\",\"0x0000E59C00021AE80002\",\"0x00600000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"36\",\"Test: 36\"\r\n\"" +
				"97\",\"0x0000E59C00021AE80002\",\"0x00610000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"35\",\"Test: 35\"\r\n\"" +
				"98\",\"0x0000E59C00021AE80002\",\"0x00620000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"34\",\"Test: 34\"\r\n\"" +
				"99\",\"0x0000E59C00021AE80002\",\"0x00630000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"33\",\"Test: 33\"\r\n\"" +
				"100\",\"0x0000E59C00021AE80002\",\"0x00640000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"32\",\"Test: 32\"\r\n\"" +
				"101\",\"0x0000E59C00021AE80002\",\"0x00650000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"31\",\"Test: 31\"\r\n\"" +
				"102\",\"0x0000E59C00021AE80002\",\"0x00660000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"30\",\"Test: 30\"\r\n\"" +
				"103\",\"0x0000E59C00021AE80002\",\"0x00670000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"29\",\"Test: 29\"\r\n\"" +
				"104\",\"0x0000E59C00021AE80002\",\"0x00680000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"28\",\"Test: 28\"\r\n\"" +
				"105\",\"0x0000E59C00021AE80002\",\"0x00690000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"27\",\"Test: 27\"\r\n\"" +
				"106\",\"0x0000E59C00021AE80002\",\"0x006A0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"26\",\"Test: 26\"\r\n\"" +
				"107\",\"0x0000E59C00021AE80002\",\"0x006B0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"25\",\"Test: 25\"\r\n\"" +
				"108\",\"0x0000E59C00021AE80002\",\"0x006C0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"24\",\"Test: 24\"\r\n\"" +
				"109\",\"0x0000E59C00021AE80002\",\"0x006D0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"23\",\"Test: 23\"\r\n\"" +
				"110\",\"0x0000E59C00021AE80002\",\"0x006E0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"22\",\"Test: 22\"\r\n\"" +
				"111\",\"0x0000E59C00021AE80002\",\"0x006F0000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"21\",\"Test: 21\"\r\n\"" +
				"112\",\"0x0000E59C00021AE80002\",\"0x00700000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"20\",\"Test: 20\"\r\n\"" +
				"113\",\"0x0000E59C00021AE80002\",\"0x00710000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"19\",\"Test: 19\"\r\n\"" +
				"114\",\"0x0000E59C00021AE80002\",\"0x00720000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"18\",\"Test: 18\"\r\n\"" +
				"115\",\"0x0000E59C00021AE80002\",\"0x00730000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"17\",\"Test: 17\"\r\n\"" +
				"116\",\"0x0000E59C00021AE80002\",\"0x00740000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"16\",\"Test: 16\"\r\n\"" +
				"117\",\"0x0000E59C00021AE80002\",\"0x00750000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"15\",\"Test: 15\"\r\n\"" +
				"118\",\"0x0000E59C00021AE80002\",\"0x00760000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"14\",\"Test: 14\"\r\n\"" +
				"119\",\"0x0000E59C00021AE80002\",\"0x00770000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"13\",\"Test: 13\"\r\n\"" +
				"120\",\"0x0000E59C00021AE80002\",\"0x00780000000000000000\",\"2\",\"0x00\",\"1909\",\"1\",\"12\",\"Test: 12\"\r\n";
		}
	}

	class DummyController : ApiController
	{
		public DummyController(IIdentity identity)
		{
			User = new GenericPrincipal(identity, null);
		}
	}

	class BiControllerHelper
	{
		public BusinessObjectFactory Factory
		{
			get
			{
				return factory = factory ?? new BusinessObjectFactory();
			}
		}
		BusinessObjectFactory factory;

		public GlbBranch CreateANewBranch(string companyCode, string branchCode)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				branch.Company.GC_Code = companyCode;
				branch.GB_Code = branchCode;
				Factory.Save();
				return branch;
			}
		}

		public GlbBranch CreateANewBranch(GlbCompany company)
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			Factory.Save();
			return branch;
		}

		public GlbStaff CreateNewStaff(string loginName, GlbBranch homeBranch)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = loginName;
			staff.GS_GB_HomeBranch = homeBranch.PK;
			Factory.Save();
			return staff;
		}

		public GlbCompany CreateANewCompany()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();
			return company;
		}

		public GlbDepartment CreateANewDepartment()
		{
			using (Db.DisposableActionForDbConnection())
			{
				var department = Factory.NewWithValidTestData<GlbDepartment>();
				Factory.Save();
				return department;
			}
		}

		public BiController GenerateBiControllerWithContext(string newBranchName)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var branch1 = CreateANewBranch(newBranchName, newBranchName);
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_GB_HomeBranch = branch1.PK;
				Factory.Save();

				var cancellationToken = new CancellationToken(false);
				var glowAuthenticationTicketIdentity = GlowTicketTestHelper.CreateStaffIdentity(staff, branch1.PK.ToGuid(), Env.CurrentDepartmentPK);

				var httpActionContext = new HttpActionContext()
				{
					ControllerContext = new HttpControllerContext()
					{
						Controller = new DummyController(glowAuthenticationTicketIdentity),
						Configuration = new HttpConfiguration()
					}
				};

				var bc = new BiController
				{
					ControllerContext = httpActionContext.ControllerContext,
					Request = new HttpRequestMessage()
					{
						RequestUri = new Uri("https://myReportServer.com/PBIRS")
					}
				};

				return bc;
			}
		}

		public BiReportCredential GetBiReportImpersonatedUserCredential()
		{
			var domain = "SAND";
			var userName = "WCATest_Admin";
			var password = "p@ssw0rd";
			return new BiReportCredential() { Domain = domain, UserName = userName, Password = password };
		}
	}

	class BIControllerForTest : BiController
	{
		protected override IBiReportsService ReportService { get; }

		public BIControllerForTest(IBiReportsService service)
		{
			ReportService = service;
		}

		public PowerBiReport[] GetExpectedWorkFlowResult()
		{
			return new[]
			{
				new PowerBiReport
				{
					ReportName = "Staff Statistics",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/System/Workflow/Staff Statistics?rs:Embed=true",
					BusinessArea = "Workflow",
					IsSystemLevel = true,
					UrlEncodedReportName = "Staff+Statistics"
				},
				new PowerBiReport
				{
					ReportName = "Task Estimate Penetration",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/System/Workflow/Task Estimate Penetration?rs:Embed=true",
					BusinessArea = "Workflow",
					IsSystemLevel = true,
					UrlEncodedReportName = "Task+Estimate+Penetration"
				},
				new PowerBiReport
				{
					ReportName = "Team Throughput And Quality Measures",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/System/Workflow/Team Throughput And Quality Measures?rs:Embed=true",
					BusinessArea = "Workflow",
					IsSystemLevel = true,
					UrlEncodedReportName = "Team+Throughput+And+Quality+Measures"
				},
				new PowerBiReport
				{
					ReportName = "Glow Test Non Standard",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/System/Workflow/Glow Test Non Standard?rs:Embed=true",
					BusinessArea = "Workflow",
					IsSystemLevel = true,
					UrlEncodedReportName = "Glow+Test+Non+Standard"
				}
			};
		}

		public PowerBiReport[] GetExpectedTelematicsResult()
		{
			return new[]
			{
				new PowerBiReport
				{
					ReportName = "Device Utilization Report",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/System/Telematics/Device Utilization Report?rs:Embed=true",
					BusinessArea = "Telematics",
					IsSystemLevel = true,
					UrlEncodedReportName = "Device+Utilization+Report"
				}
			};
		}

		public PowerBiReport[] GetExpectedLogisticsResult()
		{
			return new[]
			{
				new PowerBiReport
				{
					ReportName = "Agent Shipments Report",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/EDI/Logistics/Agent Shipments Report?rs:Embed=true",
					BusinessArea = "Logistics",
					UrlEncodedReportName = "Agent+Shipments+Report"
				},
				new PowerBiReport
				{
					ReportName = "Carrier Shipments Report",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/EDI/Logistics/Carrier Shipments Report?rs:Embed=true",
					BusinessArea = "Logistics",
					UrlEncodedReportName = "Carrier+Shipments+Report"
				},
				new PowerBiReport
				{
					ReportName = "Carrier Transit Reliability",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/EDI/Logistics/Carrier Transit Reliability?rs:Embed=true",
					BusinessArea = "Logistics",
					UrlEncodedReportName = "Carrier+Transit+Reliability"
				},
				new PowerBiReport
				{
					ReportName = "Consolidation Late Report",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/EDI/Logistics/Consolidation Late Report?rs:Embed=true",
					BusinessArea = "Logistics",
					UrlEncodedReportName = "Consolidation+Late+Report"
				},
				new PowerBiReport
				{
					ReportName = "Container Shipments Report",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/EDI/Logistics/Container Shipments Report?rs:Embed=true",
					BusinessArea = "Logistics",
					UrlEncodedReportName = "Container+Shipments+Report"
				},
				new PowerBiReport
				{
					ReportName = "Export Broker Shipments Report",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/EDI/Logistics/Export Broker Shipments Report?rs:Embed=true",
					BusinessArea = "Logistics",
					UrlEncodedReportName = "Export+Broker+Shipments+Report"
				},
				new PowerBiReport
				{
					ReportName = "Import Broker Shipments Report",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/EDI/Logistics/Import Broker Shipments Report?rs:Embed=true",
					BusinessArea = "Logistics",
					UrlEncodedReportName = "Import+Broker+Shipments+Report"
				},
				new PowerBiReport
				{
					ReportName = "Job Declarations Report",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/EDI/Logistics/Job Declarations Report?rs:Embed=true",
					BusinessArea = "Logistics",
					UrlEncodedReportName = "Job+Declarations+Report"
				},
				new PowerBiReport
				{
					ReportName = "Job Profit Dashboard",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/EDI/Logistics/Job Profit Dashboard?rs:Embed=true",
					BusinessArea = "Logistics",
					UrlEncodedReportName = "Job+Profit+Dashboard"
				},
				new PowerBiReport
				{
					ReportName = "Job Profit Summary",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/EDI/Logistics/Job Profit Summary?rs:Embed=true",
					BusinessArea = "Logistics",
					UrlEncodedReportName = "Job+Profit+Summary"
				},
				new PowerBiReport
				{
					ReportName = "Job Profit Details",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/EDI/Logistics/Job Profit Details?rs:Embed=true",
					BusinessArea = "Logistics",
					UrlEncodedReportName = "Job+Profit+Details"
				},
				new PowerBiReport
				{
					ReportName = "Main Consol Shipments Report",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/EDI/Logistics/Main Consol Shipments Report?rs:Embed=true",
					BusinessArea = "Logistics",
					UrlEncodedReportName = "Main+Consol+Shipments+Report"
				},
				new PowerBiReport
				{
					ReportName = "Revenue Job Profit Growth",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/EDI/Logistics/Revenue Job Profit Growth?rs:Embed=true",
					BusinessArea = "Logistics",
					UrlEncodedReportName = "Revenue+Job+Profit+Growth"
				},
				new PowerBiReport
				{
					ReportName = "Sailing Shipments Late Report",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/EDI/Logistics/Sailing Shipments Late Report?rs:Embed=true",
					BusinessArea = "Logistics",
					UrlEncodedReportName = "Sailing+Shipments+Late+Report"
				},
				new PowerBiReport
				{
					ReportName = "Sea Cargo Consignor Consignee Report",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/EDI/Logistics/Sea Cargo Consignor Consignee Report?rs:Embed=true",
					BusinessArea = "Logistics",
					UrlEncodedReportName = "Sea+Cargo+Consignor+Consignee+Report"
				},
				new PowerBiReport
				{
					ReportName = "Shipment Dashboard",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/EDI/Logistics/Shipment Dashboard?rs:Embed=true",
					BusinessArea = "Logistics",
					UrlEncodedReportName = "Shipment+Dashboard"
				},
				new PowerBiReport
				{
					ReportName = "Shipment Dashboard New",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/EDI/Logistics/Shipment Dashboard New?rs:Embed=true",
					BusinessArea = "Logistics",
					UrlEncodedReportName = "Shipment+Dashboard+New"
				},
				new PowerBiReport
				{
					ReportName = "Shipment Profile Report",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/EDI/Logistics/Shipment Profile Report?rs:Embed=true",
					BusinessArea = "Logistics",
					UrlEncodedReportName = "Shipment+Profile+Report"
				},
				new PowerBiReport
				{
					ReportName = "Shipping Line Report",
					ReportPath = "https://test/someService/cw1api/powerbi/loadReport/pbirs/powerbi/EDI/DAT/PowerBI/EDI/Logistics/Shipping Line Report?rs:Embed=true",
					BusinessArea = "Logistics",
					UrlEncodedReportName = "Shipping+Line+Report"
				}
			};
		}
	}
}
