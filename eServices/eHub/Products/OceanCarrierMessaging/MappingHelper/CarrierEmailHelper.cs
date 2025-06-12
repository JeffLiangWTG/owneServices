using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper
{
  public class CarrierEmailHelper
  {
    const string Issuer = "WTG-IIG";

    public virtual string GenerateToken(string secretKey, string messageType, string messageReference)
    {
      var now = DateTimeOffset.UtcNow;
      var expiration = now.AddDays(30);

      var jsonObject = new JObject
      {
        [CarrierEmailClaimNames.Expires] = string.Empty + expiration.ToUnixTimeSeconds(),
        [CarrierEmailClaimNames.MessageType] = messageType,
        [CarrierEmailClaimNames.MessageReference] = messageReference
      };
      return EncryptString(secretKey, jsonObject.ToString());
    }

    public virtual string EncryptString(string secretKey, string text)
    {
      using (var aesAlgorytm = Aes.Create())
      {
        aesAlgorytm.Key = Encoding.UTF8.GetBytes(secretKey);
        aesAlgorytm.GenerateIV();
        var iv = aesAlgorytm.IV;

        var encryptor = aesAlgorytm.CreateEncryptor(aesAlgorytm.Key, iv);

        using (var msEncrypt = new MemoryStream())
        {
          msEncrypt.Write(iv, 0, iv.Length);
          using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
          using (var swEncrypt = new StreamWriter(csEncrypt))
          {
            swEncrypt.Write(text);
          }

          return Convert.ToBase64String(msEncrypt.ToArray());
        }
      }
    }

    public virtual string DecryptString(string secretKey, string text)
    {
      using (var aesAlgorytm = Aes.Create())
      {
        var fullCipher = Convert.FromBase64String(text);
        aesAlgorytm.Key = Encoding.UTF8.GetBytes(secretKey);
        var iv = new byte[aesAlgorytm.BlockSize / 8];
        var cipher = new byte[fullCipher.Length - iv.Length];

        Array.Copy(fullCipher, iv, iv.Length);
        Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

        var decryptor = aesAlgorytm.CreateDecryptor(aesAlgorytm.Key, iv);

        using (var msDecrypt = new MemoryStream(cipher))
        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
        using (var srDecrypt = new StreamReader(csDecrypt))
        {
          return srDecrypt.ReadToEnd();
        }
      }
    }

    struct CarrierEmailClaimNames
    {
      public const string MessageType = "MessageType";
      public const string MessageReference = "MessageReference";
      public const string Expires = "Expires";
    }
  }
}
