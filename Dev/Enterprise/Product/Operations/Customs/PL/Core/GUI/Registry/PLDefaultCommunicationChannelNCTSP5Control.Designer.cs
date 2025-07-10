namespace Enterprise.Customs.PL.GUI.Registry
{
	partial class PLDefaultCommunicationChannelNCTSP5Control
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
			this.emailChannelRadioButton1 = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.seapIDRadioButton2 = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.PLDefaultCommunicationChannelNCTSP5);
			// 
			// emailChannelRadioButton1
			// 
			this.emailChannelRadioButton1.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.emailChannelRadioButton1, "IsEmailChannel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.PL.Business.PLDefaultCommunicationChannelNCTSP5)(null)).IsEmailChannel)));
			this.emailChannelRadioButton1.Checked = true;
			this.emailChannelRadioButton1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 60, true);
			this.emailChannelRadioButton1.Name = "emailChannelRadioButton1";
			this.emailChannelRadioButton1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.emailChannelRadioButton1.TabIndex = 0;
			this.emailChannelRadioButton1.TabStop = true;
			this.emailChannelRadioButton1.UseVisualStyleBackColor = true;
			// 
			// seapIDRadioButton2
			// 
			this.seapIDRadioButton2.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.seapIDRadioButton2, "IsSeapID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.PL.Business.PLDefaultCommunicationChannelNCTSP5)(null)).IsSeapID)));
			this.seapIDRadioButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 59, true);
			this.seapIDRadioButton2.Name = "seapIDRadioButton2";
			this.seapIDRadioButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.seapIDRadioButton2.TabIndex = 1;
			this.seapIDRadioButton2.TabStop = true;
			this.seapIDRadioButton2.UseVisualStyleBackColor = true;
			// 
			// PLDefaultCommunicationChannelNCTSP5Control
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.seapIDRadioButton2);
			this.Controls.Add(this.emailChannelRadioButton1);
			this.Name = "PLDefaultCommunicationChannelNCTSP5Control";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZRadioButton emailChannelRadioButton1;
		private ZArchitecture.GUI.ZRadioButton seapIDRadioButton2;
	}
}
