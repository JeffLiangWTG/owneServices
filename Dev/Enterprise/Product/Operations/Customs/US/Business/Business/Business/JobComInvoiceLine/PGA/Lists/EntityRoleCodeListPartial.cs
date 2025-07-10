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
	partial class EntityRoleCodeList
	{
		public static ICodeDescriptionPairList GetListForAPHIS(BusinessObjectFactory factory, ZString categoryType)
		{
			var key = ZString.Empty;
			switch (categoryType)
			{
				case APHISCategoryTypeCodeList.Codes.LiveAnimals:
				case APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts:
				case APHISCategoryTypeCodeList.Codes.PropagativeMaterial:
				case APHISCategoryTypeCodeList.Codes.SeedsNotForPlanting:
				case APHISCategoryTypeCodeList.Codes.FruitsAndVegetables:
				case APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts:
				case APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms:
					key = APHISCategoryTypeCodeList.Codes.LiveAnimals;
					break;
				case APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery:
					key = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
					break;
			}

			return factory.GetCachedValue<ICodeDescriptionPairList>("EntityRoleCodeListForAPHIS_" + key, () =>
				{
					var result = new CodeDescriptionPairList();
					switch (key)
					{
						case APHISCategoryTypeCodeList.Codes.LiveAnimals:
							result.AddPair(Codes.CustomsBroker, Descriptions.CustomsBroker);
							result.AddPair(Codes.LPCOAuthorizedParty, Descriptions.LPCOAuthorizedParty);
							result.AddPair(Codes.UltimateConsignee, Descriptions.UltimateConsignee);
							break;
						case APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery:
							result.AddPair(Codes.CustomsBroker, Descriptions.CustomsBroker);
							result.AddPair(Codes.CropGrower, Descriptions.CropGrower);
							result.AddPair(Codes.UltimateConsignee, Descriptions.UltimateConsignee);
							break;
					}

					return result;
				});
		}

		public static ICodeDescriptionPairList GetListForNMFSSIM(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>("EntityRoleCodeListForNMFSSIM", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes.AquacultureFacility, Descriptions.AquacultureFacility);
				result.AddPair(Codes.Producer, Descriptions.Producer);
				result.AddPair(Codes.Buyer, Descriptions.Buyer);
				result.AddPair(Codes.Consignee, Descriptions.Consignee);
				result.AddPair(Codes.Exporter, Descriptions.Exporter);
				result.AddPair(Codes.Consignor, Descriptions.Consignor);
				return result;
			});
		}
	}
}
