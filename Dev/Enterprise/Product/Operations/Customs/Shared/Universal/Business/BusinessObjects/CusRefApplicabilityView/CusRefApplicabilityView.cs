using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.Universal
{
	public sealed class CusRefApplicabilityView : AutoCusRefApplicabilityView
	{
		public CusRefApplicabilityView(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoCusRefApplicabilityView.Schema
		{
			public const string ExcludedTradeGroupsConcatenated = "ExcludedTradeGroupsConcatenated";
			public const string TradeGroupDescription = "TradeGroupDescription";
			public const string SecondTradeGroupDescription = "SecondTradeGroupDescription";
			public const string ZZT_TradeGroupDataGrouping = "ZZT_TradeGroupDataGrouping";
		}

		[ResourceStringData("Enterprise.Customs.Universal.CusRefApplicabilityView|ZZT_StartDate", Caption = "Start Date")]
		public override ZDateTime ZZT_StartDate { get => base.ZZT_StartDate; set => base.ZZT_StartDate = value; }

		[ResourceStringData("Enterprise.Customs.Universal.CusRefApplicabilityView|ZZT_EndDate", Caption = "End Date")]
		public override ZDateTime ZZT_EndDate { get => base.ZZT_EndDate; set => base.ZZT_EndDate = value; }

		[ResourceStringData("Enterprise.Customs.Universal.CusRefApplicabilityView|ZZT_AdditionalCode", Caption = "Additional Code")]
		public override ZString ZZT_AdditionalCode { get => base.ZZT_AdditionalCode; set => base.ZZT_AdditionalCode = value; }

		[ResourceStringData("Enterprise.Customs.Universal.CusRefApplicabilityView|ZZT_OrderNumber", Caption = "Order")]
		public override ZString ZZT_OrderNumber { get => base.ZZT_OrderNumber; set => base.ZZT_OrderNumber = value; }

		[ResourceStringData("Enterprise.Customs.Universal.CusRefApplicabilityView|ZZT_TradeGroupDataGrouping", Caption = "Country/Region or Grouping", ShortCaption = "Grouping")]
		public ZString ZZT_TradeGroupDataGrouping => TradeGroup?.ZZA_ZZZ_NKDataGrouping ?? ZString.Empty;

		public ZPropertyInfo ZZT_TradeGroupDataGroupingInfo => GetZPropertyInfo(Schema.ZZT_TradeGroupDataGrouping);

		public ZString TradeGroupCode => TradeGroup?.ZZA_TradeGroup ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.Universal.CusRefApplicabilityView|TradeGroupDescription", Caption = "Trade Group Description", ShortCaption = "Trade Group Desc.")]
		public ZString TradeGroupDescription => TradeGroup?.ZZA_Description ?? ZString.Empty;

		public ZPropertyInfo TradeGroupDescriptionInfo => GetZPropertyInfo(Schema.TradeGroupDescription);

		public CusRefTradeGroupView TradeGroup => Factory.Load<CusRefTradeGroupView>(ZZT_ZZA_TradeGroup);

		[RelatedBusinessObject(nameof(TradeGroup))]
		[List(nameof(Lookups) + "." + nameof(CusRefApplicabilityViewLookups.TradeGroupList))]
		[ResourceStringData("Enterprise.Customs.Universal.CusRefApplicabilityView|ZZT_ZZA_TradeGroup", Caption = "Trade Group")]
		public override ZGuid ZZT_ZZA_TradeGroup
		{
			get => base.ZZT_ZZA_TradeGroup;
			set => base.ZZT_ZZA_TradeGroup = value;
		}

		public ZString SecondTradeGroupCode => SecondTradeGroup?.ZZA_TradeGroup ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.Universal.CusRefApplicabilityView|SecondTradeGroupDescription", Caption = "Application Territory Description", ShortCaption = "App. Terr. Desc.")]
		public ZString SecondTradeGroupDescription => SecondTradeGroup?.ZZA_Description ?? ZString.Empty;

		public ZPropertyInfo SecondTradeGroupDescriptionInfo => GetZPropertyInfo(Schema.SecondTradeGroupDescription);

		public CusRefTradeGroupView SecondTradeGroup => Factory.Load<CusRefTradeGroupView>(ZZT_ZZA_SecondTradeGroup);

		[RelatedBusinessObject(nameof(SecondTradeGroup))]
		[List(nameof(Lookups) + "." + nameof(CusRefApplicabilityViewLookups.TradeGroupList))]
		[ResourceStringData("Enterprise.Customs.Universal.CusRefApplicabilityView|ZZT_ZZA_SecondTradeGroup", Caption = "Application Territory")]
		public override ZGuid ZZT_ZZA_SecondTradeGroup
		{
			get => base.ZZT_ZZA_SecondTradeGroup;
			set => base.ZZT_ZZA_SecondTradeGroup = value;
		}

		public RateView Rate => Factory.Load<RateView>(ZZT_ZZ2_Rate);

		[RelatedBusinessObject(nameof(Rate))]
		public override ZGuid ZZT_ZZ2_Rate
		{
			get => base.ZZT_ZZ2_Rate;
			set => base.ZZT_ZZ2_Rate = value;
		}

		[ReadOnly(true)]
		public override ZString ZZT_DataSet { get => base.ZZT_DataSet; set => base.ZZT_DataSet = value; }

		[ChildEditable]
		public RefCusExcludedTradeGroupCollection ExcludedTradeGroups
		{
			get
			{
				if (excludedTradeGroups == null)
				{
					excludedTradeGroups = new RefCusExcludedTradeGroupCollection(this);
					RegisterEditableChildObject(excludedTradeGroups);
				}
				return excludedTradeGroups;
			}
		}
		RefCusExcludedTradeGroupCollection excludedTradeGroups;

		[ResourceStringData("Enterprise.Customs.Universal.CusRefApplicabilityView|ExcludedTradeGroupsConcatenated", Caption = "Exclusions")]
		public ZString ExcludedTradeGroupsConcatenated
		{
			get
			{
				var sb = new ZStringBuilder();
				var localExclusions = ExcludedTradeGroups.OrderBy(x => x.TradeGroupDescription);
				foreach (var excludedGroup in localExclusions)
				{
					sb.AppendIfNotEmpty(excludedGroup.TradeGroupDescription);
				}
				return sb.ToStringWithDelimiterBetweenAppends("; ");
			}
		}
		public ZPropertyInfo ExcludedTradeGroupsConcatenatedInfo => GetZPropertyInfo(Schema.ExcludedTradeGroupsConcatenated);

		public ZBool IsApplicable(ZString tradeGroupCountry, ZDateTime assessmentDate)
		{
			return ZZT_StartDate <= assessmentDate
					&& ZZT_EndDate >= assessmentDate
					&& (tradeGroupCountry.IsEmpty || ApplicableTradeGroupCountries.Contains(tradeGroupCountry));
		}

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo) => !ZZT_IsSystem;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ZZT_DataSet = Core.Constants.Customs.Universal.DataSetTypes.OWNData;
		}

		IEnumerable<ZString> ApplicableTradeGroupCountries
		{
			get
			{
				var tradeGroup = TradeGroup;
				if (tradeGroup != null)
				{
					foreach (var tradeGroupCountry in tradeGroup.TradeGroupCountries)
					{
						var tradeGroupCountryCode = tradeGroupCountry.ZZB_RN_NKTradeGroupCountryCode;
						if (!tradeGroupCountryCode.IsEmpty && !IsExcludedTradeGroupCountry(tradeGroupCountryCode))
						{
							yield return tradeGroupCountryCode;
						}
					}
				}
			}
		}

		ZBool IsExcludedTradeGroupCountry(ZString tradeGroupCountryCode)
		{
			return ExcludedTradeGroups
				.Any(excludedTradeGroup => excludedTradeGroup.TradeGroup?.TradeGroupCountries.Any(z => z.ZZB_RN_NKTradeGroupCountryCode == tradeGroupCountryCode) ?? false);
		}
	}
}
