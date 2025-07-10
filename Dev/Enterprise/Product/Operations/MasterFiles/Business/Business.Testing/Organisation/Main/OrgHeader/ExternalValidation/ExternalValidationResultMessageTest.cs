using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ExternalValidationResultMessage))]
	sealed class ExternalValidationResultMessageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			var message = new ExternalValidationResultMessage("Error", "bla");
			AssertEquals("Error", message.MessageType);
			AssertEquals("bla", message.MessageContent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ExternalValidationResultMessage("", "");
		}
	}
}
