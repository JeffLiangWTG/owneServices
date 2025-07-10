namespace Enterprise.Customs.US.DataRegistry.GUI
{
	partial class SupervisorOverridesControl
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SupervisorOverridesControl));
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SupervisorOverridesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CommonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NominErrorsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NominErrorsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SupervisorOverridesPanel.SuspendLayout();
			this.CommonPanel.SuspendLayout();
			this.NominErrorsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NominErrorsGrid)).BeginInit();
			this.NominErrorsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.DataRegistry.Business.SupervisorOverrideData);
			// 
			// SupervisorOverridesPanel
			// 
			this.SupervisorOverridesPanel.Controls.Add(this.CommonPanel);
			this.SupervisorOverridesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupervisorOverridesPanel.Name = "SupervisorOverridesPanel";
			this.SupervisorOverridesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 376, true);
			this.SupervisorOverridesPanel.TabIndex = 0;
			// 
			// CommonPanel
			// 
			this.CommonPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CommonPanel.Controls.Add(this.NominErrorsGroupBox);
			this.CommonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CommonPanel.Name = "CommonPanel";
			this.CommonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 370, true);
			this.CommonPanel.TabIndex = 1;
			// 
			// NominErrorsGroupBox
			// 
			this.NominErrorsGroupBox.Controls.Add(this.NominErrorsGrid);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NominErrorsGroupBox, false);
			this.NominErrorsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.NominErrorsGroupBox.Name = "NominErrorsGroupBox";
			this.NominErrorsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 364, true);
			this.NominErrorsGroupBox.TabIndex = 0;
			this.NominErrorsGroupBox.TabStop = false;
			this.NominErrorsGroupBox.Text = resources.GetString("NominErrorsGroupBox.Text");
			// 
			// NominErrorsGrid
			// 
			this.NominErrorsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.NominErrorsGrid, "NominatedMessageErrors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.DataRegistry.Business.SupervisorOverrideData)(null)).NominatedMessageErrors)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DataRegistry.Business.NominatedMessageError)(((System.Collections.IList)(((Enterprise.Customs.US.DataRegistry.Business.SupervisorOverrideData)(null)).NominatedMessageErrors)).SyncRoot)).FieldName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DataRegistry.Business.NominatedMessageError)(((System.Collections.IList)(((Enterprise.Customs.US.DataRegistry.Business.SupervisorOverrideData)(null)).NominatedMessageErrors)).SyncRoot)).MessageErrorText)));
			this.NominErrorsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("b22c3119-93f1-47a3-9374-ffb3dc8fb1db", "Field Name");
			zTextBoxColumnStyleInfo1.ColumnName = "FieldName";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3db57657-69bf-48a5-9ec9-da3efef70d02", "Message Error Text");
			zTextBoxColumnStyleInfo2.ColumnName = "MessageErrorText";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.NominErrorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.NominErrorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.NominErrorsGrid.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.NominErrorsGrid.GridId = "107995d5-dd2a-44a2-b919-5604758c7f0c";
			this.NominErrorsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NominErrorsGrid.LayoutKey = "zGrid1";
			this.NominErrorsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 85, true);
			this.NominErrorsGrid.Name = "NominErrorsGrid";
			this.NominErrorsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 276, true);
			this.NominErrorsGrid.TabIndex = 0;
			// 
			// SupervisorOverridesControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupervisorOverridesPanel);
			this.Name = "SupervisorOverridesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 376, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupervisorOverridesPanel.ResumeLayout(false);
			this.SupervisorOverridesPanel.PerformLayout();
			this.CommonPanel.ResumeLayout(false);
			this.CommonPanel.PerformLayout();
			this.NominErrorsGroupBox.ResumeLayout(false);
			this.NominErrorsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.NominErrorsGrid)).EndInit();
			this.NominErrorsGrid.ResumeLayout(false);
			this.NominErrorsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel SupervisorOverridesPanel;
		private ZArchitecture.GUI.ZPanel CommonPanel;
		private ZArchitecture.GUI.ZGroupBox NominErrorsGroupBox;
		internal ZArchitecture.ZGrid NominErrorsGrid;
	}
}
