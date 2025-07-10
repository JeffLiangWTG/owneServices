using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Packing.Business;

namespace Enterprise.Customs.Business
{
	public class CusPackageCusPackableItemRelation : AutoCusPackageCusPackableItemRelation
	{
		public CusPackageCusPackableItemRelation(CusPackage package, CusPackableItem packableItem) : base(package.Factory)
		{
			Package = Argument.NotNull(package, nameof(package));
			PackableItem = Argument.NotNull(packableItem, nameof(packableItem));
		}

		public readonly CusPackableItem PackableItem;
		public readonly CusPackage Package;

		public static class Schema
		{
			public const string GoodsDescription = "GoodsDescription";
			public const string InvoiceLineNumber = "InvoiceLineNumber";
			public const string InvoiceNumber = "InvoiceNumber";
			public const string IsPacked = "IsPacked";
			public const string PackableQuantity = "PackableQuantity";
			public const string PackedQty = "PackedQty";
			public const string PackableUQ = "PackableUQ";
			public const string NetWeight = "NetWeight";
			public const string NetWeightUQ = "NetWeightUQ";
			public const string NotPackedQty = "NotPackedQty";
			public const string OriginalGoodsDescription = "OriginalGoodsDescription";
			public const string RelatedPacks = "RelatedPacks";
			public const string IsSplit = "IsSplit";
			public const string Sequence = "Sequence";
			public const string InvoiceLineNetWeight = nameof(CusPackageCusPackableItemRelation.InvoiceLineNetWeight);
			public const string InvoiceLineNetWeightUQ = nameof(CusPackageCusPackableItemRelation.InvoiceLineNetWeightUQ);
			public const string Grouping = nameof(CusPackageCusPackableItemRelation.Grouping);
		}

		public bool IsPackedReadOnly => NotPackedQty <= 0 && PackedQty <= 0;

		[ReadOnlyMember(nameof(IsPackedReadOnly))]
		[ResourceStringData("EE469384-24E0-4385-B17E-8DDD5B6E8074", Caption = "Packed?")]
		public ZBool IsPacked
		{
			get
			{
				return Package.IsPacked(PackableItem);
			}
			set
			{
				var hasChanged = IsPacked != value;
				if (!IsCopying && hasChanged)
				{
					if (value)
					{
						Package.CustomsPackItem(PackableItem, NotPackedQty);
					}
					else
					{
						Package.CustomsUnpackItem(PackableItem, PackedQty);
					}

					IsPackedInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo IsPackedInfo => GetZPropertyInfo(Schema.IsPacked);

		[ResourceStringData("BEA598B1-9654-4837-A724-CBA3349D674D", Caption = "Packable Qty")]
		public ZDecimal PackableQuantity
		{
			get => PackableItem.CUI_PackableQty;
			set
			{
				var oldValue = PackableItem.CUI_PackableQty;
				PackableItem.CUI_PackableQty = value;
				if (PackableItem.CUI_PackableQty != oldValue)
				{
					Package.CalculatePackageNetWeight(PackableItem);
				}
			}
		}

		public ZPropertyInfo PackableQuantityInfo => GetWrappedZPropertyInfo(Schema.PackableQuantity, x => PackableItem.CUI_PackableQtyInfo);

		[List(nameof(Lookups) + "." + nameof(CusPackageCusPackableItemRelationLookups.PackTypes))]
		[ResourceStringData("E9FBC99C-01B0-4D44-AB83-1D5A5EF9E930", Caption = "Packable Qty UQ", ShortCaption = "UQ")]
		public ZString PackableUQ
		{
			get => PackableItem.CUI_PackableUQ;
			set
			{
				var oldValue = PackableUQ;
				PackableItem.CUI_PackableUQ = value;
				if (oldValue != PackableUQ)
				{
					PackableItem.PackingList?.TotalPackedQtyInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo PackableUQInfo => GetWrappedZPropertyInfo(Schema.PackableUQ, x => PackableItem.CUI_PackableUQInfo);

		[DecimalPlaces(3)]
		[ReadOnlyMember(nameof(PackedQtyReadOnly))]
		[ResourceStringData("B868072E-5DB5-4526-B977-E4CCF441C4D0", Caption = "Net Weight")]
		public ZDecimal NetWeight
		{
			get => PackageItemDivot?.PkgNetWeight ?? ZDecimal.Zero;
			set
			{
				if (PackageItemDivot is PkgPackageItemDivot packageItemDivot)
				{
					var oldValue = PackableItem.TotalPackedNetWeight;
					packageItemDivot.PkgNetWeight = value;
					if (oldValue != PackableItem.TotalPackedNetWeight)
					{
						Package.UpdateNetWeightIfItemsChanged();
						Package.NetWeightInfo.RefreshBinding();
						PackableItem.PackingList?.TotalNetWeightInfo.RefreshBinding();
					}
					Validation.ValidateNetWeight();
				}
			}
		}

		public ZPropertyInfo NetWeightInfo => GetZPropertyInfo(Schema.NetWeight);

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(CusPackageCusPackableItemRelationLookups.WeightUQs))]
		[ReadOnlyMember(nameof(PackedQtyReadOnly))]
		[ResourceStringData("C263A5FA-4D18-4726-9F73-1106ABCBB614", Caption = "Net Weight UQ", ShortCaption = "UQ")]
		public ZString NetWeightUQ
		{
			get => PackageItemDivot?.PkgNetWeightUQ ?? ZString.Empty;
			set
			{
				if (PackageItemDivot is PkgPackageItemDivot packageItemDivot)
				{
					CheckMaximumLength(NetWeightUQInfo, value);
					packageItemDivot.PkgNetWeightUQ = value;
					Validation.ValidateNetWeightUQ();
				}
			}
		}

		public ZPropertyInfo NetWeightUQInfo => GetZPropertyInfo(Schema.NetWeightUQ);

		public bool PackedQtyReadOnly => !IsPacked;

		[ReadOnlyMember(nameof(PackedQtyReadOnly))]
		[ResourceStringData("ABF5213B-A001-4AA4-9152-803F79DF9A91", Caption = "Packed Qty")]
		public ZDecimal PackedQty
		{
			get => Package.GetCustomsPackedQty(PackableItem);
			set
			{
				var packed = PackedQty;
				if (value > packed)
				{
					Package.CustomsPackItem(PackableItem, value - packed);
				}
				else if (value < packed)
				{
					Package.CustomsUnpackItem(PackableItem, packed - value);
				}

				if (PackedQty != packed)
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidatePackedQty();
					}
					PackedQtyInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo PackedQtyInfo => GetZPropertyInfo(Schema.PackedQty);

		[ResourceStringData("6FD13005-514C-4BE5-8025-CD14E793E64C", Caption = "Not Packed Qty")]
		public ZDecimal NotPackedQty => PackableItem.NotPackedQty;

		public ZDecimal TotalPackedQty => PackableItem.TotalPackedQty;

		public ZDecimal TotalPackedNetWeight => PackableItem.TotalPackedNetWeight;

		[ResourceStringData("152C9019-C377-4D97-BF22-D169714CD122", Caption = "Invoice #", ShortCaption = "Inv. #")]
		public ZString InvoiceNumber => PackableItem.CUI_InvoiceNumber;

		public ZPropertyInfo InvoiceNumberInfo => GetZPropertyInfo(Schema.InvoiceNumber);

		[ResourceStringData("1833B027-D7CF-4130-A310-BE05E7D6D923", Caption = "Invoice Line #", ShortCaption = "Inv. Line #")]
		public ZShort InvoiceLineNumber => PackableItem.CUI_InvoiceLineNumber;

		public ZPropertyInfo InvoiceLineNumberInfo => GetZPropertyInfo(Schema.InvoiceLineNumber);

		[ResourceStringData("C881B3BC-0941-4960-A150-3435E3832167", Caption = "Goods Description", ShortCaption = "Goods Desc.")]
		public ZString GoodsDescription { get => PackableItem.CUI_GoodsDescription; set => PackableItem.CUI_GoodsDescription = value; }

		public ZPropertyInfo GoodsDescriptionInfo => GetWrappedZPropertyInfo(Schema.GoodsDescription, x => PackableItem.CUI_GoodsDescriptionInfo);

		[ResourceStringData("Enterprise.Customs.Business|CusPackageCusPackableItemRelation|OriginalGoodsDescription", Caption = "Original Goods Description", ShortCaption = "Original Goods Desc.")]
		public ZString OriginalGoodsDescription => PackableItem.CUI_OriginalGoodsDescription;

		public ZPropertyInfo OriginalGoodsDescriptionInfo => GetZPropertyInfo(Schema.OriginalGoodsDescription);

		public bool Split_ReadOnly => !IsSplit;

		[ReadOnlyMember(nameof(Split_ReadOnly))]
		[ResourceStringData("645A1EDD-F9F3-4B74-BE4A-ED433315595F", Caption = "Invoice Line Net Weight", ShortCaption = "Inv. Line Net Weight")]
		public ZDecimal InvoiceLineNetWeight
		{
			get => PackableItem.CUI_NetWeight;
			set
			{
				var oldValue = PackableItem.CUI_NetWeight;
				PackableItem.CUI_NetWeight = value;
				if (PackableItem.CUI_NetWeight != oldValue)
				{
					Package.CalculatePackageNetWeight(PackableItem);
				}
			}
		}

		public ZPropertyInfo InvoiceLineNetWeightInfo => GetWrappedZPropertyInfo(Schema.InvoiceLineNetWeight, x => PackableItem.CUI_NetWeightInfo);

		[ResourceStringData("84883CEE-D683-4C40-BF7F-A6524EC667AA", Caption = "Invoice Line Net Weight UQ", ShortCaption = "UQ")]
		public ZString InvoiceLineNetWeightUQ => PackableItem.CUI_NetWeightUQ;

		public ZPropertyInfo InvoiceLineNetWeightUQInfo => GetWrappedZPropertyInfo(Schema.InvoiceLineNetWeightUQ, x => PackableItem.CUI_NetWeightUQInfo);

		[ResourceStringData("Enterprise.Customs.Business|CusPackageCusPackableItemRelation|RelatedPacks", Caption = "Related Packs")]
		public ZString RelatedPacks => PackableItem.RelatedPacks;

		public ZPropertyInfo RelatedPacksInfo => GetZPropertyInfo(Schema.RelatedPacks);

		[ResourceStringData("Enterprise.Customs.Business|CusPackageCusPackableItemRelation|IsSplit", Caption = "Is Split")]
		public ZBool IsSplit => PackableItem.IsSplit;

		public ZPropertyInfo IsSplitInfo => GetZPropertyInfo(Schema.IsSplit);

		[ResourceStringData("Enterprise.Customs.Business|CusPackageCusPackableItemRelation|Sequence", ShortCaption = "Seq.", Caption = "Sequence")]
		public ZShort Sequence { get => PackableItem.CUI_Sequence; set => PackableItem.CUI_Sequence = value; }

		public ZPropertyInfo SequenceInfo => GetWrappedZPropertyInfo(Schema.Sequence, x => PackableItem.CUI_SequenceInfo);

		[ResourceStringData("Enterprise.Customs.Business|CusPackageCusPackableItemRelation|Grouping", Caption = "Grouping")]
		public ZString Grouping { get => PackableItem.Grouping; set => PackableItem.Grouping = value; }

		public ZPropertyInfo GroupingInfo => GetWrappedZPropertyInfo(Schema.Grouping, x => PackableItem.GroupingInfo);

		internal PkgPackageItemDivot PackageItemDivot
		{
			get
			{
				if (packageItemDivot == null || packageItemDivot.IsDeleted || packageItemDivot.KI_ParentID != PackableItem.PK)
				{
					if (packageItemDivot != null)
					{
						packageItemDivot.PkgNetWeightInfo.ValueChanged -= PkgNetWeightInfo_ValueChanged;
						packageItemDivot.PkgNetWeightUQInfo.ValueChanged -= PkgNetWeightUQInfo_ValueChanged;
						UnRegisterEditableChildObject(packageItemDivot);
					}
					packageItemDivot = Package.GetDivot(PackableItem);

					if (packageItemDivot != null)
					{
						RegisterEditableChildObject(packageItemDivot);
						packageItemDivot.PkgNetWeightInfo.ValueChanged += PkgNetWeightInfo_ValueChanged;
						packageItemDivot.PkgNetWeightUQInfo.ValueChanged += PkgNetWeightUQInfo_ValueChanged;
					}
				}
				return packageItemDivot;
			}
		}
		PkgPackageItemDivot packageItemDivot;

		void PkgNetWeightUQInfo_ValueChanged(object sender, System.EventArgs e)
		{
			if (e is ValueChangedEventArgs v)
			{
				NetWeightUQInfo.RefreshBinding(v.OldValue);
			}
			else
			{
				NetWeightUQInfo.RefreshBinding();
			}
		}

		void PkgNetWeightInfo_ValueChanged(object sender, System.EventArgs e)
		{
			NetWeightInfo.RefreshBinding();
			if (e is ValueChangedEventArgs v)
			{
				NetWeightInfo.RefreshBinding(v.OldValue);
			}
			else
			{
				NetWeightInfo.RefreshBinding();
			}
		}

		#region Lookups

		public CusPackageCusPackableItemRelationLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = GetNewLookups();
				}

				return fLookups;
			}
		}

		protected virtual CusPackageCusPackableItemRelationLookups GetNewLookups()
		{
			return new CusPackageCusPackableItemRelationLookups(this);
		}

		CusPackageCusPackableItemRelationLookups fLookups;

		#endregion
	}
}
