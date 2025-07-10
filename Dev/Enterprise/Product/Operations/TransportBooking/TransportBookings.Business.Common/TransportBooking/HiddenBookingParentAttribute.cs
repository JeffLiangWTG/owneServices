using System;

namespace Enterprise.TransportBookings.Shared
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class HiddenBookingParentAttribute : Attribute
	{
	}
}
