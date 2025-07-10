namespace Enterprise.Warehouse.Web.WebService
{
	public class AssignPackerToOrderWebServiceResponse : WebServiceResponse
	{
		public bool IsAssignedToAnotherPacker { get; set; }
		public string AssignedPackerCode { get; set; }
	}
}
