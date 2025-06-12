using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using CargoWise.eServices.Encryption.Common;

namespace CargoWise.eServices.Encryption.Server.Decryptor
{
	public static class EhubServerDecryptor
	{
		#region Member Variables

		const string keyXmlResource = "CargoWise.eServices.Encryption.Server.Decryptor.Resources.server-private-key.xml";
		static readonly RSADecryptor RSADecryptor;

		#endregion

		#region Constructor

		static EhubServerDecryptor()
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(keyXmlResource))
			{
				Contract.Assert(stream != null);

				using (var reader = new StreamReader(stream))
				{
					RSADecryptor = new RSADecryptor(reader.ReadToEnd());
				}
			}
		}

		#endregion

		#region Methods

		public static string Decrypt(string text)
		{
			return RSADecryptor.Decrypt(text);
		}

		public static byte[] DecryptBinary(byte[] data)
		{
			return RSADecryptor.DecryptBinary(data);
		}

		#endregion
	}
}
