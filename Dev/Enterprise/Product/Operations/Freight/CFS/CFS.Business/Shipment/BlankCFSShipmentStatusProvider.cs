using CargoWise.Types;

namespace Enterprise.Freight.CFS.Business
{
	public class BlankCFSShipmentStatusProvider : CFSShipmentStatusProvider
	{
		public BlankCFSShipmentStatusProvider(CFSShipment shipment)
			: base(shipment)
		{
		}

		protected override ZString StatusCore()
		{
			return ZString.Empty;
		}

		protected override ZString ShortStatusCore()
		{
			return ZString.Empty;
		}

		protected override ZString DetailsFromMessagesCore
		{
			get { return ZString.Empty; }
		}

		protected override StatusClass StatusClassCore()
		{
			return StatusClass.None;
		}

		protected override bool CanSaveAndPrintCore(ISaveAndPrintUI ui)
		{
			return true;
		}
	}
}
