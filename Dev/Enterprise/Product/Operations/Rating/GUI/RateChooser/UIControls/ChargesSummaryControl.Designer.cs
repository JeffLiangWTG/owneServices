using System.Windows.Forms;
using Enterprise.Rating.GUI.RateChooser.ViewModel;

namespace Enterprise.Rating.GUI.RateChooser.UIControls
{
    partial class ChargesSummaryControl
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
            this.pnlMain = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.pnlUnmappedCharges = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblUnmappedChargeCodesString = new Enterprise.ZArchitecture.ZLabel();
            this.lblUnmappedChargesLabel = new Enterprise.ZArchitecture.ZLabel();
            this.pnlDSTCharges = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblDSTChargeCodesString = new Enterprise.ZArchitecture.ZLabel();
            this.lblTotalDSTChargesPriceString = new Enterprise.ZArchitecture.ZLabel();
            this.lblDSTChargesLabel = new Enterprise.ZArchitecture.ZLabel();
            this.pnlFRTCharges = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblFRTChargeCodesString = new Enterprise.ZArchitecture.ZLabel();
            this.lblTotalFRTChargesPriceString = new Enterprise.ZArchitecture.ZLabel();
            this.lblFRTChargesLabel = new Enterprise.ZArchitecture.ZLabel();
            this.pnlOrgCharges = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblORGChargeCodesString = new Enterprise.ZArchitecture.ZLabel();
            this.lblTotalORGChargesPriceString = new Enterprise.ZArchitecture.ZLabel();
            this.lblORGChargesLabel = new Enterprise.ZArchitecture.ZLabel();
            this.pnlGroup = new System.Windows.Forms.FlowLayoutPanel();
            this.lblGroupShortName = new Enterprise.ZArchitecture.ZLabel();
            this.pbErrorIcon = new Enterprise.ZArchitecture.GUI.ZPictureBox();
            this.pbCalculationIcon = new Enterprise.ZArchitecture.GUI.ZPictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.pnlMain.SuspendLayout();
            this.pnlUnmappedCharges.SuspendLayout();
            this.pnlDSTCharges.SuspendLayout();
            this.pnlFRTCharges.SuspendLayout();
            this.pnlOrgCharges.SuspendLayout();
            this.pnlGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbErrorIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCalculationIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel);
            // 
            // pnlMain
            // 
            this.pnlMain.AutoSize = true;
            this.pnlMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlMain.Controls.Add(this.pnlUnmappedCharges);
            this.pnlMain.Controls.Add(this.pnlDSTCharges);
            this.pnlMain.Controls.Add(this.pnlFRTCharges);
            this.pnlMain.Controls.Add(this.pnlOrgCharges);
            this.pnlMain.Controls.Add(this.pnlGroup);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 100, true);
            this.pnlMain.TabIndex = 0;
            // 
            // pnlUnmappedCharges
            // 
            this.pnlUnmappedCharges.Controls.Add(this.lblUnmappedChargeCodesString);
            this.pnlUnmappedCharges.Controls.Add(this.lblUnmappedChargesLabel);
            this.pnlUnmappedCharges.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlUnmappedCharges.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 100, true);
            this.pnlUnmappedCharges.Name = "pnlUnmappedCharges";
            this.pnlUnmappedCharges.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 20, true);
            this.pnlUnmappedCharges.TabIndex = 4;
            // 
            // lblUnmappedChargeCodesString
            // 
            this.BindingSource.SetBindingMember(this.lblUnmappedChargeCodesString, "UnmappedChargeCodesString");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel)(null)).UnmappedChargeCodesString)));
            this.lblUnmappedChargeCodesString.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblUnmappedChargeCodesString.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblUnmappedChargeCodesString.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 0, true);
            this.lblUnmappedChargeCodesString.Name = "lblUnmappedChargeCodesString";
            this.lblUnmappedChargeCodesString.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 20, true);
            this.lblUnmappedChargeCodesString.TabIndex = 5;
            this.lblUnmappedChargeCodesString.Text = "UnmappedChargeCodesString";
            this.lblUnmappedChargeCodesString.MouseHover += new System.EventHandler(this.lblUnmappedChargeCodesString_MouseHover);
            // 
            // lblUnmappedChargesLabel
            // 
            this.BindingSource.SetBindingMember(this.lblUnmappedChargesLabel, "UnmappedChargesLabel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel)(null)).UnmappedChargesLabel)));
            this.lblUnmappedChargesLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblUnmappedChargesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblUnmappedChargesLabel.IsFontBold = true;
            this.lblUnmappedChargesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblUnmappedChargesLabel.Name = "lblUnmappedChargesLabel";
            this.lblUnmappedChargesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
            this.lblUnmappedChargesLabel.TabIndex = 1;
            this.lblUnmappedChargesLabel.Text = "Unmapped";
            // 
            // pnlDSTCharges
            // 
            this.pnlDSTCharges.Controls.Add(this.lblDSTChargeCodesString);
            this.pnlDSTCharges.Controls.Add(this.lblTotalDSTChargesPriceString);
            this.pnlDSTCharges.Controls.Add(this.lblDSTChargesLabel);
            this.pnlDSTCharges.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDSTCharges.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 75, true);
            this.pnlDSTCharges.Name = "pnlDSTCharges";
            this.pnlDSTCharges.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 20, true);
            this.pnlDSTCharges.TabIndex = 3;
            // 
            // lblDSTChargeCodesString
            // 
            this.BindingSource.SetBindingMember(this.lblDSTChargeCodesString, "DSTChargeCodesString");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel)(null)).DSTChargeCodesString)));
            this.lblDSTChargeCodesString.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDSTChargeCodesString.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblDSTChargeCodesString.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 0, true);
            this.lblDSTChargeCodesString.Name = "lblDSTChargeCodesString";
            this.lblDSTChargeCodesString.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
            this.lblDSTChargeCodesString.TabIndex = 6;
            this.lblDSTChargeCodesString.Text = "DSTChargeCodesString";
            this.lblDSTChargeCodesString.MouseHover += new System.EventHandler(this.lblDSTChargeCodesString_MouseHover);
            // 
            // lblTotalDSTChargesPriceString
            // 
            this.BindingSource.SetBindingMember(this.lblTotalDSTChargesPriceString, "TotalDSTChargesPriceString");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel)(null)).TotalDSTChargesPriceString)));
            this.lblTotalDSTChargesPriceString.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblTotalDSTChargesPriceString.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblTotalDSTChargesPriceString.IsFontBold = true;
            this.lblTotalDSTChargesPriceString.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(233, 0, true);
            this.lblTotalDSTChargesPriceString.Name = "lblTotalDSTChargesPriceString";
            this.lblTotalDSTChargesPriceString.AutoSize = true;
            this.lblTotalDSTChargesPriceString.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
            this.lblTotalDSTChargesPriceString.TabIndex = 4;
            this.lblTotalDSTChargesPriceString.Text = "TotalDST";
            // 
            // lblDSTChargesLabel
            // 
            this.BindingSource.SetBindingMember(this.lblDSTChargesLabel, "DSTChargesLabel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel)(null)).DSTChargesLabel)));
            this.lblDSTChargesLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblDSTChargesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblDSTChargesLabel.IsFontBold = true;
            this.lblDSTChargesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblDSTChargesLabel.Name = "lblDSTChargesLabel";
            this.lblDSTChargesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
            this.lblDSTChargesLabel.TabIndex = 1;
            this.lblDSTChargesLabel.Text = "DST";
            // 
            // pnlFRTCharges
            // 
            this.pnlFRTCharges.Controls.Add(this.lblFRTChargeCodesString);
            this.pnlFRTCharges.Controls.Add(this.lblTotalFRTChargesPriceString);
            this.pnlFRTCharges.Controls.Add(this.lblFRTChargesLabel);
            this.pnlFRTCharges.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFRTCharges.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 50, true);
            this.pnlFRTCharges.Name = "pnlFRTCharges";
            this.pnlFRTCharges.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 20, true);
            this.pnlFRTCharges.TabIndex = 2;
            // 
            // lblFRTChargeCodesString
            // 
            this.BindingSource.SetBindingMember(this.lblFRTChargeCodesString, "FRTChargeCodesString");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel)(null)).FRTChargeCodesString)));
            this.lblFRTChargeCodesString.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFRTChargeCodesString.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblFRTChargeCodesString.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 0, true);
            this.lblFRTChargeCodesString.Name = "lblFRTChargeCodesString";
            this.lblFRTChargeCodesString.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
            this.lblFRTChargeCodesString.TabIndex = 5;
            this.lblFRTChargeCodesString.Text = "FRTChargeCodesString";
            this.lblFRTChargeCodesString.MouseHover += new System.EventHandler(this.lblFRTChargeCodesString_MouseHover);
            // 
            // lblTotalFRTChargesPriceString
            // 
            this.BindingSource.SetBindingMember(this.lblTotalFRTChargesPriceString, "TotalFRTChargesPriceString");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel)(null)).TotalFRTChargesPriceString)));
            this.lblTotalFRTChargesPriceString.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblTotalFRTChargesPriceString.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblTotalFRTChargesPriceString.IsFontBold = true;
            this.lblTotalFRTChargesPriceString.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(233, 0, true);
            this.lblTotalFRTChargesPriceString.Name = "lblTotalFRTChargesPriceString";
            this.lblTotalFRTChargesPriceString.AutoSize = true;
            this.lblTotalFRTChargesPriceString.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
            this.lblTotalFRTChargesPriceString.TabIndex = 4;
            this.lblTotalFRTChargesPriceString.Text = "TotalFRT";
            // 
            // lblFRTChargesLabel
            // 
            this.BindingSource.SetBindingMember(this.lblFRTChargesLabel, "FRTChargesLabel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel)(null)).FRTChargesLabel)));
            this.lblFRTChargesLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblFRTChargesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblFRTChargesLabel.IsFontBold = true;
            this.lblFRTChargesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblFRTChargesLabel.Name = "lblFRTChargesLabel";
            this.lblFRTChargesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
            this.lblFRTChargesLabel.TabIndex = 1;
            this.lblFRTChargesLabel.Text = "FRT";
            // 
            // pnlOrgCharges
            // 
            this.pnlOrgCharges.Controls.Add(this.lblORGChargeCodesString);
            this.pnlOrgCharges.Controls.Add(this.lblTotalORGChargesPriceString);
            this.pnlOrgCharges.Controls.Add(this.lblORGChargesLabel);
            this.pnlOrgCharges.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlOrgCharges.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
            this.pnlOrgCharges.Name = "pnlOrgCharges";
            this.pnlOrgCharges.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 20, true);
            this.pnlOrgCharges.TabIndex = 1;
            // 
            // lblORGChargeCodesString
            // 
            this.BindingSource.SetBindingMember(this.lblORGChargeCodesString, "ORGChargeCodesString");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel)(null)).ORGChargeCodesString)));
            this.lblORGChargeCodesString.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblORGChargeCodesString.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblORGChargeCodesString.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 0, true);
            this.lblORGChargeCodesString.Name = "lblORGChargeCodesString";
            this.lblORGChargeCodesString.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
            this.lblORGChargeCodesString.TabIndex = 4;
            this.lblORGChargeCodesString.Text = "ORGChargeCodesString";
            this.lblORGChargeCodesString.MouseHover += new System.EventHandler(this.lblORGChargeCodesString_MouseHover);
            // 
            // lblTotalORGChargesPriceString
            // 
            this.BindingSource.SetBindingMember(this.lblTotalORGChargesPriceString, "TotalORGChargesPriceString");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel)(null)).TotalORGChargesPriceString)));
            this.lblTotalORGChargesPriceString.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblTotalORGChargesPriceString.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblTotalORGChargesPriceString.IsFontBold = true;
            this.lblTotalORGChargesPriceString.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(233, 0, true);
            this.lblTotalORGChargesPriceString.Name = "lblTotalORGChargesPriceString";
            this.lblTotalORGChargesPriceString.AutoSize = true;
            this.lblTotalORGChargesPriceString.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
            this.lblTotalORGChargesPriceString.TabIndex = 3;
            this.lblTotalORGChargesPriceString.Text = "TotalOrg";
            // 
            // lblORGChargesLabel
            // 
            this.BindingSource.SetBindingMember(this.lblORGChargesLabel, "ORGChargesLabel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel)(null)).ORGChargesLabel)));
            this.lblORGChargesLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblORGChargesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblORGChargesLabel.IsFontBold = true;
            this.lblORGChargesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblORGChargesLabel.Name = "lblORGChargesLabel";
            this.lblORGChargesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
            this.lblORGChargesLabel.TabIndex = 1;
            this.lblORGChargesLabel.Text = "ORG";
            // 
            // pnlGroup
            // 
            this.pnlGroup.Controls.Add(this.lblGroupShortName);
            this.pnlGroup.Controls.Add(this.pbErrorIcon);
            this.pnlGroup.Controls.Add(this.pbCalculationIcon);
            this.pnlGroup.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlGroup.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlGroup.Name = "pnlGroup";
            this.pnlGroup.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 20, true);
            this.pnlGroup.TabIndex = 0;
            // 
            // lblGroupShortName
            // 
            this.lblGroupShortName.AutoSize = true;
            this.BindingSource.SetBindingMember(this.lblGroupShortName, "GroupShortName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel)(null)).GroupShortName)));
            this.lblGroupShortName.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblGroupShortName.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblGroupShortName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
            this.lblGroupShortName.Name = "lblGroupShortName";
            this.lblGroupShortName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 23, true);
            this.lblGroupShortName.TabIndex = 0;
            this.lblGroupShortName.Text = "GroupShortName";
            // 
            // pbErrorIcon
            // 
            this.pbErrorIcon.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 3, true);
            this.pbErrorIcon.Name = "pbErrorIcon";
            this.pbErrorIcon.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
            this.pbErrorIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbErrorIcon.TabIndex = 10;
            this.pbErrorIcon.TabStop = false;
            this.pbErrorIcon.MouseHover += new System.EventHandler(this.pbErrorIcon_MouseHover);
            // 
            // pbCalculationIcon
            // 
            this.pbCalculationIcon.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 3, true);
            this.pbCalculationIcon.Name = "pbCalculationIcon";
            this.pbCalculationIcon.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(17, 17, true);
            this.pbCalculationIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbCalculationIcon.TabIndex = 11;
            this.pbCalculationIcon.TabStop = false;
            // 
            // ChargesSummaryControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.pnlMain);
            this.Name = "ChargesSummaryControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 100, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.pnlMain.ResumeLayout(false);
            this.pnlMain.PerformLayout();
            this.pnlUnmappedCharges.ResumeLayout(false);
            this.pnlUnmappedCharges.PerformLayout();
            this.pnlDSTCharges.ResumeLayout(false);
            this.pnlDSTCharges.PerformLayout();
            this.pnlFRTCharges.ResumeLayout(false);
            this.pnlFRTCharges.PerformLayout();
            this.pnlOrgCharges.ResumeLayout(false);
            this.pnlOrgCharges.PerformLayout();
            this.pnlGroup.ResumeLayout(false);
            this.pnlGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbErrorIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCalculationIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ZArchitecture.GUI.ZPanel pnlMain;
        private FlowLayoutPanel pnlGroup;
        private ZArchitecture.ZLabel lblGroupShortName;
        private ZArchitecture.GUI.ZPanel pnlOrgCharges;
        private ZArchitecture.ZLabel lblORGChargesLabel;
        private ZArchitecture.GUI.ZPanel pnlDSTCharges;
        private ZArchitecture.ZLabel lblDSTChargesLabel;
        private ZArchitecture.GUI.ZPanel pnlFRTCharges;
        private ZArchitecture.ZLabel lblFRTChargesLabel;
        private ZArchitecture.ZLabel lblTotalORGChargesPriceString;
        private ZArchitecture.ZLabel lblTotalDSTChargesPriceString;
        private ZArchitecture.ZLabel lblTotalFRTChargesPriceString;
        private ZArchitecture.GUI.ZPanel pnlUnmappedCharges;
        private ZArchitecture.ZLabel lblUnmappedChargeCodesString;
        private ZArchitecture.ZLabel lblUnmappedChargesLabel;
        private ZArchitecture.ZLabel lblORGChargeCodesString;
        private ZArchitecture.ZLabel lblFRTChargeCodesString;
        private ZArchitecture.ZLabel lblDSTChargeCodesString;
        private ZArchitecture.GUI.ZPictureBox pbErrorIcon;
        private ZArchitecture.GUI.ZPictureBox pbCalculationIcon;
    }
}
