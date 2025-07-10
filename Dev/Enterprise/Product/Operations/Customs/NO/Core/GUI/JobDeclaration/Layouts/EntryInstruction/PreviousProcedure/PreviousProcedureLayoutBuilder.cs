using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

sealed class PreviousProcedureLayoutBuilder : ColumnLayoutBuilder<PreviousDocumentMaster, PreviousProcedureControlBag>
{
	public override PreviousProcedureControlBag CommonBag => PreviousProcedureControlBag.Instance;

	protected override int MaxColumns => 2;
}
