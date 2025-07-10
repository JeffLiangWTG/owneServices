using System;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentsOnConsolLimitHelperTest : CollectionLimitHelperForPotentialHVLVTest<CommonConsol>
	{
		protected override IDisposable SetLimit(int limit)
		{
			return FreightDataRegistry.Instance.ShipmentsPerConsolLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, limit);
		}

		protected override IDisposable SetLimitIntroductionTime(DateTime time)
		{
			return FreightDataRegistry.Instance.ShipmentsPerConsolLimitIntroductionTimeUTC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, time);
		}

		protected override CommonConsol GetNewParentBizO()
		{
			return Factory.New<CommonConsol>();
		}

		protected override CollectionLimitHelperForPotentialHVLV GetNewHelper(CommonConsol consol)
		{
			return new ShipmentsOnConsolLimitHelper(consol);
		}

		protected override void AddNewCollectionElement(CommonConsol consol)
		{
			consol.Shipments.AddNew();
		}

		protected override string NotificationForLimitOfFour
		{
			get
			{
				return @"The number of Shipments on a Consol is limited for performance and database management reasons to 4 Shipments. Above 2 Shipments you will receive this message for every additional Shipment added. For XML imports the system will fail the import if this limit is exceeded.

If your company needs larger numbers of Shipments WiseTech Global provides an alternative method of operation that allows for a very large number of Shipments on a Master House Shipment (we call this the HVLV system or High Volume Low Value Shipment system). If you need these higher volumes (as much as 20,000 Shipments on a Manifest) contact your account manager to discuss.";
			}
		}
	}
}
