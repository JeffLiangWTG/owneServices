using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCAffirmationOfComplianceCollection))]
	sealed class USCAffirmationOfComplianceCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new USCAffirmationOfComplianceCollection(Factory);
		}
	}
}
