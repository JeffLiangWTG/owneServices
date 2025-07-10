using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class TurkeyOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider, IOrgCusCodeUniqueValidation, IOrgCusCodeNonUniqueProvider, IOrgCusCodeCustomsRegNoValidationProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, string.Format(CultureInfo.InvariantCulture, (NoResString)"Vergi Kimlik Numarası / {0}", Res.GetString("fbfe4abd-6b4c-4ac0-840d-d7a1d72a889a", "Tax & Business VAT Registration Number"))); // Accounting consumption code

			list.AddPair(OrgCusCodes.TradeRegistryNumber, string.Format(CultureInfo.InvariantCulture, (NoResString)"Ticaret Sicil Numarası / {0}", Res.GetString("OrgCusCode.CodeTypes.TradeRegistryNumber", "Trade Registry Number")));
			list.AddPair(OrgCusCodes.VDM, string.Format(CultureInfo.InvariantCulture, (NoResString)"Vergi Dairesi Müdürlüğü / {0}", Res.GetString("19419a08-3014-4504-ae86-d8901fafcc26", "Office of Tax & VAT Registration")));
			list.AddPair(OrgCusCodes.TCK, string.Format(CultureInfo.InvariantCulture, (NoResString)"TCKN - Türkiye Cumhuriyeti Kimlik Numarası / {0}", Res.GetString("73E0BDAB-9C9C-4B1D-A6B5-EF9B35933292", "Turkey Citizen Identification Number")));
			list.AddPair(OrgCusCodes.MER, string.Format(CultureInfo.InvariantCulture, (NoResString)"MERSIS Number - Merkezi Sicil Kayıt Sistemi / {0}", Res.GetString("16ADEC9D-E8CE-4CA2-8D85-FC1782A629C8", "Central Registry Number System")));
			list.AddPair(OrgCusCodes.PEC, string.Format(CultureInfo.InvariantCulture, (NoResString)"Borçlanan e-Fatura Posta Kutusu / {0}", Res.GetString("8C73F86A-9537-4084-AFAF-51F5FC9034D1", "Debtor e-Invoice Post Box Alias")));
			list.AddPair(OrgCusCodes.YFK, string.Format(CultureInfo.InvariantCulture, (NoResString)"Yabancı Firma Kayıt Sistemi (YFKS) Numarası / {0}", Res.GetString("FD17542C-02E4-455E-A542-ABF3ACC26008", "Foreign Company Registration System Number")));

			list.AddPair(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, Res.GetString("558CB332-5B0E-4911-85FE-DD6FED0E88F1", "Customs Bonded Warehouse"));
			list.AddPair(OrgCusCode.CodeTypes.TerminalControlledPremisesID, Res.GetString("F791F105-FBEC-4542-96AA-24B9837F2A81", "Customs Bonded Terminal"));
			list.AddPair(OrgCusCode.CodeTypes.CustomsOfficeForTransit, Res.GetString("OrgCusCode.CodeTypes.CustomsOfficeForTransitTR", "Customs Office For Transit"));

			// TODO: As of writing this, we are discussing if this condition should be removed.
			// See WI00825517 for the removal of this.
			if (new TurkeyComplianceInfo().IsCountryEnableComplianceEInvoicing())
			{
				list.AddPair(OrgCusCodes.VTE, string.Format(CultureInfo.InvariantCulture, (NoResString)"e-Fatura (Temel Fatura) / {0}", Res.GetString("27173a2a-89f2-4f26-8a5c-7eb65fdbf138", "e-Invoice (Basic Invoice) Customer")));
				list.AddPair(OrgCusCodes.VTC, string.Format(CultureInfo.InvariantCulture, (NoResString)"e-Fatura (Ticari Fatura) / {0}", Res.GetString("7E739014-AD06-4520-B655-C6B39B095A2F", "e-Invoice (Commercial Invoice) Customer")));
			}
			else
			{
				list.AddPair(OrgCusCodes.VTE, string.Format(CultureInfo.InvariantCulture, (NoResString)"eFactura / {0}", Res.GetString("0fc51541-12df-43b7-bf36-c5825bd1b5d3", "e-Invoice Customer")));
			}
			list.AddPair(OrgCusCodes.VTP, string.Format(CultureInfo.InvariantCulture, (NoResString)"İlgili Taraf Vergi Kimlik Numarası / {0}", Res.GetString("3384EDC3-DD82-464B-82B7-A36E61A68B8B", "VAT registration number of Third Party")));
			list.AddPair(OrgCusCodes.EOR, Res.GetString("2311C59C-8EFC-4D7E-A5AF-63C6B64517B6", "Economic Operators Registration and Identification Number"));

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Turkey);
			result.Add(OrgCusCodes.VDM);
			result.Add(OrgCusCodes.VTE);
			result.Add(OrgCusCodes.TCK);
			result.Add(OrgCusCodes.MER);
			result.Add(OrgCusCodes.PEC);

			// TODO: As of writing this, we are discussing if this condition should be removed.
			// See WI00825517 for the removal of this.
			if (new TurkeyComplianceInfo().IsCountryEnableComplianceEInvoicing())
			{
				result.Add(OrgCusCodes.VTC);
			}
			result.Add(OrgCusCodes.VTP);
			result.Add(OrgCusCodes.YFK);
			result.Add(OrgCusCodes.EOR);

			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.Turkey);
			result.Add(OrgCusCodes.VDM);
			result.Add(OrgCusCodes.VTE);
			result.Add(OrgCusCodes.TCK);
			result.Add(OrgCusCodes.MER);
			result.Add(OrgCusCodes.PEC);

			// TODO: As of writing this, we are discussing if this condition should be removed.
			// See WI00825517 for the removal of this.
			if (new TurkeyComplianceInfo().IsCountryEnableComplianceEInvoicing())
			{
				result.Add(OrgCusCodes.VTC);
			}
			result.Add(OrgCusCodes.VTP);
			result.Add(OrgCusCodes.YFK);
			result.Add(OrgCusCodes.EOR);

			return result;
		}

		HashSet<string>[] IOrgCusCodeUniqueValidation.GetCodesCannotCoexist()
		{
			var result = new[]  {
				new HashSet<string> { OrgCusCodes.VTE, OrgCusCodes.VTC }
			};

			return result;
		}

		HashSet<string> IOrgCusCodeNonUniqueProvider.GetNonUniqueCodes()
		{
			return new HashSet<string> { OrgCusCodes.VTC, OrgCusCodes.VTE, OrgCusCodes.VDM };
		}

		public static class OrgCusCodes
		{
			public const string TradeRegistryNumber = "TRN";
			public const string VDM = "VDM";
			public const string VTC = "VTC";
			public const string VTE = "VTE";
			public const string TCK = "TCK";
			public const string MER = "MER";
			public const string PEC = "PEC";
			public const string VTP = "VTP";
			public const string YFK = "YFK";
			public const string EOR = "EOR";
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode customsRegNo)
		{
			switch (customsRegNo.OK_CodeType)
			{
				case OrgCusCodes.PEC:
					MandatoryValidation.CheckEntered(customsRegNo.OK_CustomsRegNoInfo);
					PECRegistrationNumberValidator.ValidatePECRegistrationNumber(customsRegNo.OK_CustomsRegNoInfo);
					break;
			}

			OrgCusCodeValidation.ValidateCustomsCodeEORI(customsRegNo);
		}
	}
}
