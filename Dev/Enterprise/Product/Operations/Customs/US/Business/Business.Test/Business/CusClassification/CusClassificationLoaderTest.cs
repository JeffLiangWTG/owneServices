using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusClassification.Loader))]
	sealed class CusClassificationLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest() => new CusClassification.Loader(Factory);
	}
}
