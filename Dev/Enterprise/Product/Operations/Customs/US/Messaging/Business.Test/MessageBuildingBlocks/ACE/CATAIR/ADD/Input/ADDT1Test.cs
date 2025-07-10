using System.Linq;
using System.Reflection;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class ADDT1Test : NUnit.Framework.TestCase
	{
		public void TestImporterNumberIsPersonalInformation()
		{
			var messageBlockStringAttribute = (MessageBlockStringAttribute)typeof(ADDT1).GetField(nameof(ADDT1.ImporterNumber)).GetCustomAttributes(typeof(MessageBlockStringAttribute)).First();
			Assert(messageBlockStringAttribute.IsPersonalInformation);
		}
	}
}
