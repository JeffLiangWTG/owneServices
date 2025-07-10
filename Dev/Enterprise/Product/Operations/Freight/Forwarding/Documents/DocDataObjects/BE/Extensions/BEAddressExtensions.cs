using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE
{
	static class BEAddressExtensions
	{
		#region PortSystemNumber

		public static RegistrationNumber GetPortSystemNumber(this OrgAddress address, IContext context)
		{
			var portSystemLookup = new CodeDescriptionPairList();
			portSystemLookup.AddPair(OrgCusCode.CodeTypes.PortSystemNumber, Res.GetString("9819a4db-2967-4420-a889-d7e5bc5a8028", "Port System Number"));

			return new RegistrationNumber()
			{
				Value = address.GetRegistrationNumberWithFallbackToOrgHeader(OrgCusCode.CodeTypes.PortSystemNumber, Core.Constants.CountryCodes.Belgium),
				Type = new CodeDescription(portSystemLookup)
				{
					Code = OrgCusCode.CodeTypes.PortSystemNumber
				},
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.Belgium
				}
			};
		}

		#endregion

		#region EoriNumber

		public static RegistrationNumber GetEoriNumber(this OrgAddress address, IContext context, bool useDefaultCountry = true)
		{
			var eoriLookup = new CodeDescriptionPairList();
			eoriLookup.AddPair(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, Res.GetString("f1fd2f5e-48c3-4206-acf2-bceb136115f0", "EORI Number"));

			var orgCusCode = address.GetRegistrationNumberObjectWithFallbackToOrgHeader(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, null, useDefaultCountry);

			return new RegistrationNumber()
			{
				Value = orgCusCode?.OK_CustomsRegNo ?? ZString.Empty,
				Type = new CodeDescription(eoriLookup)
				{
					Code = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori
				},
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = orgCusCode?.OK_RN_NKCodeCountry ?? ZString.Empty
				}
			};
		}

		#endregion

		#region DunsNumber

		public static RegistrationNumber GetDunsNumber(this OrgAddress address, IContext context, bool useDefaultCountry = true)
		{
			var dunsLookup = new CodeDescriptionPairList();
			dunsLookup.AddPair(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, Res.GetString("E8105499-359D-4EEF-8D04-B8D0AB3305BA", "DUNS Number"));

			var orgCusCode = address.GetRegistrationNumberObjectWithFallbackToOrgHeader(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, null, useDefaultCountry);

			return new RegistrationNumber()
			{
				Value = orgCusCode?.OK_CustomsRegNo ?? ZString.Empty,
				Type = new CodeDescription(dunsLookup)
				{
					Code = OrgCusCode.CodeTypes.DataUniversalNumberingSystem
				},
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = orgCusCode?.OK_RN_NKCodeCountry ?? ZString.Empty
				}
			};
		}

		#endregion

		#region VatNumber

		public static RegistrationNumber GetBtwNumber(this OrgAddress address, IContext context)
		{
			var btwLookup = new CodeDescriptionPairList();
			btwLookup.AddPair(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, Res.GetString("7d45d90d-4031-4af3-9b73-47cbf72133e3", "BTW Number"));

			return new RegistrationNumber()
			{
				Value = address.GetRegistrationNumberWithFallbackToOrgHeader(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, Core.Constants.CountryCodes.Belgium),
				Type = new CodeDescription(btwLookup)
				{
					Code = OrgCusCode.EuropeanUnionSharedCodeTypes.BTW
				},
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.Belgium
				}
			};
		}

		#endregion

		#region GetPartyIdWithFallback

		public static RegistrationNumber GetPartyIdWithFallback(this OrgAddress address, IContext context)
		{
			var registrationNumber = new RegistrationNumber();

			registrationNumber = GetPortSystemNumber(address, context);
			if (registrationNumber.Value.IsEmpty)
			{
				registrationNumber = GetDunsNumber(address, context, false);
			}
			if (registrationNumber.Value.IsEmpty)
			{
				registrationNumber = GetEoriNumber(address, context);
			}
			if (registrationNumber.Value.IsEmpty)
			{
				registrationNumber = GetBtwNumber(address, context);
			}

			return registrationNumber;
		}

		#endregion
	}
}
