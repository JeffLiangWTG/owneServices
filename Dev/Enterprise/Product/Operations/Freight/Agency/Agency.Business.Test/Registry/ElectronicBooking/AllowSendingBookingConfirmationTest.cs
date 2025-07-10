using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AllowSendingBookingConfirmation))]
	public class AllowSendingBookingConfirmationTest : RegistryBusinessObjectTemplateTestCase<AllowSendingBookingConfirmation>
	{
		#region Validate PrincipalPK

		public void TestValidatePrincipalPK_Empty_HasError()
		{
			var collection = new AllowSendingBookingConfirmationCollection();
			var item = collection.AddNew();
			item.ValidatePrincipalPK();

			AssertNoErrors(item.PrincipalPKInfo);
		}

		public void TestValidatePrincipalPK_Invalid_HasError()
		{
			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
			var collection = new AllowSendingBookingConfirmationCollection();
			var item = collection.AddNew();
			item.PrincipalPK = orgProxy.PK;
			item.ValidatePrincipalPK();

			AssertHasError(item.PrincipalPKInfo, "Please enter a valid Principal Org.");
		}

		public void TestValidatePrincipalPK_Duplicated_Filled_Principal_HasError()
		{
			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
			orgProxy.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			orgProxy.Factory.Save();

			var collection = new AllowSendingBookingConfirmationCollection();
			var item1 = collection.AddNew();
			item1.PrincipalPK = orgProxy.PK;
			item1.ValidatePrincipalPK();

			AssertNoErrors(item1.PrincipalPKInfo);

			var item2 = collection.AddNew();
			item2.PrincipalPK = orgProxy.PK;
			item2.ValidatePrincipalPK();

			AssertHasError(item2.PrincipalPKInfo, "The Principal Org. has been duplicated and must be unique.");
		}

		public void TestValidatePrincipalPK_Duplicated_Empty_Principal_HasError()
		{
			var collection = new AllowSendingBookingConfirmationCollection();
			var item1 = collection.AddNew();
			item1.PrincipalPK = Guid.Empty;
			item1.ValidatePrincipalPK();

			AssertNoErrors(item1.PrincipalPKInfo);

			var item2 = collection.AddNew();
			item2.PrincipalPK = Guid.Empty;
			item2.ValidatePrincipalPK();

			AssertHasError(item2.PrincipalPKInfo, "The Principal Org. has been duplicated and must be unique.");
		}

		public void TestValidatePrincipalPK_OK()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsShippingLine = true;
			org1.OH_IsShippingProvider = true;
			org1.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			org1.Factory.Save();

			var collection = new AllowSendingBookingConfirmationCollection();
			var item1 = collection.AddNew();
			item1.PrincipalPK = org1.PK;
			item1.ValidatePrincipalPK();

			AssertNoErrors(item1.PrincipalPKInfo);

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_IsShippingLine = true;
			org2.OH_IsShippingProvider = true;
			org2.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			org2.Factory.Save();

			var item2 = collection.AddNew();
			item2.PrincipalPK = org2.PK;
			item2.ValidatePrincipalPK();

			AssertNoErrors(item2.PrincipalPKInfo);
		}

		#endregion

		#region Validate Enabled

		public void TestValidateEnabled_OK()
		{
			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
			orgProxy.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			orgProxy.Factory.Save();

			var collection = new AllowSendingBookingConfirmationCollection();
			var item = collection.AddNew();
			item.PrincipalPK = orgProxy.PK;
			item.Enabled = true;
			item.ValidateEnabled();

			AssertNoErrors(item.PrincipalPKInfo);
			AssertNoRowErrors(item);
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override AllowSendingBookingConfirmation GetBusinessObjectToClone()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override AllowSendingBookingConfirmation GetBusinessObjectToSerialise()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override void CheckAllPropertiesAreEqual(AllowSendingBookingConfirmation originalBusinessObject, AllowSendingBookingConfirmation newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);
			AssertEquals("PrincipalPK", originalBusinessObject.PrincipalPK, newBusinessObject.PrincipalPK);
			AssertEquals("Enabled", originalBusinessObject.Enabled, newBusinessObject.Enabled);
		}

		AllowSendingBookingConfirmation GetNewPopulatedBusinessObject()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsShippingLine = true;
			org.OH_IsShippingProvider = true;
			org.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			org.Factory.Save();

			var allowSendingBookingConfirmation = new AllowSendingBookingConfirmation();
			allowSendingBookingConfirmation.PrincipalPK = org.PK;
			allowSendingBookingConfirmation.Enabled = true;
			return allowSendingBookingConfirmation;
		}

		#endregion
	}
}
