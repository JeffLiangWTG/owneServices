using Enterprise.ZArchitecture;

namespace Enterprise.Customs.PL.GUI
{
	partial class AcceptanceCommentUserControl
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
			this.AcceptanceCommentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AcceptanceCommentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.BaseMessageSendingObject);
			// 
			// AcceptanceCommentLabel
			// 
			this.AcceptanceCommentLabel.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("PLAcceptanceCommentUserControl|AcceptanceCommentLabel", "Acceptance Comment");
			this.AcceptanceCommentLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AcceptanceCommentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 3, true);
			this.AcceptanceCommentLabel.Name = "AcceptanceCommentLabel";
			this.AcceptanceCommentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 18, true);
			this.AcceptanceCommentLabel.TabIndex = 1;
			this.AcceptanceCommentLabel.UseMnemonic = false;
			// 
			// AcceptanceCommentTextBox
			// 
			this.AcceptanceCommentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AcceptanceCommentTextBox, "AcceptanceComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.BaseMessageSendingObject)(null)).AcceptanceComment)));
			this.AcceptanceCommentTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AcceptanceCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
			this.AcceptanceCommentTextBox.Multiline = true;
			this.AcceptanceCommentTextBox.Name = "AcceptanceCommentTextBox";
			this.AcceptanceCommentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.AcceptanceCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 94, true);
			this.AcceptanceCommentTextBox.TabIndex = 2;
			// 
			// AcceptanceCommentUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AcceptanceCommentLabel);
			this.Controls.Add(this.AcceptanceCommentTextBox);
			this.Name = "AcceptanceCommentUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 120, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZLabel AcceptanceCommentLabel;
		internal ZTextBox AcceptanceCommentTextBox;
	}
}
