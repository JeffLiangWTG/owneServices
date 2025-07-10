using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ProcessQueueUserControlTest : TestCaseWithDummy
	{
		public void TestHistoryGridColumnCaptionsSetToProcessQueueCaptionsOnBind()
		{
			FieldInfo gridField = typeof(ProcessQueueUserControl).GetField("QueueHistoryGrid", BindingFlags.NonPublic | BindingFlags.Instance);

			using (ZForm testForm = new ZForm(Dummy))
			using (ProcessQueueUserControl control = new ProcessQueueUserControl())
			{
				testForm.Controls.Add(control);
				((ICompositeControlBindingSourceProvider)testForm).BindingSource.SetBindingMember(control, "");
				testForm.Show();
				Application.DoEvents();

				ZGrid historyGrid = (ZGrid)gridField.GetValue(control);

				ZGridColumnInfo queueColumn = historyGrid.GetColumnStyle(ProcessQueueLog.Schema.Queue);
				ZGridColumnInfo statusColumn = historyGrid.GetColumnStyle(ProcessQueueLog.Schema.Status);
				ZGridColumnInfo subStatusColumn = historyGrid.GetColumnStyle(ProcessQueueLog.Schema.SubStatus);
				ZGridColumnInfo reasonColumn = historyGrid.GetColumnStyle(ProcessQueueLog.Schema.Reason);
				ZGridColumnInfo assignedToColumn = historyGrid.GetColumnStyle(ProcessQueueLog.Schema.AssignedTo);

				ProcessQueueParentForTest queueParent = new ProcessQueueParentForTest(Factory);
				ProcessQueue queue = Factory.New<ProcessQueue>();
				queueParent.ActiveProcessQueueForBinding.RemoveAll();
				queueParent.ActiveProcessQueueForBinding.Add(new ActiveProcessQueueWithTestCaption(queue));
				control.SetDataBinding(queueParent, "");
				AssertEquals("NEW NAME", queueColumn.Caption);
				AssertEquals("NEW STATUS", statusColumn.Caption);
				AssertEquals("NEW SUB-STATUS", subStatusColumn.Caption);
				AssertEquals("NEW REASON", reasonColumn.Caption);
				AssertEquals("NEW ASSIGNED TO", assignedToColumn.Caption);
			}
		}

		public void TestBind()
		{
			using (ProcessQueueUserControl userControl = new ProcessQueueUserControl())
			{
				AssertEquals("Pre-condition, there should not be any DeveloperError", ZString.Empty, ErrorReporter.LastMessageReported);
				userControl.SetDataBinding(Dummy, "");
				AssertEquals(userControl.GetType().FullName + "DataSourceMustImplementInterface", ErrorReporter.LastKeyReported);
				AssertEquals("The top-level data source for this control must implement " + typeof(IProcessQueueParent).FullName, ErrorReporter.LastMessageReported);
			}

			using (ProcessQueueUserControl userControl = new ProcessQueueUserControl())
			{
				ErrorReporter.Clear();
				userControl.SetDataBinding(new ProcessQueueParentForTest(Factory), "");
				AssertEquals("Should not report any developer errors", ZString.Empty, ErrorReporter.LastMessageReported);
			}
		}

		#region ActiveProcessQueueWithTestCaption

		class ActiveProcessQueueWithTestCaption : ActiveProcessQueue
		{
			public ActiveProcessQueueWithTestCaption(ProcessQueue queue) : base(queue)
			{
			}

			public override MultilingualString StatusCaption
			{
				get { return (NoResString)"NEW STATUS"; }
			}

			public override MultilingualString AssignedToCaption
			{
				get { return (NoResString)"NEW ASSIGNED TO"; }
			}

			public override MultilingualString QueueNameCaption
			{
				get { return (NoResString)"NEW NAME"; }
			}

			public override MultilingualString SubStatusCaption
			{
				get { return (NoResString)"NEW SUB-STATUS"; }
			}

			public override MultilingualString ReasonCaption
			{
				get { return (NoResString)"NEW REASON"; }
			}
		}

		#endregion
	}
}
