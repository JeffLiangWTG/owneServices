using System;
using System.Runtime.InteropServices;

namespace Enterprise.CryptoUtilities
{
	/// <summary>
	/// Summary description for CryptoInt.
	/// </summary>
	public class CryptoPointer : CryptoUtilityObject
	{
		public CryptoPointer() : this(IntPtr.Zero) { }

		public CryptoPointer(IntPtr Value)
		{
			this.Value = Value;
		}

		public IntPtr Value
		{
			get
			{
				return Marshal.ReadIntPtr(Pointer);
			}
			set
			{
				Marshal.WriteIntPtr(Pointer, value);
			}
		}

		public IntPtr Pointer
		{
			get
			{
				if (fPointer == IntPtr.Zero)
				{
					fPointer = Marshal.AllocHGlobal(IntPtr.Size);
				}
				return fPointer;
			}
		}

		#region IDisposable Members

		public override void Dispose()
		{
			base.Dispose();
			if (fPointer != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(fPointer);
				fPointer = IntPtr.Zero;
			}
		}

		#endregion

		#region Implementation

		public IntPtr fPointer = IntPtr.Zero;

		#endregion
	}
}
