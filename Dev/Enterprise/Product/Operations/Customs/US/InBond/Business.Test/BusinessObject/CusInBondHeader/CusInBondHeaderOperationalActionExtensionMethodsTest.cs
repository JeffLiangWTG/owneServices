using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondHeaderOperationalActionExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestGetInBondHeaderIdLink()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_JobReference = "INB00001";
			AssertEquals(ControllerIDs.Customs.US.InBond, header.GetInBondHeaderIdLink().Controller);
			var declaration = Factory.New<JobDeclaration>();
			header.BH_ParentID = declaration.PK;
			header.BH_ParentTableCode = declaration.TablePrefix;
			AssertEquals(ControllerIDs.Customs.JobDeclaration, header.GetInBondHeaderIdLink().Controller);
			var shipment = Factory.New<CommonShipment>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			AssertEquals(ControllerIDs.JobShipment, header.GetInBondHeaderIdLink().Controller);
		}
	}
}
