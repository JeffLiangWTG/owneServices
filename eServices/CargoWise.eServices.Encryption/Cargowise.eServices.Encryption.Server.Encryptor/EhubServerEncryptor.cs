using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using CargoWise.eServices.Encryption.Common;

namespace CargoWise.eServices.Encryption.Server.Encryptor
{
	public static class EhubServerEncryptor
	{
		#region Member Variables

		const string publicKeyXmlResource = "CargoWise.eServices.Encryption.Server.Encryptor.Resources.client-public-key.xml";
		static readonly RSAEncryptor RSAEncryptor;

		#endregion

		#region Constructor

		static EhubServerEncryptor()
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(publicKeyXmlResource))
			{
				Contract.Assert(stream != null);

				using (var reader = new StreamReader(stream))
				{
					RSAEncryptor= new RSAEncryptor(reader.ReadToEnd());
				}
			}
		}

		#endregion

		#region Methods

		public static string Encrypt(string text)
		{
			return RSAEncryptor.Encrypt(text);
		}

		public static byte[] EncryptBinary(byte[] data)
		{
			return RSAEncryptor.EncryptBinary(data);
		}

		#endregion
	}
}
