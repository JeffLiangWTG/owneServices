using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.PickByLabel;

namespace Enterprise.Warehouse.Web.WebService.Common
{
	public static class PickByLabelHelper
	{
		// tested in GetPickByLabelPackageTest and CancelPickByLabelTest
		public static PkgPackage FindPickByLabelPackage(WebServiceResponse response, PackageFinder finder, string packageId)
		{
			if (!finder.FoundPackage)
			{
				if (finder.IsPickedAndPutaway)
				{
					response.LogError(ErrorTypes.BusinessValidationError,
						Res.GetString("2ca4e068-41d5-4f2c-9909-02e36dfb806d", "Label '{0}' is already picked and putaway.", packageId));
				}
				else
				{
					response.LogError(ErrorTypes.BusinessValidationError,
						Res.GetString("54df6e54-ddf9-45c4-b9ef-39022fcc1925", "Label '{0}' cannot be found or assigned to someone else.", packageId));
				}
			}

			return finder.Package;
		}
	}
}
