using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	static class TMiningAddressExtensions
	{
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

		#region EoriNumber

		public static RegistrationNumber GetEoriNumber(this OrgAddress address, IContext context, string countryCode = null, bool useDefaultCountry = true)
		{
			var eoriLookup = new CodeDescriptionPairList();
			eoriLookup.AddPair(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, Res.GetString("f1fd2f5e-48c3-4206-acf2-bceb136115f0", "EORI Number"));

			var orgCusCode = address.GetRegistrationNumberObjectWithFallbackToOrgHeader(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, countryCode, useDefaultCountry);

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

		#region GetPartyIdWithFallback

		public static RegistrationNumber GetPartyIdWithFallback(this OrgAddress address, IContext context)
		{
			var registrationNumber = GetDunsNumber(address, context, false);

			if (registrationNumber.Value.IsEmpty)
			{
				registrationNumber = GetEoriNumber(address, context);
			}

			return registrationNumber;
		}

		#endregion
	}
}
