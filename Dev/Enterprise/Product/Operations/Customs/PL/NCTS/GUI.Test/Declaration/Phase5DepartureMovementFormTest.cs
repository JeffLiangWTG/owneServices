using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.PL.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.PL.NCTS.GUI.Testing;

[TestedType(typeof(Phase5DepartureMovementForm))]
sealed class Phase5DepartureMovementFormTest : EU.NCTS.GUI.Testing.Phase5DepartureMovementFormAbstractTest<NctsHeader>
{
	public void TestMessagesUserControlType()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;

		using (var form = new Phase5DepartureMovementForm(nctsHeader))
		{
			var tabPage = form.FindSingle<ZTabPage>("MessagesTabPage");
			tabPage.Show();
			var messagesTabDynamicUserControl = tabPage.FindSingle<ZDynamicControlCreationUserControl>("MessagesTabDynamicUserControl");
			AssertEquals(typeof(MessagesTabUserControl), messagesTabDynamicUserControl.UserControlType);
		}
	}

	protected override void PerformExtraNctsHeaderConfiguration(NctsHeader header)
	{
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.BH_HeaderType = NctsMovementType.Codes.Departure;
		base.PerformExtraNctsHeaderConfiguration(header);
		header.MovementHeader.GoodsLocation.Address.Address1 = "E";
	}

	protected override bool AllowHasChangesOnFormOpen => true;

	[DeveloperOnlyTest]
	public override void TestMarkAsNeedingValidationIsNotCalledWhenFormLoads()
	{
		Assert(true);
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
	}
	NctsHeader header;
}
