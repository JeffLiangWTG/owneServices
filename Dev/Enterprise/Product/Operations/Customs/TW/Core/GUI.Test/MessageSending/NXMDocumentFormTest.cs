using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(NXMDocumentForm))]
	sealed class NXMDocumentFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new NXMDocumentForm(Wrapper);
		}
		public void TestShowEDocGird()
		{
			using (var form = new NXMDocumentForm(Wrapper))
			{
				form.Show();
				var supportingDocumentsGrid = form.Controls.Find("SupportingDocumentsGrid", searchAllChildren: true)[0] as ZGrid;
				CombineAssertions("Need display SupportingDocumentsGrid", () =>
				{
					Assert("Precondition: when message need to show SupportingDocuments grid", Wrapper.IsSupportingDocumentsNeededMessage);
					Assert("Display the SupportingDocuments grid", supportingDocumentsGrid.Visible);
					Assert("Enable the SupportingDocuments grid", supportingDocumentsGrid.Enabled);
				});
			}

			var (declaration, _) = GetTestObjects(ControllingMessageTypeList.Codes.NX101);
			wrapper = LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(declaration, ControllingMessageTypeList.Codes.NX101);
			using (var form = new NXMDocumentForm(Wrapper))
			{
				form.Show();
				var supportingDocumentsGrid = form.Controls.Find("SupportingDocumentsGrid", searchAllChildren: true)[0] as ZGrid;
				var messageSendingObjectsGroupBox = form.Controls.Find("messageSendingObjectsGroupBox", searchAllChildren: true)[0];
				CombineAssertions("Need not display SupportingDocumentsGrid", () =>
				{
					Assert("Precondition: when message not to show SupportingDocuments grid", !Wrapper.IsSupportingDocumentsNeededMessage);
					Assert("Not display the SupportingDocuments grid", !supportingDocumentsGrid.Visible);
					Assert("Not Enable the SupportingDocuments grid", !supportingDocumentsGrid.Enabled);
					AssertEquals("Fill the messageSendingObjectsGroupBox", DockStyle.Fill, messageSendingObjectsGroupBox.Dock);
				});
			}
		}

		public void TestMessageSendingObjectGridColumns()
		{
			using (var form = new NXMDocumentForm(Wrapper))
			{
				form.Show();
				var grid = form.Controls.Find("MessageSendingObjectsGrid", true)[0] as ZGrid;
				CombineAssertions(() =>
				{
					AssertNotNull("MessageSendingObjectsGrid should have ShouldSend column", grid.GetColumnStyle(BaseMessageSendingObject.SchemaShouldSend));
					AssertNotNull("MessageSendingObjectsGrid should have Action column", grid.GetColumnStyle(nameof(LicensingMessageSendingObject.Action)));
					AssertNotNull("MessageSendingObjectsGrid should have MessageType column", grid.GetColumnStyle(nameof(LicensingMessageSendingObject.MessageType)));
					AssertNotNull("MessageSendingObjectsGrid should have Description column", grid.GetColumnStyle(nameof(LicensingMessageSendingObject.Description)));
					AssertNotNull("MessageSendingObjectsGrid should have MessageNumber column", grid.GetColumnStyle(nameof(LicensingMessageSendingObject.MessageNumber)));
					AssertNotNull("MessageSendingObjectsGrid should have BusinessType column", grid.GetColumnStyle(nameof(LicensingMessageSendingObject.BusinessType)));
					AssertNotNull("MessageSendingObjectsGrid should have ProcessingUnit column", grid.GetColumnStyle(nameof(LicensingMessageSendingObject.ProcessingUnit)));
					AssertNull("MessageSendingObjectsGrid should not have ReasonDescription column", grid.GetColumnStyle(nameof(LicensingMessageSendingObject.ReasonDescription)));
				});
				CombineAssertions("ReasonDescription", () =>
				{
					AssertMessageSendingObjectGridHasReasonDescription(ControllingMessageTypeList.Codes.NX201_01);
					AssertMessageSendingObjectGridHasReasonDescription(ControllingMessageTypeList.Codes.NX201_07);
				});
			}
		}

		public void TestMessageSendingObjectGridColumns_NX101()
		{
			var testWrapper = GetWrapperByMessageType(ControllingMessageTypeList.Codes.NX101);
			using var form = new NXMDocumentForm(testWrapper);
			form.Show();
			var grid = form.Controls.Find("MessageSendingObjectsGrid", true)[0] as ZGrid;
			CombineAssertions(() =>
			{
				AssertNull("MessageSendingObjectsGrid shouldn't have BusinessType column", grid.GetColumnStyle(nameof(LicensingMessageSendingObject.BusinessType)));
				AssertNotNull("MessageSendingObjectsGrid should have CertificateType column", grid.GetColumnStyle(nameof(LicensingMessageSendingObject.CertificateType)));
			});
		}

		void AssertMessageSendingObjectGridHasReasonDescription(string controllingMessageType)
		{
			var (declaration, messageHeader) = GetTestObjects(controllingMessageType);
			Factory.Save();

			var wrapper = LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(declaration, messageHeader.TW1_ControllingMessageType);
			using (var form = new NXMDocumentForm(wrapper))
			{
				form.Show();
				var grid = form.Controls.Find("MessageSendingObjectsGrid", true)[0] as ZGrid;
				AssertNotNull(controllingMessageType, grid.GetColumnStyle(nameof(LicensingMessageSendingObject.ReasonDescription)));
			}
		}

		public void TestSupportingDocumentsGridColumns()
		{
			using (var form = new NXMDocumentForm(Wrapper))
			{
				form.Show();
				var grid = form.Controls.Find("SupportingDocumentsGrid", true)[0] as ZGrid;
				CombineAssertions(() =>
				{
					AssertNotNull("SupportingDocumentsGrid should have eDoc column", grid.GetColumnStyle(nameof(SupportingDocument.EDoc)));
					AssertNotNull("SupportingDocumentsGrid should have DocumentNo column", grid.GetColumnStyle(nameof(SupportingDocument.DocumentNo)));
					AssertNotNull("SupportingDocumentsGrid should have Remarks column", grid.GetColumnStyle(nameof(SupportingDocument.Remarks)));
					AssertNotNull("SupportingDocumentsGrid should have ControllingAgency column", grid.GetColumnStyle(nameof(SupportingDocument.ControllingAgency)));
					AssertNotNull("SupportingDocumentsGrid should have Type column", grid.GetColumnStyle(nameof(SupportingDocument.Type)));
					AssertNull("SupportingDocumentsGrid should not have Line Number column", grid.GetColumnStyle(nameof(SupportingDocument.LineNumber)));
				});
			}
			CombineAssertions("Line Number", () =>
			{
				AssertSupportingDocumentsGridHasLineNumber(ControllingMessageTypeList.Codes.NX201_01);
				AssertSupportingDocumentsGridHasLineNumber(ControllingMessageTypeList.Codes.NX201_07);
			});
		}

		void AssertSupportingDocumentsGridHasLineNumber(string controllingMessageType)
		{
			var (declaration, messageHeader) = GetTestObjects(controllingMessageType);
			Factory.Save();

			var wrapper = LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(declaration, messageHeader.TW1_ControllingMessageType);
			using (var form = new NXMDocumentForm(wrapper))
			{
				form.Show();
				var grid = form.Controls.Find("SupportingDocumentsGrid", true)[0] as ZGrid;
				AssertNotNull(controllingMessageType, grid.GetColumnStyle(nameof(SupportingDocument.LineNumber)));
			}
		}

		public void TestSendWithAdditionalWarningCheckBoxVisible()
		{
			using (var form = new NXMDocumentForm(Wrapper))
			{
				var sendWithAdditionalWarningCheckBox = form.Controls.Find("SendWithAdditionalWarningCheckBox", true)[0] as ZCheckBox;
				AssertEquals("sendWithAdditionalWarningCheckBox is hidden", false, sendWithAdditionalWarningCheckBox.Visible);
			}
		}

		(JobDeclarationForTestSendingObject declaration, CusTWControllingMessageHeader messageHeader) GetTestObjects(string messageType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclarationForTestSendingObject>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			var cusHead1 = declaration.ActiveEntryHeaders.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			cusHead1.CH_CEI_Instruction = entryInstruction.PK;
			var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = cusHead1.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = SharedJobMessageTypeList.Codes.Import;
			cusNum1.CE_ParentTable = AutoCusTWControllingMessageHeader.Schema.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "NO1";
			var invoice = declaration.Invoices.AddNew();
			var messageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = messageType;

			return (declaration, messageHeader);
		}

		LicensingMessageSendingObjectParent Wrapper
		{
			get
			{
				if (wrapper == null)
				{
					wrapper = GetWrapperByMessageType(ControllingMessageTypeList.Codes.NX301);
				}

				return wrapper;
			}
		}
		LicensingMessageSendingObjectParent wrapper;

		LicensingMessageSendingObjectParent GetWrapperByMessageType(ZString messageType)
		{
			var (declaration, _) = GetTestObjects(messageType);
			Factory.Save();
			return LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(declaration, messageType);
		}
	}
}
