using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class IsExpiryDateCorrectTest : WhsSecureServiceTestCase
	{
		#region TestIsExpiryDateCorrect

		[TestDate(2014, 03, 01)]
		public void TestIsExpiryDateCorrect()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("S2", "S2");

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YDDD;

			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParam.W3_ExpiryNotificationPeriod = 60;

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response1 = webService1.IsExpiryDateCorrect(data.Part1.PK.ToGuid(), data.Org1.OH_Code, new DateTime(2014, 05, 01));
			AssertEquals(null, response1.ErrorMessage);

			var webService2 = GetNewWebService();
			SetupSecurityHeader(webService2, data.Whs1, staff);
			var response2 = webService2.IsExpiryDateCorrect(data.Part1.PK.ToGuid(), data.Org1.OH_Code, new DateTime(2014, 04, 01));
			AssertEquals("The Expiry Date allows for less days before expiry, than the nominated minimum shelf life requirement i.e. Expiry Notification Period (Days).", response2.ErrorMessage);
		}

		#endregion

		[TestDate(2019, 06, 04)]
		public void TestIsExpiryDate_ReturnWarinigMessageBasedOnDateRangeValidationLimits()
		{
			var now = ZDateTime.Now.ToDateTime();
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("S2", "S2");

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YDDD;

			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response1 = webService1.IsExpiryDateCorrect(data.Part1.PK.ToGuid(), data.Org1.OH_Code, now.AddDays(+1));
			AssertEquals(null, response1.ErrorMessage);
			AssertEquals(ErrorTypes.None, response1.Error);

			var webService2 = GetNewWebService();
			SetupSecurityHeader(webService2, data.Whs1, staff);
			var response2 = webService2.IsExpiryDateCorrect(data.Part1.PK.ToGuid(), data.Org1.OH_Code, now.AddDays(-1));
			AssertEquals("Product has already expired. Are you sure this is correct?", response2.ErrorMessage);
			AssertEquals(ErrorTypes.YesNoEnquiry, response2.Error);
		}
	}
}
