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
	public partial class APHISCategoryTypeCodeList
	{
		public static ICodeDescriptionPairList GetListForProgram(BusinessObjectFactory factory, ZString programCode)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>("APHISCategoryTypeCodeList_" + programCode, () =>
				{
					var result = new CodeDescriptionPairList();
					switch (programCode)
					{
						case APHISProgramCodeList.Codes.AAC:
							result.AddPair(Codes.LiveAnimals, Descriptions.LiveAnimals);
							break;
						case APHISProgramCodeList.Codes.ABS:
							result.AddPair(Codes.GeneticallyEngineeredOrganisms, Descriptions.GeneticallyEngineeredOrganisms);
							break;
						case APHISProgramCodeList.Codes.APQ:
							result.AddPair(Codes.PropagativeMaterial, Descriptions.PropagativeMaterial);
							result.AddPair(Codes.SeedsNotForPlanting, Descriptions.SeedsNotForPlanting);
							result.AddPair(Codes.FruitsAndVegetables, Descriptions.FruitsAndVegetables);
							result.AddPair(Codes.MiscellaneousAndProcessedProducts, Descriptions.MiscellaneousAndProcessedProducts);
							result.AddPair(Codes.CutFlowersAndGreenery, Descriptions.CutFlowersAndGreenery);
							break;
						case APHISProgramCodeList.Codes.AVS:
							result.AddPair(Codes.LiveAnimals, Descriptions.LiveAnimals);
							result.AddPair(Codes.RelatedAnimalProducts, Descriptions.RelatedAnimalProducts);
							result.AddPair(Codes.AnimalProductsAndAnimalByProducts, Descriptions.AnimalProductsAndAnimalByProducts);
							break;
					}
					return result;
				});
		}
	}
}
