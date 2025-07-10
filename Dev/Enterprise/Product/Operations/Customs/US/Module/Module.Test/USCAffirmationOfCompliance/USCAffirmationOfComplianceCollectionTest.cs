using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCAffirmationOfComplianceCollection))]
	sealed class USCAffirmationOfComplianceCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new USCAffirmationOfComplianceCollection(Factory);
	}
}
