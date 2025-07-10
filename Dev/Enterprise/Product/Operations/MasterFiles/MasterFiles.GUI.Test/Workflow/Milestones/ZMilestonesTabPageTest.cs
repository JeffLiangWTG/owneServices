using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ZMilestonesTabPageTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestHostedMilestonesUserControl()
		{
			TabControl.TabPages.Add(MilestonesTabPage);
			Form.Controls.Add(TabControl);
			Form.Show();
			Application.DoEvents();

			AssertEquals("Hosted control type", typeof(ZMilestonesUserControl), MilestonesTabPage.Controls[0].GetType());
			AssertEquals("Hosted control DockStyle", DockStyle.Fill, MilestonesTabPage.Controls[0].Dock);
		}

		[RequiresSTA]
		public void TestMilestonesUserControlBinding()
		{
			ProcessTaskTemplate processTaskTemplate = this.WorkflowTemplate;
			Factory.Save();

			TabControl.TabPages.Add(MilestonesTabPage);
			Form.Controls.Add(TabControl);
			Control otherControl = new TextBox();
			Form.Controls.Add(otherControl);
			Form.Show();
			Application.DoEvents();

			otherControl.Focus();

			//No milestones expected
			AssertEquals("Milestones should be empty", 0, Dummy.WorkflowItems.Milestones.Count);
			AssertEquals("MilestonesIncludingRelatedSortable should be empty", 0, Dummy.WorkflowItems.MilestonesIncludingRelatedSortable.Count);

			//Create milestone from link
			InvokeLinkLabelClick(MilestonesTabPage.MilestonesUserControl.CreateMilestonesLinkLabelForTestOnly);
			AssertEquals("One milestone should be created from the template", 1, Dummy.WorkflowItems.Milestones.Count);
			AssertEquals("One milestone should be created from the template", 1, Dummy.WorkflowItems.MilestonesIncludingRelatedSortable.Count);
			AssertSame("Milestones should be the same", Dummy.WorkflowItems.Milestones[0], Dummy.WorkflowItems.MilestonesIncludingRelatedSortable[0]);
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

		ZMilestonesTabPage MilestonesTabPage
		{
			get
			{
				if (milestonesTabPage == null)
				{
					milestonesTabPage = new ZMilestonesTabPage();
				}
				return milestonesTabPage;
			}
		}
		ZMilestonesTabPage milestonesTabPage;

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

		ProcessTaskTemplate WorkflowTemplate
		{
			get
			{
				if (workflowTemplate == null)
				{
					workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
					workflowTemplate.P0_ProcessType = "DUM";
					ProcessTask task = workflowTemplate.WorkflowItems.Tasks.AddNew();
					ProcessTask milestone = workflowTemplate.WorkflowItems.Milestones.AddNew();
				}
				return workflowTemplate;
			}
		}
		ProcessTaskTemplate workflowTemplate;

		void InvokeLinkLabelClick(LinkLabel linkLabel)
		{
			linkLabel.GetType().InvokeMember("OnLinkClicked", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, linkLabel, new object[] { new LinkLabelLinkClickedEventArgs(null) });
		}

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
			if (milestonesTabPage != null)
			{
				milestonesTabPage.Dispose();
			}
		}

		#endregion
	}
}
