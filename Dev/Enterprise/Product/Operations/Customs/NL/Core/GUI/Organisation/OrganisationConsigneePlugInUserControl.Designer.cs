using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI
{
	partial class OrganisationConsigneePlugInUserControl : ZUserControl
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
            this.NLPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.VatDefermentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.NLPanel.SuspendLayout();
            this.VatDefermentDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.Business.NLOrgImpAddInfo);
            // 
            // NLPanel
            // 
            this.NLPanel.Controls.Add(this.VatDefermentDropEdit);
            this.NLPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NLPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.NLPanel.Name = "NLPanel";
            this.NLPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.NLPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 163, true);
            this.NLPanel.TabIndex = 2;
            this.NLPanel.Text = "NL";
            // 
            // VatDefermentDropEdit
            // 
            this.VatDefermentDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.VatDefermentDropEdit, "ZO_VATDeferment");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NL.Business.NLOrgImpAddInfo)(null)).ZO_VATDeferment)));
            this.VatDefermentDropEdit.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("be3d732a-f90b-4bea-9b70-fb9cfe1eb064", "VAT Deferment");
            this.VatDefermentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 6, true);
            this.VatDefermentDropEdit.Name = "VatDefermentDropEdit";
            this.VatDefermentDropEdit.PreBoundMaxLength = 1;            
            this.VatDefermentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
            this.VatDefermentDropEdit.TabIndex = 5;
            // 
            // OrganisationConsigneePlugInUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.NLPanel);
            this.Name = "OrganisationConsigneePlugInUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 163, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.NLPanel.ResumeLayout(false);
            this.NLPanel.PerformLayout();
            this.VatDefermentDropEdit.ResumeLayout(true);
            this.VatDefermentDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZPanel NLPanel;
		ZDropEdit VatDefermentDropEdit;
	}
}
