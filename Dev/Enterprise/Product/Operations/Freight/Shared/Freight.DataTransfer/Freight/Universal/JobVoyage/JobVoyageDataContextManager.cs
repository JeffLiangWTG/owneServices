using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Freight.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.DataTransfer.Universal
{
	sealed class JobVoyageDataContextManager : ShipmentDataContextManager<JobVoyage>, IScheduleDataContextManager, IEventTransformer
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.SailingSchedule; }
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.JV_SendersMessageReference; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery(JobVoyageSchema.JV_SendersMessageReference, matchingValues.Key);
		}

		#region UniversalEvent Context References

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			IEnumerable<KeyValuePair<TypeWithDescription, IZType>> result = null;

			if (ParentBO != null)
			{
				var references = new JobVoyageReferences(ParentBO);
				result = GetContextValues(references);
			}

			return result != null && result.Any() ? result : null;
		}

		IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetContextValues(JobVoyageReferences references)
		{
			var contextValues = new List<KeyValuePair<TypeWithDescription, IZType>>();

			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.TransportMode, references.TransportMode);
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.VesselName, references.VesselName);
			if (references.TransportMode == Core.Constants.TransportModes.Air)
			{
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.FlightNumber, references.VoyageFlight);
			}
			else
			{
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.VoyageNumber, references.VoyageFlight);
			}
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.FlightDate, references.FlightDate);
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.IsCharter, references.IsCharter);
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.LloydsNumber, references.LloydsNumber);
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.VesselCallSign, references.VesselCallSign);
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.CarrierCode, references.CarrierSCACCode);

			return contextValues;
		}

		#endregion

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new JobVoyageEventParentFinder(factory, this, logger);
		}

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		public ITopLevelDataObjectWriter GetScheduleDataObjectWriter(IDataWritingManager writeManager)
		{
			return new ScheduleDataObjectWriter(writeManager);
		}

		public bool ManagesSchedules
		{
			get { return true; }
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalDataBuss.DataObjects.Core.UniversalObjectFactory factory)
		{
			return new JobVoyageShipmentDataObjectReader(universalShipment, logger, factory);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new JobVoyageShipmentDataObjectWriter(writeManager);
		}

		public override bool ManagesShipments
		{
			get { return true; }
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		public ZArchitecture.Business.EventValue Transform(ZArchitecture.Business.EventValue sourceEventValue, UniversalEvent sourceUniversalEvent, ZArchitecture.Business.IStmALogParent logParent)
		{
			if (logParent is JobVoyage voyage)
			{
				return JobVoyageEventTransformer.Transform(sourceEventValue, voyage);
			}

			return sourceEventValue;
		}
	}
}
