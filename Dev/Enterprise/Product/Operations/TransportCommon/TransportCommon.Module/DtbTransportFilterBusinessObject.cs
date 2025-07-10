using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.TransportCommon.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Module
{
	public abstract class DtbTransportFilterBusinessObject<T> : FilterStripBusinessObject, IAccountingFilterStripHolder
		where T : DtbTransport
	{
		protected sealed override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddAccountingFilters(filters);
			AddOrganisationFilters(filters);
			AddModuleFilters(filters);

			return filters;
		}

		protected abstract void AddModuleFilters(ModuleFilterCollection filters);

		#region AddAccountingFilters

		void AddAccountingFilters(ModuleFilterCollection result)
		{
			AccountingFilterStrip.AddBillingFilters(result);
			AccountingFilterStrip.AddJobManagementFilters(result, GetSecurityCheckPoint());
		}

		protected abstract SecurityCheckpoint GetSecurityCheckPoint();

		#endregion

		#region AddWorkflowFilters

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();

			var columnOverride = GetTransportWorkflowFilterStripsHelperSchemaColumnOverride();
			var helper = new TransportWorkflowFilterStripsHelper(typeof(T), WorkflowDescriptors.DtbBookingWorkflowDescriptorCode, Factory, columnOverride);
			helper.SetShouldAddWorkflowCustomFieldsFilters(true);

			helpers.Add(helper);

			return helpers;
		}

		protected virtual SchemaColumn GetTransportWorkflowFilterStripsHelperSchemaColumnOverride()
		{
			return null;
		}

		#endregion

		#region AddOrganisationFilters

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			AddOrganisationFiltersCore(filters);

			var branchFilter = filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, DtbBookingSchema.KM_GB_Branch, new GlbBranchCollection(Factory));
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("DtbTransportFilterBusinessObject|Branch", "Branch");
			branchFilter.SupportsFiltersMatchComparisonOperator = false;
		}

		protected virtual void AddOrganisationFiltersCore(ModuleFilterCollection filters)
		{
		}

		#endregion

		#region IAccountingFilterStripHolder Members

		#region AccountingFilterStrip

		protected IAccountingFilterStrip AccountingFilterStrip
		{
			get
			{
				if (accountingFilterStrip == null)
				{
					accountingFilterStrip = ObjectFactory.New<IAccountingFilterStrip>(this);
					accountingFilterStrip.Initialize(addRevenueFilters: IsAddRevenueFilters, addWIPAccrualHasFilters: false);
				}

				return accountingFilterStrip;
			}
		}

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => AccountingFilterStripConfigurationCore;

		protected virtual Dictionary<string, object> AccountingFilterStripConfigurationCore => new Dictionary<string, object>()
		{
			{ AccountingFilterStripConfigurationKeys.BusinessObjectType, typeof(T) },
			{ AccountingFilterStripConfigurationKeys.InvoicedChargesFilterNameOverride, ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|InvoicedChargesBilling", "Invoiced / Charges / Billing") },
			{ AccountingFilterStripConfigurationKeys.InvoicedChargesFilterOptionsSelected, InvoicedChargesFilterOptions.Default | InvoicedChargesFilterOptions.LocalBillingNotPaid },
			{ AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterNameOverride, ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|InvoiceStatus", "Invoice Status") },
		};
		protected abstract bool IsAddRevenueFilters { get; }

		IAccountingFilterStrip accountingFilterStrip;

		#endregion

		#region ParentTableCode

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		#endregion

		#region TopLevelBusinessObjectQuery

		ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(T));
			result.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region AmountFiltersCategoryNameOveride

		MultilingualString IAccountingFilterStripHolder.AmountFiltersCategoryNameOveride
		{
			get { return GetAmountFiltersCategoryNameOverideCore(); }
		}

		protected virtual MultilingualString GetAmountFiltersCategoryNameOverideCore()
		{
			return null;
		}

		#endregion

		#region BillingFiltersCategoryNameOveride

		MultilingualString IAccountingFilterStripHolder.BillingFiltersCategoryNameOveride
		{
			get { return GetBillingFiltersCategoryNameOverideCore(); }
		}

		protected virtual MultilingualString GetBillingFiltersCategoryNameOverideCore()
		{
			return null;
		}

		#endregion

		#region FilterNameSuffixInOtherCategories

		MultilingualString IAccountingFilterStripHolder.FilterNameSuffixInOtherCategories
		{
			get { return GetFilterNameSuffixInOtherCategoriesCore(); }
		}

		protected virtual MultilingualString GetFilterNameSuffixInOtherCategoriesCore()
		{
			return null;
		}

		#endregion

		#endregion
	}
}
