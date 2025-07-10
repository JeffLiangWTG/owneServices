using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

namespace CargoWise.Blazor.Common.Test;

public class SecureSharedSecretGeneratorTests
{
	[Test]
	public void GenerateCreatesARandom64BitBase64String()
	{
		// This doesn't truly assert that the value is random, we rely on the CLR implementation for that
		// This merely ensures that the length and performance are sufficient
		// And that someone can't replace it with a hardcoded string
		var generator = new SecureSecretGenerator();
		var results = new Dictionary<string, string>();

		var stopwatch = Stopwatch.StartNew();

		for (var i = 0; i < 10000; i++)
		{
			var secret = generator.Generate();
			Assert.That(secret.Length, Is.EqualTo(88));
			Assert.That(results.TryGetValue(secret, out _), Is.False);
			results[secret] = secret;
		}

		stopwatch.Stop();
		Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromMilliseconds(100)));
	}
}