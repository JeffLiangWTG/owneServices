using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(Trader))]
	class TraderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestE2_AddressType()
		{
			var trader = Factory.New<Trader>();
			AssertHasCustomAttribute<ListAttribute>(trader.GetType(), "E2_AddressType", false, attr => attr.ListDataSourceMember == "Lookups.TraderTypeList");
		}

		public void TestOrganisationPK()
		{
			var trader = Factory.New<Trader>();
			AssertHasCustomAttribute<ListAttribute>(trader.GetType(), "OrganisationPK", false, attr => attr.ListDataSourceMember == "Lookups.OrgHeader_List");
		}

		public void TestDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var trader = Factory.New<Trader>();
			trader.E2_ParentID = declaration.PK;
			AssertSame(declaration, trader.Declaration);
		}

		public void TestLookups()
		{
			var trader = Factory.New<Trader>();
			AssertType<TraderLookups>(trader.Lookups);
		}

		public void TestValidation()
		{
			var trader = Factory.New<Trader>();
			AssertType<TraderValidation>(trader.Validation);
		}
	}
}
