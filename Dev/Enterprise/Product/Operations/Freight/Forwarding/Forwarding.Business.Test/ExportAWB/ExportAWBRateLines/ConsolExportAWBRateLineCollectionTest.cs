using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ConsolExportAWBRateLineCollection))]
	sealed class ConsolExportAWBRateLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			ConsolExportAWBHeader master = Factory.New<ConsolExportAWBHeader>();
			master.EH_ParentID = Factory.New<ForwardingConsol>().PK;
			return new ConsolExportAWBRateLineCollection(master);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ConsolExportAWBRateLine>();
		}
	}
}
