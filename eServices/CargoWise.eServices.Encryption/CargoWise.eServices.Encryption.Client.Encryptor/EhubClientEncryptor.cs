using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using CargoWise.eServices.Encryption.Common;

namespace CargoWise.eServices.Encryption.Client.Encryptor
{
	public static class EhubClientEncryptor
	{
		#region Member Variables

		const string publicKeyXmlResource = "CargoWise.eServices.Encryption.Client.Encryptor.Resources.server-public-key.xml";
		private static readonly RSAEncryptor clientSideEncryptor;

		#endregion

		#region Constructor

		static EhubClientEncryptor()
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(publicKeyXmlResource))
			{
				Contract.Assert(stream != null);

				using (var reader = new StreamReader(stream))
				{
					clientSideEncryptor = new RSAEncryptor(reader.ReadToEnd());
				}
			}
		}

		#endregion

		#region Methods

		public static string Encrypt(string text)
		{
			return clientSideEncryptor.Encrypt(text);
		}

		public static byte[] EncryptBinary(byte[] data)
		{
			return clientSideEncryptor.EncryptBinary(data);
		}

		#endregion
	}
}
