namespace Enterprise.Freight.Forwarding.GUI
{
	partial class PartialEventsInfoControl
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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				if (Extensions != null)
				{
					Extensions.Dispose();
				}
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
			this.detailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.totalTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.PartialEventsInfo);
			// 
			// detailsTextBox
			// 
			this.BindingSource.SetBindingMember(this.detailsTextBox, "Details");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.PartialEventsInfo)(null)).Details)));
			this.detailsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.detailsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.detailsTextBox, false);
			this.detailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.detailsTextBox.Multiline = true;
			this.detailsTextBox.Name = "detailsTextBox";
			this.detailsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.detailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 102, true);
			this.detailsTextBox.TabIndex = 0;
			// 
			// totalTextBox
			// 
			this.BindingSource.SetBindingMember(this.totalTextBox, "Total");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.PartialEventsInfo)(null)).Total)));
			this.totalTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.totalTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.totalTextBox, false);
			this.totalTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 102, true);
			this.totalTextBox.Name = "totalTextBox";
			this.totalTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 18, true);
			this.totalTextBox.TabIndex = 1;
			// 
			// PartialEventsInfoControl
			// 
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.detailsTextBox);
			this.Controls.Add(this.totalTextBox);
			this.Name = "PartialEventsInfoControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 120, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox detailsTextBox;
		private ZArchitecture.ZTextBox totalTextBox;
	}
}
