using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal
{
	class StandaloneCommercialInvoiceDataContextManager : ShipmentDataContextManager<BaseJobComInvoiceHeader>, IShipmentDataContextManager
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.CustomsCommercialInvoice; }
		}

		public override ZString DataContextKey
		{
			get { return ZString.Empty; }
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			UniversalCustoms.CommercialInvoiceHeader invoiceHeaderDataObject = null;
			var commercialInfo = universalShipment.CommercialInfo;
			if (commercialInfo != null)
			{
				var commercialInvoiceCollection = commercialInfo.CommercialInvoiceCollection;
				if (commercialInvoiceCollection != null)
				{
					invoiceHeaderDataObject = commercialInvoiceCollection.FirstOrDefault();
				}
			}
			return invoiceHeaderDataObject == null ? null : new StandaloneCommercialInvoiceDataObjectReader(universalShipment, invoiceHeaderDataObject, logger, factory);
		}

		public override bool ManagesShipments
		{
			get { return true; }
		}

		public override bool ManagesEvents
		{
			get { return true; }
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new StandaloneCommercialInvoiceDataObjectWriter(writeManager);
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null;
		}

		public override string DefaultOutputDirectory
		{
			get { return SystemDataRegistry.Instance.CustomDeclarationExportDirectory.Value; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null;
		}

		bool IShipmentDataContextManager.UseIncomingShipmentData(ITopLevelDataObject topLeveDataObject, IXmlImportLogger logger, IUniversalObjectFactory factory)
		{
			bool result = false;
			var universalShipment = (UniversalShipment)topLeveDataObject;
			if (TryGetMatchingDataTarget(universalShipment, out var dataTarget))
			{
				result = UpdateOrCreateNewBusinessObject(universalShipment, logger, dataTarget, (UniversalObjectFactory)factory);
			}

			return result;
		}

		bool UpdateOrCreateNewBusinessObject(UniversalShipment universalShipment, IXmlImportLogger logger, IDataTargetDataObject dataTarget, UniversalObjectFactory factory)
		{
			var provider = factory.BOFactory.GetUniversalCustomsDataObjectProvider(universalShipment.GetTargetCountryCode());
			BusinessObject firstBizObj = null;
			foreach (var (shipmentDataObject, invoiceHeaderDataObject) in GetInvoiceHeaderDataObjectsToProcess(universalShipment))
			{
				var bizObj = ProcessInvoiceData(shipmentDataObject, invoiceHeaderDataObject, logger, factory, ref provider);
				if (firstBizObj == null)
				{
					firstBizObj = bizObj;
				}
			}

			if (firstBizObj != null)
			{
				logger.LogTopLevelDataContextKey(() => firstBizObj.GetUniversalDataContextManager().DataContextKey);
			}

			return firstBizObj != null;
		}

		BusinessObject ProcessInvoiceData(UniversalShipment shipmentDataObject, UniversalCustoms.CommercialInvoiceHeader invoiceHeaderDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ref IUniversalCustomsDataObjectProvider provider)
		{
			ITopLevelDataObjectReader reader = null;
			if (provider != null)
			{
				reader = provider.GetNewStandaloneCommercialInvoiceDataObjectReader(shipmentDataObject, invoiceHeaderDataObject, logger, factory);
				if (reader == null)
				{
					provider = null;
				}
			}
			var bOReader = reader ?? new StandaloneCommercialInvoiceDataObjectReader(shipmentDataObject, invoiceHeaderDataObject, logger, factory);
			BusinessObject matchedBizObj = null;
			bOReader.ReadIntoBusinessObject(ref matchedBizObj);
			return matchedBizObj;
		}

		IEnumerable<Tuple<UniversalShipment, UniversalCustoms.CommercialInvoiceHeader>> GetInvoiceHeaderDataObjectsToProcess(UniversalShipment dataObject)
		{
			foreach (var invoiceInfoData in GetInvoiceHeaderDataObjectsToProcess(dataObject, dataObject.CommercialInfo))
			{
				yield return invoiceInfoData;
			}
			if (dataObject.SubShipmentCollection != null)
			{
				foreach (var subShipmentDataObject in dataObject.SubShipmentCollection)
				{
					foreach (var invoiceDataInfo in GetInvoiceHeaderDataObjectsToProcess(subShipmentDataObject))
					{
						yield return invoiceDataInfo;
					}
				}
			}
		}

		IEnumerable<Tuple<UniversalShipment, UniversalCustoms.CommercialInvoiceHeader>> GetInvoiceHeaderDataObjectsToProcess(UniversalShipment dataObject, UniversalCustoms.CommercialInfo commercialInfo)
		{
			if (commercialInfo != null)
			{
				foreach (var invoiceDataObject in GetInvoiceHeaderDataObjectsToProcess(dataObject, commercialInfo.CommercialInvoiceCollection))
				{
					yield return invoiceDataObject;
				}
				if (commercialInfo.SubGroupCollection != null)
				{
					foreach (var subCommercialInfo in commercialInfo.SubGroupCollection)
					{
						foreach (var invoiceInfoData in GetInvoiceHeaderDataObjectsToProcess(dataObject, subCommercialInfo))
						{
							yield return invoiceInfoData;
						}
					}
				}
			}
		}

		IEnumerable<Tuple<UniversalShipment, UniversalCustoms.CommercialInvoiceHeader>> GetInvoiceHeaderDataObjectsToProcess(UniversalShipment dataObject, DataObjectList<UniversalCustoms.CommercialInvoiceHeader> commercialInvoiceCollection)
		{
			if (commercialInvoiceCollection != null)
			{
				foreach (var invoiceDataObject in commercialInvoiceCollection)
				{
					yield return new Tuple<UniversalShipment, UniversalCustoms.CommercialInvoiceHeader>(dataObject, invoiceDataObject);
				}
			}
		}
	}
}
