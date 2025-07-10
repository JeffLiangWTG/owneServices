using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists.Testing
{
	class MAFProcessingOfficeListTest : CodeDescriptionEnumListTestCase
	{
		public void TestGetDefaultOfficeCodeFromUNLOCO()
		{
			MAFProcessingOfficeList list = new MAFProcessingOfficeList();
			AssertEquals("empty string", MAFProcessingOfficeList.Codes.Auckland, list.GetDefaultOfficeCodeFromUNLOCO(""));
			AssertEquals("NZAKL", MAFProcessingOfficeList.Codes.Auckland, list.GetDefaultOfficeCodeFromUNLOCO("NZAKL"));
			AssertEquals("NZCHC", MAFProcessingOfficeList.Codes.Christchurch, list.GetDefaultOfficeCodeFromUNLOCO("NZCHC"));
			AssertEquals("NZNPE", MAFProcessingOfficeList.Codes.Napier, list.GetDefaultOfficeCodeFromUNLOCO("NZNPE"));
			AssertEquals("NZZZZ", MAFProcessingOfficeList.Codes.Auckland, list.GetDefaultOfficeCodeFromUNLOCO("NZZZZ"));
		}

		protected override CodeDescriptionPairList GetNewList()
		{
			return new MAFProcessingOfficeList();
		}
	}
}
