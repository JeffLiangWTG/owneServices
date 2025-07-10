namespace Enterprise.MarketingManager.GUI
{
	partial class TouchSetupControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			this.findRecipientContactsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.transistionRulesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.transitionRulesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.dataSourceLabel = new Enterprise.ZArchitecture.ZLabel();
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.findRecipientContactsGroupBox.SuspendLayout();
			this.transistionRulesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.transitionRulesGrid)).BeginInit();
			this.transitionRulesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.GlbCompanyCampaign);
			// 
			// findRecipientContactsGroupBox
			// 
			this.findRecipientContactsGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("f3a66688-cc1c-4c2e-ac9b-b7687177f76d", "Find Recipient Contacts");
			this.findRecipientContactsGroupBox.Controls.Add(this.transistionRulesGroupBox);
			this.findRecipientContactsGroupBox.Controls.Add(this.transitionRulesGrid);
			this.findRecipientContactsGroupBox.Controls.Add(this.dataSourceLabel);
			this.findRecipientContactsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.findRecipientContactsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.findRecipientContactsGroupBox.Name = "findRecipientContactsGroupBox";
			this.findRecipientContactsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(453, 599, true);
			this.findRecipientContactsGroupBox.TabIndex = 1;
			this.findRecipientContactsGroupBox.TabStop = false;
			// 
			// transistionRulesGroupBox
			// 
			this.transistionRulesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.transistionRulesGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("3a4dea9a-cd07-41b3-a609-3ee5b4d74ba2", "Transitioning Rules");
			this.transistionRulesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 120, true);
			this.transistionRulesGroupBox.Name = "transistionRulesGroupBox";
			this.transistionRulesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(445, 469, true);
			this.transistionRulesGroupBox.TabIndex = 0;
			this.transistionRulesGroupBox.TabStop = false;
			// 
			// transitionRulesGrid
			// 
			this.transitionRulesGrid.AllowNavigation = false;
			this.transitionRulesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.transitionRulesGrid, "TransitionRulesToThisCampaign");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).TransitionRulesToThisCampaign)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignDripMarketing)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).TransitionRulesToThisCampaign)).SyncRoot)).ParentHorizontalIdForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignDripMarketing)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).TransitionRulesToThisCampaign)).SyncRoot)).GCD_G0_ParentTouch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignDripMarketing)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).TransitionRulesToThisCampaign)).SyncRoot)).ParentTouchSummary)));
			this.transitionRulesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("9AD2DB4C-E478-4B01-9FE6-5AB3CF51E2E2", "Parent Horizontal Id");
			zDropEditColumnStyleInfo1.ColumnName = "ParentHorizontalIdForBinding";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("DE55AC67-EA8E-4E48-9F85-6AADFADD5194", "Campaign Summary");
			zDropEditColumnStyleInfo2.ColumnName = "ParentTouchSummary";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("82E80DC2-B30F-41F8-956E-29182825ABA6", "Find Parent Touch");
			zGuidDropEditColumnStyleInfo1.ColumnName = "GCD_G0_ParentTouch";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.transitionRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.transitionRulesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.transitionRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.transitionRulesGrid.GridId = "47efa7ff-982b-4ceb-8919-53848c81e82a";
			this.transitionRulesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.transitionRulesGrid.LayoutKey = "transitionRulesGrid";
			this.transitionRulesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 20, true);
			this.transitionRulesGrid.Name = "transitionRulesGrid";
			this.transitionRulesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 87, true);
			this.transitionRulesGrid.TabIndex = 3;
			this.transitionRulesGrid.AfterBind += new System.EventHandler(this.TouchSetupTransitionRulesGrid_AfterBind);
			// 
			// dataSourceLabel
			// 
			this.dataSourceLabel.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("071a0ea8-84a3-41cd-b779-0cd97c1db51e", "Data Source:");
			this.dataSourceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 20, true);
			this.dataSourceLabel.Name = "dataSourceLabel";
			this.dataSourceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 24, true);
			this.dataSourceLabel.TabIndex = 19;
			this.dataSourceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.findRecipientContactsGroupBox);
			this.mainSplitContainer.Panel1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 4, 0, true);
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1050, 599, true);
			this.mainSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(433);
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, 0, 0, 0, true);
			this.mainSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(457);
			this.mainSplitContainer.SplitterWidth = 6;
			this.mainSplitContainer.TabIndex = 6;
			// 
			// TouchSetupControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.mainSplitContainer);
			this.Name = "TouchSetupControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1050, 599, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.findRecipientContactsGroupBox.ResumeLayout(false);
			this.findRecipientContactsGroupBox.PerformLayout();
			this.transistionRulesGroupBox.ResumeLayout(false);
			this.transistionRulesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.transitionRulesGrid)).EndInit();
			this.transitionRulesGrid.ResumeLayout(false);
			this.transitionRulesGrid.PerformLayout();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
			this.mainSplitContainer.ResumeLayout(false);
			this.mainSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox findRecipientContactsGroupBox;
		private ZArchitecture.GUI.ZGroupBox transistionRulesGroupBox;
		private ZArchitecture.ZGrid transitionRulesGrid;
		private ZArchitecture.ZLabel dataSourceLabel;
		private CargoWise.Windows.UI.KSplitContainer mainSplitContainer;
	}
}
