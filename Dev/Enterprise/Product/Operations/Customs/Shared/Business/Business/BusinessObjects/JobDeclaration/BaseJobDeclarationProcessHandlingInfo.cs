using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
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
	using Enterprise.ZArchitecture.Schema;

	public class BaseJobDeclarationProcessHandlingInfo : ProcessHandlingInfo
	{
		public BaseJobDeclarationProcessHandlingInfo(BaseJobDeclaration declaration)
			: base(declaration)
		{
			this.declaration = declaration;
		}
		readonly BaseJobDeclaration declaration;

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			var query = (NoResString)"EXEC GetCascadingProcessTasksForDeclaration @DeclarationPK, @EventCode";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add(ZSqlParameter.New("@DeclarationPK", declaration.PK, JobDeclarationSchema.PK));
			sqlParams.Add(ZSqlParameter.New("@EventCode", logBeingAdded.SL_SE_NKEvent, ProcessTasksSchema.P9_SE_NKMilestoneEvent));

			return GetCascadingTargets(declaration.Factory, query, sqlParams);
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

			var ordersPKs = targets.Where(t => t.JobType == "JD").Select(t => t.JobPK).Distinct();
			var processTasksPKs = targets.Select(t => t.ProcessTaskPK);

			var orders = factory.Load<Order>(new ZQuery(JobOrderHeaderSchema.PK, ordersPKs)).Cast<BusinessObject>();
			var processTasks = factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.PK, processTasksPKs));

			var links = orders.Select(job => new CascadingLink
			{
				Parent = job as IStmALogParent,
				Triggers = processTasks.Where(task => task.P9_ParentID == job.PK).ToArray()
			});

			return links.ToList();
		}
	}
}
