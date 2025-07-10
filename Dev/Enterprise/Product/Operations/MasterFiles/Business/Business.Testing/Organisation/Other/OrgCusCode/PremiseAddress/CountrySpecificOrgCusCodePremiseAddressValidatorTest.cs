using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(CountrySpecificOrgCusCodePremiseAddressValidator))]
	internal abstract class CountrySpecificOrgCusCodePremiseAddressValidatorTest<T> : TestCaseWithFactory
		where T : CountrySpecificOrgCusCodePremiseAddressValidator, new()
	{
		public void TestIsPremiseAddressRequired()
		{
			var validator = GetValidator();
			if (OrgCusCodeTypesForPremisesAddressRequired.Any())
			{
				foreach (var codeType in OrgCusCodeTypesForPremisesAddressRequired)
				{
					AssertEquals($"PremiseAddressRequired for {codeType}", true, validator.IsPremiseAddressRequired(codeType));
				}
			}
			else
			{
				AssertEquals($"PremiseAddressRequired will be false for any codeType", false, validator.IsPremiseAddressRequired(ZString.Empty));
			}
		}

		public void TestIsPremiseAddressAllowed()
		{
			var validator = GetValidator();

			if (OrgCusCodeTypesForPremisesAddressAllowed.Any() || OrgCusCodeTypesForNotAllowed.Any())
			{
				foreach (var codeType in OrgCusCodeTypesForPremisesAddressAllowed)
				{
					AssertEquals($"PremiseAddressAllowed for {codeType}", true, validator.IsPremiseAddressAllowed(codeType));
				}

				foreach (var codeType in OrgCusCodeTypesForNotAllowed)
				{
					AssertEquals($"PremiseAddress NOT Allowed for {codeType}", false, validator.IsPremiseAddressAllowed(codeType));
				}
			}
			else
			{
				AssertEquals($"PremiseAddressRequired will be false if no specified", false, validator.IsPremiseAddressAllowed(ZString.Empty));
			}
		}

		public void TestCheckPremisesAddressRequiredValidation()
		{
			var organisation = Factory.New<OrgHeader>();
			var customsCode = organisation.CustomsCodes.AddNew();

			CombineAssertions(() =>
			{
				foreach (var countryCode in CountryCodes)
				{
					customsCode.OK_RN_NKCodeCountry = countryCode;
					AssertNoErrors("No Error if No codeType", customsCode.OK_OA_PremisesAddressInfo);

					foreach (var codeType in OrgCusCodeTypesForPremisesAddressRequired)
					{
						AssertPremiseAddressRequired(customsCode, organisation.MainAddress.PK, codeType);
					}
				}
			});

			void AssertPremiseAddressRequired(OrgCusCode cusCode, ZGuid addressPk, ZString codeType)
			{
				cusCode.OK_CodeType = codeType;
				cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
				AssertHasErrorContaining($"An address is required for code type '{codeType}'", cusCode.OK_OA_PremisesAddressInfo, premisesAddressRequiredError);

				cusCode.OK_OA_PremisesAddress = addressPk;
				AssertNoErrorContaining($"No address required error for code type '{codeType}'", cusCode.OK_OA_PremisesAddressInfo, premisesAddressRequiredError);
			}
		}

		public void TestPremisesAddressDeniedValidation()
		{
			var organisation = Factory.New<OrgHeader>();
			var customsCode = organisation.CustomsCodes.AddNew();

			CombineAssertions(() =>
			{
				foreach (var countryCode in CountryCodes)
				{
					customsCode.OK_RN_NKCodeCountry = countryCode;
					AssertNoErrors("No Error if No codeType", customsCode.OK_OA_PremisesAddressInfo);

					var orgCusCodeTypesForRequiredOrAllowed = OrgCusCodeTypesForPremisesAddressRequired.Union(OrgCusCodeTypesForPremisesAddressAllowed);
					foreach (var codeType in orgCusCodeTypesForRequiredOrAllowed)
					{
						AssertPremiseAddressEnteredOnlyWhenAllowed(customsCode, organisation.MainAddress.PK, codeType, true);
					}

					foreach (var codeType in OrgCusCodeTypesForNotAllowed)
					{
						AssertPremiseAddressEnteredOnlyWhenAllowed(customsCode, organisation.MainAddress.PK, codeType, false);
					}
				}
			});

			void AssertPremiseAddressEnteredOnlyWhenAllowed(OrgCusCode cusCode, ZGuid addressPk, ZString codeType, bool isAllowed)
			{
				cusCode.OK_CodeType = codeType;
				cusCode.OK_OA_PremisesAddress = addressPk;
				if (isAllowed)
				{
					AssertNoErrors($"Address is allowed for code type '{codeType}'", cusCode.OK_OA_PremisesAddressInfo);
				}
				else
				{
					AssertHasError($"Address is NOT allowed for code type '{codeType}'", cusCode.OK_OA_PremisesAddressInfo, premisesAddressDeniedError);

					cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
					AssertNoError($"No error when no address for code type '{codeType}' ", cusCode.OK_OA_PremisesAddressInfo, premisesAddressDeniedError);
				}
			}
		}

		protected abstract string[] CountryCodes { get; }

		protected virtual string[] OrgCusCodeTypesForPremisesAddressRequired => Array.Empty<string>();

		protected virtual string[] OrgCusCodeTypesForPremisesAddressAllowed => Array.Empty<string>();

		protected virtual string[] OrgCusCodeTypesForNotAllowed => Array.Empty<string>();

		T GetValidator() => new T();

		const string premisesAddressRequiredError = "An address is required for code type '";
		const string premisesAddressDeniedError = "An address can only be entered for code types that have a physical premise associated with them.";
	}
}
