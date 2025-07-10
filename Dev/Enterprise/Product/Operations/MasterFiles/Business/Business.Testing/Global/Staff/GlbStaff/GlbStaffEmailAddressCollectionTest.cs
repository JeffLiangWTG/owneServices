using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffEmailAddressCollection))]
	sealed class GlbStaffEmailAddressCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbStaffEmailAddressCollection>
	{
		public void TestFindByEmailAddressString()
		{
			var collection = GetCollectionToTest();

			var emailaddress1 = collection.AddNew();
			emailaddress1.GSE_EmailAddress = "import@test.com";
			emailaddress1.GSE_Type = "IMP";

			var emailaddress2 = collection.AddNew();
			emailaddress2.GSE_EmailAddress = "export@test.com";
			emailaddress2.GSE_Type = "EXP";

			AssertEquals("EXP", collection.FindByEmailAddressString("export@test.com").GSE_Type);
			AssertEquals("EXP", collection.FindByEmailAddressString("ExporT@Test.com").GSE_Type);
			AssertEquals("IMP", collection.FindByEmailAddressString("import@test.com").GSE_Type);
			AssertEquals("IMP", collection.FindByEmailAddressString("Import@test.com").GSE_Type);
		}

		public void TestFindByEmailAddressType()
		{
			var collection = GetCollectionToTest();

			var emailaddress1 = collection.AddNew();
			emailaddress1.GSE_EmailAddress = "import@test.com";
			emailaddress1.GSE_Type = "IMP";

			var emailaddress2 = collection.AddNew();
			emailaddress2.GSE_EmailAddress = "export@test.com";
			emailaddress2.GSE_Type = "EXP";

			AssertEquals("export@test.com", collection.FindByEmailAddressType("EXP").GSE_EmailAddress);
			AssertEquals("import@test.com", collection.FindByEmailAddressType("IMP").GSE_EmailAddress);
		}

		protected override GlbStaffEmailAddressCollection GetCollectionToTest()
		{
			return new GlbStaffEmailAddressCollection(Factory);
		}
	}
}
