using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CurrentQueueUserControl : ZUserControl
	{
		public CurrentQueueUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != null && !hasBeenFirstBound)
			{
				SetBindToPrefixes((IBusiness)dataSource);
				hasBeenFirstBound = true;
			}
			base.SetDataBinding(dataSource, dataMember);
		}
		bool hasBeenFirstBound;

		public string BindToPrefix
		{
			get { return fBindToPrefix; }
			set { fBindToPrefix = value; }
		}

		public bool AlignControlsIntoSingleColumn
		{
			get { return fAlignControlsIntoSingleColumn; }
			set
			{
				fAlignControlsIntoSingleColumn = value;
				if (value)
				{
					AssignToLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 80);
					ReasonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 104);
					TaskAssignedToCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 80);
					ReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 104);
				}
				else
				{
					AssignToLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 8);
					ReasonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 32);
					TaskAssignedToCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 8);
					ReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 32);
				}
			}
		}

		protected virtual void SetBindToPrefixes(IBusiness dataSource)
		{
			if (!PrefixChecked && !string.IsNullOrEmpty(BindToPrefix))
			{
				QueueLabel.BindTo = BindToPrefix + QueueLabel.BindTo;
				QueueNameDropEdit.BindTo = BindToPrefix + QueueNameDropEdit.BindTo;

				StatusLabel.BindTo = BindToPrefix + StatusLabel.BindTo;
				StatusDropEdit.BindTo = BindToPrefix + StatusDropEdit.BindTo;

				SubStatusLabel.BindTo = BindToPrefix + SubStatusLabel.BindTo;
				SubStatusDropEdit.BindTo = BindToPrefix + SubStatusDropEdit.BindTo;

				AssignToLabel.BindTo = BindToPrefix + AssignToLabel.BindTo;
				TaskAssignedToCodeFindBox.BindTo = BindToPrefix + TaskAssignedToCodeFindBox.BindTo;

				ReasonLabel.BindTo = BindToPrefix + ReasonLabel.BindTo;
				ReasonTextBox.BindTo = BindToPrefix + ReasonTextBox.BindTo;

				PrefixChecked = true;
			}
		}

		string fBindToPrefix;
		bool fAlignControlsIntoSingleColumn;
		bool PrefixChecked;
	}
}
