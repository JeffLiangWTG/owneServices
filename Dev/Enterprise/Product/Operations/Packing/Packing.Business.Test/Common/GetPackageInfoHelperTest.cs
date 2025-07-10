using CargoWise.EntityFramework.Testing;

namespace Enterprise.Packing.Business.Testing
{
	public class GetPackageInfoHelperTest : TestCaseWithFactory
	{
		public void TestGetCleanSingleLineText()
		{
			var text1 = "This is a \r\nmultiline test full of\r\n white \tspaces.";
			AssertEquals("This is a multiline test full of white spaces.", GetPackageInfoHelper.GetCleanSingleLineText(text1));

			var text2 = "This is a string with trailing space. ";
			AssertEquals("This is a string with trailing space.", GetPackageInfoHelper.GetCleanSingleLineText(text2));

			AssertEquals("Spaces should be trimmed", "", GetPackageInfoHelper.GetCleanSingleLineText("    "));
			AssertEquals("Should be empty", "", GetPackageInfoHelper.GetCleanSingleLineText(""));
		}
	}
}
