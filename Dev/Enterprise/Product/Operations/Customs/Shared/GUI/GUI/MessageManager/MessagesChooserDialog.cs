using Enterprise.Customs.Business;

namespace Enterprise.Customs.GUI
{
	public partial class MessagesChooserDialog : ZArchitecture.GUI.ZChildForm
	{
		public MessagesChooserDialog()
		{
			InitializeComponent();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1048")]
		public MessagesChooserDialog(MessageChooserNonPersistent chooser) : base(chooser)
		{
			InitializeComponent();
			this.Chooser = chooser;
			this.Text = ResString.GetMultilingualString("Customs|MessagesChooserDialog|Question", $"{chooser.Question}");
			//string questionText = ResString.GetMultilingualString("Customs|MessagesChooserDialog|Question", "Default question text");
			//this.Text = string.Format(questionText, chooser.Question);
			this.SendButton.Text = ResString.GetMultilingualString("Customs|MessagesChooserDialog|Action", $"{chooser.Action}");
		}

		protected readonly MessageChooserNonPersistent Chooser;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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

		bool sendPressed;
		public bool SendPressed
		{
			get
			{
				return sendPressed;
			}
#if DEBUG
			set
			{
				sendPressed = value;
			}
#endif
		}

		protected virtual void InitializeMessageListControl()
		{
			this.MessagesCheckedListBox = new ZArchitecture.GUI.ZCheckedListBox();
			// 
			// MessagesCheckedListBox
			// 
			this.MessagesCheckedListBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.MessagesCheckedListBox.BindingItems = null;
			this.MessagesCheckedListBox.BindTo = "MessagesToSend";
			this.MessagesCheckedListBox.CheckOnClick = true;
			this.MessagesCheckedListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			this.MessagesCheckedListBox.Name = "MessagesCheckedListBox";
			this.MessagesCheckedListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 169);
			this.MessagesCheckedListBox.TabIndex = 0;
			this.Controls.Add(this.MessagesCheckedListBox);
			this.Controls.SetChildIndex(this.MessagesCheckedListBox, 0);
		}

		internal void SendButton_Click(object sender, System.EventArgs e)
		{
			sendPressed = true;
			Close();
		}

		internal void SelectAllButton_Click(object sender, System.EventArgs e)
		{
			SelectAll();
		}

		protected virtual void SelectAll()
		{
			Chooser.SelectAll();
		}

		internal void DeselectAllButton_Click(object sender, System.EventArgs e)
		{
			DeSelectAll();
		}

		protected virtual void DeSelectAll()
		{
			Chooser.DelselectAll();
		}
	}
}
