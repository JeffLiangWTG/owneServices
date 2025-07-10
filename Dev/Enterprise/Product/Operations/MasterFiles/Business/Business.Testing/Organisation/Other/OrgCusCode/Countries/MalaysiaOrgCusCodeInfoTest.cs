using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class MalaysiaOrgCusCodeInfoTest : TestCaseWithFactory
	{
		string CountryCode => Core.Constants.CountryCodes.Malaysia;

		public void TestIOrgCusCodeCustomsRegNoValidationProvider_Validate()
		{
			var validationProvider = OrgCusCodeCountryFactory.GetIOrgCusCodeCustomsRegNoValidationProvider(CountryCode);
			AssertNotNull("Pre-condition", validationProvider);

			AssertIsValidating(MalaysiaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber, "123", true);
			AssertIsValidating(MalaysiaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber, "C0123456789");
			AssertIsValidating(MalaysiaOrgCusCodeInfo.OrgCusCodes.OtherBusinessCode, "123", true);
			AssertIsValidating(MalaysiaOrgCusCodeInfo.OrgCusCodes.OtherBusinessCode, "C123");

			void AssertIsValidating(ZString codeType, ZString invalidValue, bool hasError = false)
			{
				var code = Factory.NewWithValidTestData<OrgCusCode>();
				AssertEquals("Pre-condition", false, code.OK_CustomsRegNoInfo.HasErrors());

				using (code.SuspendValidationTesting())
				{
					code.OK_CodeType = codeType;
					code.OK_CustomsRegNo = invalidValue;
					validationProvider.Validate(code);

					AssertEquals("Should has error", hasError, code.OK_CustomsRegNoInfo.HasErrors());
				}
			}
		}

		public void TestIOrgCusCodeStandardIndustrialClassificationNoValidationProvider_ValidateStandardIndustrialClassification()
		{
			var validationProvider = OrgCusCodeCountryFactory.GetIOrgCusCodeStandardIndustrialClassificationNoValidationProvider(CountryCode);
			AssertNotNull("Pre-condition", validationProvider);

			var mYMSICCodes = new List<string>
			{
				"00000",
				"49110",
				"49120",
				"49211",
				"49212",
				"49221",
				"49222",
				"49223",
				"49224",
				"49225",
				"49229",
				"49230",
				"49300",
				"50111",
				"50112",
				"50113",
				"50121",
				"50122",
				"50211",
				"50212",
				"50220",
				"51101",
				"51102",
				"51103",
				"51201",
				"51202",
				"51203",
				"52100",
				"52211",
				"52212",
				"52213",
				"52214",
				"52219",
				"52221",
				"52222",
				"52229",
				"52231",
				"52232",
				"52233",
				"52234",
				"52239",
				"52241",
				"52249",
				"52291",
				"52292",
				"52299",
				"53100",
				"53200"
			};

			foreach (var code in mYMSICCodes)
			{
				AssertIsValidating(code, false);
			}

			AssertIsValidating("11111", true);
			AssertIsValidating("AAAAA", true);

			void AssertIsValidating(ZString value, bool hasError = false)
			{
				var code = Factory.NewWithValidTestData<OrgCusCode>();
				AssertEquals("Pre-condition", false, code.OK_CustomsRegNoInfo.HasErrors());

				using (code.SuspendValidationTesting())
				{
					code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Malaysia;
					code.OK_CodeType = OrgCusCode.CodeTypes.StandardIndustrialClassification;
					code.OK_CustomsRegNo = value;

					if (hasError)
					{
						var errorMessage = "The MY SIC number '{0}' is invalid. It should be a valid MSIC code in format 'NNNNN'.";

						using (SetStrictEnforcementOfRegistrationNumberFormats(OrganisationRegistry.RegistrationNumberFormatFields.MYSIC, true))
						{
							validationProvider.ValidateStandardIndustrialClassification(code);
							AssertHasError(code.OK_CustomsRegNoInfo, string.Format(errorMessage, value));
						}

						using (SetStrictEnforcementOfRegistrationNumberFormats(OrganisationRegistry.RegistrationNumberFormatFields.MYSIC, false))
						{
							validationProvider.ValidateStandardIndustrialClassification(code);
							AssertHasWarning(code.OK_CustomsRegNoInfo, string.Format(errorMessage, value));
						}
					}
					else
					{
						AssertEquals("Should not have error", false, code.OK_CustomsRegNoInfo.HasErrors());
					}
				}
			}
		}

		IDisposable SetStrictEnforcementOfRegistrationNumberFormats(ZString registryCode, bool isValidated)
		{
			var collection = OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.Value;
			collection.Cast<CodeDescriptionBool>().Single(x => x.Code == registryCode).Bool = isValidated;

			return OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}
	}
}
