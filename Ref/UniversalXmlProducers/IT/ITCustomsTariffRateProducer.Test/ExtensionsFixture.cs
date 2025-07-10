using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test
{
	[TestFixture]
	class ExtensionsFixture
	{
		[Test]
		[SetCulture("de-DE")]
		public void ToItalianShortDateString()
		{
			Assert.AreEqual("01/01/2022", new DateTime(2022, 01, 01).ToItalianShortDateString(), "ToItalianShortDateString() is culture invariant");
		}
	}
}
