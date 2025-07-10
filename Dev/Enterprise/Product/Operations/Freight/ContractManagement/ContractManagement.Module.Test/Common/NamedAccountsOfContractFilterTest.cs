using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ContractManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Module.Testing
{
	[TestedType(typeof(NamedAccountsOfContractFilter))]
	public class NamedAccountsOfContractFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new NamedAccountsOfContractFilter("Named Account Clients", new OrgHeaderCollection(Factory), typeof(RatingContract), RatingContractSchema.Constants.Prefix, RatingContractSchema.Constants.PK);
		}
	}
}
