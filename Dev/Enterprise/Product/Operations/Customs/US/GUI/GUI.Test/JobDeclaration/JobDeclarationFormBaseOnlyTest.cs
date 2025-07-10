using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class JobDeclarationFormBaseOnlyTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestShouldSaveEDocsMasterFactoryTogether()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.DocManagerInfo.AddFileOrDocument(System.Text.Encoding.Unicode.GetBytes("Some contents"), "XXX", "DEC");
			declaration.ShouldSaveEDocsMasterFactoryTogether = true;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.FireSaveButton();
			}

			var anotherFactory = new BusinessObjectFactory();
			AssertEquals(1, anotherFactory.Load<JobDeclaration>(declaration.PK).DocManagerInfo.AllEDocs.Count);
		}

		public void TestFormCaption()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (var form = new JobDeclarationForm(declaration))
			{
				AssertEquals("Customs Declaration", form.FormCaption);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;

			using (var form = new JobDeclarationForm(declaration))
			{
				AssertEquals("Drawback", form.FormCaption);
			}

			declaration.JE_DeclarationReference = "B12345678";

			using (var form = new JobDeclarationForm(declaration))
			{
				AssertEquals("Drawback - B12345678", form.FormCaption);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			using (var form = new JobDeclarationForm(declaration))
			{
				AssertEquals("Foreign Trade Zone Declaration - B12345678", form.FormCaption);
			}
		}

		public void TestDISFeatures()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.DocsAndCartage.RequiredDocuments.AddNew();
			declaration.JE_MessageType = "";

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var disDataButton = GetDisDataButton(form);
				Assert("Should not be visible yet", !disDataButton.Visible);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				form.PlugIns.SelectPlugInTabPage(ControllerIDs.eDocsPlugIn);
				Assert("Should be visible now", disDataButton.Visible);
			}
		}

		public void TestShowNewForm_WhenControllerIDIsUSLowValueEntriesBill()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.US_EntryType = EntryTypeList.Codes.LowValue;
			declaration.JE_MessageType = USJobMessageTypeList.Codes.Import;
			Factory.Save();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.ControllerID = ControllerIDs.Customs.US.USLowValueEntriesBill;

				var saveButtonControl = form.Controls.Find("oPostingButtonsUserControl", true).OfType<ZPostingButtonsUserControl>().Single();
				saveButtonControl.SaveButton.PerformClick();
				Application.DoEvents();

				var newForm = Application.OpenForms.OfType<JobDeclarationForm>().Single();
				var newDeclaration = (JobDeclaration)newForm.BusinessEntity;
				AssertNotEquals("A new declaration should have been created", declaration.PK, newDeclaration.PK);
				Assert("New declaration should not be saved yet", !newDeclaration.IsInDatabase);

				AssertEquals("New declaration should have entry type 86", EntryTypeList.Codes.LowValue, newDeclaration.US_EntryType);
				AssertEquals("New declaration should have import message type", USJobMessageTypeList.Codes.Import, newDeclaration.JE_MessageType);

				newForm.Close();
			}
		}

		public void TestDISFeatures_PromptToMerge()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.DocsAndCartage.RequiredDocuments.AddNew().EQ_DocType = Core.Constants.RefDocTypes.Invoice;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;
			Factory.Save();
			Assert("declaration is not merged", !declaration.IsMergeDone);

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var disDataButton = GetDisDataButton(form);

				ZFormModaliser.LastFormShownForTest = null;
				ZFormModaliser.LastFormShownDialogForTest = null;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				declaration.HasChanges = true;
				disDataButton.PerformClick();
				AssertNull("PromptToMergeForm did not popped up", ZFormModaliser.LastFormShownDialogForTest);
				AssertNull("DIS form did not popped up", ZFormModaliser.LastFormShownForTest);
				Assert("declaration should not be saved", declaration.HasChanges);
				Assert("declaration should not be merged", !declaration.IsMergeDone);

				ZFormModaliser.LastFormShownForTest = null;
				ZFormModaliser.LastFormShownDialogForTest = null;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				disDataButton.PerformClick();
				AssertEquals("PromptToMergeForm popped up", "Prompt to Merge", ZFormModaliser.LastFormShownDialogForTest?.Text);
				AssertEquals("Merge failed", "You can't merge this entry because there are no invoice headers.", UnitTestUserNotification.Instance.LastMessage?.Text);
				AssertNull("DIS form did not popped up", ZFormModaliser.LastFormShownForTest);
				Assert("declaration should be saved", !declaration.HasChanges);
				Assert("declaration should not be merged", !declaration.IsMergeDone);

				ZFormModaliser.LastFormShownForTest = null;
				ZFormModaliser.LastFormShownDialogForTest = null;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				declaration.HasChanges = true;
				disDataButton.PerformClick();
				AssertEquals("DIS form popped up", "Document Image System", ZFormModaliser.LastFormShownForTest?.Text);
				Assert("declaration should be saved", !declaration.HasChanges);
				Assert("declaration should not be merged", !declaration.IsMergeDone);
			}

			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var disDataButton = GetDisDataButton(form);

				ZFormModaliser.LastFormShownForTest = null;
				ZFormModaliser.LastFormShownDialogForTest = null;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				declaration.Invoices.AddNew().InvoiceLines.AddNew().JI_CustomsQuantity = 100;
				Assert("declaration has been changed", declaration.HasChanges);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				disDataButton.PerformClick();
				AssertEquals("PromptToMergeForm popped up", "Prompt to Merge", ZFormModaliser.LastFormShownDialogForTest?.Text);
				AssertEquals("DIS form popped up", "Document Image System", ZFormModaliser.LastFormShownForTest?.Text);
				Assert("declaration should be merged", declaration.IsMergeDone);
				Assert("declaration should be saved", !declaration.HasChanges);
			}

			ZFormModaliser.LastFormShownForTest = null;
			ZFormModaliser.LastFormShownDialogForTest = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestDISFeatures_PromptToMergeNotShow()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.DocsAndCartage.RequiredDocuments.AddNew().EQ_DocType = Core.Constants.RefDocTypes.Invoice;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;
			Factory.Save();
			Assert("declaration is not merged", !declaration.IsMergeDone);

			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			declaration.Invoices.AddNew().InvoiceLines.AddNew().JI_CustomsQuantity = 100;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var disDataButton = GetDisDataButton(form);

				ZFormModaliser.LastFormShownForTest = null;
				ZFormModaliser.LastFormShownDialogForTest = null;
				UnitTestUserNotification.Instance.ClearMessages();
				declaration.Invoices[0].InvoiceLines.AddNew().JI_CustomsQuantity = 100;
				Assert("declaration has been changed", declaration.HasChanges);
				Assert("declaration require merge", declaration.MergeManager.RequiresMerge);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				disDataButton.PerformClick();
				Assert("PromptToMergeForm did not popped up", ZFormModaliser.LastFormShownDialogForTest == null || ZFormModaliser.LastFormShownDialogForTest.Text != "Prompt to Merge");
				AssertEquals("DIS form popped up", "Document Image System", ZFormModaliser.LastFormShownForTest.Text);
				Assert("declaration should not require merge", !declaration.MergeManager.RequiresMerge);
				Assert("declaration should be saved", !declaration.HasChanges);
			}
			ZFormModaliser.LastFormShownForTest = null;
			ZFormModaliser.LastFormShownDialogForTest = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		internal static Control FindMatchingControl(Control parentControl, Predicate<Control> match)
		{
			foreach (Control control in parentControl.Controls)
			{
				if (match(control))
				{
					return control;
				}
			}

			foreach (Control control in parentControl.Controls)
			{
				var result = FindMatchingControl(control, match);
				if (result != null)
				{
					return result;
				}
			}
			return null;
		}

		ZButton GetDisDataButton(JobDeclarationForm form)
		{
			var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn);
			form.PlugIns.SelectPlugInTabPage(ControllerIDs.eDocsPlugIn);
			var requiredDocumentUserControl = FindMatchingControl(plugIn.UserControl, x => x.GetType() == typeof(MasterFiles.GUI.RequiredDocumentsUserControl));
			var disDataButton = (ZButton)FindMatchingControl(requiredDocumentUserControl, x => x is ZButton && x.Name == "DISDataButton");
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
			form.PlugIns.SelectPlugInTabPage(ControllerIDs.eDocsPlugIn);

			return disDataButton;
		}
	}
}
