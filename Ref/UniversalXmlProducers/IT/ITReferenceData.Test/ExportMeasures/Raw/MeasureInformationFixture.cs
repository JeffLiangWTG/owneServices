using System;
using CargoWise.RefDbRepo.ITReferenceData.Business.ExportMeasures;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.ExportMeasures
{
	[TestFixture]
	sealed class MeasureInformationFixture
	{

		[Test]
		public void Constructor()
		{
			Assert.Throws<ArgumentNullException>(() => new MeasureInformation(tradeGroup: null, additionalCode: AdditionalCode), "When tradeGroup is null");
			Assert.Throws<ArgumentNullException>(() => new MeasureInformation(tradeGroup: TradeGroup, additionalCode: null), "When additionalCode is null");
		}

		[Test]
		public void Properties()
		{
			var measureInformation = new MeasureInformation(TradeGroup, AdditionalCode);

			Assert.AreEqual(TradeGroup, measureInformation.TradeGroup, "TradeGroup Value");
			Assert.AreEqual(AdditionalCode, measureInformation.AdditionalCode, "AdditionalCode Value");
		}

		const string TradeGroup = "ERGA OMNES";
		const string AdditionalCode = "U128";
	}
}
