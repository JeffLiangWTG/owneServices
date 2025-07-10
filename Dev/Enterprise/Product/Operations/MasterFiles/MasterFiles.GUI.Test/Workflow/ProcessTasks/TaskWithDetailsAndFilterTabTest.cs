using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public class TaskWithDetailsAndFilterTabTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestHostedFilterControl()
		{
			TabControl.TabPages.Add(TabPage);
			Form.Controls.Add(TabControl);
			Form.Show();
			Application.DoEvents();

			AssertEquals("Hosted user control type", GetTypeOfFilterControl(), FilterControl.GetType());
			AssertEquals("Hosted user control DockStyle", DockStyle.Fill, FilterControl.Dock);
		}

		[RequiresSTA]
		public void TestCreateTasksFromTemplateLinkVisible()
		{
			TabControl.TabPages.Add(TabPage);
			Form.Controls.Add(TabControl);
			Form.Show();
			Application.DoEvents();

			TabPage.CreateTasksFromTemplateLinkVisible = true;
			AssertEquals(true, FilterControl.CreateTasksFromTemplateLinkVisible);
			TabPage.CreateTasksFromTemplateLinkVisible = false;
			AssertEquals(false, FilterControl.CreateTasksFromTemplateLinkVisible);
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestCreateTasksFromTemplateLinkVisible_NoDisposableLeakAfterSetting()
		{
			using (TaskWithDetailsAndFilterControl control = new TaskWithDetailsAndFilterControl())
			{
				control.CreateTasksFromTemplateLinkVisible = true;
			}
		}

		protected virtual Type GetTypeOfFilterControl()
		{
			return typeof(TaskWithDetailsAndFilterControl);
		}

		#region Implementation

		ZChildForm Form
		{
			get
			{
				if (form == null)
				{
					form = CreateForm();
					form.ControllerID = DummyControllerIDs.Dummy;
				}
				return form;
			}
		}
		ZChildForm form;

		protected virtual ZChildForm CreateForm()
		{
			return new ZChildForm(Factory.New<DummyWithWorkflow>());
		}

		ZTabControl TabControl
		{
			get
			{
				if (tabControl == null)
				{
					tabControl = new ZTabControl();
				}
				return tabControl;
			}
		}
		ZTabControl tabControl;

		TaskWithDetailsAndFilterTab TabPage
		{
			get
			{
				if (tabPage == null)
				{
					tabPage = GetNewTaskWithDetailsAndFilterTab();
				}
				return tabPage;
			}
		}
		TaskWithDetailsAndFilterTab tabPage;

		protected virtual TaskWithDetailsAndFilterTab GetNewTaskWithDetailsAndFilterTab()
		{
			return new TaskWithDetailsAndFilterTab();
		}

		TaskWithDetailsAndFilterControl FilterControl
		{
			get { return TabPage.Controls.Count == 0 ? null : (TaskWithDetailsAndFilterControl)TabPage.Controls[0]; }
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (tabPage != null)
			{
				tabPage.Dispose();
			}
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
