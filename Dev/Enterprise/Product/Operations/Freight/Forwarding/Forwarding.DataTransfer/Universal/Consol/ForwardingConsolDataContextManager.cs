using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ForwardingConsolDataContextManager : ConsolDataContextManager<ForwardingConsol, ForwardingShipment, ForwardingContainer>, IEventTransformer, IParentEventDataContextManager
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.ForwardingConsol; }
		}

		public override string DefaultOutputDirectory
		{
			get { return SystemDataRegistry.Instance.ConsolExportDirectory.Value; }
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new ConsolDataObjectWriter(writeManager);
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new ConsolDataObjectReader(universalShipment, logger, factory, Helper);
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return recipientRoles.Any(o => o.Code == RecipientRoleType.FOR
							|| o.Code == RecipientRoleType.RAG
							|| o.Code == RecipientRoleType.SAG
							|| o.Code == RecipientRoleType.PAG
							|| o.Code == RecipientRoleType.DAG)
				&& dataSources != null && dataSources.Any(o => o.Type.GetValueOrDefault().EqualsIgnoringCase(DataContextType.ToString()));
		}

		protected override IUniversalFreightHelper GetNewHelper()
		{
			return new UniversalForwardingHelper();
		}

		protected override void OnUniversalEventAddedCore(IXmlSessionTracker logger, UniversalEvent eventAdded)
		{
			base.OnUniversalEventAddedCore(logger, eventAdded);
			eventAdded.OnCO2eRejectionEvent(ParentBO);

			if (eventAdded.EventType.Value == Events.BillStatusUpdated.Code
				&& FreightDataRegistry.Instance.EnableBoleroEBLIntegration.Value.EnableEBLIntegration)
			{
				ParentBO.OnEventsRelatedToOriginalBillNotes(eventAdded);
			}
		}

		public EventValue Transform(EventValue sourceEventValue, UniversalEvent sourceUniversalEvent, IStmALogParent logParent)
		{
			var res = sourceEventValue;

			switch (logParent)
			{
				case ForwardingContainer logParentContainer:
					res = ForwardingContainerEventTransformer.Transform(sourceEventValue, sourceUniversalEvent, logParentContainer);
					break;

				case Transport transport:
					res = TransportEventTransformer.Transform(sourceEventValue, sourceUniversalEvent, transport);
					if (transport.GetParentSafe() is ForwardingConsol consol)
					{
						res = ForwardingConsolEventTransformer.Transform(res, sourceUniversalEvent, consol);
					}
					break;

				case ForwardingConsol logParentConsol:
					res = ForwardingConsolEventTransformer.Transform(sourceEventValue, sourceUniversalEvent, logParentConsol);
					break;
			}

			if (sourceUniversalEvent.IsAirEBookingMessage())
			{
				res = new AirBookingEventTransformer().Transform(res, sourceUniversalEvent, logParent);
			}

			if (CarrierShipperReferenceMessageEventTransformer.IsVersioningEnabledMessage(sourceUniversalEvent))
			{
				return CarrierShipperReferenceMessageEventTransformer.Transform(sourceEventValue, sourceUniversalEvent, logParent);
			}

			return res;
		}

		IEnumerable<IEventDataContextManager> IParentEventDataContextManager.ChildContextManagers
		{
			get
			{
				return ParentBO.Containers.Select(
			  container =>
				  {
					  var manager = container.GetUniversalDataContextManager();
					  var containerDataContextManager = manager as ContainerDataContextManager;
					  if (containerDataContextManager != null)
					  {
						  containerDataContextManager.ShouldAddParentContextValues = false;
					  }
					  return manager;
				  }).OfType<IEventDataContextManager>();
			}
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var key = CarrierShipperReferenceMessageEventTransformer.IsVersioningEnabledMessage(matchingValues.DataObject) && ConsolCarrierShipperReferenceNumberCalculator.IsValidCarrierShipperReferenceNumber(matchingValues.Key)
				? (ZString)ConsolCarrierShipperReferenceNumberCalculator.GetConsolIDFromCarrierShipperReferenceNumber(matchingValues.Key)
				: matchingValues.Key;

			var consolQuery = new ZDBOnlyQuery(typeof(ForwardingConsol));
			consolQuery.AddToFilter(JobConsolSchema.JK_IsCancelled, false);

			consolQuery.AddToFilter(JobConsolSchema.JK_UniqueConsignRef, key);

			return consolQuery;
		}

		protected override bool BeforeLinkToExistingBusinessObject(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingConsol consolBO)
		{
			if (CarrierShipperReferenceMessageEventTransformer.IsVersioningEnabledMessage(universalShipment)
				&& CarrierShipperReferenceMessageEventTransformer.IsLowerCarrierShipperReferenceNumberReceived(universalShipment, consolBO))
			{
				logger.Log(LogType.Error, Res.GetString("c7ecc828-2a28-48ca-9a0a-06c90c0e4ed5", "Received for previous 'reset to original' version. Check with carrier for possible duplication."));

				return false;
			}

			return base.BeforeLinkToExistingBusinessObject(universalShipment, logger, factory, consolBO);
		}
	}
}
