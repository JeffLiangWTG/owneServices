using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class MergePreSaveDialogStrategyTest : TestCaseWithFactory
	{
		//post-merge error checking unit test in US.sln - TestWhenThereIsAPostMergeError
		public void TestShowPreSaveDialogsPassesBaseSaveResultThrough()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			AssertEquals("requires merge", false, declaration.MergeManager.RequiresMerge);
			MergePreSaveDialogStrategy strategy = new MergePreSaveDialogStrategy(declaration);
			AssertEquals("Passsed through", ContinueWithSave.Yes, strategy.ShowPreSaveDialogs(ContinueWithSave.Yes));
			AssertEquals("Passsed through", ContinueWithSave.No, strategy.ShowPreSaveDialogs(ContinueWithSave.No));
		}

		public void TestWhenUserAgreesToMerge()
		{
			BaseJobDeclaration declaration = GetDeclarationThatNeedsAMerge();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ContinueWithSave result = new MergePreSaveDialogStrategy(declaration).ShowPreSaveDialogs(ContinueWithSave.Yes);
			AssertEquals("requires merge", false, declaration.MergeManager.RequiresMerge);
			AssertEquals("ContinueWithSave", ContinueWithSave.Yes, result);
		}

		public void TestWhenPreConditionChecksFail()
		{
			BaseJobDeclaration declaration = GetDeclarationThatNeedsAMerge();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.InvoiceLines.RemoveAndDeleteAll(); // no invoice lines -> cannot merge
			ContinueWithSave result = new MergePreSaveDialogStrategy(declaration).ShowPreSaveDialogs(ContinueWithSave.Yes);
			AssertEquals("requires merge", true, declaration.MergeManager.RequiresMerge);
			AssertEquals("ContinueWithSave", ContinueWithSave.No, result);
		}

		BaseJobDeclaration GetDeclarationThatNeedsAMerge()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(true);
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			declaration.DoMerge();
			declaration.JE_HouseBill = "ABC";
			AssertEquals("requires merge", true, declaration.MergeManager.RequiresMerge);
			return declaration;
		}
	}
}
