using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.PL.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(PreviousDocumentsFieldsLayoutBuilder))]
sealed class PreviousDocumentsFieldsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<PreviousDocumentsFieldsLayoutBuilder, PreviousDocument, PreviousDocumentsFieldsControlBag>
{
	protected override PreviousDocumentsFieldsLayoutBuilder GetColumnLayoutBuilderForTesting()
	{
		return new PreviousDocumentsFieldsLayoutBuilder();
	}

	protected override int ExpectedMaxColumns => 2;
}
