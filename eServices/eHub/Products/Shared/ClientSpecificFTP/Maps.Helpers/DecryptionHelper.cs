using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.eServices.Encryption.Server.Decryptor;

namespace CargoWise.eHub.Products.Shared.ClientSpecificFTP.Maps.Helpers
{
    public class DecryptionHelper
    {
        public static string Decrypt(string encrypted)
        {
            return EhubServerDecryptor.Decrypt(encrypted);
        }
    }
}
