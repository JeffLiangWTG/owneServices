using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal
{
	class ConsolLinker<TConsol> : TransportAndContainerParentLinker<TConsol>
		where TConsol : CommonConsol
	{
		internal ConsolLinker(BusinessObjectFactory factory, IUniversalFreightHelper helper)
			: base(factory, helper)
		{
		}

		internal ConsolLinker(BusinessObjectFactory factory, IUniversalFreightHelper helper, IXmlImportLogger logger)
			: base(factory, helper)
		{
			this.logger = logger;
		}

		readonly IXmlImportLogger logger;

		internal BusinessObject[] GetLogParent(IXmlEventValueObject xmlEvent)
		{
			var mawbNumber = xmlEvent.Context.MAWBNumber.Replace("-", "");
			var mbolNumber = xmlEvent.Context.MBOLNumber.GetValueOrDefault();
			var ulds = xmlEvent.Context.ULDIdentifications;
			var containers = xmlEvent.Context.ContainerNumbers;

			var canBeAir = !mawbNumber.IsEmpty;
			var canBeSeaRailRoad = !mbolNumber.IsEmpty || mawbNumber.IsEmpty;

			TConsol consol = null;
			if (canBeAir && !ulds.IsNullOrEmpty())
			{
				consol = GetConsol(xmlEvent, mawbNumber, ulds[0], true);
			}

			if (consol == null && canBeSeaRailRoad && !containers.IsNullOrEmpty())
			{
				consol = GetConsol(xmlEvent, mbolNumber, containers[0], false);
			}

			if (consol == null && canBeAir && ulds.IsNullOrEmpty())
			{
				consol = GetConsol(xmlEvent, mawbNumber, ZString.Empty, true);
			}

			if (consol == null && canBeSeaRailRoad && containers.IsNullOrEmpty())
			{
				consol = GetConsol(xmlEvent, mbolNumber, ZString.Empty, false);
			}

			return consol != null
				? GetLogParent(consol, xmlEvent, consol.IsAir)
				: null;
		}

		TConsol GetConsol(IXmlEventValueObject eventValueObject, ZString masterBill, ZString containerNumber, bool isAir)
		{
			var carrierBookingRef = eventValueObject.Context.CarriersBookingReference;
			if (masterBill.IsEmpty && carrierBookingRef.IsEmpty)
			{
				return null;
			}

			var consols = GetMatchedConsols(eventValueObject, masterBill, containerNumber, isAir);
			return GetLatestConsol(consols, carrierBookingRef, masterBill);
		}

		IReadOnlyCollection<TConsol> GetMatchedConsols(IXmlEventValueObject eventValueObject, ZString masterBill, ZString containerNumber, bool isAir)
		{
			var query = BuildQuery(masterBill, eventValueObject.Context.CarriersBookingReference, isAir, false);
			var consols = factory.Load<TConsol>(query);

			var coLoadQuery = BuildQuery(masterBill, eventValueObject.Context.CarriersBookingReference, isAir, true);
			var coLoadConsols = factory.Load<TConsol>(coLoadQuery);

			if (consols.Length + coLoadConsols.Length <= 1)
			{
				return consols.Concat(coLoadConsols).ToArray();
			}

			return !masterBill.IsEmpty
				? MatchConsolsByMasterBill(eventValueObject, masterBill, containerNumber, consols, coLoadConsols)
				: MatchConsolsByCarrierC1CCode(eventValueObject, containerNumber, consols, coLoadConsols);
		}

		IReadOnlyCollection<TConsol> MatchConsolsByMasterBill(IXmlEventValueObject eventValueObject, ZString masterBill, ZString containerNumber, TConsol[] consols, TConsol[] coLoadConsols)
		{
			var matchedConsols = consols.Where(c => c.JK_MasterBillNum == masterBill).ToArray();
			var matchedCoLoadConsols = coLoadConsols.Where(c => c.JK_CoLoadMasterBill == masterBill).ToArray();

			switch (matchedConsols.Length + matchedCoLoadConsols.Length) {
				case 0:
					matchedConsols = MatchConsolsByBookingReference(eventValueObject, consols, coLoadConsols).ToArray();
					return matchedConsols.Length == 1 ? matchedConsols : null;
				case 1:
					return matchedConsols.Concat(matchedCoLoadConsols).ToArray();
				default:
					return MatchConsolsByCarrierC1CCode(eventValueObject, containerNumber, matchedConsols, matchedCoLoadConsols);
			}
		}

		IReadOnlyCollection<TConsol> MatchConsolsByBookingReference(IXmlEventValueObject eventValueObject, TConsol[] consols, TConsol[] coLoadConsols)
		{
			var carrierBookingRef = eventValueObject.Context.CarriersBookingReference;
			TConsol[] matchedConsols = [];
			TConsol[] matchedCoLoadConsols = [];

			if (!carrierBookingRef.IsEmpty)
			{
				matchedConsols = consols.Where(c => c.JK_BookingReference == carrierBookingRef).ToArray();
				matchedCoLoadConsols = coLoadConsols.Where(c => c.JK_CoLoadBookingReference == carrierBookingRef).ToArray();
			}

			return matchedConsols.Concat(matchedCoLoadConsols).ToArray();
		}

		IReadOnlyCollection<TConsol> MatchConsolsByCarrierC1CCode(IXmlEventValueObject eventValueObject, ZString containerNumber, TConsol[] consols, TConsol[] coLoadConsols)
		{
			var carrierC1CCode = eventValueObject.Context.CarrierC1CCode.GetValueOrDefault();
			TConsol[] matchedConsols = [];
			TConsol[] matchedCoLoadConsols = [];

			if (!carrierC1CCode.IsEmpty)
			{
				matchedConsols = consols.Where(c => c.ShippingLine != null && c.ShippingLine.C1CCode == carrierC1CCode).ToArray();
				matchedCoLoadConsols = coLoadConsols.Where(c => c.Creditor != null && c.Creditor.C1CCode == carrierC1CCode).ToArray();
			}

			switch (matchedConsols.Length + matchedCoLoadConsols.Length)
			{
				case 0:
					return MatchConsolsByBookingReferenceWithFallback(eventValueObject, containerNumber, consols, coLoadConsols);
				case 1:
					return matchedConsols.Concat(matchedCoLoadConsols).ToArray();
				default:
					return MatchConsolsByBookingReferenceWithFallback(eventValueObject, containerNumber, matchedConsols, matchedCoLoadConsols);
			}
		}

		IReadOnlyCollection<TConsol> MatchConsolsByBookingReferenceWithFallback(IXmlEventValueObject eventValueObject, ZString containerNumber, TConsol[] consols, TConsol[] coLoadConsols)
		{
			var carrierBookingRef = eventValueObject.Context.CarriersBookingReference;
			TConsol[] matchedConsols = [];
			TConsol[] matchedCoLoadConsols = [];

			if (!carrierBookingRef.IsEmpty)
			{
				matchedConsols = consols.Where(c => c.JK_BookingReference == carrierBookingRef).ToArray();
				matchedCoLoadConsols = coLoadConsols.Where(c => c.JK_CoLoadBookingReference == carrierBookingRef).ToArray();
			}

			switch (matchedConsols.Length + matchedCoLoadConsols.Length)
			{
				case 0:
					return MatchConsolsByContainerNumber(eventValueObject, containerNumber, consols.Concat(coLoadConsols).ToArray());
				case 1:
					return matchedConsols.Concat(matchedCoLoadConsols).ToArray();
				default:
					return MatchConsolsByContainerNumber(eventValueObject, containerNumber, matchedConsols.Concat(matchedCoLoadConsols).ToArray());
			}
		}

		IReadOnlyCollection<TConsol> MatchConsolsByContainerNumber(IXmlEventValueObject eventValueObject, ZString containerNumber, TConsol[] consols)
		{
			TConsol[] matchedConsols = [];

			if (!containerNumber.IsEmpty)
			{
				matchedConsols = consols.Where(c => c.Containers.OfType<CommonContainer>().Any(y => y.JC_ContainerNum == containerNumber)).ToArray();
			}

			var matchedConsolsLength = matchedConsols.Length;
			return matchedConsolsLength == 1
				? matchedConsols
				: MatchConsolsByMBOLOriginAndDestination(eventValueObject, matchedConsolsLength == 0 ? consols : matchedConsols);
		}

		IReadOnlyCollection<TConsol> MatchConsolsByMBOLOriginAndDestination(IXmlEventValueObject eventValueObject, TConsol[] consols)
		{
			var mbolOriginUNLOCO = eventValueObject.Context.MBOLOriginUNLOCO;
			var mbolDestinationUNLOCO = eventValueObject.Context.MBOLDestinationUNLOCO;

			if (mbolOriginUNLOCO.IsEmpty || mbolDestinationUNLOCO.IsEmpty)
			{
				return consols;
			}

			var matchedConsolsByOrigin = consols.Where(c => c.JK_RL_NKLoadPort == mbolOriginUNLOCO).ToArray();
			var matchedConsolsByDestination = consols.Where(c => c.JK_RL_NKDischargePort == mbolDestinationUNLOCO).ToArray();

			var hasMatchesByOrigin = matchedConsolsByOrigin.Length > 0;
			var hasMatchesByDestination = matchedConsolsByDestination.Length > 0;

			if (hasMatchesByOrigin && hasMatchesByDestination)
			{
				return consols.Where(c => c.JK_RL_NKLoadPort == mbolOriginUNLOCO && c.JK_RL_NKDischargePort == mbolDestinationUNLOCO).ToArray();
			}

			if (hasMatchesByOrigin)
			{
				return matchedConsolsByOrigin;
			}

			if (hasMatchesByDestination)
			{
				return matchedConsolsByDestination;
			}

			return consols;
		}

		ZQuery BuildQuery(ZString masterBill, ZString carrierBookingRef, bool isAir, bool isCoLoad)
		{
			var comparisonOperator = isAir ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual;
			var query = new ZQuery(JobConsolSchema.JK_TransportMode, comparisonOperator, Constants.TransportModes.Air);

			var isActiveQuery = new ZQuery(JobConsolSchema.JK_IsCancelled, false);
			query.AddToFilter(isActiveQuery);

			AddAgentTypeFilter(query, isCoLoad);

			ZQuery masterBillQuery = null;

			if (!masterBill.IsEmpty)
			{
				masterBillQuery = new ZQuery(isCoLoad ? JobConsolSchema.JK_CoLoadMasterBill : JobConsolSchema.JK_MasterBillNum, masterBill);
				query.AddToFilter(masterBillQuery);
			}

			if (!carrierBookingRef.IsEmpty)
			{
				var bookingRefQuery = new ZQuery(isCoLoad ? JobConsolSchema.JK_CoLoadBookingReference : JobConsolSchema.JK_BookingReference, carrierBookingRef);

				if (masterBillQuery != null)
				{
					masterBillQuery.AddToFilter(bookingRefQuery, JoinCondition.Or);
				}
				else
				{
					query.AddToFilter(bookingRefQuery, JoinCondition.And);
				}
			}

			if (isAir)
			{
				var validTime = FreightDataRegistry.Instance.MAWBRecyclePeriod.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				if (validTime > 0)
				{
					query.AddToFilter(JobConsolSchema.JK_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow.AddMonths(-validTime));
				}
			}

			helper.AddConsolParameters(query);
			return query;
		}

		void AddAgentTypeFilter(ZQuery query, bool isCoLoad)
		{
			ZQuery coLoadQuery;
			if (isCoLoad)
			{
				var coLoadReferenceQuery = new ZQuery(JobConsolSchema.JK_CoLoadMasterBill, SQLComparisonOperator.NotEqual, ZString.Empty);
				coLoadReferenceQuery.AddToFilter(JoinCondition.Or, JobConsolSchema.JK_CoLoadBookingReference, SQLComparisonOperator.NotEqual, ZString.Empty);
				coLoadQuery = new ZQuery(JobConsolSchema.JK_AgentType, SQLComparisonOperator.Equal, Constants.AgentType.CoLoad);
				coLoadQuery.AddToFilter(coLoadReferenceQuery, JoinCondition.And);
			}
			else
			{
				var coLoadReferenceQuery = new ZQuery(JobConsolSchema.JK_CoLoadMasterBill, SQLComparisonOperator.Equal, ZString.Empty);
				coLoadReferenceQuery.AddToFilter(JoinCondition.And, JobConsolSchema.JK_CoLoadBookingReference, SQLComparisonOperator.Equal, ZString.Empty);
				coLoadQuery = new ZQuery(JobConsolSchema.JK_AgentType, SQLComparisonOperator.NotEqual, Constants.AgentType.CoLoad);
				coLoadQuery.AddToFilter(coLoadReferenceQuery, JoinCondition.Or);
			}

			query.AddToFilter(coLoadQuery);
		}

		TConsol GetLatestConsol(IReadOnlyCollection<TConsol> consols, ZString carrierBookingRef, ZString masterBill)
		{
			if (consols.IsNullOrEmpty())
			{
				return null;
			}

			MatchConsolResultService.Register(factory, consols, carrierBookingRef, masterBill);

			return consols.MaxBy(consol => consol.JK_SystemCreateTimeUtc);
		}

		protected override IContainerLinker GetContainerLinker(TConsol consol)
		{
			return new ConsolContainerLinker(consol, logger);
		}
	}
}
