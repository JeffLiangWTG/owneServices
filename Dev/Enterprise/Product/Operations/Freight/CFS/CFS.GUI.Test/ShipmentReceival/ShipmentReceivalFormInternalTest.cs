using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.CFS.GUI
{
	sealed class ShipmentReceivalFormInternalTest : TestCaseWithFactory
	{
		public void TestNewRaiseEventLogForm()
		{
			var bo = Factory.New<CFSShipment>();
			bo.Consols.AddNew();
			Factory.Save();

			using (var shipmentForm = new ShipmentReceivalForm(bo))
			{
				var eventType = Enterprise.ZArchitecture.Business.Events.DeliveryOrderHandedOver;
				using (var eventAddForm = shipmentForm.NewRaiseEventLogForm(eventType))
				{
					var log = (BaseStmALog)eventAddForm.BusinessEntity;
					AssertEquals("Correct event type created", eventType.Code, log.SL_SE_NKEvent);
				}
			}
		}
	}
}
