using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GatewayChargeDefaultDebtorConfiguration))]
	class GatewayChargeDefaultDebtorConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestFixIdenticalConfigurationCrossProperty()
		{
			const string configAlreadyExists = "Configuration with identical criteria already exists.";

			var configuration2 = new GatewayChargeDefaultDebtorConfiguration();
			configurations.Add(configuration2);

			configuration2.ConsolDirection = "IMP";
			configuration2.ConsolTransportMode = "SEA";
			configuration2.ChargeGroup = "ALL";
			configuration2.ConsolPaymentTerm = PaymentType.Prepaid;
			configuration2.RelatedJob = GatewayRelatedJob.Codes.All;
			configuration2.PreviousSendingAgent = GatewayPreviousSendingAgent.Codes.All;
			configuration2.Debtor = GatewayDebtor.Codes.ReceivingAgent;

			AssertHasError(configuration2.ConsolDirectionInfo, configAlreadyExists);
			AssertNoErrors(configuration2.ConsolTransportModeInfo);
			AssertNoErrors(configuration2.ChargeGroupInfo);
			AssertNoErrors(configuration2.ConsolPaymentTermInfo);
			AssertNoErrors(configuration2.RelatedJobInfo);
			AssertNoErrors(configuration2.PreviousSendingAgentInfo);
			AssertNoErrors(configuration2.DebtorInfo);

			configuration2.RelatedJob = GatewayRelatedJob.Codes.RelatedToShipment;

			AssertNoErrors(configuration2.ConsolDirectionInfo);
			AssertNoErrors(configuration2.ConsolTransportModeInfo);
			AssertNoErrors(configuration2.ChargeGroupInfo);
			AssertNoErrors(configuration2.ConsolPaymentTermInfo);
			AssertNoErrors(configuration2.RelatedJobInfo);
			AssertNoErrors(configuration2.PreviousSendingAgentInfo);
			AssertNoErrors(configuration2.DebtorInfo);

			configuration.RelatedJob = GatewayRelatedJob.Codes.RelatedToShipment;
			AssertHasError(configuration.ConsolDirectionInfo, configAlreadyExists);
			AssertNoErrors(configuration2.ConsolTransportModeInfo);
			AssertNoErrors(configuration2.ChargeGroupInfo);
			AssertNoErrors(configuration.ConsolPaymentTermInfo);
			AssertNoErrors(configuration.RelatedJobInfo);
			AssertNoErrors(configuration.PreviousSendingAgentInfo);
			AssertNoErrors(configuration.DebtorInfo);

			configuration.PreviousSendingAgent = GatewayPreviousSendingAgent.Codes.GatewayAgent;
			AssertNoErrors(configuration.ConsolDirectionInfo);
			AssertNoErrors(configuration2.ConsolTransportModeInfo);
			AssertNoErrors(configuration2.ChargeGroupInfo);
			AssertNoErrors(configuration.ConsolPaymentTermInfo);
			AssertNoErrors(configuration.RelatedJobInfo);
			AssertNoErrors(configuration.PreviousSendingAgentInfo);
			AssertNoErrors(configuration.DebtorInfo);
		}
		public void TestValidateConsolDirection()
		{
			AssertNoErrors(configuration.ConsolDirectionInfo);

			configuration.ConsolDirection = "";
			AssertHasErrors("ConsolDirection can't be blank", configuration.ConsolDirectionInfo);

			configuration.ConsolDirection = "AAA";
			AssertHasErrors("ConsolDirection has invalid code", configuration.ConsolDirectionInfo);

			configuration.ConsolDirection = "IMP";
			Assert(!configuration.ConsolDirectionInfo.HasErrors());
		}

		public void TestValidateConsolTransportMode()
		{
			AssertNoErrors(configuration.ConsolTransportModeInfo);

			configuration.ConsolTransportMode = "";
			AssertHasErrors("ConsolTransportMode can't be blank", configuration.ConsolTransportModeInfo);

			configuration.ConsolTransportMode = "AAA";
			AssertHasErrors("ConsolTransportMode has invalid code", configuration.ConsolTransportModeInfo);

			configuration.ConsolTransportMode = "SEA";
			Assert(!configuration.ConsolTransportModeInfo.HasErrors());
		}

		public void TestValidateChargeGroup()
		{
			AssertNoErrors(configuration.ChargeGroupInfo);

			configuration.ChargeGroup = "";
			AssertHasErrors("ChargeGroup can't be blank", configuration.ChargeGroupInfo);

			configuration.ChargeGroup = "AAA";
			AssertHasErrors("ChargeGroup has invalid code", configuration.ChargeGroupInfo);

			configuration.ChargeGroup = "ORG";
			Assert(!configuration.ChargeGroupInfo.HasErrors());
		}

		public void TestValidateConsolPaymentTerm()
		{
			AssertNoErrors(configuration.ConsolPaymentTermInfo);

			configuration.ConsolPaymentTerm = "";
			AssertHasErrors("ConsolPaymentTerm can't be blank", configuration.ConsolPaymentTermInfo);

			configuration.ConsolPaymentTerm = "AAA";
			AssertHasErrors("ConsolPaymentTerm has invalid code", configuration.ConsolPaymentTermInfo);

			configuration.ConsolPaymentTerm = "PPD";
			Assert(!configuration.ConsolPaymentTermInfo.HasErrors());
		}

		public void TestValidateDebtor()
		{
			ZPropertyInfo info = configuration.DebtorInfo;
			AssertNoErrors(info);

			configuration.Debtor = "";
			AssertHasErrors("Debtor can't be blank", info);

			configuration.Debtor = "AAA";
			AssertHasErrors("Debtor has invalid code", info);

			configuration.Debtor = "SGT";
			Assert(!info.HasErrors());

			configuration.Debtor = "SPA";
			AssertHasErrors("Debtor has invalid code", info);

			configuration.RelatedJob = "SHP";
			Assert(!info.HasErrors());
		}

		public void TestValidateRelatedJob()
		{
			ZPropertyInfo info = configuration.RelatedJobInfo;
			AssertNoErrors(info);

			configuration.RelatedJob = "";
			AssertHasErrors("Related Job can't be blank", info);

			configuration.RelatedJob = GatewayRelatedJob.Codes.RelatedToShipment;
			Assert(!info.HasErrors());

			configuration.RelatedJob = "XXX";
			AssertHasErrors("Related Job has invalid code", info);

			configuration.RelatedJob = GatewayRelatedJob.Codes.NotRelatedToJob;
			AssertNoErrors(info);

			configuration.RelatedJob = GatewayRelatedJob.Codes.All;
			AssertNoErrors(info);
		}

		public void TestValidatePreviousSendingAgent()
		{
			ZPropertyInfo info = configuration.PreviousSendingAgentInfo;
			AssertNoErrors(info);

			configuration.PreviousSendingAgent = "";
			AssertHasErrors("Previous Sending Agent can't be blank", info);

			configuration.PreviousSendingAgent = "XXX";
			AssertHasErrors("Previous Sending Agent has invalid code", info);

			configuration.PreviousSendingAgent = GatewayPreviousSendingAgent.Codes.NoPrevSendingAgent;
			AssertHasErrors("Previous Sending Agent has invalid code", info);

			configuration.PreviousSendingAgent = GatewayPreviousSendingAgent.Codes.All;
			AssertNoErrors(info);

			configuration.PreviousSendingAgent = GatewayPreviousSendingAgent.Codes.GatewayAgent;
			AssertHasErrors("Previous Sending Agent has invalid code", info);

			configuration.RelatedJob = "SHP";
			configuration.PreviousSendingAgent = GatewayPreviousSendingAgent.Codes.NoPrevSendingAgent;
			AssertNoErrors(info);
		}

		public void TestCheckIdenticalConfigurationExists()
		{
			var newConfiguration = new GatewayChargeDefaultDebtorConfiguration();
			configurations.Add(newConfiguration);

			newConfiguration.ConsolDirection = "IMP";
			newConfiguration.ConsolTransportMode = "SEA";
			newConfiguration.ChargeGroup = "ALL";
			newConfiguration.ConsolPaymentTerm = "PPD";
			newConfiguration.RelatedJob = GatewayRelatedJob.Codes.All;
			newConfiguration.PreviousSendingAgent = GatewayPreviousSendingAgent.Codes.All;
			AssertHasError(newConfiguration.ConsolDirectionInfo, "Configuration with identical criteria already exists.");

			newConfiguration.ConsolDirection = "EXP";
			newConfiguration.ConsolTransportMode = "SEA";
			newConfiguration.ChargeGroup = "ALL";
			newConfiguration.ConsolPaymentTerm = "PPD";
			newConfiguration.RelatedJob = GatewayRelatedJob.Codes.All;
			newConfiguration.PreviousSendingAgent = GatewayPreviousSendingAgent.Codes.All;
			AssertNoErrors(configuration.ConsolDirectionInfo);

			newConfiguration.ConsolDirection = "IMP";
			AssertHasError(newConfiguration.ConsolDirectionInfo, "Configuration with identical criteria already exists.");
		}

		protected override void SetUp()
		{
			base.SetUp();

			configuration = new GatewayChargeDefaultDebtorConfiguration();
			configuration.ConsolDirection = "IMP";
			configuration.ConsolTransportMode = "SEA";
			configuration.ChargeGroup = "ALL";
			configuration.ConsolPaymentTerm = PaymentType.Prepaid;
			configuration.RelatedJob = GatewayRelatedJob.Codes.All;
			configuration.PreviousSendingAgent = GatewayPreviousSendingAgent.Codes.All;
			configuration.Debtor = GatewayDebtor.Codes.SendingAgent;

			configurations = new GatewayChargeDefaultDebtorConfigurationCollection();
			configurations.Add(configuration);
		}

		protected override bool RequiresFactory => false;
		protected override bool RequiresFallbackLevel => false;
		protected new GatewayChargeDefaultDebtorConfiguration BizObj => (GatewayChargeDefaultDebtorConfiguration)base.BizObj;

		GatewayChargeDefaultDebtorConfiguration configuration;
		GatewayChargeDefaultDebtorConfigurationCollection configurations;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var configuration = new GatewayChargeDefaultDebtorConfiguration();
			configuration.ConsolDirection = "ALL";
			configuration.ConsolTransportMode = "ALL";
			configuration.ChargeGroup = "ALL";
			configuration.ConsolPaymentTerm = PaymentType.Prepaid;
			configuration.RelatedJob = GatewayRelatedJob.Codes.All;
			configuration.PreviousSendingAgent = GatewayPreviousSendingAgent.Codes.All;
			configuration.Debtor = GatewayDebtor.Codes.SendingAgent;
			return configuration;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
