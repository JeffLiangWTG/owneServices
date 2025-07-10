using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderProcessHandlingInfo : ProcessHandlingInfo
	{
		public OrderProcessHandlingInfo(Order order) : base(order)
		{
			this.order = order;
		}
		readonly Order order;

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			var sqlQuery = @"
SELECT
	JobPK = JO_PK,
	ProcessTaskPK = P9_PK
FROM
	dbo.JobOrderHeader
	JOIN dbo.JobOrderLine ON JD_PK = JO_JD
	JOIN dbo.ProcessTasks ON P9_ParentID = JO_PK
WHERE
	JD_PK = @orderPK
	AND P9_RespondToCascadedEvents = 1
";
			var sqlParams = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@orderPK", order.PK, JobOrderHeaderSchema.PK),
			};
			return GetCascadingTargets(order.Factory, sqlQuery, sqlParams);
		}

		IEnumerable<CascadingLink> GetCascadingTargets(BusinessObjectFactory factory, string sqlQuery, ZSqlParameterCollection sqlParams)
		{
			var queryResult = new DynamicBusinessObjectCollection(factory);
			queryResult.Load(sqlQuery, sqlParams);

			var targets = new List<CascadingLink>();

			var orderLineGroups = queryResult.GroupBy(re => (ZGuid)re["JobPK"]);
			foreach (var group in orderLineGroups)
			{
				var orderLine = factory.Load<OrderLine>(group.Key);

				var processTasksPKs = group
					.Select(re => (ZGuid)re["ProcessTaskPK"]).Distinct();
				var processTasks = factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.PK, processTasksPKs))
					.Cast<IBaseTrigger>().ToArray();

				targets.Add(new CascadingLink
				{
					Parent = orderLine,
					Triggers = processTasks
				});
			}

			return targets.ToArray();
		}
	}
}
