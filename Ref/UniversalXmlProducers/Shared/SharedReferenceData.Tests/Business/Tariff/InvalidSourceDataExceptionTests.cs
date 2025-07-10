using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Tests
{
	[TestFixture]
	sealed class InvalidSourceDataExceptionTests
	{
		[Test]
		public void Message()
		{
			var ex1 = new InvalidSourceDataException("DATA", "AWE");
			Assert.That(ex1.Message, Is.EqualTo("One or more errors were identified with the source data, this may need to be raised with DATA. Please assign this issue to AWE."));
			var ex2 = new InvalidSourceDataException("PROV", "SOME", new List<string> { "Error1", "Error2" });
			Assert.That(ex2.Message, Is.EqualTo(
				"One or more errors were identified with the source data, this may need to be raised with PROV. Please assign this issue to SOME." + Environment.NewLine +
				"Error1" + Environment.NewLine +
				"Error2"
			));
		}
	}
}
