using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document
{
	public sealed class CIN750InNotificationPackingLineBuilder : DocPackingLineBuilder
	{
		public CIN750InNotificationPackingLineBuilder(CIN750InNotification notification)
		{
			Notification = notification;
			ReceiveConsignment = Argument.NotNull(notification.SourceBusinessObject as WhsItemReceiveConsignment, nameof(notification.SourceBusinessObject));
		}

		readonly CIN750InNotification Notification;
		readonly WhsItemReceiveConsignment ReceiveConsignment;

		public override DocPackingLine Build()
		{
			var packline = base.Build();

			var arrivedPackagesStates = TransitDocumentHelper.GetRCNArrivedPackageStates(ReceiveConsignment);

			var historyManager = new ReceiveConsignmentCIN750NotificationHistoryManager(ReceiveConsignment);

			Notification.InHistoryInfo = historyManager.InHistory;
			Notification.CorHistoryInfo = historyManager.CorHistory;
			Notification.ReceivedPackageQuantity = GetSumQuantity(arrivedPackagesStates);
			Notification.ReceivedPackageWeight = GetSumWeight(arrivedPackagesStates);

			if (arrivedPackagesStates.Any())
			{
				packline.AmountQuantity = Notification.ReceivedPackageQuantity - (Notification.InHistoryInfo.Sum(h => h.Quantity) + Notification.CorHistoryInfo.Sum(h => h.Quantity));
				packline.AmountWeight = Notification.ReceivedPackageWeight - (Notification.InHistoryInfo.Sum(h => h.Weight) + Notification.CorHistoryInfo.Sum(h => h.Weight));
				packline.Description = GetGoodsDescription(arrivedPackagesStates);
				SetShipmentDescriptionIfNotEmpty(packline, ReceiveConsignment);
				var typeAndCode = ReceiveConsignment.GetCustomsReferenceNumbersRefAndCode(includingPackageLevel: false);
				packline.AccompanyDocumentRef = typeAndCode.RefCode;
				packline.AccompanyDocumentType = typeAndCode.RefType;
				packline.TemporaryStorageDeclaration = ReceiveConsignment.CustomsReferenceNumbers.GetTempStorageDeclaration();
			}

			return packline;
		}

		protected override void AddValidation(DocPackingLine docPackingLine)
		{
			base.AddValidation(docPackingLine);
			docPackingLine.AmountQuantityInfo.AddMessageError(() => docPackingLine.AmountQuantity <= 0, Res.GetString("2df9e13d-4129-47c1-be48-65821119ccd2", "Amount Quantity must be greater than 0."));
			docPackingLine.AmountWeightInfo.AddMessageError(() => docPackingLine.AmountWeight <= 0, Res.GetString("c5b283b9-7622-4ca0-8613-d1b5f53ee0da", "Amount Weight must be greater than 0."));
		}
	}
}
