using Enterprise.eTail.GUI;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Module.Testing
{
	[TestedType(typeof(HVLVActionMethodProvider))]
	public class HVLVActionMethodProviderTest : OperationalActionMethodProviderTest
	{
		public void TestNewMethods_ForwardingShipment()
		{
			var methods = Provider.NewMethods(new ForwardingShipmentSupporter());
			AssertEquals(1, methods.Length);
			AssertEquals(typeof(UpdateHVLVStatusActionMethod), methods[0].GetType());
		}

		public void TestNewMethods_HVLVConsignment()
		{
			var methods = Provider.NewMethods(new HVLVConsignmentOperationalActionSupporter());
			AssertEquals(1, methods.Length);
			AssertEquals(typeof(UpdateHVLVItemStatusActionMethod), methods[0].GetType());
		}

		#region Implementation

		protected override ActionMethodProviderID ID => ActionMethodProviderIDs.HVLV;

		#endregion
	}
}
