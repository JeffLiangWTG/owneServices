using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business
{
	[TestedType(typeof(ViewUNDGAttribute.Loader))]
	sealed class ViewUNDGAttributeLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new ViewUNDGAttribute.Loader(Factory);
		}
	}
}
