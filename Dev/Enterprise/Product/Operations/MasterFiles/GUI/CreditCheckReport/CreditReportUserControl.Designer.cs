using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CreditReportUserControl
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
			components = new System.ComponentModel.Container();
			this.CreditCheckControl = new CreditCheckUserControl() { Dock = DockStyle.Fill };
			this.BackColor = Color.White;
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CreditCheckControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			//
			// CreditCheckControl
#if DEBUG
			TypeDescriptor.AddAttributes(this.CreditCheckControl, new SuppressDpiAwareBasherAttribute());
			this.CreditCheckControl.Controls.Cast<Control>().ForEach(o => TypeDescriptor.AddAttributes(o, new SuppressDpiAwareBasherAttribute()));
#endif
			this.CreditCheckControl.Name = "CreditCheckControl";
			// 
			// CreditReportUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(CreditCheckControl);
			this.Name = "CreditReportUserControl";
			this.Dock = System.Windows.Forms.DockStyle.Fill;
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CreditCheckControl.ResumeLayout(false);
			this.CreditCheckControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private CreditCheckUserControl CreditCheckControl;
	}
}
