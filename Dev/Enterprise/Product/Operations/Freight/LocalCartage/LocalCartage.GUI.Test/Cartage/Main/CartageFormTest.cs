using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	[TestedType(typeof(CartageForm))]
	class CartageFormTest : ZFormBasherTest
	{
		[ExpectNoExceptions]
		public void TestSave()
		{
			var cartage = Helper.CreateCartage(Core.Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			Cartage.Factory.Save();
			using (var form = new CartageForm(cartage))
			{
				form.Show();
				var container = cartage.Containers.First();
				container.JC_ArrivalSlotReference = "123";
				AssertNull(cartage.LocalClient);
				form.FireSaveButton();
			}
		}

		public void TestNotificationSubscriber()
		{
			using (var form = new CartageForm(Cartage))
			{
				Cartage.JJ_DropMode = "WUP";
				Cartage.JJ_E3_NKJobType = "ISCC";
				Cartage.ContainerBookedMoves.AddNew();
				Cartage.JJ_DropMode = "LOF";
				AssertEquals("The Form should have been set as subscriber to the Cartage.", "Would you like to override ALL Booked Movement Drop Modes with 'LOF'?", UnitTestUserNotification.Instance.LastMessage.Text);
				var subscriber = (INotificationSubscriberQueryUser)form;
				var e = new QueryUserCartageTypeDropModeEventArgs("Hey", "", "");
				subscriber.QueryUser(e);
				AssertEquals(DropMode.None, e.Response);
				AssertEquals("Drop Mode Dialog was Shown", typeof(CartageTypeDropModeDialog), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestRunSheetSecurityGUIProvider_Register()
		{
			using (var form = new CartageForm(Cartage))
			{
				var provider = RunSheetSecurityProvider.GetProvider(Factory);
				AssertNotNull(provider);
				AssertEquals(typeof(RunSheetSecurityGUIProvider), provider.GetType());
			}
		}

		public void TestJobTypeChanged_OK()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_FCLImportToCNE, null);
			RefContainer refContainer20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP");
			DummyCartageContainer container1 = new DummyCartageContainer(Factory, "C01", "FCL", refContainer20GP.PK);
			DummyCartageContainer container2 = new DummyCartageContainer(Factory, "C02", "FCL", refContainer20GP.PK);
			DummyCartageContainer container3 = new DummyCartageContainer(Factory, "C03", "FCL", refContainer20GP.PK);
			DummyCartageLooseCargo looseGoods1 = new DummyCartageLooseCargo(Factory, 3);
			DummyCartageLooseCargo looseGoods2 = new DummyCartageLooseCargo(Factory, 4);
			DummyCartageLooseCargo looseGoods3 = new DummyCartageLooseCargo(Factory, 5);
			ICartageContainer[] containers = new ICartageContainer[] { container1, container2, container3 };
			ICartageLooseCargo[] looseGoods = new ICartageLooseCargo[] { looseGoods1, looseGoods2, looseGoods3 };
			cartageType.SetContainers(containers);
			cartageType.SetLoose(looseGoods);
			//Setup Cartage
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			InternalCartageManagerHelper.PopulateCartage(cartage, cartageType);
			AssertEquals(Core.Constants.CartageJobType.NEW_FCLImportToCNE, cartage.JJ_E3_NKJobType);
			AssertEquals(3, cartage.Containers.Count());
			AssertEquals(0, cartage.LooseBookedMoves.Count);
			//Remove a Container from the Cartage to cause a discrepancy with Parent
			BusinessObject c1 = cartage.Containers.ElementAt(0);
			BusinessObject c2 = cartage.Containers.ElementAt(1);
			BusinessObject c3 = cartage.Containers.ElementAt(2);
			c3.Delete();
			AssertEquals(2, cartage.Containers.Count());
			using (CartageForm form = new CartageForm(cartage))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLImportUnpack;
				AssertEquals(@"Changing the Job Type requires a re-population of the Port Transport Job from the linked Job 'Dum1001'.

The following data will be cleared and repopulated:

 - Addresses
 - Containers / Port Transport Legs
 - Loose Bookings / Port Transport Legs

Are you sure you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				//Clicking OK will remove Clear Containers and Repopulate
				AssertEquals(Core.Constants.CartageJobType.NEW_FCLImportUnpack, cartage.JJ_E3_NKJobType);
				AssertEquals(3, cartage.Containers.Count()); //repopulated
				AssertEquals(0, cartage.LooseBookedMoves.Count);
			}
		}

		public void TestJobTypeChanged_Cancel()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_FCLImportToCNE, null);
			RefContainer refContainer20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP");
			DummyCartageContainer container1 = new DummyCartageContainer(Factory, "C01", "FCL", refContainer20GP.PK);
			DummyCartageContainer container2 = new DummyCartageContainer(Factory, "C02", "FCL", refContainer20GP.PK);
			DummyCartageContainer container3 = new DummyCartageContainer(Factory, "C03", "FCL", refContainer20GP.PK);
			DummyCartageLooseCargo looseGoods1 = new DummyCartageLooseCargo(Factory, 3);
			DummyCartageLooseCargo looseGoods2 = new DummyCartageLooseCargo(Factory, 4);
			DummyCartageLooseCargo looseGoods3 = new DummyCartageLooseCargo(Factory, 5);
			ICartageContainer[] containers = new ICartageContainer[] { container1, container2, container3 };
			ICartageLooseCargo[] looseGoods = new ICartageLooseCargo[] { looseGoods1, looseGoods2, looseGoods3 };
			cartageType.SetContainers(containers);
			cartageType.SetLoose(looseGoods);
			//Setup Cartage
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			InternalCartageManagerHelper.PopulateCartage(cartage, cartageType);
			AssertEquals(Core.Constants.CartageJobType.NEW_FCLImportToCNE, cartage.JJ_E3_NKJobType);
			AssertEquals(3, cartage.Containers.Count());
			AssertEquals(0, cartage.LooseBookedMoves.Count);
			//Remove a Container from the Cartage to cause a discrepancy with Parent
			BusinessObject c1 = cartage.Containers.ElementAt(0);
			BusinessObject c2 = cartage.Containers.ElementAt(1);
			BusinessObject c3 = cartage.Containers.ElementAt(2);
			c3.Delete();
			AssertEquals(2, cartage.Containers.Count());
			using (CartageForm form = new CartageForm(cartage))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLImportUnpack;
				AssertEquals(@"Changing the Job Type requires a re-population of the Port Transport Job from the linked Job 'Dum1001'.

The following data will be cleared and repopulated:

 - Addresses
 - Containers / Port Transport Legs
 - Loose Bookings / Port Transport Legs

Are you sure you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				//Clicking Cancel won't change a thing
				AssertEquals(Core.Constants.CartageJobType.NEW_FCLImportToCNE, cartage.JJ_E3_NKJobType);
				AssertEquals(2, cartage.Containers.Count());
				AssertEquals(0, cartage.LooseBookedMoves.Count);
			}
		}

		public void TestJobTypeChanged_OKContainerToLoose()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_FCLImportToCNE, null);
			RefContainer refContainer20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP");
			DummyCartageContainer container1 = new DummyCartageContainer(Factory, "C01", "FCL", refContainer20GP.PK);
			DummyCartageContainer container2 = new DummyCartageContainer(Factory, "C02", "FCL", refContainer20GP.PK);
			DummyCartageContainer container3 = new DummyCartageContainer(Factory, "C03", "FCL", refContainer20GP.PK);
			DummyCartageLooseCargo looseGoods1 = new DummyCartageLooseCargo(Factory, 3);
			DummyCartageLooseCargo looseGoods2 = new DummyCartageLooseCargo(Factory, 4);
			DummyCartageLooseCargo looseGoods3 = new DummyCartageLooseCargo(Factory, 5);
			ICartageContainer[] containers = new ICartageContainer[] { container1, container2, container3 };
			ICartageLooseCargo[] looseGoods = new ICartageLooseCargo[] { looseGoods1, looseGoods2, looseGoods3 };
			cartageType.SetContainers(containers);
			cartageType.SetLoose(looseGoods);
			//Setup Cartage
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			InternalCartageManagerHelper.PopulateCartage(cartage, cartageType);
			AssertEquals(Core.Constants.CartageJobType.NEW_FCLImportToCNE, cartage.JJ_E3_NKJobType);
			AssertEquals(3, cartage.Containers.Count());
			AssertEquals(0, cartage.LooseBookedMoves.Count);
			//Remove a Container from the Cartage to cause a discrepancy with Parent
			BusinessObject c1 = cartage.Containers.ElementAt(0);
			BusinessObject c2 = cartage.Containers.ElementAt(1);
			BusinessObject c3 = cartage.Containers.ElementAt(2);
			c3.Delete();
			AssertEquals(2, cartage.Containers.Count());
			using (CartageForm form = new CartageForm(cartage))
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;
				AssertEquals(@"Changing the Job Type requires a re-population of the Port Transport Job from the linked Job 'Dum1001'.

The following data will be cleared and repopulated:

 - Addresses
 - Containers / Port Transport Legs
 - Loose Bookings / Port Transport Legs

Are you sure you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				//Clicking OK will remove Clear Containers and Repopulate
				AssertEquals(Core.Constants.CartageJobType.NEW_FCLUnpackLooseToCNE, cartage.JJ_E3_NKJobType);
				AssertEquals(3, cartage.Containers.Count()); //repopulated
				AssertEquals(3, cartage.LooseBookedMoves.Count); //populated
			}
		}

		public void TestJobTypeChanged_OKLoose()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_LCLImport, null);
			RefContainer refContainer20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP");
			DummyCartageContainer container1 = new DummyCartageContainer(Factory, "C01", "FCL", refContainer20GP.PK);
			DummyCartageContainer container2 = new DummyCartageContainer(Factory, "C02", "FCL", refContainer20GP.PK);
			DummyCartageContainer container3 = new DummyCartageContainer(Factory, "C03", "FCL", refContainer20GP.PK);
			DummyCartageLooseCargo looseGoods1 = new DummyCartageLooseCargo(Factory, 3);
			DummyCartageLooseCargo looseGoods2 = new DummyCartageLooseCargo(Factory, 4);
			DummyCartageLooseCargo looseGoods3 = new DummyCartageLooseCargo(Factory, 5);
			ICartageContainer[] containers = new ICartageContainer[] { container1, container2, container3 };
			ICartageLooseCargo[] looseGoods = new ICartageLooseCargo[] { looseGoods1, looseGoods2, looseGoods3 };
			cartageType.SetContainers(containers);
			cartageType.SetLoose(looseGoods);
			//Setup Cartage
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			InternalCartageManagerHelper.PopulateCartage(cartage, cartageType);
			AssertEquals(Core.Constants.CartageJobType.NEW_LCLImport, cartage.JJ_E3_NKJobType);
			AssertEquals(0, cartage.Containers.Count());
			AssertEquals(3, cartage.LooseBookedMoves.Count);
			//Remove a Container from the Cartage to cause a discrepancy with Parent
			CommonBookedCtgMove m1 = cartage.LooseBookedMoves[0];
			CommonBookedCtgMove m2 = cartage.LooseBookedMoves[1];
			CommonBookedCtgMove m3 = cartage.LooseBookedMoves[2];
			m3.Delete();
			AssertEquals(2, cartage.LooseBookedMoves.Count);
			using (CartageForm form = new CartageForm(cartage))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_AirImport;
				AssertEquals(@"Changing the Job Type requires a re-population of the Port Transport Job from the linked Job 'Dum1001'.

The following data will be cleared and repopulated:

 - Addresses
 - Containers / Port Transport Legs
 - Loose Bookings / Port Transport Legs

Are you sure you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				//Clicking OK will remove Clear LooseBookedMoves and Repopulate
				AssertEquals(Core.Constants.CartageJobType.NEW_AirImport, cartage.JJ_E3_NKJobType);
				AssertEquals(0, cartage.Containers.Count());
				AssertEquals(3, cartage.LooseBookedMoves.Count); //repopulated
			}
		}

		public void TestJobTypeChanged_HasJobAlreadyCommenced()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_LCLImport, null);
			RefContainer refContainer20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP");
			DummyCartageContainer container1 = new DummyCartageContainer(Factory, "C01", "FCL", refContainer20GP.PK);
			DummyCartageContainer container2 = new DummyCartageContainer(Factory, "C02", "FCL", refContainer20GP.PK);
			DummyCartageContainer container3 = new DummyCartageContainer(Factory, "C03", "FCL", refContainer20GP.PK);
			DummyCartageLooseCargo looseGoods1 = new DummyCartageLooseCargo(Factory, 3);
			DummyCartageLooseCargo looseGoods2 = new DummyCartageLooseCargo(Factory, 4);
			DummyCartageLooseCargo looseGoods3 = new DummyCartageLooseCargo(Factory, 5);
			ICartageContainer[] containers = new ICartageContainer[] { container1, container2, container3 };
			ICartageLooseCargo[] looseGoods = new ICartageLooseCargo[] { looseGoods1, looseGoods2, looseGoods3 };
			cartageType.SetContainers(containers);
			cartageType.SetLoose(looseGoods);
			//Setup Cartage
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			InternalCartageManagerHelper.PopulateCartage(cartage, cartageType);
			AssertEquals(Core.Constants.CartageJobType.NEW_LCLImport, cartage.JJ_E3_NKJobType);
			AssertEquals(0, cartage.Containers.Count());
			AssertEquals(3, cartage.LooseBookedMoves.Count);
			CommonBookedCtgMove m1 = cartage.LooseBookedMoves[0];
			m1.CartageLegs[0].JU_PickupTimeIn = ZDateTime.Now;
			using (CartageForm form = new CartageForm(cartage))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_AirImport;
				AssertEquals(@"The Job Type cannot be changed if the Port Transport Job has already commenced.

The Job Type will revert back to its previous value.", UnitTestUserNotification.Instance.LastMessage.Text);
				//Nothing should change
				AssertEquals(Core.Constants.CartageJobType.NEW_LCLImport, cartage.JJ_E3_NKJobType);
				AssertEquals(0, cartage.Containers.Count());
				AssertEquals(3, cartage.LooseBookedMoves.Count);
			}
		}

		public void TestJobTypeChanged_Standalone()
		{
			var emptyCartage = Helper.CreateCartage(Core.Constants.CartageJobType.NEW_FCLImportToCNE, 0);
			using (CartageForm form = new CartageForm(emptyCartage))
			{
				emptyCartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLImportUnpack;
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}

			var containerCartage = Helper.CreateCartage(Core.Constants.CartageJobType.NEW_FCLImportToCNE, 2);
			using (CartageForm form = new CartageForm(containerCartage))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				containerCartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLImportUnpack;
				AssertEquals(@"Changing the Job Type requires a re-population of the Port Transport Legs.

Are you sure you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var looseCartage = Helper.CreateCartage(Core.Constants.CartageJobType.NEW_AirExport, 1);
			using (CartageForm form = new CartageForm(looseCartage))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				looseCartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_AirImport;
				AssertEquals(@"Changing the Job Type requires a re-population of the Port Transport Legs.

Are you sure you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestContainerModeChanged_Yes_ContainersTabPageNotificationIcon_ContainerTabStillInvisible()
		{
			var dummyParent = new DummyCartageParent(Factory);
			var cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_FCLImportToCNE, null);
			var refContainer20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP");
			var container1 = new DummyCartageContainer(Factory, "C01", "FCL", refContainer20GP.PK);
			var container2 = new DummyCartageContainer(Factory, "C02", "FCL", refContainer20GP.PK);
			var container3 = new DummyCartageContainer(Factory, "C03", "FCL", refContainer20GP.PK);
			var containers = new ICartageContainer[] { container1, container2, container3 };
			cartageType.SetContainers(containers);
			//Setup Cartage
			var cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			InternalCartageManagerHelper.PopulateCartage(cartage, cartageType);
			AssertEquals("Precondition", Core.Constants.CartageJobType.NEW_FCLImportToCNE, cartage.JJ_E3_NKJobType);
			AssertEquals("Precondition", 3, cartage.Containers.Count());
			AssertEquals("Precondition", 0, cartage.LooseBookedMoves.Count);
			AssertEquals("Precondition", Core.Constants.CartageContainerMode.Containerized, cartage.JJ_ContainerMode);
			using (var form = new CartageForm(cartage))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.Show();
				Application.DoEvents();
				var mainTabControl = GUITestHelper.FindControl<ZTemplateTabControl>(form.Controls);
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "MainTabPage");
				AssertEquals("Precondition", false, form.LooseMovesTabPage.TabVisible);
				AssertEquals("Precondition", true, form.ContainersTabPage.TabVisible);
				AssertEquals("Precondition", false, form.ContainersTabPage.HasErrors);
				AssertEquals("Precondition", -1, form.ContainersTabPage.ImageIndex);
				mainTabControl.SelectTab(form.ContainersTabPage);
				Application.DoEvents();
				AssertEquals("Precondition", true, form.ContainersTabPage.HasErrors);
				AssertGreaterThan("Precondition", form.ContainersTabPage.ImageIndex, -1);
				mainTabControl.SelectTab(mainTabPage);
				cartage.JJ_ContainerMode = Core.Constants.CartageContainerMode.Loose;
				AssertEquals("Changing the Container Mode to 'LSE' will remove the Containers from this Job. Proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				Application.DoEvents();
				// Clicking yes will change container mode to loose, removing all containers,
				// and hide the containers tab page
				CombineAssertions(() =>
				{
					AssertEquals("Should change container mode", Core.Constants.CartageContainerMode.Loose, cartage.JJ_ContainerMode);
					AssertEquals("Should get rid of containers", 0, cartage.Containers.Count());
					AssertEquals(0, cartage.LooseBookedMoves.Count);
					AssertEquals("Loose moves tab page should be visible because container mode allows loose moves", true, form.LooseMovesTabPage.TabVisible);
					AssertEquals("Containers tab page ought to be invisible", false, form.ContainersTabPage.TabVisible);
				});
				cartage.JJ_ContainerMode = Core.Constants.CartageContainerMode.Containerized;
				Application.DoEvents();
				CombineAssertions(() =>
				{
					AssertEquals("Should change container mode", Core.Constants.CartageContainerMode.Containerized, cartage.JJ_ContainerMode);
					AssertEquals(0, cartage.Containers.Count());
					AssertEquals(0, cartage.LooseBookedMoves.Count);
					AssertEquals("Loose moves tab page should be invisible again", false, form.LooseMovesTabPage.TabVisible);
					AssertEquals("Containers tab page should be visible", true, form.ContainersTabPage.TabVisible);
					AssertEquals("No containers so there should be no notifications for containers tab page", false, form.ContainersTabPage.HasNotifications);
					AssertEquals("No containers so there should be no notification icon", -1, form.ContainersTabPage.ImageIndex);
				});
			}
		}

		public void TestContainerModeChanged_DoesNotModifyErrorStatusOfContainerTabControl()
		{
			var dummyParent = new DummyCartageParent(Factory);
			var cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_FCLImportToCNE, null);
			var refContainer20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP");
			var containers = Array.Empty<ICartageContainer>();
			var looseGoods = Array.Empty<ICartageLooseCargo>();
			cartageType.SetContainers(containers);
			cartageType.SetLoose(looseGoods);
			//Setup Cartage
			var cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			InternalCartageManagerHelper.PopulateCartage(cartage, cartageType);
			AssertEquals("Precondition", Core.Constants.CartageJobType.NEW_FCLImportToCNE, cartage.JJ_E3_NKJobType);
			AssertEquals("Precondition", 0, cartage.Containers.Count());
			AssertEquals("Precondition", 0, cartage.LooseBookedMoves.Count);
			AssertEquals("Precondition", Core.Constants.CartageContainerMode.Containerized, cartage.JJ_ContainerMode);
			using (var form = new CartageForm(cartage))
			{
				form.Show();
				var mainTabControl = GUITestHelper.FindControl<ZTemplateTabControl>(form.Controls);
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "MainTabPage");
				AssertEquals("Precondition", false, form.LooseMovesTabPage.TabVisible);
				AssertEquals("Precondition", true, form.ContainersTabPage.TabVisible);
				mainTabControl.SelectTab(form.ContainersTabPage);
				AssertEquals("Precondition", false, form.ContainersTabPage.HasErrors);
				AssertEquals("Precondition", -1, form.ContainersTabPage.ImageIndex);
				NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.ComponentModel.NotificationType.Error, form.containerDetailsControl1.ContainersGrid);
				Application.DoEvents();
				AssertEquals("Precondition", true, form.ContainersTabPage.HasErrors);
				AssertGreaterThan("Precondition", form.ContainersTabPage.ImageIndex, -1);
				mainTabControl.SelectTab(mainTabPage);
				cartage.JJ_ContainerMode = Core.Constants.CartageContainerMode.Loose;
				AssertEquals("Should not be a warning, because there's no containers to delete", null, UnitTestUserNotification.Instance.LastMessage.Text);
				Application.DoEvents();
				CombineAssertions(() =>
				{
					AssertEquals("Should change container mode", Core.Constants.CartageContainerMode.Loose, cartage.JJ_ContainerMode);
					AssertEquals("Loose moves tab page should be visible because container mode allows loose moves", true, form.LooseMovesTabPage.TabVisible);
					AssertEquals("Containers tab page should be invisible, because this container mode doesn't allow moves of containers", false, form.ContainersTabPage.TabVisible);
					AssertEquals("Containers tab page should still have error, because ContainerModeChanged hasn't arbitrarily modified the state of the control ContainersGrid", true, form.ContainersTabPage.HasErrors);
					AssertGreaterThan("Containers tab page should still have error, because ContainerModeChanged hasn't arbitrarily modified the state of the control ContainersGrid", form.ContainersTabPage.ImageIndex, -1);
				});
			}
		}

		public void TestContainerModeChanged_Mixed_ContainerTabVisible()
		{
			var dummyParent = new DummyCartageParent(Factory);
			var cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_FCLImportToCNE, null);
			var refContainer20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP");
			var container1 = new DummyCartageContainer(Factory, "C01", "FCL", refContainer20GP.PK);
			var container2 = new DummyCartageContainer(Factory, "C02", "FCL", refContainer20GP.PK);
			var container3 = new DummyCartageContainer(Factory, "C03", "FCL", refContainer20GP.PK);
			var containers = new ICartageContainer[] { container1, container2, container3 };
			cartageType.SetContainers(containers);
			//Setup Cartage
			var cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			InternalCartageManagerHelper.PopulateCartage(cartage, cartageType);
			AssertEquals("Precondition", Core.Constants.CartageJobType.NEW_FCLImportToCNE, cartage.JJ_E3_NKJobType);
			AssertEquals("Precondition", 3, cartage.Containers.Count());
			AssertEquals("Precondition", 0, cartage.LooseBookedMoves.Count);
			AssertEquals("Precondition", Core.Constants.CartageContainerMode.Containerized, cartage.JJ_ContainerMode);
			using (var form = new CartageForm(cartage))
			{
				form.Show();
				Application.DoEvents();
				var mainTabControl = GUITestHelper.FindControl<ZTemplateTabControl>(form.Controls);
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "MainTabPage");
				AssertEquals("Precondition", false, form.LooseMovesTabPage.TabVisible);
				AssertEquals("Precondition", true, form.ContainersTabPage.TabVisible);
				AssertEquals("Precondition", false, form.ContainersTabPage.HasErrors);
				AssertEquals("Precondition", -1, form.ContainersTabPage.ImageIndex);
				mainTabControl.SelectTab(form.ContainersTabPage);
				Application.DoEvents();
				AssertEquals("Precondition", true, form.ContainersTabPage.HasErrors);
				AssertGreaterThan("Precondition", form.ContainersTabPage.ImageIndex, -1);
				mainTabControl.SelectTab(mainTabPage);
				cartage.JJ_ContainerMode = Core.Constants.CartageContainerMode.Mixed;
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				Application.DoEvents();
				// The change in container mode will change container mode to mixed, and the containers will remain, as will the containers tab page, and the loose moves tab page will appear
				CombineAssertions(() =>
				{
					AssertEquals("Should change container mode", Core.Constants.CartageContainerMode.Mixed, cartage.JJ_ContainerMode);
					AssertEquals("Should keep containers", 3, cartage.Containers.Count());
					AssertEquals(0, cartage.LooseBookedMoves.Count);
					AssertEquals("Loose moves tab page should be visible because container mode allows loose moves", true, form.LooseMovesTabPage.TabVisible);
					AssertEquals("Containers tab page ought to be visible", true, form.ContainersTabPage.TabVisible);
					AssertEquals("Should still have error about container", true, form.ContainersTabPage.HasErrors);
					AssertGreaterThan("Should still have notification icon", form.ContainersTabPage.ImageIndex, -1);
				});
			}
		}

		public void TestContainerModeChanged_Yes_LooseMovesTabPageNotificationIcon_LooseMovesTabStillInvisible()
		{
			var dummyParent = new DummyCartageParent(Factory);
			var cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_LCLImport, null);
			var refContainer20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP");
			var looseGoods1 = new DummyCartageLooseCargo(Factory, 3);
			var looseGoods2 = new DummyCartageLooseCargo(Factory, 4);
			var looseGoods3 = new DummyCartageLooseCargo(Factory, 5);
			var containers = Array.Empty<ICartageContainer>();
			var looseGoods = new ICartageLooseCargo[] { looseGoods1, looseGoods2, looseGoods3 };
			cartageType.SetContainers(containers);
			cartageType.SetLoose(looseGoods);
			//Setup Cartage
			var cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			InternalCartageManagerHelper.PopulateCartage(cartage, cartageType);
			AssertEquals(Core.Constants.CartageJobType.NEW_LCLImport, cartage.JJ_E3_NKJobType);
			AssertEquals(0, cartage.Containers.Count());
			AssertEquals(3, cartage.LooseBookedMoves.Count);
			using (var form = new CartageForm(cartage))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.Show();
				Application.DoEvents();
				var mainTabControl = GUITestHelper.FindControl<ZTemplateTabControl>(form.Controls);
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "MainTabPage");
				AssertEquals("Precondition", true, form.LooseMovesTabPage.TabVisible);
				AssertEquals("Precondition", false, form.ContainersTabPage.TabVisible);
				AssertEquals("Precondition", false, form.LooseMovesTabPage.HasErrors);
				AssertEquals("Precondition", -1, form.LooseMovesTabPage.ImageIndex);
				mainTabControl.SelectTab(form.LooseMovesTabPage);
				Application.DoEvents();
				AssertEquals("Precondition", true, form.LooseMovesTabPage.HasErrors);
				AssertGreaterThan("Precondition", form.LooseMovesTabPage.ImageIndex, -1);
				mainTabControl.SelectTab(mainTabPage);
				cartage.JJ_ContainerMode = Core.Constants.CartageContainerMode.Containerized;
				AssertEquals("Changing the Container Mode to 'CNT' will remove the Loose Movements from this Job. Proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				Application.DoEvents();
				// Clicking yes will change container mode to containerized, removing all loose moves,
				// and hide the loose moves tab page
				CombineAssertions(() =>
				{
					AssertEquals("Should change container mode", Core.Constants.CartageContainerMode.Containerized, cartage.JJ_ContainerMode);
					AssertEquals("Should get rid of loose moves", 0, cartage.LooseBookedMoves.Count);
					AssertEquals(0, cartage.Containers.Count());
					AssertEquals("Containers tab page should be visible because container mode allows containers", true, form.ContainersTabPage.TabVisible);
					AssertEquals("Loose moves tab page ought to be invisible", false, form.LooseMovesTabPage.TabVisible);
				});
				cartage.JJ_ContainerMode = Core.Constants.CartageContainerMode.Loose;
				Application.DoEvents();
				CombineAssertions(() =>
				{
					AssertEquals("Should change container mode", Core.Constants.CartageContainerMode.Loose, cartage.JJ_ContainerMode);
					AssertEquals(0, cartage.Containers.Count());
					AssertEquals(0, cartage.LooseBookedMoves.Count);
					AssertEquals("Containers tab page should be invisible again", false, form.ContainersTabPage.TabVisible);
					AssertEquals("Loose moves tab page should be visible", true, form.LooseMovesTabPage.TabVisible);
					AssertEquals("No loose moves so there should be no notifications for loose moves tab page", false, form.LooseMovesTabPage.HasNotifications);
					AssertEquals("No loose moves so there should be no notification icon", -1, form.LooseMovesTabPage.ImageIndex);
				});
			}
		}

		public void TestContainerModeChanged_DoesNotModifyErrorStatusOfLooseMovesTabControl()
		{
			var dummyParent = new DummyCartageParent(Factory);
			var cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_LCLImport, null);
			var refContainer20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP");
			var containers = Array.Empty<ICartageContainer>();
			var looseGoods = Array.Empty<ICartageLooseCargo>();
			cartageType.SetContainers(containers);
			cartageType.SetLoose(looseGoods);
			//Setup Cartage
			var cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			InternalCartageManagerHelper.PopulateCartage(cartage, cartageType);
			AssertEquals("Precondition", Core.Constants.CartageJobType.NEW_LCLImport, cartage.JJ_E3_NKJobType);
			AssertEquals("Precondition", 0, cartage.Containers.Count());
			AssertEquals("Precondition", 0, cartage.LooseBookedMoves.Count);
			AssertEquals("Precondition", Core.Constants.CartageContainerMode.Loose, cartage.JJ_ContainerMode);
			using (var form = new CartageForm(cartage))
			{
				form.Show();
				var mainTabControl = GUITestHelper.FindControl<ZTemplateTabControl>(form.Controls);
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "MainTabPage");
				AssertEquals("Precondition", true, form.LooseMovesTabPage.TabVisible);
				AssertEquals("Precondition", false, form.ContainersTabPage.TabVisible);
				Application.DoEvents();
				mainTabControl.SelectTab(form.LooseMovesTabPage);
				// Remove the focus from LooseDeliveryGrid - if focused there, validation errors occur
				form.LooseMovesTabPage.Focus();
				AssertEquals("Precondition", false, form.LooseMovesTabPage.HasErrors);
				AssertEquals("Precondition", -1, form.LooseMovesTabPage.ImageIndex);
				NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.ComponentModel.NotificationType.Error, form.looseMovesControl1.LooseDeliveryGrid);
				Application.DoEvents();
				AssertEquals("Precondition", true, form.LooseMovesTabPage.HasErrors);
				AssertGreaterThan("Precondition", form.LooseMovesTabPage.ImageIndex, -1);
				mainTabControl.SelectTab(mainTabPage);
				cartage.JJ_ContainerMode = Core.Constants.CartageContainerMode.Containerized;
				AssertEquals("Should not be a warning, because there's no loose moves to delete", null, UnitTestUserNotification.Instance.LastMessage.Text);
				Application.DoEvents();
				CombineAssertions(() =>
				{
					AssertEquals("Should change container mode", Core.Constants.CartageContainerMode.Containerized, cartage.JJ_ContainerMode);
					AssertEquals("Containers tab page should be visible because container mode allows containers", true, form.ContainersTabPage.TabVisible);
					AssertEquals("Loose moves tab page ought to be invisible, because this container mode doesn't allow loose moves", false, form.LooseMovesTabPage.TabVisible);
					AssertEquals("Loose moves tab page should still have error, because ContainerModeChanged hasn't arbitrarily modified the state of the control LooseDeliveryGrid", true, form.LooseMovesTabPage.HasErrors);
					AssertGreaterThan("Loose moves tab page should still have error, because ContainerModeChanged hasn't arbitrarily modified the state of the control LooseDeliveryGrid", form.LooseMovesTabPage.ImageIndex, -1);
				});
			}
		}

		public void TestContainerModeChanged_Mixed_LooseMovesTabVisible()
		{
			var dummyParent = new DummyCartageParent(Factory);
			var cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_LCLImport, null);
			var refContainer20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP");
			var looseGoods1 = new DummyCartageLooseCargo(Factory, 3);
			var looseGoods2 = new DummyCartageLooseCargo(Factory, 4);
			var looseGoods3 = new DummyCartageLooseCargo(Factory, 5);
			var containers = Array.Empty<ICartageContainer>();
			var looseGoods = new ICartageLooseCargo[] { looseGoods1, looseGoods2, looseGoods3 };
			cartageType.SetContainers(containers);
			cartageType.SetLoose(looseGoods);
			//Setup Cartage
			var cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			InternalCartageManagerHelper.PopulateCartage(cartage, cartageType);
			AssertEquals(Core.Constants.CartageJobType.NEW_LCLImport, cartage.JJ_E3_NKJobType);
			AssertEquals(0, cartage.Containers.Count());
			AssertEquals(3, cartage.LooseBookedMoves.Count);
			using (var form = new CartageForm(cartage))
			{
				form.Show();
				Application.DoEvents();
				var mainTabControl = GUITestHelper.FindControl<ZTemplateTabControl>(form.Controls);
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "MainTabPage");
				AssertEquals("Precondition", true, form.LooseMovesTabPage.TabVisible);
				AssertEquals("Precondition", false, form.ContainersTabPage.TabVisible);
				AssertEquals("Precondition", false, form.LooseMovesTabPage.HasErrors);
				AssertEquals("Precondition", -1, form.ContainersTabPage.ImageIndex);
				AssertEquals("Precondition", -1, form.LooseMovesTabPage.ImageIndex);
				mainTabControl.SelectTab(form.LooseMovesTabPage);
				Application.DoEvents();
				AssertEquals("Precondition", true, form.LooseMovesTabPage.HasErrors);
				AssertGreaterThan("Precondition", form.LooseMovesTabPage.ImageIndex, -1);
				mainTabControl.SelectTab(mainTabPage);
				cartage.JJ_ContainerMode = Core.Constants.CartageContainerMode.Mixed;
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				Application.DoEvents();
				// The change in container mode will change container mode to mixed, and the loose moves will remain, as will the loose moves tab page, and the container tab page will appear
				CombineAssertions(() =>
				{
					AssertEquals("Should change container mode", Core.Constants.CartageContainerMode.Mixed, cartage.JJ_ContainerMode);
					AssertEquals("Should keep loose moves", 3, cartage.LooseBookedMoves.Count);
					AssertEquals(0, cartage.Containers.Count());
					AssertEquals("Containers tab page should be visible because container mode allows containers", true, form.ContainersTabPage.TabVisible);
					AssertEquals("Loose moves tab page ought to be visible", true, form.LooseMovesTabPage.TabVisible);
					AssertEquals("Should still have error about container", true, form.LooseMovesTabPage.HasErrors);
					AssertGreaterThan("Should still have notification icon", form.LooseMovesTabPage.ImageIndex, -1);
				});
			}
		}

		public void TestDeleteFromContainersGrid()
		{
			var dummyParent = new DummyCartageParent(Factory);
			var cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_FCLImportToCNE, null);
			var refContainer20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP");
			var container1 = new DummyCartageContainer(Factory, "C01", "FCL", refContainer20GP.PK);
			var containers = new ICartageContainer[] { container1 };
			cartageType.SetContainers(containers);
			//Setup Cartage
			var cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			InternalCartageManagerHelper.PopulateCartage(cartage, cartageType);
			using (var form = new CartageForm(cartage))
			{
				form.Show();
				var mainTabControl = GUITestHelper.FindControl<ZTemplateTabControl>(form.Controls);
				mainTabControl.SelectTab(form.ContainersTabPage);
				Application.DoEvents();
				var containerDetailsControl = form.containerDetailsControl1;
				var containersGrid = containerDetailsControl.ContainersGrid;
				containersGrid.SetCurrentHitTestForTest(0, 0);
				containersGrid.OnPopup_CallForTesting();
				var deleteMenuItem = containersGrid.ContextMenu.MenuItems.FindByText("Delete", false);
				Assert(deleteMenuItem.Enabled);
				deleteMenuItem.PerformClick();
				form.Refresh();
#if !WINZOR
				AssertEquals("Paint should work", false, containersGrid.PaintFailedLastTime);
#endif
			}
		}

		[ExpectNoExceptions()]
		public void TestShowPreSaveDialogs_Delete()
		{
			Cartage.Factory.Save();
			using (var form = new CartageForm(Cartage))
			{
				form.Show();
				var newFactory = new BusinessObjectFactory();
				newFactory.Load<CommonCartage>(Cartage.PK).Delete();
				newFactory.Save();
				form.FireSaveButton();
			}
		}

		[ExpectNoExceptions()]
		public void TestSelectCartageLeg_ContainerLeg()
		{
			var cartage = Cartage;
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_AirExport;
			var leg = cartage.ContainerBookedMoves.AddNew().CartageLegs.AddNew();
			using (var form = new CartageForm(Cartage))
			{
				form.SelectCartageLeg(leg);
				form.Show();
				Application.DoEvents();
			}
		}

		[ExpectNoExceptions()]
		public void TestSelectCartageLeg_LooseLeg()
		{
			var cartage = Cartage;
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_EmptyCFStoCYD;
			var leg = cartage.LooseBookedMoves.AddNew().CartageLegs.AddNew();
			using (var form = new CartageForm(Cartage))
			{
				form.Show();
				form.SelectCartageLeg(leg);
				Application.DoEvents();
			}
		}

		public void TestValidateAndSave()
		{
			var cartageToDelete = Factory.New<CommonCartage>();
			var cartageNew = Factory.New<CommonCartage>();
			var cartageOther = Factory.New<CommonCartage>();
			Factory.Save();
			var controller = ZControllerFactory.Create(ControllerIDs.Cartage);
			using (var cartageToDeleteForm = controller.ShowEditForm(cartageToDelete))
			using (var cartageNewForm = (CartageForm)controller.ShowEditForm(cartageNew))
			{
				AssertEquals("Should return ContinueWithSave.Yes", ContinueWithSave.Yes, cartageNewForm.FireSaveButton());
				cartageNewForm.CartageToBeDeactivated = cartageToDelete;
				AssertEquals("Should return ContinueWithSave.No as cartageToDeactivated form is open", ContinueWithSave.No, cartageNewForm.FireSaveButton());
				cartageNewForm.CartageToBeDeactivated = cartageOther;
				AssertEquals("Should return ContinueWithSave.Yes as cartageOther form is *not* open", ContinueWithSave.Yes, cartageNewForm.FireSaveButton());
			}
		}

		public void TestOnFactorySavingBeforeTransactionCore_Workflow_TasksAndMilestonesAreCreated()
		{
			var cartage = Helper.CreateCartage(Core.Constants.CartageJobType.NEW_FCLImportToCNE, 0);
			cartage.IsRoot = true;
			var cnr = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageExporter, "CNR", "Consignor st", "2000", "Sydney", "AUSYD", false);
			var cto = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageCTO, "CTO", "Wharf st", "2000", "Sydney", "AUSYD", false);
			Factory.Save();
			using (var form = new CartageForm(cartage))
			{
				form.Show();
				form.FireValidateAllForTest();
				Application.DoEvents();
				var mainTabControl = GUITestHelper.FindControl<ZTemplateTabControl>(form.Controls);
				mainTabControl.SelectTab(form.ContainersTabPage);
				cartage.ContainerBookedMoves[0].Container.JC_ContainerNum = "C123";
				cartage.ContainerBookedMoves[0].Container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
				var leg = cartage.ContainerBookedMoves[0].CartageLegs.First();
				form.FireSaveButton();
				AssertEquals("21 Workflow Item should have been generated from template on save.", 21, leg.WorkflowItems.Count);
			}
		}

		public void TestCartage_OnGetNonContainerisedCartageLegsToPrint()
		{
			var emptyCartage = Helper.CreateCartage(Core.Constants.CartageJobType.NEW_FCLImportToCNE, 0);
			using (CartageForm form = new CartageForm(emptyCartage))
			{
				var cartageAdviceMenu = Factory.New<StmMenuItem>();
				var commonCartage = Factory.New<CommonCartage>();
				cartageAdviceMenu.SU_MenuName = "something Cartage Advice something";
				var docEventArgs = new DocumentCartageLegEventArgs(new DocumentCartageLegOptions(new DocumentCartageLegCollection(commonCartage.CartageLegs)));
				emptyCartage.RaiseOnGetNonContainerisedCartageLegsToPrint(docEventArgs);
				AssertEquals("There are no cartage legs to print.", false, docEventArgs.ContinueToPrint);
				AssertEquals("Information message should be shown.", true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				CommonBookedCtgMove move = commonCartage.LooseBookedMoves.AddNew();
				move.CartageLegs.AddNew();
				docEventArgs = new DocumentCartageLegEventArgs(new DocumentCartageLegOptions(new DocumentCartageLegCollection(new CommonCartageLegCollection(move))));
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				emptyCartage.RaiseOnGetNonContainerisedCartageLegsToPrint(docEventArgs);
				AssertEquals("Since Dialog result is Yes it should continue to print them.", true, docEventArgs.ContinueToPrint);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				emptyCartage.RaiseOnGetNonContainerisedCartageLegsToPrint(docEventArgs);
				AssertEquals("Since Dialog result is Cancel it should cancel the print.", false, docEventArgs.ContinueToPrint);
			}
		}

		public void TestHandleSaveExceptionHandlesTriggerException()
		{
			const string TriggerErrorMessageResourceStringGuid = "38fe5977-dc40-4378-9601-aa3f2a78ec17";
			const string MockTriggerErrorMessageStringInSpanish = "Se intentó insertar un sufijo duplicado en los tramos de transporte. Intente cerrar y volver a abrir el formulario.";
			var language = Core.Constants.Languages.Spanish;
			using (Res.TemporarilySwitchLanguage(language))
			using (var mockCache = Res.GetLanguageInstance(language).UseMockData())
			using (var form = (CartageForm)GetFormToBash())
			{
				mockCache.Put(TriggerErrorMessageResourceStringGuid, new ResourceStringData(TriggerErrorMessageResourceStringGuid, MockTriggerErrorMessageStringInSpanish));
				form.ValidatingForSave += delegate
				{
					throw new ZSaveException(new ZDataException(new Exception(PortTransportFormExceptionHandler.JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJobTriggerID), null, null), Factory);
				};

				UnitTestUserNotification.Instance.ClearMessages();
				form.FireSaveButton();
			}

			AssertEquals("Error message should be resource string governed by PortTransportFormExceptionHandler.JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJobTriggerMessageForUser.", MockTriggerErrorMessageStringInSpanish, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestHandleSaveExceptionFallsToDefaultExceptionHandlingForOtherExceptions()
		{
			var exceptionsCaughtByMockHandler = new List<(string message, string caption, string errorContext, Exception exception)>();
			var testNotificationHandler = new Mock<INotificationHandler>();
			testNotificationHandler
				.Setup(h => h.ReportError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()))
				.Callback((string message, string caption, string errorContext, Exception ex) =>
				{
					exceptionsCaughtByMockHandler.Add((message: message, caption: caption, errorContext: errorContext, exception: ex));
				});
			testNotificationHandler.Setup(h => h.ReportInformation(It.IsAny<string>(), It.IsAny<string>()));

			using (NotificationHandler.SetHandler(testNotificationHandler.Object))
			using (var form = (CartageForm)GetFormToBash())
			{
				form.ValidatingForSave += delegate
				{
					var testDataException = new ZDataException(new ArithmeticException("Inner test exception"), null, null);
					testDataException.SetFriendlyMessageForTest("Test exception");
					var testException = new ZSaveException(testDataException, Factory);

					throw testException;
				};

				UnitTestUserNotification.Instance.ClearMessages();
				form.FireSaveButton();
				AssertEquals("Mock handler caught 1 exception", 1, exceptionsCaughtByMockHandler.Count);
				var exceptionDetailsCaught = exceptionsCaughtByMockHandler.Single();
				CombineAssertions(() =>
				{
					AssertEquals("Caught exception (friendly message) has matching message", "Test exception", exceptionDetailsCaught.message);
					AssertEquals("Caught exception has matching exception type", typeof(ZSaveException), exceptionDetailsCaught.exception.GetType());
				});
			}
		}

		public void TestReactivation_WhenBookingIsSeviceCommenced_IsNotAllowed()
		{
			var cartage = SetupCartageForReactivation();
			Factory.Save();
			cartage.ParentBooking.KM_Status = TransportStatuses.Codes.ServiceCommenced;

			using (var form = new CartageForm(cartage))
			{
				form.Show();
				var deactivateMenuItem = FindDeactivateActionsMenuItem(form, cartage);
				deactivateMenuItem.PerformClick();

				AssertEquals("Cartage should still be inactive as booking is service commenced", true, cartage.JJ_IsCancelled);
				AssertContains("Message should be shown to inform user why the cartage cannot be reactivated", "The parent booking of this cartage is not available, so this cartage cannot be reactivated.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestReactivation_WhenBookingIsBooked_IsNotAllowed()
		{
			var cartage = SetupCartageForReactivation();
			Factory.Save();
			cartage.ParentBooking.KM_Status = TransportStatuses.Codes.Booked;

			using (var form = new CartageForm(cartage))
			{
				form.Show();
				var deactivateMenuItem = FindDeactivateActionsMenuItem(form, cartage);
				deactivateMenuItem.PerformClick();

				AssertEquals("Cartage should still be inactive as booking is booked", true, cartage.JJ_IsCancelled);
				AssertContains("Message should be shown to inform user why the cartage cannot be reactivated", "The parent booking of this cartage is not available, so this cartage cannot be reactivated.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestReactivation_WhenBookingIsInactive_IsNotAllowed()
		{
			var cartage = SetupCartageForReactivation();
			cartage.ParentBooking.KM_IsActive = false;
			Factory.Save();

			using (var form = new CartageForm(cartage))
			{
				form.Show();
				var deactivateMenuItem = FindDeactivateActionsMenuItem(form, cartage);
				deactivateMenuItem.PerformClick();

				AssertEquals("Cartage should still be inactive as booking is inactive", true, cartage.JJ_IsCancelled);
				AssertContains("Message should be shown to inform user why the cartage cannot be reactivated", "The parent booking of this cartage is not available, so this cartage cannot be reactivated.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestReactivation_WhenBookingIsActiveAndAvailable_IsAllowed()
		{
			var cartage = SetupCartageForReactivation();
			Factory.Save();

			using (var form = new CartageForm(cartage))
			{
				form.Show();
				var deactivateMenuItem = FindDeactivateActionsMenuItem(form, cartage);
				deactivateMenuItem.PerformClick();

				AssertEquals("Cartage should now be active as booking is active and available", false, cartage.JJ_IsCancelled);
				AssertNotContains("Reactivation message should not be shown", "The parent booking of this cartage is not available, so this cartage cannot be reactivated.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestReactivation_WhenNoRelatedBooking_IsAllowed()
		{
			var cartage = Helper.CreateCartage(Core.Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			cartage.JJ_IsCancelled = true;
			Factory.Save();

			using (var form = new CartageForm(cartage))
			{
				form.Show();
				var deactivateMenuItem = FindDeactivateActionsMenuItem(form, cartage);
				deactivateMenuItem.PerformClick();

				AssertEquals("Cartage should now be active as there is no related booking", false, cartage.JJ_IsCancelled);
				AssertNotContains("Reactivation message should not be shown", "The parent booking of this cartage is not available, so this cartage cannot be reactivated.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestDeactivation_WhenCartageIsActive_IsAllowed()
		{
			var cartage = SetupCartageForReactivation();
			cartage.JJ_IsCancelled = false;
			Factory.Save();

			using (var form = new CartageForm(cartage))
			{
				form.Show();
				var deactivateMenuItem = FindDeactivateActionsMenuItem(form, cartage);
				deactivateMenuItem.PerformClick();

				AssertEquals("Cartage should now be inactive as logic should not affect deactivation", true, cartage.JJ_IsCancelled);
				AssertNotContains("Reactivation message should not be shown", "The parent booking of this cartage is not available, so this cartage cannot be reactivated.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		CommonCartage SetupCartageForReactivation()
		{
			var bookingConsolidation = Factory.New<IDtbBookingConsolidation>();
			var booking = Factory.New<IDtbBooking>() as BusinessObject;
			booking[DtbBookingSchema.KM_KB_Booking] = bookingConsolidation.PK;
			var cartage = Helper.CreateCartage(Core.Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			cartage.JJ_ParentID = booking.PK;
			cartage.JJ_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			cartage.JJ_IsCancelled = true;

			return cartage;
		}

		MenuItem FindDeactivateActionsMenuItem(CartageForm form, CommonCartage cartage)
		{
			MenuItem result = null;

			if (cartage != null)
			{
				var menuName = (cartage.IsCancelled)
					? "MakeActiveName"
					: "MakeInactiveName";

				result = form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(item => item.Name == "ActionsMenuItem").MenuItems.Cast<MenuItem>().FirstOrDefault(item => item.Name == menuName);
			}

			return result;
		}

		protected override Form GetFormToBashCore()
		{
			CartageForm result = new CartageForm(Cartage)
			{ ControllerID = ControllerIDs.Cartage };
			return result;
		}

		protected override void SetUp()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			Cartage = Factory.New<CommonCartage>();
			base.SetUp();
		}

		CommonCartage Cartage;
		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;
	}
}
