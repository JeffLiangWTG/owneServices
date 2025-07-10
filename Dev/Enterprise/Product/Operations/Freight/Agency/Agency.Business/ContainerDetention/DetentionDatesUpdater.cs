using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class DetentionDatesUpdater : IDetentionDatesUpdater
	{
		public void UpdateContainerDetentionDateFromSailing(BusinessObject businessObject)
		{
			var sailing = businessObject as JobSailing;

			if (sailing != null
				&& AgencyRegistry.Instance.UpdateEmptyReturnByWhenAvailabilityDatesChange.Value
				&& sailing.Voyage != null
				&& sailing.Voyage.IsSea
				&& sailing.Destination != null
				&& sailing.Factory != null)
			{
				var billOfLadingStatus = new string[] { ShipmentStatusList.Codes.Confirmed, ShipmentStatusList.Codes.WebFwdInstruction };

				var query = new ZQuery(JobShipmentSchema.JS_JX, sailing.PK);
				query.AddToFilter(JobShipmentSchema.JS_IsShipping, true);
				query.AddToFilter(JobShipmentSchema.JS_ShipmentStatus, billOfLadingStatus);

				var billOfLadings = sailing.Factory.Load<BillOfLading>(query);

				foreach (BillOfLading billOfLading in billOfLadings)
				{
					UpdateBillOfLadingContainers(billOfLading);
				}
			}
		}

		void UpdateBillOfLadingContainers(BillOfLading billOfLading)
		{
			var log = new NullOperationalActionLog();

			foreach (BillOfLadingContainer container in billOfLading.RealContainers)
			{
				var originalUpdateReturnByDate = container.JC_EmptyReturnedBy;
				UpdateReturnByApplicator.UpdateContainer(log, container);

				if (container.JC_EmptyReturnedBy != originalUpdateReturnByDate)
				{
					var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} 'Empty Return By' recalculated from {1} to {2}", // EDI008 Log Reference Values should be in English Only
						container.HumanReadableName,
						GetDateForLogging(originalUpdateReturnByDate),
						GetDateForLogging(container.JC_EmptyReturnedBy));

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					container.Logs.AddNew(AutoEvents.EditedARecord, message);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
		}

		string GetDateForLogging(ZDateTime date)
		{
			return date.IsValid ? date.ToShortDateString() : (NoResString)"empty"; // EDI008 Log Reference Values should be in English Only
		}
	}
}


