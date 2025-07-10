using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class PackLineCollection : DependentBusinessObjectCollection<PackLine, CommonShipment>, IPackLineCollection, IPackLineParentChangeNotifiable
	{
		public PackLineCollection(CommonShipment master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		#region Business Object Collection Overrides

		protected override void EnsureNotInAnotherCollectionForSameFK(BusinessObject businessObject)
		{
			//we allow packlines to be added to CoLoadMaster shipments so CoLoadMaster can show child shipments' packlines
			if (!IsMasterRepresentingAllChildShipments)
			{
				base.EnsureNotInAnotherCollectionForSameFK(businessObject);
			}
		}

		internal void DeactivateActiveBusinessObjectCollections()
		{
			foreach (PackLine packLine in this)
			{
				packLine.DeactivateActiveBusinessObjectCollections();
			}
		}

		public override bool ReadOnly
		{
			get { return Master.ReadOnly || base.ReadOnly; }
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			var packline = dependent as PackLine;
			if (packline != null)
			{
				packline.ShipmentLinkLogs.Add($"PackLineCollection.SetCollectionRelationships");
			}

			if (!IsMasterRepresentingAllChildShipments)
			{
				base.SetCollectionRelationships(dependent);
			}

			if (packline != null)
			{
				packline.ShipmentLinkLogs.Add(string.Format(CultureInfo.InvariantCulture, "JL_JS after calling base SetCollectionRelationships: {0}", packline.JL_JS));
			}
		}

		protected override void RemoveCollectionRelationshipsCore(BusinessObject dependent, bool forDelete)
		{
			var packline = dependent as PackLine;
			if (packline != null)
			{
				packline.ShipmentLinkLogs.Add(string.Format(CultureInfo.InvariantCulture, (NoResString)"PackLineCollection.RemoveCollectionRelationshipsCore: forDelete {0}", forDelete));
				if (!forDelete)
				{
					packline.ShipmentLinkLogs.Add(System.Environment.StackTrace);
				}
			}

			base.RemoveCollectionRelationshipsCore(dependent, forDelete);

			if (packline != null)
			{
				packline.ShipmentLinkLogs.Add(string.Format(CultureInfo.InvariantCulture, "JL_JS after calling base RemoveCollectionRelationshipsCore: {0}", packline.JL_JS));
			}
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();

			if (IsMasterRepresentingAllChildShipments)
			{
				var pks = Shipment.GetPksFromAllSubShipmentsWithoutChildren();
				if (pks.Any())
				{
					result.AddToFilter(JoinCondition.Or, JobPackLinesSchema.JL_JS, pks);
				}
			}

			return result;
		}

		public override void RemoveAndDeleteAll()
		{
			List<PackLine> toRemove = new List<PackLine>();
			foreach (PackLine packline in this)
			{
				if (packline.Shipment != null)
				{
					if (packline.Shipment.PK == this.Shipment.PK)
					{
						toRemove.Add(packline);
					}
				}
			}

			foreach (PackLine packline in toRemove)
			{
				RemoveAndDelete(packline);
			}
		}

		protected override bool AllowNewCore
		{
			get
			{
				return !IsMasterRepresentingAllChildShipments;
			}
		}

		protected override bool AllowRemoveCore
		{
			get
			{
				return !IsMasterRepresentingAllChildShipments;
			}
		}

		#endregion

		#region Events

		public void NotifyDescriptionChanged(ZString oldValue, ZString newValue)
		{
			OnNotifyDescriptionChanged(oldValue, newValue);
		}

		public void NotifyWeightChanged(ZDecimal oldWeight, ZDecimal newWeight)
		{
			OnNotifyWeightChanged(oldWeight, newWeight);
		}

		public void NotifyWeightUQChanged(ZString oldWeightUQ, ZString newWeightUQ)
		{
			OnNotifyWeightUQChanged(oldWeightUQ, newWeightUQ);
		}

		public void NotifyVolumeChanged(ZDecimal oldVolume, ZDecimal newVolume)
		{
			OnNotifyVolumeChanged(oldVolume, newVolume);
		}

		public void NotifyVolumeUQChanged(ZString oldVolumeUQ, ZString newVolumeUQ)
		{
			OnNotifyVolumeUQChanged(oldVolumeUQ, newVolumeUQ);
		}

		public void NotifyPackageCountChanged(ZInt oldCount, ZInt newCount)
		{
			OnNotifyPackageCountChanged(oldCount, newCount);
		}

		public void NotifyPackageTypeChanged(ZString oldType, ZString newType)
		{
			OnNotifyPackageTypeChanged(oldType, newType);
		}

		public void NotifyLoadingMetersChanged(ZDecimal oldValue, ZDecimal newValue)
		{
			OnNotifyLoadingMetersChanged(oldValue, newValue);
		}

		#endregion

		#region Dangerous Goods

		public ZBool HasDangerousGoods
		{
			get
			{
				foreach (PackLine packLine in this)
				{
					if (packLine.UNDGs.Any(dgItem => dgItem.Substance != null))
					{
						return true;
					}
				}

				return false;
			}
		}

		#endregion

		#region PackLineCollection Totals

		public ZInt TotalPackages
		{
			get { return Totals.TotalPackages; }
		}

		public ZString TotalPackagesUnit
		{
			get { return Totals.TotalPackagesUnit; }
		}

		public ZDecimal TotalWeight
		{
			get { return Totals.TotalWeight; }
		}

		public ZString TotalWeightUnit
		{
			get { return Totals.TotalWeightUnit; }
		}

		public ZDecimal TotalVolume
		{
			get { return Totals.TotalVolume; }
		}

		public ZString TotalVolumeUnit
		{
			get { return Totals.TotalVolumeUnit; }
		}

		public ZDecimal TotalLoadingMeters
		{
			get { return Totals.TotalLoadingMeters; }
		}

		#endregion

		#region Implementation

		protected bool IsMasterRepresentingAllChildShipments
		{
			get { return Shipment != null && !Shipment.IsDeleted && Shipment.IsMasterShipmentRepresentingAllChildShipments; }
		}

		#region BusinessObjectCollection overrides

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			var line = (PackLine)bizOAdded;
			HookPackLineForShipmentTotals(line);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			var line = bizO as PackLine;
			UnHookPackLineForShipmentTotals(line);
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);
			RefreshPackLineTotalsOnShipment();
		}

		protected override void OnLoaded()
		{
			base.OnLoaded();
			RefreshPackLineTotalsOnShipment();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			PackLine packLine = (PackLine)child;
			packLine.JL_F3_NKPackType = ShipmentPackageUnit;
			packLine.JL_ActualWeightUQ = ShipmentWeightUnit;
			packLine.JL_ActualVolumeUQ = ShipmentVolumeUnit;
			SetPackLineDescription(packLine);
		}

		#endregion

		#region Update ParentShipment Totals

		void HookPackLineForShipmentTotals(PackLine line)
		{
			line.JL_PackageCountInfo.ValueChanged += new EventHandler(OnPackLineJL_PackageCountChanged);
			line.JL_ActualWeightInfo.ValueChanged += new EventHandler(OnPackLineJL_ActualWeightChanged);
			line.JL_ActualVolumeInfo.ValueChanged += new EventHandler(OnPackLineJL_ActualVolumeChanged);
		}

		void UnHookPackLineForShipmentTotals(PackLine line)
		{
			line.JL_PackageCountInfo.ValueChanged -= new EventHandler(OnPackLineJL_PackageCountChanged);
			line.JL_ActualWeightInfo.ValueChanged -= new EventHandler(OnPackLineJL_ActualWeightChanged);
			line.JL_ActualVolumeInfo.ValueChanged -= new EventHandler(OnPackLineJL_ActualVolumeChanged);
		}

		protected virtual void OnPackLineJL_PackageCountChanged(object sender, EventArgs e)
		{
		}

		protected virtual void OnPackLineJL_ActualWeightChanged(object sender, EventArgs e)
		{
		}

		protected virtual void OnPackLineJL_ActualVolumeChanged(object sender, EventArgs e)
		{
		}

		protected virtual void RefreshPackLineTotalsOnShipmentCore()
		{
		}

		#endregion

		protected virtual void SetPackLineDescription(PackLine packLine)
		{
			if (Shipment.DetailedGoodsDescriptionNoteText.IsEmpty ||
				Shipment.DetailedGoodsDescriptionNoteText.Length <= Shipment.JS_GoodsDescription.Length)
			{
				packLine.JL_Description = Shipment.JS_GoodsDescription;
			}
			else if (Shipment.DetailedGoodsDescriptionNoteText.Length > packLine.JL_DescriptionInfo.MaxLength)
			{
				packLine.JL_Description = Shipment.JS_GoodsDescription;
				if (packLine.JL_DetailedDescription.IsEmpty)
				{
					packLine.JL_DetailedDescription = Shipment.DetailedGoodsDescriptionNoteText.StripNonWesternEuropeanCharacters();
				}
			}
			else
			{
				packLine.JL_Description = Shipment.DetailedGoodsDescriptionNoteText.StripNonWesternEuropeanCharacters();
			}
		}

		protected virtual void OnNotifyDescriptionChanged(ZString oldValue, ZString newValue)
		{
			EnsurePackLineExists();
			foreach (PackLine packLine in this)
			{
				if (packLine.JL_Description == oldValue)
				{
					SetPackLineDescription(packLine);
				}
			}
		}

		protected virtual void OnNotifyWeightChanged(ZDecimal oldWeight, ZDecimal newWeight)
		{
			EnsurePackLineExists();
			if (Count == 1)
			{
				this[0].JL_ActualWeight = newWeight;
			}
		}

		protected virtual void OnNotifyWeightUQChanged(ZString oldWeightUQ, ZString newWeightUQ)
		{
			EnsurePackLineExists();
			foreach (PackLine packLine in this)
			{
				if (packLine.JL_ActualWeightUQ == oldWeightUQ)
				{
					packLine.JL_ActualWeightUQ = newWeightUQ;
				}
			}
		}

		protected virtual void OnNotifyVolumeChanged(ZDecimal oldVolume, ZDecimal newVolume)
		{
			EnsurePackLineExists();
			if (Count == 1)
			{
				this[0].JL_ActualVolume = newVolume;
			}
		}

		protected virtual void OnNotifyVolumeUQChanged(ZString oldVolumeUQ, ZString newVolumeUQ)
		{
			EnsurePackLineExists();
			foreach (PackLine packLine in this)
			{
				if (packLine.JL_ActualVolumeUQ == oldVolumeUQ)
				{
					packLine.JL_ActualVolumeUQ = newVolumeUQ;
				}
			}
		}

		protected virtual void OnNotifyPackageCountChanged(ZInt oldCount, ZInt newCount)
		{
			EnsurePackLineExists();
			if (Count == 1)
			{
				this[0].JL_PackageCount = newCount;
			}
		}

		protected virtual void OnNotifyPackageTypeChanged(ZString oldType, ZString newType)
		{
			EnsurePackLineExists();
			foreach (PackLine packLine in this)
			{
				if (packLine.JL_F3_NKPackType == oldType)
				{
					packLine.JL_F3_NKPackType = newType;
				}
			}
		}

		protected virtual void OnNotifyLoadingMetersChanged(ZDecimal oldValue, ZDecimal newValue)
		{
			EnsurePackLineExists();
			if (Count == 1)
			{
				this[0].JL_LoadingMeters = newValue;
			}
		}

		#region Shipment Units

		protected virtual ZString ShipmentPackageUnit
		{
			get { return Shipment != null ? Shipment.ShipmentInnerPacksUnit : ZString.Empty; }
		}

		protected ZString ShipmentWeightUnit
		{
			get { return Shipment != null ? Shipment.ShipmentWeightUnit : ZString.Empty; }
		}

		protected ZString ShipmentVolumeUnit
		{
			get { return Shipment != null ? Shipment.ShipmentVolumeUnit : ZString.Empty; }
		}

		#endregion

		protected virtual void EnsurePackLineExists()
		{
			if (Count == 0 && !IsMasterRepresentingAllChildShipments)
			{
				AddNew();
			}
		}

		void RefreshPackLineTotalsOnShipment()
		{
			if (Shipment != null && !IsLoading)
			{
				RefreshPackLineTotalsOnShipmentCore();
			}
		}

		protected CommonShipment Shipment
		{
			get { return Master; }
		}

		#endregion

		#region IPackLineCollection Members

		public PackLineCollectionCalculator Totals
		{
			get { return totals ?? (totals = new PackLineCollectionCalculator(this, PackLineCollectionCalculator.FilterMode.NoFilter)); }
		}
		protected PackLineCollectionCalculator totals;

		ZString IPackLineCollection.MasterPackagesUnit
		{
			get { return ShipmentPackageUnit; }
		}

		ZString IPackLineCollection.MasterWeightUnit
		{
			get { return ShipmentWeightUnit; }
		}

		ZString IPackLineCollection.MasterVolumeUnit
		{
			get { return ShipmentVolumeUnit; }
		}

		#endregion
	}
}
