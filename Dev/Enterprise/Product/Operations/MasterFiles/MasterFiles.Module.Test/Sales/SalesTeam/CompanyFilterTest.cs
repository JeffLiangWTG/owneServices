using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(SalesTeamFilterBusinessObject.CompanyFilter))]
	sealed class CompanyFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SalesTeamFilterBusinessObject.CompanyFilter("moo", new GlbCompanyCollection(Factory));
		}
	}
}
