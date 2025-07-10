using System;
using System.Runtime.InteropServices;

namespace Enterprise.CryptoUtilities
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Previously located in ThirdParty. Include until it is known that it is not required.")]
	public static class MessageType
	{
		public static readonly uint CMSG_DATA = 1;
		public static readonly uint CMSG_SIGNED = 2;
		public static readonly uint CMSG_ENVELOPED = 3;
		public static readonly uint CMSG_SIGNED_AND_ENVELOPED = 4;
		public static readonly uint CMSG_HASHED = 5;
		public static readonly uint CMSG_ENCRYPTED = 6;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct CRYPT_ATTRIBUTE
	{
		[MarshalAs(UnmanagedType.LPStr)] public string pszObjId;
		public uint cValue;
		public IntPtr rgValue;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct CRYPTOAPI_BLOB
	{
		public uint cbData;
		public IntPtr pbData;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct CRYPT_SIGN_MESSAGE_PARA
	{
		public uint cbSize;
		public uint dwMsgEncodingType;
		public IntPtr pSigningCert;
		public CRYPT_ALGORITHM_IDENTIFIER HashAlgorithm;
		public IntPtr pvHashAuxInfo;
		public uint cMsgCert;
		public IntPtr rgpMsgCert;
		public uint cMsgCrl;
		public IntPtr rgpMsgCrl;
		public uint cAuthAttr;
		public IntPtr rgAuthAttr;
		public uint cUnauthAttr;
		public IntPtr rgUnauthAttr;
		public uint dwFlags;
		public uint dwInnerContentType;
		public CRYPT_ALGORITHM_IDENTIFIER HashEncryptionAlgorithm;
		public IntPtr pvHashEncryptionAuxInfo;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct CRYPT_ALGORITHM_IDENTIFIER
	{
		[MarshalAs(UnmanagedType.LPStr)] public string pszObjId;
		//public IntPtr				pszObjId;
		public CRYPTOAPI_BLOB Parameters;
	}

	[WTG.StaticAnalysis.Annotation.CodeAlive("Previously located in ThirdParty. Include until it is known that it is not required.")]
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct CRYPT_ENCRYPT_MESSAGE_PARA
	{
		public uint cbSize;
		public uint dwMsgEncodingType;
		public IntPtr hCryptProv;
		public CRYPT_ALGORITHM_IDENTIFIER ContentEncryptionAlgorithm;
		public IntPtr pvEncryptionAuxInfo;
		public uint dwFlags;
		public uint dwInnerContentType;
	}

	[WTG.StaticAnalysis.Annotation.CodeAlive("Previously located in ThirdParty. Include until it is known that it is not required.")]
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct CRYPT_VERIFY_MESSAGE_PARA
	{
		public uint cbSize;
		public uint dwMsgAndCertEncodingType;
		public IntPtr hCryptProv;
		public IntPtr pfnGetSignerCertificate;
		public IntPtr pvGetArg;
	}

	[WTG.StaticAnalysis.Annotation.CodeAlive("Previously located in ThirdParty. Include until it is known that it is not required.")]
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public class CERT_CONTEXT
	{
		public CERT_CONTEXT(IntPtr Pointer)
		{
			Marshal.PtrToStructure(Pointer, this);
		}

		public uint dwCertEncodingType;
		public IntPtr pbCertEncoded;
		public uint cbCertEncoded;
		public IntPtr pCertInfo;
		public IntPtr hCertStore;
	}

	[WTG.StaticAnalysis.Annotation.CodeAlive("Previously located in ThirdParty. Include until it is known that it is not required.")]
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct CERT_INFO
	{
		public uint dwVersion;
		public CRYPTOAPI_BLOB SerialNumber;
		public CRYPT_ALGORITHM_IDENTIFIER SignatureAlgorithm;
		public CRYPTOAPI_BLOB Issuer;
		public long NotBefore;
		public long NotAfter;
		public CRYPTOAPI_BLOB Subject;
		public CRYPTOAPI_BLOB SubjectPublicKeyInfo;
		public CRYPTOAPI_BLOB IssuerUniqueId;
		public CRYPTOAPI_BLOB SubjectUniqueId;
		public uint cExtension;
		public IntPtr rgExtension;
	}

	[WTG.StaticAnalysis.Annotation.CodeAlive("Previously located in ThirdParty. Include until it is known that it is not required.")]
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct CRYPT_DECRYPT_MESSAGE_PARA98
	{
		public uint cbSize;
		public uint dwMsgAndCertEncodingType;
		public uint cCertStore;
		public IntPtr rghCertStore;
	}

	[WTG.StaticAnalysis.Annotation.CodeAlive("Previously located in ThirdParty. Include until it is known that it is not required.")]
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct CRYPT_KEY_PROV_INFO
	{
		public string pwszContainerName;
		public string pwszProvName;
		public uint dwProvType;
		public uint dwFlags;
		public uint cProvParam;
		public IntPtr rgProvParam;
		public uint dwKeySpec;
	}

	[WTG.StaticAnalysis.Annotation.CodeAlive("Previously located in ThirdParty. Include until it is known that it is not required.")]
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct FILETIME
	{
		public uint dwLowDateTime;
		public uint dwHighDateTime;
	}

	[WTG.StaticAnalysis.Annotation.CodeAlive("Previously located in ThirdParty. Include until it is known that it is not required.")]
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct SYSTEMTIME
	{
		public ushort wYear;
		public ushort wMonth;
		public ushort wDayOfWeek;
		public ushort wDay;
		public ushort wHour;
		public ushort wMinute;
		public ushort wSecond;
		public ushort wMilliseconds;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	internal struct CTL_USAGE
	{
		public uint cUsageIdentifier;
		public IntPtr rgpszUsageIdentifier;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct CERT_USAGE_MATCH
	{
		public uint dwType;
		public CTL_USAGE Usage;
	}

	[WTG.StaticAnalysis.Annotation.CodeAlive("Previously located in ThirdParty. Include until it is known that it is not required.")]
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct CERT_CHAIN_PARA
	{
		public uint cbSize;
		public CERT_USAGE_MATCH RequestedUsage;
		public CERT_USAGE_MATCH RequestedIssuancePolicy;
		public uint dwUrlRetrievalTimeout;
		public bool fCheckRevocationFreshnessTime;
		public uint dwRevocationFreshnessTime;
	}

	[WTG.StaticAnalysis.Annotation.CodeAlive("Previously located in ThirdParty. Include until it is known that it is not required.")]
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct CERT_CHAIN_POLICY_PARA
	{
		public uint cbSize;
		public uint dwFlags;
		public IntPtr pvExtraPolicyPara;
	}

	[WTG.StaticAnalysis.Annotation.CodeAlive("Previously located in ThirdParty. Include until it is known that it is not required.")]
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct CERT_CHAIN_POLICY_STATUS
	{
		public uint cbSize;
		public uint dwError;
		public int lChainIndex;
		public int lElementIndex;
		public IntPtr pvExtraPolicyStatus;
	}

	[WTG.StaticAnalysis.Annotation.CodeAlive("Previously located in ThirdParty. Include until it is known that it is not required.")]
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct CRYPT_DECRYPT_MESSAGE_PARA
	{
		public uint cbSize;
		public uint dwMsgAndCertEncodingType;
		public uint cCertStore;
		public IntPtr rghCertStore;
		public uint dwFlags;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct CRYPT_SMIME_CAPABILITIES
	{
		public uint cCapability;
		public IntPtr rgCapability;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	internal struct CRYPT_SMIME_CAPABILITY
	{
		[MarshalAs(UnmanagedType.LPStr)] public string pszObjId;
		public CRYPTOAPI_BLOB Parameters;
	}

	[WTG.StaticAnalysis.Annotation.CodeAlive("Previously located in ThirdParty. Include until it is known that it is not required.")]
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct CERT_KEY_CONTEXT
	{
		public uint cbSize;
		public IntPtr hCryptProv;
		public uint dwKeySpec;
	}

	[WTG.StaticAnalysis.Annotation.CodeAlive("Previously located in ThirdParty. Include until it is known that it is not required.")]
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct PUBLICKEYSTRUC
	{
		public byte bType;
		public byte bVersion;
		public ushort reserved;
		public uint aiKeyAlg;
	}

	[WTG.StaticAnalysis.Annotation.CodeAlive("Previously located in ThirdParty. Include until it is known that it is not required.")]
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct PKCS12
	{
		readonly IntPtr version;
		readonly IntPtr mac;
		readonly IntPtr authsafes;
	}
}
