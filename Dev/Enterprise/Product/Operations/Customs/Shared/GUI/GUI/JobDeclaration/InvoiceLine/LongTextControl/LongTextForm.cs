using System;
using System.Media;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	partial class LongTextForm : ZChildForm
	{
		#region Constructors

		internal LongTextForm()
		{
		}

		internal LongTextForm(BusinessObject dataSource, string bindingMember, ResourceStringData captionResourceString, CharacterCasing characterCasing)
			: base(dataSource)
		{
			if (!string.IsNullOrEmpty(bindingMember))
			{
				BindingSource.SetBindingMember(LongTextTextBox, bindingMember);
				HookLongTextTextBoxEvents();
			}

			if (captionResourceString != null)
			{
				CaptionResourceString = captionResourceString;
				LongTextGroupBox.CaptionResourceString = captionResourceString;
				LongTextTextBox.CaptionResourceString = captionResourceString;
			}
			LongTextTextBox.CharacterCasing = characterCasing;
		}

		protected void HookLongTextTextBoxEvents()
		{
			LongTextTextBox.KeyUp += LongTextTextBox_KeyUp;
			LongTextTextBox.KeyDown += LongTextTextBox_KeyDown;
		}

		protected void UnHookLongTextTextBoxEvents()
		{
			LongTextTextBox.KeyUp -= LongTextTextBox_KeyUp;
			LongTextTextBox.KeyDown -= LongTextTextBox_KeyDown;
		}

		ZBool textBoxIsOverMaxLength = false;

		void LongTextTextBox_KeyUp(object sender, KeyEventArgs e)
		{
			if (textBoxIsOverMaxLength && !e.Alt && !e.Control && e.KeyCode != Keys.ControlKey && e.KeyCode != Keys.Menu
				&& e.Modifiers != Keys.Control && e.Modifiers != Keys.Alt)
			{
				OverMaxLengthCallBack();
			}
		}

		protected virtual void OverMaxLengthCallBack()
		{
#if !WINZOR
			SystemSounds.Beep.Play();
#endif
		}

		void LongTextTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			textBoxIsOverMaxLength = IsOverMaxLength;
		}

		bool IsOverMaxLength => LongTextTextBox.TextLength == LongTextTextBox.MaxLength;

		#endregion

		#region Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					UnHookLongTextTextBoxEvents();
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		void OKButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
