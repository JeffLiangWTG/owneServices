using CargoWise.Common;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ProcessQueueUserControl : ZUserControl
	{
		public ProcessQueueUserControl()
		{
			InitializeComponent();
			currentQueueUserControl = GetCurrentQueueUserControl();
			CurrentQueueGroupBox.Controls.Add(currentQueueUserControl);
			currentQueueUserControl.AlignControlsIntoSingleColumn = false;
			currentQueueUserControl.BindToPrefix = "ActiveProcessQueueForBinding.";
			currentQueueUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != null)
			{
				IProcessQueueParent processQueueParent = dataSource as IProcessQueueParent;
				if (processQueueParent == null)
				{
					ErrorReporter.ReportOnce(
						GetType().FullName + "DataSourceMustImplementInterface",
						"The top-level data source for this control must implement " + typeof(IProcessQueueParent).FullName);
				}
				else
				{
					AssignHistoryGridColumnNames(processQueueParent);
					base.SetDataBinding(dataSource, dataMember);
				}
			}
			else
			{
				base.SetDataBinding(dataSource, dataMember);
			}
		}

		protected virtual CurrentQueueUserControl GetCurrentQueueUserControl()
		{
			return new CurrentQueueUserControl();
		}

		void AssignHistoryGridColumnNames(IProcessQueueParent parent)
		{
			if (parent.ActiveProcessQueueForBinding.Count > 0)
			{
				ActiveProcessQueue activeProcessQueue = parent.ActiveProcessQueueForBinding[0];

				ZGridColumnInfo queueColumnInfo = QueueHistoryGrid.GetColumnStyle(ProcessQueueLog.Schema.Queue);
				queueColumnInfo.Caption = activeProcessQueue.QueueNameCaption;

				ZGridColumnInfo statusColumnInfo = QueueHistoryGrid.GetColumnStyle(ProcessQueueLog.Schema.Status);
				statusColumnInfo.Caption = activeProcessQueue.StatusCaption;

				ZGridColumnInfo subStatusColumnInfo = QueueHistoryGrid.GetColumnStyle(ProcessQueueLog.Schema.SubStatus);
				subStatusColumnInfo.Caption = activeProcessQueue.SubStatusCaption;

				ZGridColumnInfo reasonColumnInfo = QueueHistoryGrid.GetColumnStyle(ProcessQueueLog.Schema.Reason);
				reasonColumnInfo.Caption = activeProcessQueue.ReasonCaption;

				ZGridColumnInfo assignedToColumnInfo = QueueHistoryGrid.GetColumnStyle(ProcessQueueLog.Schema.AssignedTo);
				assignedToColumnInfo.Caption = activeProcessQueue.AssignedToCaption;
			}
		}
	}
}
