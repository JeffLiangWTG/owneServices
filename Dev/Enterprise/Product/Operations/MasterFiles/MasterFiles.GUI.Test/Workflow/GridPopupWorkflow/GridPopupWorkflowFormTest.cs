using System;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GridPopupWorkflowForm))]
	sealed class GridPopupWorkflowFormTest : ZFormBasherTest
	{
		public void TestFormCaptionGetter_ShouldReference()
		{
			using (var form = (GridPopupWorkflowForm)GetFormToBash())
			{
				AssertEquals("Contains line's ConsigneeReference", true, form.FormCaption.IndexOf("OK", StringComparison.OrdinalIgnoreCase) != -1);
			}
		}

		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			var result = new GridPopupWorkflowForm(Factory.New<DummyWithWorkflow>(), "OK");
			result.ControllerID = DummyControllerIDs.Dummy;
			return result;
		}
	}
}
