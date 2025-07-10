using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ConsolExportAWBOtherChargesCollection))]
	sealed class ConsolExportAWBOtherChargesCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			ConsolExportAWBHeader aWBHeader = Factory.New<ConsolExportAWBHeader>();
			return new ConsolExportAWBOtherChargesCollection(aWBHeader);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ConsolExportAWBOtherCharges>();
		}
	}
}
