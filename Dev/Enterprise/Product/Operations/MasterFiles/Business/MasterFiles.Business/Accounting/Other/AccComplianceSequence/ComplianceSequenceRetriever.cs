using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public interface IComplianceSequenceRetriever
	{
		AccComplianceSequence GetComplianceSequenceFromSubType(ZString complianceSubType, ZGuid companyPK, ZGuid branchPK, ZGuid departmentPK);
		AccComplianceSequence GetComplianceSequenceFromSubType(ZString complianceSubType, ZGuid companyPK, ZGuid branchPK, ZGuid departmentPK, ZDateTime allocationDate);
		AccComplianceSequence GetComplianceSequenceFromTransactionReference(ZString complianceSubType, ZString transactionReference, ZGuid companyPK, BusinessObjectFactory factory);
		string GetSequenceNumber(string countryCode, string transactionReference, string sequencePrefix);
	}

	public class ComplianceSequenceRetriever : IComplianceSequenceRetriever
	{
		public AccComplianceSequence GetComplianceSequenceFromSubType(ZString complianceSubType, ZGuid companyPK, ZGuid branchPK, ZGuid departmentPK)
		{
			return FindSuitableBook(complianceSubType, companyPK, branchPK, departmentPK, ZDateTime.Today);
		}

		public AccComplianceSequence GetComplianceSequenceFromSubType(ZString complianceSubType, ZGuid companyPK, ZGuid branchPK, ZGuid departmentPK, ZDateTime allocationDate)
		{
			return FindSuitableBook(complianceSubType, companyPK, branchPK, departmentPK, allocationDate);
		}

		AccComplianceSequence FindSuitableBook(ZString complianceSubType, ZGuid companyPK, ZGuid branchPK, ZGuid departmentPK, ZDateTime allocationDate)
		{
			var parentSubType = AccComplianceSequence.FindParentSubTypeInRegistry(complianceSubType);
			var subType = parentSubType.IsEmpty ? complianceSubType : parentSubType;
			var date = allocationDate.IsValid ? allocationDate : ZDateTime.Today;

			return FindSuitableBook(AccComplianceSequence.FindSuitableSequenceBook(subType, Core.Constants.ComplianceBookAllocationLevel.Counter, date, date, companyPK, branchPK, departmentPK)) ??
				FindSuitableBook(AccComplianceSequence.FindSuitableSequenceBook(subType, Core.Constants.ComplianceBookAllocationLevel.BranchDepartment, date, date, companyPK, branchPK, departmentPK)) ??
				FindSuitableBook(AccComplianceSequence.FindSuitableSequenceBook(subType, Core.Constants.ComplianceBookAllocationLevel.Branch, date, date, companyPK, branchPK, departmentPK)) ??
				FindSuitableBook(AccComplianceSequence.FindSuitableSequenceBook(subType, Core.Constants.ComplianceBookAllocationLevel.Company, date, date, companyPK, branchPK, departmentPK));
		}

		AccComplianceSequence FindSuitableBook(AccComplianceSequence[] findedBooks)
		{
			if (findedBooks.Length > 1)
			{
				throw new MultipleComplianceSequenceFoundException();
			}
			else if (findedBooks.Length == 1)
			{
				return findedBooks[0];
			}
			else
			{
				return null;
			}
		}

		public AccComplianceSequence GetComplianceSequenceFromTransactionReference(ZString complianceSubType, ZString transactionReference, ZGuid companyPK, BusinessObjectFactory factory)
		{
			AccComplianceSequence result = null;
			if (!complianceSubType.IsEmpty && !transactionReference.IsEmpty)
			{
				if (!char.IsDigit(transactionReference[transactionReference.Length - 1]))
				{
					throw new FailedToFindSequenceDueToReferenceNotEndWithNumberException();
				}

				var countryCode = AccountingMasterFilesUtils.GetCountryCodeFromCompanyPK(factory, companyPK.ToGuid());
				var complianceInfo = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceNumberSequenceConfigurationProvider(countryCode);

				if (complianceInfo != null)
				{
					result = factory.LoadTop1<AccComplianceSequence>(complianceInfo.GetComplianceSequenceFromTransactionReferenceFilter(complianceSubType, transactionReference, companyPK));
				}
				if (result == null)
				{
					result = factory.LoadTop1<AccComplianceSequence>(GetComplianceSequenceFromTransactionReferenceDefaultFilter(complianceSubType, transactionReference, companyPK));
				}
			}
			return result;
		}

		ZQuery GetComplianceSequenceFromTransactionReferenceDefaultFilter(ZString complianceSubType, ZString transactionReference, ZGuid companyPK)
		{
			var result = ZQuery.NoResultQuery;
			int numberPartStartsPosition = 0;
			for (int i = transactionReference.Length - 1; i >= 0; i--)
			{
				if (!char.IsDigit(transactionReference[i]))
				{
					numberPartStartsPosition = i + 1;
					break;
				}
			}

			string prefixExcludesNumberPart = transactionReference.Substring(0, numberPartStartsPosition == 0 ? 1 : numberPartStartsPosition);
			int maxLength = AccComplianceSequenceSchema.XD_Prefix.MaxLength;
			prefixExcludesNumberPart = prefixExcludesNumberPart.Length > maxLength ? prefixExcludesNumberPart.Substring(0, maxLength) : prefixExcludesNumberPart;
			string numberSeries = transactionReference.Substring(numberPartStartsPosition);
			ZDecimal numberParsed;
			if (ZDecimal.TryParse(numberSeries, out numberParsed))
			{
				ZDBOnlyQuery complianceSequenceQuery = new ZDBOnlyQuery(typeof(AccComplianceSequence));
				complianceSequenceQuery.AddToFilter(AccComplianceSequenceSchema.XD_GC_Company, companyPK);
				var parentSubType = AccComplianceSequence.FindParentSubTypeInRegistry(complianceSubType);
				complianceSequenceQuery.AddToFilter(AccComplianceSequenceSchema.XD_SequenceClass, parentSubType.IsEmpty ? complianceSubType : parentSubType);

				var prefixSubquery = new ZQuery(AccComplianceSequenceSchema.XD_Prefix, SQLComparisonOperator.StartsWith, prefixExcludesNumberPart);
				prefixSubquery.AddToFilter(JoinCondition.Or, AccComplianceSequenceSchema.XD_Prefix, "");
				complianceSequenceQuery.AddToFilter(prefixSubquery);

				ZString startNumberFilter = string.Format("REVERSE(substring(REVERSE(XD_Prefix), 1, CASE WHEN PATINDEX('%[^0-9]%', REVERSE(XD_Prefix)) = 0 THEN LEN(XD_Prefix) ELSE PATINDEX('%[^0-9]%', REVERSE(XD_Prefix))-1 END )) + Replace(STR(XD_StartNumber, XD_MaximumNumberDigits, 0), ' ', 0) <= Convert(decimal(20, 0), {0})", numberParsed);
				complianceSequenceQuery.AddFilterAndZSQLParameterCollection(startNumberFilter, new ZSqlParameterCollection());

				ZString endNumberFilter = string.Format("REVERSE(substring(REVERSE(XD_Prefix), 1, CASE WHEN PATINDEX('%[^0-9]%', REVERSE(XD_Prefix)) = 0 THEN LEN(XD_Prefix) ELSE PATINDEX('%[^0-9]%', REVERSE(XD_Prefix))-1 END )) + Replace(STR(XD_EndNumber, XD_MaximumNumberDigits, 0), ' ', 0) >= Convert(decimal(20, 0), {0})", numberParsed);
				complianceSequenceQuery.AddFilterAndZSQLParameterCollection(endNumberFilter, new ZSqlParameterCollection());

				ZString lengthFilter = string.Format("LEN(XD_Prefix) + XD_MaximumNumberDigits = {0}", transactionReference.Length);
				complianceSequenceQuery.AddFilterAndZSQLParameterCollection(lengthFilter, new ZSqlParameterCollection());
				result = complianceSequenceQuery;
			}

			return result;
		}

		string IComplianceSequenceRetriever.GetSequenceNumber(string countryCode, string transactionReference, string sequencePrefix)
		{
			var result = string.Empty;

			var complianceInfo = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceNumberSequenceConfigurationProvider(countryCode);
			if (complianceInfo != null)
			{
				result = complianceInfo.GetSequenceNumber(transactionReference);
			}
			if (string.IsNullOrEmpty(result))
			{
				result = transactionReference.Remove(0, sequencePrefix.Length);
			}

			return result;
		}
	}
}
