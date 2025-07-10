using System.Collections.Generic;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	/// <summary>
	/// Select which of multiple possible values to apply back to job.
	/// </summary>
	public sealed partial class SingleValueSelectForm : ZChildForm
	{
		public SingleValueSelectForm()
		{
			InitializeComponent();
		}

		public SingleValueSelectForm(IEnumerable<string> values, string instructionsText, ResourceStringData formCaption)
		{
			InitializeComponent();

			this.InstructionsLabel.Text = instructionsText;
			this.CaptionResourceString = formCaption;

			var data = new ZBoolDescriptionPairList();

			foreach (var value in values)
			{
				data.AddNew(value, false);
			}

			checkedListBox.BindingItems = data;
			checkedListBox.ItemCheck += CheckedListBox_ItemCheck;
		}

		public void ShowYesNoButtonsInsteadOfOkButton()
		{
			okButton.Visible = false;
			yesButton.Visible = true;
			noButton.Visible = true;
			AcceptButton = yesButton;
		}

		// If the user ticks an item, the previously ticked item is automatically unchecked
		// If the user unticks an item, the OK button becomes disabled
		bool inAutoUncheck;
		void CheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			if (e.NewValue != CheckState.Checked)
			{
				if (!inAutoUncheck)
				{
					((ZButton)AcceptButton).Enabled = false;
				}
				return;
			}

			var checkedItems = checkedListBox.CheckedIndices;
			if (checkedItems.Count > 0)
			{
				// Uncheck the other item
				inAutoUncheck = true;
				try
				{
					checkedListBox.SetItemChecked(checkedItems[0], false);
				}
				finally
				{
					inAutoUncheck = false;
				}
			}
			((ZButton)AcceptButton).Enabled = true;
		}

		public string SelectedName
		{
			get
			{
				var checkedItems = checkedListBox.CheckedIndices;
				return checkedItems.Count > 0 ? checkedListBox.Items[checkedItems[0]].ToString() : string.Empty;
			}
		}

		public int SelectedIndex
		{
			get
			{
				var checkedIndices = checkedListBox.SelectedIndices;
				return checkedIndices.Count > 0 ? checkedListBox.SelectedIndices[0] : -1;
			}
		}

#if DEBUG
		public ZCheckedListBox CheckedListBox_ExposedForTest => checkedListBox;
#endif
	}
}
