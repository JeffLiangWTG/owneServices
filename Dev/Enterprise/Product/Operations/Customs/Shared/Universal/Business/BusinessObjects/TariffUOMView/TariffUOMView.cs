using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	[CodeProperty(TariffUOMViewSchema.Constants.ZZ8_UOM), DescriptionProperty(TariffUOMViewSchema.Constants.ZZ8_UOM)]
	public sealed class TariffUOMView : AutoTariffUOMView, ITariffDataGroupingRelatedBusinessObject
	{
		public TariffUOMView(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public new class Schema : AutoTariffUOMView.Schema
		{
			public const string TradeGroupDescription = nameof(TradeGroupDescription);
		}

		[RelatedBusinessObject("CusTariff")]
		public override ZGuid ZZ8_ZZ1_ParentTariffOrNationalCode
		{
			get { return base.ZZ8_ZZ1_ParentTariffOrNationalCode; }
			set { base.ZZ8_ZZ1_ParentTariffOrNationalCode = value; }
		}

		public TariffView CusTariff => Factory.Load<TariffView>(ZZ8_ZZ1_ParentTariffOrNationalCode);

		[RelatedBusinessObject("CusTradeGroup")]
		public override ZGuid ZZ8_ZZA_TradeGroup
		{
			get { return base.ZZ8_ZZA_TradeGroup; }
			set { base.ZZ8_ZZA_TradeGroup = value; }
		}

		public CusRefTradeGroupView CusTradeGroup => Factory.Load<CusRefTradeGroupView>(ZZ8_ZZA_TradeGroup);

		[ResourceStringData("Enterprise.Customs.Universal.TariffUOMView|ZZ8_Type", Caption = "Type")]
		[List(nameof(Lookups) + "." + nameof(TariffUOMViewLookups.TypeList))]
		public override ZString ZZ8_Type { get => base.ZZ8_Type; set => base.ZZ8_Type = value; }

		[ResourceStringData("Enterprise.Customs.Universal.TariffUOMView|ZZ8_UOM", Caption = "Unit")]
		[List(nameof(Lookups) + "." + nameof(TariffUOMViewLookups.UOMList))]
		public override ZString ZZ8_UOM { get => base.ZZ8_UOM; set => base.ZZ8_UOM = value; }

		[ReadOnly(true)]
		public override ZString ZZ8_DataSet { get => base.ZZ8_DataSet; set => base.ZZ8_DataSet = value; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.Universal.TariffUOMView|ZZ8_IsSystem", Caption = "Is System Defined")]
		public override ZBool ZZ8_IsSystem { get => base.ZZ8_IsSystem; set => base.ZZ8_IsSystem = value; }

		[ResourceStringData("Enterprise.Customs.Universal.TariffUOMView|TradeGroupDescription", Caption = "Group Description.", ShortCaption = "Gr. Desc.")]
		public ZString TradeGroupDescription
		{
			get
			{
				if (tradeGroupDescriptionCached == null)
				{
					tradeGroupDescriptionCached = new CachedProperty<ZString>(Factory, () => CusTradeGroup?.ZZA_Description ?? ZString.Empty);
				}
				return tradeGroupDescriptionCached.Value;
			}
		}
		CachedProperty<ZString> tradeGroupDescriptionCached;

		public ZPropertyInfo ZZ8_TradeGroupDescriptionInfo => GetZPropertyInfo(Schema.TradeGroupDescription);

		[ResourceStringData("Enterprise.Customs.Universal.TariffUOMView|ZZ8_ZZZ_NKDataGrouping", Caption = "Country/Region or Grouping", ShortCaption = "Grouping")]
		public override ZString ZZ8_ZZZ_NKDataGrouping
		{
			get => base.ZZ8_ZZZ_NKDataGrouping;
			set => base.ZZ8_ZZZ_NKDataGrouping = value;
		}
		ZString ITariffDataGroupingRelatedBusinessObject.DataGrouping => ZZ8_ZZZ_NKDataGrouping;

		public ZBool IsApplicable(ZString tradeGroupCountry)
			=> tradeGroupCountry.IsEmpty
			|| CusTradeGroup?.TradeGroupCountries
				.Select(x => x.ZZB_RN_NKTradeGroupCountryCode)
				.Contains(tradeGroupCountry) == true;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ZZ8_DataSet = Core.Constants.Customs.Universal.DataSetTypes.OWNData;
			ZZ8_ParentTableType = CusRefTariffSchema.Constants.Prefix;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoTariffUOMView.Loader
		{
			public Loader(BusinessObjectFactory factory) : base(factory)
			{
			}

			public TariffUOMView LoadTariffUomView(ZGuid tariffPK, ZString type, bool isSystem)
			{
				TariffUOMView result = null;
				if (tariffPK.IsValid && !type.IsEmpty)
				{
					var query = new ZQuery();
					query.AddToFilter(TariffUOMViewSchema.ZZ8_ZZ1_ParentTariffOrNationalCode, tariffPK);
					query.AddToFilter(TariffUOMViewSchema.ZZ8_Type, type);
					query.AddToFilter(TariffUOMViewSchema.ZZ8_IsSystem, isSystem);
					result = Factory.LoadTop1<TariffUOMView>(query);
				}
				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(TariffUOMView);
		}
	}
}
