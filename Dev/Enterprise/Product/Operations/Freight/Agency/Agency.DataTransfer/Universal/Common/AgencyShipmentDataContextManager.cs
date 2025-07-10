using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	abstract class AgencyShipmentDataContextManager<T> : ShipmentDataContextManager<T>, IEventTransformer, IDataContextCoordinator where T : AgencyShipment
	{
		public override ZString DataContextKey
		{
			get { return ParentBO.JS_UniqueConsignRef; }
		}

		public override string DefaultOutputDirectory
		{
			get { return SystemDataRegistry.Instance.ShipmentExportDirectory.Value; }
		}

		public override bool ManagesShipments
		{
			get { return true; }
		}

		#region Implementation

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return recipientRoles.Any(o => o.Code == RecipientRoleType.CAR);
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, matchingValues.Key);
		}

		#region UniversalEvent Context References

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			IEnumerable<KeyValuePair<TypeWithDescription, IZType>> result = null;

			if (ParentBO != null)
			{
				var references = new AgencyShipmentReferences(ParentBO);
				result = references.GetEventContextValues();
			}

			return result == null || !result.Any() ? null : result;
		}

		#endregion

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new AgencyShipmentEventParentFinder<T>(factory, this, logger);
		}

		public EventValue Transform(EventValue sourceEventValue, UniversalEvent sourceUniversalEvent, IStmALogParent logParent)
		{
			if (sourceEventValue.Code == AutoEvents.SubscriptionRequestedCode && sourceUniversalEvent != null)
			{
				return SubscriptionRequesteEventTransformer.Transform(sourceEventValue, sourceUniversalEvent);
			}

			return sourceEventValue;
		}

		#endregion

		public string GetUniqueContextIdentifier(IXmlEventValueObject xmlEvent)
		{
			return xmlEvent.EventType == AutoEvents.SubscriptionRequested.Code ? JobShipmentSchema.Constants.TableName : string.Empty;
		}
	}
}


