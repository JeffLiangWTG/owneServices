using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public class AcknowledgementMessageBox : ZMessageBox
	{
		ZCheckBox IAcknowledgeCheckBox;
		KLabel ConfirmationPromptLabel;

		public AcknowledgementMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, string promptText)
			: base(message, caption, buttons, icon, defaultButton)
		{
			InitializeComponent();
			AcceptButton = Button1;
			Button1.Enabled = false;
			ConfirmationPromptLabel.Text = promptText;
		}

		void InitializeComponent()
		{
			this.IAcknowledgeCheckBox = new ZCheckBox();
			this.ConfirmationPromptLabel = new KLabel();
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();

			this.IAcknowledgeCheckBox.CaptionResourceString = Res.GetData("55a1ce9e-9420-438a-b24d-7511c23ac316", "I acknowledge");
			this.IAcknowledgeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IAcknowledgeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 32, true);
			this.IAcknowledgeCheckBox.Name = "IAcknowledgeCheckbox";
			this.IAcknowledgeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 24, true);
			this.IAcknowledgeCheckBox.TabIndex = 0;
			this.IAcknowledgeCheckBox.UseVisualStyleBackColor = true;
			this.IAcknowledgeCheckBox.CheckedChanged += new EventHandler(this.IAcknowledgeCheckBox_CheckedChanged);

			this.ConfirmationPromptLabel.Font = new Font(OFont.NormalFontName, 8F);
			this.ConfirmationPromptLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 64, true);
			this.ConfirmationPromptLabel.Name = "ConfirmationPromptLabel";
			this.ConfirmationPromptLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 32, true);
			this.ConfirmationPromptLabel.TabIndex = 4;

			this.Controls.Add(this.IAcknowledgeCheckBox);
			this.Controls.Add(this.ConfirmationPromptLabel);
			this.Name = "AcknowledgementMessageBox";
			this.Controls.SetChildIndex(this.ConfirmationPromptLabel, 0);
			this.Controls.SetChildIndex(this.IAcknowledgeCheckBox, 0);
			this.Controls.SetChildIndex(this.TextBox, 0);
			this.Controls.SetChildIndex(this.PictureBox, 0);
			this.Controls.SetChildIndex(this.Button1, 0);
			this.Controls.SetChildIndex(this.Button2, 0);
			this.Controls.SetChildIndex(this.Button3, 0);
			this.CaptionRenderingEnabled = true;
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		void IAcknowledgeCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			Button1.Enabled = IAcknowledgeCheckBox.Checked;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ResizeConfirmationPromptLabel();
			UpdateHeightWidthSettings();
		}

		protected void UpdateHeightWidthSettings()
		{
			ConfirmationPromptLabel.Location = ControlDpiScalingHelper.NewScaledPoint(TextBox.Location.X, TextBox.Location.Y + TextBox.Height, false);

			var minWidthWhenTextBoxIsNotScrolling = Math.Max(Math.Max(TextBox.Width, ConfirmationPromptLabel.Width), IAcknowledgeCheckBox.Width) + PictureBox.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			var minWidthWhenTextBoxIsScrolling = TextBox.Right + Width - ClientRectangle.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(6);
			var width = Math.Max(minWidthWhenTextBoxIsNotScrolling, minWidthWhenTextBoxIsScrolling);
			if (Width < width)
			{
				var increasedWidth = width - Width;
				ControlDpiScalingHelper.SetWidth(this, width, false);
				Button1.Location = ControlDpiScalingHelper.NewScaledPoint(Button1.Location.X + (increasedWidth / 2), Button1.Location.Y, false);
				Button2.Location = ControlDpiScalingHelper.NewScaledPoint(Button2.Location.X + (increasedWidth / 2), Button2.Location.Y, false);
			}

			if (!string.IsNullOrEmpty(ConfirmationPromptLabel.Text))
			{
				IAcknowledgeCheckBox.Location = ControlDpiScalingHelper.NewScaledPoint(TextBox.Location.X, ConfirmationPromptLabel.Location.Y + ConfirmationPromptLabel.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(10), false);
			}
			else
			{
				IAcknowledgeCheckBox.Location = ControlDpiScalingHelper.NewScaledPoint(TextBox.Location.X, TextBox.Location.Y + TextBox.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(5), false);
			}

			ControlDpiScalingHelper.SetHeight(this, IAcknowledgeCheckBox.Location.Y + ControlDpiScalingHelper.ScaleToCurrentDpiY(95), false);
			Button1.Location = ControlDpiScalingHelper.NewScaledPoint(Button1.Location.X, IAcknowledgeCheckBox.Location.Y + ControlDpiScalingHelper.ScaleToCurrentDpiY(35), false);
			Button2.Location = ControlDpiScalingHelper.NewScaledPoint(Button2.Location.X, IAcknowledgeCheckBox.Location.Y + ControlDpiScalingHelper.ScaleToCurrentDpiY(35), false);
		}

		void ResizeConfirmationPromptLabel()
		{
			if (string.IsNullOrEmpty(ConfirmationPromptLabel.Text))
			{
				ControlDpiScalingHelper.SetWidth(ref ConfirmationPromptLabel, 0, true);
				ControlDpiScalingHelper.SetHeight(ref ConfirmationPromptLabel, 0, true);
			}
			else
			{
				ConfirmationPromptLabel.Size = TextRenderer.MeasureText(TextBox.CreateGraphics(), ConfirmationPromptLabel.Text, ConfirmationPromptLabel.Font, ControlDpiScalingHelper.NewScaledSize(MaxWidth, 0, false), TextFormatFlags.NoClipping);
			}
		}
	}
}
