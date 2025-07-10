using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgWebURLValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPU_IsPrimary()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgWebURL url1 = org.OrgWebURLs.AddNew();
			url1.PU_IsPrimary = true;
			url1.Validation.ValidatePU_IsPrimary();
			AssertNoErrors(url1.PU_IsPrimaryInfo);

			OrgWebURL url2 = org.OrgWebURLs.AddNew();
			url2.PU_IsPrimary = true;
			url2.Validation.ValidatePU_IsPrimary();
			AssertHasErrors(url2.PU_IsPrimaryInfo);
		}

		public void TestCheckPU_Type()
		{
			WebUrl.PU_Type = "";
			Assert("Error - type is mandatory", WebUrl.PU_TypeInfo.HasErrors());

			WebUrl.PU_Type = "xxx";
			Assert("No Error - type entered", !WebUrl.PU_TypeInfo.HasErrors());
		}

		#region TestMainDefaultAdded

		public void TestMainDefaultAdded()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "aaa";
			org.OH_FullName = "name";
			org.OH_RL_NKClosestPort = "UAIEV";
			org.MainAddress.OA_Address1 = "abcd";
			org.MainAddress.OA_PostCode = "123";

			org.RunPreSaveValidation();
			AssertNoErrors(org);

			OrgWebURL url1 = org.OrgWebURLs.AddNew();
			url1.PU_IsPrimary = true;
			url1.MainDefaultAdded = false;

			url1.Validation.ValidateMainDefaultAdded();
			AssertHasErrors(url1.MainDefaultAddedInfo);

			OrgWebURL url2 = org.OrgWebURLs.AddNew();
			url2.PU_IsPrimary = false;
			url2.Validation.ValidateMainDefaultAdded();
			AssertHasErrors(url2.MainDefaultAddedInfo);

			url1.PU_URL = "www.aaa.vvv";
			url1.Validation.ValidateMainDefaultAdded();

			AssertNoErrors(url1.MainDefaultAddedInfo);
		}

		#endregion

		#region TestCartageUrlContainsTrackingParameter

		public void TestCartageUrlContainsTrackingParameter()
		{
			WebUrl.PU_Type = OrgWebUrlList.Codes.CartageTracking;
			AssertNoErrors(WebUrl.PU_URLInfo);

			WebUrl.PU_URL = "www.freight.com";
			AssertHasError(WebUrl.PU_URLInfo, string.Format("The {0} web address is missing the text " + Constants.TransportCoHotlinkOpener.CargoWiseREF + ".\r\nThe text is required to enable requests' forwarding to {0} site.\r\nFor example: \"http://tracking.cargowise.com/Login/Login.aspx?QuickViewNumber={1}\".", OrgWebUrlList.Descriptions.CartageTracking, Constants.TransportCoHotlinkOpener.CargoWiseREF));

			WebUrl.PU_Type = "";
			WebUrl.Validation.ValidatePU_URL();
			AssertNoErrors(WebUrl.PU_URLInfo);

			webUrl.PU_Type = OrgWebUrlList.Codes.CartageTracking;
			WebUrl.PU_URL = "www.freight.com?tracking=" + Constants.TransportCoHotlinkOpener.CargoWiseREF;
			AssertNoErrors(WebUrl.PU_URLInfo);
		}

		#endregion

		#region Implementation

		OrgWebURL WebUrl
		{
			get { return webUrl ?? (webUrl = Factory.NewWithValidTestData<OrgWebURL>()); }
		}

		OrgWebURL webUrl;

		#endregion
	}
}
