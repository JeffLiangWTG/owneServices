using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.eTail.Integration;

public interface IHVLVTestDataProvider
{
	IComponent GetCreateTestHVLVShipmentMenuGroupIfNecessary(IBusiness parentBizo);

	IComponent GetCreateTestHVLShipmentMenuItemIfNecessary(IBusiness parentBizo);
}
