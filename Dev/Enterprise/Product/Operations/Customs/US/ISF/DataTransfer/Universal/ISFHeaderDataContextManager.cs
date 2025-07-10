using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.ISF.DataTransfer.Universal.Reader;
using Enterprise.Customs.US.ISF.DataTransfer.Universal.Writer;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal
{
	public class ISFHeaderDataContextManager : ShipmentDataContextManager<CusISFHeader>, IDataContextManagerFromEDIMessage
	{
		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new ISFHeaderDataObjectReader(universalShipment, logger, factory);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new ISFHeaderDataObjectWriter(writeManager);
		}

		public override bool ManagesShipments
		{
			get { return true; }
		}

		public override bool ManagesEvents
		{
			get { return true; }
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				var eventContextReader = new ISFEventContextReader(ParentBO);
				eventContextReader.AddCusISFContextValues(result);
			}

			return result.Count > 0 ? result : null;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ISFEventParentFinder(factory, this, logger);
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.BF_JobReference; }
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.USImporterSecurityFiling; }
		}

		public override string DefaultOutputDirectory
		{
			get { return string.Empty; }
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var result = new ZQuery(CusISFHeaderSchema.BF_JobReference, matchingValues.Key);
			return result;
		}

		public void OnLogParentFoundFromEDIMessage(IXmlSessionTracker logger, IXmlEventValueObject eventDataObject, IEDIMessage message, BusinessObject businessObject)
		{
			var eventData = eventDataObject as Event;
			if (businessObject != null && SuretyToBrokerNoticeMessageProcessorHelper.IsSuretyToBrokerNoticeMessage(eventData))
			{
				var processor = new ISFSuretyToBrokerNoticeMessageProcessor(businessObject as CusISFHeader, eventData);
				processor.SendAcknowledgementReport();
			}
		}
	}
}
