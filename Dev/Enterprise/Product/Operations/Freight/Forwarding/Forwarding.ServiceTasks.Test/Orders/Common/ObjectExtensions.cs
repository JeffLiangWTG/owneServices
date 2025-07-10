using System;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Testing.Orders;

public static class ObjectExtensions
{
	public static T With<T>(this T @this, Action<T> action)
		where T : class
	{
		action?.Invoke(@this);

		return @this;
	}
}
