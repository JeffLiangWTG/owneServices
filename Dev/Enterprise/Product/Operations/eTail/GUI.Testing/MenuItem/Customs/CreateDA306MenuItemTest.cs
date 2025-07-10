using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing;

public class CreateDA306MenuItemTest : TestCaseWithFactory
{
	public void TestCreateDA306Section38Release_StmMenuItemExists()
	{
		var shipment = Factory.New<ForwardingShipment>();
		var menu = new CreateDA306MenuItemForTest(shipment);
		var documentCommand = menu.GetDocumentCommand_ForTest();

		CombineAssertions(() =>
		{
			AssertNotNull(documentCommand);
			AssertEquals("Report Saving name", "DA 306 (Section 38 Release)", documentCommand.SU_MenuName);
			Assert("Ability to preview in Excel", documentCommand.SU_AllowRawView);
			AssertNotNull("HVLV version of the menu was retrieved", documentCommand.Documents.OfType<StmMenuTemplatePivotBase>().FirstOrDefault(x => x.Template.SO_DataContext == nameof(DataContext.ZADA306)));
		});
	}

	public void TestCreateDA306Section39Release_Action()
	{
		var shipment = Factory.New<ForwardingShipment>();
		var menu = new CreateDA306MenuItem(shipment);
		menu.PerformClick();

		AssertType<DocDeliveryForm>("The report should be successfully created.", ZFormModaliser.LastFormShownDialogForTest);
	}

	public void TestCreateDA306Section38Release_Visibility()
	{
		var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.SouthAfrica))
		{
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "ZAFCB";

			AssertCreateDA306Section38ReleaseVisible(true);
		}

		using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
		{
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var a = shipment.JobDirection == Directions.Import
				&& GlbCompany.CurrentCompany.GC_RN_NKCountryCode == CountryCodes.SouthAfrica
				&& shipment.Destination.Country.Code == CountryCodes.SouthAfrica;

			AssertCreateDA306Section38ReleaseVisible(false);
		}

		void AssertCreateDA306Section38ReleaseVisible(bool isVisible)
		{
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				customsMenu.OnPopup(EventArgs.Empty);

				var menuItem = customsMenu.MenuItems.OfType<CreateDA306MenuItem>().Single();
				if (isVisible)
				{
					AssertEquals(true, menuItem.Visible);
				}
				else
				{
					AssertEquals(false, menuItem.Visible);
				}
			}
		}
	}

	class CreateDA306MenuItemForTest : CreateDA306MenuItem
	{
		public CreateDA306MenuItemForTest(ForwardingShipment shipment) : base(shipment)
		{
		}

		public DocumentCommand GetDocumentCommand_ForTest() => GetDocumentCommand();
	}
}

