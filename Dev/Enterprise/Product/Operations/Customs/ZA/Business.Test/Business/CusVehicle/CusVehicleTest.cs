using System;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing;

[TestedType(typeof(CusVehicle))]
public class CusVehicleTest : Customs.Business.Testing.CusVehicleAbstractTest
{
	protected override Type ExpectedLookupsType => typeof(CusVehicleLookups);

	protected override Type ExpectedValidationType => typeof(CusVehicleValidation);
}
