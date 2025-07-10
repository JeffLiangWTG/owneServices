using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Message.PuescService.Credentials;

sealed class DigestBuilder : IDigestBuilder
{
	const string CreatedDateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

	public (string Nonce, string Digest, string Created) Build(string hashedPassword, Guid? nonce = null, DateTime? creationDateTime = null)
	{
		var guidString = (nonce ?? Guid.NewGuid()).ToString();
		var nonceBytes = Encoding.UTF8.GetBytes(guidString);
		var nonceHash = ComputeHash(nonceBytes);
		var created = (creationDateTime ?? DateTime.UtcNow).ToString(CreatedDateTimeFormat, CultureInfo.InvariantCulture);
		var basedPassword = Convert.ToBase64String(HashStringToBytes(hashedPassword));
		var digestBytes = CombineArrays(nonceHash, Encoding.UTF8.GetBytes(created), Encoding.UTF8.GetBytes(basedPassword));
		var digestHash = ComputeHash(digestBytes);
		return (
			Convert.ToBase64String(nonceHash),
			Convert.ToBase64String(digestHash),
			created);
	}

#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms
	static byte[] ComputeHash(byte[] input) => SHA1.HashData(input);
#pragma warning restore CA5350 // Do Not Use Weak Cryptographic Algorithms

	static byte[] HashStringToBytes(string input)
	{
		if (input.Length % 2 != 0)
		{
			throw new ArgumentException("Password provided must be SHA-1 string, invalid length");
		}

		var bytes = new byte[input.Length / 2];
		for (var i = 0; i < bytes.Length; i++)
		{
			var hexSegment = input.Substring(i * 2, 2);
			if (!byte.TryParse(hexSegment, NumberStyles.HexNumber, null, out bytes[i]))
			{
				throw new ArgumentException($"Password provided must be SHA-1 string, invalid hex value {hexSegment}");
			}
		}
		return bytes;
	}

	static byte[] CombineArrays(params byte[][] arrays)
	{
		var result = new byte[arrays.Sum(a => a.Length)];
		var offset = 0;
		foreach (var array in arrays)
		{
			Array.Copy(array, 0, result, offset, array.Length);
			offset += array.Length;
		}
		return result;
	}
}
