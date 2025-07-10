using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(NumberSettingForNonBranchSpecificForm))]
	sealed class NumberSettingForNonBranchSpecificFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			return new NumberSettingForNonBranchSpecificForm(InBondNumberSetting.New(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK)));
		}
	}
}
