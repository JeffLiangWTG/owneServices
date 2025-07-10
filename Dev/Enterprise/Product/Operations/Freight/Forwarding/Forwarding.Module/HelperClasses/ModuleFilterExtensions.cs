using System;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Module
{
	public static class ModuleFilterExtensions
	{
		public static T With<T>(this T @this, Action<T> action)
			where T : ModuleFilter
		{
			action?.Invoke(@this);

			return @this;
		}
	}
}
