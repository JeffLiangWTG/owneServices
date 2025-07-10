using System;
using Enterprise.Integration.Freight;

namespace Enterprise.Freight.Integration
{
	public interface IMultiDaysSelection
	{
		void Generate(Action<ICommonConsol, IJobSailing> action = null);
	}
}
