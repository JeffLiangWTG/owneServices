using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(WHSPack))]
	class WHSPackTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<WHSPack>
	{
		public void TestICodeDescription()
		{
			var pack = Factory.New<WHSPack>();
			ICodeDescription packCodeDescription = pack;
			AssertEquals(" (0)", pack.PackageReferenceAndPackageQty);
			AssertEquals("", packCodeDescription.Code);
			AssertEquals(" (0)", packCodeDescription.Description);
			AssertEquals(pack.PK, packCodeDescription.PK);

			pack.US_PackageQty = 120;
			AssertEquals(" (120)", pack.PackageReferenceAndPackageQty);
			AssertEquals("", packCodeDescription.Code);
			AssertEquals(" (120)", packCodeDescription.Description);

			pack.US_PackageReference = "BOX 1";
			AssertEquals("BOX 1 (120)", pack.PackageReferenceAndPackageQty);
			AssertEquals("BOX 1", packCodeDescription.Code);
			AssertEquals("BOX 1 (120)", packCodeDescription.Description);
		}

		public void TestSetDefaultValues()
		{
			var pack = Factory.New<WHSPack>();
			AssertEquals(CusAddInfoTypeAttribute.Codes.USWHSPack, pack.B7_Type);
			AssertEquals(JobDeclarationSchema.Constants.Prefix, pack.B7_ParentTableCode);
		}

		public void TestParent()
		{
			var pack = Factory.New<WHSPack>();
			pack.B7_ParentID = Declaration.PK;
			pack.B7_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			AssertEquals(Declaration, pack.Parent);
		}

		public void TestClone()
		{
			var whsPack = Declaration.WHSPacks.AddNew();
			whsPack.US_PackageQty = 10;
			var clonedPack = (WHSPack)whsPack.Clone();
			AssertEquals(10, clonedPack.US_PackageQty);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Declaration.WHSPacks.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var whsPack = declaration.WHSPacks.AddNew();
			whsPack.US_PackageQty = 1;
			return whsPack;
		}

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;
	}
}
