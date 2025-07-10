using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ZChildForm))]
	sealed class ZExceptionsUserControlTest : ZMilestonesOrExceptionsUserControlTest
	{
		[RequiresSTA]
		public void TestNavigateToWorkflowItem()
		{
			using (ZForm form = new ZForm(Dummy))
			using (ZExceptionsUserControl control = new ZExceptionsUserControl())
			{
				form.ControllerID = DummyControllerIDs.Dummy;
				control.SetDataBinding(Dummy.WorkflowItems.Exceptions, "");
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				ProcessTask task1 = Dummy.WorkflowItems.Exceptions.AddNew();
				ProcessTask task2 = Dummy.WorkflowItems.Exceptions.AddNew();

				PropertyInfo gridProperty = control.GetType().GetProperty("Grid", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
				ZGrid grid = (ZGrid)gridProperty.GetValue(control, null);

				control.NavigateToWorkflowItem(task2);
				AssertEquals(task2, grid.ListManager.GetCurrent());

				control.NavigateToWorkflowItem(task1);
				AssertEquals(task1, grid.ListManager.GetCurrent());
			}
		}

		[RequiresSTA]
		public void TestGridExceptionsUserControl()
		{
			using (ZForm form = new ZForm(Dummy))
			using (ZExceptionsUserControl control = new ZExceptionsUserControl())
			{
				form.ControllerID = DummyControllerIDs.Dummy;
				control.SetDataBinding(Dummy.WorkflowItems.Exceptions, "");
				form.Controls.Add(control);

				var grid = form.Controls.Find("ExceptionsGrid", true)[0] as ZGrid;
				var columns = grid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>();

				AssertContainsExactElementsInExactOrder("grid Column Names",
				new[] { "P9_Description",
						"ExceptionTypeCategory",
						"ExceptionTypeCategoryDescription",
						"ExceptionTypeCode",
						"ExceptionTypeDescription",
						"IsExceptionActionedForBinding",
						"P9_GS_NKAssignedStaffMember",
						"P9_GG_AssignedGroup",
						"P9_ActualDateForBinding",
						"P9_ActualDateUtcForBinding",
						"P9_ActualDateLocalForBinding",
						"ExceptionCausePK",
						"ExceptionCauseDescription",
						"ExceptionResolutionPK",
						"ExceptionResolutionDescription",
						"CompletedTimeLocal",
						"ExceptionAssignedDate",
						"P9_CompletedTimeUtc",
						"P9_RL_NKExceptionLocation",
						"P9_ExceptionDurationHours",
						"P9_ExceptionEndDate",
						"P9_SystemCreateUser",
						"P9_SystemCreateTimeUtc",
						"P9_SystemLastEditUser",
						"P9_SystemLastEditTimeUtc",
				}, columns.Select(x => x.ColumnName));
			}
		}

		[RequiresSTA]
		public void TestExceptionsGrid_IsGridLayoutConfigurable()
		{
			using (var form = new ZForm(Dummy))
			using (var control = GetNewUserControl())
			{
				form.ControllerID = DummyControllerIDs.Dummy;
				control.SetDataBinding(Dummy.WorkflowItems.Exceptions, "");
				form.Controls.Add(control);

				var grid = form.Controls.Find("ExceptionsGrid", true)[0] as ZGrid;
				AssertEquals("This is required for saving customised column layouts", true, grid.IsGridLayoutConfigurable);
			}
		}

		protected override ZMilestonesOrExceptionsUserControl GetNewUserControl()
		{
			return new ZExceptionsUserControl();
		}

		protected override WorkflowItemCollectionIncludingRelatedView GetCollectionIncludingRelatedView(ProcessTaskCollection collection)
		{
			return collection.ExceptionsIncludingRelated;
		}

		protected override WorkflowItemCollectionView GetCollectionView(ProcessTaskCollection collection)
		{
			return collection.Exceptions;
		}
	}
}
