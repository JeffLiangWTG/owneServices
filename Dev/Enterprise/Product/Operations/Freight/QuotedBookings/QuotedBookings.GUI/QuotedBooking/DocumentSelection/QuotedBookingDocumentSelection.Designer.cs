using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	partial class QuotedBookingDocumentSelection
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
			this.documentSelectionControl1 = new Enterprise.Rating.GUI.DocumentSelectionControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Rating.Business.Quote);
			// 
			// documentSelectionControl1
			// 
			this.BindingSource.SetBindingMember(this.documentSelectionControl1, ".");
			this.documentSelectionControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.documentSelectionControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.documentSelectionControl1.Name = "documentSelectionControl1";
			this.documentSelectionControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 486, true);
			this.documentSelectionControl1.TabIndex = 1;
			// 
			// QuotedBookingDocumentSelection
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.documentSelectionControl1);
			this.Name = "QuotedBookingDocumentSelection";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 486, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.Rating.GUI.DocumentSelectionControl documentSelectionControl1;

	}
}
