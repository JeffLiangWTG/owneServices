using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	class APHISCategoryTypeCodeListTest : TestCaseWithFactory
	{
		public void TestGetListForProgram()
		{
			var fullList = new APHISCategoryTypeCodeList();
			var list1 = APHISCategoryTypeCodeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.AAC);
			var list2 = APHISCategoryTypeCodeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.AAC);
			AssertEquals("Data should be cached", list1, list2);
			AssertEquals(1, list1.Count);
			AssertEquals(APHISCategoryTypeCodeList.Descriptions.LiveAnimals, list1.GetDescriptionFromCode(APHISCategoryTypeCodeList.Codes.LiveAnimals));

			list1 = APHISCategoryTypeCodeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.ABS);
			list2 = APHISCategoryTypeCodeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.ABS);
			AssertEquals("Data should be cached", list1, list2);
			AssertEquals(1, list1.Count);
			AssertEquals(APHISCategoryTypeCodeList.Descriptions.GeneticallyEngineeredOrganisms, list1.GetDescriptionFromCode(APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms));

			list1 = APHISCategoryTypeCodeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.APQ);
			list2 = APHISCategoryTypeCodeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.APQ);
			AssertEquals("Data should be cached", list1, list2);
			var expectedCodes = new[] {
				APHISCategoryTypeCodeList.Codes.PropagativeMaterial,
				APHISCategoryTypeCodeList.Codes.SeedsNotForPlanting,
				APHISCategoryTypeCodeList.Codes.FruitsAndVegetables,
				APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts,
				APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery
			};
			AssertEquals(expectedCodes.Length, list1.Count);
			foreach (var code in expectedCodes)
			{
				AssertEquals(code, fullList.GetDescriptionFromCode(code), list1.GetDescriptionFromCode(code));
			}

			list1 = APHISCategoryTypeCodeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.AVS);
			list2 = APHISCategoryTypeCodeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.AVS);
			AssertEquals("Data should be cached", list1, list2);
			expectedCodes = new[] {
				APHISCategoryTypeCodeList.Codes.LiveAnimals,
				APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts,
				APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts
			};
			AssertEquals(expectedCodes.Length, list1.Count);
			foreach (var code in expectedCodes)
			{
				AssertEquals(code, fullList.GetDescriptionFromCode(code), list1.GetDescriptionFromCode(code));
			}
		}
	}
}
