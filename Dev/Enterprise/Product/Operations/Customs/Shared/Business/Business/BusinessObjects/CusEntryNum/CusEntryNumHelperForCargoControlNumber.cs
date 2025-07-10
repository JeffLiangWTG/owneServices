using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public static class CusEntryNumHelperForCargoControlNumber
	{
		public static ZDBOnlySubQuery GetCargoControlNumberFromCusEntryNumberQuery(ZString tableName, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var isNegativeSqlOperator = comparisonOperator.IsNegativeSQLOperator() || comparisonOperator == SpecialComparisonOperator.IsBlank;
			var sqlOperator = comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery();
			var entryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, isNegativeSqlOperator);
			if (value.IsEmpty && (comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank))
			{
				entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.NotEqual, ZString.Empty);
			}
			else
			{
				AddCCNNumberFilterSubQuery(entryNumberQuery, CusEntryNumSchema.CE_EntryNum, sqlOperator, value);
			}
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, tableName);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Enterprise.Core.Constants.CountryCodes.Canada);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CanadaAdditionalReferenceNumberTypes.Codes.CCN);
			return entryNumberQuery;
		}

		public static ZDBOnlySubQuery GetCargoControlNumberFromCusAddInfoQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var isNegativeSqlOperator = comparisonOperator.IsNegativeSQLOperator() || comparisonOperator == SpecialComparisonOperator.IsBlank;
			var sqlOperator = comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery();

			var cusAddInfoQuery = new ZDBOnlySubQuery(typeof(CusAddInfo), CusAddInfoSchema.B7_ParentID, isNegativeSqlOperator);
			cusAddInfoQuery.AddToFilter(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.CACCN);
			cusAddInfoQuery.AddToFilter(CusAddInfoSchema.B7_ParentTableCode, JobDeclarationSchema.Constants.Prefix);
			cusAddInfoQuery.AddSubQuery(CusAddInfoSchema.PK, GenAddOnColumnSchema.XA_ParentID, GetCCNGenAddOnQuery(sqlOperator, value), JoinCondition.And);

			return cusAddInfoQuery;
		}

		static ZDBOnlySubQuery GetCCNGenAddOnQuery(SQLComparisonOperator sqlOperator, ZString value)
		{
			var genAddOnQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, CusAddInfoSchema.Constants.Prefix);
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, CACargoControlNumberAddInfoSchema.Constants.CA_CCNInfoNumber);
			if (value.IsEmpty && (sqlOperator == SpecialComparisonOperator.IsBlank || sqlOperator == SpecialComparisonOperator.IsNotBlank))
			{
				genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.NotEqual, ZString.Empty);
			}
			else
			{
				AddCCNNumberFilterSubQuery(genAddOnQuery, GenAddOnColumnSchema.XA_Data, sqlOperator, value);
			}
			return genAddOnQuery;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI003", Justification = "Don't have access to the alternatives it wants")]
		static void AddCCNNumberFilterSubQuery(ZDBOnlySubQuery baseQuery, SchemaStringColumn column, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var ccnQuery = new ZQuery();
			ccnQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.Or, column, comparisonOperator, value);
			baseQuery.AddToFilter(ccnQuery, JoinCondition.And);
		}
	}
}
