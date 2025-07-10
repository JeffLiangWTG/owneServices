using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(OrgSupplierPart.Loader))]
	sealed class OrgSupplierPartLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart));
		}
	}
}
