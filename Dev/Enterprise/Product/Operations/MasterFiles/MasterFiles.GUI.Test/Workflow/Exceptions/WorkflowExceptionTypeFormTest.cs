using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(WorkflowExceptionTypeForm))]
	class WorkflowExceptionTypeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var type = Factory.New<ProcessWorkflowExceptionType>();
			return new WorkflowExceptionTypeForm(type);
		}

		[RequiresSTA]
		public void TestDefaultExceptionDurationParams()
		{
			var type = Factory.New<ProcessWorkflowExceptionType>();

			using (var typeForm = new WorkflowExceptionTypeForm(type))
			{
				var groupBox = typeForm.Controls.Find("DurationGroupBox", true)[0] as ZGroupBox;
				AssertNotNull(groupBox);

				var defaultDurationHoursCalcEdit = typeForm.Controls.Find("DefaultDurationHoursCalcEdit", true)[0] as ZCalcEdit;
				AssertNotNull(defaultDurationHoursCalcEdit);

				var useStartEndToCalculateDuration = typeForm.Controls.Find("UseStartEndToCalculateDuration", true)[0] as ZCheckBox;
				AssertNotNull(useStartEndToCalculateDuration);
				AssertEquals(false, useStartEndToCalculateDuration.ReadOnly);
			}
		}
	}
}
