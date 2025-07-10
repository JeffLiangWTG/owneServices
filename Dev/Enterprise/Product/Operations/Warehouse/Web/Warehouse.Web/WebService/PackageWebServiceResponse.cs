using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class PackageWebServiceResponse : WebServiceResponse
	{
		public PackageChoiceInfoCollection PackageChoices
		{
			get { return packageChoices ?? (packageChoices = new PackageChoiceInfoCollection()); }
			set { packageChoices = value; }
		}

		PackageChoiceInfoCollection packageChoices;
	}
}
