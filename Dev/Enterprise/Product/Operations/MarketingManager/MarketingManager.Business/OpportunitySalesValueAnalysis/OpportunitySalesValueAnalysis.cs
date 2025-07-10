using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public class OpportunitySalesValueAnalysis : NonPersistentBusinessObject
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public OpportunitySalesValueAnalysis(SalesHeader salesHeader)
		{
			this.salesHeader = salesHeader;
			RegisterEditableChildObject(salesHeader);
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public OpportunitySalesValueAnalysis(OrgOpportunityValue valueItem)
		{
			this.valueItem = valueItem;
			RegisterEditableChildObject(valueItem);
		}

		readonly SalesHeader salesHeader;
		readonly OrgOpportunityValue valueItem;

		public SalesHeader SalesHeader
		{
			get { return salesHeader; }
		}

		public OrgOpportunityValue ValueItem
		{
			get { return valueItem; }
		}

		[ResourceStringData("OpportunitySalesValueAnalysis|SalesProductNameMultilingual", Caption = "Product Name")]
		public MultilingualString SalesProductNameMultilingual
		{
			get
			{
				if (salesHeader != null)
				{
					return salesHeader.SalesProduct.MP_NameMultilingual;
				}
				else
				{
					return valueItem.Lookups.ValueTypes.GetMultilingualDescriptionFromCode(valueItem.PV_RevenueType);
				}
			}
		}

		[ResourceStringData("OpportunitySalesValueAnalysis|TotalEstimatedAnnualValue", Caption = "Total Estimate Value (p.a)", ShortCaption = "Total Est Value (p.a)")]
		public ZDecimal TotalEstimatedAnnualValue => salesHeader?.TotalEstimatedAnnualValue ?? valueItem.PV_Value * 12;

		[ResourceStringData("OpportunitySalesValueAnalysis|CommittedAnnualValue", Caption = "Committed (p.a)")]
		public ZDecimal CommittedAnnualValue => salesHeader?.CommittedAnnualValue ?? ZDecimal.Zero;

		[ResourceStringData("OpportunitySalesValueAnalysis|CommittedMonthlyValue", Caption = "Committed m/avg (p.a)")]
		public ZDecimal CommittedMonthlyValue => salesHeader?.CommittedMonthlyValue ?? ZDecimal.Zero;

		[ResourceStringData("OpportunitySalesValueAnalysis|ProspectAnnualTotal", Caption = "Pipeline (p.a)")]
		public ZDecimal PipelineValue => salesHeader?.PipelineValue ?? ZDecimal.Zero;

		[ResourceStringData("OpportunitySalesValueAnalysis|UnsuccessfulAnnualValue", Caption = "Unsuccessful (p.a)")]
		public ZDecimal UnsuccessfulValue => salesHeader?.UnsuccessfulValue ?? ZDecimal.Zero;

		[ResourceStringData("OpportunitySalesValueAnalysis|TotalCurrencyCode", Caption = "Currency", ShortCaption = "Curr.")]
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public ZString TotalCurrencyCode
		{
			get
			{
				if (salesHeader != null)
				{
					return salesHeader.TotalCurrencyCode;
				}
				else
				{
					return valueItem.Opportunity != null
						? valueItem.Opportunity.P8_RX_NKEstimatedValueCurrency
						: GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				}
			}
		}

		[ResourceStringData("OpportunitySalesValueAnalysis|TotalAnnualMetricVolume", Caption = "Volume (p.a)", ShortCaption = "Volume (p.a)")]
		public ZDecimal TotalAnnualMetricVolume => salesHeader?.TotalAnnualMetricVolume ?? 0m;
	}
}
