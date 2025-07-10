using System;
using System.Runtime.InteropServices;

namespace Enterprise.CryptoUtilities
{
	/// <summary>
	/// Summary description for CryptoInt.
	/// </summary>
	public class CryptoByteArray : CryptoUtilityObject
	{
		public CryptoByteArray(byte[] Value) : this(Value.Length)
		{
			this.Value = Value;
		}

		public CryptoByteArray(int Size)
		{
			this.Size = Size;
		}

		public byte[] Value
		{
			get
			{
				byte[] Result = new byte[Size];
				Marshal.Copy(Pointer, Result, 0, Size);
				return Result;
			}
			set
			{
				if (value.Length != Size)
				{
					throw new CryptoUtilitiesException("Can't set this value because the sizes don't match up.  Size needed = " + Size + ", size supplied = " + Value.Length, 0);
				}
				Marshal.Copy(value, 0, Pointer, Size);
			}
		}

		public string ValueAsBase64String
		{
			get
			{
				return Functions.ToBase64String(Value);
			}
		}

		public IntPtr Pointer
		{
			get
			{
				if (fPointer == IntPtr.Zero)
				{
					fPointer = Marshal.AllocHGlobal(Size);
				}
				return fPointer;
			}
		}

		public int Size;

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

		protected IntPtr fPointer = IntPtr.Zero;

		#endregion
	}
}
