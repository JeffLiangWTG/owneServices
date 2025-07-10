using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using NUnit.Framework;
using static CargoWise.RefDbRepo.BEReferenceData.Business.MeasureMappingProvider;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	[TestFixture]
	sealed class MeasureMappingProviderTest
	{
		public void GetPreferences()
		{
			Assert.Throws<NotImplementedException>(() => measureMappingProvider.ConvertPreferences(new string[] { }, "", new string[] { }));
		}

		public void GetRateCode()
		{
			Assert.Throws<NotImplementedException>(() => measureMappingProvider.ConvertRateCode("", null));
		}

		[TestCase(null, "")]
		[TestCase("", "")]
		[TestCase("Unknown", "")]
		[TestCase("BAO", MeasureHelper.ConditionClass.Rate)]
		public void MeasureTypeMapping_ConditionClass(string measureType, string expectedClass)
		{
			var actual = measureTypeHelper.GetConditionClass(measureType);

			Assert.That(actual, Is.EqualTo(expectedClass), $"MeasureType: {measureType}");
		}

		[TestCase(null, "")]
		[TestCase("", "")]
		[TestCase("Unknown", "")]
		[TestCase("BAO", "EXC")]
		[TestCase("BCE", "MSC")]
		[TestCase("BVH", "LEV")]
		[TestCase("BIR", "INT")]
		[TestCase("MSI", "DTY")]
		public void MeasureTypeMapping_RateType(string measureType, string expectedRateType)
		{
			var actual = measureTypeHelper.GetRateType(measureType);

			Assert.That(actual, Is.EqualTo(expectedRateType), $"MeasureType: {measureType}");
		}

		[TestCase(null, "")]
		[TestCase("", "")]
		[TestCase("Unknown", "")]
		[TestCase("BAO", "100")]
		[TestCase("BAF", "199")]
		[TestCase("BAS", "200")]
		[TestCase("BAP", "299")]
		[TestCase("BCE", "300")]
		[TestCase("BCR", "400")]
		[TestCase("BRC", "400")]
		[TestCase("BVH", "500")]
		[TestCase("BIR", "801")]
		[TestCase("BNI", "802")]
		[TestCase("MSI", "A00")]
		[TestCase("BRP", "")]
		public void MeasureTypeMapping_RateCode(string measureType, string expectedRateCode)
		{
			var actual = measureTypeHelper.GetRateCode(measureType);

			Assert.That(actual, Is.EqualTo(expectedRateCode), $"MeasureType: {measureType}");
		}

		[Test]
		public void MeasureTypeMapping_MappingCounts()
		{
			var mappings = measureMappingProvider.GetMeasureTypeMappings();

			Assert.That(mappings.Count, Is.EqualTo(11));

			Assert.That(mappings.Where(x => x.Value.ConditionClass == MeasureHelper.ConditionClass.Rate).Count(), Is.EqualTo(11), "ConditionClass Rate");
			Assert.That(mappings.Where(x => x.Value.ConditionClass == MeasureHelper.ConditionClass.Vat).Count(), Is.EqualTo(0), "ConditionClass Vat");
			Assert.That(mappings.Where(x => x.Value.ConditionClass == MeasureHelper.ConditionClass.Control).Count(), Is.EqualTo(0), "ConditionClass Control");
			Assert.That(mappings.Where(x => x.Value.ConditionClass == MeasureHelper.ConditionClass.Class).Count(), Is.EqualTo(0), "ConditionClass Class");

			Assert.That(mappings.Where(x => string.IsNullOrEmpty(x.Value.RateType)).Count(), Is.EqualTo(0), "RateType Empty");
			Assert.That(mappings.Where(x => x.Value.RateType == RateTypes.Duty).Count(), Is.EqualTo(1), "RateType Duty");
			Assert.That(mappings.Where(x => x.Value.RateType == RateTypes.AntiDumping).Count(), Is.EqualTo(0), "RateType AntiDumping");
			Assert.That(mappings.Where(x => x.Value.RateType == RateTypes.Countervailing).Count(), Is.EqualTo(0), "RateType Countervailing");
			Assert.That(mappings.Where(x => x.Value.RateType == RateTypes.Security).Count(), Is.EqualTo(0), "RateType Security");
			Assert.That(mappings.Where(x => x.Value.RateType == RateTypes.Excises).Count(), Is.EqualTo(4), "RateType Excises");
			Assert.That(mappings.Where(x => x.Value.RateType == RateTypes.Levies).Count(), Is.EqualTo(1), "RateType Levies");
			Assert.That(mappings.Where(x => x.Value.RateType == RateTypes.Export).Count(), Is.EqualTo(0), "RateType Export");
			Assert.That(mappings.Where(x => x.Value.RateType == RateTypes.Retribution).Count(), Is.EqualTo(3), "RateType Retribution");
			Assert.That(mappings.Where(x => x.Value.RateType == RateTypes.Interest).Count(), Is.EqualTo(2), "RateType Interest");
		}

		[TestCase("Reduced rate/ Parking Rate 12%", 12, MeasureHelper.ConditionClass.Vat, VatCodesAndValues.ReducedParkingCode)]
		[TestCase("Standard rate 21%", 21, MeasureHelper.ConditionClass.Vat, VatCodesAndValues.StandardCode)]
		[TestCase("Zero rate or Exemption 0%", 0, MeasureHelper.ConditionClass.Vat, VatCodesAndValues.ZeroRatedCode)]
		[TestCase("Reduced Rate/ Standard Rate 6%", 6, MeasureHelper.ConditionClass.Vat, VatCodesAndValues.ReducedCode)]
		[TestCase("Other", 37, MeasureHelper.ConditionClass.Vat, "")]
		[TestCase("Not Vat", 20, MeasureHelper.ConditionClass.Rate, "")]
		[TestCase("No value", null, MeasureHelper.ConditionClass.Vat, "")]
		public void GetVatCode(string descrip, decimal? value, string conClass, string expectedCode)
		{
			var measure = new Measure
			{
				ConditionClass = conClass,
			}.SetComponents(new List<MeasureComponent> { new MeasureComponent { DutyAmount = value, HJID = "1" } });

			Assert.That(measureMappingProvider.GetVatCode(measure), Is.EqualTo(expectedCode), descrip);
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			measureMappingProvider = new MeasureMappingProvider();
			measureTypeHelper = new MeasureTypeHelper(measureMappingProvider);
		}

		IMeasureMappingProvider measureMappingProvider;
		MeasureTypeHelper measureTypeHelper;
	}
}
