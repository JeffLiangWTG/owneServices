using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	class ICRClassificationTest : TestCaseWithFactory
	{
		public void TestICRClassification()
		{
			var icrClassification = new ICRClassification("12.3.4", ClassificationTypeList.Codes.SSO);
			AssertEquals("1234", icrClassification.Classification);
			AssertEquals(ClassificationTypeList.Codes.SSO, icrClassification.ClassificationTypeCode);
		}
	}
}
