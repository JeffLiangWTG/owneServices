namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GatewayConsolConsumerTypeTest : ConsolConsumerTypeTest
	{
		public override void TestIsTransportModeSupported()
		{
			AssertEquals(true, ConsumerType.IsTransportModeSupported);
		}

		public override void TestIsDirectionSupported()
		{
			AssertEquals(true, ConsumerType.IsDirectionSupported);
		}

		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType()
		{
			return JobInvoicingConsumerTypes.GatewayConsol;
		}

		public override void TestMenuName()
		{
			AssertEquals("Gateway Billing", JobInvoicingConsumerTypes.GatewayConsol.MenuName(null));
		}

		public override void TestDisplayName()
		{
			AssertEquals("Gateway Billing", JobInvoicingConsumerTypes.GatewayConsol.DisplayName(null));
		}
	}
}
