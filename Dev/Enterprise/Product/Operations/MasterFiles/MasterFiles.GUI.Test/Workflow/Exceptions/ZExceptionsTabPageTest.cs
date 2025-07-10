using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ZExceptionsTabPageTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestHostedExceptionsUserControl()
		{
			TabControl.TabPages.Add(ExceptionsTabPage);
			Form.Controls.Add(TabControl);
			Form.Show();
			Application.DoEvents();

			AssertEquals("Hosted control type", typeof(ZExceptionsUserControl), ExceptionsTabPage.Controls[0].GetType());
			AssertEquals("Hosted control DockStyle", DockStyle.Fill, ExceptionsTabPage.Controls[0].Dock);
		}

		[RequiresSTA]
		public void TestSetDataBindingCore_RemoveDurationRelatedFields_WhenNotIExceptionDurationSupporter()
		{
			TabControl.TabPages.Add(ExceptionsTabPage);
			Form.Controls.Add(TabControl);
			Form.Show();
			Application.DoEvents();

			var grid = form.Controls.Find("ExceptionsGrid", true)[0] as ZGrid;
			string[] durationRelatedFields = { ProcessTasksSchema.P9_RL_NKExceptionLocation.Name, ProcessTasksSchema.P9_ExceptionDurationHours.Name, ProcessTasksSchema.P9_ExceptionEndDate.Name };

			var columnsNames = grid.ColumnStyles.OfType<ZGridColumnInfo>().Select(c => c.ColumnName).ToList();

			foreach (var columnName in durationRelatedFields)
			{
				AssertEquals(false, columnsNames.Any(x => columnsNames.Contains(columnName)));
			}
		}

		[RequiresSTA]
		public void TestSetDataBindingCore_DoNotRemoveDurationRelatedFields_WhenIExceptionDurationSupporter()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var consol = Factory.New<Forwarding.IForwardingConsol>();
			var container = Factory.New<ICommonContainer>();

			TestCase(ControllerIDs.JobShipment, shipment);
			TestCase(ControllerIDs.JobConsol, consol);
			TestCase(ControllerIDs.Containers, container);

			void TestCase<TJob>(ControllerID controllerID, TJob routingJob)
			{
				using (var form = new ZForm(routingJob))
				using (var tabControl = new ZTemplateTabControl())
				using (var exceptionsTabPage = new ZExceptionsTabPage())
				{
					form.ControllerID = controllerID;
					tabControl.TabPages.Add(exceptionsTabPage);
					form.Controls.Add(tabControl);
					form.Show();
					Application.DoEvents();

					var grid = form.Controls.Find("ExceptionsGrid", true)[0] as ZGrid;
					string[] durationRelatedFields = { ProcessTasksSchema.P9_RL_NKExceptionLocation.Name, ProcessTasksSchema.P9_ExceptionDurationHours.Name, ProcessTasksSchema.P9_ExceptionEndDate.Name };

					var columnsNames = grid.ColumnStyles.OfType<ZGridColumnInfo>().Select(c => c.ColumnName).ToList();

					foreach (var columnName in durationRelatedFields)
					{
						AssertEquals(true, columnsNames.Any(x => columnsNames.Contains(columnName)));
					}
				}
			}
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

		ZExceptionsTabPage ExceptionsTabPage
		{
			get
			{
				if (exceptionsTabPage == null)
				{
					exceptionsTabPage = new ZExceptionsTabPage();
				}
				return exceptionsTabPage;
			}
		}
		ZExceptionsTabPage exceptionsTabPage;

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
			if (exceptionsTabPage != null)
			{
				exceptionsTabPage.Dispose();
			}
		}

		#endregion
	}
}
