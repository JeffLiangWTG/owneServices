using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.OnlineSailingSchedules;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class MockOnlineSailingSchedulesDataVendor : OnlineSailingSchedulesDataVendor
	{
		public new static MockOnlineSailingSchedulesDataVendor Instance
		{
			get { return instance ?? (instance = new MockOnlineSailingSchedulesDataVendor()); }
		}

		[ThreadStatic]
		static MockOnlineSailingSchedulesDataVendor instance;

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = delegate
			{ return Instance; };
		}

		public static void UnregisterThisSubTypeOverride()
		{
			OverridableNewDelegate.ResetValue();
			instance = null;
		}

		protected override IRoutesProvider GetRoutesProvider(BusinessObjectFactory factory)
		{
			return routesProvider;
		}

		public void SetRoutesProvider(IRoutesProvider routesProvider)
		{
			this.routesProvider = routesProvider;
		}

		IRoutesProvider routesProvider;
	}
}
