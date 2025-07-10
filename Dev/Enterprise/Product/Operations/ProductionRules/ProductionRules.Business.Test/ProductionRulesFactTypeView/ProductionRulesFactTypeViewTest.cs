namespace Enterprise.ProductionRules.Business.Testing
{
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(ProductionRulesFactTypeView))]
	class ProductionRulesFactTypeViewTest : EnterpriseBusinessObjectTestCase
	{
		protected override bool IsDeleteSupported() => false;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => FactType;
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => FactType;
		ProductionRulesFactTypeView FactType => Factory.LoadTop1<ProductionRulesFactTypeView>(new ZQuery());
	}
}
