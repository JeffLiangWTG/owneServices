using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.Module.Testing;

sealed class ConsolidatedDeclarationControllerBaseOnlyTest : TestCaseWithFactory
{
	public void TestZControllerInternalsGetForm()
	{
		var mock = new Mock<ConsolidatedDeclarationController>() { CallBase = true };
		mock.Protected().Setup<IZForm>("CreateEditForm", ItExpr.IsAny<IBusiness>())
				.Returns<IBusiness>(x => new ConsolidatedDeclarationForm((ConsolidatedDeclaration)x, null));
		var provider = (ZControllerInternals)mock.Object;
		var entity = Factory.New<ConsolidatedDeclaration>();
		using var form = provider.GetForm(entity);
		AssertType<ConsolidatedDeclarationForm>(form);
	}
}
