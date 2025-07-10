using System;
using System.Runtime.InteropServices;

namespace Enterprise.CryptoUtilities
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Previously located in ThirdParty. Include until it is known that it is not required.")]
	public class CryptoUniString : CryptoUtilityObject
	{
		public CryptoUniString(string Value)
		{
			this.Value = Value;
		}

		public IntPtr Pointer
		{
			get
			{
				if (fPointer == IntPtr.Zero)
				{
					fPointer = Marshal.StringToHGlobalUni(Value);
				}
				return fPointer;
			}
		}

		protected IntPtr fPointer;
		protected readonly string Value;
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
	}
}
