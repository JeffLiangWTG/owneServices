namespace Enterprise.Freight.Agency.GUI
{
	partial class PortMessageDisplayControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			originsGrid = new Enterprise.ZArchitecture.ZGrid();
			horizontalSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			portMessages = new Enterprise.Messaging.GUI.EDIMessageUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(originsGrid)).BeginInit();
			horizontalSplitContainer.Panel1.SuspendLayout();
			horizontalSplitContainer.Panel2.SuspendLayout();
			horizontalSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.PortMessageHostCollection);
			// 
			// originsGrid
			// 
			originsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(originsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.PortMessageHost)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortMessageHost)(null)).Port)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortMessageHost)(null)).Direction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Agency.Business.PortMessageHost)(null)).EstDate)));
			originsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("PortMessageDisplayControl|32f31c25-b67e-4c66-8735-508d860ea228", "Port", "Load / Discharge Port", "The port the messages relate to.");
			zTextBoxColumnStyleInfo1.ColumnName = "Port";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("PortMessageDisplayControl|c1c788d3-216a-4e25-a56f-02978cd1b208", "Direction", "LOAD = messages relating to goods being loaded at this port.\r\nDISCHARGE = messages relating to goods being discharged at this port.");
			zTextBoxColumnStyleInfo2.ColumnName = "Direction";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("PortMessageDisplayControl|1fd043af-4a7a-4d76-853b-0e6269cc5327", "Est. Date", "Estimated Date", "If the direction is load then this will be the ETD.\r\nIf the direction is discharge then this will be the ETA.\r\nThis is mostly intended to allow sorting by date.");
			zDateEditColumnStyleInfo1.ColumnName = "EstDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			originsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			originsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			originsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			originsGrid.GridId = "ed648020-4454-4f57-94b0-afe176b8bc34";
			originsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			originsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			originsGrid.IsWholeRowSelectedOnClick = true;
			originsGrid.LayoutKey = "originsGrid";
			originsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			originsGrid.Name = "originsGrid";
			originsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 200, true);
			originsGrid.TabIndex = 0;
			// 
			// horizontalSplitContainer
			// 
			horizontalSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			horizontalSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			horizontalSplitContainer.Name = "horizontalSplitContainer";
			horizontalSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// horizontalSplitContainer.Panel1
			// 
			horizontalSplitContainer.Panel1.Controls.Add(originsGrid);
			// 
			// horizontalSplitContainer.Panel2
			// 
			horizontalSplitContainer.Panel2.Controls.Add(portMessages);
			horizontalSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 424, true);
			horizontalSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			horizontalSplitContainer.TabIndex = 1;
			// 
			// portMessages
			// 
			this.BindingSource.SetBindingMember(portMessages, ".");
			portMessages.BindPrepend = "";
			portMessages.Dock = System.Windows.Forms.DockStyle.Fill;
			portMessages.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			portMessages.Name = "portMessages";
			portMessages.ShowChangingBlueMessageHeading = false;
			portMessages.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 220, true);
			portMessages.TabIndex = 0;
			// 
			// PortMessageDisplayControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(horizontalSplitContainer);
			this.Name = "PortMessageDisplayControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 424, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(originsGrid)).EndInit();
			horizontalSplitContainer.Panel1.ResumeLayout(false);
			horizontalSplitContainer.Panel2.ResumeLayout(false);
			horizontalSplitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.ZGrid originsGrid;
		CargoWise.Windows.UI.KSplitContainer horizontalSplitContainer;
		Enterprise.Messaging.GUI.EDIMessageUserControl portMessages;
	}
}
