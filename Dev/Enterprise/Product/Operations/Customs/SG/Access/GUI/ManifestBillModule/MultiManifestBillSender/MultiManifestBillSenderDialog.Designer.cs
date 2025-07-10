using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.Access.GUI
{
	partial class MultiManifestBillSenderDialog
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			this.BottomButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ButtonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ShipmentTreeView = new Enterprise.ZArchitecture.GUI.ZTreeView();
			this.CycleFiledsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CycleNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CycleDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomButtonPanel.SuspendLayout();
			this.CycleFiledsPanel.SuspendLayout();
			this.CycleNumberDropEdit.SuspendLayout();
			this.CycleDateDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Dock = System.Windows.Forms.DockStyle.None;
			this.MainStatusBar.Enabled = false;
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 336, true);
			this.MainStatusBar.ShowPanels = false;
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.SG.Access.GUI.MultiManifestBillSender);
			// 
			// BottomButtonPanel
			// 
			this.BottomButtonPanel.Controls.Add(this.ButtonCancel);
			this.BottomButtonPanel.Controls.Add(this.SendButton);
			this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 230, true);
			this.BottomButtonPanel.Name = "BottomButtonPanel";
			this.BottomButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 37, true);
			this.BottomButtonPanel.TabIndex = 2;
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.CaptionResourceString = Enterprise.Customs.SG.Access.GUI.Res.GetData("2a6793de-5c40-40de-952b-a0e5d99b1b42", "&Cancel");
			this.ButtonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 2, true);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 27, true);
			this.ButtonCancel.TabIndex = 2;
			this.ButtonCancel.ToolTipCaption = null;
			this.ButtonCancel.UseVisualStyleBackColor = true;
			this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
			// 
			// SendButton
			// 
			this.SendButton.CaptionResourceString = Enterprise.Customs.SG.Access.GUI.Res.GetData("9045795b-161b-45e2-83d5-eb949cb09ba6", "&Send");
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(306, 2, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 27, true);
			this.SendButton.TabIndex = 1;
			this.SendButton.ToolTipCaption = null;
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// ShipmentTreeView
			// 
			this.ShipmentTreeView.BackColor = System.Drawing.SystemColors.Control;
			this.ShipmentTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipmentTreeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
			this.ShipmentTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 34, true);
			this.ShipmentTreeView.Name = "ShipmentTreeView";
			this.ShipmentTreeView.ReadOnly = true;
			this.ShipmentTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 196, true);
			this.ShipmentTreeView.TabIndex = 1;
			this.ShipmentTreeView.TreeViewSearcher = null;
			// 
			// CycleFiledsPanel
			// 
			this.CycleFiledsPanel.Controls.Add(this.CycleNumberDropEdit);
			this.CycleFiledsPanel.Controls.Add(this.CycleDateDateEdit);
			this.CycleFiledsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.CycleFiledsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CycleFiledsPanel.Name = "CycleFiledsPanel";
			this.CycleFiledsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 34, true);
			this.CycleFiledsPanel.TabIndex = 0;
			// 
			// CycleNumberDropEdit
			// 
			this.CycleNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CycleNumberDropEdit, "CycleNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.Access.GUI.MultiManifestBillSender)(null)).CycleNumber)));
			this.CycleNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 7, true);
			this.CycleNumberDropEdit.Name = "CycleNumberDropEdit";
			this.CycleNumberDropEdit.ShowDescriptionBox = false;
			this.CycleNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.CycleNumberDropEdit.TabIndex = 1;
			// 
			// CycleDateDateEdit
			// 
			this.CycleDateDateEdit.AllowDrop = true;
			this.CycleDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.CycleDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CycleDateDateEdit, "CycleDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.Access.GUI.MultiManifestBillSender)(null)).CycleDate)));
			this.CycleDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 7, true);
			this.CycleDateDateEdit.Name = "CycleDateDateEdit";
			this.CycleDateDateEdit.TabIndex = 0;
			// 
			// MultiManifestBillSenderDialog
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.SG.Access.GUI.Res.GetData("9ca06176-e463-404f-9eda-4efeb4e05fe4", "Bills To Send");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 267, true);
			this.Controls.Add(this.ShipmentTreeView);
			this.Controls.Add(this.BottomButtonPanel);
			this.Controls.Add(this.CycleFiledsPanel);
			this.DataSourceType = typeof(Enterprise.Customs.SG.Access.GUI.MultiManifestBillSender);
			this.Name = "MultiManifestBillSenderDialog";
			this.Controls.SetChildIndex(this.CycleFiledsPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomButtonPanel, 0);
			this.Controls.SetChildIndex(this.ShipmentTreeView, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomButtonPanel.ResumeLayout(false);
			this.BottomButtonPanel.PerformLayout();
			this.CycleFiledsPanel.ResumeLayout(false);
			this.CycleFiledsPanel.PerformLayout();
			this.CycleNumberDropEdit.ResumeLayout(true);
			this.CycleNumberDropEdit.PerformLayout();
			this.CycleDateDateEdit.ResumeLayout(true);
			this.CycleDateDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZTreeView ShipmentTreeView;
		private Enterprise.ZArchitecture.GUI.ZPanel BottomButtonPanel;
		private Enterprise.ZArchitecture.GUI.ZButton ButtonCancel;
		private Enterprise.ZArchitecture.GUI.ZButton SendButton;
		private ZPanel CycleFiledsPanel;
		private ZDropEdit CycleNumberDropEdit;
		private ZDateEdit CycleDateDateEdit;
	}
}
