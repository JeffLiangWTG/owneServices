using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.Common;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document
{
	public class CIN750OutNotificationPackingLineBuilder : DocPackingLineBuilder
	{
		public DocPackingLine Build(CIN750OutNotification outNotification, IEnumerable<WhsItemPackageState> outerPackages, OutNotificationAdditionalData additionalData)
		{
			var docPackingLine = new DocPackingLine(outerPackages.FirstOrDefault()?.PK ?? ZGuid.NewZGuid());
			docPackingLine.AmountQuantity = additionalData.PackageQuantityReadyToOut;
			docPackingLine.AmountWeight = additionalData.PackageWeightReadyToOut;
			docPackingLine.Description = GetGoodsDescription(outerPackages);

			AddValidation(docPackingLine);
			SetShipmentDescriptionIfNotEmpty(docPackingLine, outerPackages.FirstOrDefault()?.DispatchConsignment);
			if (outNotification.RefType.Code != CIN750RefTypes.Codes.MasterAirWaybill)
			{
				var tempStorageDeclaration = outerPackages
					.Select(p => p.ReceiveConsignmentWithInnersAndBreakDownInnersFallBack?.CustomsReferenceNumbers?.GetTempStorageDeclaration() ?? ZString.Empty)
					.FirstOrDefault(p => !p.IsEmpty);
				docPackingLine.TemporaryStorageDeclaration = tempStorageDeclaration;
			}
			return docPackingLine;
		}
	}
}
