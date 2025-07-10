using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseSupplementaryCode.Loader))]
	sealed class BaseSupplementaryCodeLoaderTest : LoaderTestCase
	{
		public void TestLoad()
		{
			var code = Factory.New<BaseSupplementaryCodeForTest>();
			code.CY_Order = 1;
			code.Parent = declaration;

			var reloadedCode = loader.Load(declaration, 1);
			AssertSame(code, reloadedCode);
		}

		public void TestLoadOrCreate()
		{
			var code = loader.LoadOrCreate<BaseSupplementaryCodeForTest, JobDeclarationWithSupplementaryCodeSupport>(declaration, 1);
			code.CY_Code = "CD1";
			AssertNotNull("Code must be created", code);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var declarationFromNewFactoryInstance = newFactory.Load<JobDeclarationWithSupplementaryCodeSupport>(declaration.PK);

			var newLoader = new BaseSupplementaryCode.Loader(newFactory);
			var codeViaNewFactory = newLoader.Load<BaseSupplementaryCodeForTest, JobDeclarationWithSupplementaryCodeSupport>(declarationFromNewFactoryInstance, 1);

			AssertNotNull("Re-loaded Code using new factory", codeViaNewFactory);
			AssertEquals("Code must be loaded", code.PK, codeViaNewFactory.PK);
		}

		protected override void SetUp()
		{
			base.SetUp();
			loader = new BaseSupplementaryCode.Loader(Factory);
			declaration = Factory.New<JobDeclarationWithSupplementaryCodeSupport>();
		}

		JobDeclarationWithSupplementaryCodeSupport declaration;
		BaseSupplementaryCode.Loader loader;

		protected override BusinessObject.Loader GetNewLoaderToTest() => loader;
	}
}


