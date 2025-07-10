using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal;

public sealed class RefHarbourRate : AutoRefHarbourRate
{
	public RefHarbourRate(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	[ResourceStringData("Enterprise.Customs.Universal.ZZRefHarbourRateCombined|ZXF_Mode", Caption = "Mode")]
	[List(nameof(Lookups) + "." + nameof(RefHarbourRateLookups.ModeList))]
	public override ZString ZXF_Mode
	{
		get { return base.ZXF_Mode; }
		set { base.ZXF_Mode = value; }
	}

	[ResourceStringData("Enterprise.Customs.Universal.ZZRefHarbourRateCombined|ZXF_Type", Caption = "Type")]
	public override ZString ZXF_Type
	{
		get { return base.ZXF_Type; }
		set { base.ZXF_Type = value; }
	}

	[ResourceStringData("Enterprise.Customs.Universal.ZZRefHarbourRateCombined|ZXF_Port", Caption = "Port")]
	public override ZString ZXF_Port
	{
		get { return base.ZXF_Port; }
		set { base.ZXF_Port = value; }
	}

	[ResourceStringData("Enterprise.Customs.Universal.ZZRefHarbourRateCombined|ZXF_Commodity", Caption = "Commodity")]
	[ReadOnly(true)]
	public override ZString ZXF_Commodity
	{
		get { return base.ZXF_Commodity; }
		set { base.ZXF_Commodity = value; }
	}

	[ResourceStringData("Enterprise.Customs.Universal.ZZRefHarbourRateCombined|ZXF_PortTaxType", Caption = "Port Tax Type")]
	[ReadOnly(true)]
	public override ZString ZXF_PortTaxType
	{
		get { return base.ZXF_PortTaxType; }
		set { base.ZXF_PortTaxType = value; }
	}

	[ResourceStringData("Enterprise.Customs.Universal.ZZRefHarbourRateCombined|ZXF_RateFormula", Caption = "Rate Formula")]
	public override ZString ZXF_RateFormula
	{
		get { return base.ZXF_RateFormula; }
		set { base.ZXF_RateFormula = value; }
	}

	[ResourceStringData("Enterprise.Customs.Universal.ZZRefHarbourRateCombined|ZXF_StartDate", Caption = "Start Date")]
	public override ZDate ZXF_StartDate
	{
		get { return base.ZXF_StartDate; }
		set { base.ZXF_StartDate = value; }
	}

	[ResourceStringData("Enterprise.Customs.Universal.ZZRefHarbourRateCombined|ZXF_EndDate", Caption = "End Date")]
	public override ZDate ZXF_EndDate
	{
		get { return base.ZXF_EndDate; }
		set { base.ZXF_EndDate = value; }
	}

	[ResourceStringData("Enterprise.Customs.Universal.ZZRefHarbourRateCombined|ZXF_CountryOrGrouping", Caption = "Country/Region or Grouping", ShortCaption = "Grouping")]
	[List(nameof(Lookups) + "." + nameof(RefHarbourRateLookups.DataGroupingList))]
	[ReadOnly(true)]
	[RelatedBusinessObject("DataGrouping")]
	public override ZString ZXF_ZZZ_NKDataGrouping
	{
		get { return base.ZXF_ZZZ_NKDataGrouping; }
		set { base.ZXF_ZZZ_NKDataGrouping = value; }
	}

	public RefDataGrouping DataGrouping
	{
		get { return Factory.LoadFromNaturalKey<RefDataGrouping>(RefDataGroupingSchema.ZZZ_DataGrouping, ZXF_ZZZ_NKDataGrouping); }
	}

	protected override ZString HumanReadableNameCore => Res.GetString("9DD6C146-754F-4B32-95E0-08CEED0EF637", "Global Harbor Rate - Port: {0}", ZXF_Port);

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
	public new class Loader : AutoRefHarbourRate.Loader
	{
		public Loader(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override Type GetTypeOfBusinessObjectToLoad()
		{
			return typeof(RefHarbourRate);
		}

		public RefHarbourRate[] Load(ZString messageType, ZString dataGrouping, ZString containerMode, ZDateTime valuationDate, ZString port, ZBool? isContainerised = null, ZString? commodity = null)
		{
			if (!valuationDate.IsValid)
			{
				return Array.Empty<RefHarbourRate>();
			}

			var effectiveCommodity = commodity ?? ZString.Empty;
			var cachedKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}_{3}_{4}_{5}__ZZRefCusHarbourRate", messageType, dataGrouping, containerMode, valuationDate, port, effectiveCommodity);
			if (isContainerised != null)
			{
				cachedKey += isContainerised.Value.ToString();
			}

			return Factory.GetCachedValue(cachedKey, () =>
			{
				var query = new ZQuery();
				query.AddToFilter(RefHarbourRateSchema.ZXF_StartDate, SQLComparisonOperator.LessThanOrEqualTo, valuationDate);
				query.AddToFilter(RefHarbourRateSchema.ZXF_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, valuationDate);

				if (!messageType.IsEmpty)
				{
					query.AddToFilter(RefHarbourRateSchema.ZXF_Type, messageType);
				}

				if (!dataGrouping.IsEmpty)
				{
					query.AddToFilter(RefHarbourRateSchema.ZXF_ZZZ_NKDataGrouping, dataGrouping);
				}
				if (!port.IsEmpty)
				{
					query.AddToFilter(RefHarbourRateSchema.ZXF_Port, port);
				}

				if (!effectiveCommodity.IsEmpty)
				{
					query.AddToFilter(RefHarbourRateSchema.ZXF_Commodity, effectiveCommodity);
				}

				var harbourRates = Factory.Load<RefHarbourRate>(query);

				if (!containerMode.IsEmpty)
				{
					if (isContainerised == null)
					{
						isContainerised = Core.Constants.ContainerModes.IsContainerised(containerMode);
					}
					harbourRates = harbourRates.Where(x => (x.ZXF_Mode == Core.Constants.ContainerModes.AgentConsol && isContainerised.Value)
															|| x.ZXF_Mode == containerMode).ToArray();
				}

				return harbourRates;
			});
		}
	}
}
