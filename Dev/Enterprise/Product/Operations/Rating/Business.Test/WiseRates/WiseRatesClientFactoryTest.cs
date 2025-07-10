using System;
using System.Text;
using System.Threading;
using AuthenticationService.Client.Models;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Rating;
using Enterprise.Rating.Business.WiseRates;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Rating.Business.Testing
{
	public class WiseRatesClientFactoryTest : TestCaseWithFactory
	{
		public void TestTryCreate_UrsEnabled_CreateClientForUrs()
		{
			var authTokenProviderMock = new Mock<IAuthTokenProvider>();
			authTokenProviderMock
				.Setup(s => s.GetToken(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Action<LoginInfo>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
				.Returns(("McLaren", ""));

			var wrFactory = new WiseRatesClientFactory(authTokenProviderMock.Object);

			using (RatingDataRegistry.Instance.UseUrsForLegacyRateSelectorAndMMS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (RatingDataRegistry.Instance.EnableUrsIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var (client, reason) = wrFactory.TryCreate("correlationId", 0, CancellationToken.None);
				var ratesServiceClient = client as RatesServiceClient;
				AssertNullOrEmpty(reason);
				AssertEquals("Send requests to URS when URS is disabled", false, ratesServiceClient.IsForUrs);
				AssertEquals("Send requests to rates service when URS is disabled", true, ratesServiceClient.IsForRatesService);
			}

			using (RatingDataRegistry.Instance.UseUrsForLegacyRateSelectorAndMMS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (RatingDataRegistry.Instance.EnableUrsIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (client, reason) = wrFactory.TryCreate("correlationId", 0, CancellationToken.None);
				var ratesServiceClient = client as RatesServiceClient;
				AssertNullOrEmpty(reason);
				AssertEquals("Send requests to URS when URS is enabled (should send since we are going to send requests to both services to make sure they return the same rates)", true, ratesServiceClient.IsForUrs);
				AssertEquals("Send requests to rates service when URS is enabled (should send since we are going to send requests to both services to make sure they return the same rates)", true, ratesServiceClient.IsForRatesService);
			}
		}

		public void TestTryCreate_RatesServiceRegistrySetting()
		{
			var authTokenProviderMock = new Mock<IAuthTokenProvider>();
			authTokenProviderMock
				.Setup(s => s.GetToken(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Action<LoginInfo>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
				.Returns(("McLaren", ""))
				.Verifiable();

			var wrFactory = new WiseRatesClientFactory(authTokenProviderMock.Object);

			const string expectedWarningText = "Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription";

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				var (client, reason) = wrFactory.TryCreate("correlationId", 0, CancellationToken.None);
				AssertEquals(expectedWarningText, reason);
				AssertNull(client);
			}

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var (client, reason) = wrFactory.TryCreate("correlationId", 0, CancellationToken.None);
				AssertNullOrEmpty(reason);
				AssertNotNull(client);
			}

			authTokenProviderMock.VerifyAll();
		}

		public void TestTryCreate_ODPLInfoEnabled()
		{
			var authTokenProviderMock = new Mock<IAuthTokenProvider>();
			authTokenProviderMock
				.Setup(s => s.GetToken(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Action<LoginInfo>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
				.Returns(("McLaren", ""))
				.Verifiable();

			var wrFactory = new WiseRatesClientFactory(authTokenProviderMock.Object);

			const string expectedWarningText = "Rates Service is only supported under license of STL. Please submit eRequest for switching license to STL or for a trial of Rates Service.";

			var billingModel = Enterprise.ProductRegistration.Client.ProductRegister.TestHelper.KeyForTest.BillingModelForTest;

			try
			{
				Globals.IsTest_ForTest.Value = true;
				Enterprise.ProductRegistration.Client.ProductRegister.TestHelper.KeyForTest.BillingModelForTest = "ODM";

				using (DataRegistryRating.Instance.DiagnosticSettingsEnableOnODPL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var (token, reason) = wrFactory.CheckAccess("correlationId", 0);
					AssertNullOrEmpty(reason);
					AssertNotNullOrEmpty(token);
				}

				using (DataRegistryRating.Instance.DiagnosticSettingsEnableOnODPL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var (token, reason) = wrFactory.CheckAccess("correlationId", 0);
					AssertEquals(expectedWarningText, reason);
					AssertNullOrEmpty(token);
				}
			}
			finally
			{
				Enterprise.ProductRegistration.Client.ProductRegister.TestHelper.KeyForTest.BillingModelForTest = billingModel;
				Globals.IsTest_ForTest.Value = false;
			}

			authTokenProviderMock.VerifyAll();
		}

		public void TestTryCreate_RatesServiceURLIsSpecified_CreateClient()
		{
			using (RatingDataRegistry.Instance.RatesServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://localhost:7200"))
			{
				var correlationID = "12345";
				var seconds = 30;
				var cts = new CancellationTokenSource(seconds);

				var providerMock = new Mock<IAuthTokenProvider>();
				providerMock
					.Setup(s => s.GetToken(correlationID, seconds, It.IsAny<Action<LoginInfo>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
					.Returns(("fake token", string.Empty))
					.Verifiable();

				var factory = new WiseRatesClientFactory(providerMock.Object);
				var (client, failureMessage) = factory.TryCreate(correlationID, seconds, cts.Token);

				AssertNotNull(client);
				AssertEquals(string.Empty, failureMessage);

				providerMock.VerifyAll();
			}
		}

		public void TestTryCreate_TokenInvalid_ReturnNull()
		{
			using (RatingDataRegistry.Instance.RatesServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://localhost:7200"))
			{
				var correlationID = "12345";
				var seconds = 30;
				var cts = new CancellationTokenSource(seconds);

				var providerMock = new Mock<IAuthTokenProvider>();
				providerMock
					.Setup(s => s.GetToken(correlationID, seconds, It.IsAny<Action<LoginInfo>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
					.Returns((string.Empty, "Email required"))
					.Verifiable();

				var factory = new WiseRatesClientFactory(providerMock.Object);
				var (client, failureMessage) = factory.TryCreate(correlationID, seconds, cts.Token);

				AssertNull(client);
				AssertEquals("Email required", failureMessage);

				providerMock.VerifyAll();
			}
		}

		public void TestTryCreate_RatesServiceURLIsNotSpecified_ReturnNull()
		{
			using (RatingDataRegistry.Instance.RatesServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			{
				var correlationID = "12345";
				var seconds = 30;
				var cts = new CancellationTokenSource(seconds);

				var providerMock = new Mock<IAuthTokenProvider>();
				providerMock
					.Setup(s => s.GetToken(correlationID, seconds, It.IsAny<Action<LoginInfo>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
					.Returns(("fake token", string.Empty))
					.Verifiable();

				var factory = new WiseRatesClientFactory(providerMock.Object);
				var (client, failureMessage) = factory.TryCreate(correlationID, seconds, cts.Token);

				AssertNull(client);
				AssertEquals("The Rates Service URL is not configured", failureMessage);

				providerMock.VerifyAll();
			}
		}

		public void TestTryCreate_RatesServiceURLIsInvalid_ReturnNull()
		{
			byte[] urlValue = Encoding.Unicode.GetBytes("http:/rates.wisegrid.net");
			var regSql = @"
UPDATE dbo.StmData 
	SET SD_Type = @SD_Type, SD_BinaryValue = @SD_BinaryValue, SD_GuidValue = @SD_GuidValue, SD_IsLogged = @SD_IsLogged
	WHERE SD_Name = @SD_Name
	AND SD_Owner is null
	AND SD_DepartmentGuid is null
IF (@@rowcount = 0)
BEGIN
	INSERT dbo.StmData (SD_PK, SD_Name, SD_Type, SD_BinaryValue, SD_GuidValue, SD_IsLogged) 
		VALUES (NEWID(), @SD_Name, @SD_Type, @SD_BinaryValue, @SD_GuidValue, @SD_IsLogged)
END
";
			using (var command = TestConnection.Command(regSql))
			{
				command.AddParameterBasedOnDbColumn("@SD_Name", "WiseRatesServiceUrl", StmDataSchema.SD_Name);
				command.AddParameterBasedOnDbColumn("@SD_Type", "STR", StmDataSchema.SD_Type);
				command.AddParameterBasedOnDbColumn("@SD_GuidValue", DBNull.Value, StmDataSchema.SD_GuidValue);
				command.AddParameterBasedOnDbColumn("@SD_IsLogged", false, StmDataSchema.SD_IsLogged);
				command.AddParameterBasedOnDbColumn("@SD_BinaryValue", urlValue, StmDataSchema.SD_BinaryValue);
				command.ExecuteNonQuery();
			}

			var correlationID = "12345";
			var seconds = 30;
			var cts = new CancellationTokenSource(seconds);

			var providerMock = new Mock<IAuthTokenProvider>();
			providerMock
				.Setup(s => s.GetToken(correlationID, seconds, It.IsAny<Action<LoginInfo>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
				.Returns(("fake token", string.Empty))
				.Verifiable();

			var factory = new WiseRatesClientFactory(providerMock.Object);
			var (client, failureMessage) = factory.TryCreate(correlationID, seconds, cts.Token);

			AssertNull(client);
			AssertEquals("The Rates Service URL in the registry (http:/rates.wisegrid.net) is invalid.", failureMessage);

			providerMock.VerifyAll();
		}

		public void TestConstructor_MustUseAuthTokenProviderForRating()
		{
			var instance = (WiseRatesClientFactory)ObjectFactory.Get<IWiseRatesClientFactory>();
			AssertType(typeof(WTGAuthTokenProviderForRating), instance.AuthTokenProvider);

			instance = new WiseRatesClientFactory();
			AssertType(typeof(WTGAuthTokenProviderForRating), instance.AuthTokenProvider);
		}

		protected override void SetUp()
		{
			Globals.IsTest_ForTest.Value = false;
		}

		protected override void TearDown()
		{
			Globals.IsTest_ForTest.Value = true;
		}
	}
}
