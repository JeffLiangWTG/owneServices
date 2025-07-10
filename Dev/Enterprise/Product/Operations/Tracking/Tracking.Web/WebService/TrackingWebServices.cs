using System;
using System.Web.UI;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;

#if DEBUG

using Enterprise.ZArchitecture.Web.GUI.Testing;

#endif

namespace Enterprise.Tracking.Web.ServerServices
{
	public sealed class TrackingWebServices
	{
		#region Constructors

		TrackingWebServices()
		{
		}

		#endregion

		#region Instance

		public static TrackingWebServices Instance
		{
			get
			{
				lock (StaticLock)
				{
					if (instance == null)
					{
						instance = new TrackingWebServices();
					}
					return instance;
				}
			}
		}

		[ThreadStatic]
		static TrackingWebServices instance;

		#endregion

		#region Properties

		public ServiceReference WebServiceReference
		{
			get
			{
				lock (InstanceLock)
				{
					if (webServiceReference == null)
					{
						ZGlobal appInstance = WebEnv.AppInstance as ZGlobal;
#if DEBUG
						if (Enterprise.ZArchitecture.Environment.Globals.IsTest)
						{
							appInstance = GetNewTestGlobal();
						}
#endif
						webServiceReference = new ServiceReference(appInstance.ApplicationRoot + "WebService/TrackingWebService.asmx");
					}
					return webServiceReference;
				}
			}
		}

		ServiceReference webServiceReference;

		#endregion

		#region Implementation

#if DEBUG

		ZGlobal GetNewTestGlobal()
		{
			return new ZTestGlobal();
		}

#endif

		readonly static object StaticLock = new object();
		readonly object InstanceLock = new object();

		#endregion
	}
}
