using CargoWise.Types;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSShipmentStatusProviderDummyObject : CFSShipmentStatusProvider
	{
		public CFSShipmentStatusProviderDummyObject(CFSShipment shipment)
			: base(shipment)
		{
		}

		protected override ZString DetailsFromMessagesCore
		{
			get { return "Cuckoo Squeaker of Message Details"; }
		}

		protected override bool CanSaveAndPrintCore(ISaveAndPrintUI ui)
		{
			return ui.Ask("foo!");
		}

		protected override ZString ShortStatusCore()
		{
			return ShortStatusExposed;
		}

		public ZString ShortStatusExposed;

		protected override ZString StatusCore()
		{
			return ZString.Empty;
		}

		protected override StatusClass StatusClassCore()
		{
			return StatusClass.None;
		}
	}
}
