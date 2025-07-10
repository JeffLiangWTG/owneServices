using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration;
using IBaseJobDeclaration = Enterprise.Integration.Customs.IBaseJobDeclaration;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class OrdersOnDeclarationLimitsHelperTest : CollectionLimitHelperForPotentialHVLVTest<BusinessObject>
	{
		protected override IDisposable SetLimit(int limit)
		{
			return ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().OrdersPerDeclarationLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, limit);
		}

		protected override IDisposable SetLimitIntroductionTime(DateTime time)
		{
			return ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().OrdersPerDeclarationLimitIntroductionTimeUTC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, time);
		}

		protected override BusinessObject GetNewParentBizO()
		{
			return (BusinessObject)Factory.New<IBaseJobDeclaration>();
		}

		protected override CollectionLimitHelperForPotentialHVLV GetNewHelper(BusinessObject declaration)
		{
			return new OrdersOnDeclarationLimitHelper((IBaseJobDeclaration)declaration);
		}

		protected override void AddNewCollectionElement(BusinessObject declaration)
		{
			((IAttachOrders)declaration).AttachedOrders.AddNew();
		}

		protected override string NotificationForLimitOfFour
		{
			get
			{
				return @"The number of Orders on a Declaration is limited for performance and database management reasons to 4 Orders. Above 2 Orders you will receive this message for every additional Order added. For XML imports the system will fail the import if this limit is exceeded.

If your company needs larger numbers of Shipments WiseTech Global provides an alternative method of operation that allows for a very large number of Shipments on a Master House Shipment (we call this the HVLV system or High Volume Low Value Shipment system). If you need these higher volumes (as much as 20,000 Shipments on a Manifest) contact your account manager to discuss.";
			}
		}
	}
}
