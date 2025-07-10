
namespace Enterprise.Customs.US.GUI
{
	partial class LiquidationMessageUserControl
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
			this.components = new System.ComponentModel.Container();
			this.MessageTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.MessageTextTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageTabControl.SuspendLayout();
			this.MessageTextTabPage.SuspendLayout();
			this.MessageDetailsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.CusLiquidation);
			// 
			// MessageTabControl
			// 
			this.MessageTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MessageTabControl.Controls.Add(this.MessageTextTabPage);
			this.MessageTabControl.Controls.Add(this.MessageDetailsTabPage);
			this.MessageTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageTabControl.Name = "MessageTabControl";
			this.MessageTabControl.SelectedIndex = 0;
			this.MessageTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 497, true);
			this.MessageTabControl.TabIndex = 13;
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.Controls.Add(this.MessageTextTextBox);
			this.MessageTextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageTextTabPage.Name = "MessageTextTabPage";
			this.MessageTextTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 470, true);
			this.MessageTextTabPage.TabIndex = 0;
			this.MessageTextTabPage.Text = "Message Text";
			// 
			// MessageTextTextBox
			// 
			this.MessageTextTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.MessageTextTextBox, "Message.EM_FormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).Message.EM_FormattedMessageText)));
			this.MessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageTextTextBox.Multiline = true;
			this.MessageTextTextBox.Name = "MessageTextTextBox";
			this.MessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 464, true);
			this.MessageTextTextBox.TabIndex = 1;
			// 
			// MessageDetailsTabPage
			// 
			this.MessageDetailsTabPage.Controls.Add(this.MessageDetailsTextBox);
			this.MessageDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageDetailsTabPage.Name = "MessageDetailsTabPage";
			this.MessageDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 470, true);
			this.MessageDetailsTabPage.TabIndex = 1;
			this.MessageDetailsTabPage.Text = "Message Details";
			this.MessageDetailsTabPage.UseVisualStyleBackColor = true;
			// 
			// MessageDetailsTextBox
			// 
			this.MessageDetailsTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.MessageDetailsTextBox, "Message.EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).Message.EM_MessageInterpretation)));
			this.MessageDetailsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageDetailsTextBox.Multiline = true;
			this.MessageDetailsTextBox.Name = "MessageDetailsTextBox";
			this.MessageDetailsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 464, true);
			this.MessageDetailsTextBox.TabIndex = 2;
			// 
			// LiquidationMessageUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.MessageTabControl);
			this.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(507, 0, true);
			this.Name = "LiquidationMessageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 497, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageTabControl.ResumeLayout(false);
			this.MessageTextTabPage.ResumeLayout(false);
			this.MessageTextTabPage.PerformLayout();
			this.MessageDetailsTabPage.ResumeLayout(false);
			this.MessageDetailsTabPage.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZTabControl MessageTabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage MessageTextTabPage;
		protected Enterprise.ZArchitecture.ZTextBox MessageTextTextBox;
		private Enterprise.ZArchitecture.GUI.ZTabPage MessageDetailsTabPage;
		protected Enterprise.ZArchitecture.ZTextBox MessageDetailsTextBox;
	}
}
