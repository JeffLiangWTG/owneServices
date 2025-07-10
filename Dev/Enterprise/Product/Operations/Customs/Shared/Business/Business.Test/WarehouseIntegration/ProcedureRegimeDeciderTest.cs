using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.Business.Testing
{
	public class ProcedureRegimeDeciderTest : TestCaseWithFactory
	{
		public void TestIsOutOfRegime()
		{
			var decider = new ProcedureRegimeDecider();
			Assert(!decider.IsOutOfRegime(procedure));

			procedure.ZZ6_OutOfWarehouse = "Y";
			Assert(decider.IsOutOfRegime(procedure));

			procedure.ZZ6_OutOfWarehouse = "N";
			procedure.ZZ6_OutofOutwardProcessing = "Y";
			Assert(decider.IsOutOfRegime(procedure));

			procedure.ZZ6_OutofOutwardProcessing = "N";
			procedure.ZZ6_OutOfInwardProcessing = "Y";
			Assert(decider.IsOutOfRegime(procedure));

			procedure.ZZ6_OutOfInwardProcessing = "N";
			Assert(!decider.IsOutOfRegime(procedure));
		}

		public void TestIsIntoRegime()
		{
			var decider = new ProcedureRegimeDecider();
			Assert(!decider.IsIntoRegime(procedure));

			procedure.ZZ6_IntoWarehouse = "Y";
			Assert(decider.IsIntoRegime(procedure));

			procedure.ZZ6_IntoWarehouse = "N";
			procedure.ZZ6_IntoInwardProcessing = "Y";
			Assert(decider.IsIntoRegime(procedure));

			procedure.ZZ6_IntoInwardProcessing = "N";
			procedure.ZZ6_IntoOutwardProcessing = "Y";
			Assert(decider.IsIntoRegime(procedure));

			procedure.ZZ6_IntoOutwardProcessing = "N";
			Assert(!decider.IsIntoRegime(procedure));
		}

		protected override void SetUp()
		{
			base.SetUp();
			procedure = Factory.New<RefCusProcedure>();
		}
		RefCusProcedure procedure;
	}
}
