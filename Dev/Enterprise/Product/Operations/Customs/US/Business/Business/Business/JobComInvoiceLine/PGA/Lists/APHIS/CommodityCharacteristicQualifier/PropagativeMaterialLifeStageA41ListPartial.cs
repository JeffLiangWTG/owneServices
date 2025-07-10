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
	partial class PropagativeMaterialLifeStageA41List
	{
		public static ICodeDescriptionPairList GetPropagativeMaterialLifeStageA41List(BusinessObjectFactory factory, ZString categoryCode)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>("APHISPropagativeMaterialLifeStageA41List" + categoryCode, () =>
			{
				var result = new CodeDescriptionPairList();
				switch (categoryCode)
				{
					case PropagativeMaterialList.Codes.BulbsAndUndergroundPortionsOfDormantPerennials:
						break;
					case PropagativeMaterialList.Codes.PlantsForPlantingOrPropagationWhole:
						result.AddPair(Codes.WithRoots, Descriptions.WithRoots);
						result.AddPair(Codes.WithoutRoots, Descriptions.WithoutRoots);
						break;
					case PropagativeMaterialList.Codes.SeedsForPlantingForSowing:
						result.AddPair(Codes.SmallSeedLot, Descriptions.SmallSeedLot);
						result.AddPair(Codes.SeedLargeLot, Descriptions.SeedLargeLot);
						result.AddPair(Codes.SeedEmbeddedObscured, Descriptions.SeedEmbeddedObscured);
						break;
					case PropagativeMaterialList.Codes.PlantCuttingsForPlantingOrPropagation:
						result.AddPair(Codes.WithRoots, Descriptions.WithRoots);
						result.AddPair(Codes.WithoutRoots, Descriptions.WithoutRoots);
						break;
					case PropagativeMaterialList.Codes.RootCuttingsOrRootCrownForPlantingOrPropagation:
						break;
					case PropagativeMaterialList.Codes.MeristemTissue:
						break;
					case PropagativeMaterialList.Codes.BudwoodGraftwood:
						result.AddPair(Codes.WithRoots, Descriptions.WithRoots);
						result.AddPair(Codes.WithoutRoots, Descriptions.WithoutRoots);
						break;
				}

				return result;
			});
		}
	}
}
