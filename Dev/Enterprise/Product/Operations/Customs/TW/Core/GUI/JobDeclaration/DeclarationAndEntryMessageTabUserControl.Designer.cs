
namespace Enterprise.Customs.TW.GUI
{
	partial class DeclarationAndEntryMessageTabUserControl
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
			this.EntryDeclarationTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.EntryTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EntryMessagesTabUserControl = new Enterprise.Customs.TW.GUI.MessagesTabUserControl();
			this.DeclarationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DiscardedMessageUserControl = new Enterprise.Customs.TW.GUI.DiscardedMessageUserControl();
			this.LicensingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LicensingMessageHeaderUserControl = new Enterprise.Customs.TW.GUI.LicensingMessageHeaderUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EntryDeclarationTabControl.SuspendLayout();
			this.EntryTabPage.SuspendLayout();
			this.EntryMessagesTabUserControl.SuspendLayout();
			this.DeclarationTabPage.SuspendLayout();
			this.DiscardedMessageUserControl.SuspendLayout();
			this.LicensingTabPage.SuspendLayout();
			this.LicensingMessageHeaderUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclaration);
			// 
			// EntryDeclarationTabControl
			// 
			this.EntryDeclarationTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.EntryDeclarationTabControl.Controls.Add(this.EntryTabPage);
			this.EntryDeclarationTabControl.Controls.Add(this.DeclarationTabPage);
			this.EntryDeclarationTabControl.Controls.Add(this.LicensingTabPage);
			this.EntryDeclarationTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryDeclarationTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryDeclarationTabControl.Name = "EntryDeclarationTabControl";
			this.EntryDeclarationTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 507, true);
			this.EntryDeclarationTabControl.TabIndex = 0;
			// 
			// EntryTabPage
			// 
			this.EntryTabPage.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("164d1e39-45e3-4b82-b25d-999f0e30e263", "Entry");
			this.EntryTabPage.Controls.Add(this.EntryMessagesTabUserControl);
			this.EntryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EntryTabPage.Name = "EntryTabPage";
			this.EntryTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.EntryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(602, 280, true);
			this.EntryTabPage.TabIndex = 0;
			this.EntryTabPage.UseVisualStyleBackColor = true;
			// 
			// EntryMessagesTabUserControl
			// 
			this.EntryMessagesTabUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryMessagesTabUserControl, "CustomsEntryHeaders.Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.EDIMessageCollection)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).Messages)));
			this.EntryMessagesTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryMessagesTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EntryMessagesTabUserControl.Name = "EntryMessagesTabUserControl";
			this.EntryMessagesTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(596, 274, true);
			this.EntryMessagesTabUserControl.TabIndex = 0;
			// 
			// DeclarationTabPage
			// 
			this.DeclarationTabPage.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("18f2545f-4fb1-42ad-86b7-2a415dd1c420", "Declaration");
			this.DeclarationTabPage.Controls.Add(this.DiscardedMessageUserControl);
			this.DeclarationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DeclarationTabPage.Name = "DeclarationTabPage";
			this.DeclarationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DeclarationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(602, 280, true);
			this.DeclarationTabPage.TabIndex = 1;
			this.DeclarationTabPage.UseVisualStyleBackColor = true;
			// 
			// DiscardedMessageUserControl
			// 
			this.DiscardedMessageUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DiscardedMessageUserControl, ".");
			this.DiscardedMessageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DiscardedMessageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DiscardedMessageUserControl.Name = "DiscardedMessageUserControl";
			this.DiscardedMessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(596, 274, true);
			this.DiscardedMessageUserControl.TabIndex = 8;
			// 
			// LicensingTabPage
			// 
			this.LicensingTabPage.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("e05b9995-16ba-4113-8340-59ea9e96f338", "Licensing");
			this.LicensingTabPage.Controls.Add(this.LicensingMessageHeaderUserControl);
			this.LicensingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LicensingTabPage.Name = "LicensingTabPage";
			this.LicensingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(802, 480, true);
			this.LicensingTabPage.TabIndex = 2;
			this.LicensingTabPage.UseVisualStyleBackColor = true;
			// 
			// LicensingMessageHeaderUserControl
			// 
			this.LicensingMessageHeaderUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LicensingMessageHeaderUserControl, "CustomsEntryInstructions.ControllingMessageHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.TW.Business.CusTWControllingMessageHeader)(((Enterprise.Customs.TW.Business.CusTWControllingMessageHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ControllingMessageHeaders)).SyncRoot)))));
			this.LicensingMessageHeaderUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LicensingMessageHeaderUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LicensingMessageHeaderUserControl.Name = "LicensingMessageHeaderUserControl";
			this.LicensingMessageHeaderUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(802, 480, true);
			this.LicensingMessageHeaderUserControl.TabIndex = 0;
			// 
			// DeclarationAndEntryMessageTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EntryDeclarationTabControl);
			this.Name = "DeclarationAndEntryMessageTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 507, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EntryDeclarationTabControl.ResumeLayout(false);
			this.EntryDeclarationTabControl.PerformLayout();
			this.EntryTabPage.ResumeLayout(false);
			this.EntryTabPage.PerformLayout();
			this.EntryMessagesTabUserControl.ResumeLayout(true);
			this.EntryMessagesTabUserControl.PerformLayout();
			this.DeclarationTabPage.ResumeLayout(false);
			this.DeclarationTabPage.PerformLayout();
			this.DiscardedMessageUserControl.ResumeLayout(true);
			this.DiscardedMessageUserControl.PerformLayout();
			this.LicensingTabPage.ResumeLayout(false);
			this.LicensingTabPage.PerformLayout();
			this.LicensingMessageHeaderUserControl.ResumeLayout(true);
			this.LicensingMessageHeaderUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabControl EntryDeclarationTabControl;
		private ZArchitecture.GUI.ZTabPage EntryTabPage;
		private ZArchitecture.GUI.ZTabPage DeclarationTabPage;
		private MessagesTabUserControl EntryMessagesTabUserControl;
		private DiscardedMessageUserControl DiscardedMessageUserControl;
		private LicensingMessageHeaderUserControl LicensingMessageHeaderUserControl;
		private ZArchitecture.GUI.ZTabPage LicensingTabPage;
	}
}
