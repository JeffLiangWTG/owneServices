using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static NUnit.Framework.XmlAssertions;

namespace Enterprise.Services.ServiceHost.Tests
{
	abstract class BaseEAdaptorControllerTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			ExceptionReporter.Instance.TestingDoReportException.Value = true;
			Globals.IsUserInteractive = false;
			Globals.SetIsUnitTestingProductionFunctionality(true);
			Globals.IsWeb = true;
			base.SetUp();
		}

		protected override void TearDown()
		{
			Globals.IsUserInteractive = true;
			Globals.SetIsUnitTestingProductionFunctionality(false);
			Globals.IsWeb = false;
			base.TearDown();
		}

		#region Assertions

		protected HttpStatusCode AssertExceptionErrorMessage(Exception ex, string expectedMessage, eAdaptorSenderHelper sender)
		{
			eAdaptorHandlerFactory.SetGetHandlerHook((ref IHttpXmlMessageHandler h) => throw ex);
			var response = sender.SendRequest("<UniversalEvent></UniversalEvent>");
			AssertContains(expectedMessage, response.result);
			return response.statusCode;
		}

		protected void AssertEDIMessageCount(int expectedCount, string transmitDirection = null)
		{
			ZQuery query = new ZQuery();
			if (transmitDirection != null)
			{
				query = new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, transmitDirection);
			}

			var factory = new BusinessObjectFactory();
			var messages = factory.Load<IEDIMessage>(query);
			AssertEquals($"Expecting {expectedCount} request message", expectedCount, messages.Length);
		}

		protected void AssertEDIMessageStatus(string expectedMessageStatus, string transmitDirection = ReceiveTransmitList.Codes.Receive)
		{
			var factory = new BusinessObjectFactory();
			var messages = factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, transmitDirection));
			AssertEquals("Expecting 1 request message", 1, messages.Length);

			var message = messages[0];
			AssertEquals(expectedMessageStatus, message.EM_Status);
		}

		protected void AssertResponseContains(string result, string type, string value)
		{
			AssertContains($@"<Type>{type}</Type>
        <Value>{value}</Value>", result);
		}

		protected void AssertMessageNumber(string result, string messageNum)
		{
			AssertContains($"<MessageNumber Type=\"MessageNumber\">{messageNum}</MessageNumber>", result);
		}

		protected void AssertExternalReferenceNumber(string result, string externalNum)
		{
			AssertIsXml(result)
			.HavingExactlyOneChildNode("MessageNumberCollection/MessageNumber",
				node => node.WithValue(externalNum).WithAttribute(a =>
					a.WithName("Type")
					 .WithValue(v => v == "External")
				)
			);
		}

		protected void AssertProcessingLogs(string result, string processingLogs)
		{
			AssertContains($"<ProcessingLog>{processingLogs}</ProcessingLog>", result);
		}

		protected void AssertResponseHeaderContains(HttpHeaders headers, string key, string value)
		{
			Assert($"Expect containing the header {key}", headers.TryGetValues(key, out var headerValue));
			if (headerValue != null)
			{
				AssertEquals($"Expect the value of the header {key}", value, headerValue.FirstOrDefault());
			}
		}

		#endregion

		#region Helpers

		protected void CreateTestPeriod()
		{
			var period = Factory.New<AccPeriodManagement>();
			period.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			period.AM_Year = 2021;
			period.AM_Period = 202103;
			period.AM_StartDate = new ZDateTime(2021, 03, 01);
			period.AM_EndDate = new ZDateTime(2021, 03, 31);
		}

		protected void CreateAUDBankAccount()
		{
			var fAUDBankAccount = Factory.LoadTop1<AccBankAccount>(new ZQuery(AccBankAccountSchema.AB_Code, "ZHSBCAUD"));
			if (fAUDBankAccount == null)
			{
				var header = Factory.NewWithValidTestData<AccGLHeader>();
				header.AG_AccountNum = "ZAUDAcc";
				fAUDBankAccount = Factory.New<AccBankAccount>();
				fAUDBankAccount.AB_Code = "ZHSBCAUD";
				fAUDBankAccount.AB_GB = GlbBranch.CurrentBranch.PK;
				fAUDBankAccount.AB_Desc = "HSBC AUD ACCT";
				fAUDBankAccount.AB_AG = header.PK;
				fAUDBankAccount.AB_BankName = "HSBC";
				fAUDBankAccount.AB_BankAbbreviation = "AUD";
				fAUDBankAccount.AB_BSB = "123456";
				fAUDBankAccount.AB_AccountNum = "12345678";
				fAUDBankAccount.AB_RX_NKAccountCurrency = "AUD";
			}
		}

		#endregion
	}
}
