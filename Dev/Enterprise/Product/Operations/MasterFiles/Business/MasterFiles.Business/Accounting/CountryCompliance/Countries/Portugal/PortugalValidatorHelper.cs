using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Portugal
{
	static class PortugalValidatorHelper
	{
		#region AccChargeCode

		public static void ClearValueCachedForValidation(this AccChargeCode chargeCode) => chargeCode.Factory.ClearCachedValue<bool>(GetCacheKey(chargeCode));

		public static void AddErrorIfChargeCodeDescriptionIsNonCompliant(ZPropertyInfo info)
		{
			AddErrorIfChargeCodeFieldChangeIsNotAllowed(info, Res.GetString("88d1026f-ad7b-4a47-80c0-41554e242f47", "You cannot edit this description. At least one transaction has been posted in a Portugal Login Company in this database using this Charge Code."));
			if (info.HasErrors() || !info.BizObj.HasChanges || !IsCompanyApplicable(info.BizObj as AccChargeCode))
			{
				return;
			}

			var description = (ZString)info.Value;
			if (description.Length < 2)
			{
				info.AddError(Res.GetString("ddb8e1eb-5e62-4c74-a34c-afe4227017c4", "Length is invalid, it must be at least 2 characters long. Please ensure you are entering the correct description for this Charge code."));
				return;
			}
			if (!description.ContainsAnyLetters || (description.Length == 2 && !description.IsLettersOnlyOrEmpty))
			{
				info.AddError(Res.GetString("83dc418e-2c5e-4b56-b960-8753de2f5a7f", "Value is invalid. Numbers and special characters only are allowed if description is longer than 2 characters and cannot be only special characters or numbers. Please ensure you are entering the correct description for this Charge code."));
			}
		}

		public static void AddErrorIfChargeCodeFieldChangeIsNotAllowed(ZPropertyInfo info, string errorMessage)
		{
			var chargeCode = info.BizObj as AccChargeCode;
			if (!info.HasErrors() && info.HasChanges && ShouldNotChangeChargeCodeIfHasTransactionPostedInTheCompany(chargeCode))
			{
				info.AddError(errorMessage);
			}
		}

		public static bool ShouldNotChangeChargeCodeIfHasTransactionPostedInTheCompany(this AccChargeCode chargeCode)
		{
			if (!IsCompanyApplicable(chargeCode))
			{
				return false;
			}

			var query = new ZQuery(AccTransactionLinesSchema.AL_GC, chargeCode.Company.PK);
			query.AddToFilter(AccTransactionLinesSchema.AL_AC, chargeCode.PK);
			return chargeCode.Factory.GetCachedValue(GetCacheKey(chargeCode), () => DoesAnyAccTransactionLineExist(chargeCode, query));
		}

		static bool IsCompanyApplicable(AccChargeCode chargeCode) =>
			chargeCode?.Company != null
			&& chargeCode.AC_IsActive
			&& chargeCode.Company.GC_RN_NKCountryCode == CountryCodes.Portugal;

		#endregion

		#region OrgHeader

		public static void ClearValueCachedForValidation(this OrgHeader org) => org.Factory.ClearCachedValue<bool>(GetCacheKey(org));

		public static void AddErrorIfOrgFullNameChangeIsNotAllowed(ZPropertyInfo info, string errorMessage) =>
			ValidateAndAddErrorIfRequires<OrgHeader>(info,
											errorMessage,
											(org) => false,
											(org) => !info.HasChanges
												|| org.CountryCode != CountryCodes.Portugal
												|| DoesThisOrgHasAnyRestrictedCusCode(org)
												|| !ShouldNotChangeOrgDetailsIfHasPostedTransactions(org));

		static bool DoesThisOrgHasAnyRestrictedCusCode(OrgHeader org) => org.CustomsCodes.Cast<OrgCusCode>().Any(c => IsRestrictedCusCode(c));

		#endregion

		#region OrgCusCode

		public static void AddErrorIfOrgCusCodeChangeIsNotAllowed(ZPropertyInfo info, string errorMessage)
		{
			var cusCode = info.BizObj as OrgCusCode;
			ValidateAndAddErrorIfRequires(info,
										errorMessage,
										(org) => false,
										(org) => !info.HasChanges || !(IsRestrictedCusCode(cusCode) && ShouldNotChangeOrgDetailsIfHasPostedTransactions(org)),
										() => cusCode.Organisation);
		}

		public static void AddErrorIfOrgCusCodeIsDuplicate(ZPropertyInfo info, string errorMessage)
		{
			var cusCode = info.BizObj as OrgCusCode;
			ValidateAndAddErrorIfRequires(info,
				errorMessage,
				(org) => false,
				(org) => !(cusCode.OK_RN_NKCodeCountry == CountryCodes.Portugal &&
					cusCode.OK_CodeType == OrgCusCode.CodeTypes.IVA &&
					cusCode.Organisation.CustomsCodes.GetOrgCusCodesForCodeAndCountry(OrgCusCode.CodeTypes.IVA, CountryCodes.Portugal).Length > 1),
				() => cusCode.Organisation);
		}

		public static bool IsThereAnyTransactionForOrgWithRestrictedCusCode(this OrgCusCode cusCode) =>
			Validate(cusCode.Organisation,
					(org) => false,
					(org) => IsRestrictedCusCode(cusCode) && ShouldNotChangeOrgDetailsIfHasPostedTransactions(org));

		#endregion

		#region Implementations

		static bool IsRestrictedCusCode(OrgCusCode cusCode) =>
				cusCode != null &&
				cusCode.IsInDatabase &&
				(ZString)cusCode.OK_RN_NKCodeCountryInfo.OriginalValue == CountryCodes.Portugal &&
				(ZString)cusCode.OK_CodeTypeInfo.OriginalValue == OrgCusCode.CodeTypes.IVA &&
				!IsEmptyPortugalIVA((ZString)cusCode.OK_CustomsRegNoInfo.OriginalValue);

		static bool ShouldNotChangeOrgDetailsIfHasPostedTransactions(OrgHeader org)
		{
			Func<ZQuery> getHeaderQuery = () =>
			{
				var headerQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				headerQuery.AddToFilter(AccTransactionHeaderSchema.AH_OH, org.PK);
				headerQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, new string[] { TransactionTypes.Invoice, TransactionTypes.CreditNote });
				var subQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbCompanySchema.PK, AccTransactionHeaderSchema.AH_GC);
				subQuery.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, CountryCodes.Portugal);
				headerQuery.AddSubQuery(subQuery, JoinCondition.And);
				return headerQuery;
			};

			return org.Factory.GetCachedValue(GetCacheKey(org), () => DoesAnyAccTransactionHeaderExist(org, getHeaderQuery()));
		}

		static void ValidateAndAddErrorIfRequires<T>(ZPropertyInfo propInfo, string errorMessage, Func<T, bool> notInApplicableCountryList, Func<T, bool> countrySpecificValidation, Func<T> getBizO = null)
			where T : BusinessObject
		{
			var bizObj = getBizO?.Invoke() ?? propInfo.BizObj as T;
			bool isValid = Validate(bizObj, notInApplicableCountryList, countrySpecificValidation);
			if (!isValid)
			{
				propInfo.AddError(errorMessage);
			}
		}

		static bool Validate<T>(T bizObj, Func<T, bool> notInApplicableCountryList, Func<T, bool> countrySpecificValidation) =>
			bizObj == null ||
			notInApplicableCountryList(bizObj) ||
			(countrySpecificValidation(bizObj));

		static string GetCacheKey(AccChargeCode chargeCode) => FormattableString.Invariant($"{chargeCode.AC_GC.ToStringKey()}-{chargeCode.PK.ToStringKey()}");

		static string GetCacheKey(OrgHeader org) => FormattableString.Invariant($"{org.PK.ToStringKey()}");

		static bool DoesAnyAccTransactionLineExist(BusinessObject bizO, ZQuery query) => DoesAnyTransactionExist(bizO, AccTransactionLinesSchema.Constants.TableName, query);

		static bool DoesAnyAccTransactionHeaderExist(BusinessObject bizO, ZQuery query) => DoesAnyTransactionExist(bizO, AccTransactionHeaderSchema.Constants.TableName, query);

		static bool DoesAnyTransactionExist(BusinessObject bizo, string tableName, ZQuery searchQuery)
		{
			return (bizo != null &&
					bizo.IsInDatabase)
					&& new BusinessObjectFactory().ExistsInDatabase(tableName, searchQuery);
		}

		public static bool IsEmptyPortugalIVA(ZString registrationNumber)
		{
			return registrationNumber.IsEmpty || registrationNumber == EmptyPortugalIVA || registrationNumber.EqualsIgnoringCase(Core.Constants.CountryCodes.Portugal + EmptyPortugalIVA);
		}

		public const string EmptyPortugalIVA = "999999990";

		#endregion
	}
}
