using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingSupplierPart))]
	sealed class TrackingSupplierPartTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSiteUser()
		{
			AssertNotNull(WebHelper.TestSiteUser);

			AssertNull("SiteUser should be null", TestProduct.SiteUser);

			WebHelper.TestSiteUser.Login(WebHelper.TestOrg.OH_Code, WebHelper.TestContact.OC_Email, WebHelper.TestContact.PasswordForTesting);
			TestProduct.SiteUser = WebHelper.TestSiteUser;
			AssertEquals("LoggedInContact", WebHelper.TestContact.PK, TestProduct.LoggedInContact.PK);
		}

		[HttpContextEnabledTest]
		public void TestFromPK()
		{
			AssertNotNull(WebHelper.TestSiteUser);
			BusinessObjectFactory loadFactory = new BusinessObjectFactory();
			TestProduct.Part.OP_PartNum = "PartNum";
			AssertEquals("No related Orgs", 0, TestProduct.Part.RelatedOrganisations.Count);
			Factory.Save();
			AssertNull("Should return Null", TrackingSupplierPart.FromPKFilteredByContact(loadFactory, TestProduct.Part.PK, WebHelper.TestSiteUser));

			WebHelper.TestSiteUser.Login(WebHelper.TestOrg.OH_Code, WebHelper.TestContact.OC_Email, WebHelper.TestContact.PasswordForTesting);
			TestProduct.SiteUser = WebHelper.TestSiteUser;

			TestProduct.Part.RelatedOrganisations.AddOrganisationIfNotExist(WebHelper.TestOrg.PK, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			TrackingSupplierPart testSupplierPartFromPK = TrackingSupplierPart.FromPKFilteredByContact(loadFactory, TestProduct.Part.PK, WebHelper.TestSiteUser);
			AssertNotNull("Part FromPK should not be null", testSupplierPartFromPK);
			AssertEquals(TestProduct.Part.PK, testSupplierPartFromPK.Part.PK);
			AssertEquals("LoggedInContact", WebHelper.TestContact.PK, testSupplierPartFromPK.LoggedInContact.PK);
		}

		[HttpContextEnabledTest]
		public void TestFromPKFilteredByRelatedOrder()
		{
			WebHelper.TestSiteUser.Login(WebHelper.TestOrg.OH_Code, WebHelper.TestContact.OC_Email, WebHelper.TestContact.PasswordForTesting);

			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "Supplier";

			TestProduct.SiteUser = WebHelper.TestSiteUser;
			TestProduct.Part.OP_PartNum = "ABC123";
			TestProduct.Part.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);

			TrackingOrder order = Factory.NewWithValidTestData<TrackingOrder>();
			order.SupplierPK = supplier.PK;
			order.BuyerPK = WebHelper.TestSiteUser.CurrentOrg;

			OrderLine line = order.OrderLines.AddNew();
			line.JO_Partno = TestProduct.Part.OP_PartNum;
			Factory.Save();

			BusinessObjectFactory loadFactory = new BusinessObjectFactory();
			WebHelper.TestSiteUser.Logout();
			AssertNull(TrackingSupplierPart.FromPKFilteredByRelatedOrder(loadFactory, ZGuid.Empty, ZGuid.Empty, null));
			AssertNull(TrackingSupplierPart.FromPKFilteredByRelatedOrder(loadFactory, TestProduct.Part.PK, ZGuid.Empty, null));
			AssertNull(TrackingSupplierPart.FromPKFilteredByRelatedOrder(loadFactory, TestProduct.Part.PK, ZGuid.Empty, new TrackingSiteUser()));
			AssertNull(TrackingSupplierPart.FromPKFilteredByRelatedOrder(loadFactory, TestProduct.Part.PK, order.PK, new TrackingSiteUser()));

			WebHelper.TestSiteUser.Login(WebHelper.TestOrg.OH_Code, WebHelper.TestContact.OC_Email, WebHelper.TestContact.PasswordForTesting);

			TrackingOrder orderFromLoadFactory = TrackingOrder.FromPKFilteredByContact(loadFactory, order.PK, WebHelper.TestSiteUser);
			AssertNotNull("Precondition: Logged in user can now view the Order", orderFromLoadFactory);
			AssertNotNull("Precondition: The Order's Supplier is linked to the Product", orderFromLoadFactory.OrderLines[0].Product);

			TrackingSupplierPart testSupplierPart = TrackingSupplierPart.FromPKFilteredByRelatedOrder(loadFactory, TestProduct.Part.PK, order.PK, WebHelper.TestSiteUser);
			AssertNotNull("TestSupplierPart", testSupplierPart);
			AssertEquals("Part", TestProduct.Part.PK, testSupplierPart.Part.PK);
			AssertEquals("LoggedInContact", WebHelper.TestContact.PK, testSupplierPart.LoggedInContact.PK);
		}

		public void TestParamsByWhsAndClient()
		{
			TestProduct.Part.OP_PartNum = "PartNum";
			TestProduct.SiteUser = WebHelper.TestSiteUser;

			WhsProductParamsByWhsAndClient param = Factory.New<WhsProductParamsByWhsAndClient>();
			param.W3_OH = WebHelper.TestOrg.PK;
			param.W3_OP = TestProduct.Part.PK;

			WhsProductParamsByWhsAndClientCollection paramCollection = TestProduct.Product.ParamsByWhsAndClient;
			AssertNotNull(paramCollection);
			AssertEquals(1, paramCollection.Count);
			AssertEquals(true, TestProduct.Product.IsRegisteredEditableChildObject(TestProduct.Product.ParamsByWhsAndClient));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			WebHelper = new TestHelper(Factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TrackingSupplierPart(Factory, Factory.New<OrgSupplierPart>());
		}

		TrackingSupplierPart TestProduct
		{
			get { return (TrackingSupplierPart)CachedBusinessObject; }
		}

		TestHelper WebHelper;

		#endregion
	}
}
