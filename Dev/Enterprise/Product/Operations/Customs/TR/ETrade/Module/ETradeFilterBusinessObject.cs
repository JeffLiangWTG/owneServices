using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.TR.ETrade.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.ETrade.Module
{
	public class ETradeFilterBusinessObject : FilterStripBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		public static class FilterConstants
		{
			public const string JobNumber = "Job #";
			public const string TemporaryRegistrationNumber = "Temporary Registration Number";
			public const string TemporaryRegistrationNumberDate = "Temporary Registration Number Date";
			public const string RegistrationNumber = "Registration Number";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var jobNumberFilter = result.AddTextFilter(FilterConstants.JobNumber, AsycudaManifestHeaderSchema.AMA_JobReference);
			jobNumberFilter.Category = FilterCategories.NumbersAndReferences;
			jobNumberFilter.MaxLength = Enterprise.Customs.ManifestBase.AsycudaManifestHeader.Schema.AMA_JobReferenceMaxLength;
			jobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("ETradeFilterConstants|JobNumber", FilterConstants.JobNumber);

			var temporaryRegistrationNumberFilter = result.AddTextFilter(FilterConstants.TemporaryRegistrationNumber, GetTemporaryRegistrationNumberQuery);
			temporaryRegistrationNumberFilter.Category = FilterCategories.NumbersAndReferences;
			temporaryRegistrationNumberFilter.MaxLength = Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.Schema.TempRegNoMaxLength;
			temporaryRegistrationNumberFilter.MultilingualDescription = ResString.GetMultilingualString("ETradeFilterConstants|TemporaryRegistrationNumber", FilterConstants.TemporaryRegistrationNumber);

			var temporaryRegistrationNumberDateFilter = result.AddDateFilter(FilterConstants.TemporaryRegistrationNumberDate, GetTemporaryRegistrationNumberDateQuery);
			temporaryRegistrationNumberDateFilter.Category = FilterCategories.Dates;
			temporaryRegistrationNumberDateFilter.MultilingualDescription = ResString.GetMultilingualString("ETradeFilterConstants|TemporaryRegistrationNumberDate", FilterConstants.TemporaryRegistrationNumberDate);

			var registrationNumberFilter = result.AddTextFilter(FilterConstants.RegistrationNumber, GetRegistrationNumberQuery);
			registrationNumberFilter.Category = FilterCategories.NumbersAndReferences;
			registrationNumberFilter.MaxLength = Enterprise.Customs.Common.CusEntryNumber.Schema.CE_EntryNumMaxLength;
			registrationNumberFilter.MultilingualDescription = ResString.GetMultilingualString("ETradeFilterConstants|RegistrationNumber", FilterConstants.RegistrationNumber);

			return result;
		}

		public override ZQuery Filter
		{
			get
			{
				var filter = base.Filter;
				filter.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, Core.Constants.CountryCodes.Turkey);
				filter.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, Messaging.Integration.ApplicationCodeList.Codes.TRETrade);
				return filter;
			}
		}

		ZDBOnlyQuery GetTemporaryRegistrationNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));

			bool notIn = false;
			if (comparisonOperator != null)
			{
				if (comparisonOperator == SpecialComparisonOperator.IsBlank)
				{
					notIn = true;
				}
			}

			var subQuery = GetQueryOnCusCodeData(notIn);

			if (!value.IsEmpty || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				subQuery.AddToFilter(CusCodeDataSchema.CY_Data, comparisonOperator, value);
			}

			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, CusCodeDataSchema.CY_ParentID, subQuery, JoinCondition.And);

			return headerQuery;
		}

		ZDBOnlySubQuery GetQueryOnCusCodeData(bool notIn)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(CusCodeData), CusCodeDataSchema.CY_ParentID, notIn);
			subQuery.AddToFilter(CusCodeDataSchema.CY_Code, CusCodeDataTypeList.Codes.TRGNO);
			subQuery.AddToFilter(CusCodeDataSchema.CY_Type, ManifestBase.ApplicationCodeTypeList.Codes.TRETrade);
			return subQuery;
		}

		ZQuery GetTemporaryRegistrationNumberDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));

			bool notIn = false;
			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				notIn = true;
			}

			var subQuery = GetQueryOnCusCodeData(notIn);

			if (!value1.IsEmpty || !value2.IsEmpty || comparisonOperator == DateComparisonOperator.HasDateEntered)
			{
				AddDateTimeRange(subQuery, comparisonOperator, JoinCondition.And, CusCodeDataSchema.CY_Date, value1, value2);
			}

			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, CusCodeDataSchema.CY_ParentID, subQuery, JoinCondition.And);

			return headerQuery;
		}

		ZDBOnlyQuery GetRegistrationNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));

			bool notIn = false;
			if (comparisonOperator != null)
			{
				if (comparisonOperator == SpecialComparisonOperator.IsBlank)
				{
					notIn = true;
				}
			}

			var subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, notIn);

			if (!value.IsEmpty || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				subQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
			}

			subQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Turkey);
			subQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration);

			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, CusEntryNumSchema.CE_ParentID, subQuery, JoinCondition.And);

			return headerQuery;
		}
	}
}
