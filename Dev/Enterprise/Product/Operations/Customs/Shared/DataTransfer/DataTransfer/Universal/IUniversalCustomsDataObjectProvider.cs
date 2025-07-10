using System.Collections.Generic;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public interface IUniversalCustomsDataObjectProvider
	{
		ICodeDescriptionPairList TableSpecificCusSupportingInfoTypeList(ZString tableCode, string dataContext);
		ICodeDescriptionPairList TableSpecificCusAddInfoTypeList(ZString tableCode, string dataContext);
		ICodeDescriptionPairList TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(ZString tableCode, string dataContext);
		ICodeDescriptionPairList TableSpecificCusCodeDataTypeList(ZString tableCode, string dataContext);
		ICodeDescriptionPairList TableSpecificCusCodeDataCodeList(ZString tableCode, string dataContext);
		ICodeDescriptionPairList TableSpecificCusReferenceTypeList(ZString tableCode, string dataContext);

		ITopLevelDataObjectReader GetNewJobDeclarationDataObjectReader(UniversalShipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment);
		UniversalDataObjectReaderHelper GetNewUniversalDataObjectReaderHelper(UniversalObjectFactory factory, string sourceCountryCode, string dataProviderForCodeMapping = null);
		ITopLevelDataObjectWriter GetNewDeclarationDataObjectWriter(IDataWritingManager manager);

		IEnumerable<ITopLevelDataObjectReader> GetNewAirManifestDataObjectReaders(UniversalShipment mawbDataObject, UniversalShipment subShipment, IXmlImportLogger logger, UniversalObjectFactory factory, bool singleHAWBCheck);
		ITopLevelDataObjectWriter GetNewAirManifestDataObjectWriter(IDataWritingManager manager);

		ITopLevelDataObjectWriter GetNewAirManifestLineDataObjectWriter(IDataWritingManager manager, AirManifest.AirManifestDataObjectWriterHelper helper);

		IEnumerable<ITopLevelDataObjectReader> GetNewCusSCAOceanBillDataObjectReaders(UniversalShipment dataObject, UniversalShipment subShipment, IXmlImportLogger logger, UniversalObjectFactory factory);

		ITopLevelDataObjectWriter GetNewCusSCAOceanBillDataObjectWriter(IDataWritingManager manager);

		ITopLevelDataObjectReader GetNewStandaloneCommercialInvoiceDataObjectReader(UniversalShipment shipmentDataObject, UniversalDataBuss.DataObjects.Universal.Customs.CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalObjectFactory factory);
		StandaloneCommercialInvoiceDataObjectWriter GetNewStandaloneCommercialInvoiceDataObjectWriter(IDataWritingManager manager);
	}
}
