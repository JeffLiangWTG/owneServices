using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Dat.Integration;

namespace CargoWise.RefDbRepo.Deployment
{
	internal class DATCrypto
	{
		static SecureStorage secureStorage;

		public static string Decrypt(string encryptPassword, bool ignoreEscape = false)
		{
			if (secureStorage == null)
			{
				secureStorage = new SecureStorage();
			}
			var decryptedPassword = secureStorage.Decrypt(encryptPassword);
			return ignoreEscape ? decryptedPassword : SecurityElement.Escape(decryptedPassword);
		}
	}
}
