using Enterprise.TransportConsignment.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.TransportConsignment.GUI.Testing
{
	class DtbConsignmentRunSheetFormForTest : DtbConsignmentRunSheetForm
	{
		public DtbConsignmentRunSheetFormForTest(DtbConsignmentRunSheet runSheet)
			: base(runSheet)
		{
		}

		public new ZGrid InstructionsGrid { get { return base.InstructionsGrid; } }
	}
}
