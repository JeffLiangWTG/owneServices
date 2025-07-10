using System;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class YesNoYesAllDialog : ZChildForm
	{
		public YesNoYesAllDialog()
		{
			InitializeComponent();
		}

		#region Click Event Handlers

		protected void YesButton_Click(object sender, EventArgs e)
		{
			Result = YesNoYesAllNoAllMessageBoxResult.Yes;
			Close();
		}

		protected void NoButton_Click(object sender, EventArgs e)
		{
			Result = YesNoYesAllNoAllMessageBoxResult.No;
			Close();
		}

		protected void YesToAllButton_Click(object sender, EventArgs e)
		{
			Result = YesNoYesAllNoAllMessageBoxResult.YesToAll;
			Close();
		}

		#endregion

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			if (MessageLabel.Bottom > YesButton.Top)
			{
				ControlDpiScalingHelper.SetHeight(this, this.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(40), false);
			}
		}

		public void SetMessage(string message)
		{
			this.MessageLabel.Text = message;
		}

		public void SetCaption(string caption)
		{
			this.Text = caption;
			this.Refresh();
		}

		public void SetYesToAllText(string text)
		{
			this.YesToAllButton.Text = text;
			this.Refresh();
		}

		public YesNoYesAllNoAllMessageBoxResult Result;
	}
}
