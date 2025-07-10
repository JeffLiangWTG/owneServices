using System.Linq;
using System.Reflection;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class AICCTITest : NUnit.Framework.TestCase
	{
		public void TestSSNIsPersonalInformation()
		{
			var messageBlockStringAttribute = (MessageBlockStringAttribute)typeof(AICCTI).GetField(nameof(AICCTI.SSN)).GetCustomAttributes(typeof(MessageBlockStringAttribute)).First();
			Assert(messageBlockStringAttribute.IsPersonalInformation);
			Assert(messageBlockStringAttribute.MaskButDoNotCheckFormat);
		}
	}
}
