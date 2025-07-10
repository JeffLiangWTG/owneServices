using System;
using CargoWise.ComponentModel;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(CusEngine))]
	class CusEngineTest : CusEngineAbstractTest
	{
		public void TestCEG_CapacityCC()
		{
			var engine = Factory.New<CusEngine>();
			AssertHasCustomAttribute<DecimalPlacesAttribute>(engine.GetType(), "CEG_CapacityCC", false, attr => attr.DecimalPlaces == 0);
		}

		public void TestCEG_CapacityHP()
		{
			var engine = Factory.New<CusEngine>();
			AssertHasCustomAttribute<DecimalPlacesAttribute>(engine.GetType(), "CEG_CapacityHP", false, attr => attr.DecimalPlaces == 0);
		}

		public void TestCEG_EngineType()
		{
			var engine = Factory.New<CusEngine>();
			AssertHasCustomAttribute<ListAttribute>(engine.GetType(), "CEG_EngineType", false, attr => attr.ListDataSourceMember == "Lookups.EngineTypeList");
			AssertHasCustomAttribute<MaxLengthAttribute>(engine.GetType(), "CEG_EngineType", false, attr => attr.MaxLength == 1);
		}

		protected override Type ExpectedLookupsType => typeof(CusEngineLookups);
		protected override Type ExpectedValidationType => typeof(CusEngineValidation);
	}
}
