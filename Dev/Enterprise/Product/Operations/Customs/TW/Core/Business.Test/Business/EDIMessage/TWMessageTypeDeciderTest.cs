using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWMessageTypeDeciderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetTypeForLoad()
		{
			var typeDecider = new TWMessageTypeDecider();
			var message = Factory.New<TWMessage>();
			var row = ((INeedRow)message).Row;
			CombineAssertions(() =>
			{
				message.EM_MessageType = "TPC";
				NUnit.Framework.Assert.That(typeDecider.GetTypeForLoad(row, Factory), NUnit.Framework.Is.EqualTo(typeof(N5110EDIMessage)), "MessageType For TPC");
				message.EM_MessageType = "TAD";
				NUnit.Framework.Assert.That(typeDecider.GetTypeForLoad(row, Factory), NUnit.Framework.Is.EqualTo(typeof(N5111EDIMessage)), "MessageType For TAD");
				message.EM_MessageType = "IRM";
				NUnit.Framework.Assert.That(typeDecider.GetTypeForLoad(row, Factory), NUnit.Framework.Is.EqualTo(typeof(N5116EDIMessage)), "MessageType For IRM");
				message.EM_MessageType = "ERM";
				NUnit.Framework.Assert.That(typeDecider.GetTypeForLoad(row, Factory), NUnit.Framework.Is.EqualTo(typeof(N5204EDIMessage)), "MessageType For ERM");
				message.EM_MessageType = "XXX";
				NUnit.Framework.Assert.That(typeDecider.GetTypeForLoad(row, Factory), NUnit.Framework.Is.EqualTo(typeof(TWMessage)), "MessageType For XXX");
				message.EM_MessageType = "";
				NUnit.Framework.Assert.That(typeDecider.GetTypeForLoad(row, Factory), NUnit.Framework.Is.EqualTo(typeof(TWMessage)), "MessageType For Empty");
			}

			);
		}
	}
}
