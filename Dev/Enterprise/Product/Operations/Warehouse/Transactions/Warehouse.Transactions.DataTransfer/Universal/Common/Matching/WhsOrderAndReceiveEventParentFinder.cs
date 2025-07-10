using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public abstract class WhsOrderAndReceiveEventParentFinder<TDocket> : WhsDocketEventParentFinder<TDocket>
		where TDocket : WhsDocket
	{
		protected WhsOrderAndReceiveEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		#region Find Jobs Matching the Event

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			TDocket result;

			// Many tests incorrectly lack a Top Level DataContext. This is to fix those test failures.
			var helper = logger.TopLevelDataContext != null ? GetNewCustomsHelper(xmlEvent, logger.TopLevelDataContext) : null;
			if (helper != null && helper.IsDataSourceCustoms)
			{
				result = GetDocketMatchingCustomsJobNumber(xmlEvent, helper);
			}
			else
			{
				var whsDocketReferences = GetAllReferencesForMatching(xmlEvent);
				var matcher = GetMatcher(whsDocketReferences);
				result = matcher.GetBestMatch();
			}

			ProcessEvent(xmlEvent);

			return result == null ? null : new[] { result };
		}

		protected void ProcessEvent(UniversalEvent xmlEvent)
		{
			var eventTypeCode = xmlEvent.EventType.GetValueOrDefault();
			if (eventTypeCode == AutoEvents.ConfirmationOfExitCode)
			{
				var processor = new COEEventProcessor(factory, logger, xmlEvent);
				processor.Execute();
			}
			else if (eventTypeCode == AutoEvents.CustomsNumberEnteredCode)
			{
				var processor = new CENEventProcessor(factory, logger, xmlEvent);
				processor.Execute();
			}
		}

		protected abstract CustomsDataSourceHelper<TDocket> GetNewCustomsHelper(UniversalEvent xmlEvent, IDataContextDataObject topLevelDataObject);

		#region GetDocketMatchingCustomsJobNumber

		TDocket GetDocketMatchingCustomsJobNumber(UniversalEvent xmlEvent, CustomsDataSourceHelper<TDocket> helper)
		{
			var docketQuery = GetDocketMatchingCustomsJobNumberQuery(helper.CustomsParentReferenceToMatchForDocket);

			var eventType = xmlEvent.EventType.GetValueOrDefault();
			var result = factory.LoadTop1<TDocket>(docketQuery);
			if (result != null)
			{
				if (!result.Warehouse.WW_IsVirtualWarehouse && !helper.IsWarehouseBondedChangeOfInventory)
				{
					ThrowReadFailureExceptionIfDocketInRealWhsIsFinalisedOrCancelled(eventType, result);
				}
				else if (eventType == Events.CancelTheWarehouseJobCode && result.IsCancelled)
				{
					ThrowDocketCancelledException(result, eventType);
				}
			}

			return result;
		}

		#region GetDocketMatchingCustomsJobNumberQuery

		ZQuery GetDocketMatchingCustomsJobNumberQuery(ZString customsParentReference)
		{
			var docketQuery = new ZQuery();
			docketQuery.AddToFilter(WhsDocketSchema.WD_DocketType, DocketTypeCode);
			docketQuery.AddToFilter(WhsDocketSchema.WD_CustomsParentReference, customsParentReference);
			docketQuery.OrderBy = WhsDocketSchema.Constants.WD_ExternalReferenceSplit + OrderByClause.Descending;

			return docketQuery;
		}

		protected abstract string DocketTypeCode { get; }

		#endregion

		#region ThrowReadFailureExceptionIfDocketInRealWhsIsFinalisedOrCancelled

		void ThrowReadFailureExceptionIfDocketInRealWhsIsFinalisedOrCancelled(ZString eventType, TDocket result)
		{
			if (result.IsCancelled)
			{
				ThrowDocketCancelledException(result, eventType);
			}
			else if (result.IsFinalised && (eventType == Events.HoldTheWarehouseOrderCode || eventType == Events.CancelTheWarehouseJobCode))
			{
				var errorMessage = Res.GetString("41fccbad-407b-4b32-9a75-cd71f607de65", "Rejected Event '{0}'. Cannot Hold or Cancel {1} as it is already Finalized.", eventType, result.HumanReadableName);
				throw new DataObjectReadFailureException(errorMessage);
			}
		}

		void ThrowDocketCancelledException(TDocket docket, ZString eventType)
		{
			var errorMessage = Res.GetString("7d7f219b-3b8c-418a-802e-df2089c7ccce", "Rejected Event '{0}'. Matching {1} is already Canceled.", eventType, docket.HumanReadableName);
			throw new DataObjectReadFailureException(errorMessage);
		}

		#endregion

		#endregion

		#region GetAllReferencesForMatching

		WhsOrderAndReceiveReferences GetAllReferencesForMatching(UniversalEvent xmlEvent)
		{
			var result = new WhsOrderAndReceiveReferences();

			IXmlEventValueObject eventValueObject = xmlEvent;
			result.ClientReference = eventValueObject.Context.ClientReference;
			result.TransportReference = eventValueObject.Context.TransportReference;

			result.References = GetAdditionalReferencesForMatching(factory, xmlEvent);
			AddSpecificReferences(result, xmlEvent);

			return result;
		}

		protected abstract void AddSpecificReferences(WhsOrderAndReceiveReferences whsOrderReferences, IXmlEventValueObject eventValueObject);
		protected abstract WhsOrderAndReceiveLastResortMatcher<TDocket> GetMatcher(WhsOrderAndReceiveReferences whsDocketReferences);

		#endregion

		#endregion
	}
}
