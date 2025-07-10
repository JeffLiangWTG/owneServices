using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Business;

namespace CargoWise.NetworkVisualisation.GUI.Services.Utils;

public static class Properties
{
	const double ToleranceForDouble = 0.00000001;

	public static bool Equivalent<T>(T a, T b) => (a, b) switch
	{
		(double x, double y) => Math.Abs(x - y) <= ToleranceForDouble,
		(System.Drawing.Color x, System.Drawing.Color y) => x.ToArgb() == y.ToArgb(),
		(ColorOffset x, ColorOffset y) => x.offset == y.offset
										&& Equivalent(x.color, y.color),
		(NodeColors x, NodeColors y) => x.angle == y.angle
										&& x.opacity == y.opacity
										&& x.colors.Count() == y.colors.Count()
										&& x.colors.Zip(y.colors).All(w => Equivalent(w.First, w.Second)),
		_ => EqualityComparer<T>.Default.Equals(a, b)
	};

	public static bool TryUpdate<T>(T newValue, T currentValue, Action<T> set)
	{
		if (Equivalent(currentValue, newValue))
		{
			return false;
		}

		set(newValue);

		return true;
	}
}
