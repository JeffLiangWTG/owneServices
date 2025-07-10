using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TR.ETrade.Business
{
	sealed class TRETradeHVLVAsycudaManifestDataObjectReaderHelper : AsycudaManifestDataObjectReaderHelper
	{
		public TRETradeHVLVAsycudaManifestDataObjectReaderHelper(ZString countryCode, BusinessObjectFactory factory)
			: base(countryCode, factory)
		{
		}

		protected override void FillManifestSpecificDataCore(Shipment shipmentDataObject, IXmlImportLogger logger, ASYCUDA.Business.AsycudaManifestHeader header, UniversalObjectFactory factory)
		{
			var hvlvShipmentDataObject = shipmentDataObject.SubShipmentCollection?.FirstOrDefault(s => string.Equals(s.ShipmentType?.Code, ShipmentTypes.HighVolumeLowValue));

			if (hvlvShipmentDataObject != null)
			{
				var genAddOnData = new[]
				{
					new AddInfo() { Key = AsycudaManifestHeader.Schema.DepartureFlight, Value = shipmentDataObject.VoyageFlightNo },
					new AddInfo() { Key = AsycudaManifestHeader.Schema.DepartureCountryCode, Value = GetCountryCodeFromUNLOCO(hvlvShipmentDataObject.PortOfOrigin) },
				};

				var genAddOnColumnInfo = new[]
				{
					new GenAddOnDetail
					{
						TypeCode = AddOnColumnDataType.GetCodeFromType(typeof(ZString)),
						AddInfoKey = AsycudaManifestHeader.Schema.DepartureFlight,
						GenAddOnColumnName = AsycudaManifestHeader.Schema.DepartureFlight,
						PropertyName = AsycudaManifestHeader.Schema.DepartureFlight
					},
					new GenAddOnDetail
					{
						TypeCode = AddOnColumnDataType.GetCodeFromType(typeof(ZString)),
						AddInfoKey = AsycudaManifestHeader.Schema.DepartureCountryCode,
						GenAddOnColumnName = AsycudaManifestHeader.Schema.DepartureCountryCode,
						PropertyName = AsycudaManifestHeader.Schema.DepartureCountryCode
					},
				};

				new GenAddOnColumnCollectionDataObjectReader(logger).ReadIntoBusinessObject(genAddOnData, genAddOnColumnInfo, header);
			}
		}

		protected override AsycudaBillDataObjectReader GetBillDataObjectReaderCore(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaManifestHeader header, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
		{
			if (dataObject.GetMatchingDataSource(DataContextType.HVLVConsignment) != null)
			{
				return new TRETradeHVLVAsycudaBillDataObjectReader(dataObject, logger, factory, header, helper, isUpdateEnabled);
			}

			return base.GetBillDataObjectReaderCore(dataObject, logger, factory, header, helper, isUpdateEnabled);
		}

		ZString GetCountryCodeFromUNLOCO(UNLOCO unloco)
		{
			if (unloco != null && unloco.Code.HasValue)
			{
				return unloco.Code.Value.SubstringSafe(0, 2);
			}

			return ZString.Empty;
		}
	}
}
