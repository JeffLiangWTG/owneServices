using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class LPCOAuthorizedPartyWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLPCOAuthorizedParty()
		{
			ILPCOAuthorizedParty lPCOAuthorizedPartyWrapper = new LPCOAuthorizedPartyWrapper("id");
			NUnit.Framework.Assert.That(lPCOAuthorizedPartyWrapper.Name, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Name should be");
			NUnit.Framework.Assert.That(lPCOAuthorizedPartyWrapper.ID, NUnit.Framework.Is.EqualTo("id").Using(CustomComparers.TypeComparison), "ID should be");
			NUnit.Framework.Assert.That(lPCOAuthorizedPartyWrapper.TypeCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "TypeCode should be");
			lPCOAuthorizedPartyWrapper = new LPCOAuthorizedPartyWrapper("id", "typeCode");
			NUnit.Framework.Assert.That(lPCOAuthorizedPartyWrapper.Name, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Name should be");
			NUnit.Framework.Assert.That(lPCOAuthorizedPartyWrapper.ID, NUnit.Framework.Is.EqualTo("id").Using(CustomComparers.TypeComparison), "ID should be");
			NUnit.Framework.Assert.That(lPCOAuthorizedPartyWrapper.TypeCode, NUnit.Framework.Is.EqualTo("typeCode").Using(CustomComparers.TypeComparison), "TypeCode should be");
		}
	}
}
