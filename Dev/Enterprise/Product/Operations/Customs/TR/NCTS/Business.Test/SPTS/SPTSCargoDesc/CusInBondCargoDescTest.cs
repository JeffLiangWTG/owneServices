using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(SPTSCargoDesc))]
	public class CusInBondCargoDescTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewSPTSCargoDesc(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewSPTSCargoDesc(factory);

		SPTSCargoDesc GetNewSPTSCargoDesc(BusinessObjectFactory factory)
		{
			var header = factory.New<SPTSHeader>();
			var container = factory.New<SPTSContainer>();
			header.HeaderContainers.Add(container);
			container.BC_ParentTableCode = CusInBondHeaderSchema.Constants.Prefix;
			container.BC_ContainerNum = "Test";
			return (SPTSCargoDesc)container.Commodities.AddNew();
		}
	}
}
