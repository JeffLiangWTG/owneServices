using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ConsolExportAWBAccountingInformationCollection))]
	sealed class ConsolExportAWBAccountingInformationCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			ConsolExportAWBHeader aWBHeader = Factory.New<ConsolExportAWBHeader>();
			return new ConsolExportAWBAccountingInformationCollection(aWBHeader, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ConsolExportAWBAccountingInformation>();
		}
	}
}
