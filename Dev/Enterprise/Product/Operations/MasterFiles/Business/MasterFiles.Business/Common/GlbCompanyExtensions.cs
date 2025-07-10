using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using ZEnvironment = Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public static class GlbCompanyExtensions
	{
		#region  GlbCompany

		public static ZEnvironment.ExchangeRate GetExchangeRate(this GlbCompany company)
		{
			var companyToUse = company ?? GlbCompany.CurrentCompany;
			return new ZEnvironment.ExchangeRate(companyToUse.GC_IsReciprocal, companyToUse.GetLocalDecimals(),
				companyToUse.PK.ToGuid());
		}

		#region GetLocalDecimals()

		public static int GetLocalDecimals(this GlbCompany company)
		{
			var companyToUse = company ?? GlbCompany.CurrentCompany;
			var localCurrencyCachedMessage = companyToUse.GetLocalCurrencyCachedMessage;
			var localCurrency = companyToUse?.LocalCurrency;
			if (localCurrency != null)
			{
				return localCurrency.Decimals;
			}
			return TryVeryHardToGetLocalDecimalsWithFallback(companyToUse, localCurrencyCachedMessage);
		}

		static int TryVeryHardToGetLocalDecimalsWithFallback(GlbCompany company, string localCurrencyCachedMessage)
		{
			if (company == null)
			{
				throw new ArgumentNullException(nameof(company), $"Call to {nameof(GetLocalDecimals)}() with null company and GlbCompany.CurrentCompany is also null.");
			}
			if (company.GC_RX_NKLocalCurrency == ZString.Empty
				&& company.IsInDatabase
				&& company.GC_IsActive)
			{
				throw new ApplicationException($"Unable to get number of local decimals for active company '{company.GC_Code}' ({company.GC_Name}): {nameof(company.GC_RX_NKLocalCurrency)} is '{company.GC_RX_NKLocalCurrency}'. Please set a Local Currency for this company.");
			}
			if (company.GC_RX_NKLocalCurrency == ZString.Empty
				&& (!company.IsInDatabase || !company.GC_IsActive)
				)
			{
				return GetLocalDecimalsFallback;
			}

			var havePreviouslyClearedUberFactoryCache = company.Factory.HasContext(GetLocalDecimalsContext.HaveClearedUberCache);
			if (company.GC_RX_NKLocalCurrency != ZString.Empty
				&& company.IsInDatabase
				&& company.GC_IsActive
				&& company.LocalCurrency == null
				&& !havePreviouslyClearedUberFactoryCache)
			{
				CargoWise.EntityFramework.RowFactory.ClearSpecificTableFromUberFactory(nameof(RefCurrency));
				company.Factory.SetContext(GetLocalDecimalsContext.HaveClearedUberCache);
			}
			if (company.LocalCurrency != null)
			{
				company.Factory.RemoveContext(GetLocalDecimalsContext.HaveClearedUberCache);
				return company.LocalCurrency.Decimals;
			}

			if (!havePreviouslyClearedUberFactoryCache)
			{
				var messageForErrorReporter = new StringBuilder();
				messageForErrorReporter.AppendLine((NoResString)"Call to GlbCompany.GetLocalDecimals() when GlbCompany.LocalCurrency is null.");
				messageForErrorReporter.AppendLine($"GlbCompany.IsInDatabase = {company.IsInDatabase}");
				messageForErrorReporter.AppendLine($"GlbCompany.IsDeleted = {company.IsDeleted}");
				messageForErrorReporter.AppendLine($"GlbCompany.IsDeleting = {company.IsDeleting}");
				messageForErrorReporter.AppendLine($"GlbCompany.GC_Code = {company.GC_Code}");
				messageForErrorReporter.AppendLine($"GlbCompany.GC_Name = {company.GC_Name}");
				messageForErrorReporter.AppendLine($"GlbCompany.GC_RX_NKLocalCurrency = {company.GC_RX_NKLocalCurrency}");
				messageForErrorReporter.AppendLine($"GlbCompany.GC_IsActive = {company.GC_IsActive}");
				messageForErrorReporter.AppendLine($"GlbCompany.Factory = {company.Factory._Instance} '{company.Factory.NameForDebugging}'");
				messageForErrorReporter.AppendLine($"Is GlbCompany.CurrentCompany = {object.ReferenceEquals(company, GlbCompany.CurrentCompany)}");
				if (localCurrencyCachedMessage != null)
				{
					messageForErrorReporter.AppendLine($"{localCurrencyCachedMessage}");
				}
				messageForErrorReporter.AppendLine($"Have cleared RefCurrency table from UberFactory cache, and GlbCompany.LocalCurrency remains null.");
				messageForErrorReporter.Append($"Returned {GetLocalDecimalsFallback} Local Decimals as fallback.");
				
				ErrorReporter.ReportOnce("GetLocalDecimalsCannotLoadLocalCurrency", messageForErrorReporter.ToString());
			}

			return GetLocalDecimalsFallback;
		}

		const int GetLocalDecimalsFallback = 2;     // This is the most common value for number of decimals in RefCurrency.

		enum GetLocalDecimalsContext
		{
			HaveClearedUberCache
		}

		#endregion

		public static bool IsExtraTaxApplicable(this GlbCompany company)
		{
			var companyToUse = company ?? GlbCompany.CurrentCompany;
			return companyToUse.GC_IsGSTRegistered &&
				   (companyToUse.GC_RN_NKCountryCode == Constants.CountryCodes.Canada
					|| companyToUse.GC_RN_NKCountryCode == Constants.CountryCodes.Ghana
					|| companyToUse.GC_RN_NKCountryCode == Constants.CountryCodes.Italy
					|| companyToUse.GC_RN_NKCountryCode == Constants.CountryCodes.CostaRica
					|| HasExtraTaxInfo());

			bool HasExtraTaxInfo()
			{
				var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(companyToUse.GC_RN_NKCountryCode) as ICountryComplianceInfo;
				return complianceInfo != null && complianceInfo.HasExtraTaxInfo().HasValue && complianceInfo.HasExtraTaxInfo().Value;
			}
		}

		public static IEnumerable<ZGuid> GetAllOrgProxiesIncludingBranches(this GlbCompany company) =>
			Argument.NotNull(company, nameof(company))
				.Branches
				.Select(b => b.GB_OH_OrgProxy)
				.Append(company.GC_OH_OrgProxy)
				.Where(x => !x.IsEmpty)
				.Distinct();

		#endregion
	}
}
