using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CommunicationWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCommunication()
		{
			ICommunication communicationWrapper = new CommunicationWrapper("id", "type");
			NUnit.Framework.Assert.That(communicationWrapper.ID, NUnit.Framework.Is.EqualTo("id").Using(CustomComparers.TypeComparison), "ID should be");
			NUnit.Framework.Assert.That(communicationWrapper.TypeID, NUnit.Framework.Is.EqualTo("type").Using(CustomComparers.TypeComparison), "TypeID should be");
		}
	}
}
