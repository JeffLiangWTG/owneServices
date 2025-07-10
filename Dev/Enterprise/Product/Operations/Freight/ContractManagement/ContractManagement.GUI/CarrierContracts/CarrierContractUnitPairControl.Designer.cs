using CargoWiseOne.ResourceStrings;
using Enterprise.ContractManagement.Business;

namespace Enterprise.ContractManagement.GUI
{
	partial class CarrierContractUnitPairControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.MainLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TEUUnitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CNUnitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TEUTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CNTextBox = new Enterprise.ZArchitecture.ZTextBox();

			this.MainLabel.SuspendLayout();
			this.TEUUnitLabel.SuspendLayout();
			this.CNUnitLabel.SuspendLayout();
			this.TEUTextBox.SuspendLayout();
			this.CNTextBox.SuspendLayout();
			this.SuspendLayout();

			this.BindingSource.DataSourceType = typeof(CarrierContractQuantityUnitPair);
			//
			// MainLabel
			//
			this.MainLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.MainLabel.AutoSize = true;
			this.MainLabel.Name = "MainLabel";
			//
			// TEUUnitLabel
			//
			this.TEUUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 3, true);
			this.TEUUnitLabel.AutoSize = true;
			this.TEUUnitLabel.CaptionResourceString = Res.GetData("013a6b22-1fb2-5b9a-453f-eaebdc18eeb9", "TEU");
			this.TEUUnitLabel.Name = "TEUUnitLabel";
			//
			// CNUnitLabel
			//
			this.CNUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 3, true);
			this.CNUnitLabel.AutoSize = true;
			this.CNUnitLabel.CaptionResourceString = Res.GetData("26123f2f-8e5e-4b82-4551-ff60bd0decb8", "Containers");
			this.CNUnitLabel.Name = "CNUnitLabel";
			//
			// TEUTextBox
			//
			this.BindingSource.SetBindingMember(this.TEUTextBox, "TEUValue");
			this.TEUTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 0, true);
			this.TEUTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 0, true);
			this.TEUTextBox.Name = "TEUTextBox";
			this.TEUTextBox.ReadOnly = true;
			this.TEUTextBox.DecimalPlaces = 2;
			this.TEUTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TEUTextBox, false);
			//
			// CNTextBox
			//
			this.BindingSource.SetBindingMember(this.CNTextBox, "ContainerValue");
			this.CNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 0, true);
			this.CNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 0, true);
			this.CNTextBox.Name = "CNTextBox";
			this.CNTextBox.ReadOnly = true;
			this.CNTextBox.DecimalPlaces = 0;
			this.CNTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CNTextBox, false);
			this.Controls.Add(MainLabel);
			this.Controls.Add(TEUUnitLabel);
			this.Controls.Add(CNUnitLabel);
			this.Controls.Add(TEUTextBox);
			this.Controls.Add(CNTextBox);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 20, true);
			this.MainLabel.ResumeLayout();
			this.MainLabel.PerformLayout();
			this.TEUUnitLabel.ResumeLayout();
			this.TEUUnitLabel.PerformLayout();
			this.CNUnitLabel.ResumeLayout();
			this.CNUnitLabel.PerformLayout();
			this.TEUTextBox.ResumeLayout();
			this.TEUTextBox.PerformLayout();
			this.CNTextBox.ResumeLayout();
			this.CNTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		Enterprise.ZArchitecture.ZLabel MainLabel;
		Enterprise.ZArchitecture.ZTextBox TEUTextBox;
		Enterprise.ZArchitecture.ZTextBox CNTextBox;
		Enterprise.ZArchitecture.ZLabel TEUUnitLabel;
		Enterprise.ZArchitecture.ZLabel CNUnitLabel;
	}
}
