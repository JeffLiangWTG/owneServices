namespace Enterprise.MasterFiles.GUI
{
	partial class TriggerConditionValueLookupForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ConfirmButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ParametersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DescriptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ParametersGrid)).BeginInit();
			this.DescriptionGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 274, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.TriggerConditionValueParameters);
			// 
			// ConfirmButton
			// 
			this.ConfirmButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.ConfirmButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8fc826f2-4d34-42bb-aed5-172682368514", "OK");
			this.ConfirmButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 240, true);
			this.ConfirmButton.Name = "ConfirmButton";
			this.ConfirmButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 28, true);
			this.ConfirmButton.TabIndex = 2;
			this.ConfirmButton.UseVisualStyleBackColor = true;
			this.ConfirmButton.Click += new System.EventHandler(this.ConfirmButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.CloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e074d193-7a51-4158-8540-92398ef3ceb8", "Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 240, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 28, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// ParametersGrid
			// 
			this.ParametersGrid.AllowNavigation = false;
			this.ParametersGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ParametersGrid, "ParameterCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TriggerConditionValueParameters)(null)).ParameterCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TriggerConditionValueParameter)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TriggerConditionValueParameters)(null)).ParameterCollection)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TriggerConditionValueParameter)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TriggerConditionValueParameters)(null)).ParameterCollection)).SyncRoot)).ParamValue)));
			this.ParametersGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("cc04f994-71cb-4612-9e2d-bd7edf5e3cde", "Parameter");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ee332a94-98b2-4eec-909e-6e3d9ee6c9de", "Value");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo1.ColumnName = "ParamValue";
			this.ParametersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ParametersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ParametersGrid.CopySelectedRowsAllowed = true;
			this.ParametersGrid.GridId = "4ef057ba-7414-4d75-9bd7-77e6c631ccc9";
			this.ParametersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ParametersGrid.LayoutKey = "zGrid1";
			this.ParametersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 78, true);
			this.ParametersGrid.Name = "ParametersGrid";
			this.ParametersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 156, true);
			this.ParametersGrid.TabIndex = 1;
			// 
			// DescriptionGroupBox
			// 
			this.DescriptionGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("277006c5-6159-43fb-b127-4ff969561591", "Description");
			this.DescriptionGroupBox.Controls.Add(this.DescriptionLabel);
			this.DescriptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 4, true);
			this.DescriptionGroupBox.Name = "DescriptionGroupBox";
			this.DescriptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 68, true);
			this.DescriptionGroupBox.TabIndex = 4;
			this.DescriptionGroupBox.TabStop = false;
			// 
			// DescriptionLabel
			// 
			this.DescriptionLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("37c03b40-185b-48ff-9014-e6316da100bf", "Choose an applicable parameter code from the list (or use your own at your discretion) and add value.");
			this.DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.DescriptionLabel.Name = "DescriptionLabel";
			this.DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(326, 49, true);
			this.DescriptionLabel.TabIndex = 0;
			// 
			// TriggerConditionValueLookupForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("147a17b6-82b7-49d5-919f-028987e8d67a", "Parameters Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 298, true);
			this.Controls.Add(this.DescriptionGroupBox);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.ConfirmButton);
			this.Controls.Add(this.ParametersGrid);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.TriggerConditionValueParameters);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "TriggerConditionValueLookupForm";
			this.Controls.SetChildIndex(this.ParametersGrid, 0);
			this.Controls.SetChildIndex(this.ConfirmButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.DescriptionGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ParametersGrid)).EndInit();
			this.DescriptionGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.ZGrid ParametersGrid;
		private ZArchitecture.GUI.ZButton ConfirmButton;
		private ZArchitecture.GUI.ZButton CloseButton;
		private ZArchitecture.GUI.ZGroupBox DescriptionGroupBox;
		private ZArchitecture.ZLabel DescriptionLabel;
	}
}