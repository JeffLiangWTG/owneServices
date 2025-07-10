using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSupBuyLinkTrnModeCollection))]
	sealed class OrgSupBuyLinkTrnModeCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgSupBuyLinkTrnModeCollection(Factory, new ZQuery());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<OrgSupBuyLinkTrnMode>();
		}
	}
}
