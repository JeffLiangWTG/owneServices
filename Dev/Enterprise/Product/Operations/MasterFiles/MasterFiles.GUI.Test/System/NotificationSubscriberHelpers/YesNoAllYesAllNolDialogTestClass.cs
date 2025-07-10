using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(YesNoAllYesAllNoAllDialog))]
	sealed class YesNoAllYesAllNolDialogTestClass : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new YesNoAllYesAllNoAllDialog();
		}

		public void TestClickResults()
		{
			using (ExposedYesNoAllYesAllNoAllDialog dialog = new ExposedYesNoAllYesAllNoAllDialog())
			{
				AssertEquals(YesNoYesAllNoAllMessageBoxResult.No, dialog.Click_No());
				AssertEquals(YesNoYesAllNoAllMessageBoxResult.Yes, dialog.Click_Yes());
				AssertEquals(YesNoYesAllNoAllMessageBoxResult.YesToAll, dialog.Click_YesToAll());
				AssertEquals(YesNoYesAllNoAllMessageBoxResult.NoToAll, dialog.Click_NoToAll());
			}
		}

		public void TestCaptionAndLabelSet()
		{
			using (ExposedYesNoAllYesAllNoAllDialog dialog = new ExposedYesNoAllYesAllNoAllDialog())
			{
				dialog.SetCaption("test");
				dialog.SetMessage("message");
				AssertEquals("test", dialog.Text);
				AssertEquals("message", dialog.MessageLabelText);
			}
		}

		class ExposedYesNoAllYesAllNoAllDialog : YesNoAllYesAllNoAllDialog
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

			public YesNoYesAllNoAllMessageBoxResult Click_NoToAll()
			{
				NoToAllButton_Click(this, new EventArgs());
				return Result;
			}

			public string MessageLabelText
			{
				get { return MessageLabel.Text; }
			}
		}
	}
}
