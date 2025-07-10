using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	class BaseJobDeclarationCollectionTest : TestCaseWithFactory
	{
		public void TestIndexer()
		{
			collection.AddNew();
			AssertNotNull("Indexer works", collection[0]);
		}

		public void TestGetBusinessObjectFromCode()
		{
			BaseJobDeclarationCollection collection = new BaseJobDeclarationCollection(Factory, "AU");
			GlbCompany otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany.SetCountry("CA");
			GlbBranch otherBranch = Factory.New<GlbBranch>();
			otherBranch.GB_RL_NKHomePort = "USLAX";
			otherBranch.GB_GC = otherCompany.PK;
			otherBranch.GB_Code = "OTH";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			BaseJobDeclaration dec1 = BaseJobDeclaration.New(Factory);
			dec1.JE_DeclarationReference = "DEC1";
			dec1.JE_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();
			AssertEquals(dec1, ((IFindBoxListProvider)collection).GetBusinessObjectFromCode(dec1.JE_DeclarationReference));
			dec1.JE_GB = otherBranch.PK;
			Factory.Save();
			AssertNull(((IFindBoxListProvider)collection).GetBusinessObjectFromCode(dec1.JE_DeclarationReference));
		}

		#region Implementation

		protected virtual string TestingCountry
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}

		BaseJobDeclarationCollection collection;
		protected override void SetUp()
		{
			base.SetUp();

			if (!string.IsNullOrEmpty(TestingCountry))
			{
				storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				GlbCompany.CurrentCompany.SetCountry(TestingCountry);
			}

			collection = new BaseJobDeclarationCollection(Factory);
		}
		protected string storedCountry;

		protected override void TearDown()
		{
			if (!string.IsNullOrEmpty(storedCountry) && storedCountry != GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}

			base.TearDown();
		}

		#endregion
	}
}
