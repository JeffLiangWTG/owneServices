namespace Enterprise.Customs.US.eManifest.GUI
{
	partial class PartyUserControl
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
			this.PartyAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.PartyTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PartyDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PartyDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.eManifest.Business.Party);
			// 
			// PartyAddressControl
			// 
			this.PartyAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PartyAddressControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.US.eManifest.Business.Party)(null)))));
			this.PartyAddressControl.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PartyAddressControl, false);
			this.PartyAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 33, true);
			this.PartyAddressControl.Name = "PartyAddressControl";
			this.PartyAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.PartyAddressControl.TabIndex = 1;
			// 
			// PartyTypeDropEdit
			// 
			this.PartyTypeDropEdit.AllowDrop = true;
			this.PartyTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PartyTypeDropEdit, "E2_AddressType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.Party)(null)).E2_AddressType)));
			this.PartyTypeDropEdit.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PartyTypeDropEdit, false);
			this.PartyTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.PartyTypeDropEdit.Name = "PartyTypeDropEdit";
			this.PartyTypeDropEdit.PreBoundMaxLength = 3;
			this.PartyTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 20, true);
			this.PartyTypeDropEdit.TabIndex = 0;
			// 
			// PartyDetailsGroupBox
			// 
			this.PartyDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("PartyUserControl|2535cb20-97e5-4bb2-ad1c-cf15d98a0256", "Party Details");
			this.PartyDetailsGroupBox.Controls.Add(this.PartyTypeDropEdit);
			this.PartyDetailsGroupBox.Controls.Add(this.PartyAddressControl);
			this.PartyDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PartyDetailsGroupBox.Name = "PartyDetailsGroupBox";
			this.PartyDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 217, true);
			this.PartyDetailsGroupBox.TabIndex = 0;
			this.PartyDetailsGroupBox.TabStop = false;
			// 
			// PartyUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PartyDetailsGroupBox);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 217, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 217, true);
			this.Name = "PartyUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 217, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PartyDetailsGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private MasterFiles.GUI.ZDocAddressControl PartyAddressControl;
		private ZArchitecture.GUI.ZDropEdit PartyTypeDropEdit;
		private ZArchitecture.GUI.ZGroupBox PartyDetailsGroupBox;
	}
}
