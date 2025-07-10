using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists.Testing
{
	using Enterprise.Customs.Business;
	public class YesNoUnknownListTest : TestCaseWithFactory
	{
		public void TestGetValueForCode()
		{
			AssertEquals("Yes", true, YesNoUnknownList.GetValueForCode(YesNoUnknownList.Codes.Yes));
			AssertEquals("No", false, YesNoUnknownList.GetValueForCode(YesNoUnknownList.Codes.No));
			AssertEquals("Unknown", null, YesNoUnknownList.GetValueForCode(YesNoUnknownList.Codes.Unknown));
			AssertEquals("Empty String", null, YesNoUnknownList.GetValueForCode(""));
			AssertEquals("Ridiculous Code", null, YesNoUnknownList.GetValueForCode("(_*_)"));
		}

		public void TestYesAndNoCodeasAreKeptTheSameAsTheYesNoList()
		{
			AssertEquals("YesNoUnknownList.Codes.Yes", YesNoList.Codes.Yes, YesNoUnknownList.Codes.Yes);
			AssertEquals("YesNoUnknownList.Codes.No", YesNoList.Codes.No, YesNoUnknownList.Codes.No);
		}
	}
}
