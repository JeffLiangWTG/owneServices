using System;
using System.Runtime.InteropServices;

namespace Enterprise.CryptoUtilities
{
	/// <summary>
	/// Summary description for CryptoInt.
	/// </summary>
	public class CryptoInt : CryptoUtilityObject
	{
		public CryptoInt() : this(0) { }

		public CryptoInt(int Value)
		{
			this.Value = Value;
		}

		public int Value
		{
			get
			{
				return Marshal.ReadInt32(Pointer);
			}
			set
			{
				Marshal.WriteInt32(Pointer, value);
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
