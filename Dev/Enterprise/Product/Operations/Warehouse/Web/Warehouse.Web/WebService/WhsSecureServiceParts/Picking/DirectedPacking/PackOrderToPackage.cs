using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		[WebMethod(Description = "Pack Order to Package")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public PackOrderToPackageWebServiceResponse PackOrderToPackage(PackageForPackingInfo packageInfo)
		{
			return HandleWebServiceRequest<PackOrderToPackageWebServiceResponse>(r => PackOrderToPackageCore(r, packageInfo));
		}

		void PackOrderToPackageCore(PackOrderToPackageWebServiceResponse response, PackageForPackingInfo packageInfo)
		{
			if (packageInfo == null)
			{
				response.LogBusinessValidationError(Res.GetString("d7f1f55d-2fab-4a4a-84ce-cb8016bc3185", "Please provide a valid Package Info."));
			}
			else if (packageInfo.IsTote && packageInfo.ToteID.IsNullOrEmpty())
			{
				response.LogBusinessValidationError(Res.GetString("dc17ecc3-7a07-41f1-b32a-105a5b7156f4", "Empty Tote ID."));
			}
			else
			{
				var order = WebServiceHelper.GetOrderByDocketID(Factory, response, SecurityHeader.WarehouseCode, packageInfo.DocketID);
				if (response.NoError())
				{
					var packageJob = Factory.LoadTop1<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, order.PK));
					if (packageInfo.IsTote)
					{
						PackOrderIntoTote(response, packageInfo, order, packageJob);
					}
					else
					{
						PackOrderIntoCarton(response, packageInfo, order, packageJob);
					}
				}
			}

			if (response.NoError())
			{
				WebServiceHelper.SaveFactoryWithExceptionHandling(
					Factory,
					response,
					(concurrencyException) => Res.GetString("9376e40b-da11-4033-a5a7-bd79243937dd", "While you have been working with this order another user has made changes. Please restart the operation and try again."));
			}
		}

		#region PackOrderIntoTote

		void PackOrderIntoTote(PackOrderToPackageWebServiceResponse response, PackageForPackingInfo packageInfo, WhsOrder order, PkgPackageJob packageJob)
		{
			ValidateToteForOrderDirectedPacking(response, packageInfo.ToteID, packageJob);
			CreateToteAndPackOrderToTote(response, packageInfo, order, packageJob);
		}

		void CreateToteAndPackOrderToTote(PackOrderToPackageWebServiceResponse response, PackageForPackingInfo packageInfo, WhsOrder order, PkgPackageJob packageJob)
		{
			if (response.NoError())
			{
				var package = CreateWarehouseTote(packageInfo.ToteID, packageJob);
				PackOrderUnpackedPackableItemsIntoPackage(response, packageInfo, order, package, packageJob);
				if (response.NoError())
				{
					response.NewPackagePK = package.PK.ToGuid();
				}
			}
		}

		#endregion

		#region PackOrderIntoCarton

		void PackOrderIntoCarton(PackOrderToPackageWebServiceResponse response, PackageForPackingInfo packageInfo, WhsOrder order, PkgPackageJob packageJob)
		{
			var carton = packageJob.Packages.AddNew(Constants.PkgUnit.Carton);
			SetCartonPackageDetails(carton, packageInfo);
			carton.KP_PackageQty = 1;
			UpdatePackageDimensions(carton, packageInfo);
			PackOrderUnpackedPackableItemsIntoPackage(response, packageInfo, order, carton, packageJob);

			if (response.NoError())
			{
				response.NewPackagePK = carton.PK.ToGuid();

				ISupportPackageIDGeneration packageToGenerateIdFor = carton;
				packageToGenerateIdFor.ShouldGenerateIDOnSaving = true;

				packageToGenerateIdFor.AfterIDGenerated += AfterIDGenerated;

				void AfterIDGenerated(object sender, EventArgs e)
				{
					response.NewPackageID = carton.KP_PackageID;
					packageToGenerateIdFor.AfterIDGenerated -= AfterIDGenerated;
				}
			}
		}

		#endregion

		void PackOrderUnpackedPackableItemsIntoPackage(WebServiceResponse response, PackageForPackingInfo packageInfo, WhsOrder order, PkgPackage package, PkgPackageJob packageJob)
		{
			var orderLines = order.Lines.Cast<WhsOrderLine>().Where(l => !l.IsComponentLineOnSalesOrder).ToArray();
			var unPackedPickLines = orderLines.ToLookup(l => l.WE_OP, l => l.PickLines.Where(pl => !packageJob.IsPacked(pl)));

			WhsPickByBOMHelper.AddFetchHintsForIsPickByBOMKitPickLine(Factory, orderLines.SelectMany(l => l.PickLines));

			var releaseLines = orderLines.SelectMany(orderLine => orderLine.ReleaseLines).Cast<WhsReleaseLine>().ToDictionary(l => l.KeyForPacking, l => l);
			foreach (var productInfo in packageInfo.ScannedProductInfos)
			{
				var pickLines = unPackedPickLines[productInfo.ProductPK]?.SelectMany(l => l).ToArray();
				if (pickLines != null && pickLines.Length > 0)
				{
					var unPackedQty = PackPickLines(package, pickLines.OrderByDescending(pl => pl.WZ_Units), releaseLines, productInfo);

					if (unPackedQty > 0)
					{
						LogFailedToPackError();
						break;
					}
				}
				else
				{
					LogFailedToPackError();
				}

				void LogFailedToPackError()
				{
					var errorMsg = packageInfo.ToteID.IsNullOrEmpty()
						? Res.GetString("032f3cd5-4c8b-4eab-9e8b-aa770f335465", "Failed to pack Order '{0}' into carton.", packageInfo.OrderReference)
						: Res.GetString("05dbc625-1f86-4419-a257-e5c3d1aa8634", "Failed to pack Order '{0}' into '{1}'.", packageInfo.OrderReference, packageInfo.ToteID);
					response.LogBusinessValidationError(errorMsg);
				}
			}
		}

		static decimal PackPickLines(PkgPackage package, IEnumerable<WhsPickLine> pickLines, Dictionary<GroupingKey, WhsReleaseLine> releaseLines, WhsPackageProductInfo productInfo)
		{
			var unPackedQty = productInfo.Quantity;
			foreach (IPackableItem packableItem in pickLines)
			{
				var releaseLine = releaseLines[packableItem.Key];
				if (unPackedQty >= packableItem.Quantity)
				{
					package.Pack(packableItem, releaseLine);
					unPackedQty -= packableItem.Quantity;
				}
				else
				{
					var newPackableItem = packableItem.Split(unPackedQty);
					package.Pack(newPackableItem, releaseLine);
					unPackedQty = 0;
				}

				if (unPackedQty == 0)
				{
					break;
				}
			}

			return unPackedQty;
		}
	}
}
