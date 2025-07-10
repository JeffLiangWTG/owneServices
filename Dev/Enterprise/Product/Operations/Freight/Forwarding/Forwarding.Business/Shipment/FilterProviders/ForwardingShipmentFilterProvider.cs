using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class ForwardingShipmentFilterProvider
	{
		public static ZDBOnlyQuery GetJobShipmentFromEntryNumber(SQLComparisonOperator sqlComparisonOperator, ZString customsEntryNo)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));

			bool isBlankComparison = sqlComparisonOperator == SpecialComparisonOperator.IsBlank;
			JoinCondition joinCondition = isBlankComparison ? JoinCondition.And : JoinCondition.Or;
			SQLComparisonOperator comparisonOperator = isBlankComparison ? SQLComparisonOperator.NotEqual : sqlComparisonOperator;

			//Case where CusEntryNum is attached to CusEntryHeader
			var cusEntryHeaderDeclarationQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration), JobDeclarationSchema.JE_JS, isBlankComparison);
			cusEntryHeaderDeclarationQuery.AddToFilter(JobDeclarationSchema.JE_JS, SQLComparisonOperator.NotEqual, null);
			cusEntryHeaderDeclarationQuery.AddSubQuery(GetGlbBranchQuery(), JoinCondition.And);
			cusEntryHeaderDeclarationQuery.AddSubQuery(GetCusEntryHeaderQuery(comparisonOperator, customsEntryNo), JoinCondition.And);
			result.AddSubQuery(cusEntryHeaderDeclarationQuery, joinCondition);

			//Case where CusEntryNum is attached to JobDeclaration
			var declarationQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration), JobDeclarationSchema.JE_JS, isBlankComparison);
			declarationQuery.AddToFilter(JobDeclarationSchema.JE_JS, SQLComparisonOperator.NotEqual, null);
			declarationQuery.AddSubQuery(GetGlbBranchQuery(), JoinCondition.And);
			declarationQuery.AddSubQuery(GetCusEntryNumberQuery(comparisonOperator, customsEntryNo, JobDeclarationSchema.Constants.TableName), JoinCondition.And);
			result.AddSubQuery(declarationQuery, joinCondition);

			//Case where CusEntryNum is attached to JobShipment
			var shipmentQuery = GetShipmentCusEntryNumberQuery(comparisonOperator, customsEntryNo, isBlankComparison);
			result.AddSubQuery(shipmentQuery, joinCondition);

			return result;
		}

		static ZDBOnlySubQuery GetGlbBranchQuery()
		{
			var companyQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbBranchSchema.GB_GC);
			companyQuery.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
			branchQuery.AddToFilter(GlbBranchSchema.GB_IsActive, true);
			branchQuery.AddSubQuery(companyQuery, JoinCondition.And);
			return branchQuery;
		}

		static ZDBOnlySubQuery GetCusEntryHeaderQuery(SQLComparisonOperator comparisonOperator, ZString customsEntryNo)
		{
			var entryHeaderQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.ICusEntryHeader), CusEntryHeaderSchema.CH_JE);
			entryHeaderQuery.AddSubQuery(GetCusEntryNumberQuery(comparisonOperator, customsEntryNo, CusEntryHeaderSchema.Constants.TableName), JoinCondition.And);

			return entryHeaderQuery;
		}

		static ZDBOnlySubQuery GetCusEntryNumberQuery(SQLComparisonOperator comparisonOperator, ZString customsEntryNo, ZString parentTable)
		{
			var entryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, parentTable);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryIsSystemGenerated, SQLComparisonOperator.Equal, ZBool.True);
			entryNumberQuery.AddToFilter_PossiblyCommaSeparated(CusEntryNumSchema.CE_EntryNum, comparisonOperator, customsEntryNo);

			return entryNumberQuery;
		}

		static ZDBOnlySubQuery GetShipmentCusEntryNumberQuery(SQLComparisonOperator comparisonOperator, ZString customsEntryNo, ZBool isBlank)
		{
			var query = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, isBlank);
			query.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			query.AddToFilter(CusEntryNumSchema.CE_ParentTable, JobShipmentSchema.Constants.TableName);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.Code);
			query.AddToFilter_PossiblyCommaSeparated(CusEntryNumSchema.CE_EntryNum, comparisonOperator, customsEntryNo);

			return query;
		}
	}
}
