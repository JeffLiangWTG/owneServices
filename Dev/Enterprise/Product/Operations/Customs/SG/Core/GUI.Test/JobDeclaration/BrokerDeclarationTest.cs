using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	sealed class BrokerDeclarationTest : TestCase
	{
		public void TestBrokerDeclaration()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			bool testBrokerDec = new BrokerDeclaration().GetBrokerDeclaration();
			AssertNotNull("Broker Declaration Message Shown", UnitTestUserNotification.Instance.LastMessage);
			string expectedMessage = "I/we declare that the information declared is true and correct. \n\n\nTo continue, and submit this Declaration;";
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Broker declaration accepted", true, testBrokerDec);
		}

		public void TestBrokerDeclarationNotAgreedOrCancelled()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			bool testBrokerDec = new BrokerDeclaration().GetBrokerDeclaration();
			AssertNotNull("Broker Declaration Message Shown", UnitTestUserNotification.Instance.LastMessage);
			string expectedMessage = "I/we declare that the information declared is true and correct. \n\n\nTo continue, and submit this Declaration;";
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Broker declaration not agreed to", false, testBrokerDec);
		}
	}
}
