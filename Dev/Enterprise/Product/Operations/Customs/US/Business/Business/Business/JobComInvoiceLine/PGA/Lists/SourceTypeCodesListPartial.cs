using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	partial class SourceTypeCodesList
	{
		public static ICodeDescriptionPairList GetListForAPHIS(BusinessObjectFactory factory, ZString programCode)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>("SourceTypeCodesList_" + programCode, () =>
			{
				var result = new CodeDescriptionPairList();
				switch (programCode)
				{
					case APHISProgramCodeList.Codes.AAC:
						result.AddPair(Codes.CountryOfSpeciesOrigin, Descriptions.CountryOfSpeciesOrigin);
						break;
					case APHISProgramCodeList.Codes.APQ:
						result.AddPair(Codes.Harvested, Descriptions.Harvested);
						result.AddPair(Codes.PlaceOfGrowth, Descriptions.PlaceOfGrowth);
						break;
					case APHISProgramCodeList.Codes.ABS:
						result.AddPair(Codes.Harvested, Descriptions.Harvested);
						result.AddPair(Codes.PlaceOfGrowth, Descriptions.PlaceOfGrowth);
						result.AddPair(Codes.CountryOfStorage, Descriptions.CountryOfStorage);
						break;
					case APHISProgramCodeList.Codes.AVS:
						result.AddPair(Codes.CountryOfDeboning, Descriptions.CountryOfDeboning);
						result.AddPair(Codes.CountryOfManipulation, Descriptions.CountryOfManipulation);
						result.AddPair(Codes.CountryOfMeatCutting, Descriptions.CountryOfMeatCutting);
						result.AddPair(Codes.CountryOfPacking, Descriptions.CountryOfPacking);
						result.AddPair(Codes.CountryOfProcessing, Descriptions.CountryOfProcessing);
						result.AddPair(Codes.CountryOfProduction, Descriptions.CountryOfProduction);
						result.AddPair(Codes.CountryOfSlaughter, Descriptions.CountryOfSlaughter);
						result.AddPair(Codes.CountryOfSlicing, Descriptions.CountryOfSlicing);
						result.AddPair(Codes.CountryOfSource, Descriptions.CountryOfSource);
						result.AddPair(Codes.CountryOfSpeciesOrigin, Descriptions.CountryOfSpeciesOrigin);
						result.AddPair(Codes.PlaceOfPacking, Descriptions.PlaceOfPacking);
						break;
				}
				return result;
			});
		}

		public static ICodeDescriptionPairList GetListForNMFS(BusinessObjectFactory factory, ZString programCode)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>("SourceTypeCodesList_" + programCode, () =>
			{
				var result = new CodeDescriptionPairList();
				if (programCode == NMFSProgramCodeList.Codes.SIM || programCode == NMFSProgramCodeList.Codes.COA)
				{
					result.AddPair(Codes.HarvestOfCaptureFisheries, Descriptions.HarvestOfCaptureFisheries);
					result.AddPair(Codes.HatcheryBasedAquaculture, Descriptions.HatcheryBasedAquaculture);
					result.AddPair(Codes.SmallVesselHarvest, Descriptions.SmallVesselHarvest);
				}
				return result;
			});
		}

		public static ICodeDescriptionPairList GetListForFishingInformation(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>("SourceTypeCodesListForFishingInformation", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes.Vessel, Descriptions.Vessel);
				result.AddPair(Codes.HarvestOfCaptureFisheries, Descriptions.HarvestOfCaptureFisheries);
				result.AddPair(Codes.HatcheryBasedAquaculture, Descriptions.HatcheryBasedAquaculture);
				result.AddPair(Codes.SmallVesselHarvest, Descriptions.SmallVesselHarvest);
				return result;
			});
		}
	}
}
