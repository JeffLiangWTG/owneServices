using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using UniversalShipment = Enterprise.Customs.DataTransfer.Universal;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class UniversalCustomsDataObjectProvider : UniversalShipment.IUniversalCustomsDataObjectProvider,
		Integration.Customs.US.IUSUniversalCustomsDataObjectProvider
	{
		#region IUniversalCustomsDataObjectProvider Members

		public ICodeDescriptionPairList TableSpecificCusSupportingInfoTypeList(ZString tableCode, string dataContext = "")
		{
			return null;
		}

		public ICodeDescriptionPairList TableSpecificCusReferenceTypeList(ZString tableCode, string dataContext) => null;

		public ICodeDescriptionPairList TableSpecificCusAddInfoTypeList(ZString tableCode, string dataContext = "")
		{
			return CusAddInfoTypeListProvider.TableSpecificCusAddInfoTypeList(tableCode);
		}

		public ICodeDescriptionPairList TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(ZString tableCode, string dataContext)
		{
			return null;
		}

		public ICodeDescriptionPairList TableSpecificCusCodeDataCodeList(ZString tableCode, string dataContext = "")
		{
			return CusCodeDataTypeAndCodeListProvider.TableSpecificCusCodeDataCodeList(tableCode);
		}

		public ICodeDescriptionPairList TableSpecificCusCodeDataTypeList(ZString tableCode, string dataContext = "")
		{
			return CusCodeDataTypeAndCodeListProvider.TableSpecificCusCodeDataTypeList(tableCode);
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
			if (shipment != null && shipment.ShipmentType.GetCodeAsUpperCase().Equals(ShipmentTypes.HighVolumeLowValue))
			{
				return new[] { ObjectFactory.Get<ITopLevelDataObjectReader>("eTailUSLVClearanceDataObjectReader", mawbDataObject, shipment, logger, factory) };
			}

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

		public IEnumerable<ITopLevelDataObjectReader> GetNewCusSCAOceanBillDataObjectReaders(UniversalDataBuss.DataObjects.Universal.Shipment dataObject, UniversalDataBuss.DataObjects.Universal.Shipment subShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			if (subShipment.ShipmentType.GetCodeAsUpperCase().Equals(ShipmentTypes.HighVolumeLowValue))
			{
				return new[] { ObjectFactory.Get<ITopLevelDataObjectReader>("eTailUSLVClearanceDataObjectReader", dataObject, subShipment, logger, factory) };
			}

			return System.Linq.Enumerable.Empty<ITopLevelDataObjectReader>();
		}

		public ITopLevelDataObjectWriter GetNewCusSCAOceanBillDataObjectWriter(IDataWritingManager manager)
		{
			return null;
		}

		#endregion

		#region IUSUniversalCustomsDataObjectProvider Members

		public void PublishDeclarationUniversalEvent(Integration.Customs.US.IJobDeclaration iDeclaration)
		{
			var declaration = iDeclaration as JobDeclaration;
			if (declaration != null)
			{
				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ExportCustomsClearedCode);
				query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + " DESC";

				var declarationLogs = declaration.LogsOfDeclarationOrShipment.GetAllLogs();
				var exportCustomsClearedLogs = declarationLogs.Find(query);
				if (exportCustomsClearedLogs.Length > 0)
				{
					var factory = new BusinessObjectFactory();
					UniversalDataBuss.DataObjects.Universal.Event[] events;
					using (factory.AddDisposableService())
					{
						events = UniversalXmlWorkflowProcessor.PublishUniversalEvent(factory, new RecipientRoleType[2] { RecipientRoleType.FOR, RecipientRoleType.SPM }, declaration, (StmALog)exportCustomsClearedLogs[0]).ToArray();
						factory.Save();
					}
				}
			}
		}

		public UniversalShipment.UniversalDataObjectReaderHelper GetNewUniversalDataObjectReaderHelper(UniversalObjectFactory factory, string sourceCountryCode, string dataProviderForCodeMapping = null)
		{
			return new UniversalDataObjectReaderHelper(factory, sourceCountryCode);
		}

		public ITopLevelDataObjectReader GetNewStandaloneCommercialInvoiceDataObjectReader(UniversalDataBuss.DataObjects.Universal.Shipment shipmentDataObject, UniversalDataBuss.DataObjects.Universal.Customs.CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new StandaloneCommercialInvoiceDataObjectReader(shipmentDataObject, invoiceDataObject, logger, factory);
		}

		public UniversalShipment.StandaloneCommercialInvoiceDataObjectWriter GetNewStandaloneCommercialInvoiceDataObjectWriter(IDataWritingManager manager)
		{
			return new StandaloneCommercialInvoiceDataObjectWriter(manager);
		}

		#endregion
	}
}
