using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class TopLevelShipmentCollection : BusinessObjectCollectionView<CommonShipment>
	{
		public TopLevelShipmentCollection(BusinessObjectCollection collectionToFilter, CommonConsol parentConsol)
			: base(collectionToFilter)
		{
			this.parentConsol = Argument.NotNull(parentConsol, "parentConsol");
			Rebuild();

			AllowAddNew = base.AllowNewCore;
		}

		protected override void RebuildOnConstruction()
		{
			// doing nothing here to avoid virtual member call happening in SubsetBusinessObjectCollection constructor. Instead Rebuild() is called from the local constructor after all the variables are set.
		}

		readonly CommonConsol parentConsol;

		public bool AllowAddNew { get; set; }

		protected override bool AllowNewCore
		{
			get { return AllowAddNew; }
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var shipment = (CommonShipment)element;
			var master = shipment.CoLoadMasterShipment;

			return
				!shipment.IsCancelled
				&&
				(
					ShouldShowChildShipments
					|| master == null
					|| !parentConsol.IsLinkedTo(master)
				);
		}

		#region ShouldShowChildShipments

		public bool ShouldShowChildShipments
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return shouldShowChildShipments; }
			set
			{
				if (value != shouldShowChildShipments)
				{
					shouldShowChildShipments = value;
					Rebuild();
				}
			}
		}

		bool shouldShowChildShipments;

		#endregion
	}
}
