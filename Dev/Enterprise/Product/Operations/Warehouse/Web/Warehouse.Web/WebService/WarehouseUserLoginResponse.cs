using System;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WarehouseUserLoginResponse : WebServiceResponse
	{
		public WarehouseUserLoginResponse()
			: base()
		{
		}

		#region Properties

		public DateTime WarehouseDateTime { get; set; }

		public bool DefaultScanAll { get; set; }

		public bool RFVolcamEnabled { get; set; }

		public string CurrentCompany { get; set; }

		public string CurrentCompanyCode { get; set; }

		public string CurrentCompanyCountryCode { get; set; }

		public string ClientEnterpriseCode { get; set; }

		public string ClientLicenceServerID { get; set; }

		public int[] EnabledFeatureFlags { get; set; }

		public int RFInactivityLimit { get; set; }

		#endregion
	}
}
