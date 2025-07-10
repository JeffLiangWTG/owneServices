namespace Enterprise.Customs.US.GUI
{
	partial class DEAUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.RightPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DEAConstituentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DEAConstituentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DEAHeaderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DEAHeaderGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RightPanel.SuspendLayout();
			this.DEAConstituentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DEAConstituentsGrid)).BeginInit();
			this.DEAConstituentsGrid.SuspendLayout();
			this.DEAHeaderGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DEAHeaderGrid)).BeginInit();
			this.DEAHeaderGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).BeginInit();
			this.SplitContainer1.Panel1.SuspendLayout();
			this.SplitContainer1.Panel2.SuspendLayout();
			this.SplitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.DEAHeaderCollection);
			// 
			// RightPanel
			// 
			this.RightPanel.Controls.Add(this.DEAConstituentsGroupBox);
			this.RightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RightPanel.Name = "RightPanel";
			this.RightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 146, true);
			this.RightPanel.TabIndex = 1;
			// 
			// DEAConstituentsGroupBox
			// 
			this.DEAConstituentsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("7cb4de87-84bf-4fab-b157-82d770d7cc52", "Constituents");
			this.DEAConstituentsGroupBox.Controls.Add(this.DEAConstituentsGrid);
			this.DEAConstituentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DEAConstituentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DEAConstituentsGroupBox.Name = "DEAConstituentsGroupBox";
			this.DEAConstituentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 146, true);
			this.DEAConstituentsGroupBox.TabIndex = 1;
			this.DEAConstituentsGroupBox.TabStop = false;
			this.DEAConstituentsGroupBox.Text = "Constituents";
			// 
			// DEAConstituentsGrid
			// 
			this.DEAConstituentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DEAConstituentsGrid, "Constituents");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.DEAHeader)(null)).Constituents)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DEAConstituent)(((System.Collections.IList)(((Enterprise.Customs.US.Business.DEAHeader)(null)).Constituents)).SyncRoot)).US_ProductCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.DEAConstituent)(((System.Collections.IList)(((Enterprise.Customs.US.Business.DEAHeader)(null)).Constituents)).SyncRoot)).US_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DEAConstituent)(((System.Collections.IList)(((Enterprise.Customs.US.Business.DEAHeader)(null)).Constituents)).SyncRoot)).US_WeightUQ)));
			this.DEAConstituentsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Product Code";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "US_ProductCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Weight";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("fc2e4bc6-f558-4db8-8b64-97cacaf7c0dc", "Weight");
			zCalcEditColumnStyleInfo1.ColumnName = "US_Weight";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("af9812d4-2728-4642-ad9f-520aad425707", "Weight");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.Caption = "UQ";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a1a091e4-3e26-4349-9c12-cd5aae132738", "UQ");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "US_WeightUQ";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("af9812d4-2728-4642-ad9f-520aad425707", "Weight");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.DEAConstituentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DEAConstituentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DEAConstituentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.DEAConstituentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DEAConstituentsGrid.GridId = "55bd7997-b7e1-483b-ada2-532cd75f5390";
			this.DEAConstituentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DEAConstituentsGrid.LayoutKey = "DEAConstituentsGrid";
			this.DEAConstituentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DEAConstituentsGrid.Name = "DEAConstituentsGrid";
			this.DEAConstituentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 127, true);
			this.DEAConstituentsGrid.TabIndex = 0;
			// 
			// DEAHeaderGroupBox
			// 
			this.DEAHeaderGroupBox.Controls.Add(this.DEAHeaderGrid);
			this.DEAHeaderGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DEAHeaderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DEAHeaderGroupBox.Name = "DEAHeaderGroupBox";
			this.DEAHeaderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 150, true);
			this.DEAHeaderGroupBox.TabIndex = 1;
			this.DEAHeaderGroupBox.TabStop = false;
			this.DEAHeaderGroupBox.Text = "DEA";
			// 
			// DEAHeaderGrid
			// 
			this.DEAHeaderGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DEAHeaderGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.DEAHeader)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.DEAHeader)(null)).US_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DEAHeader)(null)).US_CountryOfShipment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DEAHeader)(null)).US_PermitNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DEAHeader)(null)).US_RegistrationNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DEAHeader)(null)).US_FormID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DEAHeader)(null)).US_TrackingStatusDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DEAHeader)(null)).Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DEAHeader)(null)).StatusDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.DEAHeader)(null)).StatusDate)));
			this.DEAHeaderGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Line No.";
			zCalcEditColumnStyleInfo2.ColumnName = "US_LineNo";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.Caption = "Ctry/Rgn. of Shipment";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "US_CountryOfShipment";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.Caption = "Permit Number";
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "US_PermitNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.Caption = "Registration Number";
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "US_RegistrationNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo2.Caption = "Form ID";
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "US_FormID";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.Caption = "Message Status";
			zTextBoxColumnStyleInfo4.ColumnName = "US_TrackingStatusDesc";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d2e42f7b-3bd1-43b9-a9fa-818ab0b1fc60", "PGA Line Status");
			zTextBoxColumnStyleInfo5.ColumnName = "Status";
			zTextBoxColumnStyleInfo5.GroupName = Enterprise.Customs.US.GUI.Res.GetData("009398bb-4c18-4947-b8af-d8758f97a7e9", "PGA Line Status");
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d31ada51-be15-49ff-b4f2-265ea83f9900", "PGA Line Status Desc.");
			zTextBoxColumnStyleInfo6.ColumnName = "StatusDesc";
			zTextBoxColumnStyleInfo6.GroupName = Enterprise.Customs.US.GUI.Res.GetData("009398bb-4c18-4947-b8af-d8758f97a7e9", "PGA Line Status");
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("bf674f21-0063-482f-8878-46085944eb68", "PGA Line Status Date");
			zDateEditColumnStyleInfo1.ColumnName = "StatusDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.DEAHeaderGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.DEAHeaderGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.DEAHeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DEAHeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DEAHeaderGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.DEAHeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DEAHeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.DEAHeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.DEAHeaderGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.DEAHeaderGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DEAHeaderGrid.GridId = "78024478-bc80-40df-8baa-1860cb7a0095";
			this.DEAHeaderGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DEAHeaderGrid.LayoutKey = "DEAHeaderGrid";
			this.DEAHeaderGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DEAHeaderGrid.Name = "DEAHeaderGrid";
			this.DEAHeaderGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 131, true);
			this.DEAHeaderGrid.TabIndex = 0;
			// 
			// SplitContainer1
			// 
			this.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer1.Name = "SplitContainer1";
			this.SplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer1.Panel1
			// 
			this.SplitContainer1.Panel1.Controls.Add(this.DEAHeaderGroupBox);
			this.SplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 300, true);
			this.SplitContainer1.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(150);
			// 
			// SplitContainer1.Panel2
			// 
			this.SplitContainer1.Panel2.Controls.Add(this.RightPanel);
			this.SplitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(150);
			this.SplitContainer1.TabIndex = 1;
			// 
			// DEAUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer1);
			this.Name = "DEAUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RightPanel.ResumeLayout(false);
			this.RightPanel.PerformLayout();
			this.DEAConstituentsGroupBox.ResumeLayout(false);
			this.DEAConstituentsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DEAConstituentsGrid)).EndInit();
			this.DEAConstituentsGrid.ResumeLayout(false);
			this.DEAConstituentsGrid.PerformLayout();
			this.DEAHeaderGroupBox.ResumeLayout(false);
			this.DEAHeaderGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DEAHeaderGrid)).EndInit();
			this.DEAHeaderGrid.ResumeLayout(false);
			this.DEAHeaderGrid.PerformLayout();
			this.SplitContainer1.Panel1.ResumeLayout(false);
			this.SplitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).EndInit();
			this.SplitContainer1.ResumeLayout(false);
			this.SplitContainer1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZPanel RightPanel;
		internal ZArchitecture.GUI.ZGroupBox DEAConstituentsGroupBox;
		internal ZArchitecture.ZGrid DEAConstituentsGrid;
		private ZArchitecture.GUI.ZGroupBox DEAHeaderGroupBox;
		internal ZArchitecture.ZGrid DEAHeaderGrid;
		private CargoWise.Windows.UI.KSplitContainer SplitContainer1;
	}
}
