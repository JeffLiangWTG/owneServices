using System;
using System.Drawing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	class AgencyHouseBillTermsAndConditionsTest : TestCaseWithFactory
	{
		public void TestImage()
		{
			var principalPK = CreatePrincipal("Test1");

			var billOfLadingImageCollection = new BillOfLadingImageCollection();
			var billOfLadingImage = billOfLadingImageCollection.AddNew();
			billOfLadingImage.PrincipalPK = principalPK;
			billOfLadingImage.Description = (NoResString)"XYZ";
			billOfLadingImage.Image = new Bitmap(10, 10);
			billOfLadingImage.Enabled = true;

			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_OH_DeliveryAgent = principalPK;

			using (AgencyRegistry.Instance.BillOfLadingTermsAndConditionsImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, billOfLadingImageCollection))
			{
				AssertNotNull(new AgencyHouseBillTermsAndConditions(billOfLading).Image);

				billOfLading.JS_OH_DeliveryAgent = ZGuid.Empty;
				AssertNull(new AgencyHouseBillTermsAndConditions(billOfLading).Image);
			}

			billOfLading.JS_OH_DeliveryAgent = principalPK;
			billOfLadingImage.Enabled = false;

			using (AgencyRegistry.Instance.BillOfLadingTermsAndConditionsImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, billOfLadingImageCollection))
			{
				AssertNull(new AgencyHouseBillTermsAndConditions(billOfLading).Image);
			}
		}

		ZGuid CreatePrincipal(ZString code)
		{
			var principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = code;
			principal.OH_IsShippingProvider = true;

			var companyData = principal.CompanyData;
			companyData.OB_CRIsShipsAgencyPrincipal = true;
			Factory.Save();

			return principal.PK;
		}
	}
}
