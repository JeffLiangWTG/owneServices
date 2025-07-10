using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.AirManifest;
using Enterprise.Customs.TW.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.DataTransfer.Universal
{
	public class UniversalCustomsDataObjectProvider : IUniversalCustomsDataObjectProvider
	{
		#region Implementation of IUniversalCustomsDataObjectProvider

		public ICodeDescriptionPairList TableSpecificCusSupportingInfoTypeList(ZString tableCode, string dataContext = "")
		{
			var result = new CodeDescriptionPairList();
			switch (tableCode)
			{
				case JobComInvoiceLineSchema.Constants.Prefix:
					result.AddPair(CusSupportingInfoTypeList.Codes.PermitNumber, CusSupportingInfoTypeList.Descriptions.PermitNumber);
					result.AddPair(CusSupportingInfoTypeList.Codes.PermitExemptionCodes, CusSupportingInfoTypeList.Descriptions.PermitExemptionCodes);
					result.AddPair(CusSupportingInfoTypeList.Codes.CertificateOfOriginNumber, CusSupportingInfoTypeList.Descriptions.CertificateOfOriginNumber);
					result.AddPair(CusSupportingInfoTypeList.Codes.PreviousBondedEntryNumber, CusSupportingInfoTypeList.Descriptions.PreviousBondedEntryNumber);
					break;
			}
			return result;
		}

		public ICodeDescriptionPairList TableSpecificCusReferenceTypeList(ZString tableCode, string dataContext) => null;

		public ICodeDescriptionPairList TableSpecificCusAddInfoTypeList(ZString tableCode, string dataContext = "")
		{
			var result = new CodeDescriptionPairList();
			return result;
		}

		public ICodeDescriptionPairList TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(ZString tableCode, string dataContext)
		{
			return null;
		}

		public ICodeDescriptionPairList TableSpecificCusCodeDataTypeList(ZString tableCode, string dataContext = "")
		{
			var result = new CodeDescriptionPairList();

			if (tableCode == CusEntryInstructionSchema.Constants.Prefix)
			{
				result.AddPair(CusCodeDataTypeList.Codes.DeclarationDuplicate, CusCodeDataTypeList.Descriptions.DeclarationDuplicate);
			}
			return result;
		}

		public ICodeDescriptionPairList TableSpecificCusCodeDataCodeList(ZString tableCode, string dataContext = "")
		{
			var result = new CodeDescriptionPairList();
			return result;
		}

		public ITopLevelDataObjectReader GetNewJobDeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment)
		{
			return new TWJobDeclarationDataObjectReader(declarationDataObject, logger, factory, shipment);
		}

		public UniversalDataObjectReaderHelper GetNewUniversalDataObjectReaderHelper(UniversalObjectFactory factory, string sourceCountryCode, string dataProviderForCodeMapping = null)
		{
			return new TWDataObjectReaderHelper(factory, sourceCountryCode);
		}

		public ITopLevelDataObjectWriter GetNewDeclarationDataObjectWriter(IDataWritingManager manager)
		{
			return new TWJobDeclarationDataObjectWriter(manager);
		}

		public IEnumerable<ITopLevelDataObjectReader> GetNewAirManifestDataObjectReaders(Shipment mawbDataObject, Shipment subShipment, IXmlImportLogger logger, UniversalObjectFactory factory, bool singleHAWBCheck)
		{
			return Enumerable.Empty<ITopLevelDataObjectReader>();
		}

		public ITopLevelDataObjectWriter GetNewAirManifestDataObjectWriter(IDataWritingManager manager)
		{
			return null;
		}

		public ITopLevelDataObjectWriter GetNewAirManifestLineDataObjectWriter(IDataWritingManager manager, AirManifestDataObjectWriterHelper helper)
		{
			return null;
		}

		public IEnumerable<ITopLevelDataObjectReader> GetNewCusSCAOceanBillDataObjectReaders(Shipment dataObject, Shipment subShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return Enumerable.Empty<ITopLevelDataObjectReader>();
		}

		public ITopLevelDataObjectWriter GetNewCusSCAOceanBillDataObjectWriter(IDataWritingManager manager)
		{
			return null;
		}

		public ITopLevelDataObjectReader GetNewStandaloneCommercialInvoiceDataObjectReader(Shipment shipmentDataObject, CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new StandaloneCommercialInvoiceDataObjectReader(shipmentDataObject, invoiceDataObject, logger, factory);
		}

		public Customs.DataTransfer.Universal.StandaloneCommercialInvoiceDataObjectWriter GetNewStandaloneCommercialInvoiceDataObjectWriter(IDataWritingManager manager)
		{
			return new StandaloneCommercialInvoiceDataObjectWriter(manager);
		}

		#endregion
	}
}
