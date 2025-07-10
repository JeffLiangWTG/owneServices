using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	class USInBondMoveHeaderOperationalActionExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestGetInBondMovementIdLink()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_JobReference = "INB00001";
			var movementHeader = header.MovementHeader;
			movementHeader.InBondNumber = "12345678";
			Factory.Save();

			var moveHeader = Factory.Load<USInBondMoveHeader>(movementHeader.PK);
			var controllerIdLink = moveHeader.GetInBondMovementIdLink();
			AssertEquals("INB00001 (12345678)", controllerIdLink.Text);
			AssertEquals(moveHeader.PK, controllerIdLink.PK);
			AssertEquals(ControllerIDs.Customs.US.InBondMoveHeader, controllerIdLink.Controller);
		}
	}
}
