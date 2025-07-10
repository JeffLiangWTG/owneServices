using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgDebtorGroupBankDefaultCollection))]
	sealed class OrgDebtorGroupBankDefaultCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			MasterDebtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			return new OrgDebtorGroupBankDefaultCollection(MasterDebtorGroup, GlbCompany.CurrentCompany);
		}

		protected override void SetUp()
		{
			base.SetUp();
			MasterDebtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
		}

		OrgDebtorGroup MasterDebtorGroup;

		public void TestGetCollectionWithFactory()
		{
			OrgDebtorGroupBankDefaultCollection testCollection = new OrgDebtorGroupBankDefaultCollection(Factory);
			AssertEquals("Factory in collection must be the same as parameter", Factory, testCollection.Factory);
		}

		public void TestRelationshipFilter()
		{
			GlbCompany nonCurrentCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));

			OrgDebtorGroupBankDefault bankDefaultCurrentCompany = Factory.New<OrgDebtorGroupBankDefault>();
			OrgDebtorGroupBankDefault bankDefaultNonCurrentCompany = Factory.New<OrgDebtorGroupBankDefault>();

			AccBankAccount bankAccountCurrentCompany = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount bankAccountNonCurrentCompany = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccountCurrentCompany.AB_GC = GlbCompany.CurrentCompany.PK;
			bankAccountNonCurrentCompany.AB_GC = nonCurrentCompany.PK;

			bankDefaultCurrentCompany.P6_AB = bankAccountCurrentCompany.PK;
			bankDefaultNonCurrentCompany.P6_AB = bankAccountNonCurrentCompany.PK;
			bankDefaultCurrentCompany.P6_OJ = MasterDebtorGroup.PK;
			bankDefaultNonCurrentCompany.P6_OJ = MasterDebtorGroup.PK;
			bankDefaultCurrentCompany.P6_GC = GlbCompany.CurrentCompany.PK;
			bankDefaultNonCurrentCompany.P6_GC = nonCurrentCompany.PK;

			Factory.Save();

			OrgDebtorGroupBankDefaultCollection collection = new OrgDebtorGroupBankDefaultCollection(MasterDebtorGroup, GlbCompany.CurrentCompany);
			collection.Load();

			AssertEquals("Should only be one element in the list", 1, collection.Count);

			collection = new OrgDebtorGroupBankDefaultCollection(MasterDebtorGroup, null);
			collection.Load();

			AssertEquals("Should be both elements in the list", 2, collection.Count);
		}
	}
}
