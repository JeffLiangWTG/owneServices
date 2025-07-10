using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(AccreditationUpdaterForm))]
	public class AccreditationUpdaterFormTest : ZFormBasherTest
	{
		public void TestSyncSingleCode()
		{
			var accreditation1 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation1.HAC_Code = "AC1";
			var accreditation2 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation2.HAC_Code = "AC2";
			Factory.Save();
			var person = Factory.New<GlbPerson>();
			person.PER_FullName = "full name";
			var attempt1 = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt1.HAA_HAC = accreditation1.PK;
			attempt1.HAA_PER = person.PK;
			var attempt2 = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt2.HAA_HAC = accreditation2.PK;
			attempt2.HAA_PER = person.PK;
			var updater = new AccreditationUpdaterForPerson()
			{ IsFirstExamType = true, CompletionToleranceDays = 10 };
			updater.SetPersons(new[] { person });
			using (var form = new AccreditationUpdaterFormForTest(updater))
			{
				form.Show();
				form.SetTargetAccreditation(accreditation1);
				form.RunUpdateButton_Exposed.PerformClick();
			}

			var factory = new BusinessObjectFactory();
			attempt1 = factory.Load<GlbAccreditationAttempt>(attempt1.PK);
			attempt2 = factory.Load<GlbAccreditationAttempt>(attempt2.PK);
			AssertNull(attempt1);
			AssertNotNull(attempt2);
		}

		public void TestRunUpdate()
		{
			var accreditation = Factory.NewWithValidTestData<GlbAccreditation>();
			Factory.Save();
			var attempt1 = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt1.HAA_HAC = accreditation.PK;
			var attempt2 = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt2.HAA_HAC = accreditation.PK;
			var updater = new AccreditationUpdaterForAccreditation(accreditation)
			{ IsFirstExamType = true, CompletionToleranceDays = 10 };
			using (var form = new AccreditationUpdaterFormForTest(updater))
			{
				form.Show();
				form.RunUpdateButton_Exposed.PerformClick();
			}

			var factory = new BusinessObjectFactory();
			attempt1 = factory.Load<GlbAccreditationAttempt>(attempt1.PK);
			attempt2 = factory.Load<GlbAccreditationAttempt>(attempt2.PK);
			AssertNull(attempt1);
			AssertNull(attempt2);
		}

		public void TestRunUpdateValidation()
		{
			var accreditation = Factory.NewWithValidTestData<GlbAccreditation>();
			Factory.Save();
			var attempt1 = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt1.HAA_HAC = accreditation.PK;
			var attempt2 = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt2.HAA_HAC = accreditation.PK;
			var updater = new AccreditationUpdaterForAccreditation(accreditation);
			using (var form = new AccreditationUpdaterFormForTest(updater))
			{
				form.Show();
				form.RunUpdateButton_Exposed.PerformClick();
				AssertEquals("The form's errors must be fixed before the update can be run", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			}

			AssertEquals(false, attempt1.IsDeleted);
			AssertEquals(false, attempt2.IsDeleted);
		}

		public void TestControlVisibility()
		{
			var accreditation = Factory.NewWithValidTestData<GlbAccreditation>();
			Factory.Save();
			var attempt1 = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt1.HAA_HAC = accreditation.PK;
			var attempt2 = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt2.HAA_HAC = accreditation.PK;
			var updater = new AccreditationUpdaterForAccreditation(accreditation)
			{ IsFirstExamType = true };
			using (var form = new AccreditationUpdaterFormForTest(updater))
			{
				form.Show();
				var control = form.Controls.Find("completionToleranceDaysNumericUpDown", true).FirstOrDefault();
				AssertEquals(true, control.Visible);
				control = form.Controls.Find("startDateEdit", true).FirstOrDefault();
				AssertEquals(false, control.Visible);
				control = form.Controls.Find("endDateEdit", true).FirstOrDefault();
				AssertEquals(false, control.Visible);
				control = form.Controls.Find("additionalToleranceDaysNumericUpDown", true).FirstOrDefault();
				AssertEquals(false, control.Visible);
				updater.IsTimePeriodType = true;
				control = form.Controls.Find("completionToleranceDaysNumericUpDown", true).FirstOrDefault();
				AssertEquals(false, control.Visible);
				control = form.Controls.Find("startDateEdit", true).FirstOrDefault();
				AssertEquals(true, control.Visible);
				control = form.Controls.Find("endDateEdit", true).FirstOrDefault();
				AssertEquals(true, control.Visible);
				control = form.Controls.Find("additionalToleranceDaysNumericUpDown", true).FirstOrDefault();
				AssertEquals(true, control.Visible);
			}
		}

		public class AccreditationUpdaterFormForTest : AccreditationUpdaterForm
		{
			public AccreditationUpdaterFormForTest(AccreditationUpdater updater) : base(updater)
			{
			}

			public ZButton RunUpdateButton_Exposed { get => RunUpdateButton; }

			public void SetTargetAccreditation(GlbAccreditation accreditation)
			{
				accredFindBox.CodeBox.Text = accreditation.HAC_Code;
			}
		}

		protected override Form GetFormToBashCore()
		{
			var accreditation = Factory.New<GlbAccreditation>();
			return new AccreditationUpdaterForm(new AccreditationUpdaterForAccreditation(accreditation));
		}
	}
}
