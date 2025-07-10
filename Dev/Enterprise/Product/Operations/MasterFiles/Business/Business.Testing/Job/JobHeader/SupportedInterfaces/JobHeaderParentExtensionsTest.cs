using CargoWise.EntityFramework.Testing;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class JobHeaderParentExtensionsTest : TestCaseWithFactory
	{
		public void TestTablePrefix()
		{
			var shipment = Factory.New<IForwardingShipment>() as IJobHeaderParent;
			AssertEquals("JS", shipment.TablePrefix());

			var consol = Factory.New<IForwardingConsol>() as IJobHeaderParent;
			AssertEquals("JK", consol.TablePrefix());
		}
	}
}
