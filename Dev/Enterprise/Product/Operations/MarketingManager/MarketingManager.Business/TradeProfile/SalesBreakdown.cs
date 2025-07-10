using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class SalesBreakdown : AutoSalesBreakdown
	{
		#region Constructor

		public SalesBreakdown(OrgHeader org)
			: base(org.Factory)
		{
			Argument.NotNull(org, "org");

			this.org = org;
			SetReadOnlyIncludingChildren(org.ReadOnly);
		}

		#endregion

		public OrgHeader Org
		{
			get { return org; }
		}
		readonly OrgHeader org;

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ShowAllRevenue = true;
			ShowFinancialYearRevenue = true;
			CompanyFilter = Env.CurrentCompany.PK;
		}

		#endregion

		#region Properties

		#region Show Revenue Option

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.MarketingManager.Business.SalesBreakdown|ShowPerAnnumRevenue", Caption = "Per Annum")]
		public ZBool ShowPerAnnumRevenue
		{
			get => showPerAnnumRevenue;
			set
			{
				SetNonPersistentPropertyValue(ShowPerAnnumRevenueInfo, ref showPerAnnumRevenue, value);
				if (value)
				{
					SalesHeaders?.ForEach(x => x.RevenueDisplayOption = SalesHeader.RevenueDisplay.PerAnnum);
				}
			}
		}
		public ZPropertyInfo ShowPerAnnumRevenueInfo => GetZPropertyInfo(nameof(ShowPerAnnumRevenue));
		ZBool showPerAnnumRevenue;

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.MarketingManager.Business.SalesBreakdown|ShowFinancialYearRevenue", Caption = "Financial Year")]
		public ZBool ShowFinancialYearRevenue
		{
			get => showFinancialYearRevenue;
			set
			{
				SetNonPersistentPropertyValue(ShowFinancialYearRevenueInfo, ref showFinancialYearRevenue, value);
				if (value)
				{
					SalesHeaders?.ForEach(x => x.RevenueDisplayOption = SalesHeader.RevenueDisplay.FinancialYear);
				}
			}
		}
		public ZPropertyInfo ShowFinancialYearRevenueInfo => GetZPropertyInfo(nameof(ShowFinancialYearRevenue));
		ZBool showFinancialYearRevenue;

		#endregion

		#region Selected Product

		public OrgSalesProduct SelectedProduct { get; set; }

		#endregion

		#region Company Filter

		[List("Companies")]
		public ZGuid CompanyFilter
		{
			get => companyFilter;
			set
			{
				if (value != companyFilter)
				{
					SetNonPersistentPropertyValue(CompanyFilterInfo, ref companyFilter, value);
					SalesHeaders?.ForEach(x => x.CompanyFilter = value);
				}
			}
		}
		ZGuid companyFilter;

		public ZPropertyInfo CompanyFilterInfo => GetZPropertyInfo(nameof(CompanyFilter));

		public bool CompanyFilter_ReadOnly => !Env.Security.ClientIntelligenceViewAllCompaniesTradeProfile.IsAllowed;

		#endregion

		#region Require Refresh

		public ZBool RequireRefresh
		{
			get { return requireRefresh; }
			set { SetNonPersistentPropertyValue(RequireRefreshInfo, ref requireRefresh, value); }
		}
		ZBool requireRefresh = false;
		public ZPropertyInfo RequireRefreshInfo => GetZPropertyInfo(nameof(RequireRefresh));

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (SalesHeaders?.Any(x => x.HasChanges) ?? false)
			{
				requireRefresh = true;  //Prevent triggering ValueChanged event here
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded && RequireRefresh)
			{
				RequireRefresh = false; //Trigger ValueChanged event
			}
		}

		#endregion

		#endregion

		#region Collections

		public SalesHeaderCollection SalesHeaderCollection
		{
			get
			{
				ISalesValueAssociatedEntity entity = Org;
				return (SalesHeaderCollection)entity?.ActualAndProspectiveSalesHeaderCollection;
			}
		}

		public IEnumerable<SalesHeader> SalesHeaders
		{
			get { return SalesHeaderCollection?.Cast<SalesHeader>(); }
		}

		public GlbCompanyCollection Companies
		{
			get { return Factory.GetCachedValue("SalesBreakdown.Companies", () => { return new GlbCompanyCollection(Factory); }); }
		}

		#endregion
	}
}
