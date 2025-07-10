using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(CustomsNumberViewStmNumsCompanyEditorForm))]
	sealed class CustomsNumberViewStmNumsCompanyEditorFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(Factory);
			var stmNum = provider.NewCustomsNumber();
			stmNum.HasChanges = false;
			return new CustomsNumberViewStmNumsCompanyEditorForm(new CustomsNumberViewStmNumsCompanyWrapper(stmNum));
		}
	}
}
