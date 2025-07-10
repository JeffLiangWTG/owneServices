using System;
using System.Windows.Forms;

using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.GUI
{
	public partial class FrontPageUserControl : BaseCustomsEntryUserControl
	{
		#region Auto

		private readonly System.ComponentModel.Container components;

		#endregion
		public ZArchitecture.ZTextBox ExportDeclarationNumberBoundTextBox;
		public ZGroupBox DeclarationDetailsGroupBox;
		public ZArchitecture.ZTextBox StatusTextBox;

		public FrontPageUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			BaseJobDeclarationForm parentForm = FindForm() as BaseJobDeclarationForm;
			if (parentForm != null)
			{
				parentForm.Shown += new EventHandler(ParentForm_Shown);
			}
		}

		private void ParentForm_Shown(object sender, EventArgs e)
		{
			if (Visible && ControlWithFocusWhenFormIsOpened != null)
			{
				ControlWithFocusWhenFormIsOpened.Focus();
			}
		}

		protected virtual Control ControlWithFocusWhenFormIsOpened
		{
			get { return null; }
		}

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}

