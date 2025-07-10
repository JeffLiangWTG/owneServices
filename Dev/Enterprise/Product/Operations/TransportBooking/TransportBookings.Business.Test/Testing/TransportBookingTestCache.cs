using System;
using System.Collections.Generic;
using CargoWise.Common.Testing;
using CargoWise.Types;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class TransportBookingTestCache : IDisposable
	{
		TransportBookingTestCache()
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		public static TransportBookingTestCache Instance
		{
			get { return instance ?? (instance = new TransportBookingTestCache()); }
		}

		[ThreadStatic]
		static TransportBookingTestCache instance;

		public Dictionary<ZString, IZType> Data
		{
			get { return data ?? (data = new Dictionary<ZString, IZType>()); }
		}

		Dictionary<ZString, IZType> data;

		public void AddData(ZString key, IZType value)
		{
			if (!Data.ContainsKey(key))
			{
				Data.Add(key, value);
			}
		}

		void IDisposable.Dispose()
		{
			instance = null;
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}
	}
}
