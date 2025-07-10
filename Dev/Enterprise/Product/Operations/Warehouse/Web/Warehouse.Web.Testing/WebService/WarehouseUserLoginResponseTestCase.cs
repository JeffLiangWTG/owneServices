using System;
using Enterprise.Warehouse.Web.WebService.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class WarehouseUserLoginResponseTestCase : WebServiceResponseTestCase
	{
		#region TestWarehouseDateTime

		public void TestWarehouseDateTime()
		{
			var response = new WarehouseUserLoginResponse();
			AssertEquals(new DateTime(), response.WarehouseDateTime);

			var dateTime = new DateTime(2019, 10, 10, 10, 10, 10);
			response.WarehouseDateTime = dateTime;
			AssertEquals(dateTime, response.WarehouseDateTime);
		}

		#endregion

		#region TestWarehouseScanAll

		public void TestDefaultScanAll_ScanAll() => TestDefaultScanAll(true);
		public void TestDefaultScanAll_ScanQty() => TestDefaultScanAll(false);
		void TestDefaultScanAll(bool setting)
		{
			var response = new WarehouseUserLoginResponse();
			AssertEquals(false, response.DefaultScanAll);

			response.DefaultScanAll = setting;
			AssertEquals(setting, response.DefaultScanAll);
		}

		#endregion

		#region TestRFInactivityLimit

		public void TestRFInactivityLimit()
		{
			var response = new WarehouseUserLoginResponse();
			AssertEquals(0, response.RFInactivityLimit);

			response.RFInactivityLimit = 10;
			AssertEquals(10, response.RFInactivityLimit);
		}

		#endregion

		#region TestVolcamEnabled

		public void TestVolcamEnabled_True() => TestVolcamEnabled(true);
		public void TestVolcamEnabled_False() => TestVolcamEnabled(false);
		void TestVolcamEnabled(bool setting)
		{
			var response = new WarehouseUserLoginResponse();
			AssertEquals(false, response.RFVolcamEnabled);

			response.RFVolcamEnabled = setting;
			AssertEquals(setting, response.RFVolcamEnabled);
		}

		#endregion

		#region TestCurrentCompany

		public void TestCurrentCompany()
		{
			var response = new WarehouseUserLoginResponse();
			AssertEquals(null, response.CurrentCompany);

			response.CurrentCompany = "Test Company";
			AssertEquals("Test Company", response.CurrentCompany);
		}

		#endregion

		#region TestCurrentCompanyCode

		public void TestCurrentCompanyCode()
		{
			var response = new WarehouseUserLoginResponse();
			AssertEquals(null, response.CurrentCompanyCode);

			response.CurrentCompanyCode = "TST";
			AssertEquals("TST", response.CurrentCompanyCode);
		}

		#endregion

		#region TestCurrentCompanyCountryCode

		public void TestCurrentCompanyCountryCode()
		{
			var response = new WarehouseUserLoginResponse();
			AssertEquals(null, response.CurrentCompanyCountryCode);

			response.CurrentCompanyCountryCode = "NZ";
			AssertEquals("NZ", response.CurrentCompanyCountryCode);
		}

		#endregion

		#region TestClientEnterpriseCode

		public void TestClientEnterpriseCode()
		{
			var response = new WarehouseUserLoginResponse();
			AssertEquals(null, response.ClientEnterpriseCode);

			response.ClientEnterpriseCode = "RFL";
			AssertEquals("RFL", response.ClientEnterpriseCode);
		}

		#endregion

		#region TestClientLicenceServerID

		public void TestClientLicenceServerID()
		{
			var response = new WarehouseUserLoginResponse();
			AssertEquals(null, response.ClientLicenceServerID);

			response.ClientLicenceServerID = "RED";
			AssertEquals("RED", response.ClientLicenceServerID);
		}

		#endregion

		#region TestEnabledFeatureFlags

		public void TestEnabledFeatureFlags()
		{
			var response = new WarehouseUserLoginResponse();
			AssertEquals(null, response.EnabledFeatureFlags);

			response.EnabledFeatureFlags = new[] { 1, 0, 3 };
			AssertEquals(1, response.EnabledFeatureFlags[0]);
			AssertEquals(0, response.EnabledFeatureFlags[1]);
			AssertEquals(3, response.EnabledFeatureFlags[2]);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WarehouseUserLoginResponse();
		}

		#endregion
	}
}
