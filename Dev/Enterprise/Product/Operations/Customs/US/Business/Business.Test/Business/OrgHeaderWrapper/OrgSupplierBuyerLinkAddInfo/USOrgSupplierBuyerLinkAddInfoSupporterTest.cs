using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USOrgSupplierBuyerLinkAddInfo))]
	public class USOrgSupplierBuyerLinkAddInfoSupporterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var link = Factory.New<OrgSupplierBuyerLink>();
			return new USOrgSupplierBuyerLinkAddInfo(link.GetAddInfo());
		}
	}
}
