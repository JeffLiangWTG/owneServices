namespace Enterprise.Freight.GUI
{
	public partial class SplitForm
	{

		#region Designer generated code

		System.ComponentModel.IContainer components = null;
		new Enterprise.ZArchitecture.GUI.ZButton CancelButton;
		Enterprise.ZArchitecture.GUI.ZButton SplitButton;
		Enterprise.ZArchitecture.GUI.ZCalcDropEdit TotalWeightCalcDropEdit;
		Enterprise.ZArchitecture.GUI.ZCalcDropEdit TotalVolumeCalcDropEdit;
		Enterprise.ZArchitecture.GUI.ZCalcDropEdit SplitPacksCalcDropEdit;
		Enterprise.ZArchitecture.GUI.ZCalcDropEdit SplitWeightCalcDropEdit;
		Enterprise.ZArchitecture.GUI.ZCalcDropEdit SplitVolumeCalcDropEdit;
		Enterprise.ZArchitecture.ZLabel SplitTitleLabel;
		Enterprise.ZArchitecture.ZLabel TotalPackLineTitleLabel;
		Enterprise.ZArchitecture.GUI.ZCalcDropEdit TotalPacksCalcDropEdit;

		new void InitializeComponent()
		{
			this.TotalPacksCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.TotalWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.TotalVolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.SplitPacksCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.SplitWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.SplitVolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SplitButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SplitTitleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalPackLineTitleLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 139, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 22, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.PackLineSplitter);
			// 
			// TotalPacksCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalPacksCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLineSplitter)(null)).Line.JL_PackageCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLineSplitter)(null)).Line.JL_F3_NKPackType)));
			this.TotalPacksCalcDropEdit.BindToAmount = "Line.JL_PackageCount";
			this.TotalPacksCalcDropEdit.BindToUnit = "Line.JL_F3_NKPackType";
			this.TotalPacksCalcDropEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("SplitForm|53799ce7-d9ce-4b23-a965-480fb8e15a94", "Packs");
			this.TotalPacksCalcDropEdit.Decimals = 0;
			this.TotalPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 30, true);
			this.TotalPacksCalcDropEdit.Name = "TotalPacksCalcDropEdit";
			this.TotalPacksCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.TotalPacksCalcDropEdit.TabIndex = 1;
			this.TotalPacksCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// TotalWeightCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLineSplitter)(null)).Line.JL_ActualWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLineSplitter)(null)).Line.JL_ActualWeightUQ)));
			this.TotalWeightCalcDropEdit.BindToAmount = "Line.JL_ActualWeight";
			this.TotalWeightCalcDropEdit.BindToUnit = "Line.JL_ActualWeightUQ";
			this.TotalWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 30, true);
			this.TotalWeightCalcDropEdit.Name = "TotalWeightCalcDropEdit";
			this.TotalWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.TotalWeightCalcDropEdit.TabIndex = 2;
			this.TotalWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// TotalVolumeCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalVolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLineSplitter)(null)).Line.JL_ActualVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLineSplitter)(null)).Line.JL_ActualVolumeUQ)));
			this.TotalVolumeCalcDropEdit.BindToAmount = "Line.JL_ActualVolume";
			this.TotalVolumeCalcDropEdit.BindToUnit = "Line.JL_ActualVolumeUQ";
			this.TotalVolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 30, true);
			this.TotalVolumeCalcDropEdit.Name = "TotalVolumeCalcDropEdit";
			this.TotalVolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.TotalVolumeCalcDropEdit.TabIndex = 3;
			this.TotalVolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// SplitPacksCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.SplitPacksCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLineSplitter)(null)).SplitPackages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLineSplitter)(null)).Line.JL_F3_NKPackType)));
			this.SplitPacksCalcDropEdit.BindToAmount = "SplitPackages";
			this.SplitPacksCalcDropEdit.BindToUnit = "Line.JL_F3_NKPackType";
			this.SplitPacksCalcDropEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("SplitForm|024daaeb-154f-4a08-aad5-e19b89b193ac", "Packs");
			this.SplitPacksCalcDropEdit.Decimals = 0;
			this.SplitPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 74, true);
			this.SplitPacksCalcDropEdit.Name = "SplitPacksCalcDropEdit";
			this.SplitPacksCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.SplitPacksCalcDropEdit.TabIndex = 4;
			this.SplitPacksCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// SplitWeightCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.SplitWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLineSplitter)(null)).SplitWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLineSplitter)(null)).Line.JL_ActualWeightUQ)));
			this.SplitWeightCalcDropEdit.BindToAmount = "SplitWeight";
			this.SplitWeightCalcDropEdit.BindToUnit = "Line.JL_ActualWeightUQ";
			this.SplitWeightCalcDropEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("SplitForm|e6ed401f-4b48-438b-ba30-02810ff53c7d", "Weight");
			this.SplitWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 74, true);
			this.SplitWeightCalcDropEdit.Name = "SplitWeightCalcDropEdit";
			this.SplitWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.SplitWeightCalcDropEdit.TabIndex = 5;
			this.SplitWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// SplitVolumeCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.SplitVolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLineSplitter)(null)).SplitVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLineSplitter)(null)).Line.JL_ActualVolumeUQ)));
			this.SplitVolumeCalcDropEdit.BindToAmount = "SplitVolume";
			this.SplitVolumeCalcDropEdit.BindToUnit = "Line.JL_ActualVolumeUQ";
			this.SplitVolumeCalcDropEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("SplitForm|6de311b2-140e-4bc8-8e58-0d3f5961fd67", "Volume");
			this.SplitVolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 74, true);
			this.SplitVolumeCalcDropEdit.Name = "SplitVolumeCalcDropEdit";
			this.SplitVolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.SplitVolumeCalcDropEdit.TabIndex = 6;
			this.SplitVolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("SplitForm|fa8c0cf2-5234-4c4d-b5a9-fa4a80c73d2e", "Cancel");
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(456, 116, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelButton.TabIndex = 14;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// SplitButton
			// 
			this.SplitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SplitButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("SplitForm|9fe39b24-53e0-41a8-bfb7-52af4877bb7d", "Split");
			this.SplitButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 116, true);
			this.SplitButton.Name = "SplitButton";
			this.SplitButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.SplitButton.TabIndex = 13;
			this.SplitButton.Click += new System.EventHandler(this.SplitButton_Click);
			// 
			// SplitTitleLabel
			// 
			this.SplitTitleLabel.AutoSize = true;
			this.SplitTitleLabel.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("SplitForm|cbe2c453-1613-4e25-8f40-f7ff5e942f2a", "Amounts to be Split");
			this.SplitTitleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 52, true);
			this.SplitTitleLabel.Name = "SplitTitleLabel";
			this.SplitTitleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 13, true);
			this.SplitTitleLabel.TabIndex = 15;
			// 
			// TotalPackLineTitleLabel
			// 
			this.TotalPackLineTitleLabel.AutoSize = true;
			this.TotalPackLineTitleLabel.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("SplitForm|2cb1cd7a-b185-4935-b22b-162be82e06e3", "Total Packline Amounts");
			this.TotalPackLineTitleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.TotalPackLineTitleLabel.Name = "TotalPackLineTitleLabel";
			this.TotalPackLineTitleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 13, true);
			this.TotalPackLineTitleLabel.TabIndex = 16;
			// 
			// SplitForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 161, true);
			this.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("SplitForm|b6bccdc6-6c83-4ca3-b6ea-65f9c21299e7", "Split Packline");
			this.Controls.Add(this.TotalPackLineTitleLabel);
			this.Controls.Add(this.SplitTitleLabel);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.SplitButton);
			this.Controls.Add(this.SplitVolumeCalcDropEdit);
			this.Controls.Add(this.SplitWeightCalcDropEdit);
			this.Controls.Add(this.SplitPacksCalcDropEdit);
			this.Controls.Add(this.TotalVolumeCalcDropEdit);
			this.Controls.Add(this.TotalWeightCalcDropEdit);
			this.Controls.Add(this.TotalPacksCalcDropEdit);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(Enterprise.Freight.Business.PackLineSplitter);
			this.DataSourceTypeName = "Enterprise.Freight.Business.PackLineSplitter";
			this.Name = "SplitForm";
			this.Controls.SetChildIndex(this.TotalPacksCalcDropEdit, 0);
			this.Controls.SetChildIndex(this.TotalWeightCalcDropEdit, 0);
			this.Controls.SetChildIndex(this.TotalVolumeCalcDropEdit, 0);
			this.Controls.SetChildIndex(this.SplitPacksCalcDropEdit, 0);
			this.Controls.SetChildIndex(this.SplitWeightCalcDropEdit, 0);
			this.Controls.SetChildIndex(this.SplitVolumeCalcDropEdit, 0);
			this.Controls.SetChildIndex(this.SplitButton, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.SplitTitleLabel, 0);
			this.Controls.SetChildIndex(this.TotalPackLineTitleLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}
