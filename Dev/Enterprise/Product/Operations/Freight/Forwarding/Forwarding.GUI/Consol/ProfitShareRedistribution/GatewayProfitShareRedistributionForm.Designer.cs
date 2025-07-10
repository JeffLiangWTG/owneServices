using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	sealed partial class GatewayProfitShareRedistributionForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
            this.ActionButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.RedistributeProfitSharesButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.RedistributionLogButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.zPostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
            this.TopSpacePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
            this.RulesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ProfitShareRuleModuleButtonGrid = new Enterprise.Freight.Forwarding.GUI.ProfitShareRuleModuleButtonGrid();
            this.splitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
            this.ConsolsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ConsolModuleButtonGrid = new Enterprise.Freight.Forwarding.GUI.ProfitShareConsolWrapperModuleButtonGrid();
            this.ShipmentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ShipmentsGrid = new Enterprise.Freight.Forwarding.GUI.ProfitShareShipmentsGrid();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ActionButtonsPanel.SuspendLayout();
            this.zPostingButtonsUserControl.SuspendLayout();
            this.BottomPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.RulesGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ProfitShareRuleModuleButtonGrid.InnerGrid)).BeginInit();
            this.ProfitShareRuleModuleButtonGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.ConsolsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ConsolModuleButtonGrid.InnerGrid)).BeginInit();
            this.ConsolModuleButtonGrid.SuspendLayout();
            this.ShipmentsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ShipmentsGrid)).BeginInit();
            this.ShipmentsGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 29, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1120, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingProfitShareRedistribution);
            // 
            // ActionButtonsPanel
            // 
            this.ActionButtonsPanel.Controls.Add(this.RedistributeProfitSharesButton);
            this.ActionButtonsPanel.Controls.Add(this.RedistributionLogButton);
            this.ActionButtonsPanel.Controls.Add(this.zPostingButtonsUserControl);
            this.ActionButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ActionButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
            this.ActionButtonsPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1119, 28, true);
            this.ActionButtonsPanel.Name = "ActionButtonsPanel";
            this.ActionButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1120, 28, true);
            this.ActionButtonsPanel.TabIndex = 4;
            // 
            // RedistributeProfitSharesButton
            // 
            this.RedistributeProfitSharesButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("dc7487eb-08b2-4def-bf99-324efa5ea30e", "Redistribute && Share Profit", "Redistribute & Share Profit to listed shipments from selected Gateway Consols.");
            this.RedistributeProfitSharesButton.Enabled = false;
            this.RedistributeProfitSharesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 3, true);
            this.RedistributeProfitSharesButton.Name = "RedistributeProfitSharesButton";
            this.RedistributeProfitSharesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 23, true);
            this.RedistributeProfitSharesButton.TabIndex = 1;
            this.RedistributeProfitSharesButton.ToolTipCaption = null;
            this.RedistributeProfitSharesButton.UseVisualStyleBackColor = true;
            // 
            // RedistributionLogButton
            //
            this.RedistributionLogButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("81acc099-e7ff-4106-9f68-a6d4513ab043", "Redistribute Process Log");
            this.RedistributionLogButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(219, 3, true);
            this.RedistributionLogButton.Name = "RedistributionLogButton";
            this.RedistributionLogButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 23, true);
            this.RedistributionLogButton.TabIndex = 23;
            this.RedistributionLogButton.ToolTipCaption = null;
            this.RedistributionLogButton.UseVisualStyleBackColor = true;
            // 
            // zPostingButtonsUserControl
            // 
            this.zPostingButtonsUserControl.AllowDrop = true;
            this.zPostingButtonsUserControl.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.zPostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(803, 2, true);
            this.zPostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
            this.zPostingButtonsUserControl.Name = "zPostingButtonsUserControl";
            this.zPostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(309, 25, true);
            this.zPostingButtonsUserControl.TabIndex = 3;
            // 
            // TopSpacePanel
            // 
            this.TopSpacePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TopSpacePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.TopSpacePanel.Name = "TopSpacePanel";
            this.TopSpacePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1120, 5, true);
            this.TopSpacePanel.TabIndex = 0;
            // 
            // BottomPanel
            // 
            this.BottomPanel.Controls.Add(this.ActionButtonsPanel);
            this.BottomPanel.Controls.Add(this.MainStatusBar);
            this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 578, true);
            this.BottomPanel.Name = "BottomPanel";
            this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1120, 53, true);
            this.BottomPanel.TabIndex = 0;
            this.BottomPanel.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.BottomPanel.Controls.SetChildIndex(this.ActionButtonsPanel, 0);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 5, true);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.RulesGroupBox);
            this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1120, 573, true);
            this.splitContainer1.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(163);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(326);
            this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(163);
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 1;
            // 
            // RulesGroupBox
            // 
            this.RulesGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("12e1f78d-e507-46f2-afaa-6c4d7dfb2389", "Profit Share Setup");
            this.RulesGroupBox.Controls.Add(this.ProfitShareRuleModuleButtonGrid);
            this.RulesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RulesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.RulesGroupBox.Name = "RulesGroupBox";
            this.RulesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1120, 163, true);
            this.RulesGroupBox.TabIndex = 8;
            this.RulesGroupBox.TabStop = false;
			// 
			// ProfitShareRuleModuleButtonGrid
			//
			this.ProfitShareRuleModuleButtonGrid.AllowAttachDetachWithoutEditSecurity = true;
			this.ProfitShareRuleModuleButtonGrid.AllowDrop = true;
            this.ProfitShareRuleModuleButtonGrid.AlwaysRequiresSaveBeforeEdit = true;
            this.ProfitShareRuleModuleButtonGrid.AttachButtonText = Enterprise.Freight.Forwarding.GUI.Res.GetData("GatewayProfitShareRedistributionForm|ProfitShareRuleModuleButtonGrid|AttachButtonText", "Add...");
            this.BindingSource.SetBindingMember(this.ProfitShareRuleModuleButtonGrid, "ProfitShareRules");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingProfitShareRedistribution)(null)).ProfitShareRules)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingProfitShareRedistribution)(null)).OrgAgentRelationship_List)));
            this.ProfitShareRuleModuleButtonGrid.BindToFindBoxList = "OrgAgentRelationship_List";
            this.ProfitShareRuleModuleButtonGrid.DetachButtonText = Enterprise.Freight.Forwarding.GUI.Res.GetData("GatewayProfitShareRedistributionForm|ProfitShareRuleModuleButtonGrid|DetachButtonText", "Remove");
            this.ProfitShareRuleModuleButtonGrid.DetachMessage = null;
            this.ProfitShareRuleModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProfitShareRuleModuleButtonGrid.GridId = null;
            // 
            // 
            // 
            this.ProfitShareRuleModuleButtonGrid.InnerGrid.AllowNavigation = false;
            this.ProfitShareRuleModuleButtonGrid.InnerGrid.CaptionVisible = false;
            this.ProfitShareRuleModuleButtonGrid.InnerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProfitShareRuleModuleButtonGrid.InnerGrid.GridId = null;
            this.ProfitShareRuleModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.ProfitShareRuleModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
            this.ProfitShareRuleModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
            this.ProfitShareRuleModuleButtonGrid.InnerGrid.Name = "Grid";
            this.ProfitShareRuleModuleButtonGrid.InnerGrid.ReadOnly = true;
            this.ProfitShareRuleModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1114, 115, true);
            this.ProfitShareRuleModuleButtonGrid.InnerGrid.TabIndex = 0;
            this.ProfitShareRuleModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 11, true);
            this.ProfitShareRuleModuleButtonGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.ProfitShare;
            this.ProfitShareRuleModuleButtonGrid.Name = "ProfitShareRuleModuleButtonGrid";
            this.ProfitShareRuleModuleButtonGrid.NameOfAGridElement = Enterprise.Freight.Forwarding.GUI.Res.GetData("0da0ea23-858f-4832-a0f4-0b3b87ab80bd", "Profit Share Rules");
            this.ProfitShareRuleModuleButtonGrid.ReadOnly = true;
            this.ProfitShareRuleModuleButtonGrid.ShowEditButton = false;
            this.ProfitShareRuleModuleButtonGrid.ShowNewButton = false;
            this.ProfitShareRuleModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1117, 150, true);
            this.ProfitShareRuleModuleButtonGrid.TabIndex = 1;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.ConsolsGroupBox);
            this.splitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1120, 405, true);
            this.splitContainer2.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(163);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.ShipmentsGroupBox);
            this.splitContainer2.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(163);
            this.splitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(210);
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 1;
            // 
            // ConsolsGroupBox
            // 
            this.ConsolsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("a577d394-6943-4bd2-a437-950a38d81426", "G/W Consol for Profit Redistribution");
            this.ConsolsGroupBox.Controls.Add(this.ConsolModuleButtonGrid);
            this.ConsolsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConsolsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ConsolsGroupBox.Name = "ConsolsGroupBox";
            this.ConsolsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1120, 210, true);
            this.ConsolsGroupBox.TabIndex = 15;
            this.ConsolsGroupBox.TabStop = false;
            // 
            // ConsolModuleButtonGrid
            // 
            this.ConsolModuleButtonGrid.AllowDrop = true;
            this.ConsolModuleButtonGrid.AlwaysRequiresSaveBeforeEdit = true;
            this.ConsolModuleButtonGrid.AttachButtonText = Enterprise.Freight.Forwarding.GUI.Res.GetData("GatewayProfitShareRedistributionForm|ConsolModuleButtonGrid|AttachButtonText", "Add...");
            this.BindingSource.SetBindingMember(this.ConsolModuleButtonGrid, "Consols");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingProfitShareRedistribution)(null)).Consols)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingProfitShareRedistribution)(null)).Consols_List)));
            this.ConsolModuleButtonGrid.BindToFindBoxList = "Consols_List";
            this.ConsolModuleButtonGrid.DetachButtonText = Enterprise.Freight.Forwarding.GUI.Res.GetData("GatewayProfitShareRedistributionForm|ConsolModuleButtonGrid|DetachButtonText", "Remove");
            this.ConsolModuleButtonGrid.DetachMessage = null;
            this.ConsolModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConsolModuleButtonGrid.GridId = null;
            // 
            // 
            // 
            this.ConsolModuleButtonGrid.InnerGrid.AllowNavigation = false;
            this.ConsolModuleButtonGrid.InnerGrid.CaptionVisible = false;
            this.ConsolModuleButtonGrid.InnerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConsolModuleButtonGrid.InnerGrid.GridId = null;
            this.ConsolModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.ConsolModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
            this.ConsolModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
            this.ConsolModuleButtonGrid.InnerGrid.Name = "Grid";
            this.ConsolModuleButtonGrid.InnerGrid.ReadOnly = true;
            this.ConsolModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1114, 162, true);
            this.ConsolModuleButtonGrid.InnerGrid.TabIndex = 0;
            this.ConsolModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 11, true);
            this.ConsolModuleButtonGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.JobConsol;
            this.ConsolModuleButtonGrid.Name = "ConsolModuleButtonGrid";
            this.ConsolModuleButtonGrid.NameOfAGridElement = Enterprise.Freight.Forwarding.GUI.Res.GetData("4079d2aa-28ff-402a-b217-568d1ce115b3", "Consolidation");
            this.ConsolModuleButtonGrid.ReadOnly = true;
            this.ConsolModuleButtonGrid.ShowEditButton = false;
            this.ConsolModuleButtonGrid.ShowNewButton = false;
            this.ConsolModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1117, 197, true);
            this.ConsolModuleButtonGrid.TabIndex = 1;
            // 
            // ShipmentsGroupBox
            // 
            this.ShipmentsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("b55fe7e7-5d0b-4e9f-94cb-8b8e55758b7c", "G/W Consol Profit Redistributed to Shipments");
            this.ShipmentsGroupBox.Controls.Add(this.ShipmentsGrid);
            this.ShipmentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ShipmentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ShipmentsGroupBox.Name = "ShipmentsGroupBox";
            this.ShipmentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1120, 190, true);
            this.ShipmentsGroupBox.TabIndex = 17;
            this.ShipmentsGroupBox.TabStop = false;
            // 
            // ShipmentsGrid
            // 
            this.ShipmentsGrid.AllowDrop = true;
            this.ShipmentsGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.ShipmentsGrid, "Shipments");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingProfitShareRedistribution)(null)).Shipments)));
            this.ShipmentsGrid.CaptionVisible = false;
            this.ShipmentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ShipmentsGrid.GridId = "5df71d8b-a79e-4ddf-8816-cedd22fde9be";
            this.ShipmentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.ShipmentsGrid.LayoutKey = "Grid";
            this.ShipmentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 11, true);
            this.ShipmentsGrid.Name = "ShipmentsGrid";
            this.ShipmentsGrid.ReadOnly = true;
            this.ShipmentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1117, 178, true);
            this.ShipmentsGrid.TabIndex = 1;
            // 
            // GatewayProfitShareRedistributionForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1120, 631, true);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.TopSpacePanel);
            this.Controls.Add(this.BottomPanel);
            this.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingProfitShareRedistribution);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1133, 667, true);
            this.Name = "GatewayProfitShareRedistributionForm";
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ActionButtonsPanel.ResumeLayout(false);
            this.ActionButtonsPanel.PerformLayout();
            this.zPostingButtonsUserControl.ResumeLayout(true);
            this.zPostingButtonsUserControl.PerformLayout();
            this.BottomPanel.ResumeLayout(false);
            this.BottomPanel.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer1.PerformLayout();
            this.RulesGroupBox.ResumeLayout(false);
            this.RulesGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ProfitShareRuleModuleButtonGrid.InnerGrid)).EndInit();
            this.ProfitShareRuleModuleButtonGrid.ResumeLayout(true);
            this.ProfitShareRuleModuleButtonGrid.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer2.PerformLayout();
            this.ConsolsGroupBox.ResumeLayout(false);
            this.ConsolsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ConsolModuleButtonGrid.InnerGrid)).EndInit();
            this.ConsolModuleButtonGrid.ResumeLayout(true);
            this.ConsolModuleButtonGrid.PerformLayout();
            this.ShipmentsGroupBox.ResumeLayout(false);
            this.ShipmentsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ShipmentsGrid)).EndInit();
            this.ShipmentsGrid.ResumeLayout(false);
            this.ShipmentsGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		Enterprise.Core.Forms.ZPostingButtonsUserControl zPostingButtonsUserControl;
		Enterprise.ZArchitecture.GUI.ZPanel ActionButtonsPanel;
		Enterprise.ZArchitecture.GUI.ZButton RedistributeProfitSharesButton;
		Enterprise.ZArchitecture.GUI.ZButton RedistributionLogButton;
		ZPanel TopSpacePanel;
		ZPanel BottomPanel;
        CargoWise.Windows.UI.KSplitContainer splitContainer1;
        ZGroupBox RulesGroupBox;
#if DEBUG
		public
#endif
		ProfitShareRuleModuleButtonGrid ProfitShareRuleModuleButtonGrid;
		CargoWise.Windows.UI.KSplitContainer splitContainer2;
        ZGroupBox ConsolsGroupBox;
        ProfitShareConsolWrapperModuleButtonGrid ConsolModuleButtonGrid;
        ZGroupBox ShipmentsGroupBox;
        ProfitShareShipmentsGrid ShipmentsGrid;
    }
}
