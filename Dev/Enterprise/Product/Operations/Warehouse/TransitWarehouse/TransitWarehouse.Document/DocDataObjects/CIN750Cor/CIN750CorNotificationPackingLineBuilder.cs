using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document
{
	public sealed class CIN750CorNotificationPackingLineBuilder : DocPackingLineBuilder
	{
		public CIN750CorNotificationPackingLineBuilder(CIN750CorNotification notification)
		{
			Notification = notification;
			ReceiveConsignment = Argument.NotNull(notification.SourceBusinessObject as WhsItemReceiveConsignment, nameof(notification.SourceBusinessObject));
		}

		readonly CIN750CorNotification Notification;
		readonly WhsItemReceiveConsignment ReceiveConsignment;

		public override DocPackingLine Build()
		{
			var docPackingLine = base.Build();
			var historyManager = new ReceiveConsignmentCIN750NotificationHistoryManager(ReceiveConsignment);
			Notification.InHistoryInfo = historyManager.InHistory;
			Notification.CorHistoryInfo = historyManager.CorHistory;
			Notification.RCNPackageStates = TransitDocumentHelper.GetRCNArrivedPackageStates(ReceiveConsignment);
			Notification.ReceivedPackageQuantity = GetSumQuantity(Notification.RCNPackageStates);
			var adjustedOutPackageStates = ReceiveConsignment.PackageStates.Where(p => !string.IsNullOrEmpty(p.WPS_AdjustedOut) && p.WPS_AdjustedOut != "BDS" && ValidReceivedAsCode.Contains(p.WPS_ReceivedAs)
				&& (p.HandlingUnit == null || p.HandlingUnit.ReceiveConsignment == null)).ToList();
			var corPackageStates = new List<WhsItemPackageState>();

			if (adjustedOutPackageStates.Count > 0)
			{
				foreach (var pkg in adjustedOutPackageStates)
				{
					if (pkg.WPS_UnitType == PackageStateUnitType.Codes.Overpack)
					{
						if (pkg.WPS_ReceivedAs == TransitWarehouseReceiveAs.Codes.ScannedIn)
						{
							corPackageStates.Add(pkg);
						}
						else
						{
							corPackageStates.AddRange(ReceiveConsignment.PackageStates.Where(p => !string.IsNullOrEmpty(p.WPS_AdjustedOut) && p.WPS_UnitType != PackageStateUnitType.Codes.Overpack
								&& (p.Package.KP_KP_TopHandlingUnitPackage == pkg.WPS_KP_Package || (p.Package.KP_KP_TopHandlingUnitPackage == pkg.Package.KP_KP_TopHandlingUnitPackage && pkg.TopHandlingUnit.ReceiveConsignment == null))));
						}
					}
					else
					{
						corPackageStates.Add(pkg);
					}
				}

				docPackingLine = CreatePackingLineFromPackageStates(corPackageStates);
				docPackingLine.AmountQuantity = -docPackingLine.AmountQuantity - Notification.CorHistoryInfo.Sum(i => i.Quantity);
				docPackingLine.AmountWeight = -docPackingLine.AmountWeight - Notification.CorHistoryInfo.Sum(i => i.Weight);
				var historyQuantity = Notification.InHistoryInfo.Sum(i => i.Quantity) + Notification.CorHistoryInfo.Sum(i => i.Quantity);
				var historyWeight = Notification.InHistoryInfo.Sum(i => i.Weight) + Notification.CorHistoryInfo.Sum(i => i.Weight);

				if (Math.Abs(docPackingLine.AmountQuantity) > historyQuantity)
				{
					docPackingLine.AmountQuantity = -historyQuantity;
				}
				else if (docPackingLine.AmountQuantity > 0)
				{
					docPackingLine.AmountQuantity = 0;
				}

				if (Math.Abs(docPackingLine.AmountWeight) > historyWeight)
				{
					docPackingLine.AmountWeight = -historyWeight;
				}
				else if (docPackingLine.AmountWeight > 0)
				{
					docPackingLine.AmountWeight = 0;
				}
				SetShipmentDescriptionIfNotEmpty(docPackingLine, ReceiveConsignment);
				if (!((IConsignment)ReceiveConsignment).MasterBillNumber.IsEmpty)
				{
					docPackingLine.TemporaryStorageDeclaration = ReceiveConsignment.CustomsReferenceNumbers.GetTempStorageDeclaration();
				}
			}
			else
			{
				var arrivedWeight = Notification.RCNPackageStates.Sum(p => Core.Constants.Weight.Convert(p.Package.KP_Weight, p.Package.KP_WeightUQ, Core.Constants.Weight.Kilograms)).RoundTo3Digits();
				var reportedWeight = Notification.CorHistoryInfo.Sum(h => h.Weight) + Notification.InHistoryInfo.Sum(h => h.Weight);
				if (arrivedWeight != reportedWeight)
				{
					docPackingLine = CreatePackingLineFromPackageStates(Notification.RCNPackageStates);
					docPackingLine.AmountQuantity = 0;
					docPackingLine.AmountWeight = arrivedWeight - reportedWeight;
				}
			}

			return docPackingLine;
		}

		protected override void AddValidation(DocPackingLine docPackingLine)
		{
			docPackingLine.AmountQuantityInfo.AddMessageError(() => docPackingLine.AmountQuantity.IsEmpty && docPackingLine.AmountWeight.IsEmpty, Res.GetString("e7035821-1065-4b61-a01b-68ed15cea56f", "Amount Quantity is required."));
			docPackingLine.AmountWeightInfo.AddMessageError(() => docPackingLine.AmountQuantity.IsEmpty && docPackingLine.AmountWeight.IsEmpty, Res.GetString("036363c6-05f0-4136-9a09-9b52779819b2", "Amount Weight is required."));

			docPackingLine.AmountQuantityInfo.AddMessageError(() => docPackingLine.AmountQuantity < -99999, Res.GetString("4ab72586-992a-4ce2-9d35-1c83c2c16aa4", "Amount Quantity can not be less than -99999."));
			docPackingLine.AmountQuantityInfo.AddMessageError(() => docPackingLine.AmountQuantity > 0, Res.GetString("30323a47-355d-48e0-a175-651cc8062810", "Amount Quantity can not be greater than 0."));

			docPackingLine.AmountWeightInfo.AddMessageError(() => docPackingLine.AmountWeight < -99999.999m, Res.GetString("bd9f6250-b5e3-42d2-8d69-954267bacd5c", "Amount Weight can not be less than -99999.999."));
			docPackingLine.AmountWeightInfo.AddMessageError(() => docPackingLine.AmountWeight > 99999.999m, Res.GetString("284e31c0-7d91-466c-9672-2ed3345ac425", "Amount Weight can not be greater than 99999.999."));
			docPackingLine.AmountWeightInfo.AddMessageError(() => docPackingLine.AmountWeight != docPackingLine.AmountWeight, Res.GetString("43971e95-a8f4-444e-a07a-4a7415eac8e6", "The decimal precision of Amount Weight can not be greater than 3 digits."));

			docPackingLine.DescriptionInfo.AddMessageErrorIfEmpty(Res.GetString("9eb81631-23f5-4be1-875b-4872c0c9de9a", "Goods Description is required."));
		}

		static readonly List<string> ValidReceivedAsCode = new List<string>
		{
			TransitWarehouseReceiveAs.Codes.Default,
			TransitWarehouseReceiveAs.Codes.PackedPackage,
			TransitWarehouseReceiveAs.Codes.ScannedIn,
			TransitWarehouseReceiveAs.Codes.BuiltInWarehouse,
			TransitWarehouseReceiveAs.Codes.SkipScanMode
		};
	}
}
