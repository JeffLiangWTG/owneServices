using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class IsJulianBatchNumberCorrectFormatTest : WhsSecureServiceTestCase
	{
		#region TestIsJulianBatchNumberCorrectFormat

		[TestDate(2013, 02, 27)]
		public void TestIsJulianBatchNumberCorrectFormat_InvalidClientCode()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("S2", "S2");

			Helper.Factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var response = webService.IsJulianBatchNumberCorrectFormat(Guid.Empty, "", "");
			AssertEquals(false, response.IsCorrectFormat);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Please provide a valid client code.", response.ErrorMessage);
			AssertEquals(ZDateTime.Empty, response.ExpiryDate);
		}

		[TestDate(2013, 02, 27)]
		public void TestIsJulianBatchNumberCorrectFormat_InvalidProductCode()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("S2", "S2");

			Helper.Factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var response = webService.IsJulianBatchNumberCorrectFormat(Guid.Empty, data.Org1.OH_Code, "");
			AssertEquals(false, response.IsCorrectFormat);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Please provide a valid product code.", response.ErrorMessage);
			AssertEquals(ZDateTime.Empty, response.ExpiryDate);
		}

		[TestDate(2013, 02, 27)]
		public void TestIsJulianBatchNumberCorrectFormat_InvalidBatchNumber()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("S2", "S2");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YDDD;
			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParam.W3_MaximumShelfLife = 10;

			Helper.Factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var response = webService.IsJulianBatchNumberCorrectFormat(data.Part1.PK.ToGuid(), data.Org1.OH_Code, "");
			AssertEquals(false, response.IsCorrectFormat);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals(Transactions.Business.PartAttributeValidation.JulianBatchNumberIncorrectFormatMessage, response.ErrorMessage);
			AssertEquals(ZDateTime.Empty, response.ExpiryDate);
		}

		[TestDate(2013, 02, 27)]
		public void TestIsJulianBatchNumberCorrectFormat()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("S2", "S2");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YDDD;
			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParam.W3_MaximumShelfLife = 10;

			Helper.Factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var response = webService.IsJulianBatchNumberCorrectFormat(data.Part1.PK.ToGuid(), data.Org1.OH_Code, "Z1111");
			AssertEquals(true, response.IsCorrectFormat);
			AssertEquals(null, response.ErrorMessage);
			AssertEquals(new DateTime(2010 + 1, 1, 1).AddDays((111 - 1) + 10), response.ExpiryDate);
			AssertEquals(new DateTime(2010 + 1, 1, 1).AddDays((111 - 1)), response.PackingDate);
		}

		#endregion
	}
}
