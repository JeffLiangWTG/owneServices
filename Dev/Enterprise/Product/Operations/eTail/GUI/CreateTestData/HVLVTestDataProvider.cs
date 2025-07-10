using System.ComponentModel;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eTail.GUI;

public class HVLVTestDataProvider : IHVLVTestDataProvider
{
	public HVLVTestDataProvider()
	{
		isTestEnv = ObjectFactory.Get<IProductRegistration>().Key.DatabaseType == DatabaseTypes.Codes.Test
					&& GlbStaff.CurrentUser.IsSupportUser;
	}

	readonly bool isTestEnv;

	public IComponent GetCreateTestHVLVShipmentMenuGroupIfNecessary(IBusiness parentBizo)
	{
		if (isTestEnv && parentBizo != null && parentBizo is ForwardingConsol consol)
		{
			return new CreateTestHVLVShipmentMenuGroup(consol);
		}

		return null;
	}

	public IComponent GetCreateTestHVLShipmentMenuItemIfNecessary(IBusiness parentBizo)
	{
		if (isTestEnv && parentBizo != null &&
			(parentBizo is ForwardingShipment shipment && shipment.JS_ShipmentType == "HVM" ||
			 parentBizo is ForwardingConsol))
		{
			return new CreateTestHVLShipmentMenuItem(parentBizo as BusinessObject);
		}

		return null;
	}
}
