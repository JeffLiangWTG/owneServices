using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	public abstract class CusInBondHeaderDataContextManagerTest<TInBondManager, THeader, TBill, TMoveHeader, TMoveDetail, TContainer, TCommodity> : ShipmentDataContextManagerTestCase<TInBondManager, THeader> where THeader : Customs.Business.CusInBondHeader where TBill : Customs.Business.CusInBondBill where TMoveHeader : CusInBondMoveHeader where TMoveDetail : CusInBondMoveDetail where TContainer : Customs.Business.CusInBondContainer where TCommodity : Customs.Business.CusInBondCargoDesc where TInBondManager : InBondDataContextManager<THeader, TBill, TMoveHeader, TMoveDetail, TContainer, TCommodity>, new()
	{
		public abstract void TestImportInBond();
		protected override RecipientRoleType[] SupportedRecipientRoleTypes => System.Array.Empty<RecipientRoleType>();

		protected override bool ManagerChecksDataTargetToImport => true;
	}
}
