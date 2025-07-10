using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgContactAttributeCollection))]
	sealed class OrgContactAttributeCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgContact contact = Factory.New<OrgContact>();
			return new OrgContactAttributeCollection(contact);
		}
	}
}
