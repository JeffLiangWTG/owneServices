using System;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class MathExtensionHelper
	{
		public static decimal RoundUp(decimal value, sbyte digits)
		{
			if (digits == 0)
			{
				return value >= 0 ? decimal.Ceiling(value) : decimal.Floor(value);
			}

			var multiple = Convert.ToDecimal(Math.Pow(10, digits));
			return (value >= 0 ? decimal.Ceiling(value * multiple) : decimal.Floor(value * multiple)) / multiple;
		}
	}
}
