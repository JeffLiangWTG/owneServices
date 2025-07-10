using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.HRM.Common;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.HRM.Testing
{
	[TestedType(typeof(ReviewProposalEntitlement))]
	class ReviewProposalEntitlementTest : EnterpriseBusinessObjectTestCase
	{
		protected override DbConnection TestConnection => elevatedConnection ?? (elevatedConnection = Db.NewAdminConnection());
		DbConnection elevatedConnection;

		protected override BusinessObjectFactory NewFactory() => new BusinessObjectFactory(TestConnection);
	}
}
