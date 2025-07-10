using System;
using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	[TestedType(typeof(TRNctsMovementForm))]
	public class TRNctsMovementFormTest : EU.NCTS.GUI.Testing.NctsMovementFormAbstractTest<NctsHeader>
	{
		public void TestUserControlType()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival);
			header.BH_FTZMove = true;
			using (var form = new TRNctsMovementForm(header))
			{
				form.Show();
				var mainTabControl = (ZTemplateTabControl)form.Controls.Find("MainTabControl", true).First();
				mainTabControl.SelectTab(mainTabControl.GetTabPage("MainTabPage"));
				var declarationDetailsTabDynamicUserControl = (ZDynamicControlCreationUserControl)form.Controls.Find("DeclarationDetailsTabDynamicUserControl", true).First();
				AssertEquals(typeof(TRDeclarationDetailsTabUserControl), declarationDetailsTabDynamicUserControl.UserControlType);

				mainTabControl.SelectTab(mainTabControl.GetTabPage("GoodsItemsTabPage"));
				var tRItemDetailsDynamicUserControl = (ZDynamicControlCreationUserControl)form.Controls.Find("GoodsItemsTabDynamicUserControl", true).First();
				AssertEquals(typeof(TRNctsGoodsItemsUserControl), tRItemDetailsDynamicUserControl.UserControlType);
			}
		}

		public void TestSecurityTabUserControlType()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			header.BH_FTZMove = true;
			using (var form = new TRNctsMovementFormForTest(header))
			{
				form.Show();
				form.MainTabControlExposed.SelectTab(form.MainTabControlExposed.GetTabPage("SecurityTabPage"));

				var securityTabDynamicUserControl = form.FindSingle<ZDynamicControlCreationUserControl>("SecurityTabDynamicUserControl");
				AssertType<SecurityTabUserControl>(securityTabDynamicUserControl.HostedControl);
			}
		}

		public void TestManifestsToOpenTabPageVisibility()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			using (var form = new TRNctsMovementForm(header))
			{
				var mainTabControl = (ZTemplateTabControl)form.Controls.Find("MainTabControl", true).First();
				var manifestsToOpenTabPage = (ZTabPage)mainTabControl.Controls.Find("ManifestsToOpenTabPage", true).FirstOrDefault();
				Assert(manifestsToOpenTabPage.TabVisible);
			}
		}

		public void TestMenuType()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			using (var form = new TRNctsMovementFormForTest(header))
			{
				AssertEquals(form.GetNctsMessageMenuType(), typeof(NctsMessagingMenu));
			}
		}

		class TRNctsMovementFormForTest : TRNctsMovementForm
		{
			public TRNctsMovementFormForTest(NctsHeader nctsMovement) : base(nctsMovement)
			{
			}

			public Type GetNctsMessageMenuType()
			{
				return GetNctsMessageMenu().GetType();
			}

			public ZTemplateTabControl MainTabControlExposed => MainTabControl;
		}

		protected override void PerformExtraNctsHeaderConfiguration(NctsHeader header)
		{
			header.MovementHeader.GoodsItems.DeleteAll();
			var goodItem = header.MovementHeader.GoodsItems.AddNew();
			goodItem.ExportDeclarationNumber = "Export type";
		}
	}
}
