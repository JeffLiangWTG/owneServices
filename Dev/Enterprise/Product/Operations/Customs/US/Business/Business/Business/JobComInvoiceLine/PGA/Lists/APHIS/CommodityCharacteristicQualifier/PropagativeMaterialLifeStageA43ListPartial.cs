using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.US.Business.APHIS.ArticleCategory;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.APHIS.CommodityCharacteristicQualifier
{
	partial class PropagativeMaterialLifeStageA43List
	{
		public static ICodeDescriptionPairList GetPropagativeMaterialA43List(BusinessObjectFactory factory, ZString categoryCode, ZString physicalState)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>("APHISPropagativeMaterialLifeStageA43List" + categoryCode + physicalState, () =>
			{
				var result = new CodeDescriptionPairList();
				switch (categoryCode)
				{
					case PropagativeMaterialList.Codes.BulbsAndUndergroundPortionsOfDormantPerennials:
						break;
					case PropagativeMaterialList.Codes.PlantsForPlantingOrPropagationWhole:
						if (physicalState == PropagativeMaterialLifeStageA41List.Codes.WithRoots)
						{
							result.AddPair(Codes.ArtificialSoilless, Descriptions.ArtificialSoilless);
							result.AddPair(Codes.BareRootNoMedia, Descriptions.BareRootNoMedia);
							result.AddPair(Codes.Soil, Descriptions.Soil);
						}
						break;
					case PropagativeMaterialList.Codes.SeedsForPlantingForSowing:
						break;
					case PropagativeMaterialList.Codes.PlantCuttingsForPlantingOrPropagation:
						if (physicalState == PropagativeMaterialLifeStageA41List.Codes.WithRoots)
						{
							result.AddPair(Codes.ArtificialSoilless, Descriptions.ArtificialSoilless);
							result.AddPair(Codes.BareRootNoMedia, Descriptions.BareRootNoMedia);
							result.AddPair(Codes.Soil, Descriptions.Soil);
						}
						else if (physicalState == PropagativeMaterialLifeStageA41List.Codes.WithoutRoots)
						{
							result.AddPair(Codes.ArtificialSoilless, Descriptions.ArtificialSoilless);
							result.AddPair(Codes.BareRootNoMedia, Descriptions.BareRootNoMedia);
						}
						break;
					case PropagativeMaterialList.Codes.RootCuttingsOrRootCrownForPlantingOrPropagation:
						break;
					case PropagativeMaterialList.Codes.MeristemTissue:
						break;
					case PropagativeMaterialList.Codes.BudwoodGraftwood:
						if (physicalState == PropagativeMaterialLifeStageA41List.Codes.WithRoots)
						{
							result.AddPair(Codes.ArtificialSoilless, Descriptions.ArtificialSoilless);
							result.AddPair(Codes.BareRootNoMedia, Descriptions.BareRootNoMedia);
							result.AddPair(Codes.Soil, Descriptions.Soil);
						}
						else if (physicalState == PropagativeMaterialLifeStageA41List.Codes.WithoutRoots)
						{
							result.AddPair(Codes.BareRootNoMedia, Descriptions.BareRootNoMedia);
						}
						break;
				}

				return result;
			});
		}
	}
}
