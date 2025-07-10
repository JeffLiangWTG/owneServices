namespace Enterprise.MasterFiles.GUI.Organisation.UserControls.Address
{
	partial class ContactControl
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
			this.components = new System.ComponentModel.Container();
			this.ContactGuidDropEditControl = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.FaxLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EmailLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PhoneLabel = new Enterprise.ZArchitecture.ZLabel();
			this.contactDetailsForGUIBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.WebLabel = new Enterprise.ZArchitecture.ZLabel();
			this.contactDetailsForGUIBindingSource1 = new System.Windows.Forms.BindingSource(this.components);
			this.WebLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.contactDetailsForGUIBindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.contactDetailsForGUIBindingSource1)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ZAddressWithContact);
			// 
			// ContactGuidDropEditControl
			// 
			this.ContactGuidDropEditControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContactGuidDropEditControl, "ContactFK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ZAddressWithContact)(null)).ContactFK)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ContactGuidDropEditControl, false);
			this.ContactGuidDropEditControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ContactGuidDropEditControl.Name = "ContactGuidDropEditControl";
			this.ContactGuidDropEditControl.PreBoundMaxLength = 28;
			this.ContactGuidDropEditControl.ShowDescriptionBox = false;
			this.ContactGuidDropEditControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.ContactGuidDropEditControl.TabIndex = 1;
			// 
			// FaxLabel
			// 
			this.FaxLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FaxLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.FaxLabel, "ContactDetails_Fax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ZAddressWithContact)(null)).ContactDetails_Fax)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FaxLabel, false);
			this.FaxLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 39, true);
			this.FaxLabel.Name = "FaxLabel";
			this.FaxLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 13, true);
			this.FaxLabel.TabIndex = 13;
			this.FaxLabel.Text = "Fax";
			this.FaxLabel.UseMnemonic = false;
			// 
			// EmailLabel
			// 
			this.EmailLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.EmailLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.EmailLabel, "ContactDetails_Email");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ZAddressWithContact)(null)).ContactDetails_Email)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EmailLabel, false);
			this.EmailLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 52, true);
			this.EmailLabel.Name = "EmailLabel";
			this.EmailLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 13, true);
			this.EmailLabel.TabIndex = 14;
			this.EmailLabel.Text = "Email";
			this.EmailLabel.UseMnemonic = false;
			// 
			// PhoneLabel
			// 
			this.PhoneLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PhoneLabel, "ContactDetails_Phone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ZAddressWithContact)(null)).ContactDetails_Phone)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PhoneLabel, false);
			this.PhoneLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
			this.PhoneLabel.Name = "PhoneLabel";
			this.PhoneLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 13, true);
			this.PhoneLabel.TabIndex = 12;
			this.PhoneLabel.Text = "Phone / Mobile";
			this.PhoneLabel.UseMnemonic = false;
			// 
			// contactDetailsForGUIBindingSource
			// 
			this.contactDetailsForGUIBindingSource.DataSource = typeof(Enterprise.MasterFiles.Business.ContactDetailsForGUI);
			// 
			// WebLabel
			// 
			this.WebLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.WebLabel, "ContactDetails_Web");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ZAddressWithContact)(null)).ContactDetails_Web)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.WebLabel, false);
			this.WebLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 65, true);
			this.WebLabel.Name = "WebLabel";
			this.WebLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 13, true);
			this.WebLabel.TabIndex = 11;
			this.WebLabel.Text = "Web:";
			this.WebLabel.UseMnemonic = false;
			// 
			// contactDetailsForGUIBindingSource1
			// 
			this.contactDetailsForGUIBindingSource1.DataSource = typeof(Enterprise.MasterFiles.Business.ContactDetailsForGUI);
			// 
			// WebLinkLabel
			// 
			this.BindingSource.SetBindingMember(this.WebLinkLabel, "ContactDetails_WebLink");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ZAddressWithContact)(null)).ContactDetails_WebLink)));
			this.WebLinkLabel.IsFontBold = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.WebLinkLabel, false);
			this.WebLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(39, 65, true);
			this.WebLinkLabel.Name = "WebLinkLabel";
			this.WebLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 13, true);
			this.WebLinkLabel.TabIndex = 16;
			this.WebLinkLabel.Text = "www.cargowise.com";
			this.WebLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.WebLinkLabel_LinkClicked);
			// 
			// ContactControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.WebLinkLabel);
			this.Controls.Add(this.WebLabel);
			this.Controls.Add(this.EmailLabel);
			this.Controls.Add(this.FaxLabel);
			this.Controls.Add(this.PhoneLabel);
			this.Controls.Add(this.ContactGuidDropEditControl);
			this.Name = "ContactControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 86, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.contactDetailsForGUIBindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.contactDetailsForGUIBindingSource1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected ZArchitecture.GUI.ZGuidDropEdit ContactGuidDropEditControl;
		protected ZArchitecture.ZLabel FaxLabel;
		protected ZArchitecture.ZLabel EmailLabel;
		protected ZArchitecture.ZLabel PhoneLabel;
		protected ZArchitecture.ZLabel WebLabel;
		private System.Windows.Forms.BindingSource contactDetailsForGUIBindingSource;
		private System.Windows.Forms.BindingSource contactDetailsForGUIBindingSource1;
		protected ZArchitecture.GUI.ZLinkLabel WebLinkLabel;

	}
}
