using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public class DeclarationsAndShipmentsCreatedCancelledTestCase : TestCaseWithFactory
	{
		protected override BusinessObjectFactory NewFactory()
		{
			return new DeclarationsCreatedCancelledBusinessObjectFactory();
		}
	}
}
