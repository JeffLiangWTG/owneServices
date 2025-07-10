using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(CusEngine))]
sealed class CusEngineTest : CusEngineAbstractTest
{
	public void TestCEG_EngineTypeMaxLength() => AssertEquals(2, Factory.New<CusEngine>().CEG_EngineTypeInfo.MaxLength);
	public void TestCEG_EngineNumberMaxLength() => AssertEquals(17, Factory.New<CusEngine>().CEG_EngineNumberInfo.MaxLength);

	public void TestCaptions()
	{
		var engine = Factory.New<CusEngine>();
		CombineAssertions(() =>
		{
			AssertEquals("CEG_EngineType caption", "Fuel Type", DataBoundResourceStrings.GetDataForProperty(engine.CEG_EngineTypeInfo).Caption);
			AssertEquals("CEG_EngineNumber caption", "Engine No.", DataBoundResourceStrings.GetDataForProperty(engine.CEG_EngineNumberInfo).Caption);
			AssertEquals("CEG_CapacityCC caption", "Capacity", DataBoundResourceStrings.GetDataForProperty(engine.CEG_CapacityCCInfo).Caption);
		});
	}

	public void TestSupportsClone()
	{
		var engine = Factory.New<CusEngine>();
		Assert("PL Engine supports clone", engine.SupportsClone());
	}

	protected override Type ExpectedLookupsType => typeof(CusEngineLookups);
	protected override Type ExpectedValidationType => typeof(CusEngineValidation);
}
