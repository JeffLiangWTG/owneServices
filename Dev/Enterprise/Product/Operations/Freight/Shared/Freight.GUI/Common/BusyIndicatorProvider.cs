using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public class BusyIndicatorProvider : IBusyIndicatorProvider
	{
		public IDisposable NewBusyIndicator()
		{
			return new ZWaitCursorChanger();
		}

		public static void Register(BusinessObjectFactory factory)
		{
			if (factory != null)
			{
				factory.SetValue<IBusyIndicatorProvider, BusyIndicatorProvider>();
			}
		}

		public static void Register(ZForm form)
		{
			if (form != null && form.BusinessEntity != null)
			{
				Register(form.BusinessEntity.Factory);
			}
		}
	}
}
