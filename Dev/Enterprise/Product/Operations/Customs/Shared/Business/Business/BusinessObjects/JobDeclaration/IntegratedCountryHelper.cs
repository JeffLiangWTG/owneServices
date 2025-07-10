using System;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants.CountryCodes;
using static Enterprise.Integration.Customs.CustomsWare;

namespace Enterprise.Customs.Business
{
	[Immutable]
	public static class IntegratedCountryHelper
	{
		public static bool IsInterfaceEnabledCompany(ZGuid companyPK, ZString countryCode)
		{
			var registryCompanyPK = companyPK.IsValid ? companyPK.ToGuid() : Guid.Empty;
			var customsInterface = CustomsDataRegistry.Instance.LocalCountryCustomsInterface.GetValueWithoutFallback(registryCompanyPK, Guid.Empty, Guid.Empty);
			var submissionType = customsInterface?.SubmissionType.ToUpperInvariant() ?? ZString.Empty;
			return submissionType.IsEmpty ? !IsUsedToBeBuiltInOnlyCountry(countryCode) : submissionType != DeclarationApplicationCodeList.Codes.Builtin;
		}

		public static bool CustomsWareInstallations(ZString countryCode) => CustomsWareCountryCodes.Contains<string>(countryCode);

		public static readonly ImmutableArray<string> CustomsWareCountryCodes = ImmutableArray.Create(
			Ireland,
			Belgium);

		// Developers never to change HasBuiltInDeclarationCountryCodes unless specified by the Customs Senior Product Team (see Brendon for details).
		public static readonly ImmutableArray<string> HasBuiltInDeclarationCountryCodes = ImmutableArray.Create(
			_TemplateCountryName_,
			Australia,
			Canada,
			France,
			FrenchGuyana,
			Germany,
			Guadeloupe,
			Martinique,
			Mayotte,
			NewZealand,
			PuertoRico,
			Reunion,
			SaintBarthelemy,
			SaintMartin,
			Singapore,
			SouthAfrica,
			Taiwan,
			UnitedKingdom,
			UnitedStates
		);

		internal static readonly ImmutableArray<string> HasDeclarationInDevelopmentCountryCodes = ImmutableArray.Create(
			Belgium,
			Brazil,
			China,
			Denmark,
			India,
			Ireland,
			Israel,
			Italy,
			Japan,
			KoreaSouth,
			Mexico,
			Netherlands,
			Norway,
			Poland,
			Spain,
			Sweden,
			Switzerland,
			Turkey,
			UnitedArabEmirates
		);

		public static bool IsUsedToBeBuiltInOnlyCountry(ZString countryCode) => UsedToBeBuiltInOnlyCountryCodes.Contains(countryCode);

		public static bool IsABMInterfaceActivatedByCompany(ZString countryCode, Guid companyPK)
		{
			return CustomsWareInstallations(countryCode) &&
				(!string.IsNullOrWhiteSpace(CustomsDataRegistry.Instance.CustomsWareCompany.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty)) || HasCustomsWareSettings(companyPK));
		}

		static bool HasCustomsWareSettings(Guid companyPK)
		{
			var customsWareRegistry = ObjectFactory.Get<ICustomsWareRegistry>();
			return
				!string.IsNullOrWhiteSpace((string)customsWareRegistry.CustomsWareSiteID.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty)) ||
				!string.IsNullOrWhiteSpace((string)customsWareRegistry.UserName.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty)) ||
				!string.IsNullOrWhiteSpace((string)customsWareRegistry.Password.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty));
		}

		public static bool IsCustomsInterfaceActivatedByCompany(ZGuid companyPK)
		{
			var customsInterface = CustomsDataRegistry.Instance.LocalCountryCustomsInterface.GetFallBackValueAtAllLevels(companyPK.IsValid ? companyPK.ToGuid() : Guid.Empty, Guid.Empty, Guid.Empty);
			return customsInterface.CustomsInterfaceIsActivated();
		}

		public static bool CustomsInterfaceIsActivated(this LocalCountryCustomsInterface customsInterface) => customsInterface != null && !customsInterface.RecipientID.IsEmpty;

		public static bool CountryHasBuiltInDeclaration(ZString countryCode) => HasBuiltInDeclarationCountryCodes.Contains(countryCode) || IsAsycudaCustomsCountryCode(countryCode);
		public static bool CountryHasDeclarationInDevelopment(ZString countryCode) => HasDeclarationInDevelopmentCountryCodes.Contains(countryCode);

		static readonly ImmutableArray<ZString> UsedToBeBuiltInOnlyCountryCodes = ImmutableArray.Create<ZString>(
			Australia,
			Canada,
			NewZealand,
			UnitedKingdom,
			Singapore,
			_TemplateCountryName_,
			Eritrea,
			Latvia,
			UnitedStates,
			PuertoRico);

		public static bool IsAsycudaCustomsCountryCode(ZString countryCode) => AsycudaCustomsCountryCodes.Contains(countryCode);
		internal static ZString[] AsycudaCustomsCountryCodes => ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().GetAsycudaCustomsCountryCodes();

		public static bool IsInterfaceOnlySupported(string countryCode)
		{
			var declarationSubmissionProviders = ObjectFactory.Get<Hashtable>("DeclarationSubmissionProviders");
			if (declarationSubmissionProviders[countryCode] is ObjectHandle handle && handle.GetObject() is Integration.Customs.IDeclarationSubmissionProvider declarationSubmissionProvider)
			{
				return declarationSubmissionProvider.IsInterfaceOnlySupported();
			}

			return !CountryHasBuiltInDeclaration(countryCode) && !CountryHasDeclarationInDevelopment(countryCode);
		}
	}
}
