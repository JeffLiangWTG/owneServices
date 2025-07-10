using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI
{
	partial class CustomsDeclarationSendingForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.MessageSendingEDocsUserControl = new Enterprise.Customs.PL.GUI.MessageSendingEDocsUserControl();
			this.SpecificDataUserControl = new Enterprise.Customs.PL.GUI.SpecificDataUserControl();
			this.AdditionalDataTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.MessageTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EDocsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.messageSendingObjectsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
			this.MessageSendingObjectsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageSendingEDocsUserControl.SuspendLayout();
			this.SpecificDataUserControl.SuspendLayout();
			this.AdditionalDataTabControl.SuspendLayout();
			this.MessageTabPage.SuspendLayout();
			this.EDocsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// AdditionalDataGroupBox
			// 
			this.AdditionalDataGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 321, true);
			this.AdditionalDataGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 23, true);
			// 
			// SendButton
			// 
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(649, 834, true);
			// 
			// CancelButton2
			// 
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(742, 834, true);
			// 
			// messageSendingObjectsGroupBox
			// 
			this.messageSendingObjectsGroupBox.Controls.Add(this.AdditionalDataTabControl);
			this.messageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(831, 307, true);
			// 
			// MessageSendingObjectsGrid
			// 
			this.MessageSendingObjectsGrid.Dock = System.Windows.Forms.DockStyle.Top;
			this.MessageSendingObjectsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 97, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 860, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(868, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.CustomsDeclarationMessageSendingObjectParent);
			// 
			// MessageSendingEDocsUserControl
			// 
			this.MessageSendingEDocsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageSendingEDocsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent)(((Enterprise.Customs.PL.Business.CustomsDeclarationMessageSendingObjectParent)(null)))));
			this.MessageSendingEDocsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageSendingEDocsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageSendingEDocsUserControl.Name = "MessageSendingEDocsUserControl";
			this.MessageSendingEDocsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 268, true);
			this.MessageSendingEDocsUserControl.TabIndex = 0;
			// 
			// SpecificDataUserControl
			// 
			this.SpecificDataUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SpecificDataUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent)(((Enterprise.Customs.PL.Business.CustomsDeclarationMessageSendingObjectParent)(null)))));
			this.SpecificDataUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 113, true);
			this.SpecificDataUserControl.Name = "SpecificDataUserControl";
			this.SpecificDataUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 153, true);
			this.SpecificDataUserControl.TabIndex = 1;
			// 
			// AdditionalDataTabControl
			// 
			this.AdditionalDataTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.AdditionalDataTabControl.Controls.Add(this.MessageTabPage);
			this.AdditionalDataTabControl.Controls.Add(this.EDocsTabPage);
			this.AdditionalDataTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalDataTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.AdditionalDataTabControl.Name = "AdditionalDataTabControl";
			this.AdditionalDataTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(827, 290, true);
			this.AdditionalDataTabControl.TabIndex = 0;
			// 
			// MessageTabPage
			//
			this.MessageTabPage.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("3945B94F-6F25-43A6-A8DE-59514FB71F0B", "Messages");
			this.MessageTabPage.Controls.Add(this.MessageSendingObjectsGrid);
			this.MessageTabPage.Controls.Add(this.SpecificDataUserControl);
			this.MessageTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MessageTabPage.Name = "MessageTabPage";
			this.MessageTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 268, true);
			this.MessageTabPage.TabIndex = 0;
			this.MessageTabPage.Controls.SetChildIndex(this.SpecificDataUserControl, 0);
			this.MessageTabPage.Controls.SetChildIndex(this.MessageSendingObjectsGrid, 0);
			// 
			// EDocsTabPage
			//
			this.EDocsTabPage.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("88D971B3-0DEA-41E0-96DA-D4284E78CA19", "EDocs");
			this.EDocsTabPage.Controls.Add(this.MessageSendingEDocsUserControl);
			this.EDocsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.EDocsTabPage.Name = "EDocsTabPage";
			this.EDocsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 268, true);
			this.EDocsTabPage.TabIndex = 1;
			// 
			// CustomsDeclarationSendingForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(868, 883, true);
			this.DataSourceType = typeof(Enterprise.Customs.PL.Business.CustomsDeclarationMessageSendingObjectParent);
			this.Name = "CustomsDeclarationSendingForm";
			this.messageSendingObjectsGroupBox.ResumeLayout(false);
			this.messageSendingObjectsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).EndInit();
			this.MessageSendingObjectsGrid.ResumeLayout(false);
			this.MessageSendingObjectsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageSendingEDocsUserControl.ResumeLayout(true);
			this.MessageSendingEDocsUserControl.PerformLayout();
			this.SpecificDataUserControl.ResumeLayout(true);
			this.SpecificDataUserControl.PerformLayout();
			this.AdditionalDataTabControl.ResumeLayout(false);
			this.AdditionalDataTabControl.PerformLayout();
			this.MessageTabPage.ResumeLayout(false);
			this.MessageTabPage.PerformLayout();
			this.EDocsTabPage.ResumeLayout(false);
			this.EDocsTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZTabControl AdditionalDataTabControl;
		internal ZArchitecture.GUI.ZTabPage MessageTabPage;
		internal ZArchitecture.GUI.ZTabPage EDocsTabPage;
		private MessageSendingEDocsUserControl MessageSendingEDocsUserControl;
		internal SpecificDataUserControl SpecificDataUserControl;
	}
}
