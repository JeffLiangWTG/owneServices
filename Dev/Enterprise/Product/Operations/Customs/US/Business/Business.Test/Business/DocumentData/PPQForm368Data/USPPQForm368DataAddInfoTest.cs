using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USPPQForm368DataAddInfo))]
	sealed class USPPQForm368DataAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new USPPQForm368DataAddInfo(Factory.New<PPQForm368Data>().B7_AddInfoDataInfo);
	}
}
