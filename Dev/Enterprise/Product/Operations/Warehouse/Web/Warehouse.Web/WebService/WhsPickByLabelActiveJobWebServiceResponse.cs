using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.PickByLabel;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsPickByLabelActiveJobWebServiceResponse : WebServiceResponse
	{
		public WhsPickByLabelActiveJobWebServiceResponse()
		{
		}

		public void SetWhsPickByLabelActiveJobWebServiceResponse(WhsPickByLabelJob whsPickByLabelJob)
		{
			JobPK = whsPickByLabelJob?.PK.ToGuid() ?? Guid.Empty;
			var jobLabels = whsPickByLabelJob?.Labels.Cast<WhsPickByLabelLabel>();
			Labels = jobLabels?.Select(l => new PickByLabelLabelInfo(l.Package.KP_PackageID)).ToArray();

			var dockdoorLocation = whsPickByLabelJob?.DockDoorLocation;
			DockDoorLocationString = dockdoorLocation?.WLV_LocationString;
			DockDoorLocationString_UserFriendly = dockdoorLocation?.WLV_LocationString_UserFriendly;

			if (whsPickByLabelJob != null)
			{
				var picks = jobLabels
								.Select(l => l.Package)
								.DistinctBy(p => p.KP_KJ_ParentPackageJob)
								.Select(p => p.PackageJob?.ParentJob).Cast<WhsOrder>()
								.DistinctBy(o => o.WD_WP)
								.Select(o => o.Pick)
								.ToArray();
				var pickJobWrapperForPickByLabelJob = new PickJobWrapperForPickByLabelJob(whsPickByLabelJob.Factory, JobPK, whsPickByLabelJob, picks);

				IsUsingDirectedPackingConsolidation = pickJobWrapperForPickByLabelJob.CheckIfConsolidationLocationIsAllowedForJob();

				IsPackingStationAllowed = pickJobWrapperForPickByLabelJob.CheckIfPackingStationIsAllowed();

				var assignedLocation = pickJobWrapperForPickByLabelJob.GetAssignedLocationForJob();

				PackingStationPK = assignedLocation != null && assignedLocation.LocationClass.Equals(LocationClasses.Codes.PST) ? assignedLocation.LocationPK : Guid.Empty;
				AssignedPutawayLocation = assignedLocation?.LocationString ?? string.Empty;
				AssignedPutawayLocation_UserFriendly = assignedLocation?.LocationString_UserFriendly ?? string.Empty;
				AssignedPutawayLocationClass = assignedLocation?.LocationClass ?? string.Empty;

				AllowPickDockDoorLocationOverride = string.IsNullOrEmpty(pickJobWrapperForPickByLabelJob.GetAllowPickDockDoorLocationOverride());
			}
		}

		public Guid JobPK { get; set; }
		public string DockDoorLocationString { get; set; }
		public string DockDoorLocationString_UserFriendly { get; set; }
		public bool AllowPickDockDoorLocationOverride { get; set; }
		public bool IsUsingDirectedPackingConsolidation { get; set; }
		public Guid PackingStationPK { get; set; }
		public string AssignedPutawayLocation { get; set; }
		public string AssignedPutawayLocation_UserFriendly { get; set; }
		public string AssignedPutawayLocationClass { get; set; }
		public bool IsPackingStationAllowed { get; set; }
		public PickByLabelLabelInfo[] Labels { get; set; }
	}
}
