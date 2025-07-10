using System;
using System.Runtime.InteropServices;

namespace Enterprise.CryptoUtilities
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Previously located in ThirdParty. Include until it is known that it is not required.")]
	public class CryptoString : CryptoUtilityObject
	{
		public CryptoString(string Value)
		{
			this.Value = Value;
		}

		public CryptoInt Length
		{
			get
			{
				if (fLength == null)
				{
					fLength = new CryptoInt(Value.Length);
				}
				return fLength;
			}
		}

		public IntPtr Pointer
		{
			get
			{
				if (fPointer == IntPtr.Zero)
				{
					byte[] MessageToSignBytes = System.Text.Encoding.ASCII.GetBytes(Value);
					int MessageLength = MessageToSignBytes.Length;
					fPointer = Marshal.AllocHGlobal(MessageLength);
					Marshal.Copy(MessageToSignBytes, 0, fPointer, MessageLength);
				}
				return fPointer;
			}
		}

		public IntPtr PointerPointer
		{
			get
			{
				if (fPointerPointer == IntPtr.Zero)
				{
					fPointerPointer = Marshal.AllocHGlobal(4);
					Marshal.WriteIntPtr(fPointerPointer, Pointer);
				}
				return fPointerPointer;
			}
		}

		public readonly string Value;

		#region Implementation

		protected CryptoInt fLength;
		protected IntPtr fPointer = IntPtr.Zero;
		protected IntPtr fPointerPointer = IntPtr.Zero;

		#endregion

		#region IDisposable Members

		public override void Dispose()
		{
			base.Dispose();
			if (fLength != null)
			{
				fLength.Dispose();
				fLength = null;
			}
			if (fPointerPointer != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(fPointerPointer);
				fPointerPointer = IntPtr.Zero;
			}
			if (fPointer != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(fPointer);
				fPointer = IntPtr.Zero;
			}
		}

		#endregion
	}
}
