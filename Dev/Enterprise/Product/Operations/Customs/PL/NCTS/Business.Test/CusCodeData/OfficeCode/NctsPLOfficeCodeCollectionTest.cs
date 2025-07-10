using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(NctsPLOfficeCodeCollection))]
sealed class NctsPLOfficeCodeCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var parent = Factory.New<NctsHeader>();
		parent.SetMovementType(NctsMovementType.Codes.Departure);
		return new NctsPLOfficeCodeCollection(parent.MovementHeader);
	}
}
