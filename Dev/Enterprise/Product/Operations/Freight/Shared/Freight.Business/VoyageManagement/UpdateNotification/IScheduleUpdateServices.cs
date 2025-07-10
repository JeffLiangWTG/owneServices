using System;

namespace Enterprise.Freight.Business
{
	public interface IScheduleUpdateServices
	{
		Type ParentConsolType { get; }
		IScheduleUpdateQueryProvider QueryProvider { get; }
	}
}
