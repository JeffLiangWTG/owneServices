using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsDocketWebServiceResponse : WhsDocketWebServiceBaseResponse
	{
		#region Receive

		public WhsDocketInfo Docket
		{
			get { return docket ?? (docket = new WhsDocketInfo()); }
			set { docket = value; }
		}

		WhsDocketInfo docket;

		#endregion
	}
}
