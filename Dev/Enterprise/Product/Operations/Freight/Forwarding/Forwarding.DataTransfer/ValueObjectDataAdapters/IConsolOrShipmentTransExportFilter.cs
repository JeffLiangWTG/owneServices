using System;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public interface IConsolOrShipmentTransExportFilter
	{
		ZQuery Filter { get; }
		Type BusinessObjectType { get; }
	}
}
