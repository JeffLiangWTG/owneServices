using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing
{
	[TestedType(typeof(NZCCountryCollection))]
	public class NZCCountryCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new NZCCountryCollection(Factory);
		}
	}
}
