namespace Enterprise.MasterFiles.GUI
{
	public partial class MatchingTemplateForm
	{
		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.HeadingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TasksGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TasksGrid)).BeginInit();
			this.TasksGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 320, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 5;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(292);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.MatchingTemplateView);
			// 
			// HeadingLabel
			// 
			this.HeadingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.HeadingLabel, "Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MatchingTemplateView)(null)).Description)));
			this.HeadingLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.HeadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 8, true);
			this.HeadingLabel.Name = "HeadingLabel";
			this.HeadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(627, 36, true);
			this.HeadingLabel.TabIndex = 0;
			// 
			// TasksGrid
			// 
			this.TasksGrid.AllowNavigation = false;
			this.TasksGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TasksGrid, "MatchingTemplates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MatchingTemplateView)(null)).MatchingTemplates)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.MatchingTemplateViewLine)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MatchingTemplateView)(null)).MatchingTemplates)).SyncRoot)).MatchOrder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MatchingTemplateViewLine)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MatchingTemplateView)(null)).MatchingTemplates)).SyncRoot)).TemplateName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MatchingTemplateViewLine)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MatchingTemplateView)(null)).MatchingTemplates)).SyncRoot)).TemplateDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MatchingTemplateViewLine)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MatchingTemplateView)(null)).MatchingTemplates)).SyncRoot)).TemplateFallback)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MatchingTemplateViewLine)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MatchingTemplateView)(null)).MatchingTemplates)).SyncRoot)).MatchDescription)));
			this.TasksGrid.CaptionVisible = false;
			this.TasksGrid.ColorContextKey = "SuspendedTasksGrid";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "MatchOrder";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "TemplateName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(112);
			zTextBoxColumnStyleInfo2.ColumnName = "TemplateDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(144);
			zTextBoxColumnStyleInfo3.ColumnName = "TemplateFallback";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "MatchDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(480);
			this.TasksGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.TasksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TasksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TasksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TasksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.TasksGrid.GridId = "c90a8ba2-58b0-4ff7-9bbc-3469c58a475f";
			this.TasksGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TasksGrid.IsWholeRowSelectedOnClick = true;
			this.TasksGrid.LayoutKey = "zGrid1";
			this.TasksGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 46, true);
			this.TasksGrid.Name = "TasksGrid";
			this.TasksGrid.ReadOnly = true;
			this.TasksGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.TasksGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 270, true);
			this.TasksGrid.TabIndex = 1;
			this.TasksGrid.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TasksGrid_MouseDoubleClick);
			// 
			// MatchingTemplateForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MatchingTemplateForm|07dfe1ce-5ace-4d78-a106-336378c47d41", "Workflow Template Matches");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 344, true);
			this.Controls.Add(this.TasksGrid);
			this.Controls.Add(this.HeadingLabel);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.MatchingTemplateView);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.MatchingTemplateView";
			this.MinimizeBox = false;
			this.Name = "MatchingTemplateForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.HeadingLabel, 0);
			this.Controls.SetChildIndex(this.TasksGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TasksGrid)).EndInit();
			this.TasksGrid.ResumeLayout(false);
			this.TasksGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid TasksGrid;
		private Enterprise.ZArchitecture.ZLabel HeadingLabel;
	}
}
