namespace Enterprise.MasterFiles.GUI
{
	partial class EUTaxIDDefaultingControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.CostGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CostGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SellGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SellGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.CostGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CostGrid)).BeginInit();
			this.SellGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SellGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.EUTaxIDDefaultingRuleCollection);
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.CostGroupBox);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.SellGroupBox);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 335, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(164);
			this.SplitContainer.TabIndex = 1;
			// 
			// CostGroupBox
			// 
			this.CostGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EUTaxIDDefaultingControl|d54c8293-eb88-4333-b5b6-8db67afdfe79", "Cost Line Tax ID by AP Organization Tax Registration");
			this.CostGroupBox.Controls.Add(this.CostGrid);
			this.CostGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CostGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CostGroupBox.Name = "CostGroupBox";
			this.CostGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 164, true);
			this.CostGroupBox.TabIndex = 2;
			this.CostGroupBox.TabStop = false;
			// 
			// CostGrid
			// 
			this.CostGrid.AllowNavigation = false;
			this.CostGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.CostGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.EUTaxIDDefaultingRule)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EUTaxIDDefaultingRule)(null)).JobDirectionDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EUTaxIDDefaultingRule)(null)).OriginDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EUTaxIDDefaultingRule)(null)).DestinationDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.EUTaxIDDefaultingRule)(null)).CostTaxRateForOrganisationRegisteredInMyCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.EUTaxIDDefaultingRule)(null)).CostTaxRateForOrganisationRegisteredInOtherEUCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.EUTaxIDDefaultingRule)(null)).CostTaxRateForNotRegisteredOrganisation)));
			this.CostGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EUTaxIDDefaultingControl|d8615197-36dd-4bb9-8b1a-17d45b8c5a14", "Job Direction Description");
			zTextBoxColumnStyleInfo1.ColumnName = "JobDirectionDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EUTaxIDDefaultingControl|75c3f048-fb7f-4905-9fa7-23c2550f9776", "Origin");
			zTextBoxColumnStyleInfo2.ColumnName = "OriginDescription";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EUTaxIDDefaultingControl|1236206b-b6e5-43e7-bcd7-a925fb29fac2", "Destination");
			zTextBoxColumnStyleInfo3.ColumnName = "DestinationDescription";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EUTaxIDDefaultingControl|c1b45d4e-a8ec-4c3b-9546-2a169560cbf4", "My Country/Region");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "CostTaxRateForOrganisationRegisteredInMyCountry";
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EUTaxIDDefaultingControl|2c34814a-c4a5-44af-9e8c-c0d056354d34", "Other EU Country/Region");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "CostTaxRateForOrganisationRegisteredInOtherEUCountry";
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EUTaxIDDefaultingControl|d210e532-e769-4ad8-9f49-af7d5b583593", "No Registration");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "CostTaxRateForNotRegisteredOrganisation";
			this.CostGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CostGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CostGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CostGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CostGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.CostGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.CostGrid.GridId = "5f8f16db-db68-44fc-bf0d-35c8e015a021";
			this.CostGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CostGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CostGrid.LayoutKey = "Grid";
			this.CostGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CostGrid.Name = "CostGrid";
			this.CostGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(607, 145, true);
			this.CostGrid.TabIndex = 2;
			// 
			// SellGroupBox
			// 
			this.SellGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EUTaxIDDefaultingControl|4bf5a374-a3f6-47b2-92eb-a919e4560840", "Sell Line Tax ID by AR Organization Tax Registration");
			this.SellGroupBox.Controls.Add(this.SellGrid);
			this.SellGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SellGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SellGroupBox.Name = "SellGroupBox";
			this.SellGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 167, true);
			this.SellGroupBox.TabIndex = 3;
			this.SellGroupBox.TabStop = false;
			// 
			// SellGrid
			// 
			this.SellGrid.AllowNavigation = false;
			this.SellGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.SellGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.EUTaxIDDefaultingRule)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EUTaxIDDefaultingRule)(null)).JobDirectionDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EUTaxIDDefaultingRule)(null)).OriginDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EUTaxIDDefaultingRule)(null)).DestinationDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.EUTaxIDDefaultingRule)(null)).SellTaxRateForOrganisationRegisteredInMyCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.EUTaxIDDefaultingRule)(null)).SellTaxRateForOrganisationRegisteredInOtherEUCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.EUTaxIDDefaultingRule)(null)).SellTaxRateForNotRegisteredOrganisation)));
			this.SellGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EUTaxIDDefaultingControl|d8615197-36dd-4bb9-8b1a-17d45b8c5a14", "Job Direction Description");
			zTextBoxColumnStyleInfo4.ColumnName = "JobDirectionDescription";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EUTaxIDDefaultingControl|75c3f048-fb7f-4905-9fa7-23c2550f9776", "Origin");
			zTextBoxColumnStyleInfo5.ColumnName = "OriginDescription";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EUTaxIDDefaultingControl|1236206b-b6e5-43e7-bcd7-a925fb29fac2", "Destination");
			zTextBoxColumnStyleInfo6.ColumnName = "DestinationDescription";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EUTaxIDDefaultingControl|70a32cfd-c125-4473-ac2b-6b832da5155a", "My Country/Region");
			zGuidFindBoxColumnStyleInfo4.ColumnName = "SellTaxRateForOrganisationRegisteredInMyCountry";
			zGuidFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EUTaxIDDefaultingControl|70c0011e-e572-49d4-8e0c-1bb71fa274eb", "Other EU Country/Region");
			zGuidFindBoxColumnStyleInfo5.ColumnName = "SellTaxRateForOrganisationRegisteredInOtherEUCountry";
			zGuidFindBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EUTaxIDDefaultingControl|f809151c-1b34-4316-999a-2f7ea1f93214", "No Registration");
			zGuidFindBoxColumnStyleInfo6.ColumnName = "SellTaxRateForNotRegisteredOrganisation";
			this.SellGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.SellGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.SellGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.SellGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.SellGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.SellGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo6);
			this.SellGrid.GridId = "281b8091-9ff9-41ef-b8a3-e1207c01d20a";
			this.SellGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SellGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SellGrid.LayoutKey = "Grid";
			this.SellGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SellGrid.Name = "SellGrid";
			this.SellGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(607, 148, true);
			this.SellGrid.TabIndex = 3;
			// 
			// EUTaxIDDefaultingControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Name = "EUTaxIDDefaultingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 335, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			this.SplitContainer.ResumeLayout(false);
			this.CostGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CostGrid)).EndInit();
			this.SellGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SellGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		CargoWise.Windows.UI.KSplitContainer SplitContainer;
		Enterprise.ZArchitecture.GUI.ZGroupBox CostGroupBox;
		internal Enterprise.ZArchitecture.ZGrid CostGrid;
		Enterprise.ZArchitecture.GUI.ZGroupBox SellGroupBox;
		internal Enterprise.ZArchitecture.ZGrid SellGrid;

	}
}
