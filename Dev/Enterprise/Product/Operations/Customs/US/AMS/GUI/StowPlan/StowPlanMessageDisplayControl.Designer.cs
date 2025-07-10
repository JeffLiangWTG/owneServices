namespace Enterprise.Customs.US.AMS.GUI
{
	partial class StowPlanMessageDisplayControl
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
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PortMessageSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.PortGrid = new Enterprise.ZArchitecture.ZGrid();
			this.StowPlanMessagesUserControl = new Enterprise.Messaging.GUI.EDIMessageUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PortMessageSplitContainer)).BeginInit();
			this.PortMessageSplitContainer.Panel1.SuspendLayout();
			this.PortMessageSplitContainer.Panel2.SuspendLayout();
			this.PortMessageSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PortGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.AMS.Business.USVoyagePortCollection);
			// 
			// PortMessageSplitContainer
			// 
			this.PortMessageSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PortMessageSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PortMessageSplitContainer.Name = "PortMessageSplitContainer";
			this.PortMessageSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// PortMessageSplitContainer.Panel1
			// 
			this.PortMessageSplitContainer.Panel1.Controls.Add(this.PortGrid);
			// 
			// PortMessageSplitContainer.Panel2
			// 
			this.PortMessageSplitContainer.Panel2.Controls.Add(this.StowPlanMessagesUserControl);
			this.PortMessageSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 376, true);
			this.PortMessageSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.PortMessageSplitContainer.TabIndex = 0;
			// 
			// PortGrid
			// 
			this.PortGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PortGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.VoyagePort)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.VoyagePort)(null)).Port)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.VoyagePort)(null)).MessageStatusDescription)));
			this.PortGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("9c9223d4-3dc7-4cd9-ab8b-d40a93cb36fe", "Port");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "Port";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("111a5167-17e8-4da8-a9d7-e3d15a87b668", "Status");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "MessageStatusDescription";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.PortGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PortGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PortGrid.CopySelectedRowsAllowed = true;
			this.PortGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PortGrid.GridId = "984f3550-9cef-4dde-855c-354bfc61b573";
			this.PortGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PortGrid.LayoutKey = "PortGrid";
			this.PortGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PortGrid.Name = "PortGrid";
			this.PortGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 200, true);
			this.PortGrid.TabIndex = 0;
			// 
			// StowPlanMessagesUserControl
			// 
			this.StowPlanMessagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StowPlanMessagesUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(((Enterprise.Customs.US.AMS.Business.VoyagePort)(null)))));
			this.StowPlanMessagesUserControl.BindPrepend = "";
			this.StowPlanMessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StowPlanMessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StowPlanMessagesUserControl.Name = "StowPlanMessagesUserControl";
			this.StowPlanMessagesUserControl.ShowChangingBlueMessageHeading = false;
			this.StowPlanMessagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 172, true);
			this.StowPlanMessagesUserControl.TabIndex = 0;
			// 
			// StowPlanMessageDisplayControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.PortMessageSplitContainer);
			this.Name = "StowPlanMessageDisplayControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 376, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PortMessageSplitContainer.Panel1.ResumeLayout(false);
			this.PortMessageSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PortMessageSplitContainer)).EndInit();
			this.PortMessageSplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PortGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer PortMessageSplitContainer;
		private ZArchitecture.ZGrid PortGrid;
		private Enterprise.Messaging.GUI.EDIMessageUserControl StowPlanMessagesUserControl;
	}
}
