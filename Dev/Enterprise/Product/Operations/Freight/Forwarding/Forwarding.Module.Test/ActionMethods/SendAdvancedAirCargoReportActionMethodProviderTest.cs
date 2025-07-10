using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(SendAdvancedAirCargoReportActionMethodProvider))]
	public class SendAdvancedAirCargoReportActionMethodProviderTest : OperationalActionMethodProviderTest
	{
		public void TestNewConsolMethods()
		{
			var methods = Provider.NewMethods(new ForwardingConsolActionSupporter());
			AssertEquals(2, methods.Length);
			AssertEquals(typeof(SendACASConsolActionMethod), methods[0].GetType());
			AssertEquals(typeof(SendCCTConsolActionMethod), methods[1].GetType());
		}
		public void TestNewShipmentMethods()
		{
			var methods = Provider.NewMethods(new ForwardingShipmentSupporter());
			AssertEquals(2, methods.Length);
			AssertEquals(typeof(SendACASShipmentActionMethod), methods[0].GetType());
			AssertEquals(typeof(SendCCTShipmentActionMethod), methods[1].GetType());
		}
		#region Implementation

		protected override ActionMethodProviderID ID
		{
			get { return ActionMethodProviderIDs.SendAdvancedAirCargoReport; }
		}

		#endregion
	}
}
