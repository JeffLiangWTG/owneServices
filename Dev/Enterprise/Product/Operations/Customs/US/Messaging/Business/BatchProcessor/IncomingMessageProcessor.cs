using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Messaging.Business
{
	public abstract class IncomingMessageProcessor : BaseMessageProcessor
	{
		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			return new List<ApplicationTypeMessageProcessor>();
		}

		protected override EDIMessageOrder MessageOrder => EDIMessageOrder.CreateTime;

		protected override ZQuery ValidBranchesForMessageFilter
		{
			get
			{
				ZQuery result;
				if (!BranchesForMessageFilterList.TryGetValue(GlbCompany.CurrentCompany.PK, out result))
				{
					result = GetNewQueryForCurrentCompanyBranches();
					BranchesForMessageFilterList.Add(GlbCompany.CurrentCompany.PK, result);
				}
				return result;
			}
		}

		protected virtual ZQuery GetNewQueryForCurrentCompanyBranches()
		{
			var branches = SharedFactory.Load<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK));
			return new ZQuery(EDIMessageSchema.EM_GB, branches.Select(x => x.PK));
		}

		Dictionary<ZGuid, ZQuery> BranchesForMessageFilterList
		{
			get { return branchesForMessageFilterList ?? (branchesForMessageFilterList = new Dictionary<ZGuid, ZQuery>()); }
		}
		Dictionary<ZGuid, ZQuery> branchesForMessageFilterList;
	}
}
