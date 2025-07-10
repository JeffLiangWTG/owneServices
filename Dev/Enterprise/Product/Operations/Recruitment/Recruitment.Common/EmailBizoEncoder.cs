using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

namespace Enterprise.Recruitment.Common
{
	public static class EmailBizoEncoder
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "string interpolating is not a code smell")]
		public const string UserInfoText = " Please enter your message above this line. Please do not delete this message. ";

		public static string GetMailToForBizo(BusinessObject bo, string email)
		{
			var body = $"\r\n###{UserInfoText}###{EncodeBizo(bo)}###\r\n";
			return $"mailto:{email}?body={body}";
		}

		const byte Prime = 7;
		const int HashLength = 1;
		const int MinTableCodeLength = 2;
		const int GuidLength = 16;

		public static BusinessObject DecodeBizo(BusinessObjectFactory factory, string message)
		{
			var unpacked = Decode(message);
			if (unpacked != null)
			{
				var (prefix, pk) = unpacked.Value;

				return factory.Load(prefix, pk);
			}

			return null;
		}

		public static string EncodeBizo(BusinessObject bo)
			=> Encode(bo.TablePrefix, bo.PK.ToGuid());

		static byte CalculateHash(IEnumerable<byte> bytes)
			=> unchecked(bytes.Aggregate((result, b) => (byte)((result * Prime) + b)));

		static string Encode(string tableCode, Guid pk)
			=> ToUrlSafeBase64(Convert.ToBase64String(Pack(tableCode, pk)));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Regex strings")]
		public const string RegexString = @"[A-Za-z0-9-_]+";

		static (string tableCode, Guid pk)? Decode(string encoded)
		{
			if (string.IsNullOrEmpty(encoded) || !Regex.IsMatch(encoded, $"^{RegexString}$"))
			{
				return null;
			}

			byte[] blob;
			try
			{
				blob = Convert.FromBase64String(FromUrlSafeBase64(encoded));
			}
			catch (FormatException) // Invalid Base64 string, eg 'a'
			{
				return null;
			}

			return Unpack(blob);
		}

		static byte[] Pack(string tableCode, Guid pk)
		{
			var result = new List<byte>();
			result.AddRange(pk.ToByteArray());
			result.AddRange(Encoding.ASCII.GetBytes(tableCode));
			result.Insert(0, CalculateHash(result));

			return result.ToArray();
		}

		static (string tableCode, Guid pk)? Unpack(byte[] buffer)
		{
			if (buffer.Length < (HashLength + MinTableCodeLength + GuidLength))
			{
				return null;
			}

			var hash = buffer[0];
			if (hash != CalculateHash(buffer.Skip(1)))
			{
				return null;
			}

			var guid = new Guid(buffer.Skip(HashLength).Take(GuidLength).ToArray());
			var tableCode = Encoding.ASCII.GetString(buffer.Skip(HashLength + GuidLength).ToArray());

			return (tableCode, guid);
		}

		#region RFC 4648, URL Safe base64 encoding. It's basically regular base64 with a couple of characters swapped and no padding

		static string ToUrlSafeBase64(string regularBase64)
			=> replaceChars.Aggregate(regularBase64.TrimEnd(PaddingChar), (e, p) => e.Replace(p.original, p.urlsafe));

		static string FromUrlSafeBase64(string urlSafeBase64)
		{
			var base64 = replaceChars.Aggregate(urlSafeBase64, (e, p) => e.Replace(p.urlsafe, p.original));
			switch (base64.Length % 4)
			{
				case 2:
					base64 += new string(PaddingChar, 2);
					break;
				case 3:
					base64 += new string(PaddingChar, 1);
					break;
			}

			return base64;
		}

		readonly static ImmutableArray<(string original, string urlsafe)> replaceChars = ImmutableArray.Create(("+", "-"), ("/", "_"));
		const char PaddingChar = '=';

		#endregion
	}
}
