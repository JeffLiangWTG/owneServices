using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class ConsolLocator<TBusinessObject>
		where TBusinessObject : CommonConsol
	{
		public TBusinessObject Find(BusinessObjectFactory factory, ZString masterBill, ZString agentsReference, ZString transportMode, ZString firstLoad, ZString lastDischarge, ZDateTime eTD)
		{
			TBusinessObject result = FindCore(factory, masterBill, agentsReference, transportMode, firstLoad, lastDischarge, eTD, false);

			if (result == null && !masterBill.IsEmpty && !agentsReference.IsEmpty)
			{
				result = FindCore(factory, ZString.Empty, agentsReference, transportMode, firstLoad, lastDischarge, eTD, true);
			}
			return result;
		}

		TBusinessObject FindCore(BusinessObjectFactory factory, ZString masterBill, ZString agentsReference, ZString transportMode, ZString firstLoad, ZString lastDischarge, ZDateTime eTD, bool checkForEmptyMasterBill)
		{
			var filter = new ZQuery();
			TBusinessObject result = null;

			if (!masterBill.IsEmpty || !agentsReference.IsEmpty)
			{
				if (!masterBill.IsEmpty || checkForEmptyMasterBill)
				{
					filter.AddToFilter(JobConsolSchema.JK_MasterBillNum, masterBill);
				}

				if (masterBill.IsEmpty)
				{
					filter.AddToFilter(JobConsolSchema.JK_AgentsReference, agentsReference);
				}

				if (!transportMode.IsEmpty)
				{
					filter.AddToFilter(JobConsolSchema.JK_TransportMode, transportMode);

					if (transportMode == Core.Constants.TransportModes.Air)
					{
						var createTimeFilter = new ZQuery();

						createTimeFilter.AddToFilter(JobConsolSchema.JK_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddMonths(-Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value));
						createTimeFilter.AddToFilter(JoinCondition.Or, JobConsolSchema.JK_SystemCreateTimeUtc, SQLComparisonOperator.Equal, null);

						filter.AddToFilter(createTimeFilter);
					}
				}

				TBusinessObject[] possibleResults = GetConsols(factory, filter);
				if (possibleResults.Length == 1)
				{
					result = possibleResults[0];
				}
				else if (possibleResults.Length > 1)
				{
					foreach (TBusinessObject current in possibleResults)
					{
						if (Matches(transportMode, firstLoad, lastDischarge, eTD, current))
						{
							result = current;
							break;
						}
					}
				}
			}
			return result;
		}

		bool Matches(ZString transportMode, ZString firstLoad, ZString lastDischarge, ZDateTime eTD, TBusinessObject consol)
		{
			bool result = (!firstLoad.IsEmpty || !lastDischarge.IsEmpty) &&
				(firstLoad.IsEmpty || firstLoad.ToLower() == consol.JK_RL_NKLoadPort.ToLower()) &&
				(lastDischarge.IsEmpty || lastDischarge.ToLower() == consol.JK_RL_NKDischargePort.ToLower());

			if (!result && eTD.IsValid)
			{
				switch (transportMode)
				{
					case Core.Constants.TransportModes.Air:
					case Core.Constants.TransportModes.Road:
						result = IsMatchingAirOrRoadConsol(eTD, consol);
						break;

					case Core.Constants.TransportModes.Sea:
						result = IsMatchingSeaConsol(eTD, consol);
						break;
				}
			}

			return result;
		}

		bool IsMatchingAirOrRoadConsol(ZDateTime eTD, TBusinessObject consol)
		{
			return consol.Transports.DepartureTransport.JW_ETD == eTD;
		}

		bool IsMatchingSeaConsol(ZDateTime eTD, TBusinessObject consol)
		{
			return consol.Transports.DepartureTransport.JW_ETD >= eTD && consol.Transports.DepartureTransport.JW_ETD.AddMonths(1) <= eTD;
		}

		protected TBusinessObject[] GetConsols(BusinessObjectFactory factory, ZQuery filter)
		{
			return factory.Load<TBusinessObject>(filter);
		}
	}
}
