using System.Linq;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer
{
	public class SGHVLVAsycudaBillDataObjectReader : AsycudaBillDataObjectReader
	{
		public SGHVLVAsycudaBillDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaManifestHeader header, AsycudaManifestDataObjectReaderHelper helper)
			: base(dataObject, logger, factory, header, helper, true)
		{
		}

		protected override void PopulateBusinessObject(ASYCUDA.Business.AsycudaBill bill)
		{
			var billRow = GetColumnIndexer(bill);
			if (!bill.HasManifestBeenSubmittedToCustomsIncludingChildren)
			{
				SetValue(billRow, AsycudaBillSchema.ABL_BillNumber, dataObject.WayBillNumber);
				SetValue(billRow, AsycudaBillSchema.ABL_ManifestQty, dataObject.TotalNoOfPieces);
				SetValue(billRow, AsycudaBillSchema.ABL_ManifestUQ, dataObject.TotalNoOfPacksPackageType);
				SetValue(billRow, AsycudaBillSchema.ABL_GrossWeight, dataObject.TotalWeight);
				SetValue(billRow, AsycudaBillSchema.ABL_GrossWeightUQ, dataObject.TotalWeightUnit);
				SetValue(billRow, AsycudaBillSchema.ABL_Volume, dataObject.TotalVolume);
				SetValue(billRow, AsycudaBillSchema.ABL_VolumeUQ, dataObject.TotalVolumeUnit);
				SetValue(billRow, AsycudaBillSchema.ABL_GoodsDescription, dataObject.GoodsDescription);
				if (dataObject.ShipmentIncoTerm.TryGetCodeAsUpperCase(out var incoTermCode))
				{
					var prepaidCollect = IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, incoTermCode);
					switch (prepaidCollect)
					{
						case Core.Constants.PaymentType.Collect:
							SetValue(billRow, AsycudaBillSchema.ABL_PrepaidCollect, Core.Constants.DomesticPaymentTerms.Collect);
							break;
						case Core.Constants.PaymentType.Prepaid:
							SetValue(billRow, AsycudaBillSchema.ABL_PrepaidCollect, Core.Constants.DomesticPaymentTerms.Prepaid);
							break;
					}
				}
				SetValue(billRow, AsycudaBillSchema.ABL_FreightValue, dataObject.GoodsValue);
				SetValue(billRow, AsycudaBillSchema.ABL_RX_NKFreightValueCurrency, dataObject.GoodsValueCurrency);

				FillOrganizations(billRow);
				FillCustomReference(bill);

				FillPacks(bill);
			}
		}

		void FillCustomReference(ASYCUDA.Business.AsycudaBill bill)
		{
			if (dataObject.CustomsReferenceCollection != null && dataObject.CustomsReferenceCollection.Count != 0)
			{
				var declarationReference = dataObject.CustomsReferenceCollection.FirstOrDefault(r => r.Type.Code.GetValueOrDefault().Equals(nameof(Core.Constants.DataContext.Declaration)));
				if (declarationReference != null && !string.IsNullOrEmpty(declarationReference.Reference))
				{
					var customsJobNumberDetail = new GenAddOnDetail() { GenAddOnColumnName = AsycudaBill.Schema.CustomsJobNumber, PropertyName = AsycudaBill.Schema.CustomsJobNumber, Value = declarationReference.Reference };
					customsJobNumberDetail.ReadIntoBusinessObject(true, bill);
				}
			}
		}

		void FillPacks(ASYCUDA.Business.AsycudaBill bill)
		{
			if (dataObject.PackingLineCollection != null)
			{
				helper.PacksReaderHelper.MarkUnprocessedExistingObjectFor(factory, bill);
				foreach (var packingLineDataObject in dataObject.PackingLineCollection)
				{
					if (packingLineDataObject.PackedItemCollection != null)
					{
						foreach (var packedItemDataObject in packingLineDataObject.PackedItemCollection)
						{
							var commercialInvoiceLine = packedItemDataObject.FindMatchingCommercialInvoiceLine(dataObject);
							var pack = new SGHVLVAsycudaPackDataObjectReader(packingLineDataObject, packedItemDataObject, commercialInvoiceLine, logger, factory, bill).ReadIntoBusinessObject();
							helper.PacksReaderHelper.MarkProcessed(pack);
						}
					}
					else
					{
						var pack = new SGHVLVAsycudaPackDataObjectReader(packingLineDataObject, null, null, logger, factory, bill).ReadIntoBusinessObject();
						helper.PacksReaderHelper.MarkProcessed(pack);
					}
				}
				helper.PacksReaderHelper.DeleteUnprocessedObjectsFor(bill, logger);
			}
		}
	}
}
