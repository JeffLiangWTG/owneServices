using Enterprise.Customs.NL.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.GUI
{
	public sealed class Phase5DepartureDetailsLayoutBuilder : EU.NCTS.GUI.DepartureDetailsLayoutBuilder<NctsHeader>
	{
		protected override int MaxColumns => 1;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(Phase5DepartureDetailsControlBag.Instance.CalCalculationMethodDropEdit, x => !(x.MovementHeader.IsSimplifiedNctsProcedure && !x.MovementHeader.IsTIRDeclaration), x => x.MovementHeader.IsSimplifiedNctsProcedureInfo, x => x.MovementHeader.BM_InBondEntryTypeInfo);
			SetVisibility(Phase5DepartureDetailsControlBag.Instance.DateLimitAndCalculationUserControl, x => x.MovementHeader.IsSimplifiedNctsProcedure && !x.MovementHeader.IsTIRDeclaration, x => x.MovementHeader.IsSimplifiedNctsProcedureInfo, x => x.MovementHeader.BM_InBondEntryTypeInfo);
		}
	}
}
