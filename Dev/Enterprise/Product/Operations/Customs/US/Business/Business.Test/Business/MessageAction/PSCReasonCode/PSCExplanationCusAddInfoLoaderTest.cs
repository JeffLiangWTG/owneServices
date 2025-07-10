using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PSCExplanationCusAddInfo.Loader))]
	sealed class PSCExplanationCusAddInfoLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest() => new PSCExplanationCusAddInfo.Loader(Factory);
	}
}
