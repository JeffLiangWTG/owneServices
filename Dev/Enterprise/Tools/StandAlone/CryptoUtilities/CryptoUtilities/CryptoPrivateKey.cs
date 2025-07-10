using System;
using System.Runtime.InteropServices;

namespace Enterprise.CryptoUtilities
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Previously located in ThirdParty. Include until it is known that it is not required.")]
	public class CryptoPrivateKey : CryptoUtilityObject
	{
		public CryptoPrivateKey(IntPtr CertificateContextPointer)
		{
			this.CertificateContextPointer = CertificateContextPointer;
		}

		public override void Dispose()
		{
			base.Dispose();
			ReleaseContext();
			DisposeExportedKey();
			DisposeSize();
			DisposeKeyHandle();
			DisposeCertHandleCryptProv();
			DisposeKeyType();
		}

		protected IntPtr CertificateContextPointer;

		protected CryptoPointer CertHandleCryptProv
		{
			get
			{
				if (fCertHandleCryptProv == null)
				{
					fCertHandleCryptProv = new CryptoPointer();

					if (!Functions.CryptAcquireCertificatePrivateKey(CertificateContextPointer, 0, IntPtr.Zero, fCertHandleCryptProv.Pointer, KeyType.Pointer, Free.Pointer))
					{
						throw new CryptoUtilitiesException("Couldn't get a handle to the private key", Marshal.GetLastWin32Error());
					}
				}
				return fCertHandleCryptProv;
			}
		}
		protected CryptoPointer fCertHandleCryptProv;

		protected CryptoUInt KeyType
		{
			get
			{
				if (fKeyType == null)
				{
					fKeyType = new CryptoUInt();
				}
				return fKeyType;
			}
		}
		protected CryptoUInt fKeyType;

		protected CryptoBool Free
		{
			get
			{
				if (fFree == null)
				{
					fFree = new CryptoBool();
				}
				return fFree;
			}
		}
		protected CryptoBool fFree;

		protected CryptoInt Size
		{
			get
			{
				if (fSize == null)
				{
					fSize = new CryptoInt();
					if (!Functions.CryptExportKey(KeyHandle.Value, IntPtr.Zero, 0x7, 0, IntPtr.Zero, Size.Pointer))
					{
						throw new CryptoUtilitiesException("Couldn't export private key", Marshal.GetLastWin32Error());
					}
				}
				return fSize;
			}
		}
		protected CryptoInt fSize;

		public CryptoByteArray ExportedKey
		{
			get
			{
				if (fExportedKey == null)
				{
					fExportedKey = new CryptoByteArray(Size.Value);
					{
						if (!Functions.CryptExportKey(KeyHandle.Value, IntPtr.Zero, 0x7, 0, fExportedKey.Pointer, Size.Pointer))
						{
							throw new CryptoUtilitiesException("Couldn't export private key", Marshal.GetLastWin32Error());
						}
					}
				}
				return fExportedKey;
			}
		}
		protected CryptoByteArray fExportedKey;

		protected CryptoPointer KeyHandle
		{
			get
			{
				if (fKeyHandle == null)
				{
					fKeyHandle = new CryptoPointer();
					if (!Functions.CryptGetUserKey(CertHandleCryptProv.Value, KeyType.Value, fKeyHandle.Pointer))
					{
						throw new CryptoUtilitiesException("Couldn't get a handle to the private key", Marshal.GetLastWin32Error());
					}
				}
				return fKeyHandle;
			}
		}
		protected CryptoPointer fKeyHandle;

		protected void ReleaseContext()
		{
			if (fFree != null && Free.Value && !Functions.CryptReleaseContext(CertHandleCryptProv.Value, 0))
			{
				throw new CryptoUtilitiesException("Failed to release context", Marshal.GetLastWin32Error());
			}
		}

		protected void DisposeFree()
		{
			if (fFree != null)
			{
				fFree.Dispose();
				fFree = null;
			}
		}

		protected void DisposeExportedKey()
		{
			if (fExportedKey != null)
			{
				fExportedKey.Dispose();
				fExportedKey = null;
			}
		}

		protected void DisposeSize()
		{
			if (fSize != null)
			{
				fSize.Dispose();
				fSize = null;
			}
		}

		protected void DisposeKeyHandle()
		{
			if (fKeyHandle != null)
			{
				Functions.CryptDestroyKey(KeyHandle.Value);
				fKeyHandle.Dispose();
				fKeyHandle = null;
			}
		}

		protected void DisposeKeyType()
		{
			if (fKeyType != null)
			{
				fKeyType.Dispose();
				fKeyType = null;
			}
		}

		protected void DisposeCertHandleCryptProv()
		{
			if (fCertHandleCryptProv != null)
			{
				fCertHandleCryptProv.Dispose();
				fCertHandleCryptProv = null;
			}
		}

	}
}
