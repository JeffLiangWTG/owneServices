namespace Enterprise.MasterFiles.GUI
{
	partial class MaximumAllowedTransactionAmountControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.GroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MaximumAllowedHeaderAmountEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MaximumAllowedLineAmountEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.MaximumAllowedTransactionAmount);
			// 
			// GroupBox
			// 
			this.GroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MaximumAllowedTransactionAmountControl|a45b95b1-b21f-47dd-975b-af37fdf260f1", "Settings");
			this.GroupBox.Controls.Add(this.MaximumAllowedHeaderAmountEdit);
			this.GroupBox.Controls.Add(this.MaximumAllowedLineAmountEdit);
			this.GroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GroupBox.Name = "GroupBox";
			this.GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 130, true);
			this.GroupBox.TabIndex = 1;
			this.GroupBox.TabStop = false;
			// 
			// HeaderMaximumAllowedAmountEdit
			// 
			this.BindingSource.SetBindingMember(this.MaximumAllowedHeaderAmountEdit, "MaximumAllowedHeaderAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.MaximumAllowedTransactionAmount)(null)).MaximumAllowedHeaderAmount)));
			this.MaximumAllowedHeaderAmountEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MaximumAllowedTransactionAmountControl|ef68a5e9-7114-44e7-b9e5-af225655a159", "Maximum Allowed Header Amount");
			this.MaximumAllowedHeaderAmountEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(206, 28, true);
			this.MaximumAllowedHeaderAmountEdit.Name = "HeaderMaximumAllowedAmountEdit";
			this.MaximumAllowedHeaderAmountEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.MaximumAllowedHeaderAmountEdit.TabIndex = 2;
			this.MaximumAllowedHeaderAmountEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LineMaximumAllowedAmountEdit
			// 
			this.BindingSource.SetBindingMember(this.MaximumAllowedLineAmountEdit, "MaximumAllowedLineAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.MaximumAllowedTransactionAmount)(null)).MaximumAllowedLineAmount)));
			this.MaximumAllowedLineAmountEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MaximumAllowedTransactionAmountControl|998a1783-c12c-44f0-bcc6-072e4b75568c", "Maximum Allowed Line Amount");
			this.MaximumAllowedLineAmountEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(206, 67, true);
			this.MaximumAllowedLineAmountEdit.Name = "LineMaximumAllowedAmountEdit";
			this.MaximumAllowedLineAmountEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.MaximumAllowedLineAmountEdit.TabIndex = 3;
			this.MaximumAllowedLineAmountEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MaximumAllowedTransactionAmountControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GroupBox);
			this.Name = "MaximumAllowedTransactionAmountControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 133, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GroupBox.ResumeLayout(false);
			this.GroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		ZArchitecture.GUI.ZGroupBox GroupBox;
		ZArchitecture.ZCalcEdit MaximumAllowedHeaderAmountEdit;
		ZArchitecture.ZCalcEdit MaximumAllowedLineAmountEdit;

		#endregion
	}
}
