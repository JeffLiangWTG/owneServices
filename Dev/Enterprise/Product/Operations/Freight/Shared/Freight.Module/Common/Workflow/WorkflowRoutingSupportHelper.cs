using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Module
{
	public static class WorkflowRoutingSupportHelper
	{
		#region Transport Linked Events

		public static bool IsTransportLinkedEvent(ZString @event)
		{
			return @event.IsEmpty ||
				@event == Events.Departure.Code ||
				@event == Events.Arrival.Code ||
				@event == Events.GateOut.Code ||
				@event == Events.GateIn.Code;
		}

		#endregion

		#region Transport Query

		public static void AddTransportSubQuery(ZDBOnlySubQuery resultSubQuery, ZString origin, ZString destination)
		{
			if (!origin.IsEmpty || !destination.IsEmpty)
			{
				ZDBOnlyQuery mainQuery = new ZDBOnlyQuery(typeof(ProcessTask));

				ZDBOnlySubQuery unlinkedQuery = new ZDBOnlySubQuery(typeof(Transport), ProcessTasksSchema.P9_ReferencedID);
				unlinkedQuery.AddToFilter(JobConsolTransportSchema.JW_JX, SQLComparisonOperator.Equal, null);
				if (!origin.IsEmpty)
				{
					unlinkedQuery.AddToFilter(JobConsolTransportSchema.JW_RL_NKLoadPort, origin);
				}

				if (!destination.IsEmpty)
				{
					unlinkedQuery.AddToFilter(JobConsolTransportSchema.JW_RL_NKDiscPort, destination);
				}

				mainQuery.AddSubQuery(unlinkedQuery, JoinCondition.Or);

				ZDBOnlySubQuery linkedQuery = new ZDBOnlySubQuery(typeof(Transport), ProcessTasksSchema.P9_ReferencedID);
				ZDBOnlySubQuery sailingSubQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobConsolTransportSchema.JW_JX);

				if (!origin.IsEmpty)
				{
					ZDBOnlySubQuery originSubQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
					originSubQuery.AddToFilter(JobVoyOriginSchema.JA_RL_NKPortOfLoading, origin);
					sailingSubQuery.AddSubQuery(originSubQuery, JoinCondition.And);
				}

				if (!destination.IsEmpty)
				{
					ZDBOnlySubQuery destSubQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobSailingSchema.JX_JB);
					destSubQuery.AddToFilter(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, destination);
					sailingSubQuery.AddSubQuery(destSubQuery, JoinCondition.And);
				}

				linkedQuery.AddSubQuery(sailingSubQuery, JoinCondition.And);
				mainQuery.AddSubQuery(linkedQuery, JoinCondition.Or);

				resultSubQuery.AddToFilter(mainQuery, JoinCondition.And);
			}
		}

		#endregion
	}
}
