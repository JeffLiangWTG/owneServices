namespace Enterprise.Customs.PL.GUI
{
	partial class MessageSendingEntryLinesGridUserControl
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
			this.EntryLinesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EntryLinesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EntryLinesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLinesGrid)).BeginInit();
			this.EntryLinesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.RetrospectiveQuotaRequestMessageSendingObjectParent);
			// 
			// EntryLinesGroupBox
			// 
			this.EntryLinesGroupBox.Controls.Add(this.EntryLinesGrid);
			this.EntryLinesGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EntryLinesGroupBox, false);
			this.EntryLinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryLinesGroupBox.Name = "EntryLinesGroupBox";
			this.EntryLinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 105, true);
			this.EntryLinesGroupBox.TabIndex = 0;
			this.EntryLinesGroupBox.TabStop = false;
			// 
			// EntryLinesGrid
			// 
			this.EntryLinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryLinesGrid, "EntryLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.PL.Business.RetrospectiveQuotaRequestMessageSendingObjectParent)(null)).EntryLines)));
			this.EntryLinesGrid.CaptionVisible = false;
			this.EntryLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryLinesGrid.GridId = "D18047F5-31C6-4975-9F93-2EF440F54F47";
			this.EntryLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryLinesGrid.LayoutKey = "EntryLinesGrid";
			this.EntryLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
			this.EntryLinesGrid.Name = "EntryLinesGrid";
			this.EntryLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 90, true);
			this.EntryLinesGrid.TabIndex = 0;
			// 
			// MessageSendingEntryLinesGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EntryLinesGroupBox);
			this.Name = "MessageSendingEntryLinesGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 206, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EntryLinesGroupBox.ResumeLayout(false);
			this.EntryLinesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLinesGrid)).EndInit();
			this.EntryLinesGrid.ResumeLayout(false);
			this.EntryLinesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox EntryLinesGroupBox;
		internal ZArchitecture.ZGrid EntryLinesGrid;
	}
}
