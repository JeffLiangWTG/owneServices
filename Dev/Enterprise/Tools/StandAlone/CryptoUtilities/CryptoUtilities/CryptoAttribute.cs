using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Enterprise.CryptoUtilities
{
	public class CryptoAttribute
	{
		#region Constants

		public abstract class AttributeType
		{
			public const string szOID_RSA_emailAddr = "1.2.840.113549.1.9.1";
			public const string szOID_RSA_unstructName = "1.2.840.113549.1.9.2";
			public const string szOID_RSA_contentType = "1.2.840.113549.1.9.3";
			public const string szOID_RSA_messageDigest = "1.2.840.113549.1.9.4";
			public const string szOID_RSA_signingTime = "1.2.840.113549.1.9.5";
			public const string szOID_RSA_counterSign = "1.2.840.113549.1.9.6";
			public const string szOID_RSA_challengePwd = "1.2.840.113549.1.9.7";
			public const string szOID_RSA_unstructAddr = "1.2.840.113549.1.9.8";
			public const string szOID_RSA_extCertAttrs = "1.2.840.113549.1.9.9";
			public const string szOID_RSA_certExtensions = "1.2.840.113549.1.9.14";
			public const string szOID_RSA_SMIMECapabilities = "1.2.840.113549.1.9.15";
			public const string szOID_RSA_preferSignedData = "1.2.840.113549.1.9.15.1";
		}

		#endregion

		public CryptoAttribute(string Attribute, object Value)
		{
			if (Value is SMIMECapabilities)
			{
				Value = ((SMIMECapabilities)Value).CapabilitiesStruct;
			}
			InitialiseStructure(Attribute, new CryptoDataBlob(Attribute, Value));
		}

		public void Dispose()
		{
			for (int i = 0; i < ValueItems.Length; i++)
			{
				if (ValueItems[i] != null)
				{
					ValueItems[i].Dispose();
					ValueItems[i] = null;
				}
			}
			if (AttributeStruct.rgValue != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(AttributeStruct.rgValue);
				AttributeStruct.rgValue = IntPtr.Zero;
			}
			Functions.CryptoAttributesInMemory--;
		}

		public void CopyToUnmanagedMemory(IntPtr Pointer)
		{
			Marshal.StructureToPtr(AttributeStruct, Pointer, false);
		}

		#region Implementation

		protected void InitialiseStructure(string Attribute, params CryptoDataBlob[] ValueItems)
		{
			Interlocked.Increment(ref Functions.CryptoAttributesInMemory);

			AttributeStruct.pszObjId = Attribute;
			int BLOBStructSize = Marshal.SizeOf(typeof(CRYPTOAPI_BLOB));
			this.ValueItems = ValueItems;
			//Marshal the BLOBs and the array of BLOB pointers
			AttributeStruct.cValue = (uint)ValueItems.Length;

			AttributeStruct.rgValue = Marshal.AllocHGlobal((int)(BLOBStructSize * AttributeStruct.cValue));
			for (int i = 0; i < AttributeStruct.cValue; i++)
			{
				unchecked
				{
					ValueItems[i].CopyToUnmanagedMemory(new IntPtr(AttributeStruct.rgValue.ToInt64() + BLOBStructSize * i));
				}
			}
		}

		protected CryptoDataBlob[] ValueItems;
		protected internal CRYPT_ATTRIBUTE AttributeStruct;

		#endregion
	}
}
