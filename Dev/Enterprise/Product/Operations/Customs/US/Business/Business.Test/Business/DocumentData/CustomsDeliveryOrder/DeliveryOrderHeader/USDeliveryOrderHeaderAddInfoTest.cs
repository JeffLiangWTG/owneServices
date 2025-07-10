using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USDeliveryOrderHeaderAddInfo))]
	sealed class USDeliveryOrderHeaderAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new USDeliveryOrderHeaderAddInfo(Factory.New<DeliveryOrderHeader>().B7_AddInfoDataInfo);
	}
}
