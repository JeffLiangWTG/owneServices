using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusInBondCargoDesc))]
	sealed class CusInBondCargoDescTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewCusInBondCargoDesc(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewCusInBondCargoDesc(factory);

		CusInBondCargoDesc GetNewCusInBondCargoDesc(BusinessObjectFactory factory)
		{
			var header = factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var container = moveDetail.Containers.AddNew();
			return (CusInBondCargoDesc)container.Commodities.AddNew();
		}
	}
}
