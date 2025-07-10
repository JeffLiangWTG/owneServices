using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class UnAllocatedShipmentView : BusinessObjectCollectionView<CommonShipment>
	{
		public UnAllocatedShipmentView(ShipmentCollection shipments, JobSailing sailing)
			: base(shipments)
		{
			Init(sailing);
		}

		void Init(JobSailing sailing)
		{
			Sailing = sailing;
			showOnlyReceived = true;
			showOnlyThisSailing = true;
			Rebuild();
		}

		protected override void RebuildOnConstruction()
		{
			// will rebuild after initialisation
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			CommonShipment shipment = (CommonShipment)element;
			bool isPartOfCollection = true;

			if (ShowOnlyThisSailing && Sailing != null)
			{
				isPartOfCollection = HasSailing(shipment) && IsAttachedToSailing(shipment, Sailing);
			}

			if (isPartOfCollection && ShowOnlyReceived)
			{
				isPartOfCollection = shipment.IsReceived;
			}

			return isPartOfCollection;
		}

		bool HasSailing(CommonShipment shipment)
		{
			return shipment.Sailing != null;
		}

		bool IsAttachedToSailing(CommonShipment shipment, JobSailing sailing)
		{
			return shipment.Sailing.PK == sailing.PK ||
				(
					shipment.Sailing.JX_JA_RL_NKPortOfLoading == Sailing.JX_JA_RL_NKPortOfLoading &&
					shipment.Sailing.JX_JB_RL_NKPortOfDischarge == Sailing.JX_JB_RL_NKPortOfDischarge
				);
		}

		protected JobSailing Sailing { get; private set; }

		public ZBool ShowOnlyThisSailing
		{
			get { return showOnlyThisSailing; }
			set
			{
				if (showOnlyThisSailing != value)
				{
					showOnlyThisSailing = value;
					Rebuild();
				}
			}
		}
		ZBool showOnlyThisSailing;

		public ZBool ShowOnlyReceived
		{
			get { return showOnlyReceived; }
			set
			{
				if (showOnlyReceived != value)
				{
					showOnlyReceived = value;
					Rebuild();
				}
			}
		}
		ZBool showOnlyReceived;

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
