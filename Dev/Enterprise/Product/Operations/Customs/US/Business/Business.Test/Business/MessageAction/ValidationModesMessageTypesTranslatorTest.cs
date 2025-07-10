using System;
using Enterprise.Customs.Common.US;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ValidationModesMessageTypesTranslatorTest : TestCase
	{
		public void TestGetValidationModesRelatedTo()
		{
			AssertEquals("Border Cargo Release message sets Cargo Release validation", ValidationModes.CargoRelease, ValidationModesMessageTypesTranslator.GetValidationModesRelatedTo(ImportMessageStatusList.MessageType.BorderCargoRelease));
			AssertEquals(ValidationModes.CargoRelease, ValidationModesMessageTypesTranslator.GetValidationModesRelatedTo(ImportMessageStatusList.MessageType.CargoRelease));
			AssertEquals(ValidationModes.EntrySummary, ValidationModesMessageTypesTranslator.GetValidationModesRelatedTo(ImportMessageStatusList.MessageType.EntrySummary));
			AssertEquals(ValidationModes.CargoRelease, ValidationModesMessageTypesTranslator.GetValidationModesRelatedTo(ImportMessageStatusList.MessageType.ACECargoRelease));
			AssertExceptionThrown<NotSupportedException>(() => ValidationModesMessageTypesTranslator.GetValidationModesRelatedTo(ImportMessageStatusList.MessageType.InBondUpdate));
		}
	}
}
