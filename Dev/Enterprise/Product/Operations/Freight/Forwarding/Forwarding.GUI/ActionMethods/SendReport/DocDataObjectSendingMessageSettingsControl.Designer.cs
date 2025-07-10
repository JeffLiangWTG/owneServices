namespace Enterprise.Freight.Forwarding.GUI
{
	partial class DocDataObjectSendingMessageSettingsControl
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
			this.deErrorAction = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			lblErrorAction = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.GUI.DocDataObjectSendingMessageSettings);
			// 
			// lblErrorAction
			// 
			lblErrorAction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			  | System.Windows.Forms.AnchorStyles.Right)));
			lblErrorAction.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			lblErrorAction.Name = "lblErrorAction";
			lblErrorAction.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 22, true);
			lblErrorAction.TabIndex = 1;
			lblErrorAction.Text = "On Error:";
			// 
			// deErrorAction
			// 
			this.deErrorAction.AllowDrop = true;
			this.deErrorAction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.deErrorAction, "ErrorAction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.GUI.DocDataObjectSendingMessageSettings)(null)).ErrorAction)));
			this.deErrorAction.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 28, true);
			this.deErrorAction.Name = "deErrorAction";
			this.deErrorAction.PreBoundMaxLength = 3;
			this.deErrorAction.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.deErrorAction.TabIndex = 1;
			// 
			// SendReportSettingsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.deErrorAction);
			this.Controls.Add(lblErrorAction);
			this.Name = "DocDataObjectSendingMessageSettingsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 62, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel lblErrorAction;
		private ZArchitecture.GUI.ZDropEdit deErrorAction;
	}
}
