using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Freight.Agency.Business.AgencyCountry;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(OrgHeaderCollectionWithoutNew))]
	internal class OrgHeaderCollectionWithoutNewTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgHeaderCollectionWithoutNew(Factory);
		}
	}
}
