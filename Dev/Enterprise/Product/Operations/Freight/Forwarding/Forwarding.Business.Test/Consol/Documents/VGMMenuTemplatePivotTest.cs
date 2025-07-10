using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class VGMMenuTemplatePivotTest : TestCaseWithFactory
	{
		public void TestVGMDocTypeEnabledForEDocsSaving()
		{
			var menu = new VGMMenuItem(Factory, VGMMessageAction.SendOriginal, "VGM Test");
			var pivot = new VGMMenuTemplatePivot(Factory, menu, "VGM Doc");

			AssertNotNull(pivot.DocType);
			Assert(pivot.DocType.RT_LogSystemCreatedDocsToEDocs);
		}
	}
}
