using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(CustomsNumberViewStmNumsCompanyEditorForm))]
	sealed class CustomsNumberViewStmNumsBranchEditorFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(Factory);
			var stmNum = provider.NewCustomsNumber();
			stmNum.HasChanges = false;
			var wrapper = new CustomsNumberViewStmNumsCompanyWrapper(stmNum);
			wrapper.IsBranchLevel = true;
			wrapper.HasChanges = false;
			return new CustomsNumberViewStmNumsCompanyEditorForm(wrapper);
		}
	}
}
