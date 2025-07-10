using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ViewStmNumsEditorForm))]
	sealed class ViewStmNumsEditorFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var stmNum = Factory.New<StaffViewStmNums>();
			return new ViewStmNumsEditorForm(stmNum);
		}

		#endregion
	}
}
