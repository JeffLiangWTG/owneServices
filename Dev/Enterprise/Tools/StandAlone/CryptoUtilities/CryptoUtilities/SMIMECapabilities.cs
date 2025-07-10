using System;
using System.Runtime.InteropServices;

namespace Enterprise.CryptoUtilities
{
	public sealed class SMIMECapabilities
	{
		public static SMIMECapabilities GetInstance()
		{
			if (MyInstance == null)
			{
				MyInstance = new SMIMECapabilities();
			}
			return MyInstance;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1022:ThreadStaticSetInStaticInitializerRule", Justification = "Baseline issue")]
		[ThreadStatic]
		private static SMIMECapabilities MyInstance = null;

		SMIMECapabilities()
		{
			int CapabilityStructSize = Marshal.SizeOf(typeof(CRYPT_SMIME_CAPABILITY));
			Capabilities = new SMIMECapability[7];
			Capabilities[0] = new SMIMECapability(Capability.szOID_RSA_RC2CBC, Array.Empty<byte>());
			Capabilities[1] = new SMIMECapability(Capability.szOID_RSA_DES_EDE3_CBC, new byte[4] { 0x02, 0x02, 0x00, 0x80 });
			Capabilities[2] = new SMIMECapability(Capability.szOID_RSA_DES_EDE3_CBC, new byte[3] { 0x02, 0x01, 0x40 });
			Capabilities[3] = new SMIMECapability(Capability.szOID_OIWSEC_desCBC, Array.Empty<byte>());
			Capabilities[4] = new SMIMECapability(Capability.szOID_RSA_DES_EDE3_CBC, new byte[3] { 0x02, 0x01, 0x28 });
			Capabilities[5] = new SMIMECapability(Capability.szOID_RSA_SHA1RSA, Array.Empty<byte>());
			Capabilities[6] = new SMIMECapability(Capability.szOID_RSA_MD5RSA, Array.Empty<byte>());

			CapabilitiesStruct = new CRYPT_SMIME_CAPABILITIES();
			CapabilitiesStruct.cCapability = (uint)Capabilities.Length;
			CapabilitiesStruct.rgCapability = Marshal.AllocHGlobal((int)(CapabilitiesStruct.cCapability * CapabilityStructSize));

			for (int i = 0; i < CapabilitiesStruct.cCapability; i++)
			{
				unchecked
				{
					Marshal.StructureToPtr(Capabilities[i].CapabilityStruct, new IntPtr(CapabilitiesStruct.rgCapability.ToInt64() + i * CapabilityStructSize), false);
				}
			}
			//CapabilitiesStruct.
			//	CapabilitiesStruct.
		}

		~SMIMECapabilities()
		{
			if (Capabilities != null)
			{
				for (int i = 0; i < Capabilities.Length; i++)
				{
					Capabilities[i].Dispose();
					Capabilities[i] = null;
				}
			}
			if (CapabilitiesStruct.rgCapability != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(CapabilitiesStruct.rgCapability);
			}
			CapabilitiesStruct.rgCapability = IntPtr.Zero;
			CapabilitiesStruct.cCapability = 0;

		}
		internal CRYPT_SMIME_CAPABILITIES CapabilitiesStruct;
		private readonly SMIMECapability[] Capabilities;

		internal class SMIMECapability
		{
			public SMIMECapability(string CapabilityOID, byte[] ParameterValue)
			{
				CapabilityStruct = new CRYPT_SMIME_CAPABILITY();
				CapabilityStruct.pszObjId = CapabilityOID;

				CapabilityStruct.Parameters.cbData = (uint)ParameterValue.Length;
				CapabilityStruct.Parameters.pbData = Marshal.AllocHGlobal(CapabilityStruct.Parameters.pbData);
				Marshal.Copy(ParameterValue, 0, CapabilityStruct.Parameters.pbData, (int)CapabilityStruct.Parameters.cbData);
			}

			public void Dispose()
			{
				if (CapabilityStruct.Parameters.pbData != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(CapabilityStruct.Parameters.pbData);
					CapabilityStruct.Parameters.pbData = IntPtr.Zero;
					CapabilityStruct.Parameters.cbData = 0;
				}
			}
			internal CRYPT_SMIME_CAPABILITY CapabilityStruct;
		}

		internal class Capability
		{
			public static readonly string szOID_RSA_RC2CBC = "1.2.840.113549.3.2";
			public static readonly string szOID_RSA_DES_EDE3_CBC = "1.2.840.113549.3.7";

			public static readonly string szOID_OIWSEC_desCBC = "1.3.14.3.2.7";

			public static readonly string szOID_RSA_SHA1RSA = "1.2.840.113549.1.1.5";

			public static readonly string szOID_RSA_MD5RSA = "1.2.840.113549.1.1.4";

		}
	}
}
