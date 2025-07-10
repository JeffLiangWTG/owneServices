using System;
using CargoWise.ComponentModel;

namespace Enterprise.TransportBookings.Shared
{
	public interface IDtbBookingMenuProvider
	{
		IMenuItem ConstructMenu(Func<bool> callback = null);

		bool ShowFormsFromMainThread { get; set; }
	}
}
