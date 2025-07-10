using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentProcessHandlingInfo : ProcessHandlingInfo
	{
		public ForwardingShipmentProcessHandlingInfo(ForwardingShipment shipment)
			: base(shipment)
		{
			this.shipment = shipment;
		}
		readonly ForwardingShipment shipment;

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			var sqlQuery = shipment.IsCoLoadMaster || shipment.IsBlindCoLoadMaster || shipment.IsAssemblyMaster
				? (NoResString)"EXEC GetCascadingProcessTasksForMasterShipment @ShipmentPK, @EventCode" // T-SQL query
				: "SELECT * FROM dbo.GetCascadingProcessTasksForShipment(@ShipmentPK, @EventCode)"; // T-SQL query

			var sqlParams = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@ShipmentPK", shipment.PK, JobShipmentSchema.PK),
				ZSqlParameter.New("@EventCode", logBeingAdded.SL_SE_NKEvent, ProcessTasksSchema.P9_SE_NKMilestoneEvent)
			};

			return GetCascadingTargets(shipment.Factory, sqlQuery, sqlParams);
		}

		internal static IEnumerable<CascadingLink> GetCascadingTargets(BusinessObjectFactory factory, string sqlQuery, ZSqlParameterCollection sqlParams)
		{
			var queryResult = new DynamicBusinessObjectCollection(factory);
			queryResult.Load(sqlQuery, sqlParams);

			var targets = queryResult.Select(result => new
			{
				JobType = (ZString)result["JobType"],
				JobPK = (ZGuid)result["JobPK"],
				ProcessTaskPK = (ZGuid)result["ProcessTaskPK"]
			}).ToList();

			var shipmentsPKs = targets.Where(t => t.JobType == "JS").Select(t => t.JobPK).Distinct();
			var bookingLinesPKs = targets.Where(t => t.JobType == "DL").Select(t => t.JobPK).Distinct();
			var ordersPKs = targets.Where(t => t.JobType == "JD").Select(t => t.JobPK).Distinct();
			var containersPKs = targets.Where(t => t.JobType == "JC").Select(t => t.JobPK).Distinct();
			var processTasksPKs = targets.Select(t => t.ProcessTaskPK);

			var shipments = factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.PK, shipmentsPKs)).Cast<BusinessObject>();
			var orders = factory.Load<Order>(new ZQuery(JobOrderHeaderSchema.PK, ordersPKs)).Cast<BusinessObject>();
			var containers = factory.Load<ForwardingContainer>(new ZQuery(JobContainerSchema.PK, containersPKs)).Cast<BusinessObject>();
			var processTasks = factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.PK, processTasksPKs));

			var jobs = shipments.Concat(orders).Concat(containers);
			var links = jobs.Select(job => new CascadingLink
			{
				Parent = job as IStmALogParent,
				Triggers = processTasks.Where(task => task.P9_ParentID == job.PK).ToArray()
			});

			return links.ToList();
		}

		protected override IEnumerable<PropagationLink> PopulatePropagationTargets()
		{
			if (shipment.IsDeleted)
			{
				yield break;
			}

			if (shipment.CoLoadMasterShipment != null && shipment.CoLoadMasterShipment != shipment && !shipment.CoLoadMasterShipment.IsBuyersConsolLead)
			{
				yield return new PropagationLink(shipment.CoLoadMasterShipment, shipment.CoLoadMasterShipment.CoLoadShipments, "Shipments");
			}

			for (int i = 0; i < shipment.Consols.Count; i++)
			{
				var consol = shipment.Consols[i];
				yield return new PropagationLink(consol, consol.Shipments, "Shipments");
			}
		}

		protected override bool IsEventExcludedFromCascadingOrPropagation(ZString eventCode)
		{
			switch (eventCode)
			{
				case AutoEvents.PackingCompletedCode:
				case AutoEvents.UnpackingCompletedCode:
				case AutoEvents.CalculateDeliveryDateWithExceptionsRequestedCode:
					return true;
				default:
					return base.IsEventExcludedFromCascadingOrPropagation(eventCode);
			}
		}
	}
}
