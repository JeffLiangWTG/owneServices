using System;
using System.Globalization;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		[WebMethod(Description = "Close Package for Packing")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public SinglePackageForPackingWebServiceResponse ClosePackageForPacking(PackageForPackingInfo packageInfo, Guid printerPK)
		{
			return HandleWebServiceRequest<SinglePackageForPackingWebServiceResponse>(r => ClosePackageForPackingCore(r, packageInfo, printerPK));
		}

		void ClosePackageForPackingCore(SinglePackageForPackingWebServiceResponse response, PackageForPackingInfo packageInfo, Guid printerPK)
		{
			if (packageInfo == null)
			{
				response.LogBusinessValidationError(Res.GetString("6ed38593-e058-4389-941b-34717d2bd6aa", "Please provide a valid Package Info."));
			}
			else
			{
				var order = WebServiceHelper.GetOrderByDocketID(Factory, response, SecurityHeader.WarehouseCode, packageInfo.DocketID);
				if (response.NoError())
				{
					var package = Factory.Load<PkgPackage>(packageInfo.PK);
					if (package == null)
					{
						response.LogBusinessValidationError(Res.GetString("04a5788a-3eb2-4439-8a58-82ec3c5d7112", "Tote # Or Package ID '{0}' cannot be found.", packageInfo.ToteID));
					}
					else
					{
						if (order.PackageJob.Packages.Contains(package))
						{
							var packageToPack = packageInfo.IsPackageSplitForPacking && !packageInfo.IsDirectedPacking ? SplitPackageForPacking(packageInfo.ScannedProductInfos, order, package) : package;
							if (package.GetIsTote())
							{
								UpdateTotePackage(response, order, packageToPack, packageInfo);
							}
							else
							{
								SetPackType(response, packageToPack, packageInfo);
							}

							if (response.Error == ErrorTypes.None)
							{
								UpdatePackageDimensions(packageToPack, packageInfo);

								if (ClosePackage(response, packageToPack, printerPK) && ValidateAndSave(response, order, packageToPack))
								{
									response.PackageForPackingInfo = GetNewPackageForPackingInfo(packageToPack, packageInfo);
								}
							}
						}
						else
						{
							response.LogBusinessValidationError(Res.GetString("09c4762d-cdc0-4dc2-9cd6-1af475b4edc1", "Package '{0}' cannot be found in Order '{1}'.", packageInfo.ToteID, packageInfo.DocketID));
						}
					}
				}
			}
		}

		PkgPackage SplitPackageForPacking(WhsPackageProductInfo[] productInfos, WhsOrder order, PkgPackage packageToSplit)
		{
			var newPackage = packageToSplit.PackageJob.Packages.AddNew();
			foreach (var productInfo in productInfos)
			{
				MovePackedItemsToNewPackage(order, productInfo, packageToSplit, newPackage);
			}

			return newPackage;
		}

		void MovePackedItemsToNewPackage(WhsOrder order, WhsPackageProductInfo productInfo, PkgPackage currentPackage, PkgPackage newPackage)
		{
			var orderLines = order.Lines.Cast<WhsOrderLine>().Where(line => line.WE_OP == productInfo.ProductPK).ToArray();
			var divots = Business.PackageHelper.GetAllPackageDivotsForProduct(orderLines, currentPackage);
			var qtyToMove = productInfo.Quantity;

			foreach (var divot in divots)
			{
				var packableItem = divot.PackedItem;
				var releaseLines = orderLines.SelectMany(orderLine => orderLine.ReleaseLines).Cast<WhsReleaseLine>();
				var releaseLine = releaseLines.Single(rl => rl.KeyForPacking == divot.PackedItem.Key);
				divot.DeleteForRepacking(releaseLine);

				if (qtyToMove >= packableItem.Quantity)
				{
					var packageToPack = newPackage;
					packageToPack.Pack(packableItem, releaseLine);
					qtyToMove -= packableItem.Quantity;
				}
				else
				{
					SplitAndMoveItemsToNewPackage(packableItem, releaseLine, qtyToMove, currentPackage, newPackage);
					qtyToMove = 0;
				}

				if (qtyToMove == 0)
				{
					break;
				}
			}
		}

		void SplitAndMoveItemsToNewPackage(IPackableItem packableItem, WhsReleaseLine releaseLine, decimal qtyToMove, PkgPackage currentPackage, PkgPackage newPackage)
		{
			SplitAndMovePickLine(packableItem, releaseLine, qtyToMove, currentPackage, newPackage);
		}

		void SplitAndMovePickLine(IPackableItem packableItem, WhsReleaseLine releaseLine, decimal qtyToMove, PkgPackage currentPackage, PkgPackage newPackage)
		{
			var newPackableItem = packableItem.Split(qtyToMove);
			currentPackage.Pack(packableItem, releaseLine);
			newPackage.Pack(newPackableItem, releaseLine);
		}

		void UpdateTotePackage(SinglePackageForPackingWebServiceResponse response, WhsOrder order, PkgPackage packageToPack, PackageForPackingInfo packageInfo)
		{
			UpdatePackTypeFromLinkedCartonSize(packageToPack, packageInfo);

			SetCartonPackageDetails(packageToPack, packageInfo);

			ISupportPackageIDGeneration packageToGenerateIdFor = packageToPack;
			packageToGenerateIdFor.ShouldGenerateIDOnSaving = true;

			packageToGenerateIdFor.AfterIDGenerated += AfterIDGenerated;

			void AfterIDGenerated(object sender, EventArgs e)
			{
				order.Logs.AddNew(ZArchitecture.Business.Events.WarehouseOrderPacking, string.Format(CultureInfo.CurrentCulture, "Transferred items from Tote {0} into Package {1}.", packageInfo.ToteID, packageToPack.KP_PackageID));
				packageToGenerateIdFor.AfterIDGenerated -= AfterIDGenerated;
			}
		}

		static void SetCartonPackageDetails(PkgPackage packageToPack, PackageForPackingInfo packageInfo)
		{
			packageToPack.KP_PackageID = ZString.Empty;
			packageToPack.SetIsTote(false);
			packageToPack.KP_WeightUQ = packageInfo.WeightUQ;
			packageToPack.KP_DimensionUQ = packageInfo.DimensionUQ;
			packageToPack.KP_IsHeld = false;
		}

		void SetPackType(SinglePackageForPackingWebServiceResponse response, PkgPackage packageToPack, PackageForPackingInfo packageInfo)
		{
			if (packageInfo.IsDirectedPacking)
			{
				UpdatePackTypeFromLinkedCartonSize(packageToPack, packageInfo);
			}
			else if (!string.IsNullOrEmpty(packageInfo.PackType))
			{
				if (packageToPack.Lookups.PackTypes.ContainsCode(packageInfo.PackType))
				{
					packageToPack.KP_F3_NKPackType = packageInfo.PackType;
					packageToPack.KP_WeightUQ = packageInfo.WeightUQ;
					packageToPack.KP_DimensionUQ = packageInfo.DimensionUQ;
				}
				else
				{
					response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("fca14e26-ac06-4cbb-ab5b-2221462ed023", "Attempt to set invalid Pack Type '{0}'.", packageInfo.PackType));
				}
			}
		}

		void UpdatePackTypeFromLinkedCartonSize(PkgPackage packageToPack, PackageForPackingInfo packageInfo)
		{
			var newPackType = Constants.PkgUnit.Carton;
			if (packageInfo.IsUsingCartonSizes && !string.IsNullOrEmpty(packageInfo.CartonSize))
			{
				var cartonSize = Factory.LoadTop1<WhsCartonSize>(new ZQuery(WhsCartonSizeSchema.WCS_Code, packageInfo.CartonSize));
				if (cartonSize != null)
				{
					newPackType = cartonSize.WCS_F3_NKPackType;
				}
			}

			if (packageToPack.KP_F3_NKPackType != newPackType)
			{
				packageToPack.KP_F3_NKPackType = newPackType;
			}
			else
			{
				packageToPack.PokePackageDataChangedEvent();
			}
		}

		void UpdatePackageDimensions(PkgPackage packageToPack, PackageForPackingInfo packageInfo)
		{
			if (!string.IsNullOrEmpty(packageInfo.WeightUQ))
			{
				packageToPack.KP_WeightUQ = packageInfo.WeightUQ;
			}

			if (!string.IsNullOrEmpty(packageInfo.DimensionUQ))
			{
				packageToPack.KP_DimensionUQ = packageInfo.DimensionUQ;
			}

			packageToPack.KP_TareWeight = packageInfo.EmptyWeight;
			packageToPack.KP_Weight = packageInfo.Weight;
			packageToPack.KP_Length = packageInfo.Length;
			packageToPack.KP_Width = packageInfo.Width;
			packageToPack.KP_Height = packageInfo.Height;
		}

		bool ClosePackage(SinglePackageForPackingWebServiceResponse response, PkgPackage packageToPack, Guid printerPK)
		{
			var printerName = GetPrinterNameFromPKString(printerPK);
			PickLineUpdater.ClosePackageWithPrinter(packageToPack, printerName, response);
			return response.NoError();
		}

		bool ValidateAndSave(SinglePackageForPackingWebServiceResponse response, WhsOrder order, PkgPackage packageToPack)
		{
			var result = false;
			if (order.HasErrors)
			{
				var errorMessage = new ZStringBuilder(Res.GetString("45153d1a-1022-4359-b243-96e9ce76fb92", "There are errors that need to be corrected before this package can be closed."));
				errorMessage.AppendLine(order.NotificationsIncludingChildren.GetErrors().ToUniqueMessageListString(System.Environment.NewLine));
				response.LogError(ErrorTypes.BusinessValidationError, errorMessage.ToStringWithNewLineBetweenAppends());
			}
			else
			{
				result = SaveWithExceptionHandling(response);
			}

			return result;
		}

		bool SaveWithExceptionHandling(SinglePackageForPackingWebServiceResponse response)
		{
			var saved = true;
			try
			{
				Factory.Save();
			}
			catch (ZSaveException saveEx)
			{
				saved = false;
				response.LogBusinessValidationError(
					Res.GetString("8db18989-3126-4f8b-9ab1-a0c0367bed4f", "Issue closing and saving Package:\r\n{0}", string.Join("\r\n", saveEx.InnerException.FriendlyMessage)));
			}
			catch (ZCannotSaveException ex)
			{
				saved = false;
				response.LogBusinessValidationError(ex.Message);
			}

			return saved;
		}

		string GetPrinterNameFromPKString(Guid printerPK)
		{
			var printerName = string.Empty;

			if (printerPK != Guid.Empty)
			{
				var printers = WhsCommonLookups.GetPrintersList(Factory);
				printerName = printers.Cast<IStmPrintQueue>().SingleOrDefault(p => p.PK == printerPK)?.SQ_DisplayName ?? string.Empty;
			}

			return printerName;
		}

		PackageForPackingInfo GetNewPackageForPackingInfo(PkgPackage packageToPack, PackageForPackingInfo packageInfo)
		{
			return new PackageForPackingInfo
			{
				PK = packageToPack.PK.ToGuid(),
				OrderReference = packageInfo.OrderReference,
				DocketID = packageInfo.DocketID,
				IsUsingCarrierLabelIntegration = packageInfo.IsUsingCarrierLabelIntegration,
				PackageID = packageToPack.KP_PackageID,
				ToteID = packageInfo.ToteID,
				PackType = packageToPack.KP_F3_NKPackType,
				Weight = packageToPack.KP_Weight,
				WeightUQ = packageToPack.KP_WeightUQ,
				PackageWeightTolerance = packageInfo.PackageWeightTolerance,
				PackageWeightToleranceEnabled = packageInfo.PackageWeightToleranceEnabled,
				ClientEnforceProductScan = packageInfo.ClientEnforceProductScan,
				Length = packageToPack.KP_Length,
				Width = packageToPack.KP_Width,
				Height = packageToPack.KP_Height,
				DimensionUQ = packageToPack.KP_DimensionUQ,
				IsTote = false,
				IsUsingCartonSizes = packageInfo.IsUsingCartonSizes,
				DocketStatus = packageInfo.DocketStatus,
				JobID = packageInfo.JobID,
				RequiredDate = packageInfo.RequiredDate,
				ClientCode = packageInfo.ClientCode,
				OrderIsUsingDirectedPackingConsolidation = packageInfo.OrderIsUsingDirectedPackingConsolidation,
				PackageRequiresPutaway = packageInfo.PackageRequiresPutaway,
				AssignedDockDoorLocationString = packageInfo.AssignedDockDoorLocationString,
				AssignedDockDoorLocationStringUserFriendly = packageInfo.AssignedDockDoorLocationStringUserFriendly,
				AssignedPutawayLocationClass = packageInfo.AssignedPutawayLocationClass,
				AssignedPutawayLocationString = packageInfo.AssignedPutawayLocationString,
				AssignedPutawayLocationStringUserFriendly = packageInfo.AssignedPutawayLocationStringUserFriendly,
				AllowedToOverrideDockDoorLocation = packageInfo.AllowedToOverrideDockDoorLocation,
			};
		}
	}
}
