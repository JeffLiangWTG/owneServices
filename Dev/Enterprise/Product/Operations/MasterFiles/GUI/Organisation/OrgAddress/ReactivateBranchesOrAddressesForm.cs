using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ReactivateBranchesOrAddressesForm : ZChildForm
	{
		readonly ReactivateBranchOrAddressModel reactivateModel;

		public ReactivateBranchesOrAddressesForm(ReactivateBranchOrAddressModel model) : base(model)
		{
			reactivateModel = model;
			InitializeComponent();

			HeaderLabel.Text = model.ForAddress ? Res.GetString("4A28E6C8-BB93-4586-881D-7D4B92ECCD3E", "Addresses To Reactivate") : Res.GetString("1E9A25D5-2629-490A-BD81-959C432C93D6", "Branches To Reactivate");
			FormCaption = model.ForAddress ? Res.GetString("EDE0D784-030A-46C3-9C22-96C05F60FDDC", "Reactivate Addresses") : Res.GetString("1C0DD13F-110A-4F28-9C86-BCCA56FCDA02", "Reactivate Branches");
			InitGrid();
			SetCheckBoxText();
			reactivateModel.HasSelectedItemsInfo.ValueChanged += HasSelectedItemsInfo_ValueChanged;
			SetActivateButtonEnabledStatus();
		}

		protected ZUserControl ReactivateGrid { get; private set; }

		void InitGrid()
		{
			if (reactivateModel.ForAddress)
			{
				ReactivateGrid = new ReactivateAddressesGrid();
			}
			else
			{
				ReactivateGrid = new ReactivateBranchesGrid();
			}

			ReactivateGrid.SuspendLayout();
			ReactivateGrid.AllowDrop = true;
			ReactivateGrid.AutoSize = true;
			BindingSource.SetBindingMember(ReactivateGrid, "ReactivateBranchOrAddressCollection");
			ReactivateGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			ReactivateGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 33, true);
			ReactivateGrid.Name = "ReactivateGrid";
			ReactivateGrid.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			ReactivateGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(535, 277, true);
			ReactivateGrid.TabIndex = 2;
			FormLayout.Controls.Add(ReactivateGrid, 0, 1);
			ReactivateGrid.ResumeLayout(true);
			ReactivateGrid.PerformLayout();
		}

		public override string FormCaption { get; }
		public override string FormVerb => string.Empty;

		void ActivateButton_Click(object sender, EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.OK;
			reactivateModel.ApplyActiveStatus();
			Close();
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void SelectUnselectAllCheckBox_CheckStateChanged(object sender, EventArgs e)
		{
			SetCheckBoxText();
		}

		void SetCheckBoxText()
		{
			SelectUnselectAllCheckBox.Text = reactivateModel.SelectedAll ? Res.GetString("400C9812-FD0E-41CE-8B6A-0A29ACEDE416", "Deselect All") : Res.GetString("AA3B601B-9649-44CB-9EC6-F69AF8E0909B", "Selected All");
		}

		void HasSelectedItemsInfo_ValueChanged(object sender, EventArgs e)
		{
			SetActivateButtonEnabledStatus();
		}

		void SetActivateButtonEnabledStatus()
		{
			ActivateButton.Enabled = reactivateModel.HasSelectedItems;
		}
	}
}
