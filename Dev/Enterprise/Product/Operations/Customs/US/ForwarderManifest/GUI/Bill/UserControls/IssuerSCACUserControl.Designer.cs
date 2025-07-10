using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	partial class IssuerSCACUserControl
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
			this.IssuerSCACCodeFindBox = new ZCodeFindBox();
			this.IssuerSCACLabel = new ZLabel();
			this.MasterBOLTextBox = new ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.IssuerSCACGroupBox = new ZGroupBox();
			this.IssuerSCACCodeFindBox.SuspendLayout();
			this.IssuerSCACLabel.SuspendLayout();
			this.MasterBOLTextBox.SuspendLayout();
			this.IssuerSCACGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// IssuerSCACCodeFindBox
			// 
			this.IssuerSCACCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IssuerSCACCodeFindBox, "MasterBill+ABL_BillIssuer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((USExportAsycudaManifestHeader)(null)).MasterBill.ABL_BillIssuer)));
			this.IssuerSCACCodeFindBox.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("2109DE4E-451B-4F14-A616-5D8765B8A8EB", "Issuer SCAC");
			this.IssuerSCACCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 10, true);
			this.IssuerSCACCodeFindBox.Name = "IssuerSCACCodeFindBox";
			this.IssuerSCACCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Carrier;
			this.IssuerSCACCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.IssuerSCACCodeFindBox.ParentType = null;
			this.IssuerSCACCodeFindBox.PreBoundMaxLength = 4;
			this.IssuerSCACCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 20, true);
			this.IssuerSCACCodeFindBox.TabIndex = 18;
			// 
			// MasterBOLTextBox
			// 
			this.BindingSource.SetBindingMember(this.MasterBOLTextBox, "MasterBOL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((USExportAsycudaManifestHeader)(null)).MasterBOL)));
			this.MasterBOLTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 35, true);
			this.MasterBOLTextBox.Name = "MasterBOLTextBox";
			this.MasterBOLTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.MasterBOLTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.MasterBOLTextBox.TabIndex = 19;
			//
			// IssuerSCACLabel
			//
			this.IssuerSCACLabel.AutoSize = true;
			this.IssuerSCACLabel.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("743BF57B-E929-4BF1-B865-94B6732867E8", "Complete this section only if attaching House Bills\r\nIf filing Simple/Direct Bills, leave this section blank");
			this.IssuerSCACLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.IssuerSCACLabel.IsFontBold = true;
			this.IssuerSCACLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 10, true);
			this.IssuerSCACLabel.Name = "IssuerSCACLabel";
			this.IssuerSCACLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 40, true);
			this.IssuerSCACLabel.TabIndex = 0;
			this.IssuerSCACLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			//
			// IssuerSCACGroupBox
			//
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.IssuerSCACGroupBox, false);
			this.IssuerSCACGroupBox.Controls.Add(this.IssuerSCACCodeFindBox);
			this.IssuerSCACGroupBox.Controls.Add(this.IssuerSCACLabel);
			this.IssuerSCACGroupBox.Controls.Add(this.MasterBOLTextBox);
			this.IssuerSCACGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IssuerSCACGroupBox.Name = "IssuerSCACGroupBox";
			this.IssuerSCACGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 60, true);
			this.IssuerSCACGroupBox.TabIndex = 3;
			this.IssuerSCACGroupBox.TabStop = false;
			//
			// IssuerSCACUserControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IssuerSCACGroupBox);
			this.Name = "IssuerSCACUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.IssuerSCACCodeFindBox.ResumeLayout(true);
			this.IssuerSCACCodeFindBox.PerformLayout();
			this.IssuerSCACLabel.ResumeLayout(true);
			this.IssuerSCACLabel.PerformLayout();
			this.MasterBOLTextBox.ResumeLayout(true);
			this.MasterBOLTextBox.PerformLayout();
			this.IssuerSCACGroupBox.ResumeLayout(true);
			this.IssuerSCACGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZCodeFindBox IssuerSCACCodeFindBox;
		internal ZGroupBox IssuerSCACGroupBox;
		internal ZLabel IssuerSCACLabel;
		internal ZTextBox MasterBOLTextBox;
	}
}
