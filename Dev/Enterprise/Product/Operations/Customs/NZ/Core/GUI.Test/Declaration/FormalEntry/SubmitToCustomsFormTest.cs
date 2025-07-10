using System;
using System.Windows.Forms;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Declaration.Testing;
using Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry;
using Enterprise.Customs.NZ.Registry;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Declaration.FormalEntry.Testing
{
	[TestedType(typeof(SubmitToCustomsForm))]
	public class SubmitToCustomsFormTest : ZFormBasherTest
	{
		public void TestMergeMonitorSuspender()
		{
			var declaration = Factory.New<JobDeclaration>();
			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForSea();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupExportInvoiceHeader("1001001", "FOB", "NZD", 10000m, ExchangeRateIndicatorList.Codes.NZD);
			decCreator.SetupExportInvoiceLine("8301100000F", "PADLOCKS", "AU", 10000m);
			decCreator.AddHouseBillAndContainerWithPackingDetailsAgainstContainerOnly("HOUSEBILL1", "OOCL0000006", "FCL", 100, "PK");
			decCreator.MergeDeclaration();
			AssertEquals("Declaration.MergeManager.RequiresMerge", false, declaration.MergeManager.RequiresMerge);
			var manager = new MessageManager(declaration, MessageManager.OperationType.SubmitMessage);
			AssertEquals("Declaration.MergeManager.RequiresMerge", false, declaration.MergeManager.RequiresMerge);
			using (var form = new SubmitToCustomsForm(manager))
			{
				declaration.JE_PDOOtherInfoValue = true;
			}

			AssertEquals("Declaration.MergeManager.RequiresMerge", false, declaration.MergeManager.RequiresMerge);
			declaration.JE_PDOOtherInfoValue = false;
			AssertEquals("Declaration.MergeManager.RequiresMerge", true, declaration.MergeManager.RequiresMerge);
		}

		public void TestOkButton()
		{
			var declaration = Factory.New<JobDeclaration>();
			var messageManager = new MessageManager(declaration, MessageManager.OperationType.SubmitMessage);
			using (var testForm = new SubmitToCustomsForm(messageManager))
			{
				messageManager.EnteredPinNumber = CurrentUsersPin.TestSystemPinCode;
				messageManager.EnteredRemarks = "The upside down catfish is a member of the Synodontis family";
				testForm.OkButton_Click(null, new EventArgs());
				AssertEquals("Should have no errors", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOkButtonFailsWithPin()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			declaration.DeclarationNumber = "12345678";
			var manager = new MessageManager(declaration, MessageManager.OperationType.SubmitMessage);
			using (var testForm = new SubmitToCustomsForm(manager))
			{
				testForm.OkButton_Click(null, new EventArgs());
				AssertEquals("Invalid PIN: Does not match your PIN entered in the Staff Master File.Invalid Remarks: Must have Remarks sending any amendment messages.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOkButtonFailsWithInvalidRemarks()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			declaration.DeclarationNumber = "12345678";
			var messageManager = new MessageManager(declaration, MessageManager.OperationType.SubmitMessage);
			using (var testForm = new SubmitToCustomsForm(messageManager))
			{
				messageManager.EnteredPinNumber = CurrentUsersPin.TestSystemPinCode;
				messageManager.EnteredRemarks = "";
				testForm.OkButton_Click(null, new EventArgs());
				AssertEquals("Should have remarks error", "Invalid Remarks: Must have Remarks sending any amendment messages.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ExportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var manager = new MessageManager(declaration, MessageManager.OperationType.ResetToOriginal);
			var newForm = new SubmitToCustomsForm(manager);
			MissingResourceStringChecker.ExcludeFromTest(newForm.RemarksTextBox);
			MissingResourceStringChecker.ExcludeFromTest(newForm.pinEntryTextBox);
			MissingResourceStringChecker.ExcludeFromTest(newForm.declarationTextBox);
			return newForm;
		}
		#endregion
	}
}
