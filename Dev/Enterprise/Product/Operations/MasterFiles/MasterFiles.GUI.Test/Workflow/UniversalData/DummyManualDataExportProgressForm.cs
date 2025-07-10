using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[DesignerCategory("Code")]
	public class DummyManualDataExportProgressForm : ZForm, IManualDataExportProgressForm
	{
		ZButton sendButton;
		ZButton closeButton;

		ZLabel titleLabel;
		ZTextBox textBox;

		readonly IContainer components = new Container();

		public ZForm Form
		{
			get { return this; }
		}

		public ZLabel TitleLabel
		{
			get
			{
				if (titleLabel == null)
				{
					titleLabel = new ZLabel();
					components.Add(titleLabel);
				}
				return titleLabel;
			}
		}

		public ZTextBox NotificationsTextBox
		{
			get
			{
				if (textBox == null)
				{
					textBox = new ZTextBox();
					textBox.CharacterCasing = CharacterCasing.Normal;
					components.Add(textBox);
				}
				return textBox;
			}
		}

		public ZButton SendButton
		{
			get
			{
				if (sendButton == null)
				{
					sendButton = new ZButton();
					components.Add(sendButton);
				}
				return sendButton;
			}
		}

		public ZButton CloseButton
		{
			get
			{
				if (closeButton == null)
				{
					closeButton = new ZButton();
					components.Add(closeButton);
				}
				return closeButton;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
