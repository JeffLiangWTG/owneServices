using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MDMAdminPanelAddressCollection))]
	sealed class MDMAdminPanelAddressCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new MDMAdminPanelAddressCollection(Factory);
		}
	}
}
