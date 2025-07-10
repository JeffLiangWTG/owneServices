using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	[TestedType(typeof(DeferredSubmissionForm))]
	sealed class DeferredSubmissionFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var helper = new DeferredSubmissionHelper(Declaration);
			var deferredSubmission = new DeferredSubmission(helper);
			return new DeferredSubmissionForm(deferredSubmission);
		}

		JobDeclaration Declaration => fDeclaration ?? (fDeclaration = Factory.NewWithValidTestData<JobDeclaration>());
		JobDeclaration fDeclaration;

		public void TestArrivalDateEditIsReadOnly()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				var arrivalDateEdit = form.FindSingle<ZDateEdit>("ArrivalDateEdit");
				AssertEquals("ArrivalDateEdit is ReadOnly", true, arrivalDateEdit.ReadOnly);
			}
		}

		public void TestDeferredAccountDropEditIsReadOnly()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				var deferredAccountDropEdit = form.FindSingle<ZDropEdit>("DeferredAccountDropEdit");
				AssertEquals("DeferredAccountDropEdit is ReadOnly", true, deferredAccountDropEdit.ReadOnly);
				var overwriteCheckBox = form.FindSingle<ZCheckBox>("OverwriteCheckBox");
				overwriteCheckBox.Checked = true;
				AssertEquals("DeferredAccountDropEdit is Editable", false, deferredAccountDropEdit.ReadOnly);
				overwriteCheckBox.Checked = false;
				AssertEquals("DeferredAccountDropEdit is ReadOnly", true, deferredAccountDropEdit.ReadOnly);
			}
		}

		public void TestSubmissionDateEditIsReadOnly()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				var messageDeferralSettings = new AutomaticDeferredSelection()
				{ AllowAutomaticDeferredSelection = false, DaysBeforeETA = 0 };
				using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
				{
					var submissionDateEdit = form.FindSingle<ZDateEdit>("SubmissionDateEdit");
					AssertEquals("SubmissionDateEdit is ReadOnly", true, submissionDateEdit.ReadOnly);
					var overwriteCheckBox = form.FindSingle<ZCheckBox>("OverwriteCheckBox");
					overwriteCheckBox.Checked = true;
					AssertEquals("SubmissionDateEdit is ReadOnly", true, submissionDateEdit.ReadOnly);
					overwriteCheckBox.Checked = false;
					AssertEquals("SubmissionDateEdit is ReadOnly", true, submissionDateEdit.ReadOnly);
				}

				messageDeferralSettings = new AutomaticDeferredSelection()
				{ AllowAutomaticDeferredSelection = false, DaysBeforeETA = 2 };
				using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
				{
					Factory.ClearCachedValue<AutomaticDeferredSelection>(DeferredSubmissionHelper.MessageDeferralSettingsCacheKey);
					var submissionDateEdit = form.FindSingle<ZDateEdit>("SubmissionDateEdit");
					AssertEquals("SubmissionDateEdit is ReadOnly", true, submissionDateEdit.ReadOnly);
					var overwriteCheckBox = form.FindSingle<ZCheckBox>("OverwriteCheckBox");
					overwriteCheckBox.Checked = true;
					AssertEquals("SubmissionDateEdit is Editable", false, submissionDateEdit.ReadOnly);
					overwriteCheckBox.Checked = false;
					AssertEquals("SubmissionDateEdit is ReadOnly", true, submissionDateEdit.ReadOnly);
					Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
					overwriteCheckBox.Checked = true;
					AssertEquals("SubmissionDateEdit is ReadOnly", true, submissionDateEdit.ReadOnly);
					overwriteCheckBox.Checked = false;
					AssertEquals("SubmissionDateEdit is ReadOnly", true, submissionDateEdit.ReadOnly);
				}
			}
		}

		public void TestCannotSubmitWhenFormIsInError()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("DFM");
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CompanyData.OB_IsCreditor = true;
			org1.OH_FullName = "ORG1";
			org1.OH_Code = "AG1";
			org1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "11223344", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var maps = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			var mapping1 = maps.AddNew();
			mapping1.AccountStartDay = 5;
			mapping1.OrganizationPK = org1.PK;
			mapping1.CreditorPK = org1.PK;
			mapping1.CustomsOfficeCode = "DFM";
			mapping1.Cash = false;
			mapping1.ImporterPays = false;
			mapping1.FinancialAccountNumber = "1111111111";
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
			var messageDeferralSettings = new AutomaticDeferredSelection()
			{ AllowAutomaticDeferredSelection = true, DaysBeforeETA = 2 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				using (var form = (DeferredSubmissionForm)GetFormToBash())
				{
					Declaration.JE_CustomsOffice = "DFM";
					form.Show();
					var deferredSubmission = form.BusinessEntity as DeferredSubmission;
					deferredSubmission.IsOverwritten = true;
					deferredSubmission.SubmissionDate = ZDate.Today.AddDays(-1);
					deferredSubmission.PaymentMethod = "D";
					deferredSubmission.DeferredAccount = "1111111111";
					var submitButton = form.FindSingle<ZButton>("SubmitButton");
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					submitButton.PerformClick();
					AssertMultilineASCIIEquals("LastMessage", "There are errors that need to be corrected before this Declaration can be Submitted.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Does not submit", DialogResult.None, form.DialogResult);
					AssertEquals("SubmissionDate has error", true, deferredSubmission.SubmissionDateInfo.HasNotifications());
					AssertEquals("PaymentMethod no error", false, deferredSubmission.PaymentMethodInfo.HasNotifications());
					AssertEquals("DeferredAccount no error", false, deferredSubmission.DeferredAccountInfo.HasNotifications());
					deferredSubmission.SubmissionDate = ZDate.Today;
					UnitTestUserNotification.Instance.ClearMessages();
					submitButton.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Submits", DialogResult.OK, form.DialogResult);
					AssertEquals("SubmissionDate no error", false, deferredSubmission.SubmissionDateInfo.HasNotifications());
					AssertEquals("PaymentMethod no error", false, deferredSubmission.PaymentMethodInfo.HasNotifications());
					AssertEquals("DeferredAccount no error", false, deferredSubmission.DeferredAccountInfo.HasNotifications());
					deferredSubmission.PaymentMethod = "";
					UnitTestUserNotification.Instance.ClearMessages();
					form.DialogResult = DialogResult.None;
					submitButton.PerformClick();
					AssertMultilineASCIIEquals("LastMessage", "There are errors that need to be corrected before this Declaration can be Submitted.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Does not submit", DialogResult.None, form.DialogResult);
					AssertEquals("PaymentMethod has error", true, deferredSubmission.PaymentMethodInfo.HasNotifications());
					AssertEquals("SubmissionDate no error", false, deferredSubmission.SubmissionDateInfo.HasNotifications());
					AssertEquals("DeferredAccount no error", false, deferredSubmission.DeferredAccountInfo.HasNotifications());
					deferredSubmission.PaymentMethod = "D";
					UnitTestUserNotification.Instance.ClearMessages();
					submitButton.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Submits", DialogResult.OK, form.DialogResult);
					AssertEquals("SubmissionDate no error", false, deferredSubmission.SubmissionDateInfo.HasNotifications());
					AssertEquals("PaymentMethod no error", false, deferredSubmission.PaymentMethodInfo.HasNotifications());
					AssertEquals("DeferredAccount no error", false, deferredSubmission.DeferredAccountInfo.HasNotifications());
					deferredSubmission.DeferredAccount = "9999999999";
					UnitTestUserNotification.Instance.ClearMessages();
					form.DialogResult = DialogResult.None;
					submitButton.PerformClick();
					AssertMultilineASCIIEquals("LastMessage", "There are errors that need to be corrected before this Declaration can be Submitted.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Does not submit", DialogResult.None, form.DialogResult);
					AssertEquals("DeferredAccount has error", true, deferredSubmission.DeferredAccountInfo.HasNotifications());
					AssertEquals("SubmissionDate no error", false, deferredSubmission.SubmissionDateInfo.HasNotifications());
					AssertEquals("PaymentMethod no error", false, deferredSubmission.PaymentMethodInfo.HasNotifications());
				}
			}
		}
	}
}
