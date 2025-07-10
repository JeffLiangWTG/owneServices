using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Module
{
	public sealed class SundryChargesFilterStrip : FilterStripBusinessObject, IAccountingFilterStripHolder
	{
		#region Descriptions

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "filter related")]
		public static class Descriptions
		{
			// Numbers & Refs
			public const string JobNumber = "Job #";

			// Dates
			public const string FromDate = "From Date";
			public const string ToDate = "To Date";

			// Organisations / Staff
			public const string BillToParty = "Bill to Party";

			// Modes And Types
			public const string Activity = "Activity";
			public const string Mode = "Mode";
			public const string Type = "Type";

			//Text Search
			public const string Description = "Description";
		}

		#endregion

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			ModuleFilter jobNumberFilter = new ModuleFountainFilter(Descriptions.JobNumber, JobSundryChargesSchema.D4_JobNumber, "SC");
			jobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|SundryChargesFilter|JobNumber", "Job #");

			return jobNumberFilter;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection collection = new ModuleFilterCollection();

			AddDateFilters(collection);
			AddOrganisationsAndStaffFilters(collection);
			AddModesAndTypesFilters(collection);
			AddTextFilters(collection);

			AccountingFilterStrip.AddBillingFilters(collection);
			AccountingFilterStrip.AddJobManagementFilters(collection, Env.Security.AgencySundryChargesJobInvoicing);

			return collection;
		}

		void AddDateFilters(ModuleFilterCollection collection)
		{
			collection.AddDateFilter(Descriptions.FromDate, JobSundryChargesSchema.D4_FromDate).MultilingualDescription = ResString.GetMultilingualString("Shipping|SundryChargesFilter|FromDate", "From Date");
			collection.AddDateFilter(Descriptions.ToDate, JobSundryChargesSchema.D4_ToDate).MultilingualDescription = ResString.GetMultilingualString("Shipping|SundryChargesFilter|ToDate", "To Date");
		}

		void AddOrganisationsAndStaffFilters(ModuleFilterCollection collection)
		{
			var filter = collection.AddGuidFilter(Descriptions.BillToParty, ModuleIDs.Organisation, JobSundryChargesSchema.D4_OH_BillToParty, new DebtorCollection(Factory));
			filter.MultilingualDescription = ResString.GetMultilingualString("Shipping|SundryChargesFilter|BillToParty", "Bill to Party");
		}

		void AddModesAndTypesFilters(ModuleFilterCollection collection)
		{
			collection.AddTextFilter(Descriptions.Activity, JobSundryChargesSchema.D4_SundryJobActivity, AgencyRegistry.Instance.SundryChargeActivities.Value).MultilingualDescription = ResString.GetMultilingualString("Shipping|SundryChargesFilter|Activity", "Activity");
			collection.AddTextFilter(Descriptions.Mode, JobSundryChargesSchema.D4_SundryJobMode, AgencyRegistry.Instance.SundryChargeModes.Value).MultilingualDescription = ResString.GetMultilingualString("Shipping|SundryChargesFilter|Mode", "Mode");
			collection.AddTextFilter(Descriptions.Type, JobSundryChargesSchema.D4_SundriesJobType, AgencyRegistry.Instance.SundryChargeTypes.Value).MultilingualDescription = ResString.GetMultilingualString("Shipping|SundryChargesFilter|Type", "Type");
		}

		void AddTextFilters(ModuleFilterCollection collection)
		{
			collection.AddTextFilter(Descriptions.Description, JobSundryChargesSchema.D4_SundriesDescription).MultilingualDescription = ResString.GetMultilingualString("Shipping|SundryChargesFilter|Description", "Description");
		}

		#region IAccountingFilterStripHolder Members

		internal IAccountingFilterStrip AccountingFilterStrip
		{
			get
			{
				if (AccountingFilterStrip_innerValue == null)
				{
					AccountingFilterStrip_innerValue = (IAccountingFilterStrip)Activator.CreateInstance(ObjectFactory.GetType<IAccountingFilterStrip>(), this);
					AccountingFilterStrip_innerValue.Initialize(addProfitLossReasonFilters: true);
				}

				return AccountingFilterStrip_innerValue;
			}
		}
		IAccountingFilterStrip AccountingFilterStrip_innerValue;

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => new Dictionary<string, object>()
		{
			{ AccountingFilterStripConfigurationKeys.BusinessObjectType, typeof(SundryCharges) },
			{ AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterNameOverride, ResString.GetMultilingualString("Shipping|SundryChargesFilter|InvoiceStatus", "Invoice Status") }
		};

		ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(SundryCharges));
			result.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return result;
		}

		MultilingualString IAccountingFilterStripHolder.AmountFiltersCategoryNameOveride
		{
			get { return null; }
		}

		MultilingualString IAccountingFilterStripHolder.BillingFiltersCategoryNameOveride
		{
			get { return null; }
		}

		MultilingualString IAccountingFilterStripHolder.FilterNameSuffixInOtherCategories
		{
			get { return null; }
		}

		#endregion
	}
}



