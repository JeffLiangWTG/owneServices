using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(MessagesChooserDialog))]
	sealed class MessagesChooserDialogTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestSendPressedIsSet()
		{
			using (MessagesChooserDialog dialog = new MessagesChooserDialog())
			{
				dialog.Show();
				AssertEquals("OKPressed", false, dialog.SendPressed);
				dialog.SendButton_Click(this, new EventArgs());
				AssertEquals("OKPressed", true, dialog.SendPressed);
			}
		}

		public void TestSelectAll()
		{
			using (MessagesChooserDialog dialog = new MessagesChooserDialog(Chooser))
			{
				dialog.SelectAllButton_Click(this, new EventArgs());
				AssertEquals(2, Chooser.SelectedManagers.Length);
			}
		}

		public void TestDeselectAll()
		{
			using (MessagesChooserDialog dialog = new MessagesChooserDialog(Chooser))
			{
				dialog.SelectAllButton_Click(this, new EventArgs());
				dialog.DeselectAllButton_Click(this, new EventArgs());
				AssertEquals(0, Chooser.SelectedManagers.Length);
			}
		}

		protected override Form GetFormToBashCore() => new MessagesChooserDialog();

		MessageChooserNonPersistent chooser;
		MessageChooserNonPersistent Chooser
		{
			get
			{
				if (chooser == null)
				{
					var testManager1 = new TestHelperSingleMessageManager(Factory.New<DummyBusinessObject>(), "name1");
					var testManager2 = new TestHelperSingleMessageManager(Factory.New<DummyBusinessObject>(), "name2");
					var managers = new SingleMessageManager[] { testManager1, testManager2 };
					chooser = new MessageChooserNonPersistent(managers, "question", "Action");
				}

				return chooser;
			}
		}
	}
}
