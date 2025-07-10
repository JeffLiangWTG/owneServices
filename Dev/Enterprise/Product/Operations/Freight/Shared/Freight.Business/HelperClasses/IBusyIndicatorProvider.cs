using System;

namespace Enterprise.Freight.Business
{
	public interface IBusyIndicatorProvider
	{
		IDisposable NewBusyIndicator();
	}
}
