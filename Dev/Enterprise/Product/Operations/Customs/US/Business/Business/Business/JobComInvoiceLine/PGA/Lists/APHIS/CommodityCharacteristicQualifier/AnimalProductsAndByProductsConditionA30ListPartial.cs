using CargoWise.EntityFramework;
using CargoWise.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.APHIS.CommodityCharacteristicQualifier
{
	partial class AnimalProductsAndByProductsConditionA30List
	{
		public static ICodeDescriptionPairList GetListForAnimalConsumption(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>("AnimalProductsAndByProductsConditionA30List_AnimalConsumption", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Codes.Inedible, Descriptions.Inedible);
					return result;
				});
		}
	}
}
