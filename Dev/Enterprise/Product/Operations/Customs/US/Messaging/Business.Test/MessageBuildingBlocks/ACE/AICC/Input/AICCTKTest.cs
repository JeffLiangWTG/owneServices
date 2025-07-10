using System.Linq;
using System.Reflection;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class AICCTKTest : NUnit.Framework.TestCase
	{
		public void TestTINEINSSNCBPAssignedIsPersonalInformation()
		{
			var messageBlockStringAttribute = (MessageBlockStringAttribute)typeof(AICCTK).GetField(nameof(AICCTK.TINEINSSNCBPAssigned)).GetCustomAttributes(typeof(MessageBlockStringAttribute)).First();
			Assert(messageBlockStringAttribute.IsPersonalInformation);
		}
	}
}
