using System;
using System.IO;
using System.Security.Cryptography;

namespace CargoWise.RefDbRepo.Common.Utils;

public static class StreamHashCalculator
{
	public static string CalculateHash(Stream stream)
	{
		Argument.Argument.NotNull(stream, "stream could not be null");
		using var hashAlgorithm = SHA256.Create();
		stream.Position = 0;
		var hashBytes = hashAlgorithm.ComputeHash(stream);
		return BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();
	}
}
