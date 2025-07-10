using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public abstract class ExtendedDocketFilterBusinessObject : DocketFilterBusinessObject, IAccountingFilterStripHolder
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();
			if (IncludeTransportCoFilter)
			{
				var transportCompanyNameFilter = result.AddTextFilter("Transport Company Name", GetTransportCompanyNameQuery);
				transportCompanyNameFilter.MultilingualDescription = ResString.GetMultilingualString("25817022-6641-4e48-b77e-a6ca106fa475", "Transport Company Name");
				transportCompanyNameFilter.MaxLength = Math.Min(OrgHeaderSchema.OH_FullName.MaxLength, JobDocAddressSchema.E2_CompanyName.MaxLength);
				transportCompanyNameFilter.Category = FilterCategories.Organisations;
			}

			if (IncludeAccountingFilters)
			{
				AccountingFilterStrip.AddBillingFilters(result);
				AccountingFilterStrip.AddJobManagementFilters(result, JobInvoicingSecurity);
			}

			return result;
		}

		protected override ModuleGuidFilter TransportCoFilter => (ModuleGuidFilter)ModuleFilters["Transport Co"];

		protected virtual bool IncludeAccountingFilters => true;

		#endregion

		#region Lookups

		#region Docket Status Filter

		protected override ZQuery GetStatusQueryCore(ZString value)
		{
			ZQuery result;

			if (value != StatusFilterTypes.UnfinalisedFilterType)
			{
				result = new ZQuery(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.Equal, value);
				if (value == DocketStatus.Codes.Entered)
				{
					result.AddToFilter(new ZQuery(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.Equal, DocketStatus.Codes.Error), JoinCondition.Or);
				}
			}
			else
			{
				result = GetDocketStatusUnFinalisedQuery();
			}

			return result;
		}

		static ZQuery GetDocketStatusUnFinalisedQuery()
		{
			var result = new ZQuery(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, (ZString)DocketStatus.Codes.Finalised);
			result.AddToFilter(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, (ZString)DocketStatus.Codes.Cancelled);
			return result;
		}

		#endregion

		#region DocketStatuses

		protected override CodeDescriptionPairList GetDocketStatusCore()
		{
			var status = base.GetDocketStatusCore();
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.New));
			status.AddPair(StatusFilterTypes.UnfinalisedFilterType, Res.GetString("aef3e149-3481-4934-b9f4-0009e09bfbcb", "Un-finalized"));
			return status;
		}

		#endregion

		#endregion

		#region IAccountingFilterStripHolder Members

		protected IAccountingFilterStrip AccountingFilterStrip
		{
			get
			{
				if (accountingFilterStrip == null)
				{
					accountingFilterStrip = (IAccountingFilterStrip)Activator.CreateInstance(ObjectFactory.GetType<IAccountingFilterStrip>(), this);
					accountingFilterStrip.Initialize(addProfitLossReasonFilters: true);
				}

				return accountingFilterStrip;
			}
		}

		IAccountingFilterStrip accountingFilterStrip;

		ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
		{
			var docketQuery = new ZDBOnlyQuery(typeof(WhsDocket));
			docketQuery.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return docketQuery;
		}

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => null;

		protected abstract SecurityCheckpoint JobInvoicingSecurity { get; }

		MultilingualString IAccountingFilterStripHolder.AmountFiltersCategoryNameOveride => null;

		MultilingualString IAccountingFilterStripHolder.BillingFiltersCategoryNameOveride => null;

		MultilingualString IAccountingFilterStripHolder.FilterNameSuffixInOtherCategories => null;

		#endregion
	}
}
