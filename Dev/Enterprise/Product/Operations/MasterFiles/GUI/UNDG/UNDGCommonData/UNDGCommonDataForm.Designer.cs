namespace Enterprise.MasterFiles.GUI
{
	public partial class UNDGCommonDataForm
	{
		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.SelectedTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_StateDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 285, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(766, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(897);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.UNDGCommonData);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(516, 258, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 25, true);
			this.PostingButtonsUserControl.TabIndex = 3;
			// 
			// SelectedTextBox
			// 
			this.SelectedTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SelectedTextBox, "DC_Descriptor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGCommonData)(null)).DC_Descriptor)));
			this.SelectedTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SelectedTextBox, false);
			this.SelectedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 38, true);
			this.SelectedTextBox.Multiline = true;
			this.SelectedTextBox.Name = "SelectedTextBox";
			this.SelectedTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.SelectedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(742, 166, true);
			this.SelectedTextBox.TabIndex = 0;
			// 
			// DG_StateDropDownEdit
			// 
			this.BindingSource.SetBindingMember(this.DG_StateDropDownEdit, "DC_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGCommonData)(null)).DC_Type)));
			this.DG_StateDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 12, true);
			this.DG_StateDropDownEdit.Name = "DG_StateDropDownEdit";
			this.DG_StateDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.DG_StateDropDownEdit.TabIndex = 15;
			// 
			// zDropEdit1
			// 
			this.BindingSource.SetBindingMember(this.zDropEdit1, "DC_Language");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGCommonData)(null)).DC_Language)));
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(497, 12, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 20, true);
			this.zDropEdit1.TabIndex = 16;
			// 
			// UNDGCommonDataForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(766, 309, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGCommonDataForm|79947461-5add-4330-a7a6-95aedcb8ce00", "Dangerous Goods Common Provision");
			this.Controls.Add(this.zDropEdit1);
			this.Controls.Add(this.DG_StateDropDownEdit);
			this.Controls.Add(this.SelectedTextBox);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.UNDGCommonData);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 345, true);
			this.Name = "UNDGCommonDataForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.SelectedTextBox, 0);
			this.Controls.SetChildIndex(this.DG_StateDropDownEdit, 0);
			this.Controls.SetChildIndex(this.zDropEdit1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		protected Enterprise.ZArchitecture.ZTextBox SelectedTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit DG_StateDropDownEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit1;
	}
}
