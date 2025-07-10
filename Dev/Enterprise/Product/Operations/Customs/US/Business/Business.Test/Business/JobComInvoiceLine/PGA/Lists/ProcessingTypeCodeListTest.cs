namespace Enterprise.Customs.US.Business.Testing
{
	class ProcessingTypeCodeListTest : CodeDescriptionPairListTest
	{
		public void TestGetListForNMFS()
		{
			var list1 = ProcessingTypeCodeList.GetListForNMFS(Factory, NMFSProgramCodeList.Codes.SIM);
			var expectedCodes = new[] {
				ProcessingTypeCodeList.Codes.NmfsDressed,
				ProcessingTypeCodeList.Codes.NmfsFillet,
				ProcessingTypeCodeList.Codes.NmfsGilledAndGutted,
				ProcessingTypeCodeList.Codes.NmfsOther,
				ProcessingTypeCodeList.Codes.NmfsRound,
				ProcessingTypeCodeList.Codes.NmfsSteak,
				ProcessingTypeCodeList.Codes.NmfsRadiationSterilized,
			};
			AssertList<ProcessingTypeCodeList>(list1, expectedCodes);

			list1 = ProcessingTypeCodeList.GetListForNMFS(Factory, NMFSProgramCodeList.Codes.AMR);
			AssertEquals("List should be null", 0, list1.Count);
		}
	}
}
