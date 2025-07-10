using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(SendProductInformationRequestForm))]
	sealed class SendProductInformationRequestFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var productInformation = new ProductInformation(Factory, Declaration, InvoiceLines);
			return new SendProductInformationRequestForm(productInformation);
		}

		public void TestFormHeading()
		{
			using (var testForm = (SendProductInformationRequestForm)GetFormToBashCore())
			{
				AssertEquals("Send Product Information Request", testForm.FormHeading);
			}
		}

		public void TestDelivery()
		{
			SetupConfig();
			var sentMessages = new List<string>();

			using (eHubMessagingRegistry.Instance.eHubSendInterchangesToTestGateway.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var testForm = new SendProductInformationRequestFormForTest(ProductInformation))
			{
				var handler = new MockHttpMessageHandler((HttpRequestMessage request, CancellationToken cancellationToken) =>
				{
					sentMessages.Add(request.Content.ToString());
					return null;
				});

				using (var mockHttpClient = new HttpClient(handler))
				{
					testForm.SenderExposed.MockedHttpClient = mockHttpClient;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ProductInformation.SendFrom = "123";
					testForm.DeliverButton_Click(null, null);
					AssertNotContains("Product Information Delivered Successfully", UnitTestUserNotification.Instance.LastMessage.Text);

					sentMessages.Clear();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ProductInformation.SendFrom = "sender@email.address";
					testForm.DeliverButton_Click(null, null);
					AssertContains("Product Information Delivered Successfully", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(testForm.DialogResult, DialogResult.OK);
					Assert(!testForm.Visible);
					Assert(sentMessages.Count == 2);
					AssertEquals("Using config AudienceID", ProdAudienceId, ((ClassificationAssistantTokenServiceForTest)testForm.SenderExposed.TokenService).GetAudForTest());
				}

				sentMessages.Clear();
				handler = new MockHttpMessageHandler((HttpRequestMessage request, CancellationToken cancellationToken) =>
				{
					sentMessages.Add(request.Content.ToString());
					HttpResponseMessage response = null;
					if (sentMessages.Count > 1)
					{
						response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };
					}
					return response;
				});

				using (var mockHttpClient = new HttpClient(handler))
				{
					testForm.SenderExposed.MockedHttpClient = mockHttpClient;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testForm.DeliverButton_Click(null, null);
					AssertEquals("Message send failed", @"Below message(s) send failed:
[Test Supplier 2]: Not Found (NotFound)", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

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

		const string ProdUrl = "https://classificationassistant.cargowise.com/v1/integration/cwnext-declaration";
		const string TestUrl = "https://classificationassistant-test.cargowise.com/v1/integration/cwnext-declaration";
		const string ProdAudienceId = "997755";
		const string TestAudienceId = "112233";

		void SetupConfig()
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

		class SendProductInformationRequestFormForTest : SendProductInformationRequestForm
		{
			public SendProductInformationRequestFormForTest(ProductInformation information)
				: base(information)
			{
			}

			public ProductInformationSenderForTest SenderExposed => (ProductInformationSenderForTest)Sender;

			protected override ProductInformationSender Sender => sender ??= new ProductInformationSenderForTest(ProductInformation);
			ProductInformationSenderForTest sender;
		}
	}
}
