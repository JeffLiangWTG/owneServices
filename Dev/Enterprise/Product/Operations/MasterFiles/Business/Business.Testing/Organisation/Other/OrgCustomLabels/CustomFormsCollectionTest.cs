using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CustomFormsCollection))]
	sealed class CustomFormsCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			return new CustomFormsCollection(org, Factory);
		}
	}
}
