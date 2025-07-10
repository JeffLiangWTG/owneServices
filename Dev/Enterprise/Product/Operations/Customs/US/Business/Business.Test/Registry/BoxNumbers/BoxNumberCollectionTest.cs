using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(BoxNumberCollection))]
	sealed class BoxNumberCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<BoxNumberCollection>
	{
		public void TestIndexer_BranchPK()
		{
			BusinessObject newCompany = (BusinessObject)Factory.New<IGlbCompany>();
			newCompany[GlbCompanySchema.GC_Name] = "Dummy Company";
			newCompany[GlbCompanySchema.GC_Code] = "Z@Z";
			newCompany[GlbCompanySchema.GC_OH_OrgProxy] = Env.CurrentCompany.OrganisationPK;

			BusinessObject branch1 = (BusinessObject)Factory.New<IGlbBranch>();
			branch1[GlbBranchSchema.GB_BranchName] = "Dummy Branch 1";
			branch1[GlbBranchSchema.GB_Code] = "Z@1";
			branch1[GlbBranchSchema.GB_GC] = newCompany.PK;
			branch1[GlbBranchSchema.GB_OH_OrgProxy] = Env.CurrentBranch.OrganisationPK;

			BusinessObject branch2 = (BusinessObject)Factory.New<IGlbBranch>();
			branch2[GlbBranchSchema.GB_BranchName] = "Dummy Branch 2";
			branch2[GlbBranchSchema.GB_Code] = "Z@2";
			branch2[GlbBranchSchema.GB_GC] = newCompany.PK;
			branch2[GlbBranchSchema.GB_OH_OrgProxy] = Env.CurrentBranch.OrganisationPK;
			Factory.Save();

			BoxNumberCollection collection = GetCollectionToTest();
			collection.CompanyPK = newCompany.PK;
			BoxNumber boxNo = collection.AddNew();
			boxNo.TransportMode = "ALL";

			AssertNotNull(collection[0]);
		}

		public void TestIsEmpty()
		{
			BoxNumberCollection collection = GetCollectionToTest();
			AssertEquals(0, collection.Count);
			AssertEquals(true, collection.IsEmpty);
			collection.AddNew();

			AssertEquals(false, collection.IsEmpty);
		}

		public void TestUpdateBranchDetails()
		{
			BusinessObject newCompany = (BusinessObject)Factory.New<IGlbCompany>();
			newCompany[GlbCompanySchema.GC_Name] = "Dummy Company";
			newCompany[GlbCompanySchema.GC_Code] = "Z@Z";
			newCompany[GlbCompanySchema.GC_OH_OrgProxy] = Env.CurrentCompany.OrganisationPK;

			BusinessObject branch1 = (BusinessObject)Factory.New<IGlbBranch>();
			branch1[GlbBranchSchema.GB_BranchName] = "Dummy Branch 1";
			branch1[GlbBranchSchema.GB_Code] = "Z@1";
			branch1[GlbBranchSchema.GB_GC] = newCompany.PK;
			branch1[GlbBranchSchema.GB_OH_OrgProxy] = Env.CurrentBranch.OrganisationPK;

			BusinessObject branch2 = (BusinessObject)Factory.New<IGlbBranch>();
			branch2[GlbBranchSchema.GB_BranchName] = "Dummy Branch 2";
			branch2[GlbBranchSchema.GB_Code] = "Z@2";
			branch2[GlbBranchSchema.GB_GC] = newCompany.PK;
			branch2[GlbBranchSchema.GB_OH_OrgProxy] = Env.CurrentBranch.OrganisationPK;
			Factory.Save();

			BoxNumberCollection collection = GetCollectionToTest();
			collection.CompanyPK = newCompany.PK;
			BoxNumber boxNo = collection.AddNew();
			//collection.UpdateBoxNoDetails();
			AssertEquals(1, collection.Count);
			//AssertEquals(true, collection.ContainsBranch(branch1.PK));
			//AssertEquals(true, collection.ContainsBranch(branch2.PK));
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override BoxNumberCollection GetCollectionToTest()
		{
			return new BoxNumberCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new BoxNumber();
		}
	}
}
