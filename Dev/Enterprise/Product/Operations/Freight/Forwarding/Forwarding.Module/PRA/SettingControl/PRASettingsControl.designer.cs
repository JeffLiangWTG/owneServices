namespace Enterprise.Freight.Forwarding.Module
{
	partial class PRASettingsControl
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
			Enterprise.ZArchitecture.ZLabel lblErrorBehaviour;
			this.deErrorBehaviour = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			lblErrorBehaviour = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.deErrorBehaviour.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(PRASettings);
			// 
			// lblErrorBehaviour
			// 
			lblErrorBehaviour.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			lblErrorBehaviour.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			lblErrorBehaviour.Name = "lblErrorBehaviour";
			lblErrorBehaviour.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 22, true);
			lblErrorBehaviour.TabIndex = 1;
			lblErrorBehaviour.Text = "On Error / Message Error:";
			// 
			// deErrorBehaviour
			// 
			this.deErrorBehaviour.AllowDrop = true;
			this.deErrorBehaviour.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.deErrorBehaviour, "ErrorBehaviour");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PRASettings)(null)).ErrorBehaviour)));
			this.deErrorBehaviour.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 28, true);
			this.deErrorBehaviour.Name = "deErrorBehaviour";
			this.deErrorBehaviour.PreBoundMaxLength = 3;
			this.deErrorBehaviour.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.deErrorBehaviour.TabIndex = 1;
			// 
			// PRASettingsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.deErrorBehaviour);
			this.Controls.Add(lblErrorBehaviour);
			this.Name = "PRASettingsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 62, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.deErrorBehaviour.ResumeLayout(true);
			this.deErrorBehaviour.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit deErrorBehaviour;
	}
}
