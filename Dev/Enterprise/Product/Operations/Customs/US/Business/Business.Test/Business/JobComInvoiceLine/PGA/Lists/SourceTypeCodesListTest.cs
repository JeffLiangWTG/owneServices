namespace Enterprise.Customs.US.Business.Testing
{
	class SourceTypeCodesListTest : CodeDescriptionPairListTest
	{
		public void TestGetListForAPHIS()
		{
			var list1 = SourceTypeCodesList.GetListForAPHIS(Factory, APHISProgramCodeList.Codes.AVS);
			var list2 = SourceTypeCodesList.GetListForAPHIS(Factory, APHISProgramCodeList.Codes.AVS);
			AssertEquals("Data should be cached", list1, list2);
			var expectedCodes = new[] {
						SourceTypeCodesList.Codes.CountryOfSource,
						SourceTypeCodesList.Codes.CountryOfDeboning,
						SourceTypeCodesList.Codes.CountryOfManipulation,
						SourceTypeCodesList.Codes.CountryOfMeatCutting,
						SourceTypeCodesList.Codes.CountryOfPacking,
						SourceTypeCodesList.Codes.CountryOfProcessing,
						SourceTypeCodesList.Codes.CountryOfProduction,
						SourceTypeCodesList.Codes.CountryOfSlaughter,
						SourceTypeCodesList.Codes.CountryOfSlicing,
						SourceTypeCodesList.Codes.CountryOfSpeciesOrigin,
						SourceTypeCodesList.Codes.PlaceOfPacking
			};
			AssertList<SourceTypeCodesList>(list1, expectedCodes);

			list1 = SourceTypeCodesList.GetListForAPHIS(Factory, APHISProgramCodeList.Codes.AAC);
			list2 = SourceTypeCodesList.GetListForAPHIS(Factory, APHISProgramCodeList.Codes.AAC);
			AssertEquals("Data should be cached", list1, list2);
			AssertList<SourceTypeCodesList>(list1, new[] { SourceTypeCodesList.Codes.CountryOfSpeciesOrigin });

			list1 = SourceTypeCodesList.GetListForAPHIS(Factory, APHISProgramCodeList.Codes.APQ);
			list2 = SourceTypeCodesList.GetListForAPHIS(Factory, APHISProgramCodeList.Codes.APQ);
			AssertEquals("Data should be cached", list1, list2);
			expectedCodes = new[] {
						SourceTypeCodesList.Codes.Harvested,
						SourceTypeCodesList.Codes.PlaceOfGrowth
			};
			AssertList<SourceTypeCodesList>(list1, expectedCodes);

			list1 = SourceTypeCodesList.GetListForAPHIS(Factory, APHISProgramCodeList.Codes.ABS);
			list2 = SourceTypeCodesList.GetListForAPHIS(Factory, APHISProgramCodeList.Codes.ABS);
			AssertEquals("Data should be cached", list1, list2);
			expectedCodes = new[] {
						SourceTypeCodesList.Codes.Harvested,
						SourceTypeCodesList.Codes.PlaceOfGrowth,
						SourceTypeCodesList.Codes.CountryOfStorage
			};
			AssertList<SourceTypeCodesList>(list1, expectedCodes);
		}

		public void TestGetListForNMFS()
		{
			var list1 = SourceTypeCodesList.GetListForNMFS(Factory, NMFSProgramCodeList.Codes.SIM);
			var expectedCodes = new[] {
				SourceTypeCodesList.Codes.HarvestOfCaptureFisheries,
				SourceTypeCodesList.Codes.HatcheryBasedAquaculture,
				SourceTypeCodesList.Codes.SmallVesselHarvest
			};
			AssertList<SourceTypeCodesList>(list1, expectedCodes);

			list1 = SourceTypeCodesList.GetListForNMFS(Factory, NMFSProgramCodeList.Codes.COA);
			AssertList<SourceTypeCodesList>(list1, expectedCodes);

			list1 = SourceTypeCodesList.GetListForNMFS(Factory, NMFSProgramCodeList.Codes.AMR);
			AssertEquals("List should be null", 0, list1.Count);
		}
	}
}
