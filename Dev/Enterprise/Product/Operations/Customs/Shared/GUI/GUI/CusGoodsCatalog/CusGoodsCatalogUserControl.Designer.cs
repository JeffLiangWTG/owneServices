namespace Enterprise.Customs.GUI
{
	partial class CusGoodsCatalogUserControl
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
			this.GoodsCatalogGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.VersionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AuthorityIdentifierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneeFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.TariffNumFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.TypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CatalogCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GoodsCatalogGroupBox.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.ConsigneeFindBox.SuspendLayout();
			this.TariffNumFindBox.SuspendLayout();
			this.TypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseCusGoodsCatalog);
			//
			// GoodsCatalogGroupBox
			//
			this.GoodsCatalogGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("616af7de-0f6d-4968-accd-9f29807d1691", "Goods Catalog");
			this.GoodsCatalogGroupBox.Controls.Add(this.VersionTextBox);
			this.GoodsCatalogGroupBox.Controls.Add(this.StatusDropEdit);
			this.GoodsCatalogGroupBox.Controls.Add(this.AuthorityIdentifierTextBox);
			this.GoodsCatalogGroupBox.Controls.Add(this.ConsigneeFindBox);
			this.GoodsCatalogGroupBox.Controls.Add(this.TariffNumFindBox);
			this.GoodsCatalogGroupBox.Controls.Add(this.TypeDropEdit);
			this.GoodsCatalogGroupBox.Controls.Add(this.DescriptionTextBox);
			this.GoodsCatalogGroupBox.Controls.Add(this.CatalogCodeTextBox);
			this.GoodsCatalogGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.GoodsCatalogGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsCatalogGroupBox.Name = "GoodsCatalogGroupBox";
			this.GoodsCatalogGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 207, true);
			this.GoodsCatalogGroupBox.TabIndex = 2;
			this.GoodsCatalogGroupBox.TabStop = false;
			//
			// VersionTextBox
			//
			this.BindingSource.SetBindingMember(this.VersionTextBox, "CGC_AuthorityVersion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusGoodsCatalog)(null)).CGC_AuthorityVersion)));
			this.VersionTextBox.DecimalPlaces = 0;
			this.VersionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(309, 58, true);
			this.VersionTextBox.Name = "VersionTextBox";
			this.VersionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.VersionTextBox.TabIndex = 8;
			this.VersionTextBox.Text = "0";
			this.VersionTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.VersionTextBox.TrackDisposedAccess = true;
			//
			// StatusDropEdit
			//
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "CGC_AuthorityStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseCusGoodsCatalog)(null)).CGC_AuthorityStatus)));
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(481, 58, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.PreBoundMaxLength = 1;
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.StatusDropEdit.TabIndex = 4;
			//
			// AuthorityIdentifierTextBox
			//
			this.BindingSource.SetBindingMember(this.AuthorityIdentifierTextBox, "CGC_AuthorityIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusGoodsCatalog)(null)).CGC_AuthorityIdentifier)));
			this.AuthorityIdentifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 58, true);
			this.AuthorityIdentifierTextBox.Name = "AuthorityIdentifierTextBox";
			this.AuthorityIdentifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 20, true);
			this.AuthorityIdentifierTextBox.TabIndex = 2;
			//
			// ConsigneeFindBox
			//
			this.ConsigneeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeFindBox, "CGC_OH_Owner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseCusGoodsCatalog)(null)).CGC_OH_Owner)));
			this.ConsigneeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 120, true);
			this.ConsigneeFindBox.Name = "ConsigneeFindBox";
			this.ConsigneeFindBox.ParentModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.ConsigneeFindBox.ParentType = null;
			this.ConsigneeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 20, true);
			this.ConsigneeFindBox.TabIndex = 6;
			//
			// TariffNumFindBox
			//
			this.TariffNumFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffNumFindBox, "CGC_Tariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusGoodsCatalog)(null)).CGC_Tariff)));
			this.TariffNumFindBox.ErrorForUnsupportedCountry = null;
			this.TariffNumFindBox.GetEffectiveDate = null;
			this.TariffNumFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 89, true);
			this.TariffNumFindBox.Name = "TariffNumFindBox";
			this.TariffNumFindBox.NeedLoadParentDataGroup = true;
			this.TariffNumFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TariffNumFindBox.ParentType = null;
			this.TariffNumFindBox.SelectNomenclatureModes = null;
			this.TariffNumFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.TariffNumFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 20, true);
			this.TariffNumFindBox.TabIndex = 5;
			this.TariffNumFindBox.TariffType = null;
			//
			// TypeDropEdit
			//
			this.TypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TypeDropEdit, "CGC_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseCusGoodsCatalog)(null)).CGC_Type)));
			this.TypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(309, 27, true);
			this.TypeDropEdit.Name = "TypeDropEdit";
			this.TypeDropEdit.PreBoundMaxLength = 3;
			this.TypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.TypeDropEdit.TabIndex = 1;
			//
			// DescriptionTextBox
			//
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "CGC_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusGoodsCatalog)(null)).CGC_Description)));
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 151, true);
			this.DescriptionTextBox.Multiline = true;
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 39, true);
			this.DescriptionTextBox.TabIndex = 7;
			//
			// CatalogCodeTextBox
			//
			this.BindingSource.SetBindingMember(this.CatalogCodeTextBox, "CGC_CatalogCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusGoodsCatalog)(null)).CGC_CatalogCode)));
			this.CatalogCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 27, true);
			this.CatalogCodeTextBox.Name = "CatalogCodeTextBox";
			this.CatalogCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 20, true);
			this.CatalogCodeTextBox.TabIndex = 0;
			//
			// CusGoodsCatalogUserControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GoodsCatalogGroupBox);
			this.Name = "CusGoodsCatalogUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 271, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GoodsCatalogGroupBox.ResumeLayout(false);
			this.GoodsCatalogGroupBox.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.ConsigneeFindBox.ResumeLayout(true);
			this.ConsigneeFindBox.PerformLayout();
			this.TariffNumFindBox.ResumeLayout(true);
			this.TariffNumFindBox.PerformLayout();
			this.TypeDropEdit.ResumeLayout(true);
			this.TypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZGroupBox GoodsCatalogGroupBox;
		public Universal.GUI.TariffFindBox TariffNumFindBox;
		public ZArchitecture.GUI.ZDropEdit TypeDropEdit;
		public ZArchitecture.ZTextBox DescriptionTextBox;
		public ZArchitecture.ZTextBox CatalogCodeTextBox;
		public ZArchitecture.GUI.ZGuidFindBox ConsigneeFindBox;
		public ZArchitecture.GUI.ZDropEdit StatusDropEdit;
		public ZArchitecture.ZTextBox AuthorityIdentifierTextBox;
		public ZArchitecture.ZTextBox VersionTextBox;
	}
}
