using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.DIS.Business;
using Enterprise.Customs.US.DIS.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DIS.GUI.Testing
{
	[TestedType(typeof(DISForm))]
	sealed class DISFormTest : ZFormBasherTest
	{
		public void TestSecurityCheck()
		{
			var declaration = (MasterFiles.Business.DIS.IUSDISHost)JobDeclaration;
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			var eDocs = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);
			var requiredDocument = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			Factory.Save();
			var hostWrapper = new DISHostWrapper(declaration);
			Env.Security.CustomsDISEdit.IsAllowed = false;
			using (var form = new DISForm(hostWrapper))
			{
				form.Show();
				AssertEquals(false, form.SendButton.Enabled);
			}
			Env.Security.CustomsDISEdit.IsAllowed = true;
			using (var form = new DISForm(hostWrapper))
			{
				form.Show();
				AssertEquals(true, form.SendButton.Enabled);
				Env.Security.CustomsDISSendMessage.IsAllowed = false;
				Env.Security.CustomsDISSendWithMessageErrors.IsAllowed = false;
				form.SendButton.PerformClick();
				AssertEquals("Please enter at least one DIS document first.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				var disDocument = hostWrapper.DISDocuments.AddNew();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.SendButton.PerformClick();
				AssertEquals(Env.Security.GetErrorMessageForNotAllowed(Env.Security.CustomsDISSendMessage), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				Env.Security.CustomsDISSendMessage.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.SendButton.PerformClick();
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				disDocument.RequiredDocumentPK = requiredDocument.PK;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.SendButton.PerformClick();
				AssertEquals(Customs.Business.SingleMessageManager.MessageErrorsExistWithNoSecurityRight + " " + Env.Security.CustomsDISSendWithMessageErrors.DisplayTextPathToSecurityRight, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				Env.Security.CustomsDISSendWithMessageErrors.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.SendButton.PerformClick();
				AssertContains("It is likely that your message(s) will be rejected by Customs, as they have the following message errors:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(MessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestCancelledChangesNotSerialized()
		{
			const string xml1 = @"<DISDocument xmlns=""http://www.cargowise.com/Schemas/DISDocument""><IDSuffix>1</IDSuffix><DocumentLabel>CBP07</DocumentLabel><DocumentDescription>Example</DocumentDescription><EDocsDocumentPK>3bf5d443-a5b6-457c-b7a9-d436dca3ea1e</EDocsDocumentPK><PGAs><DISPGA><Code>AMS</Code></DISPGA><DISPGA><Code>APH</Code></DISPGA><DISPGA><Code>CBP</Code></DISPGA></PGAs></DISDocument>";

			var declaration = (MasterFiles.Business.DIS.IUSDISHost)JobDeclaration;
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			var eDocs = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);

			var requiredDocument = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();

			var addInfo = requiredDocument.AddInfos.AddNew();

			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			addInfo.EX_AddInfo = xml1;

			Factory.Save();

			var hostWrapper = new DISHostWrapper(declaration);

			using (DISForm form1 = new DISForm(hostWrapper))
			{
				form1.Show();

				Assert(!form1.SaveButton.Enabled);
				Assert(hostWrapper.DISDocuments.Count == 1);

				form1.disUserControl1.DocumentsGrid.CurrentRowIndex = 0;
				var deleteMenuButton = form1.disUserControl1.DocumentsGrid.ContextMenu.MenuItems.FindByText("Delete");
				deleteMenuButton.PerformClick();

				Assert(hostWrapper.DISDocuments.Count == 0);
				Assert(hostWrapper.HasChanges = true);

				form1.CancelOrCloseButton.PerformClick();

				AssertContains("Should have asked if you would like to save changes", "This record has been modified", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				Assert(hostWrapper.DISDocuments.Count == 0);
			}

			var hostWrapper2 = new DISHostWrapper(declaration);

			using (DISForm form2 = new DISForm(hostWrapper2))
			{
				form2.Show();
				Assert(!form2.SaveButton.Enabled);

				hostWrapper2.DISDocuments[0].DocumentDescription = "Example change";

				form2.SaveButton.PerformClick();
			}
		}

		public void TestSaveButton()
		{
			var declaration = (MasterFiles.Business.DIS.IUSDISHost)JobDeclaration;
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			var eDocs = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);

			var requiredDocument = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var hostWrapper = new DISHostWrapper(declaration);

			using (DISForm form = new DISForm(hostWrapper))
			{
				form.Show();

				Assert(!form.SaveButton.Enabled);
				var disDocument = hostWrapper.DISDocuments.AddNew();
				disDocument.RequiredDocumentPK = ZGuid.Empty;
				hostWrapper.HasChanges = true; //this usually happens when a new element is added to a grid on UI

				form.SaveButton.PerformClick();
				Assert("Precondition", disDocument.HasErrors);
				Assert("Should have validated and showed the reason why system cannot save", !requiredDocument.IsInDatabase);
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				disDocument.RequiredDocumentPK = requiredDocument.PK;
				Assert("Precondition", !disDocument.HasErrors);

				form.SaveButton.PerformClick();
				AssertNotContains("Should have validated and showed the reason why system cannot save", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				Assert("Saved", requiredDocument.IsInDatabase);
				Assert(!form.SaveButton.Enabled);
			}
		}

		public void TestSaveButton_DisabledWhenDisplayModeIsBrowse()
		{
			var declaration = (MasterFiles.Business.DIS.IUSDISHost)JobDeclaration;
			var docManagerSupport = (IDocManagerSupport)declaration;

			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);

			var disHostWrapper = new DISHostWrapper(declaration);

			using (DISForm form = new DISForm(disHostWrapper))
			{
				AssertNotNull(form);

				form.Show();
				form.ControllerID = ControllerIDs.Customs.DocumentImageSystem;

				Assert(!form.SaveButton.Enabled);

				var disDocument = disHostWrapper.DISDocuments.AddNew();
				disDocument.RequiredDocumentPK = ZGuid.Empty;

				form.SaveButton.Enabled = true;

				form.disUserControl1.DocumentsGrid.CurrentRowIndex = 0;
				var deleteMenuButton = form.disUserControl1.DocumentsGrid.ContextMenu.MenuItems.FindByText("Delete");

				disHostWrapper.DISDocuments.RemoveAndDelete(disDocument);
				disHostWrapper.HasChanges = false; //this usually happens when a new element is deleted from a grid on UI
				form.SaveButton.Enabled = true; //this usually happens when a new element is deleted from a grid on UI

				deleteMenuButton.PerformClick();

				if (form.SaveButton.Enabled)
				{
					form.SaveButton.PerformClick();

					disHostWrapper.HasChanges = true;
					form.DisplayMode = ODisplayMode.Browse;
					form.ControllerID = ControllerIDs.Customs.DocumentImageSystem;

					AssertNoExceptionThrown(() =>
					{
						form.SaveButton.PerformClick();
					});
				}
				else
				{
					Assert(!form.SaveButton.Enabled);
				}
			}
		}

		public void TestButtonText()
		{
			HostWrapper.HasChanges = false;

			using (var form = new DISForm(HostWrapper))
			{
				form.Show();
				AssertEquals("&Close", form.CancelOrCloseButton.Text);
				AssertEquals("&Send", form.SendButton.Text);

				HostWrapper.HasChanges = true;
				AssertEquals("&Cancel", form.CancelOrCloseButton.Text);
				AssertEquals("Save && &Send", form.SendButton.Text);
			}
		}

		public void TestSendButtonWithMessageError()
		{
			var declaration = (MasterFiles.Business.DIS.IUSDISHost)JobDeclaration;
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			var eDocs = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);

			using (DISForm form = new DISForm(HostWrapper))
			{
				form.Show();

				var disDocument = HostWrapper.DISDocuments.AddNew();

				var requiredDocument = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
				disDocument.RequiredDocumentPK = requiredDocument.PK;
				disDocument.EDocsDocumentPK = eDocs.UniqueKey;
				disDocument.RunPreSaveValidation();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				Assert("Precondition", !disDocument.HasErrors);
				Assert("Precondition", disDocument.HasMessageErrors);

				form.SendButton.PerformClick();
				Assert("Saved", requiredDocument.IsInDatabase);
				AssertEquals(typeof(MessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestSendButtonWithoutDocuments()
		{
			using (DISForm form = new DISForm(HostWrapper))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.SendButton.PerformClick();
				AssertContains("Please enter at least one DIS document first.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSaveDISWithCBPRequestsID()
		{
			var declaration = (MasterFiles.Business.DIS.IUSDISHost)JobDeclaration;
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			var eDocs = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);

			var baseDeclaration = ((BaseJobDeclaration)declaration);
			var entryHeader = baseDeclaration.ActiveEntryHeaders.AddNew();
			entryHeader["CH_MessageType"] = "SE";
			entryHeader.EntryNumber = "36194291";

			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = "SO";
			message.EM_LinkUniqueID = entryHeader.PK;
			message.EM_LinkTable = entryHeader.TableName;
			message.EM_MessageText = "B003001221SO                                                                    " +
				"SO103002221  36194291 0193-108633000HDMUHYUNDAI FAITH       44E  022415         " +
				"SO20CR X00019645                                                                " +
				"SO40RHDMUQSWB4396229                                       00000633CT   00000633" +
				"SO50031115131395BILL ARRIVED                                                    " +
				"SO60031115131303PENDING INTENSIVE EXAM                                          " +
				"SO60031115131396DOCUMENT REQUIRED                               01              " +
				"SO60031115131396DOCUMENT REQUIRED                               02              " +
				"Y  3001221SO00000";

			var requiredDocument = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var hostWrapper = new DISHostWrapper(declaration);

			var disDocument = hostWrapper.DISDocuments.AddNew();
			disDocument.RequiredDocumentPK = ZGuid.Empty;

			string expectedCode = "9602-031115";
			string expectedDescription;

			using (DISForm form = new DISForm(hostWrapper))
			{
				form.Show();

				var requestIDDropEdit = (ZDropEdit)form.Controls.Find("RequestIDDropEdit", true)[0];
				var requestIDs = (CodeDescriptionPairList)requestIDDropEdit.List;

				Assert("Precondition: CBP request list from SO message", requestIDs.ContainsCode("9601-031115"));
				Assert("Precondition: CBP request list from SO message", requestIDs.ContainsCode("9602-031115"));

				expectedDescription = requestIDs[expectedCode].Description;

				disDocument.CBPRequest.ID = expectedCode;
				disDocument.RequiredDocumentPK = requiredDocument.PK;
				form.SaveButton.PerformClick();
			}

			using (DISForm form = new DISForm(hostWrapper))
			{
				form.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();
				var requestIDDropEdit = (ZDropEdit)form.Controls.Find("RequestIDDropEdit", true)[0];
				AssertEquals("RequestIDDropEdit.Code", expectedCode, requestIDDropEdit.Text);
				AssertEquals("RequestIDDropEdit.Description", expectedDescription, requestIDDropEdit.DescriptionBox.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new DISForm(HostWrapper);
		}

		DISHostWrapper HostWrapper
		{
			get { return hostWrapper ?? (hostWrapper = new DISHostWrapper((MasterFiles.Business.DIS.IUSDISHost)JobDeclaration)); }
		}
		DISHostWrapper hostWrapper;

		BusinessObject JobDeclaration
		{
			get { return jobDeclaration ?? (jobDeclaration = new TestHelper(Factory).GetJobDeclaration()); }
		}
		BusinessObject jobDeclaration;
	}
}
