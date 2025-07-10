using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	class APHISGovernmentAgencyProcessingCodeListTest : TestCaseWithFactory
	{
		public void TestGetListForProgram()
		{
			var fullList = new APHISGovernmentAgencyProcessingCodeList();
			var list1 = APHISGovernmentAgencyProcessingCodeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.ABS);
			var list2 = APHISGovernmentAgencyProcessingCodeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.ABS);
			AssertEquals("Data should be cached", list1, list2);
			var expectedCodes = new[] {
				APHISGovernmentAgencyProcessingCodeList.Codes.CBPAgriculture,
				APHISGovernmentAgencyProcessingCodeList.Codes.APHISPlantInspectionStation
			};
			AssertEquals(expectedCodes.Length, list1.Count);
			foreach (var code in expectedCodes)
			{
				AssertEquals(code, fullList.GetDescriptionFromCode(code), list1.GetDescriptionFromCode(code));
			}

			list1 = APHISGovernmentAgencyProcessingCodeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.APQ);
			list2 = APHISGovernmentAgencyProcessingCodeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.APQ);
			AssertEquals("Data should be cached", list1, list2);
			expectedCodes = new[] {
				APHISGovernmentAgencyProcessingCodeList.Codes.CBPAgriculture,
				APHISGovernmentAgencyProcessingCodeList.Codes.APHISPlantInspectionStation,
				APHISGovernmentAgencyProcessingCodeList.Codes.APHISPreClearance
			};
			AssertEquals(expectedCodes.Length, list1.Count);
			foreach (var code in expectedCodes)
			{
				AssertEquals(code, fullList.GetDescriptionFromCode(code), list1.GetDescriptionFromCode(code));
			}

			foreach (var programType in new[] { APHISProgramCodeList.Codes.AAC, APHISProgramCodeList.Codes.AVS })
			{
				list1 = APHISGovernmentAgencyProcessingCodeList.GetListForProgram(Factory, programType);
				list2 = APHISGovernmentAgencyProcessingCodeList.GetListForProgram(Factory, programType);
				AssertEquals("Data should be cached for " + programType, list1, list2);
				expectedCodes = new[]
				{
					APHISGovernmentAgencyProcessingCodeList.Codes.CBPAgriculture,
					APHISGovernmentAgencyProcessingCodeList.Codes.APHISVSPortVeterinarian,
					APHISGovernmentAgencyProcessingCodeList.Codes.APHISVSAnimalImportCenter
				};
				AssertEquals(programType, expectedCodes.Length, list1.Count);
				foreach (var code in expectedCodes)
				{
					AssertEquals(programType + "_" + code, fullList.GetDescriptionFromCode(code), list1.GetDescriptionFromCode(code));
				}
			}
		}
	}
}
