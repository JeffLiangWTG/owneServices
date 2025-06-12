using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using CargoWise.eServices.Encryption.Common;

namespace CargoWise.eServices.Encryption.Client.Decryptor
{
	public static class EhubClientDecryptor
	{
		#region Member Variables

		const string keyXmlResource = "CargoWise.eServices.Encryption.Client.Decryptor.Resources.client-private-key.xml";
		static readonly RSADecryptor ClientSideDecryptor;

		#endregion

		#region Constructor

		static EhubClientDecryptor()
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(keyXmlResource))
			{
				Contract.Assert(stream != null);

				using (var reader = new StreamReader(stream))
				{
					ClientSideDecryptor = new RSADecryptor(reader.ReadToEnd());
				}
			}
		}

		#endregion

		#region Methods

		public static string Decrypt(string text)
		{
			return ClientSideDecryptor.Decrypt(text);
		}

		public static byte[] DecryptBinary(byte[] data)
		{
			return ClientSideDecryptor.DecryptBinary(data);
		}

		#endregion
	}
}
