using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ProductInformationSender))]
	sealed class ProductInformationSenderTest : TestCaseWithFactory
	{
		public void TestSend()
		{
			SetupConfig();

			using (eHubMessagingRegistry.Instance.eHubSendInterchangesToTestGateway.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var sentMessages = new List<string>();
				var sender = new ProductInformationSenderForTest(ProductInformation);

				var handler = new MockHttpMessageHandler((HttpRequestMessage request, CancellationToken cancellationToken) =>
				{
					sentMessages.Add(request.Content.ReadAsStringAsync().Result);
					return null;
				});

				using (var mockHttpClient = new HttpClient(handler))
				{
					sender.MockedHttpClient = mockHttpClient;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ProductInformation.SendFrom = "123";
					var result = sender.TrySend();
					AssertType<SendResult.Failure>(result);
					AssertNotNullOrEmpty("When ProductInformation has error", ((SendResult.Failure)result).ErrorMessage);

					sentMessages.Clear();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ProductInformation.SendFrom = "sender@email.address";
					AssertType<SendResult.Success>(sender.TrySend());
					Assert(sentMessages.Count == 2);
					AssertEquals("Using config AudienceID", ProdAudienceId, sender.TokenService.aud);

					AssertEquals(message1, sentMessages[0]);
					AssertEquals(message2, sentMessages[1]);
				}

				sentMessages.Clear();
				handler = new MockHttpMessageHandler((HttpRequestMessage request, CancellationToken cancellationToken) =>
				{
					sentMessages.Add(request.Content.ReadAsStringAsync().Result);
					HttpResponseMessage response = null;
					if (sentMessages.Count > 1)
					{
						response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };
					}
					return response;
				});

				using (var mockHttpClient = new HttpClient(handler))
				{
					sender.MockedHttpClient = mockHttpClient;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var result = sender.TrySend();
					AssertType<SendResult.Failure>(result);
					AssertEquals("Message send failed", @"Below message(s) send failed:
[Test Supplier 2]: Not Found (NotFound)", ((SendResult.Failure)result).ErrorMessage);
				}
			}
		}

		const string message1 = @"{""sender"":""sender@email.address"",""receivers"":[""sup1@email.address""],""integration"":{""type"":""cwnext-declaration"",""ehub_id"":""EDIEDIDAT"",""declaration_number"":""TestJE001""},""context"":{""supplier"":""Test Supplier 1"",""consignee"":""Test Importer"",""masterbill"":""M0001"",""housebill"":""H0001"",""voyage_flight"":""QF001"",""owner_ref"":""OwnerRef"",""port_of_loading"":""TPORT""},""requested_classifications"":[{""imp_exp"":""e"",""country"":""er""}],""lines"":[{""integration"":{""invoice_number"":""TESTJZ001"",""invoice_line"":""1""},""context"":{""part_no"":""Prod01""},""desc"":""Invoice Line 1 Description""}]}";
		const string message2 = @"{""sender"":""sender@email.address"",""receivers"":[""sup2@email.address""],""integration"":{""type"":""cwnext-declaration"",""ehub_id"":""EDIEDIDAT"",""declaration_number"":""TestJE001""},""context"":{""supplier"":""Test Supplier 2"",""consignee"":""Test Importer"",""masterbill"":""M0001"",""housebill"":""H0001"",""voyage_flight"":""QF001"",""owner_ref"":""OwnerRef"",""port_of_loading"":""TPORT""},""requested_classifications"":[{""imp_exp"":""e"",""country"":""er""}],""lines"":[{""integration"":{""invoice_number"":""TESTJZ002"",""invoice_line"":""1""},""context"":{""part_no"":""Prod02""},""desc"":""Invoice Line 2 Description""}]}";

		public void TestSend_NoUrl()
		{
			var sender = new ProductInformationSender(ProductInformation);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var result = sender.TrySend();
			AssertType<SendResult.Failure>(result);
			AssertEquals("The system does not have a post URL, please contact your administrator.", ((SendResult.Failure)result).ErrorMessage);
		}

		public void TestSend_NoAudienceID()
		{
			SetupConfig(false);
			var sender = new ProductInformationSender(ProductInformation);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var result = sender.TrySend();
			AssertType<SendResult.Failure>(result);
			AssertEquals("The system does not have an Audience ID, please contact your administrator.", ((SendResult.Failure)result).ErrorMessage);
		}

		public void TestSend_NoCertificate()
		{
			SetupConfig();

			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemToSystemTrustInfo()))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var sender = new ProductInformationSender(ProductInformation);
				var result = sender.TrySend();
				AssertType<SendResult.Failure>(result);
				AssertContains("The system doesn't have valid certificate.", ((SendResult.Failure)result).ErrorMessage);
				AssertContains("Please update the certificate via service task 'TCM'. If this issue persists, please contact your administrator.", ((SendResult.Failure)result).ErrorMessage);
			}

			var securityInstance = CreateSecurityInstance(Factory);
			securityInstance.MaintainConsolAllowViewTransactEBL.IsAllowed = true;

			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, MasterFiles.Business.Testing.AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo()))
			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var sender = new ProductInformationSender(ProductInformation);
				var result = sender.TrySend();
				AssertType<SendResult.Failure>(result);
				AssertNotContains("Gets a different API error which is masked by the trust service", "The system doesn't have valid certificate.", ((SendResult.Failure)result).ErrorMessage);
				AssertContains("Please update the certificate via service task 'TCM'. If this issue persists, please contact your administrator.", ((SendResult.Failure)result).ErrorMessage);
			}
		}

		public static SecurityCore CreateSecurityInstance(BusinessObjectFactory factory)
		{
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsController = false;

			var securityCollection = new GlbSecurityCollection(factory);
			securityCollection.Load();

			return new SecurityCore(securityCollection, staff, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		public void TestConfigSettings()
		{
			SetupConfig();
			var productInformation = new ProductInformation(Factory, Declaration, InvoiceLines);

			using (eHubMessagingRegistry.Instance.eHubSendInterchangesToTestGateway.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var sender = new ProductInformationSenderForTest(productInformation);
				AssertEquals("Prod URL", ProdUrl, sender.GetUrl());
				AssertEquals("Prod AudienceID", ProdAudienceId, sender.GetAudienceID());
			}

			using (eHubMessagingRegistry.Instance.eHubSendInterchangesToTestGateway.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var sender = new ProductInformationSenderForTest(productInformation);
				AssertEquals("Test URL", TestUrl, sender.GetUrl());
				AssertEquals("Test AudienceID", TestAudienceId, sender.GetAudienceID());
			}
		}

		public void TestJSONGarbage()
		{
			var context = new ProductInformationDeliveryContext { sender = "'\"&quot;:{}[]" };
			var jsonString = JsonSerializer.Serialize(context, new JsonSerializerOptions { IncludeFields = true });
			AssertContains("\"sender\":\"\\u0027\\u0022\\u0026quot;:{}[]\"", jsonString);
		}

		void SetupConfig(bool includeAudienceID = true)
		{
			CreateRefSysConfigIfNotExist(
				ProductInformationSender.RefSysConfigKey_HSAsstUrlProd,
				"HS Classification Assistant URL - Prod",
				"The URL that will be used when posting paramaters to the HS Classification Assistant. There is a Test and Prod address.",
				ProdUrl);

			CreateRefSysConfigIfNotExist(
				ProductInformationSender.RefSysConfigKey_HSAsstUrlTest,
				"HS Classification Assistant URL - Test",
				"The URL that will be used when posting paramaters to the HS Classification Assistant. There is a Test and Prod address.",
				TestUrl);

			if (includeAudienceID)
			{
				CreateRefSysConfigIfNotExist(
					ProductInformationSender.RefSysConfigKey_HSAsstAudTest,
					"Test AudienceId",
					"The Test AudienceId.",
					TestAudienceId);

				CreateRefSysConfigIfNotExist(
					ProductInformationSender.RefSysConfigKey_HSAsstAudProd,
					"Prod AudienceId",
					"The Prod AudienceId.",
					ProdAudienceId);
			}

			void CreateRefSysConfigIfNotExist(string code, string description, string longDescription, string value)
			{
				var configType = Factory.Load<RefSysConfigType>(new ZQuery(RefSysConfigTypeSchema.ZRT_ConfigCode, code)).FirstOrDefault();
				if (configType == null)
				{
					configType = Factory.New<RefSysConfigType>();
					configType.ZRT_ConfigCode = code;
					configType.ZRT_Description = description;
					configType.ZRT_LongDescription = longDescription;
				}

				var startDate = ZDateTime.UtcToday;
				RefSysConfig config = new RefSysConfig.Loader(Factory).Load(code, startDate);
				if (config == null)
				{
					config = Factory.New<RefSysConfig>();
					config.ZRC_ZRT_NKConfigCode = code;
					config.ZRC_StartDate = startDate;
				}

				config.ZRC_DecimalValue = 0;
				config.ZRC_StringValue = value;
				config.ZRC_EndDate = new ZDateTime(2079, 6, 6);
			}
		}

		const string ProdUrl = "https://classificationassistant.cargowise.com/v1/integration/cwnext-declaration";
		const string TestUrl = "https://classificationassistant-test.cargowise.com/v1/integration/cwnext-declaration";
		const string ProdAudienceId = "998877";
		const string TestAudienceId = "112233";

		BaseJobDeclaration declaration;
		BaseJobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
					supplier1.OH_Code = "SUP1";
					supplier1.OH_FullName = "Test Supplier 1";
					var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
					supplier2.OH_Code = "SUP2";
					supplier2.OH_FullName = "Test Supplier 2";

					var consignee = Factory.NewWithValidTestData<OrgHeader>();
					consignee.OH_Code = "IMP1";
					consignee.OH_FullName = "Test Importer";

					declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
					declaration.JE_DeclarationReference = "TestJE001";
					declaration.JE_OH_Importer = consignee.PK;
					declaration.JE_MasterBill = "M0001";
					declaration.JE_HouseBill = "H0001";
					declaration.JE_VoyageFlightNo = "QF001";
					declaration.JE_OwnerRef = "OwnerRef";
					declaration.JE_RL_NKPortOfLoading = "TPort";

					var invoice1 = declaration.Invoices.AddNew();
					invoice1.JZ_InvoiceNumber = "TESTJZ001";
					invoice1.JZ_OH_Supplier = supplier1.PK;
					var invoice2 = declaration.Invoices.AddNew();
					invoice2.JZ_InvoiceNumber = "TESTJZ002";
					invoice2.JZ_OH_Supplier = supplier2.PK;

					var invoiceLine1 = declaration.InvoiceLines.AddNew();
					invoiceLine1.JI_Calc_Invoice = "TESTJZ001";
					invoiceLine1.JI_PartNo = "Prod01";
					invoiceLine1.JI_Description = "Invoice Line 1 Description";
					var invoiceLine2 = declaration.InvoiceLines.AddNew();
					invoiceLine2.JI_Calc_Invoice = "TESTJZ002";
					invoiceLine2.JI_PartNo = "Prod02";
					invoiceLine2.JI_Description = "Invoice Line 2 Description";
				}
				return declaration;
			}
		}

		BaseJobComInvoiceLine[] InvoiceLines => Declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>().ToArray();

		ProductInformation productInformation;
		ProductInformation ProductInformation
		{
			get
			{
				if (productInformation == null)
				{
					productInformation = new ProductInformation(Factory, Declaration, InvoiceLines);
					productInformation.SendFrom = "sender@email.address";
					var contact1 = productInformation.DeliveryContacts.AddNew();
					contact1.Supplier = "SUP1";
					contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
					contact1.DeliveryAddress = "sup1@email.address";
					var contact2 = productInformation.DeliveryContacts.AddNew();
					contact2.Supplier = "SUP2";
					contact2.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
					contact2.DeliveryAddress = "sup2@email.address";
				}
				return productInformation;
			}
		}

		class MockHttpMessageHandler : HttpMessageHandler
		{
			public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> onSend)
			{
				this.OnSend = onSend;
			}

			Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> OnSend { get; }

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				HttpResponseMessage response = null;
				if (OnSend != null)
				{
					response = OnSend?.Invoke(request, cancellationToken);
				}
				if (response == null)
				{
					response = new HttpResponseMessage { StatusCode = HttpStatusCode.NoContent };
				}
				return Task.Run(() => response);
			}
		}
	}

	public class ProductInformationSenderForTest : ProductInformationSender
	{
		public ProductInformationSenderForTest(ProductInformation information)
			: base(information)
		{
		}

		public HttpClient MockedHttpClient { get; set; }

		protected override HttpClient GetPostWebClient(string baseAddress, string token)
		{
			return MockedHttpClient ?? base.GetPostWebClient(baseAddress, token);
		}

		public ClassificationAssistantTokenService TokenService;

		protected override ClassificationAssistantTokenService GetTokenService(string aud)
		{
			return TokenService = new ClassificationAssistantTokenServiceForTest(aud);
		}
	}

	public class ClassificationAssistantTokenServiceForTest : ClassificationAssistantTokenService
	{
		public ClassificationAssistantTokenServiceForTest(string aud)
			: base(aud) { }

		public override string GetAccessToken() => "TestToken";

		public string GetAudForTest() => aud;
	}
}
