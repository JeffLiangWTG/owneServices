using System;
using System.Runtime.InteropServices;

namespace Enterprise.CryptoUtilities
{
	/// <summary>
	/// Summary description for CryptoInt.
	/// </summary>
	public class CryptoBool : IDisposable
	{
		public CryptoBool() : this(false) { }

		public CryptoBool(bool Value)
		{
			this.Value = Value;
		}

		public bool Value
		{
			get
			{
				return Marshal.ReadByte(Pointer) != 0;
			}
			set
			{
				Marshal.WriteByte(Pointer, value ? (byte)1 : (byte)0);
			}
		}

		public IntPtr Pointer
		{
			get
			{
				if (fPointer == IntPtr.Zero)
				{
					fPointer = Marshal.AllocHGlobal(4);
				}
				return fPointer;
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
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
