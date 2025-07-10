using CargoWise.Windows.UI;

namespace Enterprise.Customs.US.eManifest.GUI
{
	partial class ManifestUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		/// 
		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.ManifestTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.TripTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.tripUserControl1 = new Enterprise.Customs.US.eManifest.GUI.TripUserControl();
			this.splitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.allEquipmentUserControl1 = new Enterprise.Customs.US.eManifest.GUI.AllEquipmentUserControl();
			this.crewMembersUserControl1 = new Enterprise.Customs.US.eManifest.GUI.CrewMembersUserControl();
			this.ShipmentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ShipmentsUserControl = new Enterprise.Customs.US.eManifest.GUI.ShipmentsUserControl();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessagesUserControl = new Enterprise.Customs.US.eManifest.GUI.MessagesUserControl();
			this.StatusTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.StatusUserControl = new Enterprise.Customs.US.eManifest.GUI.StatusUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ManifestTabControl.SuspendLayout();
			this.TripTabPage.SuspendLayout();
			this.tripUserControl1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.allEquipmentUserControl1.SuspendLayout();
			this.crewMembersUserControl1.SuspendLayout();
			this.ShipmentsTabPage.SuspendLayout();
			this.ShipmentsUserControl.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.MessagesUserControl.SuspendLayout();
			this.StatusUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.eManifest.Business.Trip);
			// 
			// ManifestTabControl
			// 
			this.ManifestTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ManifestTabControl.Controls.Add(this.TripTabPage);
			this.ManifestTabControl.Controls.Add(this.ShipmentsTabPage);
			this.ManifestTabControl.Controls.Add(this.MessagesTabPage);
			this.ManifestTabControl.Controls.Add(this.StatusTabPage);
			this.ManifestTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ManifestTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ManifestTabControl.Name = "ManifestTabControl";
			this.ManifestTabControl.SelectedIndex = 0;
			this.ManifestTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 500, true);
			this.ManifestTabControl.TabIndex = 0;
			// 
			// TripTabPage
			// 
			this.TripTabPage.Controls.Add(this.tripUserControl1);
			this.TripTabPage.Controls.Add(this.splitContainer);
			this.TripTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TripTabPage.Name = "TripTabPage";
			this.TripTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TripTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 73, true);
			this.TripTabPage.TabIndex = 0;
			this.TripTabPage.Text = "Trip";
			// 
			// tripUserControl1
			// 
			this.tripUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.tripUserControl1, ".");
			this.tripUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.tripUserControl1.Name = "tripUserControl1";
			this.tripUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(845, 134, true);
			this.tripUserControl1.TabIndex = 0;
			// 
			// splitContainer
			// 
			this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 140, true);
			this.splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.allEquipmentUserControl1);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.crewMembersUserControl1);
			this.splitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(179, 353, true);
			this.splitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(67);
			this.splitContainer.TabIndex = 3;
			// 
			// allEquipmentUserControl1
			// 
			this.allEquipmentUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.allEquipmentUserControl1, ".");
			this.allEquipmentUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.allEquipmentUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.allEquipmentUserControl1.Name = "allEquipmentUserControl1";
			this.allEquipmentUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 353, true);
			this.allEquipmentUserControl1.TabIndex = 1;
			// 
			// crewMembersUserControl1
			// 
			this.crewMembersUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.crewMembersUserControl1, ".");
			this.crewMembersUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.crewMembersUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.crewMembersUserControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 320, true);
			this.crewMembersUserControl1.Name = "crewMembersUserControl1";
			this.crewMembersUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 353, true);
			this.crewMembersUserControl1.TabIndex = 2;
			// 
			// ShipmentsTabPage
			// 
			this.ShipmentsTabPage.Controls.Add(this.ShipmentsUserControl);
			this.ShipmentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ShipmentsTabPage.Name = "ShipmentsTabPage";
			this.ShipmentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ShipmentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 73, true);
			this.ShipmentsTabPage.TabIndex = 3;
			this.ShipmentsTabPage.Text = "Shipments";
			// 
			// ShipmentsUserControl
			// 
			this.ShipmentsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentsUserControl, ".");
			this.ShipmentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipmentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ShipmentsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 430, true);
			this.ShipmentsUserControl.Name = "ShipmentsUserControl";
			this.ShipmentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 430, true);
			this.ShipmentsUserControl.TabIndex = 0;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.Controls.Add(this.MessagesUserControl);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 73, true);
			this.MessagesTabPage.TabIndex = 4;
			this.MessagesTabPage.Text = "Messages";
			// 
			// MessagesUserControl
			// 
			this.MessagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessagesUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)))));
			this.MessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessagesUserControl.Name = "MessagesUserControl";
			this.MessagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 67, true);
			this.MessagesUserControl.TabIndex = 0;
			// 
			// StatusTabPage
			//
			this.StatusTabPage.Controls.Add(this.StatusUserControl);
			this.StatusTabPage.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("9845EAD1-EFE8-4BEF-A320-57CC87B2A289", "Status");
			this.StatusTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.StatusTabPage.Name = "StatusTabPage";
			this.StatusTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.StatusTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(857, 473, true);
			this.StatusTabPage.TabIndex = 5;
			this.StatusTabPage.UseVisualStyleBackColor = true;
			// 
			// StatusUserControl
			// 
			this.StatusUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)))));
			this.StatusUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatusUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.StatusUserControl.Name = "StatusUserControl";
			this.StatusUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(851, 467, true);
			this.StatusUserControl.TabIndex = 0;
			// 
			// ManifestUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ManifestTabControl);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 500, true);
			this.Name = "ManifestUserControl";
			this.Size = this.MinimumSize;
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ManifestTabControl.ResumeLayout(false);
			this.ManifestTabControl.PerformLayout();
			this.TripTabPage.ResumeLayout(false);
			this.TripTabPage.PerformLayout();
			this.tripUserControl1.ResumeLayout(true);
			this.tripUserControl1.PerformLayout();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.splitContainer.PerformLayout();
			this.allEquipmentUserControl1.ResumeLayout(true);
			this.allEquipmentUserControl1.PerformLayout();
			this.crewMembersUserControl1.ResumeLayout(true);
			this.crewMembersUserControl1.PerformLayout();
			this.ShipmentsTabPage.ResumeLayout(false);
			this.ShipmentsTabPage.PerformLayout();
			this.ShipmentsUserControl.ResumeLayout(true);
			this.ShipmentsUserControl.PerformLayout();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.MessagesUserControl.ResumeLayout(true);
			this.MessagesUserControl.PerformLayout();
			this.StatusUserControl.ResumeLayout(true);
			this.StatusUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabControl ManifestTabControl;
		private ZArchitecture.GUI.ZTabPage TripTabPage;
		private ZArchitecture.GUI.ZTabPage ShipmentsTabPage;
		private ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private Enterprise.Customs.US.eManifest.GUI.MessagesUserControl MessagesUserControl;
		private StatusUserControl StatusUserControl;
		private ShipmentsUserControl ShipmentsUserControl;
		private TripUserControl tripUserControl1;
		private CrewMembersUserControl crewMembersUserControl1;
		private AllEquipmentUserControl allEquipmentUserControl1;
		KSplitContainer splitContainer;
		private ZArchitecture.GUI.ZTabPage StatusTabPage;
	}
}
