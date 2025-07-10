using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USDeliveryOrderLineAddInfo))]
	sealed class USDeliveryOrderLineAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new USDeliveryOrderLineAddInfo(Factory.New<DeliveryOrderLine>().B7_AddInfoDataInfo);
		}
	}
}
