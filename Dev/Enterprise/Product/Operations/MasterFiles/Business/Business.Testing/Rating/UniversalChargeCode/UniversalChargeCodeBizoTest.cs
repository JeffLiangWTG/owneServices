using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Rating.Test;

[TestedType(typeof(UniversalChargeCodeBizo))]
public class UniversalChargeCodeBizoTest : MappedChargeCodeBizoTest
{
	#region Implementation

	protected override BusinessObject GetNewBusinessObject() => new UniversalChargeCodeBizo(Factory);

	#endregion
}
