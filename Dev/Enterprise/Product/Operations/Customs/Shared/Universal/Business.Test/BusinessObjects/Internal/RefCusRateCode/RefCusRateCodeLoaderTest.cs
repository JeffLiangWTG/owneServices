using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusRateCode.Loader))]
	public class RefCusRateCodeLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new RefCusRateCode.Loader(Factory);
		}
	}
}
