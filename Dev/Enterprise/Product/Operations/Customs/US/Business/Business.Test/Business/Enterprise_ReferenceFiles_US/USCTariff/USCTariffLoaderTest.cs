using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCTariff.Loader))]
	public class USCTariffLoaderTest : ReferenceFileLoaderTestCase<USCTariff>
	{
		protected override ZString Code
		{
			get { return "123456780"; }
		}

		protected override ZString InvalidCode
		{
			get { return "letter"; }
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new USCTariff.Loader(Factory);
		}
	}
}
