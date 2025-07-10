using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	sealed class EntryLineFilterBusinessObject : Customs.Module.EntryLineFilterBusinessObject
	{
		public EntryLineFilterBusinessObject(IBusinessObjectCollection gridCollection)
			: base(gridCollection)
		{
			this.gridCollection = gridCollection;
		}
		readonly IBusinessObjectCollection gridCollection;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args) => new EntryLineFilterBusinessObject(gridCollection);

		protected override ZDBOnlySubQuery EntryHeaderSubQuery
		{
			get
			{
				var result = base.EntryHeaderSubQuery;
				var messageTypeENS = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
				result.AddToFilter(CusEntryHeaderSchema.CH_MessageType, messageTypeENS);
				return result;
			}
		}

		protected override EntryNumberFilterSubGroup GetNewEntryNumberFilterSubGroup() => new UsEntryNumberFilterSubGroup();

		protected override void AddDateFilters(ModuleFilterCollection filters)
		{
			base.AddDateFilters(filters);
			filters.AddDateFilter(DeclarationFilterConstants.EntryDate, GetEntryDateQuery);
		}

		protected override bool IncludeFirstArrival => false;

		protected override void AddLocationFilters(ModuleFilterCollection filters)
		{
			base.AddLocationFilters(filters);
			var newFilter = filters.AddNkFilter(DeclarationFilterConstants.PortOfEntry, GetPortOfEntryQuery, ModuleIDs.Customs.Universal.ZZRefCusCodeList, ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today));
			newFilter.Category = FilterCategories.Locations;
			newFilter = filters.AddNkFilter(DeclarationFilterConstants.DischargeSchedDK, GetDischargeSchedDKQuery, ModuleIDs.Customs.Universal.ZZRefCusCodeList, ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today));
			newFilter.Category = FilterCategories.Locations;
		}

		protected override void AddOrgFilters(ModuleFilterCollection filters)
		{
			base.AddOrgFilters(filters);
			filters.AddGuidFilter(DeclarationFilterConstants.ImporterOfRecord, ModuleIDs.Organisation, GetImporterOfRecordQuery, Lookups.Consignees);
		}

		ZQuery GetEntryDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var result = new ZDBOnlyQuery(typeof(CusEntryLine));
			var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryLineSchema.CL_CH);
			entryHeaderQuery.AddToFilter(QueryHelper.GetQueryWithSubQueryOnGenAddOnColumn(USAddInfoSchema.Constants.US_EntryDate, comparisonOperator, value1, value2));
			result.AddSubQuery(entryHeaderQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetPortOfEntryQuery(ZString portOfEntry)
		{
			var result = new ZDBOnlyQuery(typeof(CusEntryLine));
			var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryLineSchema.CL_CH);
			entryHeaderQuery.AddToFilter(QueryHelper.GetQueryWithSubQueryOnGenAddOnColumn(USAddInfoSchema.Constants.US_SchDEntry, portOfEntry));
			result.AddSubQuery(entryHeaderQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetDischargeSchedDKQuery(ZString portOfDischarge)
		{
			var result = new ZDBOnlyQuery(typeof(CusEntryLine));
			var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryLineSchema.CL_CH);
			entryHeaderQuery.AddToFilter(QueryHelper.GetQueryWithSubQueryOnGenAddOnColumn(USAddInfoSchema.Constants.US_SchDArrival, portOfDischarge));
			result.AddSubQuery(entryHeaderQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetImporterOfRecordQuery(ZGuid importerOfRecord)
		{
			var result = new ZDBOnlyQuery(typeof(CusEntryLine));
			var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryLineSchema.CL_CH);
			var subQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
			var subAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDeclarationSchema.JE_OA_DeclarantAddress);
			subAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, SQLComparisonOperator.Equal, importerOfRecord);
			subQuery.AddSubQuery(subAddressQuery, JoinCondition.And);
			entryHeaderQuery.AddSubQuery(subQuery, JoinCondition.And);
			result.AddSubQuery(entryHeaderQuery, JoinCondition.And);
			return result;
		}

		GenAddOnColumnQueryHelper QueryHelper => queryHelper ?? (queryHelper = new GenAddOnColumnQueryHelper(typeof(CusEntryHeader), typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE));
		GenAddOnColumnQueryHelper queryHelper;

		internal class UsEntryNumberFilterSubGroup : EntryNumberFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var entryLineQuery = new ZDBOnlyQuery(typeof(CusEntryLine));
				var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryLineSchema.CL_CH);
				entryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
				var declarationQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
				var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				cusEntryNumQuery.AddToFilter(filter);
				cusEntryNumQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				cusEntryNumQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
				declarationQuery.AddSubQuery(cusEntryNumQuery, JoinCondition.And);
				entryHeaderQuery.AddSubQuery(declarationQuery, JoinCondition.And);
				entryLineQuery.AddSubQuery(entryHeaderQuery, JoinCondition.And);
				var result = new ZQuery().AddToFilter(entryLineQuery);
				result.AddFilterAndZSQLParameterCollection(@"
CL_PK IN
(SELECT A.CL_PK FROM dbo.CusEntryLine A
INNER JOIN (SELECT CL_CH,CL_LineNumber,(COUNT(*)-1) as MaxLineNumber FROM dbo.CusEntryLine GROUP BY CL_CH,CL_LineNumber) B ON A.CL_CH = B.CL_CH AND A.CL_LineNumber = B.CL_LineNumber
WHERE A.CL_CustomsPostedStatus = 'ACT' AND
A.CL_AdValoremTariff <> '' AND 
A.CL_AddInfo NOT LIKE '%SupLine=Y%' AND
(A.CL_AddInfo NOT LIKE '%ChildLineNum=%' OR 
A.CL_AddInfo LIKE '%ChildLineNum=' + CONVERT(varchar(2), B.MaxLineNumber) + '%'))
", null);

				return result;
			}
		}
	}
}
