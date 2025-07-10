using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	[TestedType(typeof(UYCInterchange))]
	public class UYCInterchangeTest : EDIInterchangeTest
	{
		public void TestProperties()
		{
			var interchange = Factory.New<UYCInterchange>();
			AssertEquals("EI_ApplicationCode", EDIInterchange.ApplicationCodes.UYCustoms, interchange.EI_ApplicationCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<UYCInterchange>();
		}
	}
}
