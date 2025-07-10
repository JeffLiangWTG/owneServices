using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing;

[TestedType(typeof(RefHarbourRateCollection))]
public class ZZRefHarbourRateCombinedCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		return new RefHarbourRateCollection(Factory);
	}
}
