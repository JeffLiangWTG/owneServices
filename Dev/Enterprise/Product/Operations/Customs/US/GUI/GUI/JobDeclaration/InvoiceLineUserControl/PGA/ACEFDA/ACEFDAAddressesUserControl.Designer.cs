namespace Enterprise.Customs.US.GUI
{
	partial class ACEFDAAddressesUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ACEFDAAddressesUserControl));
			this.AddressSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.DocAddressGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AddressSplitContainer)).BeginInit();
			this.AddressSplitContainer.Panel1.SuspendLayout();
			this.AddressSplitContainer.Panel2.SuspendLayout();
			this.AddressSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocAddressGrid)).BeginInit();
			this.DocAddressGrid.SuspendLayout();
			this.DocAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.ACEFDAJobDocAddressDependentCollection);
			// 
			// AddressSplitContainer
			// 
			this.AddressSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddressSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.AddressSplitContainer.IsSplitterFixed = true;
			this.AddressSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AddressSplitContainer.Name = "AddressSplitContainer";
			// 
			// AddressSplitContainer.Panel1
			// 
			this.AddressSplitContainer.Panel1.Controls.Add(this.DocAddressGrid);
			// 
			// AddressSplitContainer.Panel2
			// 
			this.AddressSplitContainer.Panel2.Controls.Add(this.DocAddressControl);
			this.AddressSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(903, 192, true);
			this.AddressSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(646);
			this.AddressSplitContainer.SplitterWidth = 1;
			this.AddressSplitContainer.TabIndex = 0;
			// 
			// DocAddressGrid
			// 
			this.DocAddressGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DocAddressGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.ACEFDAJobDocAddress)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ACEFDAJobDocAddress)(null)).E2_AddressType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ACEFDAJobDocAddress)(null)).AddressDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.ACEFDAJobDocAddress)(null)).E2_AddressOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.ACEFDAJobDocAddress)(null)).OrganisationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.ACEFDAJobDocAddress)(null)).Lookups.OrgHeader_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ACEFDAJobDocAddress)(null)).E2_CompanyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ACEFDAJobDocAddress)(null)).E2_Address1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ACEFDAJobDocAddress)(null)).E2_Address2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ACEFDAJobDocAddress)(null)).E2_RN_NKCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ACEFDAJobDocAddress)(null)).E2_State)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ACEFDAJobDocAddress)(null)).E2_City)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ACEFDAJobDocAddress)(null)).E2_Postcode)));
			this.DocAddressGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("37fdb8b7-6016-4e85-a88b-5f81c93f709e", "Address Type");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "E2_AddressType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("431508e0-68fc-475e-bc31-21eacd463313", "Desc.", "Description", "Address Description", "");
			zTextBoxColumnStyleInfo1.ColumnName = "AddressDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e425f88b-d154-45a2-a811-6e773c8453fd", "Override");
			zCheckBoxColumnStyleInfo1.ColumnName = "E2_AddressOverride";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(64);
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups.OrgHeader_List";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9d8ed421-e8c0-478b-80e6-2fbdf798e88c", "Org.", "Organization", "");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "OrganisationPK";
			zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ae0e2502-aa99-492b-931d-0e6f80f775bb", "Company Name");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "E2_CompanyName";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("1fb01115-6d15-4095-98d5-982ac4f4ef19", "Address 1");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "E2_Address1";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("045b634c-cbc1-4a60-a0f9-d9c2bdd7fe68", "Address 2");
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "E2_Address2";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(112);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("1fb01115-6d15-4095-98d5-982ac4f4ef19", "Country/Region");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "E2_RN_NKCountryCode";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(96);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ccfe4ca0-d674-451e-938e-612144c417cb", "State");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "E2_State";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f8e3dfc9-7c68-4eef-80e3-bba449d25765", "City");
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "E2_City";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("67cfde7e-0bee-4894-89ae-ee52974a0009", "Post Code");
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "E2_Postcode";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.DocAddressGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.DocAddressGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DocAddressGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DocAddressGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.DocAddressGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DocAddressGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DocAddressGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DocAddressGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.DocAddressGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.DocAddressGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.DocAddressGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.DocAddressGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocAddressGrid.GridId = "46e45554-bccb-426f-97ce-7c4cf4d4a682";
			this.DocAddressGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DocAddressGrid.LayoutKey = "DocAddressGrid";
			this.DocAddressGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocAddressGrid.Name = "DocAddressGrid";
			this.DocAddressGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 192, true);
			this.DocAddressGrid.TabIndex = 0;
			// 
			// DocAddressControl
			// 
			this.DocAddressControl.AddressValidationProcessCmdKey = null;
			this.DocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocAddressControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.US.Business.ACEFDAJobDocAddress)(null)))));
			this.DocAddressControl.BindToOrganisations = "DocAddressOrganizations";
			this.DocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.DocAddressControl.Name = "DocAddressControl";
			this.DocAddressControl.ReadOnly = false;
			this.DocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.DocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.DocAddressControl.TabIndex = 0;
			this.DocAddressControl.Text = "Organization";
			this.DocAddressControl.ValidationJustForced = false;
			// 
			// ACEFDAAddressesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.AddressSplitContainer);
			this.Name = "ACEFDAAddressesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(903, 192, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AddressSplitContainer.Panel1.ResumeLayout(false);
			this.AddressSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AddressSplitContainer)).EndInit();
			this.AddressSplitContainer.ResumeLayout(false);
			this.AddressSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocAddressGrid)).EndInit();
			this.DocAddressGrid.ResumeLayout(false);
			this.DocAddressGrid.PerformLayout();
			this.DocAddressControl.ResumeLayout(true);
			this.DocAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer AddressSplitContainer;
		private ZArchitecture.ZGrid DocAddressGrid;
		private MasterFiles.GUI.ZDocAddressControl DocAddressControl;
	}
}
