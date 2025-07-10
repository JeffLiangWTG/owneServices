using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(PreviousProcedureLayoutBuilder))]
sealed class PreviousProcedureLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<PreviousProcedureLayoutBuilder, PreviousDocumentMaster, PreviousProcedureControlBag>
{
	protected override PreviousProcedureLayoutBuilder GetColumnLayoutBuilderForTesting() => new ();

	protected override int ExpectedMaxColumns => 2;
}
