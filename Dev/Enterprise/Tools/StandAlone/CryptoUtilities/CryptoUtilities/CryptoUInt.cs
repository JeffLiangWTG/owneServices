using System;
using System.Runtime.InteropServices;

namespace Enterprise.CryptoUtilities
{
	/// <summary>
	/// Summary description for CryptoInt.
	/// </summary>
	public class CryptoUInt : CryptoUtilityObject
	{
		public CryptoUInt() : this(0) { }

		public CryptoUInt(uint Value)
		{
			this.Value = Value;
		}

		public uint Value
		{
			get
			{
				return (uint)Marshal.ReadInt32(Pointer);
			}
			set
			{
				Marshal.WriteInt32(Pointer, (int)value);
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
