using System.Windows.Forms;

namespace Enterprise.Customs.TR.GUI
{
	partial class FTPSettingsRegistryItemUserControl
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
			this.FTPAddressDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FTPPortCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.FTPInTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FTPOutTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExportUnionUserCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExportUnionUserPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExportUnionPaymentPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.FTPSettings);
			// 
			// FTPAddressDropEdit
			// 
			this.BindingSource.SetBindingMember(this.FTPAddressDropEdit, "FTPAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.FTPSettings)(null)).FTPAddress)));
			this.FTPAddressDropEdit.Name = "FTPAddressDropEdit";
			this.FTPAddressDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 0, true);
			this.FTPAddressDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 40, true);
			this.FTPAddressDropEdit.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			// 
			// FTPPortCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.FTPPortCalcEdit, "Port");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.FTPSettings)(null)).Port)));
			this.FTPPortCalcEdit.DecimalPlaces = 0;
			this.FTPPortCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 25, true);
			this.FTPPortCalcEdit.Name = "FTPPortCalcEdit";
			this.FTPPortCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 40, true);
			this.FTPPortCalcEdit.TabIndex = 1;
			this.FTPPortCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.FTPPortCalcEdit.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			// 
			// FTPInTextBox
			// 
			this.BindingSource.SetBindingMember(this.FTPInTextBox, "Inbox");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.FTPSettings)(null)).Inbox)));
			this.FTPInTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FTPInTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 50, true);
			this.FTPInTextBox.Name = "FTPInTextBox";
			this.FTPInTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 40, true);
			this.FTPInTextBox.TabIndex = 2;
			this.FTPInTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			// 
			// FTPOutTextBox
			// 
			this.BindingSource.SetBindingMember(this.FTPOutTextBox, "Outbox");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.FTPSettings)(null)).Outbox)));
			this.FTPOutTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FTPOutTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 75, true);
			this.FTPOutTextBox.Name = "FTPOutTextBox";
			this.FTPOutTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 40, true);
			this.FTPOutTextBox.TabIndex = 3;
			this.FTPOutTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			// 
			// ExportUnionUserCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportUnionUserCodeTextBox, "ExportUnionUserCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.FTPSettings)(null)).ExportUnionUserCode)));
			this.ExportUnionUserCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ExportUnionUserCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 100, true);
			this.ExportUnionUserCodeTextBox.Name = "ExportUnionUserCodeTextBox";
			this.ExportUnionUserCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 40, true);
			this.ExportUnionUserCodeTextBox.TabIndex = 4;
			this.ExportUnionUserCodeTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			// 
			// ExportUnionUserPasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportUnionUserPasswordTextBox, "ExportUnionUserPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.FTPSettings)(null)).ExportUnionUserPassword)));
			this.ExportUnionUserPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ExportUnionUserPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 125, true);
			this.ExportUnionUserPasswordTextBox.Name = "ExportUnionUserPasswordTextBox";
			this.ExportUnionUserPasswordTextBox.PasswordChar = '*';
			this.ExportUnionUserPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 40, true);
			this.ExportUnionUserPasswordTextBox.TabIndex = 5;
			this.ExportUnionUserPasswordTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			// 
			// ExportUnionPaymentPasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportUnionPaymentPasswordTextBox, "ExportUnionPaymentPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.FTPSettings)(null)).ExportUnionPaymentPassword)));
			this.ExportUnionPaymentPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ExportUnionPaymentPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 150, true);
			this.ExportUnionPaymentPasswordTextBox.Name = "ExportUnionPaymentPasswordTextBox";
			this.ExportUnionPaymentPasswordTextBox.PasswordChar = '*';
			this.ExportUnionPaymentPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 40, true);
			this.ExportUnionPaymentPasswordTextBox.TabIndex = 6;
			this.ExportUnionPaymentPasswordTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			// 
			// FTPSettingsRegistryItemUserControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FTPAddressDropEdit);
			this.Controls.Add(this.FTPPortCalcEdit);
			this.Controls.Add(this.FTPInTextBox);
			this.Controls.Add(this.FTPOutTextBox);
			this.Controls.Add(this.ExportUnionUserCodeTextBox);
			this.Controls.Add(this.ExportUnionUserPasswordTextBox);
			this.Controls.Add(this.ExportUnionPaymentPasswordTextBox);
			this.Name = "FTPSettingsRegistryItemUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 250, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZDropEdit FTPAddressDropEdit;
		public Enterprise.ZArchitecture.ZCalcEdit FTPPortCalcEdit;
		public Enterprise.ZArchitecture.ZTextBox FTPInTextBox;
		public Enterprise.ZArchitecture.ZTextBox FTPOutTextBox;
		public Enterprise.ZArchitecture.ZTextBox ExportUnionUserCodeTextBox;
		public Enterprise.ZArchitecture.ZTextBox ExportUnionUserPasswordTextBox;
		public Enterprise.ZArchitecture.ZTextBox ExportUnionPaymentPasswordTextBox;
	}
}
