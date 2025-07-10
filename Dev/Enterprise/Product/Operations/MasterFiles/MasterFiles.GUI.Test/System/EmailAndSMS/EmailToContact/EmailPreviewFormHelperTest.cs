using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class EmailPreviewFormHelperTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestPreview()
		{
			EmailToContactBusinessObject emailBizO = new EmailToContactBusinessObject(Factory.New<DummyBusinessObject>());
			EmailPreviewFormHelper.ShowPreviewForm(emailBizO);
			AssertEquals(typeof(RichTextEmailDisplayZForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertNotNull(emailBizO.PreviewFilePath);
			Assert("File should be cleaned up", !File.Exists(emailBizO.PreviewFilePath));
			ZFormModaliser.LastFormShownDialogForTest.Dispose();
		}
	}
}
