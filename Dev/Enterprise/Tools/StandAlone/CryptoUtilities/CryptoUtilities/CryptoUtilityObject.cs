using System;
using System.Runtime.InteropServices;

using CargoWise.Common.Testing;

namespace Enterprise.CryptoUtilities
{
	public class CryptoUtilityObject : IDisposable
	{
		public CryptoUtilityObject()
		{
#if DEBUG
			ResisterDisposable();
#endif
		}

		#region IDisposable Members

		public virtual void Dispose()
		{
#if DEBUG
			UnResisterDisposable();
#endif
		}

		#endregion

		#region Implementation

		protected void ResisterDisposable()
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
			//			Assembly CoreAssembly = Assembly.Load("Core");
			//			Type DisposableListenerType = CoreAssembly.GetType("DisposableLeakListener");
			//			FieldInfo InstanceField = DisposableListenerType.GetField(("Instance"));
			//			object Listener = InstanceField.GetValue(null);
			//			MethodInfo RegisterDisposableMethod = DisposableListenerType.GetMethod("RegisterDisposable", new Type[]{typeof(IDisposable)});
			//			RegisterDisposableMethod.Invoke(Listener, new object[]{this});
		}

		protected void UnResisterDisposable()
		{
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
			//			Assembly CoreAssembly = Assembly.Load("Core");
			//			Type DisposableListenerType = CoreAssembly.GetType("DisposableLeakListener");
			//			FieldInfo InstanceField = DisposableListenerType.GetField(("Instance"));
			//			object Listener = InstanceField.GetValue(null);
			//			MethodInfo UnRegisterDisposableMethod = DisposableListenerType.GetMethod("UnRegisterDisposable", new Type[]{typeof(IDisposable)});
			//			UnRegisterDisposableMethod.Invoke(Listener, new object[]{this});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "exception message")]
		protected static void HandleWin32Error()
		{
			int errorCode = Marshal.GetLastWin32Error();
			string errorMessage = "";
			if (errorCode == -2146885620)
			{
				errorMessage = "The data could not be decrypted with the given private certificate.";
			}
			else if (errorCode == -2146889726)
			{
				errorMessage = "Unknown cryptographic algorithm.";
			}
			Exception e = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
			if (e != null)
			{
				errorMessage += " " + e.Message;
			}
			throw new CryptoUtilitiesException("CryptoUtility Exception. " + errorMessage, errorCode);
		}

		#endregion
	}
}
