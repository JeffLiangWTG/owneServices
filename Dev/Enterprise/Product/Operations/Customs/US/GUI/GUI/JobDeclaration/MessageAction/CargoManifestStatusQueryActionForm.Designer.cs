
namespace Enterprise.Customs.US.GUI
{
	partial class CargoManifestStatusQueryActionForm
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
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.ActionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ActionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InbondLevelGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LevelsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ActionDropEdit.SuspendLayout();
			this.InbondLevelGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LevelsGrid)).BeginInit();
			this.LevelsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 238, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(292);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.CargoManifestStatusQueryHeaderObject);
			// 
			// ActionLabel
			// 
			this.ActionLabel.AutoSize = true;
			this.ActionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 23, true);
			this.ActionLabel.Name = "ActionLabel";
			this.ActionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 13, true);
			this.ActionLabel.TabIndex = 1;
			this.ActionLabel.Text = "Action:";
			// 
			// ActionDropEdit
			// 
			this.ActionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ActionDropEdit, "ActionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CargoManifestStatusQueryHeaderObject)(null)).ActionCode)));
			this.ActionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 19, true);
			this.ActionDropEdit.MaxItemsToShowInDropDown = 15;
			this.ActionDropEdit.Name = "ActionDropEdit";
			this.ActionDropEdit.PreBoundMaxLength = 3;
			this.ActionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 20, true);
			this.ActionDropEdit.TabIndex = 0;
			// 
			// InbondLevelGroupBox
			// 
			this.InbondLevelGroupBox.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.InbondLevelGroupBox.Controls.Add(this.LevelsGrid);
			this.InbondLevelGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 48, true);
			this.InbondLevelGroupBox.Name = "InbondLevelGroupBox";
			this.InbondLevelGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 149, true);
			this.InbondLevelGroupBox.TabIndex = 1;
			this.InbondLevelGroupBox.TabStop = false;
			this.InbondLevelGroupBox.Text = "Related Records to Send Messages For";
			// 
			// LevelsGrid
			// 
			this.LevelsGrid.AllowNavigation = false;
			this.LevelsGrid.BackColor = System.Drawing.SystemColors.WindowFrame;
			this.BindingSource.SetBindingMember(this.LevelsGrid, "SendingObjects");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.CargoManifestStatusQueryHeaderObject)(null)).SendingObjects)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CargoManifestQuerySendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CargoManifestStatusQueryHeaderObject)(null)).SendingObjects)).SyncRoot)).HumanFriendlyReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.CargoManifestQuerySendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CargoManifestStatusQueryHeaderObject)(null)).SendingObjects)).SyncRoot)).ShouldSendMessage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.CargoManifestQuerySendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CargoManifestStatusQueryHeaderObject)(null)).SendingObjects)).SyncRoot)).RequestForRelatedBOL)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.CargoManifestQuerySendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CargoManifestStatusQueryHeaderObject)(null)).SendingObjects)).SyncRoot)).UpdateEntryWithResults)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CargoManifestQuerySendingObject)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CargoManifestStatusQueryHeaderObject)(null)).SendingObjects)).SyncRoot)).LimitOutputOption)));
			this.LevelsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Reference";
			zTextBoxColumnStyleInfo1.ColumnName = "HumanFriendlyReference";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(245);
			zCheckBoxColumnStyleInfo1.Caption = "Send Message?";
			zCheckBoxColumnStyleInfo1.ColumnName = "ShouldSendMessage";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("72aa79e5-055d-4c55-87a3-962cded6d5cc", "Request For Related BOL");
			zCheckBoxColumnStyleInfo2.ColumnName = "RequestForRelatedBOL";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d80e76de-adfd-4ce2-b3bc-a9fac88c8552", "Update Entry With Results");
			zCheckBoxColumnStyleInfo3.ColumnName = "UpdateEntryWithResults";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9be16d28-1f3e-4bf3-a395-0b1ca0455d26", "Output Option");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zDropEditColumnStyleInfo1.ColumnName = "LimitOutputOption";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.LevelsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LevelsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.LevelsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.LevelsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.LevelsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.LevelsGrid.CopySelectedRowsAllowed = true;
			this.LevelsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LevelsGrid.GridId = "fce4ca82-015a-4460-b4e4-616275e8768b";
			this.LevelsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LevelsGrid.LayoutKey = "LevelsGrid";
			this.LevelsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LevelsGrid.Name = "LevelsGrid";
			this.LevelsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 130, true);
			this.LevelsGrid.TabIndex = 0;
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(611, 204, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SendButton.TabIndex = 2;
			this.SendButton.Text = "Send";
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(692, 204, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 3;
			this.CancelButton.Text = "Cancel";
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// CargoManifestStatusQueryActionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f330daf4-bfbd-42ce-aba9-4c5a5824094c", "Cargo Manifest Status Query");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 262, true);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.ActionLabel);
			this.Controls.Add(this.InbondLevelGroupBox);
			this.Controls.Add(this.ActionDropEdit);
			this.DataSourceAssemblyName = "Enterprise.Customs.US.Business";
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.CargoManifestStatusQueryHeaderObject);
			this.DataSourceTypeName = "Enterprise.Customs.US.Business.CargoManifestStatusQueryHeaderObject";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 290, true);
			this.Name = "CargoManifestStatusQueryActionForm";
			this.Text = "Cargo Manifest Status Query";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ActionDropEdit, 0);
			this.Controls.SetChildIndex(this.InbondLevelGroupBox, 0);
			this.Controls.SetChildIndex(this.ActionLabel, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ActionDropEdit.ResumeLayout(true);
			this.ActionDropEdit.PerformLayout();
			this.InbondLevelGroupBox.ResumeLayout(false);
			this.InbondLevelGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LevelsGrid)).EndInit();
			this.LevelsGrid.ResumeLayout(false);
			this.LevelsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel ActionLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ActionDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox InbondLevelGroupBox;
		internal Enterprise.ZArchitecture.ZGrid LevelsGrid;
		internal Enterprise.ZArchitecture.GUI.ZButton SendButton;
		private new Enterprise.ZArchitecture.GUI.ZButton CancelButton;
	}
}
