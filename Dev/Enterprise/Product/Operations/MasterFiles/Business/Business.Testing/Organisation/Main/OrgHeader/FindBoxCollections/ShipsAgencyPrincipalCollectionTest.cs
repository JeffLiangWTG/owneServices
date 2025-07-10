using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ShipsAgencyPrincipalCollection))]
	sealed class ShipsAgencyPrincipalCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestFilter()
		{
			SetUpOrgs();
			Factory.Save();

			ShipsAgencyPrincipalCollection collection = new ShipsAgencyPrincipalCollection(Factory);

			collection.Load();
			AssertEquals("Principal should be in the collection", true, collection.Contains(Principal));
			AssertEquals("ShippingLine should be in the collection", false, collection.Contains(ShippingLine));
			AssertEquals("NonShippingLine should not be in the collection", false, collection.Contains(NotShippingLine));
		}

		public void TestIsValidPrincipal()
		{
			SetUpOrgs();
			Factory.Save();

			ShipsAgencyPrincipalCollection collection = new ShipsAgencyPrincipalCollection(Factory);

			collection.Load();
			AssertEquals("Principal should be considered valid", true, collection.IsValidPrincipal(Principal));
			AssertEquals("ShippingLine should not be considered valid", false, collection.IsValidPrincipal(ShippingLine));
			AssertEquals("NonShippingLine should not be considered valid", false, collection.IsValidPrincipal(NotShippingLine));
		}

		public void TestNewChildDefaults()
		{
			ShipsAgencyPrincipalCollection collection = new ShipsAgencyPrincipalCollection(Factory);
			OrgHeader principal = collection.AddNew();
			AssertEquals("OH_IsShippingProvider must be set", true, principal.OH_IsShippingProvider);
			AssertEquals("OB_CRIsShipsAgencyPrincipal must be set", true, principal.CompanyData.OB_CRIsShipsAgencyPrincipal);
		}

		public void TestSetsDefaultBusinessObjectFilter()
		{
			ShipsAgencyPrincipalCollection collection = new ShipsAgencyPrincipalCollection(Factory);
			AssertEquals("Should have a default Secondary Type" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Secondary Type" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ShipsAgencyPrincipalCollection(Factory);
		}

		OrgHeader NotShippingLine;
		OrgHeader ShippingLine;
		OrgHeader Principal;

		public void SetUpOrgs()
		{
			NotShippingLine = Factory.NewWithValidTestData<OrgHeader>();

			ShippingLine = Factory.NewWithValidTestData<OrgHeader>();
			ShippingLine.OH_IsShippingProvider = true;

			Principal = Factory.NewWithValidTestData<OrgHeader>();
			Principal.OH_IsShippingProvider = true;
			Principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
		}

		#endregion
	}
}
