using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer
{
	public sealed class SGAsycudaManifestDataObjectReaderHelper : AsycudaManifestDataObjectReaderHelper
	{
		public SGAsycudaManifestDataObjectReaderHelper(BusinessObjectFactory factory)
			: base(Core.Constants.CountryCodes.Singapore, factory)
		{
		}

		protected override IEnumerable<GenAddOnDetail> GetAsycudaPackedItemGenAddOnColumnListCore(ASYCUDA.Business.AsycudaPackedItem basePackedItem)
		{
			var packedItem = (AsycudaPackedItem)basePackedItem;
			yield return new GenAddOnDetail() { TypeCode = AddOnColumnDataType.GetCodeFromObject(packedItem.GoodsType), AddInfoKey = AddInfoConstants.PackedItem.GoodsType, GenAddOnColumnName = AsycudaPackedItem.Schema.GoodsType, PropertyName = AsycudaPackedItem.Schema.GoodsType };
			yield return new GenAddOnDetail() { TypeCode = AddOnColumnDataType.GetCodeFromObject(packedItem.GSTPaid), AddInfoKey = AddInfoConstants.PackedItem.GSTPaymentIndicator, GenAddOnColumnName = AsycudaPackedItem.Schema.GSTPaid, PropertyName = AsycudaPackedItem.Schema.GSTPaid };
			foreach (var detail in base.GetAsycudaPackedItemGenAddOnColumnListCore(packedItem))
			{
				yield return detail;
			}
		}

		protected override IDictionary<ZString, ZString> GetPackedItemEntryNumberMappingCore()
		{
			var result = new Dictionary<ZString, ZString>();
			result.Add(AddInfoConstants.PackedItem.PermitNumber, ASYCUDA.Business.Constants.CustomsEntryType.TradeNetPermit);
			result.Add(AddInfoConstants.PackedItem.EntryNumber, ASYCUDA.Business.Constants.CustomsEntryType.ACCESSPermit);
			return result;
		}

		protected override IEnumerable<GenAddOnDetail> GetAsycudaBillGenAddOnColumnListCore(ASYCUDA.Business.AsycudaBill baseBillBO)
		{
			var countryBO = (AsycudaBill)baseBillBO;
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(countryBO.SG_PayeeIndicator), AddInfoKey = AddInfoConstants.BillCountry.PayeeIndicator, GenAddOnColumnName = AsycudaBill.Schema.SG_PayeeIndicator, PropertyName = AsycudaBill.Schema.SG_PayeeIndicator };
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(countryBO.SG_PartyStatus), AddInfoKey = AddInfoConstants.BillCountry.PartyStatus, GenAddOnColumnName = AsycudaBill.Schema.SG_PartyStatus, PropertyName = AsycudaBill.Schema.SG_PartyStatus };
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(countryBO.SG_PartyID), AddInfoKey = AddInfoConstants.BillCountry.PartyIndicator, GenAddOnColumnName = AsycudaBill.Schema.SG_PartyID, PropertyName = AsycudaBill.Schema.SG_PartyID };
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(countryBO.CycleDate), AddInfoKey = AddInfoConstants.BillCountry.CycleDate, GenAddOnColumnName = AsycudaBill.Schema.CycleDate, PropertyName = AsycudaBill.Schema.CycleDate };
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(countryBO.CycleNumber), AddInfoKey = AddInfoConstants.BillCountry.CycleNumber, GenAddOnColumnName = AsycudaBill.Schema.CycleNumber, PropertyName = AsycudaBill.Schema.CycleNumber };
			foreach (var detail in base.GetAsycudaBillGenAddOnColumnListCore(countryBO))
			{
				yield return detail;
			}
		}

		protected override IEnumerable<GenAddOnDetail> GetAdditionalInfoColumnListCore(ASYCUDA.Business.AsycudaBill baseBillBO)
		{
			var countryBO = (AsycudaBill)baseBillBO;
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(countryBO.GSTNReferenceNo), AddInfoKey = AddInfoConstants.BillCountry.GSTNReferenceNo, GenAddOnColumnName = AsycudaBill.Schema.GSTNReferenceNo, PropertyName = AsycudaBill.Schema.GSTNReferenceNo };
		}

		protected override AsycudaBillDataObjectReader GetBillDataObjectReaderCore(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaManifestHeader header, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
		{
			if (dataObject.GetMatchingDataSource(DataContextType.HVLVConsignment) != null)
			{
				return new SGHVLVAsycudaBillDataObjectReader(dataObject, logger, factory, header, helper);
			}
			return new SGAsycudaBillDataObjectReader(dataObject, logger, factory, header, helper, isUpdateEnabled);
		}

		protected override ZString GetManifestTypeByDataObjectCore(Shipment shipment)
		{
			var shipemntDataObject = shipment.IsHVLV() ? shipment.SubShipmentCollection[0] : shipment;
			var origin = shipemntDataObject.PortOfOrigin != null ? shipemntDataObject.PortOfOrigin.Code :
						shipemntDataObject.PortOfLoading != null ? shipemntDataObject.PortOfLoading.Code : ZString.Empty;

			var destination = shipemntDataObject.PortOfDestination != null ? shipemntDataObject.PortOfDestination.Code :
				shipemntDataObject.PortOfDischarge != null ? shipemntDataObject.PortOfDischarge.Code : ZString.Empty;

			var result = ImportExportHelper.IsImport(origin.Value, destination.Value) ? SGManifestTypes.Codes.MGI : SGManifestTypes.Codes.MGE;
			return result;
		}
	}
}
