using System;
using CargoWise.eServices.Encryption.Server.Decryptor;

namespace CargoWise.eHub.Products.Shared.GLSHK.Maps.Helpers
{
	public class DecryptionHelper
	{
		public static string Decrypt(string encrypted)
		{
			return EhubServerDecryptor.Decrypt(encrypted);
		}
	}
}
