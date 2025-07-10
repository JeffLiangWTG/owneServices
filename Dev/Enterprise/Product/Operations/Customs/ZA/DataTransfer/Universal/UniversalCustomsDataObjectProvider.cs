using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.Customs.DataTransfer.Universal;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	public class UniversalCustomsDataObjectProvider : UniversalShipment.IUniversalCustomsDataObjectProvider
	{
		#region IUniversalCustomsDataObjectProvider Members

		public ICodeDescriptionPairList TableSpecificCusSupportingInfoTypeList(ZString tableCode, string dataContext = "")
		{
			return null;
		}

		public ICodeDescriptionPairList TableSpecificCusReferenceTypeList(ZString tableCode, string dataContext) => null;

		public ICodeDescriptionPairList TableSpecificCusAddInfoTypeList(ZString tableCode, string dataContext = "")
		{
			return null;
		}

		public ICodeDescriptionPairList TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(ZString tableCode, string dataContext)
		{
			return null;
		}

		public ICodeDescriptionPairList TableSpecificCusCodeDataCodeList(ZString tableCode, string dataContext = "")
		{
			CodeDescriptionPairList result = null;
			switch (tableCode)
			{
				case CusEntryInstructionSchema.Constants.Prefix:
					result = new DocumentStatusCodes();
					break;
				case CusEntryLineSchema.Constants.Prefix:
					result = new CodeDescriptionPairList();
					result.AddRange(ZZRefCusCodeListCombined.Loader.Load(new BusinessObjectFactory(), Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, ZDateTime.Today));
					break;
			}
			return result;
		}

		public ICodeDescriptionPairList TableSpecificCusCodeDataTypeList(ZString tableCode, string dataContext = "")
		{
			CodeDescriptionPairList result = null;
			switch (tableCode)
			{
				case CusEntryInstructionSchema.Constants.Prefix:
					result = new CodeDescriptionPairList();
					result.AddPair(CusCodeDataTypeList.Codes.CaseNumber, CusCodeDataTypeList.Descriptions.CaseNumber);
					break;
				case CusEntryLineSchema.Constants.Prefix:
					result = new CodeDescriptionPairList();
					result.AddPair(CusCodeDataTypeList.Codes.AdditionalInformation, CusCodeDataTypeList.Descriptions.AdditionalInformation);
					break;
			}
			return result;
		}

		public ITopLevelDataObjectWriter GetNewDeclarationDataObjectWriter(IDataWritingManager manager)
		{
			return new DeclarationDataObjectWriter(manager);
		}

		public ITopLevelDataObjectReader GetNewJobDeclarationDataObjectReader(UniversalDataBuss.DataObjects.Universal.Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment)
		{
			return new JobDeclarationDataObjectReader(declarationDataObject, logger, factory, shipment);
		}

		public IEnumerable<ITopLevelDataObjectReader> GetNewAirManifestDataObjectReaders(UniversalDataBuss.DataObjects.Universal.Shipment mawbDataObject, UniversalDataBuss.DataObjects.Universal.Shipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory, bool singleHAWBCheck)
		{
			return System.Linq.Enumerable.Empty<ITopLevelDataObjectReader>();
		}

		public ITopLevelDataObjectWriter GetNewAirManifestDataObjectWriter(IDataWritingManager manager)
		{
			return null;
		}

		public ITopLevelDataObjectWriter GetNewAirManifestLineDataObjectWriter(IDataWritingManager manager, UniversalShipment.AirManifest.AirManifestDataObjectWriterHelper helper)
		{
			return null;
		}

		public UniversalShipment.UniversalDataObjectReaderHelper GetNewUniversalDataObjectReaderHelper(UniversalObjectFactory factory, string sourceCountryCode, string dataProviderForCodeMapping = null)
		{
			return new UniversalDataObjectReaderHelper(factory);
		}

		public ITopLevelDataObjectReader GetNewStandaloneCommercialInvoiceDataObjectReader(UniversalDataBuss.DataObjects.Universal.Shipment shipmentDataObject, UniversalDataBuss.DataObjects.Universal.Customs.CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return null;
		}

		public UniversalShipment.StandaloneCommercialInvoiceDataObjectWriter GetNewStandaloneCommercialInvoiceDataObjectWriter(IDataWritingManager manager)
		{
			return null;
		}

		public IEnumerable<ITopLevelDataObjectReader> GetNewCusSCAOceanBillDataObjectReaders(UniversalDataBuss.DataObjects.Universal.Shipment dataObject, UniversalDataBuss.DataObjects.Universal.Shipment subShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return System.Linq.Enumerable.Empty<ITopLevelDataObjectReader>();
		}

		public ITopLevelDataObjectWriter GetNewCusSCAOceanBillDataObjectWriter(IDataWritingManager manager)
		{
			return null;
		}

		#endregion
	}
}
