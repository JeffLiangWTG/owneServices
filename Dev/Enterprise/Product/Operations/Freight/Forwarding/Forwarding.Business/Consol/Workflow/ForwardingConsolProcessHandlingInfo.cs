using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingConsolProcessHandlingInfo : ProcessHandlingInfo
	{
		public ForwardingConsolProcessHandlingInfo(ForwardingConsol consol)
			: base(consol)
		{
			this.consol = consol;
		}
		readonly ForwardingConsol consol;

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			var sqlQuery = "EXEC GetCascadingProcessTasksForConsol @ConsolPK, @EventCode"; // T-SQL query

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add(ZSqlParameter.New("@ConsolPK", consol.PK, JobConsolSchema.PK));
			sqlParams.Add(ZSqlParameter.New("@EventCode", logBeingAdded.SL_SE_NKEvent, ProcessTasksSchema.P9_SE_NKMilestoneEvent));

			return ForwardingShipmentProcessHandlingInfo.GetCascadingTargets(consol.Factory, sqlQuery, sqlParams);
		}

		protected override bool IsEventLogApplicableForCascading(IStmALog logBeingAdded)
		{
			switch (logBeingAdded.SL_SE_NKEvent)
			{
				case Events.FreightLoadedCode:
				case Events.FreightUnloadedCode:
				case Events.ReceivedCode:
					return !logBeingAdded.Parameters.ContainsKey(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Partial);

				default:
					return true;
			}
		}

		protected override bool IsEventLogApplicableForPropagation(IStmALog logBeingAdded)
		{
			if (logBeingAdded.SL_Reference.StartsWith((NoResString)"Propagated: All Shipments"))
			{
				return false;
			}

			return logBeingAdded.SL_SE_NKEvent == Events.CalculateDeliveryDateWithExceptionsRequested.Code
				|| logBeingAdded.SL_SE_NKEvent == Events.ExceptionRaised.Code;
		}

		protected override IEnumerable<PropagationLink> PopulatePropagationTargets()
		{
			if (consol.IsDeleted)
			{
				yield break;
			}

			for (int i = 0; i < consol.Shipments.Count; i++)
			{
				var shipment = consol.Shipments[i];
				yield return new PropagationLink(shipment, shipment.Consols, "Consols");
			}
		}
	}
}
