using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public sealed class NorwayOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeCustomsRegNoValidationProvider, IOrgCusCodeProvider
	{
		public static class OrgCusCodes
		{
			public const string SSN = "SSN";
		}

		ZString CountryCode => Core.Constants.CountryCodes.Norway;

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode orgCusCode)
		{
			switch (orgCusCode.OK_CodeType)
			{
				case OrgCusCodes.SSN:
					var validation = NorwayPersonalNumberValidator.Validate(orgCusCode.OK_CustomsRegNo);
					if (!validation.Success)
					{
						orgCusCode.OK_CustomsRegNoInfo.AddError(validation.ErrorMessage);
					}
					break;
			}

			OrgCusCodeValidation.ValidateCustomsCodeEORI(orgCusCode);
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.NorwayCodeTypes.MVA, Country.TaxCodeDescriptions.VATCodeTemplate(OrgCusCode.NorwayCodeTypes.MVA)); // Accounting consumption code

			list.AddPair(OrgCusCodes.SSN, Res.GetString("EBFD5C0A-D108-11BE-4F23-E7014F5FC24B", "Social Security Number"));
			list.AddPair(OrgCusCode.CodeTypes.OrganizationNumber, Res.GetString("FB632A88-7E1E-8798-4CA5-0A6746859E07", "Business Registration Number"));
			list.AddPair(OrgCusCode.CodeTypes.CustomsOfficeForTransit, Res.GetString("C1D1D176-25FA-FA95-4263-825B4396A93C", "Customs Office For Transit"));
			list.AddPair(OrgCusCode.NorwayCodeTypes.EMD, Res.GetString("3EC477B5-8B77-4B6D-810F-D36C558F4694", "EMMA Declarant Code"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(CountryCode);
			result.Add(OrgCusCode.NorwayCodeTypes.MVA);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(CountryCode);
			result.Add(OrgCusCode.NorwayCodeTypes.MVA);
			result.Remove(OrgCusCode.CodeTypes.CorporationCode);
			return result;
		}
	}
}
