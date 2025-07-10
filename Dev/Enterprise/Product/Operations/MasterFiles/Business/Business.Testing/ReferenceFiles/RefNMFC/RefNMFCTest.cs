using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefNMFC))]
	sealed class RefNMFCTest : EnterpriseBusinessObjectTestCase
	{
		#region Properties

		public void TestFN_Code()
		{
			Assert(NMFC.FN_CodeInfo.ReadOnly);
		}

		public void TestFN_Class()
		{
			NMFC.FN_ItemNo = "112";
			NMFC.FN_Class = "345";
			AssertEquals("345", NMFC.FN_Class);
			AssertEquals("112|345", NMFC.FN_Code);
		}

		public void TestFN_ItemNo()
		{
			NMFC.FN_ItemNo = "112";
			NMFC.FN_Class = "345";
			AssertEquals("112", NMFC.FN_ItemNo);
			AssertEquals("112|345", NMFC.FN_Code);
		}

		public void TestHumanReadableNameCore()
		{
			NMFC.FN_Code = "Test";
			NMFC.FN_Description = "Testing, One, Two, Three";

			AssertEquals("NMFC - Test - Testing, One, Two, Three", NMFC.HumanReadableName);
		}

		#endregion

		#region Implementation
		BusinessObjectFactory TestFactory;
		RefNMFC NMFC;

		protected override void SetUp()
		{
			base.SetUp();
			TestFactory = new BusinessObjectFactory();
			NMFC = TestFactory.New<RefNMFC>();
		}
		#endregion
	}
}
