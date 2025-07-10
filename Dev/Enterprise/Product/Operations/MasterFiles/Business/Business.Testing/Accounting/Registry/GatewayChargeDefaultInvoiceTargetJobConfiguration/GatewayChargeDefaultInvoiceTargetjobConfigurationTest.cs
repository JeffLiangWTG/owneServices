using CargoWise.ComponentModel;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GatewayChargeDefaultInvoiceTargetJobConfiguration))]
	class GatewayChargeDefaultInvoiceTargetjobConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateConsolDirection()
		{
			AssertNoErrors(configuration.ConsolDirectionInfo);

			configuration.ConsolDirection = "";
			AssertHasErrors("ConsolDirection can't be blank", configuration.ConsolDirectionInfo);

			configuration.ConsolDirection = "AAA";
			AssertHasErrors("ConsolDirection has invalid code", configuration.ConsolDirectionInfo);

			configuration.ConsolDirection = "EXP";
			Assert(!configuration.ConsolDirectionInfo.HasErrors());
		}

		public void TestValidateConsolTransportMode()
		{
			AssertNoErrors(configuration.ConsolTransportModeInfo);

			configuration.ConsolTransportMode = "";
			AssertHasErrors("ConsolTransportMode can't be blank", configuration.ConsolTransportModeInfo);

			configuration.ConsolTransportMode = "AAA";
			AssertHasErrors("ConsolTransportMode has invalid code", configuration.ConsolTransportModeInfo);

			configuration.ConsolTransportMode = "AIR";
			Assert(!configuration.ConsolTransportModeInfo.HasErrors());
		}

		public void TestValidatePreviousSendingAgentType()
		{
			AssertNoErrors(configuration.PreviousSendingAgentTypeInfo);

			configuration.PreviousSendingAgentType = "";
			AssertHasErrors("PreviousSendingAgentType can't be blank", configuration.PreviousSendingAgentTypeInfo);

			configuration.PreviousSendingAgentType = "AAA";
			AssertHasErrors("PreviousSendingAgentType has invalid code", configuration.PreviousSendingAgentTypeInfo);

			configuration.PreviousSendingAgentType = "GTA";
			Assert(!configuration.PreviousSendingAgentTypeInfo.HasErrors());
		}

		public void TestValidateInvoiceTargetJobType()
		{
			AssertNoErrors(configuration.InvoiceTargetJobTypeInfo);

			configuration.InvoiceTargetJobType = "";
			AssertHasErrors("InvoiceTargetJobType can't be blank", configuration.InvoiceTargetJobTypeInfo);

			configuration.InvoiceTargetJobType = "AAA";
			AssertHasErrors("InvoiceTargetJobType has invalid code", configuration.InvoiceTargetJobTypeInfo);

			configuration.InvoiceTargetJobType = "REL";
			Assert(!configuration.InvoiceTargetJobTypeInfo.HasErrors());
		}

		public void TestCheckIdenticalConfigurationExists()
		{
			var newConfiguration = new GatewayChargeDefaultInvoiceTargetJobConfiguration();
			configurations.Add(newConfiguration);

			newConfiguration.ConsolDirection = "IMP";
			newConfiguration.ConsolTransportMode = "SEA";
			newConfiguration.PreviousSendingAgentType = "SGT";
			newConfiguration.InvoiceTargetJobType = "REL";
			AssertNoError(newConfiguration.ConsolDirectionInfo, "Configuration with identical criteria already exists.");

			newConfiguration = new GatewayChargeDefaultInvoiceTargetJobConfiguration();
			configurations.Add(newConfiguration);
			newConfiguration.ConsolDirection = "IMP";
			newConfiguration.ConsolTransportMode = "SEA";
			newConfiguration.PreviousSendingAgentType = "SGT";
			AssertHasError(newConfiguration.ConsolDirectionInfo, "Configuration with identical criteria already exists.");
		}

		protected override void SetUp()
		{
			base.SetUp();

			configuration = new GatewayChargeDefaultInvoiceTargetJobConfiguration();
			configuration.ConsolDirection = "ALL";
			configuration.ConsolTransportMode = "ALL";
			configuration.PreviousSendingAgentType = "ALL";
			configuration.InvoiceTargetJobType = "SCL";

			configurations = new GatewayChargeDefaultInvoiceTargetJobConfigurationCollection();
			configurations.Add(configuration);
		}

		protected override bool RequiresFactory => false;
		protected override bool RequiresFallbackLevel => false;
		protected new GatewayChargeDefaultInvoiceTargetJobConfiguration BizObj => (GatewayChargeDefaultInvoiceTargetJobConfiguration)base.BizObj;

		GatewayChargeDefaultInvoiceTargetJobConfiguration configuration;
		GatewayChargeDefaultInvoiceTargetJobConfigurationCollection configurations;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var configuration = new GatewayChargeDefaultInvoiceTargetJobConfiguration();
			configuration.ConsolDirection = "ALL";
			configuration.ConsolTransportMode = "ALL";
			configuration.PreviousSendingAgentType = "ALL";
			configuration.InvoiceTargetJobType = "SCL";
			return configuration;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
