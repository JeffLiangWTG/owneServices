using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(YesNoYesAllDialog))]
	sealed class YesNoYesAllDialogTestClass : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new YesNoYesAllDialog();
		}

		public void TestClickResults()
		{
			using (ExposedYesNoYesAllDialog dialog = new ExposedYesNoYesAllDialog())
			{
				AssertEquals(YesNoYesAllNoAllMessageBoxResult.No, dialog.Click_No());
				AssertEquals(YesNoYesAllNoAllMessageBoxResult.Yes, dialog.Click_Yes());
				AssertEquals(YesNoYesAllNoAllMessageBoxResult.YesToAll, dialog.Click_YesToAll());
			}
		}

		public void TestCaptionAndLabelSet()
		{
			using (ExposedYesNoYesAllDialog dialog = new ExposedYesNoYesAllDialog())
			{
				dialog.SetCaption("test");
				dialog.SetMessage("message");
				AssertEquals("test", dialog.Text);
				AssertEquals("message", dialog.MessageLabelText);
			}
		}

		class ExposedYesNoYesAllDialog : YesNoYesAllDialog
		{
			public YesNoYesAllNoAllMessageBoxResult Click_Yes()
			{
				YesButton_Click(this, new EventArgs());
				return Result;
			}

			public YesNoYesAllNoAllMessageBoxResult Click_No()
			{
				NoButton_Click(this, new EventArgs());
				return Result;
			}

			public YesNoYesAllNoAllMessageBoxResult Click_YesToAll()
			{
				YesToAllButton_Click(this, new EventArgs());
				return Result;
			}

			public string MessageLabelText
			{
				get { return MessageLabel.Text; }
			}
		}
	}
}
