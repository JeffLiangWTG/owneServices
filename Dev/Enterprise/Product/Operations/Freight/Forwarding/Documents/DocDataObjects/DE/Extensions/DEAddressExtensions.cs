using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.DE
{
	static class DEAddressExtensions
	{
		#region PortCarrierCode

		public static RegistrationNumber GetPortCarrierCode(this OrgAddress address, string operationalPort, IContext context)
		{
			var registrationNumberValue = ZString.Empty;
			var portCodeLookup = new CodeDescriptionPairList();
			string portCode = GermanyOrgCusCodeInfo.OrgCusCodes.BHT;
			string portCodeDescription = Res.GetString("d9f669ac-03ed-409a-bc3e-8ba06b4bcee9", "Carrier code for BHT Port Community System");
			portCodeLookup.AddPair(portCode, portCodeDescription);
			registrationNumberValue = address.GetRegistrationNumberWithFallbackToOrgHeader(portCode, Core.Constants.CountryCodes.Germany);

			return new RegistrationNumber()
			{
				Value = address.GetRegistrationNumberWithFallbackToOrgHeader(portCode, Core.Constants.CountryCodes.Germany),
				Type = new CodeDescription(portCodeLookup)
				{
					Code = portCode
				},
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.Germany
				}
			};
		}

		#endregion

		#region WarehouseCode
		public static RegistrationNumber GetWarehouseCode(this OrgAddress address, IContext context)
		{
			var registrationNumberValue = ZString.Empty;
			string warehouseCode = GermanyOrgCusCodeInfo.OrgCusCodes.CWC;
			registrationNumberValue = address.GetRegistrationNumberWithFallbackToOrgHeader(warehouseCode, Core.Constants.CountryCodes.Germany);
			if (registrationNumberValue.IsEmpty)
			{
				warehouseCode = OrgCusCode.CodeTypes.EDISiteID;
				registrationNumberValue = address.GetRegistrationNumberWithFallbackToOrgHeader(warehouseCode, Core.Constants.CountryCodes.Germany);
			}
			var warehouseCodeLookup = new CodeDescriptionPairList();
			string warehouseCodeDescription = warehouseCode == GermanyOrgCusCodeInfo.OrgCusCodes.CWC
				? Res.GetString("6077f091-589a-4c24-9e4e-586f102944b0", "Customs Warehouse Code")
				: Res.GetString("6e44cf3e-c759-4063-8fa8-fc2e8754d6aa", "EDI Site ID");
			warehouseCodeLookup.AddPair(warehouseCode, warehouseCodeDescription);

			return new RegistrationNumber()
			{
				Value = registrationNumberValue,
				Type = new CodeDescription(warehouseCodeLookup)
				{
					Code = warehouseCode
				},
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.Germany
				}
			};
		}

		#endregion

		#region GetEoriBranchSuffix

		public static RegistrationNumber GetEoriBranchSuffix(this OrgAddress address, IContext context, string countryCode = null, bool useDefaultCountry = true)
		{
			var ebsLookup = new CodeDescriptionPairList();
			ebsLookup.AddPair(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, Res.GetString("47CE542C-D04C-4081-A7EA-0DAEA12D1F1A", "EORI Branch Suffix"));

			var orgCusCode = address.GetRegistrationNumberObjectWithFallbackToOrgHeader(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, countryCode, useDefaultCountry);

			return new RegistrationNumber()
			{
				Value = orgCusCode?.OK_CustomsRegNo ?? ZString.Empty,
				Type = new CodeDescription(ebsLookup)
				{
					Code = GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix
				},
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = orgCusCode?.OK_RN_NKCodeCountry ?? ZString.Empty
				}
			};
		}

		#endregion
	}
}
