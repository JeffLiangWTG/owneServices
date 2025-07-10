using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(ConsolidatedDeclarationLayoutBuilder<ConsolidatedDeclaration>))]
	sealed class ConsolidatedDeclarationLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ConsolidatedDeclarationLayoutBuilder<ConsolidatedDeclaration>, ConsolidatedDeclaration, ConsolidatedDeclarationControlBag>
	{
		protected override ConsolidatedDeclarationLayoutBuilder<ConsolidatedDeclaration> GetColumnLayoutBuilderForTesting()
		{
			return new ConsolidatedDeclarationLayoutBuilder<ConsolidatedDeclaration>();
		}

		protected override int ExpectedMaxColumns => 3;

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
