using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Enterprise.CryptoUtilities
{
	/// <summary>
	/// Summary description for CryptoDataBlob.
	/// </summary>
	public class CryptoDataBlob
	{
		public CryptoDataBlob(string CryptoStructureType, object Value)
		{
			InitialiseStructure(CryptoStructureType, Value);
		}

		public void Dispose()
		{
			if (BLOBStruct.pbData != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(BLOBStruct.pbData);
				BLOBStruct.pbData = IntPtr.Zero;
				BLOBStruct.cbData = 0;
			}
			Interlocked.Decrement(ref Functions.CryptoDataBlobsInMemory);
		}

		public void CopyToUnmanagedMemory(IntPtr PointerToCopyTo)
		{
			Marshal.StructureToPtr(BLOBStruct, PointerToCopyTo, false);
		}

		#region Implementation

		protected void InitialiseStructure(string CryptoStructureType, object StructureToEncode)
		{
			if (StructureToEncode is DateTime)
			{
				StructureToEncode = ((DateTime)StructureToEncode).ToFileTime();
			}

			IntPtr StructureToEncodePointer = Marshal.AllocHGlobal(Marshal.SizeOf(StructureToEncode));
			Marshal.StructureToPtr(StructureToEncode, StructureToEncodePointer, false);
			IntPtr EncodedOutputSize = Marshal.AllocHGlobal(4);
			//find the size of the output
			if (!Functions.CryptEncodeObject(0x00010001, CryptoStructureType, StructureToEncodePointer, IntPtr.Zero, EncodedOutputSize))
			{
				throw new CryptoUtilitiesException("Couldnt encode structure.  Error num: " + Marshal.GetLastWin32Error(), Marshal.GetLastWin32Error());
			}
			IntPtr EncodedStructurePointer = Marshal.AllocHGlobal(Marshal.ReadInt32(EncodedOutputSize));
			if (!Functions.CryptEncodeObject(0x00010001, CryptoStructureType, StructureToEncodePointer, EncodedStructurePointer, EncodedOutputSize))
			{
				throw new CryptoUtilitiesException("Couldnt encode structure.  Error num: " + Marshal.GetLastWin32Error(), Marshal.GetLastWin32Error());
			}

			uint EncodedBytesSize = (uint)Marshal.ReadInt32(EncodedOutputSize);

			Marshal.FreeHGlobal(EncodedOutputSize);
			Marshal.FreeHGlobal(StructureToEncodePointer);

			InitialiseStructure(EncodedStructurePointer, EncodedBytesSize);
		}

		protected void InitialiseStructure(IntPtr ValueBytesPointer, uint ValueBytesSize)
		{
			Interlocked.Increment(ref Functions.CryptoDataBlobsInMemory);
			BLOBStruct.cbData = ValueBytesSize;
			BLOBStruct.pbData = ValueBytesPointer;
		}

		protected internal CRYPTOAPI_BLOB BLOBStruct;

		#endregion
	}
}
