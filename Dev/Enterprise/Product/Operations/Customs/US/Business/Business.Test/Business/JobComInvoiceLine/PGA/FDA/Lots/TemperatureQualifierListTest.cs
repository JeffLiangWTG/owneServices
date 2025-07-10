using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	class TemperatureQualifierListTest : TestCase
	{
		public void TestIsACECargoReleaseType()
		{
			AssertEquals(true, TemperatureQualifierList.AreDetailsMandatory(TemperatureQualifierList.Codes.Frozen));
			AssertEquals(true, TemperatureQualifierList.AreDetailsMandatory(TemperatureQualifierList.Codes.Refrigerated));
			AssertEquals(true, TemperatureQualifierList.AreDetailsMandatory(TemperatureQualifierList.Codes.DryIce));
			AssertEquals(false, TemperatureQualifierList.AreDetailsMandatory(TemperatureQualifierList.Codes.Ambient));
			AssertEquals(false, TemperatureQualifierList.AreDetailsMandatory(TemperatureQualifierList.Codes.Flashpoint));
		}
	}
}
