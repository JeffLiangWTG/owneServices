using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(MessageChooserNonPersistent))]
	sealed class MessageChooserNonPersistentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMessagesToSend()
		{
			AssertEquals("Count", 2, Chooser.MessagesToSend.Count);
			AssertEquals("Description", "name1", Chooser.MessagesToSend[0].Description);
			AssertEquals("Description", "name2", Chooser.MessagesToSend[1].Description);
			AssertEquals("Description", false, Chooser.MessagesToSend[0].Value);
			AssertEquals("Description", false, Chooser.MessagesToSend[1].Value);
		}

		public void TestSelectedManagers()
		{
			AssertEquals("Length", 0, Chooser.SelectedManagers.Length);
			Chooser.MessagesToSend[1].Value = true;
			AssertEquals("Length", 1, Chooser.SelectedManagers.Length);
			AssertEquals("Length", "name2", Chooser.SelectedManagers[0].MessageFriendlyName);
		}

		public void TestSelectAll()
		{
			Chooser.SelectAll();
			AssertEquals("Value[0]", true, Chooser.MessagesToSend[0].Value);
			AssertEquals("Value[1]", true, Chooser.MessagesToSend[1].Value);
		}

		public void TestDeselectAll()
		{
			Chooser.SelectAll();
			Chooser.DelselectAll();
			AssertEquals("Value[0]", false, Chooser.MessagesToSend[0].Value);
			AssertEquals("Value[1]", false, Chooser.MessagesToSend[1].Value);
		}

		public void TestSelectDefaultToSendManagers()
		{
			Chooser.SelectDefaultToSendManagers((manager) => manager == TestManager1);
			AssertEquals("Value[0]", true, Chooser.MessagesToSend[0].Value);
			AssertEquals("Value[1]", false, Chooser.MessagesToSend[1].Value);
		}

		public void TestQuestion()
		{
			AssertEquals("Question", "question", Chooser.Question);
		}

		public void TestActionButton()
		{
			AssertEquals("Action", "Action", Chooser.Action);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Chooser;
		}

		MessageChooserNonPersistent chooser;
		MessageChooserNonPersistent Chooser
		{
			get
			{
				if (chooser == null)
				{
					chooser = new MessageChooserNonPersistent(Managers, "question", "Action");
				}
				return chooser;
			}
		}

		SingleMessageManager[] managers;
		SingleMessageManager[] Managers
		{
			get
			{
				if (managers == null)
				{
					managers = new SingleMessageManager[] { TestManager1, TestManager2 };
				}
				return managers;
			}
		}

		TestHelperSingleMessageManager testManager1;
		TestHelperSingleMessageManager TestManager1
		{
			get
			{
				if (testManager1 == null)
				{
					testManager1 = new TestHelperSingleMessageManager(Factory.New<DummyBusinessObject>(), "name1");
				}
				return testManager1;
			}
		}

		TestHelperSingleMessageManager testManager2;
		TestHelperSingleMessageManager TestManager2
		{
			get
			{
				if (testManager2 == null)
				{
					testManager2 = new TestHelperSingleMessageManager(Factory.New<DummyBusinessObject>(), "name2");
				}
				return testManager2;
			}
		}

		#endregion
	}
}
