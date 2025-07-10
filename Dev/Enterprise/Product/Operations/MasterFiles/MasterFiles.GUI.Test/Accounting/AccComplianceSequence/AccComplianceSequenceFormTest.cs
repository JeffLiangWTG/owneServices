using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccComplianceSequenceForm))]
	public class AccComplianceSequenceFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			AccComplianceSequence sequence = Factory.New<AccComplianceSequence>();
			var result = new AccComplianceSequenceFormForTest(sequence);
			result.ControllerID = ControllerIDs.AccComplianceSequence;
			return result;
		}

		public void TestAuditPluginIsAdded()
		{
			using (var form = (AccComplianceSequenceForm)GetFormToBashCore())
			{
				AssertNotNull("Compliance Sequence form should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}

		public void TestDeactivateVerb()
		{
			AccComplianceSequence sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			using (AccComplianceSequenceForm form = new AccComplianceSequenceFormForTest(sequence))
			{
				form.DisplayMode = ODisplayMode.Delete;
				AssertEquals(form.FormVerb, "Deactivate");
			}
		}

		public void TestEnablePaperStockOptionsToPrintComplianceDocuments()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();

			using (AccountingMasterFilesRegistry.Instance.EnablePaperStockOptionsToPrintComplianceDocuments.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var form = new AccComplianceSequenceFormForTest(sequence))
			{
				form.Show();
				Assert(form.MenuGuidFindBox.Enabled);
				Assert(form.XD_MaxChargesPerTransactionBoundText.Enabled);
				Assert(form.ComplianceRollupTypeDropEdit.Enabled);
				Assert(form.PrinterFindBox.Enabled);
			}

			using (AccountingMasterFilesRegistry.Instance.EnablePaperStockOptionsToPrintComplianceDocuments.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (var form = new AccComplianceSequenceFormForTest(sequence))
			{
				form.Show();
				Assert(!form.MenuGuidFindBox.Enabled);
				Assert(!form.XD_MaxChargesPerTransactionBoundText.Enabled);
				Assert(!form.ComplianceRollupTypeDropEdit.Enabled);
				Assert(!form.PrinterFindBox.Enabled);
			}
		}

		public void TestExtraValidationRequirementForTW()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC;
			sequence.XD_Code = "TXC";
			sequence.XD_Prefix = "AA";
			sequence.XD_StartNumber = 1;
			sequence.XD_EndNumber = 99999999;
			sequence.XD_NextNumber = 1;
			sequence.XD_MaximumNumberDigits = 8;
			sequence.XD_Description = "this is TXI - LM1";
			sequence.XD_StartDate = ZDate.Today;
			sequence.XD_ExpiryDate = ZDateTime.Today.AddDays(10);

			using (AccComplianceSequenceForm form = new AccComplianceSequenceFormForTest(sequence))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.Show();
				form.ValidateAndSave();
				AssertEquals("Credit controlled doc approval form will not be shown. Instead", @"You have chosen an End Number of 99999999.
You cannot edit your End Number once your Compliance Invoice Book has been saved.
If your Compliance Number Series are governed by local Tax Authorities, please ensure you have entered your End Number correctly.
Are you sure you want 99999999 as your End Number?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestExtraValidationRequirement()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Peru);
			AccComplianceSequence sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_SequenceClass = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
			sequence.XD_Code = "LM1";
			sequence.XD_Prefix = "";
			sequence.XD_StartNumber = 1;
			sequence.XD_EndNumber = 999999999;
			sequence.XD_NextNumber = 1;
			sequence.XD_MaximumNumberDigits = 9;
			sequence.XD_Description = "this is TXI - LM1";

			using (AccComplianceSequenceForm form = new AccComplianceSequenceFormForTest(sequence))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.Show();
				form.ValidateAndSave();
				AssertEquals("Credit controlled doc approval form will not be shown. Instead", @"You have NOT assigned a Series Prefix.
Are you sure you want to save this Compliance Invoice Book?", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (AccComplianceSequenceForm form = new AccComplianceSequenceFormForTest(sequence))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.Show();
				form.ValidateAndSave();
				AssertEquals("Credit controlled doc approval form will not be shown. Instead", @"You have chosen an End Number of 999999999.
You cannot edit your End Number once your Compliance Invoice Book has been saved.
If your Compliance Number Series are governed by local Tax Authorities, please ensure you have entered your End Number correctly.
Are you sure you want 999999999 as your End Number?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestComplianceNumberFormatDropEdit()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			using (var form = new AccComplianceSequenceFormForTest(sequence))
			{
				form.Show();

				AssertNotNull(form.ComplianceNumberFormatDropEdit);
				AssertEquals("ComplianceNumberFormatDropEdit", form.ComplianceNumberFormatDropEdit.Name);
			}
		}

		[TestDate(2012, 12, 12)]
		public void TestUpdateGUIIfBookFull()
		{
			AccComplianceSequence sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_SequenceClass = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
			sequence.XD_Code = "LM1";
			sequence.XD_StartNumber = 1;
			sequence.XD_EndNumber = 100;
			sequence.XD_NextNumber = 101;
			sequence.XD_MaximumNumberDigits = 9;
			using (AccComplianceSequenceForm form = new AccComplianceSequenceFormForTest(sequence))
			{
				form.Show();
				form.UpdateGUIIfBookFull(true);
				AssertEquals(form.NextNoTextBox.Visible, false);
				AssertEquals(form.numberFullLabel.Visible, true);
				AssertEquals(form.VoidingSequenceButton.Enabled, false);
				AssertEquals(form.XD_ExpiryDateEdit.DateTimeValue, new ZDateTime(2012, 12, 12));
			}
		}

		public void TestIsActiveCheckBox_CheckedChanged()
		{
			var sequence = CreateSequence("X1", "X1 Test", "TXE");

			using (AccountingMasterFilesRegistry.Instance.AllowReActivationOfInactiveComplianceBook.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (var form = new AccComplianceSequenceFormForTest(sequence))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.IsActiveCheckBox.Checked = false;
				AssertEquals(@"You are about to flag a compliance book as inactive. 
This action cannot be reversed. Once Inactive, a compliance book cannot be activated again and you will not be able to assign compliance numbers from this book.
This is controlled by this registry: Accounting -> Government Compliance Invoice Document -> Allow re-activation of inactive compliance books.
Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				Assert(!form.IsActiveCheckBox.Checked);
				Assert(!form.IsActiveCheckBox.Enabled);
			}

			sequence.XD_IsActive = false;

			using (AccountingMasterFilesRegistry.Instance.AllowReActivationOfInactiveComplianceBook.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (var form = new AccComplianceSequenceFormForTest(sequence))
			{
				form.Show();
				Assert(!form.IsActiveCheckBox.Checked);
				Assert(!form.IsActiveCheckBox.Enabled);
			}

			using (AccountingMasterFilesRegistry.Instance.AllowReActivationOfInactiveComplianceBook.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var form = new AccComplianceSequenceFormForTest(sequence))
			{
				form.Show();
				Assert(!form.IsActiveCheckBox.Checked);
				Assert(form.IsActiveCheckBox.Enabled);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.IsActiveCheckBox.Checked = true;
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(form.IsActiveCheckBox.Checked);
				Assert(form.IsActiveCheckBox.Enabled);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.IsActiveCheckBox.Checked = false;
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!form.IsActiveCheckBox.Checked);
				Assert(form.IsActiveCheckBox.Enabled);
			}
		}

		public void TestIsActiveCheckBox_UpdateCheckBoxAccessibility()
		{
			var complianceSequance1 = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequance1.XD_IsActive = false;
			var complianceSequance2 = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequance2.XD_IsActive = false;

			var mockProvier = new Mock<IComplianceSequencePresentationProvider>();
			mockProvier.Setup(x => x.CanReactivate(It.Is<AccComplianceSequence>(o => o == complianceSequance1))).Returns(ZString.Empty);
			mockProvier.Setup(x => x.CanReactivate(It.Is<AccComplianceSequence>(o => o == complianceSequance2))).Returns("Can not reactive");

			var mockFactory = new Mock<IAccountingMasterFilesDependencyFactory>();
			mockFactory.Setup(x => x.GetComplianceSequencePresentationProvider()).Returns(mockProvier.Object);

			using (ObjectFactory.Substitute(mockFactory.Object))
			{
				using (var form = new AccComplianceSequenceFormForTest(complianceSequance1))
				{
					form.Show();
					AssertEquals(true, form.IsActiveCheckBox.Enabled);
				}

				using (var form = new AccComplianceSequenceFormForTest(complianceSequance2))
				{
					form.Show();
					AssertEquals(false, form.IsActiveCheckBox.Enabled);
				}

				mockFactory.Verify(x => x.GetComplianceSequencePresentationProvider(), Times.Exactly(2));
				mockProvier.Verify(x => x.CanReactivate(complianceSequance1), Times.Once);
				mockProvier.Verify(x => x.CanReactivate(complianceSequance2), Times.Once);
			}
		}

		public void TestVoidingSequence()
		{
			var sequence = CreateSequence("X1", "X1 Test", "TXE");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			using (AccComplianceSequenceForm form = new AccComplianceSequenceFormForTest(sequence))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.VoidingSequenceButton_Click(null, null);
				AssertEquals("Voiding of Sequence No. is not allowed for 'TXE' and 'TCE' sub type.", UnitTestUserNotification.Instance.LastMessage.Text);

				sequence.XD_SequenceClass = "TCE";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.VoidingSequenceButton_Click(null, null);
				AssertEquals("Voiding of Sequence No. is not allowed for 'TXE' and 'TCE' sub type.", UnitTestUserNotification.Instance.LastMessage.Text);

				sequence.XD_SequenceClass = "NTC";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.VoidingSequenceButton_Click(null, null);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		AccComplianceSequence CreateSequence(string code, string description, string sequenceClass)
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_Code = code;
			sequence.XD_Description = description;
			sequence.XD_MaximumNumberDigits = 8;
			sequence.XD_StartNumber = 1;
			sequence.XD_EndNumber = 100;
			sequence.XD_NextNumber = 5;
			sequence.XD_IsActive = true;
			sequence.XD_SequenceClass = sequenceClass;
			sequence.XD_Prefix = "XYZ";
			return sequence;
		}

		public void TestValidityPeriodMandatory_PST()
		{
			AssertValidityPeriodMandatory(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		public void TestValidityPeriodMandatory_INV()
		{
			AssertValidityPeriodMandatory(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		void AssertValidityPeriodMandatory(string dateOption)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var sequence = CreateSequence("X1", "X1 Test", ItalyComplianceInfo.ComplianceSubTypeCodes.ARI);

				using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code))
				using (var form = new AccComplianceSequenceFormForTest(sequence))
				{
					form.Show();

					Assert(!sequence.IsInDatabase);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.OnApplyButtonClick(this, EventArgs.Empty);

					AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(sequence.IsInDatabase);
				}

				sequence = CreateSequence("X2", "X2 Test", ItalyComplianceInfo.ComplianceSubTypeCodes.ARS);

				using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, dateOption))
				using (var form = new AccComplianceSequenceFormForTest(sequence))
				{
					var startDate = new ZDate(2019, 01, 01);

					form.Show();
					Assert(!sequence.IsInDatabase);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.OnApplyButtonClick(this, EventArgs.Empty);

					AssertNotNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertHasError(sequence.XD_StartDateInfo, "Please enter a value.");
					AssertHasError(sequence.XD_ExpiryDateInfo, "Please enter an Expiry Date.");
					Assert(!sequence.IsInDatabase);

					sequence.XD_StartDate = startDate;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.OnApplyButtonClick(this, EventArgs.Empty);

					AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNoErrors("StartDate should have no errors", sequence.XD_StartDateInfo);
					AssertHasError(sequence.XD_ExpiryDateInfo, "Please enter an Expiry Date.");
					Assert(!sequence.IsInDatabase);

					sequence.XD_ExpiryDate = startDate.AddDays(30);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.OnApplyButtonClick(this, EventArgs.Empty);

					AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNoErrors("StartDate should have no errors", sequence.XD_StartDateInfo);
					AssertNoErrors("StartDate should have no errors", sequence.XD_ExpiryDateInfo);
					Assert(sequence.IsInDatabase);
				}
			}
		}

		public void TestValidityPeriodMandatory_AP()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var sequence = CreateSequence("X1", "X1 Test", ItalyComplianceInfo.ComplianceSubTypeCodes.API);

				using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AP.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code))
				using (var form = new AccComplianceSequenceFormForTest(sequence))
				{
					form.Show();

					Assert(!sequence.IsInDatabase);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.OnApplyButtonClick(this, EventArgs.Empty);

					AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(sequence.IsInDatabase);
				}
			}
		}
	}

	class AccComplianceSequenceFormForTest : AccComplianceSequenceForm
	{
		public AccComplianceSequenceFormForTest(AccComplianceSequence bO) : base(bO) { }

		public new void OnApplyButtonClick(object sender, EventArgs e) => base.OnApplyButtonClick(sender, e);
	}
}
