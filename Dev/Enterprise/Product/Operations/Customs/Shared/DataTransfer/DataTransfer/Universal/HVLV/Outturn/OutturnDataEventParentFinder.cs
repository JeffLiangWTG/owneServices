using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn
{
	public class OutturnDataEventParentFinder : EventParentFinder
	{
		public OutturnDataEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			var result = System.Array.Empty<BusinessObject>();

			if (xmlEvent?.ContextCollection is List<Context> contextCollection && xmlEvent.EventType.GetValueOrDefault() == AutoEvents.GateInCode && contextCollection.Any(c => c.Type.Type.Equals(nameof(UniversalEvent.ContextTypes.DepotCode))))
			{
				if (UniversalEventHasRequiredContextTypes(xmlEvent, out var depotCode, out var containerNumber, out var gateInTime, out var missingParamsText))
				{
					var matchingOutturns = GetMatchingOutturns(depotCode, containerNumber);
					if (matchingOutturns.Any())
					{
						PopulateCargoReceiptDateOnOutturns(matchingOutturns, xmlEvent);
						logger.Log(LogType.Information, ResString.GetMultilingualString("422017c2-3e66-4d3a-a769-d8b6fd92f948", "Found Outturns with Premise - '{0}', Container Number - '{1}'. Cargo Receipt Date has been updated to '{2}'.", depotCode, containerNumber, gateInTime.ToStandardDateTimeString()));

						result = matchingOutturns.ToArray();
					}
					else
					{
						logger.Log(LogType.Information, ResString.GetMultilingualString("c32924e0-70ee-479f-89a0-98ffca3fab3d", "Gate In event received but did not find any matching Outturn with Premise - '{0}', Container Number - '{1}' to update.", depotCode, containerNumber));
					}
				}
				else
				{
					logger.Log(LogType.Information, ResString.GetMultilingualString("bdccc9c6-40bb-494e-b801-84d6bc96a347", "Missing mandatory context types from Gate In event: {0}", missingParamsText));
				}
			}

			return result;
		}

		#region Transit Warehouse Receive

		bool UniversalEventHasRequiredContextTypes(UniversalEvent xmlEvent, out ZString depotCode, out ZString containerNumber, out ZDateTime gateInTime, out string missingParamsText)
		{
			var eventDataObject = xmlEvent as IXmlEventValueObject;
			depotCode = eventDataObject.Context.DepotCode;
			containerNumber = eventDataObject.Context.ContainerNumbers?.FirstOrDefault() ?? ZString.Empty;
			gateInTime = eventDataObject.Context.TimeOfArrival.GetValueOrDefault();

			missingParamsText = string.Join(", ", new[] {
				(Res.GetString("1e7f9db7-50aa-459b-aec0-92c50fed8ae2", "Depot Code"), depotCode),
				(Res.GetString("ecaa4b7a-5e26-4144-9eaa-495ad42a2d47", "Container Code"), containerNumber),
				(Res.GetString("05fa7d9b-770d-4978-9fa4-b8eb727adf46", "Gate In Time"), (ZString)gateInTime.ToStandardDateTimeString())
			}
			.Where(e => e.Item2.IsEmpty)
			.Select(e => e.Item1));

			return !depotCode.IsEmpty && !gateInTime.IsEmpty && !containerNumber.IsEmpty;
		}

		List<CusOutturn> GetMatchingOutturns(ZString depotCode, ZString containerNumber)
		{
			return factory.Load<CusOutturn>(GetMatchingOutturnsQuery(depotCode, containerNumber)).ToList();
		}

		ZQuery GetMatchingOutturnsQuery(ZString depotCode, ZString containerNumber)
		{
			var outturnQuery = new ZDBOnlyQuery(typeof(CusOutturn));

			if (depotCode.IsEmpty || containerNumber.IsEmpty)
			{
				outturnQuery = (ZDBOnlyQuery)ZQuery.NoResultQuery;
			}
			else
			{
				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), CusOutturnHeaderSchema.C6_OA_OutturningPremise);
				orgAddressQuery.AddToFilter(OrgAddressSchema.OA_Code, depotCode);

				var headerQuery = new ZDBOnlySubQuery(typeof(CusOutturnHeader), CusOutturnSchema.C5_C6);
				headerQuery.AddSubQuery(orgAddressQuery, JoinCondition.And);

				outturnQuery.AddToFilter(CusOutturnSchema.C5_ContainerNumber, containerNumber);
				outturnQuery.AddToFilter(CusOutturnSchema.C5_CargoReceiptDate, SQLComparisonOperator.Equal, ZDateTime.Empty);
				outturnQuery.AddSubQuery(headerQuery, JoinCondition.And);
			}

			return outturnQuery;
		}

		void PopulateCargoReceiptDateOnOutturns(List<CusOutturn> outturns, IXmlEventValueObject eventDataObject)
		{
			var gateInEventProcessor = new TWGateEventProcessor(eventDataObject, logger);

			foreach (var outturn in outturns)
			{
				gateInEventProcessor.Process(outturn);
			}
		}

		#endregion
	}
}
