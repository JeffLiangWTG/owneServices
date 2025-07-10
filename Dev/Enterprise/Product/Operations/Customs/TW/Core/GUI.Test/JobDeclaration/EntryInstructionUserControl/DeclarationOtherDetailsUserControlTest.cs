using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(DeclarationOtherDetailsUserControl))]
	sealed class DeclarationOtherDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestCalculateDaysOfDelayedDeclarationIfRequiredWhenDateForDutyChanged_SyncFromShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateAtFinalDestination = new ZDateTime(2019, 03, 01);
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 03, 20);
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = false;
			entryInstruction.CEI_DaysOfDelayedDeclaration = 0;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				if (brokerageControl.DeclarationUserControlForTesting is JobDeclarationUserControl declarationUserControl)
				{
					var entryDetailsTabPage = declarationUserControl.FindSingle<ZTabPage>(x => x.Name == "EntryDetailsTabPage");
					declarationUserControl.RightTabControl.SelectedTab = entryDetailsTabPage;
					AssertEquals(4, entryInstruction.CEI_DaysOfDelayedDeclaration);
				}
			}

			declaration.JE_OverrideFreightDefaults = true;
			entryInstruction.CEI_DaysOfDelayedDeclaration = 0;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				if (brokerageControl.DeclarationUserControlForTesting is JobDeclarationUserControl declarationUserControl)
				{
					var entryDetailsTabPage = declarationUserControl.FindSingle<ZTabPage>(x => x.Name == "EntryDetailsTabPage");
					declarationUserControl.RightTabControl.SelectedTab = entryDetailsTabPage;
					AssertEquals(0, entryInstruction.CEI_DaysOfDelayedDeclaration);
				}
			}

			declaration.JE_OverrideFreightDefaults = false;
			entryInstruction.CEI_DaysOfDelayedDeclaration = 0;
			Factory.Save();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				if (brokerageControl.DeclarationUserControlForTesting is JobDeclarationUserControl declarationUserControl)
				{
					var entryDetailsTabPage = declarationUserControl.FindSingle<ZTabPage>(x => x.Name == "EntryDetailsTabPage");
					declarationUserControl.RightTabControl.SelectedTab = entryDetailsTabPage;
					AssertEquals(0, entryInstruction.CEI_DaysOfDelayedDeclaration);
				}
			}
		}

		public void TestCalculateDaysOfDelayedDeclarationIfRequiredWhenDateForDutyChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				if (brokerageControl.DeclarationUserControlForTesting is JobDeclarationUserControl declarationUserControl)
				{
					var entryDetailsTabPage = declarationUserControl.FindSingle<ZTabPage>(x => x.Name == "EntryDetailsTabPage");
					declarationUserControl.RightTabControl.SelectedTab = entryDetailsTabPage;
					var entryInstruction = declaration.CusEntryInstruction;
					declaration.JE_DateAtFinalDestination = new ZDateTime(2019, 03, 01);
					entryInstruction.CEI_DaysOfDelayedDeclaration = 0;
					entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 03, 20);
					AssertEquals(4, entryInstruction.CEI_DaysOfDelayedDeclaration);

					entryInstruction.CEI_DaysOfDelayedDeclaration = 5;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 03, 21);
					CombineAssertions(() =>
					{
						AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Caption);
						AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals(5, entryInstruction.CEI_DaysOfDelayedDeclaration);
					});

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
					CombineAssertions(() =>
					{
						AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Caption);
						AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals(5, entryInstruction.CEI_DaysOfDelayedDeclaration);
					});

					entryInstruction.CEI_DaysOfDelayedDeclaration = 4;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 03, 21);
					CombineAssertions(() =>
					{
						AssertEquals("Days Of Delayed", UnitTestUserNotification.Instance.LastMessage.Caption);
						AssertEquals("The entered Days of Delayed is 4, and the calculated Days of Delayed is 5, do you want to override with calculated Days of Delayed?", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals(5, entryInstruction.CEI_DaysOfDelayedDeclaration);
					});

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 03, 22);
					AssertEquals(5, entryInstruction.CEI_DaysOfDelayedDeclaration);

					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					entryInstruction.CEI_DaysOfDelayedDeclaration = 0;
					entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 03, 23);
					AssertEquals(0, entryInstruction.CEI_DaysOfDelayedDeclaration);
				}
			}
		}

		public void TestCalculateDaysOfDelayedDeclarationIfRequiredWhenDateAtFinalDestinationChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				if (brokerageControl.DeclarationUserControlForTesting is JobDeclarationUserControl jobDeclarationUserControl)
				{
					var entryDetailsTabPage = jobDeclarationUserControl.FindSingle<ZTabPage>(x => x.Name == "EntryDetailsTabPage");
					jobDeclarationUserControl.RightTabControl.SelectedTab = entryDetailsTabPage;
					var declarationOtherDetailsControl = entryDetailsTabPage.FindSingle<DeclarationOtherDetailsUserControl>(x => x.Name == "TWDeclarationOtherDetailsUserControl");
					var entryInstruction = declaration.CusEntryInstruction;
					entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 03, 24);
					declaration.JE_DateAtFinalDestination = new ZDateTime(2019, 03, 05);
					AssertEquals(4, entryInstruction.CEI_DaysOfDelayedDeclaration);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					declaration.JE_DateAtFinalDestination = new ZDateTime(2019, 03, 04);
					CombineAssertions(() =>
					{
						AssertEquals("Days Of Delayed", UnitTestUserNotification.Instance.LastMessage.Caption);
						AssertEquals("The entered Days of Delayed is 4, and the calculated Days of Delayed is 5, do you want to override with calculated Days of Delayed?", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals(5, entryInstruction.CEI_DaysOfDelayedDeclaration);
					});

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					declaration.JE_DateAtFinalDestination = new ZDateTime(2019, 03, 03);
					AssertEquals(5, entryInstruction.CEI_DaysOfDelayedDeclaration);

					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					entryInstruction.CEI_DaysOfDelayedDeclaration = 0;
					declaration.JE_DateAtFinalDestination = new ZDateTime(2019, 03, 02);
					AssertEquals(0, entryInstruction.CEI_DaysOfDelayedDeclaration);
				}
			}
		}

		public void TestUpdateRORLinesPaymentMethodWhenCEI_RORPaymentMethodChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				if (brokerageControl.DeclarationUserControlForTesting is JobDeclarationUserControl declarationUserControl)
				{
					var entryDetailsTabPage = declarationUserControl.FindSingle<ZTabPage>(x => x.Name == "EntryDetailsTabPage");
					declarationUserControl.RightTabControl.SelectedTab = entryDetailsTabPage;
					var entryInstruction = declaration.CusEntryInstruction;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					entryInstruction.CEI_RORPaymentMethod = RORPaymentMethodList.Codes.NonCashPayment;
					CombineAssertions("Thers is no any ROR lines.", () =>
					{
						AssertNull(UnitTestUserNotification.Instance.LastMessage.Caption);
						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					});

					var rorInvoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
					rorInvoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
					var ctTax = rorInvoiceLine.Taxes.AddNew();
					ctTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.CT;
					var ssTax = rorInvoiceLine.Taxes.AddNew();
					ssTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.SS;
					var nonRorInvoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
					nonRorInvoiceLine.JI_Procedure = Constants.ProcedureCodes._37;
					entryInstruction.CEI_RORPaymentMethod = RORPaymentMethodList.Codes.RorPayment;
					CombineAssertions("Test Answer Yes, then update all ROR lines.", () =>
					{
						AssertEquals("'ROR' payment method", UnitTestUserNotification.Instance.LastMessage.Caption);
						AssertEquals("Do you want to update all ROR lines to be use same 'ROR' payment method?", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals(RORPaymentMethodList.Codes.RorPayment, rorInvoiceLine.JI_DtyPymntMthd);
						AssertEquals(RORPaymentMethodList.Codes.RorPayment, rorInvoiceLine.JI_VatPymntMthd);
						AssertEquals(RORPaymentMethodList.Codes.RorPayment, rorInvoiceLine.JI_TpfPymntMthd);
						AssertEquals(RORPaymentMethodList.Codes.RorPayment, ctTax.JLT_MethodOfPayment);
						AssertEquals(RORPaymentMethodList.Codes.RorPayment, ssTax.JLT_MethodOfPayment);
						AssertEquals(ZString.Empty, nonRorInvoiceLine.JI_DtyPymntMthd);
						AssertEquals(ZString.Empty, nonRorInvoiceLine.JI_VatPymntMthd);
						AssertEquals(ZString.Empty, nonRorInvoiceLine.JI_TpfPymntMthd);
					});

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					entryInstruction.CEI_RORPaymentMethod = RORPaymentMethodList.Codes.NonCashPayment;
					CombineAssertions("Test Answer No, then do nothing.", () =>
					{
						AssertEquals("'DEF' payment method", UnitTestUserNotification.Instance.LastMessage.Caption);
						AssertEquals("Do you want to update all ROR lines to be use same 'DEF' payment method?", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals(RORPaymentMethodList.Codes.RorPayment, rorInvoiceLine.JI_DtyPymntMthd);
						AssertEquals(RORPaymentMethodList.Codes.RorPayment, rorInvoiceLine.JI_VatPymntMthd);
						AssertEquals(RORPaymentMethodList.Codes.RorPayment, rorInvoiceLine.JI_TpfPymntMthd);
						AssertEquals(RORPaymentMethodList.Codes.RorPayment, ctTax.JLT_MethodOfPayment);
						AssertEquals(RORPaymentMethodList.Codes.RorPayment, ssTax.JLT_MethodOfPayment);
						AssertEquals(ZString.Empty, nonRorInvoiceLine.JI_DtyPymntMthd);
						AssertEquals(ZString.Empty, nonRorInvoiceLine.JI_VatPymntMthd);
						AssertEquals(ZString.Empty, nonRorInvoiceLine.JI_TpfPymntMthd);
					});
				}
			}
		}

		public void TestControlsVisible()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				if (jobDeclarationUserControl != null)
				{
					var entryDetailsTabPage = jobDeclarationUserControl.FindSingle<ZTabPage>(x => x.Name == "EntryDetailsTabPage");
					AssertEquals("DeclarationOtherDetailsUserControl is contructed", 1, entryDetailsTabPage.Controls.OfType<DeclarationOtherDetailsUserControl>().Count());
					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
					var declarationOtherDetailsControl = entryDetailsTabPage.FindSingle<DeclarationOtherDetailsUserControl>(x => x.Name == "TWDeclarationOtherDetailsUserControl");
					var controlNameListExport = new List<string>(new string[] { "DocumentNumbersUserControl" });
					foreach (var controlName in controlNameListExport)
					{
						var control = declarationOtherDetailsControl.FindSingle<Control>(x => x.Name == controlName);
						AssertNotNull(control);
						AssertEquals($"The control name '{control.Name}' in should be visible when message type is Export", control.Visible, true);
					}

					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					var controlNameListImport = new List<string>(new string[] { "CEI_WaiverOfExemptionCheckBox", "CEI_PrintDutyMemoCheckBox", "CEI_DaysOfDelayedDeclarationCalcEdit", "ExaminationDetailsGroupBox", "ExaminationDetailsPanel", "TW_ICIExamTimeDateEdit", "ICIExamLocationDropEdit", "RORPaymentMethodLDropEdit" });
					foreach (var controlName in controlNameListImport)
					{
						var control = declarationOtherDetailsControl.FindSingle<Control>(x => x.Name == controlName);
						AssertNotNull(control);
						AssertEquals($"The control name '{control.Name}' in should be visible when message type is Import", control.Visible, true);
					}
				}
			}
		}

		public void TestSplitMarkCheckBoxVisibility()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				if (jobDeclarationUserControl != null)
				{
					var entryDetailsTabPage = jobDeclarationUserControl.FindSingle<ZTabPage>(x => x.Name == "EntryDetailsTabPage");
					var declarationOtherDetailsControl = entryDetailsTabPage.FindSingle<DeclarationOtherDetailsUserControl>(x => x.Name == "TWDeclarationOtherDetailsUserControl");
					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
					declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
					var splitMarkControl = declarationOtherDetailsControl.FindSingle<ZCheckBox>(x => x.Name == "SplitMarkCheckBox");
					AssertNotNull(splitMarkControl);
					AssertEquals($"The control name '{splitMarkControl.Name}' in should not be visible when message type is Export", splitMarkControl.Visible, false);
					declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
					AssertEquals($"The control name '{splitMarkControl.Name}' in should not be visible when message type is Export", splitMarkControl.Visible, false);
					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
					AssertEquals($"The control name '{splitMarkControl.Name}' in should be visible when message type is Import and transport mode is AIR", splitMarkControl.Visible, true);
					declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
					AssertEquals($"The control name '{splitMarkControl.Name}' in should be visible when message type is Import and transport mode is SEA", splitMarkControl.Visible, true);
				}
			}
		}

		public void TestCharacterCasing()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;
				var declarationControl = brokerageControl.DeclarationTabPage.FindSingle<JobDeclarationUserControl>(x => x.Name == "JobDeclarationUserControl");
				var entryDetailsTabPage = declarationControl.FindSingle<ZTabPage>(x => x.Name == "EntryDetailsTabPage");
				var declarationOtherDetailsUserControl = entryDetailsTabPage.FindSingle<DeclarationOtherDetailsUserControl>(x => x.Name == "TWDeclarationOtherDetailsUserControl");
				foreach (var name in new string[] { "TW_TradersRemarksTextBox" })
				{
					var textBox = declarationOtherDetailsUserControl.FindSingle<ZTextBox>(c => c.Name == name);
					AssertNotNull(name, textBox);
					AssertEquals(name, CharacterCasing.Normal, textBox.CharacterCasing);
				}
			}
		}

		[TestDate(2019, 06, 22, 11, 13, 24)]
		public void TestSwitchingTabsPicksUpICIExamChanges()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_CustomsOffice = "AA";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G1;
			entryInstruction.CEI_CustomsOffice = "AA";
			invoiceLine.JI_CEI = entryInstruction.PK;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123456";
			var mergedLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = mergedLine.PK;
			entryInstruction.TW_ICIExamLocation = "123";
			entryInstruction.TW_ICIExamTime = ZDateTime.Now;
			Factory.Save();
			AssertEquals("Exam Location was saved", "123", entryInstruction.TW_ICIExamLocation);
			AssertEquals("Exam Time was saved", ZDateTime.Now, entryInstruction.TW_ICIExamTime);
			AssertEquals("Matching service was created", 1, declaration.DocsAndCartage.Services.Count);
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;
				var declarationControl = brokerageControl.DeclarationTabPage.FindSingle<JobDeclarationUserControl>(x => x.Name == "JobDeclarationUserControl");
				var entryDetailsTabPage = declarationControl.FindSingle<ZTabPage>(x => x.Name == "EntryDetailsTabPage");
				var declarationOtherDetailsUserControl = entryDetailsTabPage.FindSingle<DeclarationOtherDetailsUserControl>(x => x.Name == "TWDeclarationOtherDetailsUserControl");
				var examLocationControl = declarationOtherDetailsUserControl.FindSingle<ZDropEdit>("ICIExamLocationDropEdit");
				AssertEquals("123", examLocationControl.Text);
				var examTimeControl = declarationOtherDetailsUserControl.FindSingle<ZDateEdit>("TW_ICIExamTimeDateEdit");
				AssertEquals(ZDateTime.Now, examTimeControl.DateTimeValue);
				declarationControl.RightTabControl.SelectedTab = declarationControl.OrganisationsTabPage;
				declaration.DocsAndCartage.Services[0].ES_SubLocation = "456";
				declaration.DocsAndCartage.Services[0].ES_Booked = ZDateTime.BrettsBirthday;
				declarationControl.RightTabControl.SelectedTab = entryDetailsTabPage;
				AssertEquals("456", examLocationControl.Text);
				AssertEquals(ZDateTime.BrettsBirthday, examTimeControl.DateTimeValue);
			}
		}

		public void TestBindingMembers()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_CustomsOffice = "AA";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G1;
			entryInstruction.CEI_CustomsOffice = "AA";
			invoiceLine.JI_CEI = entryInstruction.PK;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;
				var declarationControl = brokerageControl.DeclarationTabPage.FindSingle<JobDeclarationUserControl>(x => x.Name == "JobDeclarationUserControl");
				var entryDetailsTabPage = declarationControl.FindSingle<ZTabPage>(x => x.Name == "EntryDetailsTabPage");
				var entryInstructionUserControl = entryDetailsTabPage.FindSingle<DeclarationOtherDetailsUserControl>(x => x.Name == "TWDeclarationOtherDetailsUserControl");
				var bingdingSource = entryInstructionUserControl.BindingSource;
				var tradersRemarksNoteTextBox = entryInstructionUserControl.FindSingle<ZTextBox>(c => c.Name == "TW_TradersRemarksTextBox");
				AssertEquals("BindingMember", "CustomsEntryInstructions.TW_TradersRemarks", bingdingSource.GetBindingMember(tradersRemarksNoteTextBox));
				bingdingSource = declarationControl.BindingSource;
				var packageDescriptionLongTextControl = declarationControl.FindSingle<LongTextControl>(c => c.Name == "CEI_PackageDescriptionLongTextControl");
				AssertEquals("BindingMember", "CustomsEntryInstructions.CEI_PackageDescription", bingdingSource.GetBindingMember(packageDescriptionLongTextControl));
			}
		}

		public void TestControlsReadOnly()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				if (jobDeclarationUserControl != null)
				{
					var entryDetailsTabPage = jobDeclarationUserControl.FindSingle<ZTabPage>(x => x.Name == "EntryDetailsTabPage");
					jobDeclarationUserControl.RightTabControl.SelectedTab = entryDetailsTabPage;
					var entryInstructionUserControl = entryDetailsTabPage.FindSingle<DeclarationOtherDetailsUserControl>(x => x.Name == "TWDeclarationOtherDetailsUserControl");
					var ucrNumberTextBox = entryInstructionUserControl.FindSingle<ZTextBox>(x => x.Name == "UCRNumberTextBox");
					AssertEquals(false, ucrNumberTextBox.ReadOnly);
				}
			}
		}

		public void TestTW_GoodsLocationControlsCaption()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				if (jobDeclarationUserControl != null)
				{
					var entryDetailsTabPage = jobDeclarationUserControl.FindSingle<ZTabPage>(x => x.Name == "EntryDetailsTabPage");
					jobDeclarationUserControl.RightTabControl.SelectedTab = entryDetailsTabPage;
					var entryInstructionUserControl = entryDetailsTabPage.FindSingle<DeclarationOtherDetailsUserControl>(x => x.Name == "TWDeclarationOtherDetailsUserControl");
					var controlTW_GoodsLocationFindBox = entryInstructionUserControl.FindSingle<ZCodeFindBox>(x => x.Name == "CEI_GoodsLocationFindBox");
					AssertEquals("Export Location", controlTW_GoodsLocationFindBox.CaptionResourceString.Caption);
					AssertEquals("The receipt location of the Office of Receipt.", controlTW_GoodsLocationFindBox.CaptionResourceString.FullDescription);
					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					AssertEquals("Receipt Location", controlTW_GoodsLocationFindBox.CaptionResourceString.Caption);
					AssertEquals("The receipt location of the Office of Receipt.", controlTW_GoodsLocationFindBox.CaptionResourceString.FullDescription);
				}
			}
		}

		public void TestHandleDeclarationControlVisibilityChangedCore()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new ZForm(declaration))
			{
				var control = new JobDeclarationUserControl { JobDeclaration = declaration };
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				var entryDetailsTabPage = control.FindSingle<ZTabPage>(x => x.Name == "EntryDetailsTabPage");
				control.RightTabControl.SelectedTab = entryDetailsTabPage;
				var entryInstructionUserControl = entryDetailsTabPage.FindSingle<DeclarationOtherDetailsUserControl>(x => x.Name == "TWDeclarationOtherDetailsUserControl");
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				var controlBankAccountTextBox = entryInstructionUserControl.FindSingle<ZTextBox>(x => x.Name == "BankAccountTextBox");
				Assert("Bank Account should not be visible", !controlBankAccountTextBox.Visible);
				var defermentAccountNumberDropEdit = entryInstructionUserControl.FindSingle<ZDropEdit>(x => x.Name == "JE_DefermentAccountNumberDropEdit");
				Assert("Guarantee should not be visible", !defermentAccountNumberDropEdit.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				Assert("Bank Account should be visible", controlBankAccountTextBox.Visible);
				Assert("Guarantee should be visible", defermentAccountNumberDropEdit.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
				Assert("Bank Account should not be visible", !controlBankAccountTextBox.Visible);
				Assert("Guarantee should not be visible", !defermentAccountNumberDropEdit.Visible);
			}
		}

		public void TestJE_DefermentAccountNumberDropEditDescriptionBoxVisible()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new ZForm(declaration))
			{
				var control = new JobDeclarationUserControl();
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				var entryDetailsTabPage = control.FindSingle<ZTabPage>(x => x.Name == "EntryDetailsTabPage");
				control.RightTabControl.SelectedTab = entryDetailsTabPage;
				var entryInstructionUserControl = entryDetailsTabPage.FindSingle<DeclarationOtherDetailsUserControl>(x => x.Name == "TWDeclarationOtherDetailsUserControl");
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				var controlJE_DefermentAccountNumberDropEdit = entryInstructionUserControl.FindSingle<ZDropEdit>(x => x.Name == "JE_DefermentAccountNumberDropEdit");
				Assert("Guarantee DescriptionBox should be not visible", !controlJE_DefermentAccountNumberDropEdit.DescriptionBox.Visible);
			}
		}

		public void TestJE_CustomsOfficeCodeFindBox()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;
				var declarationControl = brokerageControl.DeclarationTabPage.FindSingle<JobDeclarationUserControl>(x => x.Name == "JobDeclarationUserControl");
				var entryDetailsTabPage = declarationControl.FindSingle<ZTabPage>(x => x.Name == "EntryDetailsTabPage");
				declarationControl.RightTabControl.SelectedTab = entryDetailsTabPage;
				var entryInstructionUserControl = entryDetailsTabPage.FindSingle<DeclarationOtherDetailsUserControl>(x => x.Name == "TWDeclarationOtherDetailsUserControl");
				var control = entryInstructionUserControl.FindSingle<UserControl>(x => x.Name == "JE_CustomsOfficeDropEdit");
				Assert(control is ZDropEdit);
			}
		}

		public void TestJE_LocationOfGoodsCodeFindBox()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;
				var declarationControl = brokerageControl.DeclarationTabPage.FindSingle<JobDeclarationUserControl>(x => x.Name == "JobDeclarationUserControl");
				var entryDetailsTabPage = declarationControl.FindSingle<ZTabPage>(x => x.Name == "EntryDetailsTabPage");
				declarationControl.RightTabControl.SelectedTab = entryDetailsTabPage;
				var entryInstructionUserControl = entryDetailsTabPage.FindSingle<DeclarationOtherDetailsUserControl>(x => x.Name == "TWDeclarationOtherDetailsUserControl");
				var control = entryInstructionUserControl.FindSingle<UserControl>(x => x.Name == "JE_LocationOfGoodsCodeFindBox");
				Assert(control is ZCodeFindBox);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals(false, control.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals(true, control.Visible);
			}
		}

		public void TestJE_CustomsOfficeDropEdit()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;
				var declarationControl = brokerageControl.DeclarationTabPage.FindSingle<JobDeclarationUserControl>(x => x.Name == "JobDeclarationUserControl");
				var entryDetailsTabPage = declarationControl.FindSingle<ZTabPage>(x => x.Name == "EntryDetailsTabPage");
				declarationControl.RightTabControl.SelectedTab = entryDetailsTabPage;
				var entryInstructionUserControl = entryDetailsTabPage.FindSingle<DeclarationOtherDetailsUserControl>(x => x.Name == "TWDeclarationOtherDetailsUserControl");
				var control = entryInstructionUserControl.FindSingle<ZDropEdit>(x => x.Name == "JE_CustomsOfficeDropEdit");
				AssertEquals("Office of Unlading", control.CaptionResourceString.Caption);
				AssertEquals("Unlading Office", control.CaptionResourceString.ShortCaption);
				AssertEquals("The Office of Unlading of the declaration. It's used to generate the third and fourth digits of the entry number.", control.CaptionResourceString.FullDescription);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Office of Lading", control.CaptionResourceString.Caption);
				AssertEquals("Lading Office", control.CaptionResourceString.ShortCaption);
				AssertEquals("The Office of Lading of the declaration. It's used to generate the third and fourth digits of the entry number.", control.CaptionResourceString.FullDescription);
			}
		}
	}
}
