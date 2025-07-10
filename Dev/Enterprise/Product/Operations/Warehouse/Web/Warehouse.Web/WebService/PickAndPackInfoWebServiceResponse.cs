using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class PickAndPackInfoWebServiceResponse : WebServiceResponse
	{
		public PickAndPackInfoWebServiceResponse()
		{
			DefaultPackType = "";
		}

		public string DefaultPackType
		{
			get;
			set;
		}

		public CodeDescriptionPairInfo[] PackTypes
		{
			get;
			set;
		}

		public PackageInfo[] ExistingPackages
		{
			get;
			set;
		}

		public PackageInfo[] ExistingClosedPackages
		{
			get;
			set;
		}

		public bool IsUsingOwnLabel
		{
			get;
			set;
		}

		public bool PromptForWeightAndDims { get; set; }

		public bool SupportsCarrierLabelIntegration { get; set; }
	}
}
