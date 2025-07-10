using System.Windows.Forms;

namespace Enterprise.MasterFiles.GUI
{
	partial class GlbPersonAddressControl
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
				cancellationToken.Dispose();
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
			this.CountryFindBox = new Enterprise.MasterFiles.GUI.Internal.ZCodeFindBoxFixedPreBoundMaxLength();
			this.ValidateAddressButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.StateBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.PostCodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CityBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Address2BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Address1BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClearFieldsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CountryFindBox.SuspendLayout();
			this.StateBoundDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbPerson);
			// 
			// CountryFindBox
			// 
			this.CountryFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryFindBox, "PER_RN_NKCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_RN_NKCountry)));
			this.CountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 45, true);
			this.CountryFindBox.Name = "CountryFindBox";
			this.CountryFindBox.PreBoundMaxLength = 3;
			this.CountryFindBox.ShouldResize = false;
			this.CountryFindBox.ShowDescriptionBox = false;
			this.CountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 17, true);
			this.CountryFindBox.TabIndex = 41;
			// 
			// ValidateAddressButton
			// 
			this.ValidateAddressButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ValidateAddressButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 2, true);
			this.ValidateAddressButton.Name = "ValidateAddressButton";
			this.ValidateAddressButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ValidateAddressButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 22, true);
			this.ValidateAddressButton.TabIndex = 46;
			this.ValidateAddressButton.Text = " ";
			this.ValidateAddressButton.ToolTipCaption = null;
			this.ValidateAddressButton.UseVisualStyleBackColor = true;
			this.ValidateAddressButton.Click += new System.EventHandler(this.ValidateAddressButton_Click);
			// 
			// StateBoundDropEdit
			// 
			this.StateBoundDropEdit.AllowDrop = true;
			this.StateBoundDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.StateBoundDropEdit, "PER_State");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_State)));
			this.StateBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(187, 66, true);
			this.StateBoundDropEdit.Name = "StateBoundDropEdit";
			this.StateBoundDropEdit.PreBoundMaxLength = 4;
			this.StateBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 17, true);
			this.StateBoundDropEdit.TabIndex = 44;
			// 
			// PostCodeBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.PostCodeBoundTextBox, "PER_Postcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_Postcode)));
			this.PostCodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 66, true);
			this.PostCodeBoundTextBox.Name = "PostCodeBoundTextBox";
			this.PostCodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.PostCodeBoundTextBox.TabIndex = 42;
			// 
			// CityBoundTextBox
			// 
			this.CityBoundTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CityBoundTextBox, "PER_City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_City)));
			this.CityBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CityBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(187, 45, true);
			this.CityBoundTextBox.Name = "CityBoundTextBox";
			this.CityBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 17, true);
			this.CityBoundTextBox.TabIndex = 43;
			// 
			// Address2BoundTextBox
			// 
			this.Address2BoundTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.Address2BoundTextBox, "PER_HomeAddress2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_HomeAddress2)));
			this.Address2BoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Address2BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 23, true);
			this.Address2BoundTextBox.Name = "Address2BoundTextBox";
			this.Address2BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 17, true);
			this.Address2BoundTextBox.TabIndex = 40;
			// 
			// Address1BoundTextBox
			// 
			this.Address1BoundTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.Address1BoundTextBox, "PER_HomeAddress1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_HomeAddress1)));
			this.Address1BoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Address1BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 2, true);
			this.Address1BoundTextBox.Name = "Address1BoundTextBox";
			this.Address1BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 17, true);
			this.Address1BoundTextBox.TabIndex = 39;
			// 
			// ClearFieldsButton
			// 
			this.ClearFieldsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ClearFieldsButton.Image = global::Enterprise.MasterFiles.GUI.Properties.Resources.xIcon;
			this.ClearFieldsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 2, true);
			this.ClearFieldsButton.Name = "ClearFieldsButton";
			this.ClearFieldsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ClearFieldsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(19, 18, true);
			this.ClearFieldsButton.TabIndex = 45;
			this.ClearFieldsButton.TabStop = false;
			this.ClearFieldsButton.ToolTipCaption = null;
			this.ClearFieldsButton.UseVisualStyleBackColor = true;
			this.ClearFieldsButton.Click += new System.EventHandler(this.ClearFieldsButton_Click);
			// 
			// GlbPersonAddressControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CountryFindBox);
			this.Controls.Add(this.ValidateAddressButton);
			this.Controls.Add(this.StateBoundDropEdit);
			this.Controls.Add(this.PostCodeBoundTextBox);
			this.Controls.Add(this.CityBoundTextBox);
			this.Controls.Add(this.Address2BoundTextBox);
			this.Controls.Add(this.Address1BoundTextBox);
			this.Controls.Add(this.ClearFieldsButton);
			this.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.Name = "GlbPersonAddressControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 87, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CountryFindBox.ResumeLayout(true);
			this.CountryFindBox.PerformLayout();
			this.StateBoundDropEdit.ResumeLayout(true);
			this.StateBoundDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private Internal.ZCodeFindBoxFixedPreBoundMaxLength CountryFindBox;
		protected ZArchitecture.GUI.ZButton ValidateAddressButton;
		protected ZArchitecture.GUI.ZDropEditWithFixedWidth StateBoundDropEdit;
		protected ZArchitecture.ZTextBox PostCodeBoundTextBox;
		protected ZArchitecture.ZTextBox CityBoundTextBox;
		protected ZArchitecture.ZTextBox Address2BoundTextBox;
		protected ZArchitecture.ZTextBox Address1BoundTextBox;
		protected ZArchitecture.GUI.ZButton ClearFieldsButton;
	}
}
