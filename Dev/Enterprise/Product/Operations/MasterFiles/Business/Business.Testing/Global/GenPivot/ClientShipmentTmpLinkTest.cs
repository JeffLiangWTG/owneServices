using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ClientShipmentTmpLink))]
	sealed class ClientShipmentTmpLinkTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTmpLink()
		{
			ZGuid clientPK1 = new ZGuid("c4d2790d-7caa-4321-b9a2-77310ba8899b");
			ZGuid clientPK2 = new ZGuid("cee81915-449a-4580-8f48-2ea7377ffce3");
			ZGuid bookingPK = new ZGuid("a28d4b2f-dd22-4780-9b48-fd2a23c6efbc");

			ClientShipmentTmpLink.Store(clientPK1, bookingPK, Factory);
			ClientShipmentTmpLink.Store(clientPK2, bookingPK, Factory);

			AssertEquals(clientPK2, ClientShipmentTmpLink.Consume(bookingPK, Factory));
			AssertEquals(ZGuid.Empty, ClientShipmentTmpLink.Consume(bookingPK, Factory));
		}
	}
}
