using System;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region PickAndPossiblyReleaseCaptureAndPossiblyPack

		[WebMethod(Description = "Confirm pick lines qty")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsConfirmPickLineQtyWebServiceResponse PickAndPossiblyReleaseCaptureAndPossiblyPack(PickLinesToPickedPackTypeInfo[] pickLinesToPickedPackTypes, PickingInfo pickingInfo, PackageInfo packagePackInto, bool createPackagesForPickedPacks)
		{
			return HandleWebServiceRequest<WhsConfirmPickLineQtyWebServiceResponse>(response => PickAndPossiblyReleaseCaptureAndPossiblyPack(response, pickLinesToPickedPackTypes, pickingInfo, packagePackInto, createPackagesForPickedPacks));
		}

		void PickAndPossiblyReleaseCaptureAndPossiblyPack(WhsConfirmPickLineQtyWebServiceResponse response, PickLinesToPickedPackTypeInfo[] pickLinesToPickedPackTypes, PickingInfo pickingInfo, PackageInfo packagePackInto, bool createPackagesForPickedPacks)
		{
			if (pickingInfo.IsPickingSuspended)
			{
				SuspendPickingCore(response);
			}
			else if (pickingInfo.PickedQty == 0m && pickingInfo.ShouldSplit) // From RF this combination never sent to this service
			{
				response.LogBusinessValidationError(Res.GetString("d3cf5f70-787d-4c4d-8705-6855950cff7c", "Can only Pick zero units when shorting a Pick."));
			}

			if (createPackagesForPickedPacks && packagePackInto != null)
			{
				throw new ArgumentException("Should not attempt to create new packages for picked pack types when performing Pick and Pack.");
			}

			var pickWasSuspendedOnly = (pickingInfo.PickedQty == 0m && pickingInfo.ShouldSplit && pickingInfo.IsPickingSuspended);

			if (response.NoError() && !pickWasSuspendedOnly)
			{
				var pickLinePKs = pickLinesToPickedPackTypes.SelectMany(p => p.PickLinePKs);
				var pickLines = Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.PK, pickLinePKs));
				if (pickLines.Length > 0)
				{
					var package = packagePackInto != null
						? GetPackageToPack(response, pickLines[0].DocketLine.Docket as WhsOrder, packagePackInto)
						: null;

					if (packagePackInto == null || package != null)
					{
						ConfirmPickedQuantity(response, pickingInfo, pickLinesToPickedPackTypes, pickLines, package, createPackagesForPickedPacks);
					}
				}
				else
				{
					response.LogBusinessValidationError(Res.GetString("a2bcf657-cf7a-4f57-b8cf-b71eafbec758", "Pick Line could not be found"));
				}
			}
		}

		static PkgPackage GetPackageToPack(WebServiceResponse response, WhsOrder order, PackageInfo packagePackInto)
		{
			PkgPackage result = null;

			if (order == null && packagePackInto != null)
			{
				response.LogBusinessValidationError(Res.GetString("fac64b5f-a554-478b-a460-702cd3c688a8", "No Warehouse Order found."));
			}
			else
			{
				result = PackageHelper.GetPackageAndValidate(response, packagePackInto.PackageID, packagePackInto.PK, order.Factory);
			}

			return result;
		}

		void ConfirmPickedQuantity(
			WhsConfirmPickLineQtyWebServiceResponse response,
			PickingInfo pickingInfo,
			PickLinesToPickedPackTypeInfo[] pickLinesToPickedPackTypes,
			WhsPickLine[] pickLines,
			PkgPackage package,
			bool createPackagesForPickedPacks)
		{
			var user = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			try
			{
				var shortedOrderLinePKs = PickLineUpdater.ConfirmPickLinesPickedQty(pickLines, pickingInfo, user, pickLinesToPickedPackTypes, package, createPackagesForPickedPacks);
				WebServiceHelper.SaveFactoryWithExceptionHandling(user.Factory, response, (concurrencyException) => Res.GetString("2b473ae9-28ec-4cb5-8904-f3d04a8a975a", "While you have been working with this job another user has made changes. Please restart the operation and try again."));
				response.ShortedOrderLinePKs = shortedOrderLinePKs.ToArray();
			}
			catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
			{
				response.LogBusinessValidationError(ex.Message);
			}
		}

		#endregion
	}
}
