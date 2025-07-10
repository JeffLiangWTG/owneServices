using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.Testing;
using Enterprise.Customs.NZ.Business.MessageBuilders.ECIWriteOff.Manifesting;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Declaration.ECIWriteOff.Manifesting.Testing
{
	[TestedType(typeof(SubmitToCustomsForm))]
	public class SubmitToCustomsFormTest : ZFormBasherTest
	{
		public void TestOkButton()
		{
			TestHelper.SetupMessagingEnvironment();
			var entryHeader = Factory.New<CusEntryHeader>();
			var manifestCreator = new TestManifestCreator(entryHeader, "081-22222222", "QF223", "USSFO", "NZWLG", new ZDateTime(2005, 12, 2), new ZDateTime(2005, 12, 3));
			var declaration1 = manifestCreator.AddDeclaration(Business.JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "HOUSEBILL1", "RATS HEADS", 15.2m, 4, 45.54m);
			var declaration2 = manifestCreator.AddDeclaration(Business.JobMessageTypeList.Codes.Export, manifestCreator.Supplier2, manifestCreator.Importer2, "HOUSEBILL2", "RATS TEETH", 13.4m, 2, 27.72m);
			var manager = new MessageManager(declaration1, Business.MessageBuilders.ECIWriteOff.Manifesting.MessageManager.OperationType.SubmitMessage);
			using (var form = new SubmitToCustomsForm(manager))
			{
				form.OkButton_Click(null, new EventArgs());
				AssertEquals(form.DialogResult, System.Windows.Forms.DialogResult.OK);
			}
		}

		public void TestDeclarationsGetLoadedInTheGridOnShow()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var manifestCreator = new TestManifestCreator(entryHeader, "081-22222222", "QF223", "USSFO", "NZWLG", new ZDateTime(2005, 12, 2), new ZDateTime(2005, 12, 3));
			var declaration1 = manifestCreator.AddDeclaration(Business.JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "HOUSEBILL1", "RATS HEADS", 15.2m, 4, 45.54m);
			var declaration2 = manifestCreator.AddDeclaration(Business.JobMessageTypeList.Codes.Export, manifestCreator.Supplier2, manifestCreator.Importer2, "HOUSEBILL2", "RATS TEETH", 13.4m, 2, 27.72m);
			Factory.Save();
			var manager = new MessageManager(declaration2, Business.MessageBuilders.ECIWriteOff.Manifesting.MessageManager.OperationType.SubmitMessage);
			using (var form = new SubmitToCustomsForm(manager))
			{
				form.Show();
				AssertEquals("Form.DeclarationModuleButtonGrid.List.Count", 2, form.FindSingle<ZGrid>("DeclarationsGrid").List.Count);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var manifestCreator = new TestManifestCreator(entryHeader, "081-22222222", "QF223", "USSFO", "NZWLG", new ZDateTime(2005, 12, 2), new ZDateTime(2005, 12, 3));
			var declaration1 = manifestCreator.AddDeclaration(Business.JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "HOUSEBILL1", "RATS HEADS", 15.2m, 4, 45.54m);
			var declaration2 = manifestCreator.AddDeclaration(Business.JobMessageTypeList.Codes.Export, manifestCreator.Supplier2, manifestCreator.Importer2, "HOUSEBILL2", "RATS TEETH", 13.4m, 2, 27.72m);
			var manager = new MessageManager(declaration1, MessageManager.OperationType.ResetToOriginal);
			var testedForm = new SubmitToCustomsForm(manager);
			MissingResourceStringChecker.ExcludeFromTest(testedForm.RemarksTextBox);
			return testedForm;
		}
	}
}
