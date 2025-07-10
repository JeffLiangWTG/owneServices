namespace Enterprise.Customs.GUI
{
	partial class ShipmentDetailsUserControl
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
			this.HouseBillParcelPostTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GoodsOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ShipmentDetailsOriginUserControl = new Enterprise.Customs.GUI.ShipmentDetailsOriginUserControl();
			this.ShipmentDetailsFinalDestinationUserControl = new Enterprise.Customs.GUI.ShipmentDetailsFinalDestinationUserControl();
			this.GoodsDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OwnersReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.WeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.VolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.TotalNoOfPiecesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ContainerCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalNoOfPacksCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ShipmentDetailsIncoTermsUserControl = new Enterprise.Customs.GUI.ShipmentDetailsIncoTermsUserControl();
			this.ShipmentDetailsScreeningUserControl = new Enterprise.Customs.GUI.ShipmentDetailsScreeningUserControl();
			this.DeclarationLanguageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.UCRTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MarksAndNumbersNotePopupEdit = new Enterprise.Freight.GUI.ZStmNotePopupEditWithBindableText();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GoodsOriginCodeFindBox.SuspendLayout();
			this.ShipmentDetailsOriginUserControl.SuspendLayout();
			this.ShipmentDetailsFinalDestinationUserControl.SuspendLayout();
			this.WeightCalcDropEdit.SuspendLayout();
			this.VolumeCalcDropEdit.SuspendLayout();
			this.TotalNoOfPacksCalcDropEdit.SuspendLayout();
			this.ShipmentDetailsIncoTermsUserControl.SuspendLayout();
			this.ShipmentDetailsScreeningUserControl.SuspendLayout();
			this.DeclarationLanguageDropEdit.SuspendLayout();
			this.MarksAndNumbersNotePopupEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// HouseBillParcelPostTextBox
			// 
			this.HouseBillParcelPostTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.HouseBillParcelPostTextBox, "JE_HouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_HouseBill)));
			this.HouseBillParcelPostTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 16, true);
			this.HouseBillParcelPostTextBox.Name = "HouseBillParcelPostTextBox";
			this.HouseBillParcelPostTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.HouseBillParcelPostTextBox.TabIndex = 0;
			// 
			// GoodsOriginCodeFindBox
			// 
			this.GoodsOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsOriginCodeFindBox, "JE_GoodsOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_GoodsOrigin)));
			this.GoodsOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 43, true);
			this.GoodsOriginCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.GoodsOriginCodeFindBox.Name = "GoodsOriginCodeFindBox";
			this.GoodsOriginCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GoodsOriginCodeFindBox.ParentType = null;
			this.GoodsOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.GoodsOriginCodeFindBox.TabIndex = 1;
			// 
			// ShipmentDetailsOriginUserControl
			// 
			this.ShipmentDetailsOriginUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentDetailsOriginUserControl, ".");
			this.ShipmentDetailsOriginUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 68, true);
			this.ShipmentDetailsOriginUserControl.Name = "ShipmentDetailsOriginUserControl";
			this.ShipmentDetailsOriginUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.ShipmentDetailsOriginUserControl.TabIndex = 2;
			// 
			// ShipmentDetailsFinalDestinationUserControl
			// 
			this.ShipmentDetailsFinalDestinationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentDetailsFinalDestinationUserControl, ".");
			this.ShipmentDetailsFinalDestinationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 97, true);
			this.ShipmentDetailsFinalDestinationUserControl.Name = "ShipmentDetailsFinalDestinationUserControl";
			this.ShipmentDetailsFinalDestinationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.ShipmentDetailsFinalDestinationUserControl.TabIndex = 3;
			// 
			// GoodsDescriptionTextBox
			// 
			this.GoodsDescriptionTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.GoodsDescriptionTextBox, "JE_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_GoodsDescription)));
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 126, true);
			this.GoodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.GoodsDescriptionTextBox.TabIndex = 4;
			// 
			// OwnersReferenceTextBox
			// 
			this.OwnersReferenceTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.OwnersReferenceTextBox, "JE_OwnerRef");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OwnerRef)));
			this.OwnersReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 152, true);
			this.OwnersReferenceTextBox.Name = "OwnersReferenceTextBox";
			this.OwnersReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.OwnersReferenceTextBox.TabIndex = 5;
			// 
			// WeightCalcDropEdit
			// 
			this.WeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TotalWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TotalWeightUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.WeightUnitList)));
			this.WeightCalcDropEdit.BindToAmount = "JE_TotalWeight";
			this.WeightCalcDropEdit.BindToList = "Lookups.WeightUnitList";
			this.WeightCalcDropEdit.BindToUnit = "JE_TotalWeightUnit";
			this.WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 178, true);
			this.WeightCalcDropEdit.Name = "WeightCalcDropEdit";
			this.WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.WeightCalcDropEdit.TabIndex = 6;
			this.WeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TotalVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TotalVolumeUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.VolumeUnitList)));
			this.VolumeCalcDropEdit.BindToAmount = "JE_TotalVolume";
			this.VolumeCalcDropEdit.BindToList = "Lookups.VolumeUnitList";
			this.VolumeCalcDropEdit.BindToUnit = "JE_TotalVolumeUnit";
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 204, true);
			this.VolumeCalcDropEdit.Name = "VolumeCalcDropEdit";
			this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.VolumeCalcDropEdit.TabIndex = 7;
			this.VolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// TotalNoOfPiecesCalcEdit
			// 
			this.TotalNoOfPiecesCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalNoOfPiecesCalcEdit, "JE_TotalNoOfPieces");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TotalNoOfPieces)));
			this.TotalNoOfPiecesCalcEdit.DecimalPlaces = 2;
			this.TotalNoOfPiecesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 230, true);
			this.TotalNoOfPiecesCalcEdit.Name = "TotalNoOfPiecesCalcEdit";
			this.TotalNoOfPiecesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.TotalNoOfPiecesCalcEdit.TabIndex = 8;
			this.TotalNoOfPiecesCalcEdit.Text = "0";
			this.TotalNoOfPiecesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalNoOfPiecesCalcEdit.TrackDisposedAccess = true;
			// 
			// ContainerCountCalcEdit
			// 
			this.ContainerCountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ContainerCountCalcEdit, "JE_ContainerCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_ContainerCount)));
			this.ContainerCountCalcEdit.DecimalPlaces = 2;
			this.ContainerCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 256, true);
			this.ContainerCountCalcEdit.Name = "ContainerCountCalcEdit";
			this.ContainerCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.ContainerCountCalcEdit.TabIndex = 9;
			this.ContainerCountCalcEdit.Text = "0";
			this.ContainerCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ContainerCountCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalNoOfPacksCalcDropEdit
			// 
			this.TotalNoOfPacksCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TotalNoOfPacksCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TotalNoOfPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TotalNoOfPacksPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.JE_TotalNoOfPacksPackType_List)));
			this.TotalNoOfPacksCalcDropEdit.BindToAmount = "JE_TotalNoOfPacks";
			this.TotalNoOfPacksCalcDropEdit.BindToList = "Lookups.JE_TotalNoOfPacksPackType_List";
			this.TotalNoOfPacksCalcDropEdit.BindToUnit = "JE_TotalNoOfPacksPackType";
			this.TotalNoOfPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 282, true);
			this.TotalNoOfPacksCalcDropEdit.Name = "TotalNoOfPacksCalcDropEdit";
			this.TotalNoOfPacksCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.TotalNoOfPacksCalcDropEdit.TabIndex = 10;
			this.TotalNoOfPacksCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// ShipmentDetailsIncoTermsUserControl
			// 
			this.ShipmentDetailsIncoTermsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentDetailsIncoTermsUserControl, ".");
			this.ShipmentDetailsIncoTermsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 308, true);
			this.ShipmentDetailsIncoTermsUserControl.Name = "ShipmentDetailsIncoTermsUserControl";
			this.ShipmentDetailsIncoTermsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.ShipmentDetailsIncoTermsUserControl.TabIndex = 11;
			// 
			// ShipmentDetailsScreeningUserControl
			// 
			this.ShipmentDetailsScreeningUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentDetailsScreeningUserControl, ".");
			this.ShipmentDetailsScreeningUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 335, true);
			this.ShipmentDetailsScreeningUserControl.Name = "ShipmentDetailsScreeningUserControl";
			this.ShipmentDetailsScreeningUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.ShipmentDetailsScreeningUserControl.TabIndex = 12;
			// 
			// DeclarationLanguageDropEdit
			// 
			this.DeclarationLanguageDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarationLanguageDropEdit, "JE_DeclarationLanguage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_DeclarationLanguage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.DeclarationLanguageList)));
			this.DeclarationLanguageDropEdit.BindToList = "Lookups.DeclarationLanguageList";
			this.DeclarationLanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 363, true);
			this.DeclarationLanguageDropEdit.Name = "DeclarationLanguageDropEdit";
			this.DeclarationLanguageDropEdit.PreBoundMaxLength = 2;
			this.DeclarationLanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.DeclarationLanguageDropEdit.TabIndex = 13;
			// 
			// UCRTextBox
			// 
			this.UCRTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.UCRTextBox, "JE_UCR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_UCR)));
			this.UCRTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 390, true);
			this.UCRTextBox.Name = "UCRTextBox";
			this.UCRTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.UCRTextBox.TabIndex = 14;
			//
			// MarksAndNumbersNotePopupEdit
			//
			this.MarksAndNumbersNotePopupEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MarksAndNumbersNotePopupEdit, "JE_MarksAndNumbersShort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_MarksAndNumbersShort)));
			this.MarksAndNumbersNotePopupEdit.ButtonText = "More...";
			this.MarksAndNumbersNotePopupEdit.ButtonWidth = 92;
			this.MarksAndNumbersNotePopupEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 416, true);
			this.MarksAndNumbersNotePopupEdit.MaximumNoteLength = null;
			this.MarksAndNumbersNotePopupEdit.Name = "MarksAndNumbersNotePopupEdit";
			this.MarksAndNumbersNotePopupEdit.NoteTypeDescription = "Marks & Numbers";
			this.MarksAndNumbersNotePopupEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 20, true);
			this.MarksAndNumbersNotePopupEdit.TabIndex = 15;
			// 
			// ShipmentDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UCRTextBox);
			this.Controls.Add(this.DeclarationLanguageDropEdit);
			this.Controls.Add(this.HouseBillParcelPostTextBox);
			this.Controls.Add(this.GoodsOriginCodeFindBox);
			this.Controls.Add(this.ShipmentDetailsOriginUserControl);
			this.Controls.Add(this.ShipmentDetailsFinalDestinationUserControl);
			this.Controls.Add(this.GoodsDescriptionTextBox);
			this.Controls.Add(this.OwnersReferenceTextBox);
			this.Controls.Add(this.WeightCalcDropEdit);
			this.Controls.Add(this.VolumeCalcDropEdit);
			this.Controls.Add(this.TotalNoOfPiecesCalcEdit);
			this.Controls.Add(this.ContainerCountCalcEdit);
			this.Controls.Add(this.TotalNoOfPacksCalcDropEdit);
			this.Controls.Add(this.ShipmentDetailsIncoTermsUserControl);
			this.Controls.Add(this.ShipmentDetailsScreeningUserControl);
			this.Controls.Add(this.MarksAndNumbersNotePopupEdit);
			this.Name = "ShipmentDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 440, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GoodsOriginCodeFindBox.ResumeLayout(true);
			this.GoodsOriginCodeFindBox.PerformLayout();
			this.ShipmentDetailsOriginUserControl.ResumeLayout(true);
			this.ShipmentDetailsOriginUserControl.PerformLayout();
			this.ShipmentDetailsFinalDestinationUserControl.ResumeLayout(true);
			this.ShipmentDetailsFinalDestinationUserControl.PerformLayout();
			this.WeightCalcDropEdit.ResumeLayout(true);
			this.WeightCalcDropEdit.PerformLayout();
			this.VolumeCalcDropEdit.ResumeLayout(true);
			this.VolumeCalcDropEdit.PerformLayout();
			this.TotalNoOfPacksCalcDropEdit.ResumeLayout(true);
			this.TotalNoOfPacksCalcDropEdit.PerformLayout();
			this.ShipmentDetailsIncoTermsUserControl.ResumeLayout(true);
			this.ShipmentDetailsIncoTermsUserControl.PerformLayout();
			this.ShipmentDetailsScreeningUserControl.ResumeLayout(true);
			this.ShipmentDetailsScreeningUserControl.PerformLayout();
			this.DeclarationLanguageDropEdit.ResumeLayout(true);
			this.DeclarationLanguageDropEdit.PerformLayout();
			this.MarksAndNumbersNotePopupEdit.ResumeLayout(true);
			this.MarksAndNumbersNotePopupEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZTextBox HouseBillParcelPostTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox GoodsOriginCodeFindBox;
		internal ShipmentDetailsOriginUserControl ShipmentDetailsOriginUserControl;
		internal ShipmentDetailsFinalDestinationUserControl ShipmentDetailsFinalDestinationUserControl;
		internal Enterprise.ZArchitecture.ZTextBox GoodsDescriptionTextBox;
		internal Enterprise.ZArchitecture.ZTextBox OwnersReferenceTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit WeightCalcDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit VolumeCalcDropEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit TotalNoOfPiecesCalcEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit ContainerCountCalcEdit;
		internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit TotalNoOfPacksCalcDropEdit;
		internal ShipmentDetailsIncoTermsUserControl ShipmentDetailsIncoTermsUserControl;
		internal ShipmentDetailsScreeningUserControl ShipmentDetailsScreeningUserControl;
		internal ZArchitecture.GUI.ZDropEdit DeclarationLanguageDropEdit;
		internal ZArchitecture.ZTextBox UCRTextBox;
		internal Enterprise.Freight.GUI.ZStmNotePopupEditWithBindableText MarksAndNumbersNotePopupEdit;
	}
}
