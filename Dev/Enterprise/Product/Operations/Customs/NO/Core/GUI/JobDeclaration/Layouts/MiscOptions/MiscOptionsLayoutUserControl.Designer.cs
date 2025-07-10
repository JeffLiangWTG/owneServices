using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI
{
	partial class MiscOptionsLayoutUserControl
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
			this.RelatedDeclarationsUserControl = new Enterprise.Customs.NO.GUI.RelatedDeclarationsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RelatedDeclarationsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.JobDeclaration);
			//
			// RelatedDeclarations
			//
			this.RelatedDeclarationsUserControl.AllowDrop = true;
			this.RelatedDeclarationsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 271, true);
			this.BindingSource.SetBindingMember(this.RelatedDeclarationsUserControl, ".");
			this.RelatedDeclarationsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 193, true);
			this.RelatedDeclarationsUserControl.Name = "RelatedDeclarationsUserControl";
			this.RelatedDeclarationsUserControl.TabIndex = 5;
			// 
			// MiscOptionsLayoutUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.RelatedDeclarationsUserControl);
			this.Name = "MiscOptionsLayoutUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RelatedDeclarationsUserControl.ResumeLayout(true);
			this.RelatedDeclarationsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		public RelatedDeclarationsUserControl RelatedDeclarationsUserControl;
	}
}
