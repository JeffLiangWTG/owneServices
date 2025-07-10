using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(SalesTeamFilterBusinessObject.GlobalOrCurrentLoginCompanyFilter))]
	sealed class GlobalOrCurrentLoginCompanyFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SalesTeamFilterBusinessObject.GlobalOrCurrentLoginCompanyFilter("moo");
		}
	}
}
