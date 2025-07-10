using CargoWise.Types;
using Enterprise.Customs.NL.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.GUI.Testing;

[TestedType(typeof(Phase5DepartureDetailsLayoutBuilder))]
sealed class Phase5DepartureDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<Phase5DepartureDetailsLayoutBuilder, NctsHeader, EU.NCTS.GUI.DepartureDetailsControlBag>
{
	protected override int ExpectedMaxColumns => 1;

	protected override Phase5DepartureDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new Phase5DepartureDetailsLayoutBuilder();

	public void TestDateLimitDateEditVisibility()
	{
		CombineAssertions(() =>
		{
			var layout = ((IPanelLayoutProvider)new Phase5DepartureDetailsLayout()).Layout;
			movementHeader.IsSimplifiedNctsProcedure = ZBool.False;
			AssertEquals("IsSimplifiedNctsProcedure: false, IsTIRDeclaration: false", false, layout.IsVisible(Phase5DepartureDetailsControlBag.Instance.DateLimitAndCalculationUserControl, nctsHeader));

			movementHeader.IsSimplifiedNctsProcedure = ZBool.True;
			AssertEquals("IsSimplifiedNctsProcedure: true, IsTIRDeclaration: false", true, layout.IsVisible(Phase5DepartureDetailsControlBag.Instance.DateLimitAndCalculationUserControl, nctsHeader));

			movementHeader.BM_InBondEntryType = EU.NCTS.Business.NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
			AssertEquals("IsSimplifiedNctsProcedure: true, IsTIRDeclaration: true", false, layout.IsVisible(Phase5DepartureDetailsControlBag.Instance.DateLimitAndCalculationUserControl, nctsHeader));
			AssertSequencesEqual("Visibility dependencies", new[] { movementHeader.IsSimplifiedNctsProcedureInfo, movementHeader.BM_InBondEntryTypeInfo }, layout.GetVisibilityDependencies(Phase5DepartureDetailsControlBag.Instance.DateLimitAndCalculationUserControl, nctsHeader));
		});
	}

	public void TestCalCalculationMethodDropEditVisibility()
	{
		CombineAssertions(() =>
		{
			var layout = ((IPanelLayoutProvider)new Phase5DepartureDetailsLayout()).Layout;
			movementHeader.IsSimplifiedNctsProcedure = ZBool.False;
			AssertEquals("IsSimplifiedNctsProcedure: false, IsTIRDeclaration: true", true, layout.IsVisible(Phase5DepartureDetailsControlBag.Instance.CalCalculationMethodDropEdit, nctsHeader));

			movementHeader.IsSimplifiedNctsProcedure = ZBool.True;
			AssertEquals("IsSimplifiedNctsProcedure: true, IsTIRDeclaration: false", false, layout.IsVisible(Phase5DepartureDetailsControlBag.Instance.CalCalculationMethodDropEdit, nctsHeader));

			movementHeader.BM_InBondEntryType = EU.NCTS.Business.NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
			AssertEquals("IsSimplifiedNctsProcedure: true, IsTIRDeclaration: true", true, layout.IsVisible(Phase5DepartureDetailsControlBag.Instance.CalCalculationMethodDropEdit, nctsHeader));
			AssertSequencesEqual("Visibility dependencies", new[] { movementHeader.IsSimplifiedNctsProcedureInfo, movementHeader.BM_InBondEntryTypeInfo }, layout.GetVisibilityDependencies(Phase5DepartureDetailsControlBag.Instance.CalCalculationMethodDropEdit, nctsHeader));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		movementHeader = nctsHeader.MovementHeader;
	}
	NctsHeader nctsHeader;
	NctsDepartureMovementHeader movementHeader;
}
