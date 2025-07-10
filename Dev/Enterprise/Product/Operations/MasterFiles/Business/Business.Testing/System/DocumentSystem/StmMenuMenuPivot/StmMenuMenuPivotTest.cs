using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StmMenuMenuPivot))]
	sealed class StmMenuMenuPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestStmMenuMenuPivot()
		{
			StmMenuMenuPivot pivot = Factory.New<StmMenuMenuPivot>();
			Assert("SF_SU_Inward IsEmpty", pivot.SF_SU_Inward.IsEmpty);
			Assert("SF_SU_Outward IsEmpty", pivot.SF_SU_Outward.IsEmpty);
		}

		public void TestMenuName()
		{
			var pivot = Factory.New<StmMenuMenuPivot>();
			AssertNull(pivot.Outward);
			AssertEquals(ZString.Empty, pivot.MenuName);

			var outward = Factory.New<StmMenuItem>();
			outward.SU_MenuName = "Wibbly wobbly timey wimey stuff";
			pivot.SF_SU_Outward = outward.PK;

			AssertNotNull(pivot.Outward);
			AssertEquals("Wibbly wobbly timey wimey stuff", pivot.MenuName);
		}

		public void TestDocumentIndex()
		{
			var pivot = Factory.New<StmMenuMenuPivot>();
			pivot.SF_Index = 1;
			AssertEquals("Document index: 1", pivot.DocumentIndex);
		}
	}
}
