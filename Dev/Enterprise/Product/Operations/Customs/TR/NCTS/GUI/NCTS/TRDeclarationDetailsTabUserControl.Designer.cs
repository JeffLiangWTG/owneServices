using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.TR.NCTS.GUI
{
	partial class TRDeclarationDetailsTabUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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
			this.StampDutyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.StampDutyStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StampDutyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RegistrationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.Trailer1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Trailer2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BM_MoveToFTZCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BM_CustomsOfficeAtBorderDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LrnRegistrationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LrnRegistrationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TraderDetailsGroupBox.SuspendLayout();
			this.CustomsOfficesGroupBox.SuspendLayout();
			this.GuaranteesGroupBox.SuspendLayout();
			this.ContainersGroupBox.SuspendLayout();
			this.TransportDetailsGroupBox.SuspendLayout();
			this.GoodsLocationNormalGroupBox.SuspendLayout();
			this.GoodsLocationSimplifiedGroupBox.SuspendLayout();
			this.DeclarationDetailsGroupBox.SuspendLayout();
			this.BrokerFindBox.SuspendLayout();
			this.CountryOfDestinationDropEdit.SuspendLayout();
			this.MainDataPanel.SuspendLayout();
			this.LeftDataPanel.SuspendLayout();
			this.DeclarationTypeDropEdit.SuspendLayout();
			this.CountryOfDispatchDropEdit.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.MessageStatusDropEdit.SuspendLayout();
			this.MeansOfTransportCrossingBorderNationalityDropEdit.SuspendLayout();
			this.TransportModeAtBorderDropEdit.SuspendLayout();
			this.MeansOfTransportAtDepartureNationalityDropEdit.SuspendLayout();
			this.InlandTransportModeDropEdit.SuspendLayout();
			this.PlaceOfLoadingNormalCodeFindBox.SuspendLayout();
			this.ControlResultDateLimitDateEdit.SuspendLayout();
			this.PlaceOfLoadingSimplifiedCodeFindBox.SuspendLayout();
			this.AgreedLocationOfGoodsCodePanel.SuspendLayout();
			this.GoodsLocationNormalMainPanel.SuspendLayout();
			this.TirCarnetExpiryDateEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StampDutyGroupBox.SuspendLayout();
			this.StampDutyStatusDropEdit.SuspendLayout();
			this.RegistrationDateEdit.SuspendLayout();
			this.BM_MoveToFTZCheckBox.SuspendLayout();
			this.BM_CustomsOfficeAtBorderDropEdit.SuspendLayout();
			this.LrnRegistrationNumberTextBox.SuspendLayout();
			this.LrnRegistrationDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// CustomsOfficesGroupBox
			//
			this.CustomsOfficesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
			this.CustomsOfficesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 174, true);
			// 
			// GuaranteesGroupBox
			// 
			this.GuaranteesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 214, true);
			this.GuaranteesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 174, true);
			// 
			// ContainersGroupBox
			// 
			this.ContainersGroupBox.Dock = System.Windows.Forms.DockStyle.None;
			this.ContainersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 440, true);
			this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 194, true);
			this.ContainersGroupBox.TabIndex = 3;
			// 
			// TransportDetailsGroupBox
			// 
			this.TransportDetailsGroupBox.Controls.Add(this.Trailer1TextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.Trailer2TextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.BM_MoveToFTZCheckBox);
			this.TransportDetailsGroupBox.Controls.Add(this.BM_CustomsOfficeAtBorderDropEdit);
			this.TransportDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 240, true);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.InlandTransportModeDropEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.MeansOfTransportAtDepartureIdentityTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.MeansOfTransportAtDepartureNationalityDropEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.TransportModeAtBorderDropEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.MeansOfTransportCrossingBorderIdentityTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.MeansOfTransportCrossingBorderNationalityDropEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.Trailer1TextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.Trailer2TextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.BM_MoveToFTZCheckBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.BM_CustomsOfficeAtBorderDropEdit, 0);
			// 
			// GoodsLocationNormalGroupBox
			// 
			this.GoodsLocationNormalGroupBox.Dock = System.Windows.Forms.DockStyle.None;
			this.GoodsLocationNormalGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 550, true);
			// 
			// GoodsLocationSimplifiedGroupBox
			// 
			this.GoodsLocationSimplifiedGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 612, true);
			this.GoodsLocationSimplifiedGroupBox.Dock = System.Windows.Forms.DockStyle.None;
			// 
			// LeftDataPanel
			// 
			this.LeftDataPanel.Controls.Add(this.StampDutyGroupBox);
			this.LeftDataPanel.Controls.SetChildIndex(this.StampDutyGroupBox, 0);
			this.LeftDataPanel.Controls.SetChildIndex(this.CustomsOfficesGroupBox, 0);
			this.LeftDataPanel.Controls.SetChildIndex(this.GuaranteesGroupBox, 0);
			this.LeftDataPanel.Controls.SetChildIndex(this.ContainersGroupBox, 0);
			// 
			// MeansOfTransportCrossingBorderNationalityDropEdit
			// 
			this.MeansOfTransportCrossingBorderNationalityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 150, true);
			this.MeansOfTransportCrossingBorderNationalityDropEdit.TabIndex = 6;
			// 
			// MeansOfTransportCrossingBorderIdentityTextBox
			// 
			this.MeansOfTransportCrossingBorderIdentityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 128, true);
			this.MeansOfTransportCrossingBorderIdentityTextBox.TabIndex = 5;
			// 
			// TransportModeAtBorderDropEdit
			// 
			this.TransportModeAtBorderDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 172, true);
			this.TransportModeAtBorderDropEdit.TabIndex = 7;
			// 
			// InlandTransportModeDropEdit
			// 
			this.InlandTransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 106, true);
			this.InlandTransportModeDropEdit.TabIndex = 4;
			// 
			// ContainersAndSealsZDynamicUserControl
			// 
			this.ContainersAndSealsZDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 159, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.NCTS.Business.NctsHeader);
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 85, true);
			this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 111, true);
			this.SimplifiedNctsProcedureCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 133, true);
			this.SafetyAndSecurityCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(402, 133, true);
			this.DeclarationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 155, true);
			this.DeclarationDetailsGroupBox.Controls.Add(this.LrnRegistrationDateEdit);
			this.DeclarationDetailsGroupBox.Controls.Add(this.LrnRegistrationNumberTextBox);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.LrnRegistrationNumberTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.LrnRegistrationDateEdit, 0);
			// 
			// StampDutyGroupBox
			// 
			this.StampDutyGroupBox.Controls.Add(this.StampDutyStatusDropEdit);
			this.StampDutyGroupBox.Controls.Add(this.StampDutyCalcEdit);
			this.StampDutyGroupBox.Controls.Add(this.RegistrationDateEdit);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StampDutyGroupBox, false);
			this.StampDutyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 350, true);
			this.StampDutyGroupBox.Name = "StampDutyGroupBox";
			this.StampDutyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 85, true);
			this.StampDutyGroupBox.TabIndex = 2;
			this.StampDutyGroupBox.TabStop = false;
			// 
			// StampDutyStatusDropEdit
			// 
			this.StampDutyStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StampDutyStatusDropEdit, "StampDutyStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).StampDutyStatus)));
			this.StampDutyStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 15, true);
			this.StampDutyStatusDropEdit.Name = "StampDutyStatusDropEdit";
			this.StampDutyStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 15, true);
			this.StampDutyStatusDropEdit.TabIndex = 0;
			// 
			// StampDutyCalcEdit
			// 
			this.StampDutyCalcEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StampDutyCalcEdit, "StampDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).StampDuty)));
			this.StampDutyCalcEdit.CaptionResourceString = null;
			this.StampDutyCalcEdit.DecimalPlaces = 2;
			this.StampDutyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 39, true);
			this.StampDutyCalcEdit.Name = "StampDutyCalcEdit";
			this.StampDutyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 15, true);
			this.StampDutyCalcEdit.TabIndex = 0;
			this.StampDutyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RegistrationDateEdit
			// 
			this.RegistrationDateEdit.AllowDrop = true;
			this.RegistrationDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.RegistrationDateEdit, "RegistrationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).RegistrationDate)));
			this.RegistrationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 63, true);
			this.RegistrationDateEdit.Name = "RegistrationDateEdit";
			this.RegistrationDateEdit.TabIndex = 0;
			// 
			// Trailer1TextBox
			// 
			this.BindingSource.SetBindingMember(this.Trailer1TextBox, "Trailer1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).Trailer1)));
			this.Trailer1TextBox.CaptionResourceString = null;
			this.Trailer1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 62, true);
			this.Trailer1TextBox.Name = "Trailer1TextBox";
			this.Trailer1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 20, true);
			this.Trailer1TextBox.TabIndex = 2;
			// 
			// Trailer2TextBox
			// 
			this.BindingSource.SetBindingMember(this.Trailer2TextBox, "Trailer2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).Trailer2)));
			this.Trailer2TextBox.CaptionResourceString = null;
			this.Trailer2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 84, true);
			this.Trailer2TextBox.Name = "Trailer2TextBox";
			this.Trailer2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 20, true);
			this.Trailer2TextBox.TabIndex = 3;
			//
			// BM_MoveToFTZCheckBox
			// 
			this.BM_MoveToFTZCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.BM_MoveToFTZCheckBox, "MovementHeader.MoveToFTZ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).MovementHeader.MoveToFTZ)));
			this.BM_MoveToFTZCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.BM_MoveToFTZCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 194, true);
			this.BM_MoveToFTZCheckBox.Name = "BM_MoveToFTZCheckBox";
			this.BM_MoveToFTZCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.BM_MoveToFTZCheckBox.TabIndex = 4;
			this.BM_MoveToFTZCheckBox.UseVisualStyleBackColor = true;
			this.BM_MoveToFTZCheckBox.CheckedChanged += this.MoveToFTZInfo_ValueChanged;
			// 
			// BM_CustomsOfficeAtBorderDropEdit
			// 
			this.BM_CustomsOfficeAtBorderDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BM_CustomsOfficeAtBorderDropEdit, "MovementHeader.BM_CustomsOfficeAtBorder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_CustomsOfficeAtBorder)));
			this.BM_CustomsOfficeAtBorderDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 214, true);
			this.BM_CustomsOfficeAtBorderDropEdit.Name = "BM_CustomsOfficeAtBorderDropEdit";
			this.BM_CustomsOfficeAtBorderDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 15, true);
			this.BM_CustomsOfficeAtBorderDropEdit.CaptionResourceString = Res.GetData("92310979-5680-4D95-9838-D96BBE41A46D", "Goods Ship to Code");
			// 
			// LrnRegistrationNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.LrnRegistrationNumberTextBox, "LrnRegistrationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).LrnRegistrationNumber)));
			this.LrnRegistrationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 61, true);
			this.LrnRegistrationNumberTextBox.Name = "LrnRegistrationNumberTextBox";
			this.LrnRegistrationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 30, true);
			this.LrnRegistrationNumberTextBox.TabIndex = 2;
			this.LrnRegistrationNumberTextBox.TabStop = false;
			// 
			// LrnRegistrationDateEdit
			// 
			this.LrnRegistrationDateEdit.AllowDrop = true;
			this.LrnRegistrationDateEdit.AutoCompleteMonthThreshold = 1;
			this.LrnRegistrationDateEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.LrnRegistrationDateEdit, "LrnRegistrationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).LrnRegistrationDate)));
			this.LrnRegistrationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 61, true);
			this.LrnRegistrationDateEdit.Name = "LrnRegistrationDateEdit";
			this.LrnRegistrationDateEdit.TabIndex = 2;
			this.LrnRegistrationDateEdit.TabStop = false;
			// 
			// TRDeclarationDetailsTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "TRDeclarationDetailsTabUserControl";
			this.TraderDetailsGroupBox.ResumeLayout(false);
			this.TraderDetailsGroupBox.PerformLayout();
			this.CustomsOfficesGroupBox.ResumeLayout(false);
			this.CustomsOfficesGroupBox.PerformLayout();
			this.GuaranteesGroupBox.ResumeLayout(false);
			this.GuaranteesGroupBox.PerformLayout();
			this.ContainersGroupBox.ResumeLayout(false);
			this.ContainersGroupBox.PerformLayout();
			this.TransportDetailsGroupBox.ResumeLayout(false);
			this.TransportDetailsGroupBox.PerformLayout();
			this.GoodsLocationNormalGroupBox.ResumeLayout(false);
			this.GoodsLocationNormalGroupBox.PerformLayout();
			this.GoodsLocationSimplifiedGroupBox.ResumeLayout(false);
			this.GoodsLocationSimplifiedGroupBox.PerformLayout();
			this.DeclarationDetailsGroupBox.ResumeLayout(false);
			this.DeclarationDetailsGroupBox.PerformLayout();
			this.BrokerFindBox.ResumeLayout(true);
			this.BrokerFindBox.PerformLayout();
			this.CountryOfDestinationDropEdit.ResumeLayout(true);
			this.CountryOfDestinationDropEdit.PerformLayout();
			this.MainDataPanel.ResumeLayout(false);
			this.MainDataPanel.PerformLayout();
			this.LeftDataPanel.ResumeLayout(false);
			this.LeftDataPanel.PerformLayout();
			this.DeclarationTypeDropEdit.ResumeLayout(true);
			this.DeclarationTypeDropEdit.PerformLayout();
			this.CountryOfDispatchDropEdit.ResumeLayout(true);
			this.CountryOfDispatchDropEdit.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.MessageStatusDropEdit.ResumeLayout(true);
			this.MessageStatusDropEdit.PerformLayout();
			this.MeansOfTransportCrossingBorderNationalityDropEdit.ResumeLayout(true);
			this.MeansOfTransportCrossingBorderNationalityDropEdit.PerformLayout();
			this.TransportModeAtBorderDropEdit.ResumeLayout(true);
			this.TransportModeAtBorderDropEdit.PerformLayout();
			this.MeansOfTransportAtDepartureNationalityDropEdit.ResumeLayout(true);
			this.MeansOfTransportAtDepartureNationalityDropEdit.PerformLayout();
			this.InlandTransportModeDropEdit.ResumeLayout(true);
			this.InlandTransportModeDropEdit.PerformLayout();
			this.PlaceOfLoadingNormalCodeFindBox.ResumeLayout(true);
			this.PlaceOfLoadingNormalCodeFindBox.PerformLayout();
			this.ControlResultDateLimitDateEdit.ResumeLayout(true);
			this.ControlResultDateLimitDateEdit.PerformLayout();
			this.PlaceOfLoadingSimplifiedCodeFindBox.ResumeLayout(true);
			this.PlaceOfLoadingSimplifiedCodeFindBox.PerformLayout();
			this.AgreedLocationOfGoodsCodePanel.ResumeLayout(false);
			this.AgreedLocationOfGoodsCodePanel.PerformLayout();
			this.GoodsLocationNormalMainPanel.ResumeLayout(false);
			this.GoodsLocationNormalMainPanel.PerformLayout();
			this.TirCarnetExpiryDateEdit.ResumeLayout(true);
			this.TirCarnetExpiryDateEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StampDutyGroupBox.ResumeLayout(false);
			this.StampDutyGroupBox.PerformLayout();
			this.StampDutyStatusDropEdit.ResumeLayout(true);
			this.StampDutyStatusDropEdit.PerformLayout();
			this.RegistrationDateEdit.ResumeLayout(true);
			this.RegistrationDateEdit.PerformLayout();
			this.BM_MoveToFTZCheckBox.ResumeLayout(true);
			this.BM_MoveToFTZCheckBox.PerformLayout();
			this.BM_CustomsOfficeAtBorderDropEdit.ResumeLayout(true);
			this.BM_CustomsOfficeAtBorderDropEdit.PerformLayout();
			this.LrnRegistrationNumberTextBox.ResumeLayout(true);
			this.LrnRegistrationNumberTextBox.PerformLayout();
			this.LrnRegistrationDateEdit.ResumeLayout(true);
			this.LrnRegistrationDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZDropEdit StampDutyStatusDropEdit;
		Enterprise.ZArchitecture.ZCalcEdit StampDutyCalcEdit;
		Enterprise.ZArchitecture.GUI.ZDateEdit RegistrationDateEdit;
		ZArchitecture.GUI.ZGroupBox StampDutyGroupBox;
		ZArchitecture.ZTextBox Trailer1TextBox;
		ZArchitecture.ZTextBox Trailer2TextBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox BM_MoveToFTZCheckBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit BM_CustomsOfficeAtBorderDropEdit;
		Enterprise.ZArchitecture.ZTextBox LrnRegistrationNumberTextBox;
		Enterprise.ZArchitecture.GUI.ZDateEdit LrnRegistrationDateEdit;
	}
}
