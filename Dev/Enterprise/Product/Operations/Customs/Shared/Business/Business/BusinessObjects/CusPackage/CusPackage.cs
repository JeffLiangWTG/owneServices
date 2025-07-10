using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Integration.Packing;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	[UserDefinedValues]
	public class CusPackage : PkgPackage,
		Integration.Customs.ICusPackage,
		ICanDelete,
		IPackageWeightCalculationFieldSettingSupporter
	{
		public CusPackage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : PkgPackage.Schema
		{
			public const string NetWeight = "NetWeight";
			public const string UnitGrossWeight = "UnitGrossWeight";
			public const string UnitNetWeight = "UnitNetWeight";
			public const string CustomAttribute1 = "CustomAttribute1";
			public const string CustomAttribute2 = "CustomAttribute2";
			public const string CustomFlag1 = "CustomFlag1";
			public const string CustomFlag2 = "CustomFlag2";
			public const string CustomDate1 = "CustomDate1";
			public const string CustomDate2 = "CustomDate2";
			public const string CustomDecimal1 = "CustomDecimal1";
			public const string CustomDecimal2 = "CustomDecimal2";
		}

		public static new readonly CusPackageTypeDecider TypeDecider = new CusPackageTypeDecider();

		protected override PkgPackageValidation GetNewValidationCoreForUnfinalisedPackageJob() => new CusPackageValidationForUnfinalisedPackageJob(this);

		protected override PkgPackageCollection GetPackageCollectionCore() => new CusPackageCollection(this);

		public CusPackageJob CusPackageJob => Factory.Load<CusPackageJob>(KP_KJ_ParentPackageJob);

		public CusPackingList PackingList => CusPackageJob?.PackingList;

		protected override PkgPackageLookups GetNewLookups()
		{
			return new CusPackageLookups(this);
		}

		[DecimalPlaces(3)]
		[ResourceStringData("Enterprise.Customs.Business.CusPackage|NetWeight", Caption = "Net Weight")]
		public ZDecimal NetWeight
		{
			get
			{
				if (netWeight == null)
				{
					netWeight = CalculateNetWeight();
				}
				return Math.Max(0m, netWeight.Value);
			}
			set
			{
				if (IsSettingNetWeight)
				{
					ErrorReporter.ReportOnce("CusPackage.NetWeight_Set",
						"Called NetWeight from within itself" + System.Environment.NewLine +
						"Original Value = " + NetWeight.ToString(3) + " with PK = " + PK + System.Environment.NewLine +
						"New Value = " + value.ToString(3));
				}
				else
				{
					using (GetNewPackageWeightCalculationFieldSettingSupporter(PackageWeightCalculationFieldSettingType.NetWeight))
					{
						var oldValue = netWeight;
						netWeight = value;
						if (!IsCopying && oldValue != value)
						{
							UpdateGrossWeightOrTareWeightIfChangedFromNetWeightChanges(value);
							UpdateUnitNetWeightIfNetWeightOrPkgQtyChanged();
							if (!IsValidationSuspended && Validation is CusPackageValidationForUnfinalisedPackageJob validation)
							{
								validation.ValidateNetWeight();
							}
							NetWeightInfo.RefreshBinding(oldValue);
							PackingList?.TotalNetWeightInfo.RefreshBinding();
						}
					}
				}
			}
		}
		ZDecimal? netWeight;

		public ZPropertyInfo NetWeightInfo => GetZPropertyInfo(Schema.NetWeight);

		public override ZDecimal KP_Weight
		{
			get { return base.KP_Weight; }
			set
			{
				if (IsSettingKPWeight)
				{
					ErrorReporter.ReportOnce("CusPackage.KP_Weight_Set",
						"Called KP_Weight from within itself" + System.Environment.NewLine +
						"Original Value = " + KP_Weight.ToString(3) + " with PK = " + PK + System.Environment.NewLine +
						"New Value = " + value.ToString(3));
				}
				else
				{
					using (GetNewPackageWeightCalculationFieldSettingSupporter(PackageWeightCalculationFieldSettingType.KP_Weight))
					{
						var oldValue = KP_Weight;
						var valueToSet = Math.Max(0m, value);
						base.KP_Weight = valueToSet;
						if (!IsCopying && oldValue != KP_Weight)
						{
							UpdateUnitGrossWeightIfGrossWeightOrPkgQtyChanged();
							UpdateNetWeightOrTareWeightIfGrossWeightChanged();
							PackingList?.TotalGrossWeightInfo.RefreshBinding();
						}
					}
				}
			}
		}

		public override ZString KP_WeightUQ
		{
			get { return base.KP_WeightUQ; }
			set
			{
				base.KP_WeightUQ = value;
				PackingList?.TotalNetWeightInfo.RefreshBinding();
				PackingList?.TotalGrossWeightInfo.RefreshBinding();
			}
		}

		public ZBool NetWeightSpecifiedOnPackItems => PackedItemDivots.Cast<PkgPackageItemDivot>().Any(c => c.PkgNetWeight > 0);

		[ChildEditable]
		public CusPackageCusPackableItemRelationCollection PackableItemRelataions
		{
			get
			{
				if (packableItemRelataions == null)
				{
					packableItemRelataions = GetNewPackableItemRelataionsCollection();
					packableItemRelataions.Load();
					RegisterEditableChildObject(packableItemRelataions);
				}
				return packableItemRelataions;
			}
		}
		CusPackageCusPackableItemRelationCollection packableItemRelataions;

		protected virtual CusPackageCusPackableItemRelationCollection GetNewPackableItemRelataionsCollection() => new CusPackageCusPackableItemRelationCollection(this);

		public ZBool IsPackableItemRelataionsLoaded => packableItemRelataions != null;

		public void CustomsPackItem(CusPackableItem itemToPack, ZDecimal qtyToPack)
		{
			var divot = GetDivot(itemToPack);
			if (divot == null)
			{
				divot = PackedItemDivots.AddNew();
				divot.KI_ParentID = itemToPack.PK;
				divot.KI_ParentTableCode = itemToPack.TablePrefix;
				divot.KI_PackedQty = qtyToPack;
			}
			else
			{
				if (divot.KI_PackedQty == -qtyToPack)
				{
					PackedItemDivots.Delete(divot);
				}
				else
				{
					divot.KI_PackedQty += qtyToPack;
				}
			}
			CalculatePackageNetWeight(itemToPack);
		}

		public void CustomsUnpackItem(CusPackableItem itemToUnpack, ZDecimal qtyToUnpack)
		{
			var divot = GetDivot(itemToUnpack);
			if (divot != null)
			{
				if (divot.KI_PackedQty == qtyToUnpack)
				{
					PackedItemDivots.Delete(divot);
				}
				else
				{
					divot.KI_PackedQty -= qtyToUnpack;
				}
			}
			CalculatePackageNetWeight(itemToUnpack);
		}

		public void CalculatePackageNetWeight(CusPackableItem cusPackableItem)
		{
			var divot = GetDivot(cusPackableItem);
			if (divot != null)
			{
				divot.PkgNetWeightUQ = cusPackableItem.CUI_NetWeightUQ;
				if (cusPackableItem.CUI_PackableQty > 0)
				{
					divot.PkgNetWeight = decimal.Round(divot.KI_PackedQty / cusPackableItem.CUI_PackableQty * cusPackableItem.CUI_NetWeight, 3);
				}

				if (cusPackableItem.NotPackedQty == ZDecimal.Zero)
				{
					divot.PkgNetWeight += cusPackableItem.CUI_NetWeight - cusPackableItem.TotalPackedNetWeight;
				}
			}
			UpdateNetWeightIfItemsChanged();
		}

		public ZDecimal GetCustomsPackedQty(IPackableItem packableItem) => GetDivot(packableItem)?.KI_PackedQty ?? ZDecimal.Zero;

		public ZDecimal GetCustomsPackedNetWeight(IPackableItem packableItem) => GetDivot(packableItem)?.PkgNetWeight ?? ZDecimal.Zero;

		public ZString GetCustomsPackedNetWeightUQ(IPackableItem packableItem) => GetDivot(packableItem)?.PkgNetWeightUQ ?? Core.Constants.Weight.Kilograms;

		internal PkgPackageItemDivot GetDivot(IPackableItem packableItem) => PackedItemDivots.SingleOrDefault(x => x.KI_ParentID == packableItem.PK);

		protected override PkgPackageItemDivotCollection GetPackedItemDivotsCollection()
		{
			var collection = base.GetPackedItemDivotsCollection();
			collection.CollectionCountChange += PackedItemDivots_CollectionCountChange;
			collection.Cast<PkgPackageItemDivot>().ForEach(divot =>
			{
				divot.KI_PackedQtyInfo.ValueChanged -= OnPackedQtyChange;
				divot.KI_PackedQtyInfo.ValueChanged += OnPackedQtyChange;
			});
			return collection;
		}

		void PackedItemDivots_CollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.BizObject is PkgPackageItemDivot divot)
			{
				if (e.ItemAdded)
				{
					divot.KI_PackedQtyInfo.ValueChanged -= OnPackedQtyChange;
					divot.KI_PackedQtyInfo.ValueChanged += OnPackedQtyChange;
				}
				if (e.ItemRemoved)
				{
					divot.KI_PackedQtyInfo.ValueChanged -= OnPackedQtyChange;
				}
			}
			OnPackedQtyChange(sender, null);
		}

		void OnPackedQtyChange(object sender, EventArgs e)
		{
			CusPackageJob?.PackingList?.TotalPackedQtyInfo.RefreshBinding();
		}

		#region ICanDelete Members

		public override bool CanDelete
		{
			get { return CanDeleteCore; }
		}

		bool CanDeleteCore
		{
			get { return PackageJob != null && PackageJob.Packages.Count > 1; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return CanDelete ? null : ResString.GetMultilingualString("E417E816-C649-4EF2-9670-5D96B8333278", "A packing list must have at least one package"); }
		}

		#endregion

		protected override void SetWeightFromTemplate(IPackageTemplate template)
		{
			if (KP_Weight.IsEmpty)
			{
				KP_Weight = template.TareWeight;
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new CusPackageFetchStrategy(this);

		public override ZInt KP_PackageQty
		{
			get => base.KP_PackageQty;
			set
			{
				if (IsSettingPackageQty)
				{
					ErrorReporter.ReportOnce("CusPackage.KP_PackageQty_Set",
						"Called KP_PackageQty from within itself" + System.Environment.NewLine +
						"Original Value = " + KP_PackageQty.ToString() + " with PK = " + PK + System.Environment.NewLine +
						"New Value = " + value.ToString());
				}
				else
				{
					using (GetNewPackageWeightCalculationFieldSettingSupporter(PackageWeightCalculationFieldSettingType.KP_PackageQty))
					{
						var oldValue = KP_PackageQty;
						base.KP_PackageQty = value;
						if (!IsCopying && oldValue != KP_PackageQty)
						{
							UpdateUnitGrossWeightIfGrossWeightOrPkgQtyChanged();
							UpdateUnitNetWeightIfNetWeightOrPkgQtyChanged();
						}
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.Business.CusPackage|UnitGrossWeight", Caption = "Unit Gross Weight", ShortCaption = "Unit GW")]
		public ZDecimal UnitGrossWeight
		{
			get
			{
				if (unitGrossWeight == null)
				{
					unitGrossWeight = CalculateUnitGrossWeight();
				}
				return unitGrossWeight.Value;
			}
			set
			{
				using (GetNewPackageWeightCalculationFieldSettingSupporter(PackageWeightCalculationFieldSettingType.UnitGrossWeight))
				{
					var oldValue = UnitGrossWeight;
					unitGrossWeight = value;
					if (!IsCopying && oldValue != UnitGrossWeight)
					{
						UpdateGrossWeightIfChangedFromUnitGrossWeightChanges();
					}
					UnitGrossWeightInfo.RefreshBinding(oldValue);
				}
			}
		}

		ZDecimal? unitGrossWeight;

		public ZPropertyInfo UnitGrossWeightInfo => GetZPropertyInfo(Schema.UnitGrossWeight);

		[ResourceStringData("Enterprise.Customs.Business.CusPackage|UnitNetWeight", Caption = "Unit Net Weight", ShortCaption = "Unit NW")]
		public ZDecimal UnitNetWeight
		{
			get
			{
				if (unitNetWeight == null)
				{
					unitNetWeight = CalculateUnitNetWeight();
				}
				return unitNetWeight.Value;
			}
			set
			{
				using (GetNewPackageWeightCalculationFieldSettingSupporter(PackageWeightCalculationFieldSettingType.UnitNetWeight))
				{
					var oldValue = UnitNetWeight;
					unitNetWeight = value;
					if (!IsCopying && oldValue != UnitNetWeight)
					{
						UpdateNetWeightIfChangedFromUnitNetWeightChanges();
					}
					UnitNetWeightInfo.RefreshBinding(oldValue);
				}
			}
		}

		ZDecimal? unitNetWeight;

		public ZPropertyInfo UnitNetWeightInfo => GetZPropertyInfo(Schema.UnitNetWeight);

		public override ZShort KP_Sequence
		{
			get => base.KP_Sequence;
			set
			{
				if (value >= ZShort.Zero)
				{
					var oldValue = base.KP_Sequence;
					if (oldValue != value)
					{
						base.KP_Sequence = value;
						if (!IsCopying)
						{
							var sequenceFrom = value;
							if (sequenceFrom == ZShort.Zero || (oldValue != ZShort.Zero && oldValue < sequenceFrom))
							{
								sequenceFrom = oldValue;
							}

							Sequence(PackageJob?.Packages, sequenceFrom);
						}
					}
				}
			}
		}

		void Sequence(PkgPackageCollection packageCollection, ZShort sequenceFrom)
		{
			if (packageCollection != null && packageCollection.Any())
			{
				var packages = packageCollection.OrderBy(p => p.KP_Sequence).ToArray();

				for (var i = sequenceFrom - 1; i < packages.Length; i++)
				{
					var newSeq = (Int16)(i + 1);
					if (packages[i].KP_Sequence != newSeq)
					{
						packages[i].KP_Sequence = newSeq;
					}
				}
			}
		}

		bool IsSettingKPWeight => IsFieldSettingInProgress(PackageWeightCalculationFieldSettingType.KP_Weight);

		bool IsSettingPackageQty => IsFieldSettingInProgress(PackageWeightCalculationFieldSettingType.KP_PackageQty);

		bool IsSettingUnitGrossWeight => IsFieldSettingInProgress(PackageWeightCalculationFieldSettingType.UnitGrossWeight);

		bool IsSettingNetWeight => IsFieldSettingInProgress(PackageWeightCalculationFieldSettingType.NetWeight);

		bool IsSettingUnitNetWeight => IsFieldSettingInProgress(PackageWeightCalculationFieldSettingType.UnitNetWeight);

		bool IsSettingTareWeight => IsFieldSettingInProgress(PackageWeightCalculationFieldSettingType.KP_TareWeight);

		bool IsSettingDunnageWeight => IsFieldSettingInProgress(PackageWeightCalculationFieldSettingType.KP_DunnageWeight);

		void UpdateGrossWeightIfChangedFromUnitGrossWeightChanges()
		{
			if (!IsSettingKPWeight && !(IsSettingUnitGrossWeight && IsSettingPackageQty))
			{
				var newGrossWeight = CalculateTotalWeightFromUnitWeight(KP_PackageQty, UnitGrossWeight);
				if (KP_Weight != newGrossWeight)
				{
					KP_Weight = newGrossWeight;
				}
			}
		}

		void UpdateNetWeightIfChangedFromUnitNetWeightChanges()
		{
			if (!IsSettingNetWeight && !(IsSettingUnitNetWeight && IsSettingPackageQty))
			{
				var newNetWeight = CalculateTotalWeightFromUnitWeight(KP_PackageQty, UnitNetWeight);
				if (NetWeight != newNetWeight)
				{
					NetWeight = newNetWeight;
				}
			}
		}

		ZDecimal CalculateUnitNetWeight()
		{
			return CalculateUnitWeight(NetWeight, KP_PackageQty);
		}

		ZDecimal CalculateUnitGrossWeight()
		{
			return CalculateUnitWeight(KP_Weight, KP_PackageQty);
		}

		ZDecimal CalculateUnitWeight(ZDecimal totalWeight, ZInt quantity)
		{
			return totalWeight == 0m || quantity == 0 ? 0m : decimal.Round(totalWeight / quantity, 3);
		}

		ZDecimal CalculateTotalWeightFromUnitWeight(ZInt packageQty, ZDecimal unitWeight)
		{
			return decimal.Round(packageQty * unitWeight, 3);
		}

		public ZDecimal CalculateNetWeight()
		{
			var result = ZDecimal.Zero;
			if (PackedItemDivots.Count > 0)
			{
				result = CalculateAllPackedItemDivotsNetWeight();
			}

			if (result.IsEmpty)
			{
				result = KP_Weight - KP_TareWeight - KP_DunnageWeight;
			}

			return result;
		}

		public void UpdateNetWeightIfItemsChanged()
		{
			var newNetWeight = CalculateAllPackedItemDivotsNetWeight();
			if (NetWeight != newNetWeight)
			{
				NetWeight = newNetWeight;
			}
		}

		public ZDecimal CalculateAllPackedItemDivotsNetWeight() => PackedItemDivots.Cast<PkgPackageItemDivot>().Sum(divot => divot.PkgNetWeight);

		void UpdateUnitGrossWeightIfGrossWeightOrPkgQtyChanged()
		{
			if (!IsSettingUnitGrossWeight)
			{
				var newUnitGrossWeight = CalculateUnitGrossWeight();
				if (UnitGrossWeight != newUnitGrossWeight)
				{
					UnitGrossWeight = newUnitGrossWeight;
				}
			}
		}

		void UpdateUnitNetWeightIfNetWeightOrPkgQtyChanged()
		{
			if (!IsSettingUnitNetWeight)
			{
				var newUnitNetWeight = CalculateUnitNetWeight();
				if (UnitNetWeight != newUnitNetWeight)
				{
					UnitNetWeight = newUnitNetWeight;
				}
			}
		}

		void UpdateNetWeightOrTareWeightIfGrossWeightChanged()
		{
			if (NetWeight.IsEmpty && !IsSettingNetWeight)
			{
				NetWeight = CalculateNetWeight();
			}
			else if (!NetWeight.IsEmpty && !IsSettingTareWeight)
			{
				KP_TareWeight = KP_Weight - NetWeight - KP_DunnageWeight;
			}
		}

		void UpdateGrossWeightOrTareWeightIfChangedFromNetWeightChanges(ZDecimal netWeight)
		{
			if (KP_Weight.IsEmpty && !IsSettingKPWeight)
			{
				KP_Weight = netWeight + KP_TareWeight + KP_DunnageWeight;
			}
			else if (!KP_Weight.IsEmpty && !IsSettingTareWeight)
			{
				KP_TareWeight = KP_Weight - netWeight - KP_DunnageWeight;
			}
		}

		void UpdateGrossWeightOrNetWeightIfTareWeightChanged()
		{
			if (NetWeight.IsEmpty && !IsSettingNetWeight)
			{
				NetWeight = KP_Weight - KP_TareWeight - KP_DunnageWeight;
			}
			else if (!NetWeight.IsEmpty && !IsSettingKPWeight)
			{
				KP_Weight = NetWeight + KP_TareWeight + KP_DunnageWeight;
			}
		}

		bool IsFieldSettingInProgress(object type)
		{
			return FieldSettingTypes.TryGetValue(type, out var index) && index > 0;
		}

		Dictionary<object, int> FieldSettingTypes => fieldSettingTypes ?? (fieldSettingTypes = new Dictionary<object, int>());
		Dictionary<object, int> fieldSettingTypes;

		#region IPackageWeightCalculationFieldSettingSupporter
		void IPackageWeightCalculationFieldSettingSupporter.Start(object type)
		{
			if (type != null)
			{
				if (!FieldSettingTypes.ContainsKey(type))
				{
					FieldSettingTypes.Add(type, 0);
				}
				FieldSettingTypes[type]++;
			}
		}

		void IPackageWeightCalculationFieldSettingSupporter.Stop(object type)
		{
			if (type != null)
			{
				if (FieldSettingTypes.ContainsKey(type))
				{
					FieldSettingTypes[type]--;
				}
			}
		}

		public IDisposable GetNewPackageWeightCalculationFieldSettingSupporter(object type)
		{
			return new PackageWeightCalculationFieldSettingSupporter(this, type);
		}
		#endregion

		protected override bool ForceUpdateWeightIfPackageQtyChanged => false;

		[ResourceStringData("0C4C138E-696C-4A6C-87A9-5481C7149E49", Caption = "Dunnage Weight")]
		public override ZDecimal KP_DunnageWeight
		{
			get => base.KP_DunnageWeight;
			set
			{
				if (IsSettingDunnageWeight)
				{
					ErrorReporter.ReportOnce("CusPackage.KP_DunnageWeight",
						"Called KP_DunnageWeight from within itself" + System.Environment.NewLine +
						"Original Value = " + KP_DunnageWeight.ToString(3) + " with PK = " + PK + System.Environment.NewLine +
						"New Value = " + value.ToString(3));
				}
				else
				{
					using (GetNewPackageWeightCalculationFieldSettingSupporter(PackageWeightCalculationFieldSettingType.KP_DunnageWeight))
					{
						var oldValue = KP_DunnageWeight;
						var valueToSet = Math.Max(0m, value);
						base.KP_DunnageWeight = valueToSet;
						if (!IsCopying && oldValue != KP_DunnageWeight)
						{
							PackingList?.TotalGrossWeightInfo.RefreshBinding();
						}
					}
				}
			}
		}

		[ResourceStringData("4262E023-0683-4269-AC30-9D53DC05FA12", Caption = "Tare Weight")]
		public override ZDecimal KP_TareWeight
		{
			get => base.KP_TareWeight;
			set
			{
				if (IsSettingTareWeight)
				{
					ErrorReporter.ReportOnce("CusPackage.KP_TareWeight_Set",
						"Called KP_TareWeight from within itself" + System.Environment.NewLine +
						"Original Value = " + KP_TareWeight.ToString(3) + " with PK = " + PK + System.Environment.NewLine +
						"New Value = " + value.ToString(3));
				}
				else
				{
					using (GetNewPackageWeightCalculationFieldSettingSupporter(PackageWeightCalculationFieldSettingType.KP_TareWeight))
					{
						var oldValue = KP_TareWeight;
						var valueToSet = Math.Max(0m, value);
						base.KP_TareWeight = valueToSet;
						if (!IsCopying && oldValue != KP_TareWeight)
						{
							PackingList?.TotalGrossWeightInfo.RefreshBinding();
						}
					}
				}
			}
		}

		protected override void AfterKP_TareWeightSet(ZDecimal oldValue)
		{
			if (oldValue != KP_TareWeight)
			{
				UpdateGrossWeightOrNetWeightIfTareWeightChanged();
			}
		}

		#region CustomLabels Properties
		[ResourceStringData("Enterprise.Customs.Business.CusPackage|CustomAttribute1", Caption = "Custom Attribute 1")]
		[MaxLength(GenAddOnColumn.Schema.XA_DataMaxLength)]
		public ZString CustomAttribute1
		{
			get => this.GetUserDefinedValue<ZString>(Schema.CustomAttribute1);
			set
			{
				var oldValue = CustomAttribute1;
				this.SetUserDefinedValue(Schema.CustomAttribute1, value);
				if (!IsValidationSuspended)
				{
					if (Validation is CusPackageValidationForUnfinalisedPackageJob validation)
					{
						validation.ValidateCustomAttribute1();
					}
				}
				CustomAttribute1Info.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CustomAttribute1Info => GetZPropertyInfo(Schema.CustomAttribute1);

		[MaxLength(GenAddOnColumn.Schema.XA_DataMaxLength)]
		[ResourceStringData("Enterprise.Customs.Business.CusPackage|CustomAttribute2", Caption = "Custom Attribute 2")]
		public ZString CustomAttribute2
		{
			get => this.GetUserDefinedValue<ZString>(Schema.CustomAttribute2);
			set
			{
				var oldValue = CustomAttribute2;
				this.SetUserDefinedValue(Schema.CustomAttribute2, value);
				if (!IsValidationSuspended)
				{
					if (Validation is CusPackageValidationForUnfinalisedPackageJob validation)
					{
						validation.ValidateCustomAttribute2();
					}
				}
				CustomAttribute2Info.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CustomAttribute2Info => GetZPropertyInfo(Schema.CustomAttribute2);

		[ResourceStringData("Enterprise.Customs.Business.CusPackage|CustomFlag1", Caption = "Custom Flag 1")]
		public ZBool CustomFlag1
		{
			get => this.GetUserDefinedValue<ZBool>(Schema.CustomFlag1);
			set
			{
				this.SetUserDefinedValue(Schema.CustomFlag1, value);
				CustomFlag1Info.RefreshBinding();
			}
		}

		public ZPropertyInfo CustomFlag1Info => GetZPropertyInfo(Schema.CustomFlag1);

		[ResourceStringData("Enterprise.Customs.Business.CusPackage|CustomFlag2", Caption = "Custom Flag 2")]
		public ZBool CustomFlag2
		{
			get => this.GetUserDefinedValue<ZBool>(Schema.CustomFlag2);
			set
			{
				this.SetUserDefinedValue(Schema.CustomFlag2, value);
				CustomFlag2Info.RefreshBinding();
			}
		}

		public ZPropertyInfo CustomFlag2Info => GetZPropertyInfo(Schema.CustomFlag2);

		[ResourceStringData("Enterprise.Customs.Business.CusPackage|CustomDate1", Caption = "Custom Date 1")]
		public ZDateTime CustomDate1
		{
			get => this.GetUserDefinedValue<ZDateTime>(Schema.CustomDate1);
			set
			{
				var oldValue = CustomDate1;
				this.SetUserDefinedValue(Schema.CustomDate1, value);
				if (!IsValidationSuspended)
				{
					if (Validation is CusPackageValidationForUnfinalisedPackageJob validation)
					{
						validation.ValidateCustomDate1();
					}
				}
				CustomDate1Info.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CustomDate1Info => GetZPropertyInfo(Schema.CustomDate1);

		[ResourceStringData("Enterprise.Customs.Business.CusPackage|CustomDate2", Caption = "Custom Date 2")]
		public ZDateTime CustomDate2
		{
			get => this.GetUserDefinedValue<ZDateTime>(Schema.CustomDate2);
			set
			{
				var oldValue = CustomDate2;
				this.SetUserDefinedValue(Schema.CustomDate2, value);
				if (!IsValidationSuspended)
				{
					if (Validation is CusPackageValidationForUnfinalisedPackageJob validation)
					{
						validation.ValidateCustomDate2();
					}
				}
				CustomDate2Info.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CustomDate2Info => GetZPropertyInfo(Schema.CustomDate2);

		[DecimalPlaces(6)]
		[ResourceStringData("Enterprise.Customs.Business.CusPackage|CustomDecimal1", Caption = "Custom Number 1")]
		public ZDecimal CustomDecimal1
		{
			get => this.GetUserDefinedValue<ZDecimal>(Schema.CustomDecimal1);
			set
			{
				var oldValue = CustomDecimal1;
				this.SetUserDefinedValue(Schema.CustomDecimal1, value);
				if (!IsValidationSuspended)
				{
					if (Validation is CusPackageValidationForUnfinalisedPackageJob validation)
					{
						validation.ValidateCustomDecimal1();
					}
				}
				CustomDecimal1Info.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CustomDecimal1Info => GetZPropertyInfo(Schema.CustomDecimal1);

		[DecimalPlaces(6)]
		[ResourceStringData("Enterprise.Customs.Business.CusPackage|CustomDecimal2", Caption = "Custom Number 2")]
		public ZDecimal CustomDecimal2
		{
			get => this.GetUserDefinedValue<ZDecimal>(Schema.CustomDecimal2);
			set
			{
				var oldValue = CustomDecimal2;
				this.SetUserDefinedValue(Schema.CustomDecimal2, value);
				if (!IsValidationSuspended)
				{
					if (Validation is CusPackageValidationForUnfinalisedPackageJob validation)
					{
						validation.ValidateCustomDecimal2();
					}
				}
				CustomDecimal2Info.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CustomDecimal2Info => GetZPropertyInfo(Schema.CustomDecimal2);
		#endregion
	}
}
