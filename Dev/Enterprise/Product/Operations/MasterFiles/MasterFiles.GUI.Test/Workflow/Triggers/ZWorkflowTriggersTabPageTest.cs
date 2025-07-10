using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ZWorkflowTriggersTabPageTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestHostedUserControl()
		{
			TabControl.TabPages.Add(WorkflowTriggersTabPage);
			Form.Controls.Add(TabControl);
			Form.Show();
			Application.DoEvents();

			AssertEquals("Hosted control type", typeof(ZWorkflowTriggersUserControl), WorkflowTriggersTabPage.Controls[0].GetType());
			AssertEquals("Hosted control DockStyle", DockStyle.Fill, WorkflowTriggersTabPage.Controls[0].Dock);
		}

		#region Implementation

		ZForm Form
		{
			get
			{
				if (form == null)
				{
					form = new ZForm(Dummy);
					form.ControllerID = DummyControllerIDs.Dummy;
				}
				return form;
			}
		}
		ZForm form;

		ZTemplateTabControl TabControl
		{
			get
			{
				if (tabControl == null)
				{
					tabControl = new ZTemplateTabControl();
				}
				return tabControl;
			}
		}
		ZTemplateTabControl tabControl;

		ZWorkflowTriggersTabPage WorkflowTriggersTabPage
		{
			get
			{
				if (workflowTriggersTabPage == null)
				{
					workflowTriggersTabPage = new ZWorkflowTriggersTabPage();
				}
				return workflowTriggersTabPage;
			}
		}
		ZWorkflowTriggersTabPage workflowTriggersTabPage;

		DummyWithWorkflow Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<DummyWithWorkflow>();
				}
				return dummy;
			}
		}
		DummyWithWorkflow dummy;

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
			if (tabControl != null)
			{
				tabControl.Dispose();
			}
			if (workflowTriggersTabPage != null)
			{
				workflowTriggersTabPage.Dispose();
			}
		}

		#endregion
	}
}
