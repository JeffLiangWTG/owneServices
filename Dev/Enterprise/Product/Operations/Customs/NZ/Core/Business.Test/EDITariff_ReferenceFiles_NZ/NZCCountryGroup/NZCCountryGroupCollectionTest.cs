using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing
{
	[TestedType(typeof(NZCCountryGroupCollection))]
	public class NZCCountryGroupCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new NZCCountryGroupCollection(Factory);
		}
	}
}
