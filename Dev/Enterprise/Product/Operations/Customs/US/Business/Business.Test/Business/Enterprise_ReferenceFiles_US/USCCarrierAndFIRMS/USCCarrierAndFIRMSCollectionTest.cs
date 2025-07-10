using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCCarrierAndFIRMSCollection))]
	sealed class USCCarrierAndFIRMSCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new USCCarrierAndFIRMSCollection(Factory);
	}
}
