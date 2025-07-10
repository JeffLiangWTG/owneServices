using CargoWise.Types;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class DummyCFSShipmentStatusProvider : CFSShipmentStatusProvider
	{
		public DummyCFSShipmentStatusProvider(CFSShipment shipment)
			: base(shipment)
		{
		}

		public ZString ShortStatusExposed;
		public StatusClass StatusClassExposed;
		public ZString StatusCoreExposed;
		public ZString DetailsFromMessagesExposed;
		public bool CanSaveAndPrintExposed;

		protected override ZString ShortStatusCore()
		{
			return ShortStatusExposed;
		}

		protected override StatusClass StatusClassCore()
		{
			return StatusClassExposed;
		}

		protected override ZString StatusCore()
		{
			return StatusCoreExposed;
		}

		protected override ZString DetailsFromMessagesCore
		{
			get { return DetailsFromMessagesExposed; }
		}

		protected override bool CanSaveAndPrintCore(ISaveAndPrintUI ui)
		{
			return CanSaveAndPrintExposed;
		}
	}
}
