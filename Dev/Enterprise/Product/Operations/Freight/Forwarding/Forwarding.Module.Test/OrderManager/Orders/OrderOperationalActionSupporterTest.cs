using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Module.Testing
{
	[TestedType(typeof(OrderOperationalActionSupporter))]
	internal class OrderOperationalActionSupporterTest : OperationalActionSupporterTest<OrderOperationalActionSupporter>
	{
		#region Implementation

		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Orders; }
		}

		#endregion
	}
}
