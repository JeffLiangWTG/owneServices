using System.Collections.Generic;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public class BrazilOrgCusCodeInfo :
		OrgCusCodeInfo,
		IOrgCusCodeProvider,
		IOrgCusCodeUniqueValidation,
		IOrgCusCodeCustomsRegNoValidationProvider,
		IOrgCusCodeLookupsProvider
	{
		ZString CountryCode => CountryCodes.Brazil;

		public static class OrgCusCodes
		{
			public const string CMT = "CMT";
			public const string CNPJ = "CJN";
			public const string IndividualTaxPayerRegistration = "CPF";
			public const string StateTaxPayerRegistration = "IEF";
			public const string MunicipalTaxPayerRegistration = "IMF";
			public const string OccupationCodesOfIndividuals = "CBO";
			public const string WorkerIdentificationNumber = "NIT";
			public const string RSN = "RSN";
			public const string RLR = "RLR";
			public const string RLP = "RLP";
			public const string AEO = "AEO";
			public const string TIN = "TIN";
			public const string MunicipalRegistrationTaxAuthority = "IMM";
			public const string RootCNPJ = "RTC";
			public const string ForeignOperatorInternalCode = "FOI";
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.CMT, Country.GetDefaultTaxCodeDescription(BrazilOrgCusCodeInfo.OrgCusCodes.CMT)); // Accounting consumption code

			list.RemoveCode(OrgCusCode.CodeTypes.TaxFileCode);
			list.OverridePair(OrgCusCode.CodeTypes.CorporationCode, string.Format(CultureInfo.InvariantCulture, (NoResString)"NIRE Número de Identificação no Registro de Empresas / {0}", Res.GetString("OrgCusCode.CodeTypes.CompanyRegistrationNumber", "Company Registration Number")));
			list.AddPair(OrgCusCodes.CNPJ, string.Format(CultureInfo.InvariantCulture, (NoResString)"CNPJ Cadastro Nacional da Pessoa Jurídica / {0}", Res.GetString("8FB36C45-75BF-40E9-96C1-E4E0D5CB8EA3", "Business Tax Payer Registration")));
			list.AddPair(OrgCusCodes.IndividualTaxPayerRegistration, string.Format(CultureInfo.InvariantCulture, (NoResString)"Cadastro de Pessoa Fisica / {0}", Res.GetString("1AF3AB5D-2E00-498F-8412-1FC5E8BC7715", "Individual Tax Payer Registration")));
			list.AddPair(OrgCusCodes.StateTaxPayerRegistration, string.Format(CultureInfo.InvariantCulture, (NoResString)"IE Inscrição Estadual / {0}", Res.GetString("A753C980-7A40-4FD2-B5A5-7E4E0688BF93", "State Tax Payer Registration")));
			list.AddPair(OrgCusCodes.MunicipalTaxPayerRegistration, string.Format(CultureInfo.InvariantCulture, (NoResString)"IM Inscrição Municipal / {0}", Res.GetString("78183F1B-6E88-4E33-A7A9-061B11EB8E43", "Municipal Tax Payer Registration")));
			list.AddPair(OrgCusCodes.OccupationCodesOfIndividuals, string.Format(CultureInfo.InvariantCulture, (NoResString)"Códigos Brasileiro de Operações / {0}", Res.GetString("07AA75FE-AB2A-4CD8-91BF-14AE44370950", "Occupation Codes of Individuals")));
			list.AddPair(OrgCusCodes.WorkerIdentificationNumber, string.Format(CultureInfo.InvariantCulture, (NoResString)"Número de Identificação do Trabalhador / {0}", Res.GetString("51F26AEE-8049-45EF-9BDC-F7353E61BABD", "Worker Identification Number")));
			list.AddPair(OrgCusCodes.TIN, string.Format(CultureInfo.InvariantCulture, (NoResString)"Número de Identificação do Comerciante / {0}", Res.GetString("373E2123-7892-41F9-9442-5D6EC3F74734", "Trader Identification Number")));
			list.AddPair(OrgCusCodes.RSN, string.Format(CultureInfo.InvariantCulture, (NoResString)"Regime Simples Nacional / {0}", Res.GetString("8FF74D9E-F0E9-4C0E-9324-CD9B729C2650", "Simplified Regime")));
			list.AddPair(OrgCusCodes.RLR, string.Format(CultureInfo.InvariantCulture, (NoResString)"Regime Lucro Real / {0}", Res.GetString("C99B01EF-B279-4999-A0CD-D00278F5A4C4", "Real Profit Regime")));
			list.AddPair(OrgCusCodes.RLP, string.Format(CultureInfo.InvariantCulture, (NoResString)"Regime Lucro Presumido / {0}", Res.GetString("B91F3C3B-2B08-4375-8497-3D47FF65BF95", "Presumed Profit Regime")));
			list.AddPair(OrgCusCodes.AEO, Res.GetString("B3C2FE90-60FA-45B1-9E96-859CADA0C683", "Authorized Economic Operator"));
			list.AddPair(OrgCusCodes.MunicipalRegistrationTaxAuthority, string.Format(CultureInfo.InvariantCulture, (NoResString)"Municipio de Registro da Inscrição Municipal / {0}", Res.GetString("383A6116-DEBE-457E-AAEB-DE945BD0FC56", "Municipal Registration Tax Authority")));
			list.AddPair(OrgCusCodes.RootCNPJ, string.Format(CultureInfo.InvariantCulture, (NoResString)"CNPJ Raiz / {0}", Res.GetString("BE1E0956-A0B0-46CB-9829-303BCEBF0F33", "Root CNPJ")));
			list.AddPair(OrgCusCodes.ForeignOperatorInternalCode, string.Format(CultureInfo.InvariantCulture, (NoResString)"Código Interno do Operador Estrangeiro / {0}", Res.GetString("2CDC36CA-AE81-42EA-A38F-FBB32FAA3F33", "Foreign Operator Internal Code")));

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(CountryCode);
			result.Add(OrgCusCodes.RSN);
			result.Add(OrgCusCodes.RLR);
			result.Add(OrgCusCodes.RLP);
			result.Add(OrgCusCodes.IndividualTaxPayerRegistration);
			result.Add(OrgCusCodes.StateTaxPayerRegistration);
			result.Add(OrgCusCodes.MunicipalTaxPayerRegistration);
			result.Add(OrgCusCodes.MunicipalRegistrationTaxAuthority);

			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(CountryCode);
			result.Add(OrgCusCodes.RSN);
			result.Add(OrgCusCodes.RLR);
			result.Add(OrgCusCodes.RLP);
			result.Add(OrgCusCodes.IndividualTaxPayerRegistration);
			result.Add(OrgCusCodes.OccupationCodesOfIndividuals);
			result.Add(OrgCusCodes.StateTaxPayerRegistration);
			result.Add(OrgCusCodes.MunicipalTaxPayerRegistration);
			result.Add(OrgCusCodes.MunicipalRegistrationTaxAuthority);

			result.Remove(OrgCusCode.CodeTypes.TaxFileCode);

			return result;
		}

		HashSet<string>[] IOrgCusCodeUniqueValidation.GetCodesCannotCoexist()
		{
			var result = new[]
			{
				new HashSet<string>
				{
					OrgCusCodes.RSN,
					OrgCusCodes.RLR,
					OrgCusCodes.RLP
				}
			};

			return result;
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode orgCusCode)
		{
			if (orgCusCode.OK_CodeType == OrgCusCodes.CNPJ)
			{
				if (!CNPJValidator.ValidateCNPJ(orgCusCode.OK_CustomsRegNo))
				{
					orgCusCode.OK_CustomsRegNoInfo.AddErrorIfEnforced(Res.GetString("f2d87169-a265-4894-9929-d382f30f344e", "The entered CNPJ is not valid.\r\n\r\nA CNPJ is a specially provided number built with a checksum to ensure its validity. An error on this field indicates that the given number is DEFINITELY INVALID."), OrganisationRegistry.RegistrationNumberFormatFields.BRCNPJ);
				}
			}

			if (orgCusCode.OK_CodeType == OrgCusCodes.MunicipalRegistrationTaxAuthority)
			{
				var hasNoBRIMF = orgCusCode.Header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCodes.MunicipalTaxPayerRegistration, CountryCodes.Brazil) == null;
				if (hasNoBRIMF)
				{
					orgCusCode.OK_CustomsRegNoInfo.AddWarning(Res.GetString("A8710EE6-6BB9-40CC-93AE-7B28EADDF314", "IMM codes should only be recorded when there is an IMF code."));
				}
			}
		}

		CodeDescriptionPairList IOrgCusCodeLookupsProvider.GetCustomsRegNoLookupList(ZString orgCusCodeType)
		{
			if (orgCusCodeType == OrgCusCodes.MunicipalRegistrationTaxAuthority)
			{
				return ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().GetTaxAuthorities(CountryCode, AccountingMasterFilesTaxFrameworkConstants.TaxAuthorityTypeList.Municipal.Code);
			}

			return null;
		}

		string IOrgCusCodeLookupsProvider.GetCustomsRegNoFieldType(ZString orgCusCodeType)
		{
			if (orgCusCodeType == OrgCusCodes.MunicipalRegistrationTaxAuthority)
			{
				return nameof(FieldType.TextDropEdit);
			}

			return null;
		}
	}
}

