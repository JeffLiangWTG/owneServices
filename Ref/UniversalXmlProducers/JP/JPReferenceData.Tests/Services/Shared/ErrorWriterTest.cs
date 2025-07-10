using System;
using System.IO;
using CargoWise.RefDbRepo.JPReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class ErrorWriterTest
	{
		[Test]
		public void TestWriteError()
		{
			ErrorWriter.WriteError(Message);
			var errors = stringWriter?.ToString();
			Assert.That(errors, Contains.Substring($"[ERROR]{Message}"));

		}

		[Test]
		public void TestWriteException()
		{
			ErrorWriter.WriteException(new Exception(Message));
			var errors = stringWriter?.ToString();
			Assert.That(errors, Contains.Substring("[ERROR][EXCEPTION]"));
			Assert.That(errors, Contains.Substring(Message));
		}

		[SetUp]
		public void Setup()
		{
			stringWriter = new StringWriter();
			Console.SetError(stringWriter);
		}

		StringWriter stringWriter;
		const string Message = "Hello";
	}
}
