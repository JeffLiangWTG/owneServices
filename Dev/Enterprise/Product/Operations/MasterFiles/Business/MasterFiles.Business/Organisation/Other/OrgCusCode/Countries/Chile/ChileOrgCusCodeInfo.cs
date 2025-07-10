using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class ChileOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider, IOrgCusCodeCustomsRegNoValidationProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.IVA, Country.GetDefaultTaxCodeDescription(OrgCusCode.CodeTypes.IVA)); // Accounting consumption code

			#region SuppressResourceStringsCheckRegion

			list.AddPair(OrgCusCodes.RUT, MultilingualString.Join(" / ", (NoResString)"RUT", ResString.GetMultilingualString("35d60870-e15f-4228-8a70-7f3ad2cfe547", "Tax Identification Number")));
			list.AddPair(OrgCusCodes.GEM, MultilingualString.Join(" / ", (NoResString)"Giro Empresa", ResString.GetMultilingualString("1e898cc6-5648-46d0-bd09-b08cf018c100", "Business Activity Description")));
			list.AddPair(OrgCusCodes.SOL, MultilingualString.Join(" / ", (NoResString)"RUT Solicitante", ResString.GetMultilingualString("10307c93-2e50-4c7b-8d6e-a24423d6a516", "Fiscal Representative Code")));
			list.AddPair(OrgCusCodes.AEO, Res.GetString("OrgCusCode.ChileCodeTypes.AEO", "Authorized Economic Operator"));
			list.AddPair(OrgCusCodes.ACT, Res.GetString("DE31DB58-1FF6-475A-82D1-31D35AF8A4A7", "{0} / Business Activity Code", "Acteco"));
			list.AddPair(OrgCusCodes.REX, Res.GetString("OrgCusCode.EuropeanUnionSharedCodeTypes.RegisteredExporterNumber", "Registered Exporter Number"));

			#endregion

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			return OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Chile);
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.Chile);
			result.Add(OrgCusCodes.ACT);
			return result;
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode orgCusCode)
		{
			switch (orgCusCode.OK_CodeType)
			{
				case OrgCusCodes.RUT:
				case OrgCusCodes.SOL:
					new CLRUTAndSOLValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
					break;
				case OrgCusCodes.ACT:
					new CLACTValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
					break;
			}
		}

		public static class OrgCusCodes
		{
			public const string RUT = "RUT";
			public const string GEM = "GEM";
			public const string SOL = "SOL";
			public const string AEO = "AEO";
			public const string ACT = "ACT";
			public const string REX = "REX";
		}
	}
}
