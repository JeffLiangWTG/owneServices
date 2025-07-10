using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Moq;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class ValidateWebServiceAndWarehouseAndStaffTest : WhsSecureServiceTestCase
	{
		#region TestValidateWebServiceAndStaffAndWarehouse

		public void TestValidateWebServiceAndStaffAndWarehouse()
		{
			var warehouse1 = Helper.CreateWarehouse("W1");
			var staff1 = Helper.CreateGlbStaff("ST1", "ST1");
			staff1.StaffPlainTextPassword = "secret";
			Helper.Factory.Save();

			var response = new WebServiceResponse();
			var webService = GetNewWebService(warehouse1, staff1, "secret");
			AssertNotNullOrEmpty(staff1.GS_LoginName);
			AssertEquals(webService.SecurityHeader.UserName, staff1.GS_LoginName);
			var encryptedPassword = GetEncryptedText("secret");
			AssertNotNullOrEmpty(encryptedPassword);
			AssertEquals(webService.SecurityHeader.Password, encryptedPassword);
			AssertNotNullOrEmpty(warehouse1.WW_WarehouseCode);
			AssertEquals(webService.SecurityHeader.WarehouseCode, warehouse1.WW_WarehouseCode);

			var result = webService.ValidateWebServiceAndStaffAndWarehouse(response);

			Assert(result);
			AssertEquals(ErrorTypes.None, response.Error);
		}

		public void TestValidateWebServiceAndStaffAndWarehouse_NoSecurityHeaders()
		{
			var response = new WebServiceResponse();
			var webService = GetNewWebService();

			var result = webService.ValidateWebServiceAndStaffAndWarehouse(response);

			Assert("Validation should be failed due to no security headers.", !result);
			AssertEquals(ErrorTypes.LoginFailed, response.Error);
			AssertEquals("Please provide login credentials to use this service.", response.ErrorMessage);
		}

		public void TestValidateWebServiceAndStaffAndWarehouse_InvalidWarehouseCode()
		{
			var warehouse1 = Helper.CreateWarehouse("W1");
			var staff1 = Helper.CreateGlbStaff("ST1", "ST1");
			staff1.StaffPlainTextPassword = "password";
			Helper.Factory.Save();

			var response = new WebServiceResponse();
			var webService = GetNewWebService(warehouse1, staff1, "password");
			webService.SecurityHeader.WarehouseCode = "XXX";

			var result = webService.ValidateWebServiceAndStaffAndWarehouse(response);

			Assert("Validation should be failed due to invalid warehouse code", !result);
			AssertEquals(ErrorTypes.LoginFailed, response.Error);
			AssertEquals("Please provide login credentials to use this service.", response.ErrorMessage);
		}

		public void TestValidateWebServiceAndStaffAndWarehouse_InvalidUser()
		{
			var warehouse1 = Helper.CreateWarehouse("W1");
			var staff1 = Helper.CreateGlbStaff("ST1", "ST1");
			Helper.Factory.Save();

			var response = new WebServiceResponse();
			var webService = GetNewWebService(warehouse1, staff1);
			webService.SecurityHeader.UserName = "anonymous";

			var result = webService.ValidateWebServiceAndStaffAndWarehouse(response);

			Assert("Validation should be failed due to no security headers.", !result);
			AssertEquals(ErrorTypes.LoginFailed, response.Error);
			AssertEquals("You must enter a correct user name and/or password. Please try again.", response.ErrorMessage);
		}

		public void TestValidateWebServiceAndStaffAndWarehouse_InvalidPassword()
		{
			var warehouse1 = Helper.CreateWarehouse("W1");
			var staff1 = Helper.CreateGlbStaff("ST1", "ST1");
			Helper.Factory.Save();

			var response1 = new WebServiceResponse();
			var webService1 = GetNewWebService(warehouse1, staff1);
			webService1.SecurityHeader.Password = "helloworld";

			AssertExceptionThrown<FormatException>(() =>
			{
				webService1.ValidateWebServiceAndStaffAndWarehouse(response1);
			});

			var response2 = new WebServiceResponse();
			var webService2 = GetNewWebService(warehouse1, staff1);
			webService2.SecurityHeader.Password = GetEncryptedText("helloworld");

			var result2 = webService2.ValidateWebServiceAndStaffAndWarehouse(response2);

			Assert("Validation should be failed due to wrong password.", !result2);
			AssertEquals(ErrorTypes.LoginFailed, response2.Error);
			AssertEquals("You must enter a correct user name and/or password. Please try again.", response2.ErrorMessage);
		}

		[NUnit.Framework.TestDate(2007, 10, 10, 10, 10, 10)]
		public void TestValidateLoginReturnsWarehouseDateAndTime()
		{
			var warehouse = Helper.CreateWarehouse("W1");
			Helper.Factory.Save();

			var webService = GetNewWebService(warehouse);
			var response = webService.ValidateLogin();
			AssertSuccessfulResponse(response, webService);
			AssertEquals(new DateTime(2007, 10, 10, 10, 10, 10), response.WarehouseDateTime);
		}

		public void TestValidateLoginReturnsCompanyAndLicenceInfo()
		{
			var warehouse = Helper.CreateWarehouse("W1");
			Helper.Factory.Save();

			var productRegisterMock = new Mock<IProductRegistration>();
			var licenceKeyMock = new Mock<IProductRegistrationKey>();
			productRegisterMock.Setup(pr => pr.Key).Returns(licenceKeyMock.Object);
			productRegisterMock.Setup(pr => pr.IsWiseTechGlobalInternalSystem()).Returns(true);

			licenceKeyMock.Setup(lk => lk.ServerCode).Returns("SYDCO-Test");
			licenceKeyMock.Setup(lk => lk.EnterpriseCode).Returns("WTG");

			using (ObjectFactory.Substitute(productRegisterMock.Object))
			using (ObjectFactory.Substitute(licenceKeyMock.Object))
			{
				var company = Helper.Factory.New<GlbCompany>();
				company.GC_Code = "CO2";
				company.CompanyName = "Code Opening 2nd";
				company.GC_RN_NKCountryCode = "NZ";

				var branch = company.Branches.AddNew();
				branch.GB_Code = "BR2";

				Helper.Factory.Save();

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				using (var webService = GetNewWebService())
				{
					var response = webService.ValidateLogin();

					CombineAssertions(() =>
					{
						AssertSuccessfulResponse(response, webService);

						AssertEquals("Code Opening 2nd", response.CurrentCompany);
						AssertEquals("CO2", response.CurrentCompanyCode);
						AssertEquals("NZ", response.CurrentCompanyCountryCode);
						AssertEquals("WTG", response.ClientEnterpriseCode);
						AssertEquals("SYDCO-Test", response.ClientLicenceServerID);
					});
				}
			}
		}

		public void TestValidateLogin_ReturnsRFInActivityLimit()
		{
			using (WarehouseDataRegistry.Instance.RFInactivityPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 15))
			{
				var warehouse = Helper.CreateWarehouse("W1");
				Helper.Factory.Save();

				var webService = GetNewWebService(warehouse);
				var response = webService.ValidateLogin();
				AssertSuccessfulResponse(response, webService);
				AssertEquals(15, response.RFInactivityLimit);
			}
		}

		#endregion
	}
}
