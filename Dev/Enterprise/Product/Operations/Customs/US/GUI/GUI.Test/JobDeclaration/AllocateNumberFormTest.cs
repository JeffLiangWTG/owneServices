using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(AllocateNumberForm))]
	sealed class AllocateNumberFormTest : ZFormBasherTest
	{
		public void TestOKButtonClick()
		{
			using (var form = new AllocateNumberForm(AllocateNumber))
			{
				form.Show();
				form.OKButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
				form.Show();
				allocateNumber.AE_Number = "CRAP";
				form.OKButton.PerformClick();
				AssertEquals("Error There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals(DialogResult.None, form.DialogResult);
			}
		}

		public void TestSetCaptions()
		{
			using (var form = new AllocateNumberForm(AllocateNumber))
			{
				AssertEquals("Allocate Entry Number", form.FormCaption);
				AssertEquals("&Allocate Entry Number", form.OKButton.Text);
				AssertEquals("Entry Number", form.AllocateNumberGroupBox.Text);
				form.SetCaptions(Enterprise.Customs.Common.CusEntryNumberTypes.UnitedStates.Protest);
				AssertEquals("Allocate CBP Assigned Number", form.FormCaption);
				AssertEquals("Allocate CBP Number", form.OKButton.Text);
				AssertEquals("CBP Assigned Number", form.AllocateNumberGroupBox.Text);
				AssertEquals("Enter the number, click 'Allocate CBP Number' and CBP Number will be allocated for this job.", form.DescriptionLabel.Text);
			}
		}

		protected override Form GetFormToBashCore() => new AllocateNumberForm(AllocateNumber);

		AllocateNumber allocateNumber;
		AllocateNumber AllocateNumber
		{
			get
			{
				if (allocateNumber == null)
				{
					var args = new AllocateNumberArgs();
					args.MaxLength = 8;
					args.Branch = GlbBranch.CurrentBranch;
					args.EntryFilerCode = "XXX";
					args.Factory = Factory;
					args.ValidateNumber = ((info, validator) => validator.ValidateFormalEntryNumber(info));
					allocateNumber = new AllocateNumber(args);
				}

				return allocateNumber;
			}
		}
	}
}
