using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CommonMiscOptionsLayoutBuilder<BaseJobDeclaration>))]
	sealed class CommonMiscOptionsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<CommonMiscOptionsLayoutBuilder<BaseJobDeclaration>, BaseJobDeclaration, CommonMiscOptionsControlBag>
	{
		public void TestPaidByDropEditVisibility()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;

			var layout = ((IPanelLayoutProvider)new CommonMiscOptionsLayouts()).Layout;
			CombineAssertions(() =>
			{
				AssertEquals("Interfaced", true, layout.IsVisible(CommonMiscOptionsControlBag.Instance.PaidByDropEdit, declaration));
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				AssertEquals("Built-in", false, layout.IsVisible(CommonMiscOptionsControlBag.Instance.PaidByDropEdit, declaration));
			});
		}

		protected override int ExpectedMaxColumns => 1;

		protected override CommonMiscOptionsLayoutBuilder<BaseJobDeclaration> GetColumnLayoutBuilderForTesting() => new CommonMiscOptionsLayoutBuilder<BaseJobDeclaration>();
	}
}
