using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class CartageZoneDistanceControl
	{
		private ZCheckBox ACIZoneCheckBox;
		private CartageCalculatorPanel CartagePanel;
		private ZDropEdit EquipmentDropEdit;
		private ZDropEdit ConvFactorDropEdit;
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			this.EquipmentDropEdit = new ZDropEditWithFixedWidth();
			this.ConvFactorDropEdit = new ZDropEditWithFixedWidth();
			this.CartagePanel = new CartageCalculatorPanel();
			this.ACIZoneCheckBox = new ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = DataSourceTypeForBinding;
			// 
			// EquipmentDropEdit
			// 
			this.EquipmentDropEdit.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.EquipmentDropEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CartageZoneDistanceControl|e3aac9bd-9e85-4b86-bc56-83c8bd5babd2", "Drop Mode");
			this.EquipmentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 103, true);
			this.EquipmentDropEdit.Name = "EquipmentDropEdit";
			this.EquipmentDropEdit.PreBoundMaxLength = 3;
			this.EquipmentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.EquipmentDropEdit.TabIndex = 4;
			// 
			// ConvFactorDropEdit
			// 
			this.ConvFactorDropEdit.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.ConvFactorDropEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CartageZoneDistanceControl|2383c1d6-4d95-4bcf-b087-ad9305bec3ef", "Conv. Factor");
			this.ConvFactorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 79, true);
			this.ConvFactorDropEdit.Name = "ConvFactorDropEdit";
			this.ConvFactorDropEdit.PreBoundMaxLength = 5;
			this.ConvFactorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			this.ConvFactorDropEdit.TabIndex = 2;
			// 
			// CartagePanel
			// 
			this.CartagePanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
						| System.Windows.Forms.AnchorStyles.Left
						| System.Windows.Forms.AnchorStyles.Right;
			this.CartagePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CartagePanel.Name = "CartagePanel";
			this.CartagePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 73, true);
			this.CartagePanel.TabIndex = 0;
			// 
			// ACIZoneCheckBox
			// 
			this.ACIZoneCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			this.ACIZoneCheckBox.AutoSize = true;
			this.ACIZoneCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CartageZoneDistanceControl|d1e50553-34f8-45dd-bd64-9d6a6b9abf96", "US/Canada ACI Zones");
			this.ACIZoneCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ACIZoneCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 106, true);
			this.ACIZoneCheckBox.Name = "ACIZoneCheckBox";
			this.ACIZoneCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ACIZoneCheckBox.TabIndex = 5;
			this.ACIZoneCheckBox.UseVisualStyleBackColor = true;
			// 
			// CartageZoneDistanceControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ACIZoneCheckBox);
			this.Controls.Add(this.CartagePanel);
			this.Controls.Add(this.ConvFactorDropEdit);
			this.Controls.Add(this.EquipmentDropEdit);
			this.Name = "CartageZoneDistanceControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
