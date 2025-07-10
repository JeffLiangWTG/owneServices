using System;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.MasterFiles.Business;
	using NUnit.Framework;

	public class BrokerValidationTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			new BrokerValidation(null);
		}

		public void TestValidateBrokerage()
		{
			var validationResult = brokerValidation.ValidateAgentAndBrokerDetails();
			AssertContains("Company brokerage ID not set up", BrokerValidation.MissingBrokerIdMessage, validationResult);
			AssertContains("Declarant ID not set up", BrokerValidation.MissingDeclarantIdMessage, validationResult);
			AssertContains("Declarant communications not set up", BrokerValidation.MissingDeclarantCommunicationsMessage, validationResult);

			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			validationResult = brokerValidation.ValidateAgentAndBrokerDetails();
			AssertNotContains("Company brokerage ID now set up - no error", BrokerValidation.MissingBrokerIdMessage, validationResult);
			AssertContains("Declarant ID not set up", BrokerValidation.MissingDeclarantIdMessage, validationResult);
			AssertContains("Declarant communications not set up", BrokerValidation.MissingDeclarantCommunicationsMessage, validationResult);

			declarant.GetNZWrapper().NZBPassword.GP_UserID = "40006206E";
			validationResult = brokerValidation.ValidateAgentAndBrokerDetails();
			AssertNotContains("Company brokerage ID now set up - no error", BrokerValidation.MissingBrokerIdMessage, validationResult);
			AssertNotContains("Declarant ID now set up - no error", BrokerValidation.MissingDeclarantIdMessage, validationResult);
			AssertContains("Declarant communications not set up", BrokerValidation.MissingDeclarantCommunicationsMessage, validationResult);

			declarant.GS_EmailAddress = "test.user@company.org";
			validationResult = brokerValidation.ValidateAgentAndBrokerDetails();
			AssertNotContains("Company brokerage ID now set up - no error", BrokerValidation.MissingBrokerIdMessage, validationResult);
			AssertNotContains("Declarant ID now set up - no error", BrokerValidation.MissingDeclarantIdMessage, validationResult);
			AssertNotContains("Declarant communications now set up - no error", BrokerValidation.MissingDeclarantCommunicationsMessage, validationResult);
		}

		public void TestValidateBrokerageID()
		{
			AssertContains("Company brokerage ID not set up", BrokerValidation.MissingBrokerIdMessage, brokerValidation.ValidateAgentAndBrokerDetails());

			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			AssertNotContains("Company brokerage ID now set up - no error", BrokerValidation.MissingBrokerIdMessage, brokerValidation.ValidateAgentAndBrokerDetails());
		}

		public void TestValidateDeclarantID()
		{
			AssertNotContains("Declarant ID not required - no error", BrokerValidation.MissingDeclarantIdMessage, brokerValidation.ValidateAgentAndBrokerDetails(checkDeclarantId: false));
			AssertContains("Declarant ID not set up", BrokerValidation.MissingDeclarantIdMessage, brokerValidation.ValidateAgentAndBrokerDetails());

			declarant.GetNZWrapper().NZBPassword.GP_UserID = "40006206E";
			AssertNotContains("Declarant ID now set up - no error", BrokerValidation.MissingDeclarantIdMessage, brokerValidation.ValidateAgentAndBrokerDetails());
		}

		public void TestValidateDeclarantCommunications()
		{
			AssertContains("Declarant communications not set up", BrokerValidation.MissingDeclarantCommunicationsMessage, brokerValidation.ValidateAgentAndBrokerDetails());

			declarant.GS_EmailAddress = "test.user@company.org";
			AssertNotContains("Declarant communications now set up - no error", BrokerValidation.MissingDeclarantCommunicationsMessage, brokerValidation.ValidateAgentAndBrokerDetails());
		}

		protected override void SetUp()
		{
			base.SetUp();
			declarant = Factory.NewWithValidTestData<GlbStaff>();
			brokerValidation = new BrokerValidation(new TSWGlbStaffWrapper(declarant));
		}
		GlbStaff declarant;
		BrokerValidation brokerValidation;
	}
}
