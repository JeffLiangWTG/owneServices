using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public class EntryLineFilterBusinessObject : FilterStripBusinessObject
	{
		public EntryLineFilterBusinessObject(IBusinessObjectCollection gridCollection)
		{
			this.gridCollection = (GlobalCusEntryLineCollection)gridCollection;
		}
		readonly GlobalCusEntryLineCollection gridCollection;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
			=> new EntryLineFilterBusinessObject(gridCollection);

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			if (gridCollection != null)
			{
				Lookups.Importer = gridCollection.Importer;
			}

			ModuleFilterCollection result = new ModuleFilterCollection();
			AddOrgFilters(result);
			AddTextFilters(result);
			AddNumberFilters(result);
			AddDateFilters(result);
			AddLocationFilters(result);

			ModuleNkFilter filter = result.AddNkFilter(DeclarationFilterConstants.Country, GetCountryQuery, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
			filter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;

			return result;
		}

		ZQuery GetCountryQuery(ZString userEnteredCountryCode)
		{
			var country = GlbCompany.CurrentCompany.Country;
			var dBOnlyQuery = new ZDBOnlyQuery(typeof(CusEntryLine));
			var entryHeaderQuery = EntryHeaderSubQuery;
			var jobDeclarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), CusEntryHeaderSchema.CH_JE);
			jobDeclarationQuery.AddToFilter(JobDeclarationFilter.ForCountry(true, country.Code, Factory));
			entryHeaderQuery.AddSubQuery(jobDeclarationQuery, JoinCondition.And);
			dBOnlyQuery.AddSubQuery(entryHeaderQuery, JoinCondition.And);

			var result = new ZQuery();
			result.AddToFilter(dBOnlyQuery);
			return result;
		}

		protected virtual ZDBOnlySubQuery EntryHeaderSubQuery
		{
			get { return new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryLineSchema.CL_CH); }
		}

		protected virtual void AddOrgFilters(ModuleFilterCollection filters)
		{
			var importerFilter = filters.AddGuidFilter(DeclarationFilterConstants.OrgFilterTypes.Importer, ModuleIDs.Organisation, JobDeclarationSchema.JE_OH_Importer, Lookups.Consignees);
			importerFilter.Visibility = FilterVisibility.AlwaysVisible;
			importerFilter.SubGroup = new ImporterFilterSubGroup();
			importerFilter.MultilingualDescription = ResString.GetMultilingualString("EntryLineFilter|Importer", DeclarationFilterConstants.OrgFilterTypes.Importer);
		}

		protected virtual void AddTextFilters(ModuleFilterCollection filters)
		{
		}

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			var partsFilter = filters.AddGuidFilter(DeclarationFilterConstants.Product, ModuleIDs.SupplierPart, JobComInvoiceLineSchema.JI_OP, Lookups.PartsList);
			partsFilter.Visibility = FilterVisibility.AlwaysVisible;
			partsFilter.Category = FilterCategories.NumbersAndReferences;
			partsFilter.SubGroup = new OrgSupplierPartFilterSubGroup();
			partsFilter.MultilingualDescription = ResString.GetMultilingualString("EntryLineFilter|Product", DeclarationFilterConstants.Product);

			var orderFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef, JobDeclarationSchema.JE_OwnerRef);
			orderFilter.MultilingualDescription = ResString.GetMultilingualString("EntryLineFilter|OrderNumberOwnersRef", DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef);
			orderFilter.SubGroup = new OrderNumberOwnerRefSpecificFieldSubGroup();
			orderFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			orderFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);

			var decRefFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.DeclarationReference, JobDeclarationSchema.JE_DeclarationReference);
			decRefFilter.SubGroup = new DeclarationReferenceFilterSubGroup();
			decRefFilter.MultilingualDescription = ResString.GetMultilingualString("EntryLineFilter|DeclarationReference", DeclarationFilterConstants.NumberFilterTypes.DeclarationReference);

			var entryNumberFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.EntryNumber, CusEntryNumSchema.CE_EntryNum);
			entryNumberFilter.Visibility = FilterVisibility.AlwaysApplied;
			entryNumberFilter.SubGroup = GetNewEntryNumberFilterSubGroup();
			entryNumberFilter.MultilingualDescription = ResString.GetMultilingualString("EntryLineFilter|EntryNumber", DeclarationFilterConstants.NumberFilterTypes.EntryNumber);

			var tariffNumberFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.TariffNumber, GetTariffQuery);
			tariffNumberFilter.Category = FilterCategories.NumbersAndReferences;
			tariffNumberFilter.Visibility = FilterVisibility.AlwaysVisible;
			tariffNumberFilter.MaxLength = CusEntryLineSchema.CL_AdValoremTariff.MaxLength;
			tariffNumberFilter.MultilingualDescription = ResString.GetMultilingualString("EntryLineFilter|TariffNumber", DeclarationFilterConstants.NumberFilterTypes.TariffNumber);

			var classFilter = filters.AddGuidFilter(DeclarationFilterConstants.ImportClassification, ModuleIDs.ImportClassification, GetClassificationQuery, Lookups.ClassificationList);
			classFilter.Category = FilterCategories.NumbersAndReferences;
			classFilter.MultilingualDescription = ResString.GetMultilingualString("EntryLineFilter|ImportClassification", DeclarationFilterConstants.ImportClassification);

			filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.LineNumber, GetLineNumberQuery).MultilingualDescription = ResString.GetMultilingualString("EntryLineFilter|LineNumber", DeclarationFilterConstants.NumberFilterTypes.LineNumber);
			var attr1Filter1 = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.PartAttribute1, JobComInvoiceLineSchema.JI_PartAttrib1);
			attr1Filter1.SubGroup = new PartAttributeFilterSubGroup();
			attr1Filter1.MultilingualDescription = ResString.GetMultilingualString("EntryLineFilter|PartAttribute1", DeclarationFilterConstants.NumberFilterTypes.PartAttribute1);
			var attr1Filter2 = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.PartAttribute2, JobComInvoiceLineSchema.JI_PartAttrib2);
			attr1Filter2.SubGroup = new PartAttributeFilterSubGroup();
			attr1Filter2.MultilingualDescription = ResString.GetMultilingualString("EntryLineFilter|PartAttribute2", DeclarationFilterConstants.NumberFilterTypes.PartAttribute2);
			var attr3Filter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.PartAttribute3, JobComInvoiceLineSchema.JI_PartAttrib3);
			attr3Filter.SubGroup = new PartAttributeFilterSubGroup();
			attr3Filter.MultilingualDescription = ResString.GetMultilingualString("EntryLineFilter|PartAttribute3", DeclarationFilterConstants.NumberFilterTypes.PartAttribute3);
		}

		class PartAttributeFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var invoiceLineSubQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceLine), JobComInvoiceLineSchema.JI_CL);
				invoiceLineSubQuery.AddToFilter(filter);

				var result = new ZDBOnlyQuery(typeof(CusEntryLine));
				result.AddSubQuery(invoiceLineSubQuery, JoinCondition.And);

				return result;
			}
		}

		class OrderNumberOwnerRefSpecificFieldSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(CusEntryLine));
				var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryLineSchema.CL_CH);
				var queryString = string.Format("CH_JE IN (SELECT JE_PK FROM dbo.JobDeclarationOrderNumber {0})", filter.GetAsWhereClause(false));
				entryHeaderQuery.AddFilterAndZSQLParameterCollection(queryString, new ZSqlParameterCollection(filter.Params), true);
				query.AddSubQuery(entryHeaderQuery, JoinCondition.And);

				return query;
			}
		}

		protected virtual void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.CommercialInvoiceDate, GetInvoiceDateQuery).MultilingualDescription = ResString.GetMultilingualString("EntryLineFilter|CommercialInvoiceDate", DeclarationFilterConstants.DateFilterTypes.CommercialInvoiceDate);
			if (IncludeFirstArrival)
			{
				filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.FirstArrival, GetFirstArrivalDateQuery).MultilingualDescription = ResString.GetMultilingualString("EntryLineFilter|FirstArrival", DeclarationFilterConstants.DateFilterTypes.FirstArrival);
			}

			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.Created, GetCreatedDateQuery).MultilingualDescription = ResString.GetMultilingualString("EntryLineFilter|Created", DeclarationFilterConstants.DateFilterTypes.Created);
			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.DateOfArrival, GetArrivalDateQuery).MultilingualDescription = ResString.GetMultilingualString("EntryLineFilter|DateOfArrival", DeclarationFilterConstants.DateFilterTypes.DateOfArrival);
		}

		protected virtual bool IncludeFirstArrival
		{
			get { return true; }
		}

		protected virtual bool IncludeDrawbackDateFilter
		{
			get { return false; }
		}

		protected virtual void AddLocationFilters(ModuleFilterCollection filters)
		{
		}

		class DeclarationReferenceFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(CusEntryLine));
				var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryLineSchema.CL_CH);
				var jobDeclarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), CusEntryHeaderSchema.CH_JE);
				jobDeclarationQuery.AddToFilter(filter);
				entryHeaderQuery.AddSubQuery(jobDeclarationQuery, JoinCondition.And);
				query.AddSubQuery(entryHeaderQuery, JoinCondition.And);
				return query;
			}
		}

		protected virtual EntryNumberFilterSubGroup GetNewEntryNumberFilterSubGroup()
		{
			return new EntryNumberFilterSubGroup();
		}

		public class EntryNumberFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZQuery result = new ZQuery();
				result.AddToFilter(CusEntryLineSchema.CL_CustomsPostedStatus, SQLComparisonOperator.Equal, Customs.Business.EntryLineStatusList.Codes.Active);
				result.AddToFilter(CusEntryLineSchema.CL_AdValoremTariff, SQLComparisonOperator.NotEqual, ZString.Empty);
				ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(CusEntryLine));
				ZDBOnlySubQuery entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryLineSchema.CL_CH);
				ZDBOnlySubQuery cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				cusEntryNumQuery.AddToFilter(filter);  // Need to check this functionally - this will make the subGroup test shut up, but it might modify the function (it will no longer include the is-blank clause).
				cusEntryNumQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				entryHeaderQuery.AddSubQuery(cusEntryNumQuery, JoinCondition.And);
				dBOnlyQuery.AddSubQuery(entryHeaderQuery, JoinCondition.And);
				result.AddToFilter(dBOnlyQuery);
				return result;
			}
		}

		class ImporterFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZQuery result = new ZQuery();
				ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(CusEntryLine));
				ZDBOnlySubQuery entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryLineSchema.CL_CH);
				ZDBOnlySubQuery jobDeclarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), CusEntryHeaderSchema.CH_JE);
				jobDeclarationQuery.AddToFilter(filter);
				entryHeaderQuery.AddSubQuery(jobDeclarationQuery, JoinCondition.And);
				dBOnlyQuery.AddSubQuery(entryHeaderQuery, JoinCondition.And);
				result.AddToFilter(dBOnlyQuery);
				return result;
			}
		}

		protected ZQuery GetLineNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery result = new ZQuery();
			short shortResult;
			Int16.TryParse(value, out shortResult);
			result.AddToFilter(CusEntryLineSchema.CL_LineNumber, SQLComparisonOperator.Equal, shortResult);
			return result;
		}

		class OrgSupplierPartFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZQuery result = new ZQuery();
				ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(CusEntryLine));
				ZDBOnlySubQuery invoiceLineQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceLine), JobComInvoiceLineSchema.JI_CL);
				invoiceLineQuery.AddToFilter(filter);
				dBOnlyQuery.AddSubQuery(invoiceLineQuery, JoinCondition.And);
				result.AddToFilter(dBOnlyQuery);
				return result;
			}
		}

		protected ZQuery GetTariffQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery result = new ZQuery();
			result.AddToFilter_PossiblyCommaSeparated(CusEntryLineSchema.CL_AdValoremTariff, comparisonOperator, value);
			return result;
		}

		protected ZQuery GetClassificationQuery(ZGuid classification)
		{
			ZQuery result = new ZQuery();
			ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(CusEntryLine));
			ZDBOnlySubQuery invoiceLineQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceLine), JobComInvoiceLineSchema.JI_CL);
			invoiceLineQuery.AddToFilter(JobComInvoiceLineSchema.JI_CC, classification);
			dBOnlyQuery.AddSubQuery(invoiceLineQuery, JoinCondition.And);
			result.AddToFilter(dBOnlyQuery);
			return result;
		}

		ZQuery GetInvoiceDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(CusEntryLine));
			ZDBOnlySubQuery invoiceLineQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceLine), JobComInvoiceLineSchema.JI_CL);
			ZDBOnlySubQuery invoiceHeaderQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceHeader), JobComInvoiceLineSchema.JI_JZ);
			AddDateRange(invoiceHeaderQuery, comparisonOperator, JoinCondition.And, JobComInvoiceHeaderSchema.JZ_InvoiceDate, value1.Date, value2.Date);
			invoiceLineQuery.AddSubQuery(invoiceHeaderQuery, JoinCondition.And);
			dBOnlyQuery.AddSubQuery(invoiceLineQuery, JoinCondition.And);
			return dBOnlyQuery;
		}

		ZQuery GetFirstArrivalDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(CusEntryLine));
			ZDBOnlySubQuery entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryLineSchema.CL_CH);
			ZDBOnlySubQuery jobDeclarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), CusEntryHeaderSchema.CH_JE);
			AddDateRange(jobDeclarationQuery, comparisonOperator, JoinCondition.And, JobDeclarationSchema.JE_DateOfFirstArrival, value1.Date, value2.Date);
			entryHeaderQuery.AddSubQuery(jobDeclarationQuery, JoinCondition.And);
			dBOnlyQuery.AddSubQuery(entryHeaderQuery, JoinCondition.And);
			return dBOnlyQuery;
		}

		ZQuery GetArrivalDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(CusEntryLine));
			ZDBOnlySubQuery entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryLineSchema.CL_CH);
			ZDBOnlySubQuery jobDeclarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), CusEntryHeaderSchema.CH_JE);
			AddDateRange(jobDeclarationQuery, comparisonOperator, JoinCondition.And, JobDeclarationSchema.JE_DateOfArrival, value1.Date, value2.Date);
			entryHeaderQuery.AddSubQuery(jobDeclarationQuery, JoinCondition.And);
			dBOnlyQuery.AddSubQuery(entryHeaderQuery, JoinCondition.And);
			return dBOnlyQuery;
		}

		ZQuery GetCreatedDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var value1utc = value1.IsValid ? Env.Time.GetUtcFromLocalTime(value1.ToDateTime()) : ZDateTime.Empty;
			var value2utc = value2.IsValid ? Env.Time.GetUtcFromLocalTime(value2.ToDateTime()) : ZDateTime.Empty;
			var dBOnlyQuery = new ZDBOnlyQuery(typeof(CusEntryLine));
			var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryLineSchema.CL_CH);
			var jobDeclarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), CusEntryHeaderSchema.CH_JE);
			AddDateTimeRange(jobDeclarationQuery, comparisonOperator, JoinCondition.And, JobDeclarationSchema.JE_SystemCreateTimeUtc, value1utc, value2utc);
			entryHeaderQuery.AddSubQuery(jobDeclarationQuery, JoinCondition.And);
			dBOnlyQuery.AddSubQuery(entryHeaderQuery, JoinCondition.And);
			return dBOnlyQuery;
		}

		#endregion

		#region Lookups

		public EntryLineFilterLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = GetNewLookups();
				}
				return lookups;
			}
		}
		EntryLineFilterLookups lookups;

		protected virtual EntryLineFilterLookups GetNewLookups()
		{
			return new EntryLineFilterLookups(this);
		}

		#endregion

	}
}
