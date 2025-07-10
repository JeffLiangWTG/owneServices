using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobRequiredDocAttribCollection))]
	sealed class JobRequiredDocAttribCollectionTest : ActiveBusinessObjectCollectionTestCase<JobRequiredDocAttribCollection>
	{
		public void TestHasAttributeType()
		{
			var master = Factory.New<JobRequiredDocument>();
			var attrib1 = master.Attributes.AddNew();
			attrib1.D0_AttribName = "BOB";

			var attrib2 = master.Attributes.AddNew();
			attrib2.D0_AttribName = "WENDY";

			var attrib3 = master.Attributes.AddNew();
			attrib3.D0_AttribName = "SMITH";

			AssertEquals(true, master.Attributes.HasAttributeType("BOB"));
			AssertEquals(true, master.Attributes.HasAttributeType("WENDY"));
			AssertEquals(true, master.Attributes.HasAttributeType("SMITH"));
			AssertEquals(false, master.Attributes.HasAttributeType("JANE"));
		}

		public void TestIndexer_TypeAndValue()
		{
			var master = Factory.New<JobRequiredDocument>();
			var attrib1 = master.Attributes.AddNew();
			attrib1.D0_AttribName = "BOB";
			attrib1.D0_AttribValue = "BUILDER";

			var attrib2 = master.Attributes.AddNew();
			attrib2.D0_AttribName = "WENDY";
			attrib2.D0_AttribValue = "BUILDER";

			var attrib3 = master.Attributes.AddNew();
			attrib3.D0_AttribName = "WENDY";
			attrib3.D0_AttribValue = "DESTROYER";

			var attrib4 = master.Attributes.AddNew();
			attrib4.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CompanyCode;
			attrib4.D0_AttribValue = GlbCompany.CurrentCompany.PK.ToString();

			var attrib5 = master.Attributes.AddNew();
			attrib5.D0_AttribName = "WENDY";
			attrib5.D0_AttribValue = "";

			AssertEquals(attrib1, master.Attributes["BOB", "BUILDER"]);
			AssertNull(master.Attributes["BOB", "DESTROYER"]);
			AssertEquals(attrib2, master.Attributes["WENDY", "BUILDER"]);
			AssertEquals(attrib3, master.Attributes["WENDY", "DESTROYER"]);
			AssertEquals(attrib4, master.Attributes["COMPANY CODE", GlbCompany.CurrentCompany.GC_Code]);
			AssertEquals(attrib5, master.Attributes["WENDY", "ROLLBACK"]);
		}

		protected override JobRequiredDocAttribCollection GetCollectionToTest()
		{
			JobRequiredDocument master = Factory.New<JobRequiredDocument>();
			return new JobRequiredDocAttribCollection(master);
		}
	}
}
