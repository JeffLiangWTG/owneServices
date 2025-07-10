using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.Common;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document
{
	public sealed class CIN750OutNotificationBuilder : CIN750NotificationBuilder<WhsItemDispatchConsignment, CIN750OutNotification>
	{
		public CIN750OutNotificationBuilder(WhsItemDispatchConsignment dcn, OutNotificationAdditionalData additionalData) : base(dcn)
		{
			OuterPackages = sourceBO.PackageStates.Where(p => (p.TopHandlingUnit == null || p.TopHandlingUnit.WPS_WDC_TransitDispatchConsignment != sourceBO.PK) && !p.WPS_WDH_TransitDispatchHeader.IsEmpty);
			if (additionalData != null)
			{
				AdditionalData = additionalData;
			}
			else
			{
				AdditionalData = new OutNotificationAdditionalData()
				{
					PackageQuantityReadyToOut = 0,
					PackageWeightReadyToOut = 0
				};
			}
		}

		IEnumerable<WhsItemPackageState> OuterPackages { get; }

		OutNotificationAdditionalData AdditionalData { get; }

		#region Details

		protected override (ZString RefType, ZString RefCode) GetRefTypeAndRefCode()
		{
			var refType = ZString.Empty;
			var refCode = ZString.Empty;

			var masterBillNumber = sourceBO.MasterBillNumber;

			if (!masterBillNumber.IsEmpty)
			{
				refType = CIN750RefTypes.Codes.MasterAirWaybill;
				refCode = masterBillNumber;
			}
			else
			{
				var houseBillNumber = sourceBO.HouseBillNumber;
				if (!houseBillNumber.IsEmpty)
				{
					refType = CIN750RefTypes.Codes.HouseAirWaybill;
					refCode = houseBillNumber;
				}
				else
				{
					refType = CIN750RefTypes.Codes.Reference;
					refCode = TransitDocumentHelper.GetEnterpiseAndServerCode() + OuterPackages.GroupBy(p => p.ReceiveConsignmentID).FirstOrDefault()?.Key;
				}
			}

			return (refType, refCode);
		}

		protected override CIN750OutNotification GetDocDataObject() =>
			new CIN750OutNotification(
				nameof(WhsItemDispatchConsignment),
				sourceBO.WDC_ConsignmentID);

		protected override OrgAddress GetDeclaredInWarehouse() => sourceBO.Warehouse?.WarehouseAddress;

		protected override void PopulateProperties(CIN750OutNotification outNotification)
		{
			var toCTOWarehouse = sourceBO.CTODocAddress.Address;
			outNotification.ToCTO = AddressBuilder.Create(context, toCTOWarehouse);
			outNotification.ToCTOCINNumber = toCTOWarehouse?.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CIN);
			outNotification.ToCTOCIN = outNotification.ToCTOCINNumber?.Value ?? TransitDocDataConstants.NOTCIN;
			outNotification.CustomsStatus = TransitDocumentHelper.GetCustomsStatus(sourceBO);

			outNotification.CustomsDocuments = OuterPackages.GroupBy(p => p.ReceiveConsignmentWithInnersAndBreakDownInnersFallBack)
				.Where(p => p.Key != null)
				.OrderBy(r => r.Key.WRC_ConsignmentID)
				.Select(p =>
				{
					var typeAndCode = p.Key.GetCustomsReferenceNumbersRefAndCode(includingPackageLevel: true);
					return new CIN750CustomsDocument()
					{
						RefType = typeAndCode.RefType,
						RefCode = typeAndCode.RefCode
					};
				}).Where(c => !c.RefType.IsEmpty).OrderBy(c => c.RefType).ToArray();

			if (outNotification.CustomsDocuments?.Count == 0)
			{
				outNotification.CustomsDocuments = new CIN750CustomsDocument[] {
					new CIN750CustomsDocument() {
						RefType = ZString.Empty,
						RefCode = ZString.Empty
					}
				};
			}

			foreach (var customsDocument in outNotification.CustomsDocuments)
			{
				customsDocument.RefTypeInfo.AddMessageErrorIfEmpty(CIN750NotificationValidation.WithoutCustomsDocumentsErrorMessage);
				customsDocument.RefCodeInfo.AddMessageErrorIfEmpty(CIN750NotificationValidation.WithoutCustomsDocumentsErrorMessage);
			}

			if (AdditionalData != null)
			{
				outNotification.MaxQuantity = AdditionalData.PackageQuantityReadyToOut;
				outNotification.MaxWeight = AdditionalData.PackageWeightReadyToOut;
			}
		}

		protected override void PopulatePackingLines(CIN750OutNotification outNotification)
		{
			outNotification.Goods = new List<DocPackingLine> { new CIN750OutNotificationPackingLineBuilder().Build(outNotification, OuterPackages, AdditionalData) };
		}

		#endregion
	}
}
