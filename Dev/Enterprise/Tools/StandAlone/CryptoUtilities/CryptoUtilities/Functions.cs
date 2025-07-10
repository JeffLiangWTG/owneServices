using System;
using System.Runtime.InteropServices;
using System.Text;
using CargoWise.Common.Testing;

namespace Enterprise.CryptoUtilities
{
	public abstract class Functions
	{
		[SuppressThreadStaticFieldMessage]
		internal static int CryptoDataBlobsInMemory;
		[SuppressThreadStaticFieldMessage]
		internal static int CryptoAttributesInMemory;
		[SuppressThreadStaticFieldMessage]
		internal static int CryptoAttributeCollectionsInMemory;

		public static string ToBase64String(byte[] Contents)
		{
			int MaxCharacters = 80;
			int BytesPerLine = ((MaxCharacters - 2) / 4) * 3;
			int ContentsIndex = 0;
			int EstimatedLength = (Contents.Length * 4) / 3 + 2 * Contents.Length / BytesPerLine + 10;
			StringBuilder MyBuilder = new StringBuilder();
			while (ContentsIndex < Contents.Length)
			{
				int ByteBlockLength = BytesPerLine;
				if ((ContentsIndex + ByteBlockLength) > Contents.Length)
				{
					ByteBlockLength = Contents.Length - ContentsIndex;
				}
				MyBuilder.Append(Convert.ToBase64String(Contents, ContentsIndex, ByteBlockLength));
				MyBuilder.Append("\r\n");
				ContentsIndex += ByteBlockLength;
			}
			string ReturnValue = MyBuilder.ToString();
			return ReturnValue;
		}

		public static string GetNiceByteArrayOutput(byte[] ByteArray)
		{
			return GetNiceByteArrayOutput(ByteArray, false);
		}

		public static string GetNiceByteArrayOutput(byte[] ByteArray, bool ReverseOrder)
		{
			StringBuilder ResultBuilder = new StringBuilder();
			if (ByteArray == null)
			{
				ResultBuilder.Append("null");
			}
			else
			{
				if (!ReverseOrder)
				{
					foreach (byte MyByte in ByteArray)
					{
						string ByteString = System.Convert.ToString(MyByte, 16);
						if (ByteString.Length == 1)
						{
							ByteString = "0" + ByteString;
						}
						ResultBuilder.Append(ByteString + " ");
					}
				}
				else
				{
					for (int i = ByteArray.Length - 1; i >= 0; i--)
					{
						string ByteString = System.Convert.ToString(ByteArray[i], 16);
						if (ByteString.Length == 1)
						{
							ByteString = "0" + ByteString;
						}

						ResultBuilder.Append(ByteString + " ");
					}
				}
			}

			if (ResultBuilder.Length == 0)
			{
				return "";
			}

			return ResultBuilder.ToString(0, ResultBuilder.Length - 1); //take off last space
		}

		[DllImport("crypt32.dll", CharSet = CharSet.Ansi)]
		internal static extern bool CertGetCertificateContextProperty(
			IntPtr pCertContext,
			uint dwPropId,
			IntPtr pvData,
			ref uint pcbData
			);

		[DllImport("crypt32.dll", CharSet = CharSet.Ansi)]
		internal static extern uint CertGetNameString(
			IntPtr pCertContext,
			uint dwType,
			uint dwFlags,
			IntPtr pvTypePara,
			IntPtr pszNameString,
			uint cchNameString
			);

		[DllImport("crypt32.dll", CharSet = CharSet.Ansi)]
		internal static extern bool CertFreeCertificateContext(
			IntPtr pCertContext
			);

		[DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern IntPtr CertDuplicateCertificateContext(
			IntPtr pCertContext
			);

		[DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern IntPtr CertCreateContext(
			uint dwContextType,
			uint dwEncodingType,
			IntPtr pbEncoded,
			uint cbEncoded,
			uint dwFlags,
			IntPtr pCreatePara
			);

		[DllImport("Crypt32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern bool CertCompareCertificate(
			uint dwCertEncodingType,
			IntPtr pCertId1,
			IntPtr pCertId2
			);

		[DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern bool CryptDestroyKey(
			IntPtr hKey
			);

		[DllImport("Crypt32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern int CertVerifyTimeValidity(
			IntPtr pTimeToVerify,
			IntPtr pCertInfo
			);

		[DllImport("Crypt32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern bool CertVerifySubjectCertificateContext(
			IntPtr pSubject,
			IntPtr pIssuer,
			IntPtr pdwFlags
			);

		[DllImport("Crypt32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern bool CryptEncodeObject(
			uint dwCertEncodingType,
			[MarshalAs(UnmanagedType.LPStr)] string lpszStructType,
			IntPtr pvStructInfo,
			IntPtr pbEncoded,
			IntPtr pcbEncoded
			);

		[DllImport("Crypt32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern bool CryptEncryptMessage(
			IntPtr pEncryptPara,
			uint cRecipientCert,
			IntPtr rgpRecipientCert,
			IntPtr pbToBeEncrypted,
			uint cbToBeEncrypted,
			IntPtr pbEncryptedBlob,
			IntPtr pcbEncryptedBlob
			);

		[DllImport("CryptUI.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern bool CryptUIDlgViewContext(
			uint dwContextType,
			IntPtr pvContext,
			uint hwnd,
			IntPtr pwszTitle,
			uint dwFlags,
			IntPtr pvReserved
			);

		[DllImport("Crypt32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern bool CryptVerifyMessageSignature(
			IntPtr pVerifyPara,
			uint dwSignerIndex,
			IntPtr pbSignedBlob,
			uint cbSignedBlob,
			IntPtr pbDecoded,
			IntPtr pcbDecoded,
			IntPtr ppSignerCert
			);

		[DllImport("Crypt32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern bool CryptSignMessage(
			IntPtr pSignPara,
			bool fDetachedSignature,
			uint cToBeSigned,
			IntPtr rgpbToBeSigned,
			IntPtr rgcbToBeSigned,
			IntPtr pbSignedBlob,
			IntPtr pcbSignedBlob
			);

		[DllImport("Crypt32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern IntPtr CertFindCertificateInStore(
			IntPtr hCertStore,
			uint dwCertEncodingType,
			uint dwFindFlags,
			uint dwFindType,
			IntPtr pvFindPara,
			IntPtr pPrevCertContext
			);

		[DllImport("Crypt32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern bool CertSetCertificateContextProperty(
			IntPtr pCertContext,
			uint dwPropId,
			uint dwFlags,
			IntPtr pvData
			);

		[DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern bool CryptExportKey(
			IntPtr hKey,
			IntPtr hExpKey,
			uint dwBlobType,
			uint dwFlags,
			IntPtr pbData,
			IntPtr pdwDataLen
			);

		[DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern bool CryptGetUserKey(
			IntPtr hProv,
			uint dwKeySpec,
			IntPtr phUserKey
			);

		[DllImport("Crypt32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern bool CryptAcquireCertificatePrivateKey(
			IntPtr pCert,
			uint dwFlags,
			IntPtr pvReserved,
			IntPtr phCryptProv,
			IntPtr pdwKeySpec,
			IntPtr pfCallerFreeProv
			);

		[DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern bool CryptReleaseContext(
			IntPtr hProv,
			uint dwFlags
			);

		[DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern IntPtr CertOpenSystemStore(
			IntPtr hProv,
			String szSubsystemProtocol
			);

		[DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern IntPtr CertEnumCertificatesInStore(
			IntPtr hCertStore,
			IntPtr pPrevCertContext
			);

		[DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern bool CryptAcquireContext(
			ref IntPtr hProv,
			string pszContainer,
			string pszProvider,
			uint dwProvType,
			uint dwFlags
					);

		[DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern bool CertCloseStore(
			IntPtr hCertStore,
			uint dwFlags
			);

		[DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern IntPtr PFXImportCertStore(
			IntPtr pPFX,
			IntPtr szPassword,
			//string szPassword,
			uint dwFlags
			);

		[DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern bool PFXIsPFXBlob(
			IntPtr pPFX
			);

		[DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern bool CertAddSerializedElementToStore(
			IntPtr hCertStore,
			IntPtr pbElement,
			uint cbElement,
			uint dwAddDisposition,
			uint dwFlags,
			uint dwContextTypeFlags,
			IntPtr pdwContextType,
			IntPtr ppvContext
			);

		[DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		internal static extern bool CertSerializeCertificateStoreElement(
			IntPtr pCertContext,
			uint dwFlags,
			IntPtr pbElement,
			IntPtr pcbElement
			);

		[DllImport("Crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern bool CryptDecryptMessage(
			IntPtr pDecryptPara,
			IntPtr pbEncryptedBlob,
			uint cbEncryptedBlob,
			IntPtr pbDecrypted,
			IntPtr pcbDecrypted,
			IntPtr ppXchgCert
			);
	}
}
