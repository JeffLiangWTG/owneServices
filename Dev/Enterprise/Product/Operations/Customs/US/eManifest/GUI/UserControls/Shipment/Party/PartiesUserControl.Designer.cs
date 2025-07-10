namespace Enterprise.Customs.US.eManifest.GUI
{
	partial class PartiesUserControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.PartyUserControl = new Enterprise.Customs.US.eManifest.GUI.PartyUserControl();
			this.PartiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PartiesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PartiesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PartiesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.eManifest.Business.Shipment);
			// 
			// PartyUserControl
			// 
			this.PartyUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PartyUserControl, "Parties");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.US.eManifest.Business.Party)(((Enterprise.Customs.US.eManifest.Business.Party)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Parties)).SyncRoot)))));
			this.PartyUserControl.CaptionResourceString = null;
			this.PartyUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PartyUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 83, true);
			this.PartyUserControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 217, true);
			this.PartyUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 217, true);
			this.PartyUserControl.Name = "PartyUserControl";
			this.PartyUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 217, true);
			this.PartyUserControl.TabIndex = 1;
			// 
			// PartiesGroupBox
			// 
			this.PartiesGroupBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("PartiesUserControl|857c732a-9d91-4899-960a-cc00e24e1dd3", "Other Parties");
			this.PartiesGroupBox.Controls.Add(this.PartiesGrid);
			this.PartiesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PartiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PartiesGroupBox.Name = "PartiesGroupBox";
			this.PartiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 83, true);
			this.PartiesGroupBox.TabIndex = 0;
			this.PartiesGroupBox.TabStop = false;
			// 
			// PartiesGrid
			// 
			this.PartiesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PartiesGrid, "Parties");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Parties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Party)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Parties)).SyncRoot)).E2_AddressType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.eManifest.Business.Party)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Parties)).SyncRoot)).OrganisationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.eManifest.Business.Party)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Parties)).SyncRoot)).E2_AddressOverride)));
			this.PartiesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.Caption = null;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("PartiesUserControl|2ebdc7cc-f47d-4272-81b3-6d5438680bcd", "Type");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "E2_AddressType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo1.Caption = null;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("PartiesUserControl|ea003459-a8f4-4992-ae13-6a72d1a72063", "Party");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "OrganisationPK";
			zCheckBoxColumnStyleInfo1.Caption = null;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = null;
			zCheckBoxColumnStyleInfo1.ColumnName = "E2_AddressOverride";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.PartiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PartiesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.PartiesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.PartiesGrid.CopySelectedRowsAllowed = true;
			this.PartiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PartiesGrid.GridId = "f9a64bd8-74a4-474e-a42e-eb7cacbfa2e4";
			this.PartiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PartiesGrid.LayoutKey = "PartiesGrid";
			this.PartiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PartiesGrid.Name = "PartiesGrid";
			this.PartiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 64, true);
			this.PartiesGrid.TabIndex = 0;
			// 
			// PartiesUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.PartiesGroupBox);
			this.Controls.Add(this.PartyUserControl);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 2000, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 300, true);
			this.Name = "PartiesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PartiesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PartiesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private PartyUserControl PartyUserControl;
		private ZArchitecture.GUI.ZGroupBox PartiesGroupBox;
		private ZArchitecture.ZGrid PartiesGrid;
	}
}
