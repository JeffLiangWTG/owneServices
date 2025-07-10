using System.Drawing;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(BillOfLadingImage))]
	public class BillOfLadingImageTest : RegistryBusinessObjectTemplateTestCase<BillOfLadingImage>
	{
		#region Validate PrincipalPK

		public void TestValidatePrincipalPK_Empty_HasError()
		{
			var collection = new BillOfLadingImageCollection();
			var bi = collection.AddNew();
			bi.ValidatePrincipalPK();

			AssertHasError(bi.PrincipalPKInfo, "Please enter a Principal Org..");
		}

		public void TestValidatePrincipalPK_Invalid_HasError()
		{
			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
			var collection = new BillOfLadingImageCollection();
			var bi = collection.AddNew();
			bi.PrincipalPK = orgProxy.PK;
			bi.ValidatePrincipalPK();

			AssertHasError(bi.PrincipalPKInfo, "Please enter a valid principal Org..");
		}

		public void TestValidatePrincipalPK_Duplicated_HasError()
		{
			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
			orgProxy.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			orgProxy.Factory.Save();

			var collection = new BillOfLadingImageCollection();
			var bi1 = collection.AddNew();
			bi1.PrincipalPK = orgProxy.PK;
			bi1.ValidatePrincipalPK();

			AssertNoErrors(bi1.PrincipalPKInfo);

			var bi2 = collection.AddNew();
			bi2.PrincipalPK = orgProxy.PK;
			bi2.ValidatePrincipalPK();

			AssertHasError(bi2.PrincipalPKInfo, "A principal org. may only appear once in this list.");
		}

		public void TestValidatePrincipalPK_OK()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsShippingLine = true;
			org1.OH_IsShippingProvider = true;
			org1.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			org1.Factory.Save();

			var collection = new BillOfLadingImageCollection();
			var bi1 = collection.AddNew();
			bi1.PrincipalPK = org1.PK;
			bi1.ValidatePrincipalPK();

			AssertNoErrors(bi1.PrincipalPKInfo);

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_IsShippingLine = true;
			org2.OH_IsShippingProvider = true;
			org2.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			org2.Factory.Save();

			var bi2 = collection.AddNew();
			bi2.PrincipalPK = org2.PK;
			bi2.ValidatePrincipalPK();

			AssertNoErrors(bi2.PrincipalPKInfo);
		}

		#endregion

		#region Validate Enabled

		public void TestValidateEnabled_PrincipalOrImageEmpty_HasError()
		{
			var collection = new BillOfLadingImageCollection();
			var bi1 = collection.AddNew();

			bi1.Enabled = true;
			bi1.ValidateEnabled();

			AssertHasError(bi1.PrincipalPKInfo, "Please enter a Principal Org..");
			AssertHasRowError(bi1, "Please select an Image.");
		}

		public void TestValidateEnabled_OK()
		{
			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
			orgProxy.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			orgProxy.Factory.Save();

			var collection = new BillOfLadingImageCollection();
			var bi = collection.AddNew();
			bi.PrincipalPK = orgProxy.PK;
			bi.Enabled = true;
			bi.Image = new Bitmap(1, 1);
			bi.ValidateEnabled();

			AssertNoErrors(bi.PrincipalPKInfo);
			AssertNoRowErrors(bi);
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override BillOfLadingImage GetBusinessObjectToClone()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override BillOfLadingImage GetBusinessObjectToSerialise()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override void CheckAllPropertiesAreEqual(BillOfLadingImage originalBusinessObject, BillOfLadingImage newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);
			AssertEquals("PrincipalPK", originalBusinessObject.PrincipalPK, newBusinessObject.PrincipalPK);
			AssertEquals("Description", originalBusinessObject.Description, newBusinessObject.Description);
			AssertEquals("Enabled", originalBusinessObject.Enabled, newBusinessObject.Enabled);
		}

		BillOfLadingImage GetNewPopulatedBusinessObject()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsShippingLine = true;
			org.OH_IsShippingProvider = true;
			org.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			org.Factory.Save();

			var billOfLadingImage = new BillOfLadingImage();
			billOfLadingImage.PrincipalPK = org.PK;
			billOfLadingImage.Description = "Bill Of Image Description";
			billOfLadingImage.Enabled = true;
			billOfLadingImage.Image = new Bitmap(1, 1);
			return billOfLadingImage;
		}

		#endregion
	}
}
