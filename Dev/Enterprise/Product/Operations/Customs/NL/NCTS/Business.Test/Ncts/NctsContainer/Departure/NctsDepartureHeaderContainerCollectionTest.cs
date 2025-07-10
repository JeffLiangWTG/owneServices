using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(NctsDepartureHeaderContainerCollection))]
public sealed class NctsDepartureHeaderContainerCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(NctsMovementType.Codes.Departure);
		return (NctsDepartureHeaderContainerCollection)header.DepartureHeaderContainers;
	}
}
