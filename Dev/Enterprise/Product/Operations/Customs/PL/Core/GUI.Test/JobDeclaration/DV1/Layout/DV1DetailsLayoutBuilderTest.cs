using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(DV1DetailsLayoutBuilder<JobDeclaration>))]
class DV1DetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<DV1DetailsLayoutBuilder<JobDeclaration>, JobDeclaration, DV1DetailsControlBag>
{
	protected override DV1DetailsLayoutBuilder<JobDeclaration> GetColumnLayoutBuilderForTesting() => new DV1DetailsLayoutBuilder<JobDeclaration>();

	protected override int ExpectedMaxColumns => 3;

	protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

	protected override bool ExpectedNarrowColumnForMediumControls => true;
}
