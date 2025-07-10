using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class HVLVItemDataContextManager : EventDataContextManager<HVLVItem>, IEventTransformer
	{
		public override DataContextType DataContextType => DataContextType.HVLVItem;

		public override ZString DataContextKey => ParentBO.HVI_ItemId;

		public override string DefaultOutputDirectory => null;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger) =>
			new ZQuery(HVLVItemSchema.HVI_ItemId, matchingValues.Key);

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				result.AddIfNotEmpty(UniversalEvent.ContextTypes.ShippersReference, ParentBO.HVI_ShipperReference);
			}

			return result;
		}

		protected override void OnUniversalEventAddedCore(IXmlSessionTracker logger, UniversalEvent eventAdded)
		{
			base.OnUniversalEventAddedCore(logger, eventAdded);
			switch (eventAdded.EventType.GetValueOrDefault())
			{
				case AutoEvents.ScannedCode:
					ParentBO.UpdateFromScan(eventAdded);
					break;
				case AutoEvents.StatusUpdatedCode:
					var referenceParameters = StmALog.GetParametersFromReference(eventAdded.EventReference);
					if (referenceParameters.TryGetValue(EventReferenceParameters.Codes.New, out var newStatus)
						&& !string.IsNullOrEmpty(newStatus))
					{
						using (ParentBO.SuspendAddStatusUpdatedEvent())
						{
							ParentBO.HVI_Status = newStatus;
						}
					}

					break;
				default:
					break;
			}
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) =>
			new HVLVItemEventParentFinder(this, factory, logger);

		EventValue IEventTransformer.Transform(EventValue sourceEventValue, UniversalEvent sourceUniversalEvent, IStmALogParent logParent)
		{
			var item = logParent as HVLVItem;
			if (item != null && sourceEventValue.EventType.Code == AutoEvents.StatusUpdatedCode)
			{
				var referenceNumber = item.GetItemEventReferenceNumber();
				var referenceType = item.GetItemEventReferenceType();
				var parameters = new Dictionary<string, string>(sourceEventValue.Parameters);
				parameters.Add(EventReferenceParameters.Codes.Old, item.HVI_Status);
				parameters.Add(EventReferenceParameters.Codes.ReferenceNumber, referenceNumber);
				parameters.Add(EventReferenceParameters.Codes.Type, referenceType);
				return new EventValue(
					sourceEventValue.EventType,
					sourceEventValue.IsEstimate,
					sourceEventValue.DeferFiringWorkflow,
					sourceEventValue.EventTime,
					sourceEventValue.Reference,
					parameters);
			}

			return sourceEventValue;
		}
	}
}
