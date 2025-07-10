using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;

namespace Enterprise.Customs.US.Business.Testing
{
	class EntityRoleCodeListTest : TestCaseWithFactory
	{
		public void TestGetListForNMFSSIM()
		{
			var fullList = new EntityRoleCodeList();

			var list1 = EntityRoleCodeList.GetListForNMFSSIM(Factory);
			var expectedCodes = new[] {
				EntityRoleCodeList.Codes.AquacultureFacility,
				EntityRoleCodeList.Codes.Producer,
				EntityRoleCodeList.Codes.Buyer,
				EntityRoleCodeList.Codes.Consignee,
				EntityRoleCodeList.Codes.Exporter,
				EntityRoleCodeList.Codes.Consignor
			};
			AssertEquals(expectedCodes.Length, list1.Count);
			foreach (var code in expectedCodes)
			{
				AssertEquals(code, fullList.GetDescriptionFromCode(code), list1.GetDescriptionFromCode(code));
			}
		}

		public void TestGetListForAPHIS()
		{
			var categoryTypes = new APHISCategoryTypeCodeList();
			var list1 = EntityRoleCodeList.GetListForAPHIS(Factory, APHISCategoryTypeCodeList.Codes.LiveAnimals);
			ICodeDescriptionPairList list2 = null;
			foreach (var code in new[]
			{
				APHISCategoryTypeCodeList.Codes.LiveAnimals,
				APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts,
				APHISCategoryTypeCodeList.Codes.PropagativeMaterial,
				APHISCategoryTypeCodeList.Codes.SeedsNotForPlanting,
				APHISCategoryTypeCodeList.Codes.FruitsAndVegetables,
				APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts,
				APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms
			})
			{
				categoryTypes.RemoveCode(code);
				list2 = EntityRoleCodeList.GetListForAPHIS(Factory, code);
				AssertEquals("Cached", list1, list2);
				AssertEquals("Count", 3, list2.Count);
				AssertEquals(EntityRoleCodeList.Codes.CustomsBroker, EntityRoleCodeList.Descriptions.CustomsBroker, list2.GetDescriptionFromCode(EntityRoleCodeList.Codes.CustomsBroker));
				AssertEquals(EntityRoleCodeList.Codes.LPCOAuthorizedParty, EntityRoleCodeList.Descriptions.LPCOAuthorizedParty, list2.GetDescriptionFromCode(EntityRoleCodeList.Codes.LPCOAuthorizedParty));
				AssertEquals(EntityRoleCodeList.Codes.UltimateConsignee, EntityRoleCodeList.Descriptions.UltimateConsignee, list2.GetDescriptionFromCode(EntityRoleCodeList.Codes.UltimateConsignee));
			}
			categoryTypes.RemoveCode(APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery);
			list1 = EntityRoleCodeList.GetListForAPHIS(Factory, APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery);
			list2 = EntityRoleCodeList.GetListForAPHIS(Factory, APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery);
			AssertEquals("Cached", list1, list2);
			AssertEquals("Count", 3, list2.Count);
			AssertEquals(EntityRoleCodeList.Codes.CustomsBroker, EntityRoleCodeList.Descriptions.CustomsBroker, list2.GetDescriptionFromCode(EntityRoleCodeList.Codes.CustomsBroker));
			AssertEquals(EntityRoleCodeList.Codes.CropGrower, EntityRoleCodeList.Descriptions.CropGrower, list2.GetDescriptionFromCode(EntityRoleCodeList.Codes.CropGrower));
			AssertEquals(EntityRoleCodeList.Codes.UltimateConsignee, EntityRoleCodeList.Descriptions.UltimateConsignee, list2.GetDescriptionFromCode(EntityRoleCodeList.Codes.UltimateConsignee));
			list1 = EntityRoleCodeList.GetListForAPHIS(Factory, "");
			foreach (ICodeDescription pair in categoryTypes)
			{
				list2 = EntityRoleCodeList.GetListForAPHIS(Factory, pair.Code);
				AssertEquals("Cached", list1, list2);
				AssertEquals("Count", 0, list2.Count);
			}
		}
	}
}
