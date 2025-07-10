using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(OrgSupBuyLinkTrnModeAddInfo))]
	sealed class OrgSupBuyLinkTrnModeAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<OrgSupBuyLinkTrnMode>().AddInfo;
		}
	}
}
