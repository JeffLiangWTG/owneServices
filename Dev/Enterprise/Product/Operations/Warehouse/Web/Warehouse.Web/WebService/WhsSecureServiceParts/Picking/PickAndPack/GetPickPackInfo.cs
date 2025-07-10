using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region GetPickPackInfo

		[WebMethod(Description = "Get Pick and Pack Information")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public PickAndPackInfoWebServiceResponse GetPickPackInfo(string orderDocketID)
		{
			return HandleWebServiceRequest<PickAndPackInfoWebServiceResponse>(r => SetPickPackInfo(r, orderDocketID));
		}

		void SetPickPackInfo(PickAndPackInfoWebServiceResponse response, string orderDocketID)
		{
			var order = WebServiceHelper.GetOrderByDocketID(Factory, response, SecurityHeader.WarehouseCode, orderDocketID);
			if (order != null)
			{
				var pickPackParameter = WebServiceHelper.GetPickPackParameter(order);
				if (pickPackParameter != null)
				{
					var packages = order.PackageJob.Packages;

					response.DefaultPackType = order.PackageJob.GetDefaultOuterPackType();
					response.PackTypes = GetRefPackTypeCodeDescriptionPairsHelper.GetAsCodeDescriptionPair(SecurityHeader.IsAndroidDevice, Factory).Select(p => new CodeDescriptionPairInfo(p)).ToArray();
					response.ExistingPackages = packages.Find(GetPackagesQuery(false)).Select(p => new PackageInfo(p)).ToArray();
					if (pickPackParameter.WPP_IsUsingOwnLabel)
					{
						response.IsUsingOwnLabel = pickPackParameter.WPP_IsUsingOwnLabel;
						response.ExistingClosedPackages = packages.Find(GetPackagesQuery(true)).Select(p => new PackageInfo(p)).ToArray();
					}

					response.PromptForWeightAndDims = pickPackParameter.WPP_PromptForWeightAndDimensions;
					response.SupportsCarrierLabelIntegration = order.CarrierBookingAgent != null;
				}
			}
		}

		ZQuery GetPackagesQuery(bool isClosed)
		{
			var closedPackagesQuery = new ZQuery();
			closedPackagesQuery.AddToFilter(PkgPackageSchema.KP_ClosedTimeUtc, isClosed ? SQLComparisonOperator.NotEqual : SQLComparisonOperator.Equal, null);
			closedPackagesQuery.AddToFilter(JoinCondition.Or, PkgPackageSchema.KP_IsClosed, SQLComparisonOperator.Equal, isClosed ? 1 : 0);

			return closedPackagesQuery;
		}

		#endregion
	}
}
