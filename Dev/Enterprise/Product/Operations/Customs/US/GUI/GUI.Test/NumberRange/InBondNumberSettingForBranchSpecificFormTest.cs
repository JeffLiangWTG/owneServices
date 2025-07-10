using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(InBondNumberSettingForBranchSpecificForm))]
	sealed class InBondNumberSettingForBranchSpecificFormTest : ZFormBasherTest
	{
		public void TestSetNextNumberButton_Click()
		{
			using (InBondNumberSettingForBranchSpecificForm form = new InBondNumberSettingForBranchSpecificForm(InBondNumberSetting))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				long nextBranchNumber = (long)(InBondNumberSetting.CurrentNextNumber + 100);
				string warningMessage = NumberSettingForNonBranchSpecificForm.ChangingNumberCanCauseCustomsRejectDuplication(nextBranchNumber);
				InBondNumberSetting.NextNumber = nextBranchNumber;
				form.SetNextNumberButton.PerformClick();
				AssertEquals(warningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(nextBranchNumber, (long)InBondNumberSetting.CurrentNextNumber);
			}
		}

		protected override Form GetFormToBashCore() => new InBondNumberSettingForBranchSpecificForm(InBondNumberSetting);

		InBondNumberSetting entryNumberSetting;
		InBondNumberSetting InBondNumberSetting
		{
			get
			{
				DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
				return entryNumberSetting ?? (entryNumberSetting = InBondNumberSetting.New(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK)));
			}
		}
	}
}
