using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CargoWise.eHub.Shared.Crypto
{
	public class CmsHelpers
	{
		public static byte[] ComputeSignature(byte[] content, byte[] keyData, string password)
		{
			return ComputeSignature(content, keyData, password, new List<Pkcs9AttributeObject>(), true);
		}

		public static byte[] ComputeSignature(byte[] content, byte[] keyData, string password, IEnumerable<Pkcs9AttributeObject> attributes)
		{
			return ComputeSignature(content, keyData, password, attributes, true);
		}

		public static byte[] ComputeSignature(byte[] content, byte[] keyData, string password, bool detachSignature)
		{
			return ComputeSignature(content, keyData, password, new List<Pkcs9AttributeObject>(), detachSignature);
		}

		public static byte[] ComputeSignature(byte[] content, byte[] keyData, string password, IEnumerable<Pkcs9AttributeObject> attributes, bool detachSignature = true, Oid digestAlgorithm = null)
		{
			X509Certificate2Collection certs = new X509Certificate2Collection();
			certs.Import(keyData, password, X509KeyStorageFlags.EphemeralKeySet);
			var clientCert = certs.Cast<X509Certificate2>().LastOrDefault();

			if (clientCert == null)
				throw new InvalidOperationException("Key does not contain certificate for signing usage.");

			ContentInfo contentInfo = new ContentInfo(content);
			SignedCms signedCms = new SignedCms(contentInfo, detachSignature);
			CmsSigner signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, clientCert) { IncludeOption = X509IncludeOption.EndCertOnly };
			if (digestAlgorithm != null)
			{
				signer.DigestAlgorithm = digestAlgorithm;
			}

			foreach (var att in attributes)
			{
				signer.SignedAttributes.Add(att);
			}
			signedCms.ComputeSignature(signer);
			return signedCms.Encode();
		}

		public static string ComputeSignatureBase64(byte[] content, byte[] keyData, string password, int chunkSize)
		{
			return ComputeSignatureBase64(content, keyData, password, chunkSize, true);
		}

		public static string ComputeSignatureBase64(byte[] content, byte[] keyData, string password, int chunkSize, bool detachSignature = true)
		{
			var signatureBase64 = Convert.ToBase64String(ComputeSignature(content, keyData, password, detachSignature));

			if (chunkSize > 0)
			{
				var signatureChars = signatureBase64.ToCharArray();
				var sb = new StringBuilder();
				for (int i = 0; i < signatureChars.Length; i += chunkSize)
					sb.Append(signatureChars, i, Math.Min(chunkSize, signatureChars.Length - i)).AppendLine();
				return sb.ToString();
			}
			else
				return signatureBase64;
		}

		public static byte[] EncryptMessage(byte[] message, byte[] keyData, AlgorithmIdentifier encryptionAlgorithm = null)
		{
			X509Certificate2Collection certs = new X509Certificate2Collection();
			certs.Import(keyData);
			var recipientCert = certs.Cast<X509Certificate2>().LastOrDefault();

			if (recipientCert == null)
				throw new InvalidOperationException("Key does not contain certificate for encryption usage.");

			ContentInfo contentInfo = new ContentInfo(message);
			EnvelopedCms envelopedCms = encryptionAlgorithm == null
				? new EnvelopedCms(contentInfo)
				: new EnvelopedCms(contentInfo, encryptionAlgorithm);

			CmsRecipient cmsRecipient = new CmsRecipient(SubjectIdentifierType.SubjectKeyIdentifier, recipientCert);
			envelopedCms.Encrypt(cmsRecipient);
			return envelopedCms.Encode();
		}

		public static byte[] DecryptMessage(byte[] message, byte[] keyData, string password)
		{
			X509Certificate2Collection certs = new X509Certificate2Collection();
			certs.Import(keyData, password, X509KeyStorageFlags.EphemeralKeySet);

			EnvelopedCms envelopedCms = new EnvelopedCms();
			envelopedCms.Decode(message);
			envelopedCms.Decrypt(certs);
			return envelopedCms.ContentInfo.Content;
		}

		public static SubjectIdentifier GetRecipientIdentifier(byte[] encryptedBytes)
		{
			var envelopedCms = new EnvelopedCms();
			envelopedCms.Decode(encryptedBytes);
			return envelopedCms.RecipientInfos[0].RecipientIdentifier;
		}

		public static SubjectIdentifier GetSignerIdentifier(byte[] signature)
		{
			var signedCms = new SignedCms();
			signedCms.Decode(signature);
			return signedCms.SignerInfos[0].SignerIdentifier;
		}

		public static void VerifySignature(byte[] signedData, byte[] signature, byte[] signerCert)
		{
			var contentInfo = new ContentInfo(signedData);
			var signedCms = new SignedCms(contentInfo, true);
			signedCms.Decode(signature);
			var x509CertCollection = new X509Certificate2Collection();
			x509CertCollection.Import(signerCert);
			signedCms.CheckSignature(x509CertCollection, true);
		}

		public static byte[] ExtractDataFromSignature(byte[] signature)
		{
			var cms = new SignedCms();
			cms.Decode(signature);
			return cms.ContentInfo.Content;
		}
	}
}
