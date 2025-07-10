using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region ShortTote

		[WebMethod(Description = "Short Tote to provided items.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse ShortTote(PackageForPackingInfo packageInfo)
		{
			return HandleWebServiceRequest<WebServiceResponse>(r => ShortTote(r, packageInfo));
		}

		void ShortTote(WebServiceResponse response, PackageForPackingInfo packageInfo)
		{
			if (response.ValidateShouldNotBeNull(packageInfo, nameof(packageInfo)) && response.ValidateShouldNotBeNull(packageInfo.ScannedProductInfos, nameof(packageInfo.ScannedProductInfos)))
			{
				var toteId = packageInfo.ToteID.ToUpper(Culture.Invariant);
				var order = WebServiceHelper.GetOrderByDocketID(Factory, response, SecurityHeader.WarehouseCode, packageInfo.DocketID);
				var package = PackageHelper.GetPackageAndValidateForClosing(response, toteId, packageInfo.PK, Factory);

				if (package != null)
				{
					if (package.GetIsTote())
					{
						foreach (var scannedProductInfo in packageInfo.ScannedProductInfos.Where(p => p.ExpectedQty > p.Quantity))
						{
							ShortAndRepackProduct(order, scannedProductInfo, package);
						}
						UnpackUnscannedProducts(order, packageInfo.ScannedProductInfos.Select(i => i.ProductPK), package);

						package.Factory.Save();
					}
					else
					{
						response.LogBusinessValidationError(Res.GetString("29ca3f3c-adaf-41a5-a052-ae800e8f53b7", "The package is not a tote."));
					}
				}
			}
		}

		void ShortAndRepackProduct(WhsOrder order, WhsPackageProductInfo productInfo, PkgPackage package)
		{
			var orderLines = order.Lines.Cast<WhsOrderLine>().Where(line => line.WE_OP == productInfo.ProductPK).ToArray();
			var divots = PackageHelper.GetAllPackageDivotsForProduct(orderLines, package);
			var qtyToPack = productInfo.Quantity;

			foreach (var divot in divots)
			{
				var packableItem = divot.PackedItem;
				var releaseLine = GetDivotReleaseLine(orderLines, divot);

				if (qtyToPack > 0)
				{
					if (qtyToPack >= packableItem.Quantity)
					{
						qtyToPack -= packableItem.Quantity;
					}
					else
					{
						divot.DeleteForRepacking(releaseLine);
						package.Pack(packableItem.Split(qtyToPack), releaseLine);
						qtyToPack = 0;
					}
				}
				else
				{
					divot.DeleteForRepacking(releaseLine);
				}
			}
		}

		WhsReleaseLine GetDivotReleaseLine(IEnumerable<WhsOrderLine> orderLines, PkgPackageItemDivot divot)
		{
			var releaseLines = orderLines.SelectMany(orderLine => orderLine.ReleaseLines).Cast<WhsReleaseLine>();
			return releaseLines.Single(rl => rl.KeyForPacking == divot.PackedItem.Key);
		}

		void UnpackUnscannedProducts(WhsOrder order, IEnumerable<Guid> scannedProductPKs, PkgPackage package)
		{
			var unscannedOrderLines = order.Lines.Cast<WhsOrderLine>().Where(l => !scannedProductPKs.Contains(l.WE_OP.ToGuid()));
			if (unscannedOrderLines.Any())
			{
				var divots = PackageHelper.GetAllPackageDivotsForProduct(unscannedOrderLines.ToArray(), package);

				foreach (var divot in divots)
				{
					divot.DeleteForRepacking(GetDivotReleaseLine(unscannedOrderLines, divot));
				}
			}
		}

		#endregion
	}
}
