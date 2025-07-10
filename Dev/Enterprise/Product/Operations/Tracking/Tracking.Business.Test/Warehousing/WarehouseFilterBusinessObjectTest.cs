using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(FilterStripBusinessObject))]
	[HttpContextEnabledTest]
	public abstract class WarehouseFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestWebProductFilter()
		{
			var webHelper = new TestHelper(Factory);

			WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			webHelper.TestSiteUser.Login(OrgHeader1.OH_Code, OrgContact1.OC_Email, OrgContact1.PasswordForTesting);

			var filterBusinessObject = GetNewFilterStripBusinessObject();
			var productFilter = filterBusinessObject["Product"] as ModuleGuidFilter;

			productFilter.Property = WebPart1.PK;
			AssertEquals("No errors exist", productFilter.HasErrors, false);

			productFilter.Property = WebPart2.PK;
			AssertEquals("Errors exist", productFilter.HasErrors, true);
			AssertEquals("Property error", productFilter.Notifications.First().Message, "Error - Property: Enter a valid selection.");

			webHelper.TestSiteUser.Logout();
			WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			webHelper.TestSiteUser.Login(OrgHeader1.OH_Code, OrgContact1.OC_Email, OrgContact1.PasswordForTesting);

			productFilter.Property = WebPart1.PK;
			AssertEquals("No errors exist", productFilter.HasErrors, false);

			productFilter.Property = WebPart2.PK;
			AssertEquals("No errors exist", productFilter.HasErrors, false);
		}

		public void TestWebProductCollection()
		{
			var webHelper = new TestHelper(Factory);

			WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			webHelper.TestSiteUser.Login(OrgHeader1.OH_Code, OrgContact1.OC_Email, OrgContact1.PasswordForTesting);

			var filterBusinessObject = GetNewFilterStripBusinessObject();
			var productFilter = filterBusinessObject["Product"] as ModuleGuidFilter;

			var collection1 = (BusinessObjectCollection)productFilter.List;
			collection1.Load();
			AssertEquals(1, collection1.Count);
			AssertEquals(WebPart1.PK, collection1.First().PK);

			webHelper.TestSiteUser.Logout();
			WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			webHelper.TestSiteUser.Login(OrgHeader2.OH_Code, OrgContact2.OC_Email, OrgContact2.PasswordForTesting);

			var collection2 = (BusinessObjectCollection)productFilter.List;
			collection2.Load();
			AssertEquals(1, collection2.Count);
			AssertEquals(WebPart2.PK, collection2.First().PK);
		}

		#region SetUp & TearDown

		OrgHeader OrgHeader1;
		OrgHeader OrgHeader2;
		OrgContact OrgContact1;
		OrgContact OrgContact2;
		OrgSupplierPart WebPart1;
		OrgSupplierPart WebPart2;
		bool OldRegistryValue;

		protected override void SetUp()
		{
			Globals.IsWeb = true;
			base.SetUp();

			OldRegistryValue = WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.Value;

			OrgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty = Factory.New<OrgRelatedParty>();
			relatedParty.PR_OH_Parent = OrgHeader2.PK;
			relatedParty.PR_OH_RelatedParty = OrgHeader1.PK;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;

			OrgContact1 = OrgHeader1.Contacts.AddNew();
			OrgContact1.OC_ContactName = "contactname";
			OrgContact1.OC_Email = "email@email.em";
			OrgContact1.SetHashedPassword("pswrd");
			OrgContact1.OC_WebAccessEnabled = true;

			OrgContact2 = OrgHeader2.Contacts.AddNew();
			OrgContact2.OC_ContactName = "OtherName";
			OrgContact2.OC_Email = "OtherEmail@email.em";
			OrgContact2.SetHashedPassword("pswrd2");
			OrgContact2.OC_WebAccessEnabled = true;

			var warehouseHelper = new WhsTestHelperFunctions(Factory);

			WebPart1 = warehouseHelper.CreateProduct(OrgHeader1, "ABC");
			WebPart2 = warehouseHelper.CreateProduct(OrgHeader2, "DEF");

			Factory.Save();
		}

		protected override void TearDown()
		{
			Globals.IsWeb = false;
			base.TearDown();

			WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OldRegistryValue);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return (FilterStripBusinessObject)Activator.CreateInstance(GetExpectedBusinessObjectType());
		}

		#endregion
	}
}
