using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class MessageNumStrategyIncludingLicenceCodeTest : TestCaseWithFactory
	{
		public void TestStrategyWorks()
		{
			var message = Factory.New<EDIMessageForTesting>();
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var message2 = factory2.New<EDIMessageForTesting>();
			factory2.Save();
			AssertEquals("EDIEDIDAT_1", message.EM_MessageNum);
			AssertEquals("EDIEDIDAT_2", message2.EM_MessageNum);
		}

		sealed class EDIMessageForTesting : EDIMessage
		{
			public EDIMessageForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
				MessageNumberStrategy = new MessageNumStrategyIncludingLicenceCode(() => Company, () => Environment.Env.NumberFountains.EDIFACTNumberFountain("M", "ENT", "CBP"), Factory);
			}
		}
	}
}
