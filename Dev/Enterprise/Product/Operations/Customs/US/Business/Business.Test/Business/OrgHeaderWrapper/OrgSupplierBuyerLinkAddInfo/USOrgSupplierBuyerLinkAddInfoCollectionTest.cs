using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USOrgSupplierBuyerLinkAddInfoCollection))]
	public class USOrgSupplierBuyerLinkAddInfoCollectionTest : NonPersistentBusinessObjectCollectionTestCase<USOrgSupplierBuyerLinkAddInfoCollection>
	{
		public override void TestAdd()
		{
			var collection = new USOrgSupplierBuyerLinkAddInfoCollection(Factory);
			collection.Add(GetNewElementToAddToTheCollection());
			AssertEquals(1, collection.Count);
		}

		public override void TestAddNew()
		{
			var collection = new USOrgSupplierBuyerLinkAddInfoCollection(Factory);
			collection.Add(GetNewElementToAddToTheCollection());
			AssertEquals(1, collection.Count);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			var collection = new USOrgSupplierBuyerLinkAddInfoCollection(Factory);
			collection.Add(GetNewElementToAddToTheCollection());
			AssertEquals(1, collection.Count);
		}

		protected override USOrgSupplierBuyerLinkAddInfoCollection GetCollectionToTest()
		{
			return new USOrgSupplierBuyerLinkAddInfoCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.UnitedStates;
			return new USOrgSupplierBuyerLinkAddInfo(link.AddInfo as OrgSupplierBuyerLinkAddInfo);
		}
	}
}
