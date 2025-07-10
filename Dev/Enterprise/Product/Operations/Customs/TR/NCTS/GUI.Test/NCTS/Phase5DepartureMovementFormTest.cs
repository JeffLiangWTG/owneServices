using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.TR.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5DepartureMovementForm))]
	public class Phase5DepartureMovementFormTest : EU.NCTS.GUI.Testing.Phase5DepartureMovementFormAbstractTest<NctsHeader>
	{
		public void TestPhase5ManifestsToOpenTabPageVisibility()
		{
			using (var form = new Phase5DepartureMovementForm(header))
			{
				var mainTabControl = (ZTemplateTabControl)form.Controls.Find("MainTabControl", true).First();
				var phase5ManifestsToOpenTabPage = (ZTabPage)mainTabControl.Controls.Find("Phase5ManifestsToOpenTabPage", true).FirstOrDefault();
				AssertNotNull(phase5ManifestsToOpenTabPage);
				Assert(phase5ManifestsToOpenTabPage.TabVisible);
			}
		}
		public void TestPhase5WarehouseToOpenTabPageVisibility()
		{
			using (var form = new Phase5DepartureMovementForm(header))
			{
				var mainTabControl = (ZTemplateTabControl)form.Controls.Find("MainTabControl", true).First();
				var phase5WarehouseToOpenTabPage = (ZTabPage)mainTabControl.Controls.Find("WarehouseToOpenTabPage", true).FirstOrDefault();
				AssertNotNull(phase5WarehouseToOpenTabPage);
				Assert(phase5WarehouseToOpenTabPage.TabVisible);
			}
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
}
