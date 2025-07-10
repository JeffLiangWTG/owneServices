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
	partial class AnimalProductsAndByProductsConditionA32List
	{
		public static ICodeDescriptionPairList GetListForPoultry(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>("AnimalProductsAndByProductsConditionA32List_Poultry", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Codes.AvesPoultryProducts, Descriptions.AvesPoultryProducts);
					return result;
				});
		}

		public static ICodeDescriptionPairList GetListFor06_09_13_15_16(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>("AnimalProductsAndByProductsConditionA32List_06_09_13_15_16", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes.AvesPoultryProducts, Descriptions.AvesPoultryProducts);
				result.AddPair(Codes.BovineBeefProducts, Descriptions.BovineBeefProducts);
				result.AddPair(Codes.CamelidCamelProducts, Descriptions.CamelidCamelProducts);
				result.AddPair(Codes.CapraGoatProducts, Descriptions.CapraGoatProducts);
				result.AddPair(Codes.CervidDeerElkAndMooseProducts, Descriptions.CervidDeerElkAndMooseProducts);
				result.AddPair(Codes.EquineHorseProducts, Descriptions.EquineHorseProducts);
				result.AddPair(Codes.OtherRuminantProducts, Descriptions.OtherRuminantProducts);
				result.AddPair(Codes.OvisSheepProducts, Descriptions.OvisSheepProducts);
				result.AddPair(Codes.SusPorkProducts, Descriptions.SusPorkProducts);
				result.AddPair(Codes.TrichosurusBrushtailPossumProducts, Descriptions.TrichosurusBrushtailPossumProducts);
				return result;
			});
		}
	}
}
