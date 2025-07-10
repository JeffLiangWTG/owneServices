using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AutoSendMessageCusAddInfo.Loader))]
	sealed class AutoSendMessageCusAddInfoLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest() => new AutoSendMessageCusAddInfo.Loader(Factory);
	}
}
