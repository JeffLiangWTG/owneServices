using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PSCReasonCusCodeData.Loader))]
	sealed class PSCReasonCusCodeDataLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest() => new PSCReasonCusCodeData.Loader(Factory);
	}
}
