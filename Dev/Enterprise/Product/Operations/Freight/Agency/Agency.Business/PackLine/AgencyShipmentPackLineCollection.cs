using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentPackLineCollection : OuterPackLineCollection, ICollectionChangeTrackable<UNDGDataItem>
	{
		public AgencyShipmentPackLineCollection(AgencyShipment master)
			: base(master, master.Factory)
		{
			Factory.Saved += FactoryOnSaved;
		}

		public new AgencyShipmentPackLine AddNew()
		{
			return (AgencyShipmentPackLine)base.AddNew();
		}

		public new AgencyShipmentPackLine this[int index]
		{
			get { return (AgencyShipmentPackLine)Elements[index]; }
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(AgencyShipmentPackLine);
		}

		#region ICollectionChangeTrackable<UNDGDataItem>

		readonly List<UNDGDataItem> originalUNDGs = new List<UNDGDataItem>();

		public void ResetChangeTracking()
		{
			originalUNDGs.Clear();
			originalUNDGs.AddRange(UNDGs);
		}

		IEnumerable<UNDGDataItem> UNDGs
		{
			get
			{
				return this.Cast<AgencyShipmentPackLine>()
					.SelectMany(packLine => packLine.UNDGs);
			}
		}

		IEnumerable<UNDGDataItem> ICollectionChangeTrackable<UNDGDataItem>.AddedElements
		{
			get { return UNDGs.Except(originalUNDGs); }
		}

		IEnumerable<UNDGDataItem> ICollectionChangeTrackable<UNDGDataItem>.UpdatedElements
		{
			get { return UNDGs.Where(undg => undg.IsInDatabase && undg.HasChanges); }
		}

		IEnumerable<UNDGDataItem> ICollectionChangeTrackable<UNDGDataItem>.RemovedElements
		{
			get { return originalUNDGs.Except(UNDGs); }
		}

		IEnumerable<UNDGDataItem> ICollectionChangeTrackable<UNDGDataItem>.ChangedElements
		{
			get
			{
				ICollectionChangeTrackable<UNDGDataItem> undgsChangeTrackable = this;

				return undgsChangeTrackable.AddedElements
					.Concat(undgsChangeTrackable.UpdatedElements)
					.Concat(undgsChangeTrackable.RemovedElements);
			}
		}

		#endregion

		#region Implementation

		protected override void OnLoaded()
		{
			base.OnLoaded();
			ResetChangeTracking();
		}

		void FactoryOnSaved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				ResetChangeTracking();
			}
		}

		protected override void SetPackLineDescription(PackLine packLine)
		{
			ZString detailedDescription = Shipment.DetailedGoodsDescriptionNoteText;
			packLine.JL_DetailedDescription = detailedDescription.IsEmpty ? Shipment.JS_GoodsDescription : detailedDescription;
		}

		protected override void EnsurePackLineExists()
		{
			if (!((AgencyShipment)Master).IsTopLevelPacksMode)
			{
				base.EnsurePackLineExists();
			}
		}

		#endregion
	}
}


