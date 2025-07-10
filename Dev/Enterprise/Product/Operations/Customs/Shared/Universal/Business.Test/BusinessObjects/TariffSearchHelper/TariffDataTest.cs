using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(TariffData))]
	class TariffDataTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new TariffData();
		}
	}
}
