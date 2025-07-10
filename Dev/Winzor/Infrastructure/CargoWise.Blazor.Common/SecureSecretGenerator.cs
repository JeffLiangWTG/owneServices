using System;
using System.Security.Cryptography;

namespace CargoWise.Blazor.Common;

public interface ISecureSecretGenerator
{
	string Generate();
}

public class SecureSecretGenerator : ISecureSecretGenerator
{
	/// <summary>
	/// Generates a random BASE64 string using recommend NET 6 best practices
	/// </summary>
	/// <returns></returns>
	public string Generate() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}