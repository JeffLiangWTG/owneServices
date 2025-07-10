using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(AdditionalDocumentForm))]
	public sealed class AdditionalDocumentFormTest : MessageSendingFormAbstractTest<AdditionalDocumentForm, AdditionalDocumentMessageSendingObjectParent>
	{
		public void TestSendAdditionalDocumentMessageMenuItem()
		{
			using (GetFormToBashCore())
			{
				var testMenu = new EDIMenuForTest();
				testMenu.Declaration = Declaration;
				UnitTestUserNotification.Instance.ClearMessages();
				testMenu.RefreshMenu();
				AssertEquals(true, testMenu.SendAdditionalDocumentMessageMenuItem.Visible);
				Factory.Save();
				testMenu.SendAdditionalDocumentMessageMenuItem.PerformClick();
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(AdditionalDocumentForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestChangeGridColumnsVisibility()
		{
			CombineAssertions("SupportingDocumentsGrid Columns Visible", () =>
			{
				AssertChangeGridColumnsVisibility(MessageTypeList.Codes.ADM, true);
				AssertChangeGridColumnsVisibility(MessageTypeList.Codes.ICD, false);
			});

			CombineAssertions("SupportingDocumentsGrid Columns Availability", () =>
			{
				AssertChangeGridColumnsAvailability(MessageTypeList.Codes.ADM, false);
				AssertChangeGridColumnsAvailability(MessageTypeList.Codes.ICD, true);
			});
		}

		void AssertChangeGridColumnsVisibility(ZString messageType, ZBool columnsVisible)
		{
			var declarationWrapper = new AdditionalDocumentMessageSendingObjectParent(Declaration, messageType);
			using var form = new AdditionalDocumentForm(declarationWrapper);
			var messageSendingObjectsGrid = form.FindSingleOrDefault<ZGrid>(x => x.Name == "MessageSendingObjectsGrid");
			if (columnsVisible)
			{
				AssertEquals("The column of ContactOffice should be visibility on MessageSendingObjectsGrid", columnsVisible, messageSendingObjectsGrid.GetColumnStyle(AdditionalDocumentMessageSendingObject.TWSchema.ContactOffice).IsVisible);
			}
			else
			{
				AssertNull("The column of ContactOffice should be hidden on MessageSendingObjectsGrid", messageSendingObjectsGrid.GetColumnStyle(AdditionalDocumentMessageSendingObject.TWSchema.ContactOffice));
			}
		}

		void AssertChangeGridColumnsAvailability(ZString messageType, ZBool columnAvailability)
		{
			var declarationWrapper = new AdditionalDocumentMessageSendingObjectParent(Declaration, messageType);
			using var form = new AdditionalDocumentForm(declarationWrapper);
			var messageSendingObjectsGrid = form.FindSingleOrDefault<ZGrid>(x => x.Name == "MessageSendingObjectsGrid");
			AssertEquals("The column of Action should be visibility on MessageSendingObjectsGrid", columnAvailability, !messageSendingObjectsGrid.GetColumnStyle(MessageSendingObject.TWSchema.Action).IsUnavailable);
		}

		protected override Form GetFormToBashCore()
		{
			return new AdditionalDocumentForm(DeclarationWrapper);
		}

		protected override AdditionalDocumentMessageSendingObjectParent GetDeclarationWrapper()
		{
			Declaration.ActiveEntryHeaders[0].CH_EntryStatus = "ADD";
			return new AdditionalDocumentMessageSendingObjectParent(Declaration, MessageTypeList.Codes.ADM);
		}

		class EDIMenuForTest : EDIMenu
		{
			public new MenuItem SendAdditionalDocumentMessageMenuItem => base.SendAdditionalDocumentMessageMenuItem;
		}
	}
}
