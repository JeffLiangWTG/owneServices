using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusClassificationLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestClassification()
		{
			CusClassification parent = Factory.New<CusClassification>();
			NUnit.Framework.Assert.That(parent, NUnit.Framework.Is.EqualTo(parent.Lookups.Classification));
		}
	}
}
