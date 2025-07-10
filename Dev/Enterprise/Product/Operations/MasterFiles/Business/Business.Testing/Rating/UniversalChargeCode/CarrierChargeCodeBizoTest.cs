using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Rating.Test;

[TestedType(typeof(CarrierChargeCodeBizo))]
public class CarrierChargeCodeBizoTest : MappedChargeCodeBizoTest
{
	#region Implementation

	protected override BusinessObject GetNewBusinessObject() => new CarrierChargeCodeBizo(Factory);

	#endregion
}
