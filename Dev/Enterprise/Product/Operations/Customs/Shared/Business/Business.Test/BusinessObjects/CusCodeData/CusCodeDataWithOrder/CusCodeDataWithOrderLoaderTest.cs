using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusCodeDataWithOrder.Loader))]
	sealed class CusCodeDataWithOrderLoaderTest : LoaderTestCase
	{
		public void TestLoad()
		{
			var code = Factory.New<CusCodeDataWithOrderForTest>();
			code.CY_Order = 1;
			code.CY_Type = "TES";
			code.Parent = classification;

			var reloadedCode = loader.Load<CusCodeDataWithOrderForTest, BaseCusClassification>(classification, 1, "TES");
			AssertSame(code, reloadedCode);
		}

		public void TestLoadOrCreate()
		{
			var code = loader.LoadOrCreate<CusCodeDataWithOrderForTest, BaseCusClassification>(classification, 1, "TES");
			AssertNotNull("Code must be created", code);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var classificationViaNewFactory = newFactory.Load<BaseCusClassification>(classification.PK);

			var newLoader = new CusCodeDataWithOrder.Loader(newFactory);
			var codeViaNewFactory = newLoader.Load<CusCodeDataWithOrderForTest, BaseCusClassification>(classificationViaNewFactory, 1, "TES");

			AssertNotNull("Re-loaded Code using new factory", codeViaNewFactory);
			AssertEquals("Code must be loaded", code.PK, codeViaNewFactory.PK);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => loader;

		protected override void SetUp()
		{
			base.SetUp();
			loader = new CusCodeDataWithOrder.Loader(Factory);
			classification = Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "TestLookup";
		}

		BaseCusClassification classification;
		CusCodeDataWithOrder.Loader loader;
	}
}
