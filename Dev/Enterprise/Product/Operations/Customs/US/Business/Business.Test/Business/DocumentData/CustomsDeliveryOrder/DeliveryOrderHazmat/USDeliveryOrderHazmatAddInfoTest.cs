using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USDeliveryOrderHazmatAddInfo))]
	sealed class USDeliveryOrderHazmatAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.DeliveryOrderHeaders.AddNew();
			return new USDeliveryOrderHazmatAddInfo(header.DeliveryOrderHazmats.AddNew().B7_AddInfoDataInfo);
		}
	}
}
