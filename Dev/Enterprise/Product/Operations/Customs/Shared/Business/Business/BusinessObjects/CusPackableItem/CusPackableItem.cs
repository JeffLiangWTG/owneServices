using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusPackableItem : AutoCusPackableItem, IClusterKeyWorker, IPackableItem, IShortSequenceNumberLine
	{
		public new class Schema : AutoCusPackableItem.Schema
		{
			public const string CUI_InvoiceNumber = "CUI_InvoiceNumber";
			public const string CUI_InvoiceLineNumber = "CUI_InvoiceLineNumber";
			public const string Grouping = "Grouping";
		}

		public CusPackableItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static readonly CusPackableItemTypeDecider TypeDecider = new CusPackableItemTypeDecider();

		[MaxLength(4000)]
		public ZString Grouping
		{
			get
			{
				if (fGrouping == null)
				{
					fGrouping = new HiddenTextNote(this, PredefinedNoteTypes.Instance.PackingListItemGrouping.Description);
				}

				return fGrouping.Text;
			}
			set
			{
				var oldValue = Grouping;
				CheckMaximumLength(GroupingInfo, value);
				if (oldValue != value)
				{
					if (fGrouping == null)
					{
						fGrouping = new HiddenTextNote(this, PredefinedNoteTypes.Instance.PackingListItemGrouping.Description);
					}
					fGrouping.SetNoteText(this, GroupingInfo, value);
				}
			}
		}

		public ZPropertyInfo GroupingInfo => GetZPropertyInfo(nameof(Grouping));

		HiddenTextNote fGrouping;

		[RelatedBusinessObject("InvoiceLine")]
		public override ZGuid CUI_JI { get => base.CUI_JI; set => base.CUI_JI = value; }

		public override ZShort CUI_Sequence
		{
			get => base.CUI_Sequence;
			set
			{
				if (value > 0)
				{
					ZShort oldValue = CUI_Sequence;
					base.CUI_Sequence = value;
					if (!IsCopying)
					{
						PackingList?.PackableItemSequenceNumberGenerator.RecalculateWhenRenumbered(this, oldValue);
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusPackableItemLookups.PackTypes))]
		public override ZString CUI_PackableUQ { get => base.CUI_PackableUQ; set => base.CUI_PackableUQ = value; }

		public BaseJobComInvoiceLine InvoiceLine => Factory.Load<BaseJobComInvoiceLine>(CUI_JI);

		public ZString CUI_InvoiceNumber => InvoiceLine?.JI_Calc_Invoice ?? ZString.Empty;

		public ZShort CUI_InvoiceLineNumber => InvoiceLine?.JI_LineNo ?? ZShort.Zero;

		public ZString CUI_OriginalGoodsDescription => InvoiceLine?.GetPackableItemGoodsDescription() ?? ZString.Empty;

		public ZDecimal NotPackedQty => CUI_PackableQty - TotalPackedQty;

		public ZDecimal TotalPackedQty
		{
			get
			{
				if (totalPackedQty == null)
				{
					totalPackedQty = new CachedProperty<ZDecimal>(Factory, () =>
					{
						return PackingList?.PackageJob?.Packages.Cast<CusPackage>().Sum(x => x.GetCustomsPackedQty(this)) ?? 0;
					});
				}
				return totalPackedQty.Value;
			}
		}
		CachedProperty<ZDecimal> totalPackedQty;

		public ZDecimal TotalPackedNetWeight
		{
			get
			{
				if (totalPackedNetWeight == null)
				{
					totalPackedNetWeight = new CachedProperty<ZDecimal>(Factory, () =>
					{
						return PackingList?.PackageJob?.Packages.Cast<CusPackage>().Sum(x => Core.Constants.Weight.ConvertSafe(x.GetCustomsPackedNetWeight(this), x.GetCustomsPackedNetWeightUQ(this), CUI_NetWeightUQ)) ?? ZDecimal.Zero;
					});
				}
				return totalPackedNetWeight.Value;
			}
		}
		CachedProperty<ZDecimal> totalPackedNetWeight;

		[RelatedBusinessObject("PackingList")]
		public override ZGuid CUI_CUL
		{
			get => base.CUI_CUL;
			set
			{
				var oldValue = CUI_CUL;
				var oldPackingList = PackingList;
				base.CUI_CUL = value;
				if (oldValue != CUI_CUL)
				{
					if (oldPackingList != null)
					{
						oldPackingList.PackableItemSequenceNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
					}

					var packingList = PackingList;
					if (packingList != null)
					{
						packingList.PackableItemSequenceNumberGenerator.RecalculateWhenAdded(this);
					}
				}
			}
		}

		public CusPackingList PackingList => Factory.Load<CusPackingList>(CUI_CUL);

		internal ZString RelatedPacks
		{
			get
			{
				if (relatedPacksCached == null)
				{
					relatedPacksCached = new CachedProperty<ZString>(Factory, () =>
					{
						var result = ZString.Empty;
						var query = new ZQuery(PkgPackageItemDivotSchema.KI_ParentID, PK);
						query.FetchOnlyFromLocalCache = !IsInDatabase;
						var packageItemDivots = Factory.Load<PkgPackageItemDivot>(query);
						if (packageItemDivots.Any())
						{
							var packagePKs = packageItemDivots.Select(x => x.KI_KP_Package);
							var packages = PackingList.PackageJob.Packages.Cast<PkgPackage>().Where(x => packagePKs.Contains(x.PK));
							if (packages.Any())
							{
								result = string.Join(",", packages.GroupBy(x => x.KP_MarksAndNumbers).Select(g => g.First().KP_MarksAndNumbers));
							}
						}
						return result;
					});
				}
				return relatedPacksCached.Value;
			}
		}
		CachedProperty<ZString> relatedPacksCached;

		internal ZBool IsSplit
		{
			get
			{
				if (isSplitCached == null)
				{
					isSplitCached = new CachedProperty<ZBool>(Factory, () => PackingList?.PackableItems.Any(x => x.CUI_JI == CUI_JI && x.PK != PK) ?? false);
				}
				return isSplitCached.Value;
			}
		}
		CachedProperty<ZBool> isSplitCached;

		public override void Delete()
		{
			var packingList = PackingList;
			if (packingList != null)
			{
				packingList.PackableItemSequenceNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
				DeletePackableItemRelationWhenDeletePackableItem();
			}

			this.DeleteChildren<PkgPackageItemDivot>(PkgPackageItemDivotSchema.KI_ParentID);
			base.Delete();
		}

		protected override void DeleteForDataRefresh()
		{
			DeletePackableItemRelationWhenDeletePackableItem();
			base.DeleteForDataRefresh();
		}

		void DeletePackableItemRelationWhenDeletePackableItem()
		{
			PackingList?.PackageJob?.Packages.Cast<CusPackage>()
				.Where(x => x.IsPackableItemRelataionsLoaded).Select(x => x.PackableItemRelataions)
				.ForEach(x => x.DeletePackableItemRelationByPackableItem(this));
		}

		public void ResetValuesFromInvoiceLine()
		{
			InvoiceLine?.SetDefaultValuesForNewPackableItem(this);
		}

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CUI_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(CusPackingList);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)CUI_CULInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;
		#endregion

		#region IPackableItem Members

		ZDecimal IPackableItem.Quantity => CUI_PackableQty;

		GroupingKey IPackableItem.Key => CusPackingGroupingKey.New(this);

		#region IShortSequenceNumberLine
		ZShort ISequenceNumberLine<ZShort>.SequenceNumber { get => CUI_Sequence; set => CUI_Sequence = value; }

		ZGuid ISequenceNumberLine.FKToHeader => CUI_CUL;
		#endregion

		IPackableItem IPackableItem.Split(ZDecimal qtyToSplit) => null;

		void IPackableItem.ReMerge()
		{
		}
		#endregion

		protected override bool SupportsCloneCore() => true;
	}
}
