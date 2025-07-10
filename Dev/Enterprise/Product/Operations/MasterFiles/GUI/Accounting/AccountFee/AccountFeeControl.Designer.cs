namespace Enterprise.MasterFiles.GUI
{
	partial class AccountFeeControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.GLAccountBoundFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AccountFeeRuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AmountCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.OverrideCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GLAccountBoundFindBox.SuspendLayout();
			this.AccountFeeRuleDropEdit.SuspendLayout();
			this.AmountCalcFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccountFeeSettings);
			// 
			// GLAccountBoundFindBox
			// 
			this.GLAccountBoundFindBox.AllowDrop = true;
			this.GLAccountBoundFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.GLAccountBoundFindBox, "AAF_AG_GLAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccountFeeSettings)(null)).AAF_AG_GLAccount)));
			this.GLAccountBoundFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ce0577c4-fbb5-4461-a15f-aeda6b007972", "GL Account");
			this.GLAccountBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 54, true);
			this.GLAccountBoundFindBox.Name = "GLAccountBoundFindBox";
			this.GLAccountBoundFindBox.PopupCaption = null;
			this.GLAccountBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 20, true);
			this.GLAccountBoundFindBox.TabIndex = 2;
			// 
			// AccountFeeRuleDropEdit
			// 
			this.AccountFeeRuleDropEdit.AllowDrop = true;
			this.AccountFeeRuleDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AccountFeeRuleDropEdit, "AAF_Rule");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccountFeeSettings)(null)).AAF_Rule)));
			this.AccountFeeRuleDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("eba66a4f-db0b-4ed7-b68e-7301d528d77e", "Account Fee Rule");
			this.AccountFeeRuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 28, true);
			this.AccountFeeRuleDropEdit.Name = "AccountFeeRuleDropEdit";
			this.AccountFeeRuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 20, true);
			this.AccountFeeRuleDropEdit.TabIndex = 1;
			// 
			// AmountCalcFindBox
			// 
			this.AmountCalcFindBox.AllowDrop = true;
			this.AmountCalcFindBox.BindToAmount = "AAF_FeeAmount";
			this.AmountCalcFindBox.BindToUnit = "AAF_RX_NKFeeCurrency";
			this.AmountCalcFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceiptForm|f4d05d1b-c3fb-480a-afb4-5a102e572791", "Flat Fee Amount");
			this.AmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.AmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 80, true);
			this.AmountCalcFindBox.Name = "AmountCalcFindBox";
			this.AmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.AmountCalcFindBox.TabIndex = 3;
			// 
			// OverrideCheckbox
			// 
			this.OverrideCheckbox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OverrideCheckbox, "OverrideSettings");
			this.SetDataSourceBinding(this.OverrideCheckbox, "Text", "OverrideText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccountFeeSettings)(null)).OverrideSettings)));
			this.OverrideCheckbox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8f77a644-04cf-450d-b2e4-50fd916ad689", "Override");
			this.OverrideCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideCheckbox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OverrideCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 4, true);
			this.OverrideCheckbox.Name = "OverrideCheckbox";
			this.OverrideCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 17, true);
			this.OverrideCheckbox.TabIndex = 0;
			this.OverrideCheckbox.UseVisualStyleBackColor = true;
			// 
			// AccountFeeControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("611bae99-8fda-4d74-a700-244c4c8c0aaf", "Account Fee Settings");
			this.Controls.Add(this.OverrideCheckbox);
			this.Controls.Add(this.AccountFeeRuleDropEdit);
			this.Controls.Add(this.AmountCalcFindBox);
			this.Controls.Add(this.GLAccountBoundFindBox);
			this.Name = "AccountFeeControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 3, 5, 3, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 109, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GLAccountBoundFindBox.ResumeLayout(true);
			this.GLAccountBoundFindBox.PerformLayout();
			this.AccountFeeRuleDropEdit.ResumeLayout(true);
			this.AccountFeeRuleDropEdit.PerformLayout();
			this.AmountCalcFindBox.ResumeLayout(true);
			this.AmountCalcFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGuidFindBox GLAccountBoundFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit AccountFeeRuleDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcFindBox AmountCalcFindBox;
		protected System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
		private ZArchitecture.GUI.ZCheckBox OverrideCheckbox;
	}
}
