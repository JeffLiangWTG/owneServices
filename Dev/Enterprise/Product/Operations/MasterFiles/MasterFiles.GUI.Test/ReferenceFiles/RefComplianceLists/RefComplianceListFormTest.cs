using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Testing
{
	[TestedType(typeof(RefComplianceListForm))]
	sealed class RefComplianceListFormTest : ZFormBasherTest
	{
		public void TestRefComplianceListUserControl()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var refComplianceListUserControl = (RefComplianceListUserControl)form.FindSingle<ZUserControl>("RefComplianceListUserControl");

				AssertNotNull(refComplianceListUserControl);
			}
		}

		public void TestWorkflowTabpage()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var tabControl = form.FindAll<ZTemplateTabControl>().Single();
				AssertNotEquals("WorkflowTabPage Visible", -1, tabControl.TabPages.IndexOf(tabControl.GetTabPage("WorkflowTabPage")));
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			dataBoundRefComplianceList = Factory.New<RefComplianceList>();
			var form = new RefComplianceListForm(dataBoundRefComplianceList);
			form.ControllerID = ControllerIDs.RefComplianceList;

			return form;
		}

		RefComplianceList dataBoundRefComplianceList;

		#endregion
	}
}
