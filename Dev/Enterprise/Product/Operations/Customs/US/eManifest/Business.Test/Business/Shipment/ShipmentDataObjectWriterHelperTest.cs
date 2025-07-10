using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class ShipmentDataObjectWriterHelperTest : TestCaseWithFactory
	{
		public void TestUniversalDataObjectWriterHelper()
		{
			var tripBO = Factory.New<Trip>();
			var shipmentBO = tripBO.Shipments.AddNew();
			var writer = ShipmentDataObjectWriter.New(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, shipmentBO)), shipmentBO, tripBO);
			AssertNotNull(writer.helper);
			AssertType<ShipmentDataObjectWriterHelper>(writer.helper);
		}
	}
}
