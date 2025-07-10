using System.Windows.Forms;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	[TestedType(typeof(CustomsNumberViewStmNumsEditorForm))]
	sealed class CustomsNumberViewStmNumsEditorFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var provider = GlbCompany.CurrentCompany.CustomsNumberProvider;
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			stmNum.Provider = provider;
			var result = new CustomsNumberViewStmNumsEditorForm(new SGCustomsNumberViewStmNumsWrapper(stmNum));
			result.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 712);
			return result;
		}
	}
}
