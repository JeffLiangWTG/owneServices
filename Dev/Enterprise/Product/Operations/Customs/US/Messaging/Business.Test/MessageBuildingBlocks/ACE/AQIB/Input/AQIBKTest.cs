using System.Linq;
using System.Reflection;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class AQIBKTest : NUnit.Framework.TestCase
	{
		public void TestImporterNumber1IsPersonalInformation()
		{
			var messageBlockStringAttribute = (MessageBlockStringAttribute)typeof(AQIBK).GetField(nameof(AQIBK.ImporterNumber1)).GetCustomAttributes(typeof(MessageBlockStringAttribute)).First();
			Assert(messageBlockStringAttribute.IsPersonalInformation);
		}
	}
}
