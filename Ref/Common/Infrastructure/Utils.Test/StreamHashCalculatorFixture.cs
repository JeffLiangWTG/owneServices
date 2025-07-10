using System;
using System.IO;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test;

[TestFixture]
class StreamHashCalculatorFixture
{
	[Test]
	public void TestCalculateHashIfFileDoesNotExist()
	{
		Assert.Throws<ArgumentNullException>(() => StreamHashCalculator.CalculateHash(null));
	}

	[Test]
	public void TestHashesAreSameIfSameFile()
	{
		var testDacPacFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles\\model1.xml");
		using var testDacPacFileStream = File.OpenRead(testDacPacFile);
		var hash1 = StreamHashCalculator.CalculateHash(testDacPacFileStream);
		var hash2 = StreamHashCalculator.CalculateHash(testDacPacFileStream);
		Assert.That(hash1, Is.EqualTo(hash2));
	}

	[Test]
	public void TestHashesAreDifferentIfDifferentFile()
	{
		var testDacPacFile1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles\\model1.xml");
		var testDacPacFile2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles\\model2.xml");
		using var testDacPacFileStream1 = File.OpenRead(testDacPacFile1);
		using var testDacPacFileStream2 = File.OpenRead(testDacPacFile2);
		var hash1 = StreamHashCalculator.CalculateHash(testDacPacFileStream1);
		var hash2 = StreamHashCalculator.CalculateHash(testDacPacFileStream2);
		Assert.That(hash1, Is.Not.EqualTo(hash2));
	}
}
