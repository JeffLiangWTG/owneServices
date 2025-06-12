using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests.TransformHelpers
{
	[TestClass]
	public class SGCustomsDataModelAccessorTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void GetSGCustomsAccount_Success()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockContextFactory = MockRepository.GenerateMock<ContextFactory<eHubTransactionsContext>>();
			mockContextFactory.Stub(x => x.CreateContext()).Return(mockContext);

			var testClients = new TestDbSet<eHubClient>();
			mockContext.Stub(x => x.eHubClients).Return(testClients);

			var testClient = new eHubClient { CC_ID = "CLIENT001" };
			testClients.Add(testClient);

			var testClientRegistrations = new TestDbSet<eHubClientRegistration>();
			mockContext.Stub(x => x.eHubClientRegistrations).Return(testClientRegistrations);

			testClientRegistrations.Add(new eHubClientRegistration
			{
				eHubRegistrationType = new eHubRegistrationType { RT_ID = "SGCustomsAccount" },
				CX_Qualifier = "PW",
				CX_Code = "VWGT001",
				CX_CC = testClient.CC_PK
			});

			var dataModelAccessor = new SGCustomsDataModelAccessor(mockContextFactory);

			string result = dataModelAccessor.GetSGCustomsAccount("PW", "CLIENT001");

			Assert.AreEqual("VWGT001", result);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void GetSGCustomsAccount_Errors()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockContextFactory = MockRepository.GenerateMock<ContextFactory<eHubTransactionsContext>>();
			mockContextFactory.Stub(x => x.CreateContext()).Return(mockContext);

			var testClients = new TestDbSet<eHubClient>();
			mockContext.Stub(x => x.eHubClients).Return(testClients);

			var testClientRegistrations = new TestDbSet<eHubClientRegistration>();
			mockContext.Stub(x => x.eHubClientRegistrations).Return(testClientRegistrations);

			testClientRegistrations.Add(new eHubClientRegistration
			{
				eHubClient = new eHubClient { CC_ID = "CLIENT001" },
				eHubRegistrationType = new eHubRegistrationType { RT_ID = "SGCustomsAccount" },
				CX_Qualifier = "PW",
				CX_Code = "VWGT001"
			});

			var dataModelAccessor = new SGCustomsDataModelAccessor(mockContextFactory);

			try
			{
				string result = dataModelAccessor.GetSGCustomsAccount(null, "CLIENT001");
				Assert.Fail("Should have thrown ArgumentException.");
			}
			catch (ArgumentException ex)
			{
				Assert.AreEqual("brokerID", ex.ParamName);
			}

			try
			{
				string result = dataModelAccessor.GetSGCustomsAccount(" ", "CLIENT001");
				Assert.Fail("Should have thrown ArgumentException.");
			}
			catch (ArgumentException ex)
			{
				Assert.AreEqual("brokerID", ex.ParamName);
				Assert.AreEqual(true, ex.Message.StartsWith("Argument is null or empty or whitespace"));
			}

			try
			{
				string result = dataModelAccessor.GetSGCustomsAccount("PW", null);
				Assert.Fail("Should have thrown ArgumentException.");
			}
			catch (ArgumentException ex)
			{
				Assert.AreEqual("eHubClientID", ex.ParamName);
			}

			try
			{
				string result = dataModelAccessor.GetSGCustomsAccount("PW", " ");
				Assert.Fail("Should have thrown ArgumentException.");
			}
			catch (ArgumentException ex)
			{
				Assert.AreEqual("eHubClientID", ex.ParamName);
				Assert.AreEqual(true, ex.Message.StartsWith("Argument is null or empty or whitespace"));
			}

			try
			{
				string result = dataModelAccessor.GetSGCustomsAccount("PW", "EHSAN0001");
				Assert.Fail("Should have thrown ArgumentException.");
			}
			catch (ArgumentException ex)
			{
				Assert.AreEqual("eHubClientID", ex.ParamName);
				Assert.AreEqual(true, ex.Message.StartsWith("Argument is not valid, no matching CC_ID value in database"));
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void GetSGCustomsSenderID_Success()
		{
			var dataModelAccessor = new SGCustomsDataModelAccessor();
			string result = dataModelAccessor.GetSGCustomsSenderID("VWGT001");

			Assert.AreEqual("VWGT.VWGT001", result);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void GetSGCustomsSenderID_Error_Null()
		{
			var dataModelAccessor = new SGCustomsDataModelAccessor();
			try
			{
				string result = dataModelAccessor.GetSGCustomsSenderID(null);
				Assert.Fail("Should have thrown ArgumentException.");
			}
			catch (ArgumentException ex)
			{
				Assert.AreEqual("accountName", ex.ParamName);
			}

			try
			{
				string result = dataModelAccessor.GetSGCustomsSenderID(" ");
				Assert.Fail("Should have thrown ArgumentException.");
			}
			catch (ArgumentException ex)
			{
				Assert.AreEqual("accountName", ex.ParamName);
				Assert.AreEqual(true, ex.Message.StartsWith("Argument is null or empty or whitespace"));
			}

			try
			{
				string result = dataModelAccessor.GetSGCustomsSenderID("VWG");
				Assert.Fail("Should have thrown ArgumentException.");
			}
			catch (ArgumentException ex)
			{
				Assert.AreEqual("accountName", ex.ParamName);
				Assert.AreEqual(true, ex.Message.StartsWith("Argument must be at least 4 characters in length"));
			}
		}
	}
}
