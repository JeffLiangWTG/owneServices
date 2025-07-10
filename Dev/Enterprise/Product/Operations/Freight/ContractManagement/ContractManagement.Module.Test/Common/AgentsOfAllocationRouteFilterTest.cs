using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ContractManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Module.Testing
{
	[TestedType(typeof(AgentsOfAllocationRouteFilter))]
	public class AgentOfAllocationRouteFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AgentsOfAllocationRouteFilter("Agents", new OrgHeaderCollection(Factory), typeof(RatingContractAllocationLine), RatingContractAllocationLineSchema.Constants.PK);
		}
	}
}
