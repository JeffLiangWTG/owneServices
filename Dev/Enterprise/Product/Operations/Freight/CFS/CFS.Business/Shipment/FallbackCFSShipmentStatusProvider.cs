using CargoWise.Types;

namespace Enterprise.Freight.CFS.Business
{
	public class FallbackCFSShipmentStatusProvider : CFSShipmentStatusProvider
	{
		public FallbackCFSShipmentStatusProvider(CFSShipment shipment)
			: base(shipment)
		{
			MainStatusProvider = CFSShipmentStatusProvider.New(shipment);
			OldStatusProvider = new OldCFSShipmentStatusProvider(shipment);
		}

		protected override ZString StatusCore()
		{
			ZString result = MainStatusProvider.Status;
			if (result.IsEmpty)
			{
				result = OldStatusProvider.Status;
			}

			return result;
		}

		protected override ZString DetailsFromMessagesCore
		{
			get
			{
				ZString result = MainStatusProvider.DetailsFromMessages;
				if (result.IsEmpty)
				{
					result = OldStatusProvider.DetailsFromMessages;
				}

				return result;
			}
		}

		protected override ZString ShortStatusCore()
		{
			ZString result = MainStatusProvider.ShortStatus;
			if (result.IsEmpty)
			{
				result = OldStatusProvider.ShortStatus;
			}

			return result;
		}

		protected override StatusClass StatusClassCore()
		{
			StatusClass result = MainStatusProvider.StatusClass;
			if (result == StatusClass.None)
			{
				result = OldStatusProvider.StatusClass;
			}

			return result;
		}

		protected override bool CanSaveAndPrintCore(ISaveAndPrintUI ui)
		{
			if (!MainStatusProvider.ShortStatus.IsEmpty || MainStatusProvider.GetType() == typeof(BlankCFSShipmentStatusProvider))
			{
				return MainStatusProvider.CanSaveAndPrint(ui);
			}

			return OldStatusProvider.CanSaveAndPrint(ui);
		}

		readonly CFSShipmentStatusProvider MainStatusProvider;
		readonly OldCFSShipmentStatusProvider OldStatusProvider;
	}
}
