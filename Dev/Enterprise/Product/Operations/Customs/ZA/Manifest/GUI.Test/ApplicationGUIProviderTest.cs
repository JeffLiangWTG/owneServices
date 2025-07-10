using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Manifest.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using AsycudaContainer = Enterprise.Customs.ZA.Manifest.Business.AsycudaContainer;
using AsycudaManifestHeader = Enterprise.Customs.ZA.Manifest.Business.AsycudaManifestHeader;
using CusPerson = Enterprise.Customs.ZA.Manifest.Business.CusPerson;

namespace Enterprise.Customs.ZA.Manifest.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override Type ExpectedBillLayoutType => typeof(ZABillLayouts);

		protected override Type ExpectedBillPartiesLayoutType => typeof(ZADefaultBillPartiesLayouts);

		protected override void AssertGetContainersGridColumnVisibility(IReadOnlyDictionary<bool, string[]> columnVisibility)
		{
			if (columnVisibility.TryGetValue(true, out var visibleColumns))
			{
				AssertContainsExactElementsInAnyOrder(
				new[]
				{
					AsycudaContainer.Schema.ACN_ContainerNumber,
					AsycudaContainer.Schema.LandedPurpose,
					AsycudaContainer.Schema.ACN_EmptyFullIndicator,
					AsycudaContainer.Schema.ACN_RC_ContainerType,
					AsycudaContainer.Schema.ACN_Seal1,
					AsycudaContainer.Schema.ACN_SealType1,
					AsycudaContainer.Schema.ACN_SealingPartyType,
					AsycudaContainer.Schema.ACN_Seal2,
					AsycudaContainer.Schema.ACN_SealType2,
					AsycudaContainer.Schema.ACN_SealingPartyType2,
					AsycudaContainer.Schema.ACN_Seal3,
					AsycudaContainer.Schema.ACN_SealType3,
					AsycudaContainer.Schema.ACN_SealingPartyType3,
					AsycudaContainer.Schema.ACN_SealingPartyName,
					AsycudaContainer.Schema.ACN_NumberOfPackages,
					AsycudaContainer.Schema.ACN_CommodityCode,
					AsycudaContainer.Schema.ACN_GoodsWeight,
					AsycudaContainer.Schema.ACN_GoodsWeightUQ,
					AsycudaContainer.Schema.ACN_StowageLocation
				},
				visibleColumns);
			}

			if (columnVisibility.TryGetValue(false, out string[] invisibleColumns))
			{
				AssertContainsExactElementsInAnyOrder(
					new string[]
					{
						AutoAsycudaContainer.Schema.ACN_Seal1UnloadingState,
						AutoAsycudaContainer.Schema.ACN_Seal2UnloadingState,
						AutoAsycudaContainer.Schema.ACN_Seal3UnloadingState,
					},
					invisibleColumns);
			}
		}

		protected override void AssertGetContainersGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			Assertion.AssertEquals(1, columnInfos.Length);
		}

		protected override void AssertGetContainersGridColumnsOrder(string[] columnsOrder)
		{
			Assertion.AssertEquals(19, columnsOrder.Length);
		}

		public void TestGetMessagesGridExtraMenuItems()
		{
			var header = CreateNewManifest();
			Factory.Save();
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			using (var grid = new ZGrid())
			{
				var menuItems = provider.GetMessagesGridExtraMenuItems(grid, header).ToArray();
				AssertEquals(1, menuItems.Length);
				AssertEquals("Request Customs Resend of Responses", menuItems[0].Text);
				AssertEquals(1, menuItems[0].MenuItems.Count);
				AssertEquals("Latest Response", menuItems[0].MenuItems[0].Text);
			}
		}

		public void TestSetMessagesGridExtraMenuItemsVisibility()
		{
			var header = CreateNewManifest();
			Factory.Save();
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			using (var grid = new ZGrid())
			{
				var menuItems = provider.GetMessagesGridExtraMenuItems(grid, header).ToArray();
				provider.SetMessagesGridExtraMenuItemsVisibility(menuItems, Factory.New<CUSCAREDIMessage>());
				AssertEquals(1, menuItems.Length);
				AssertEquals("Request Customs Resend of Responses", menuItems[0].Text);
				Assert(menuItems[0].Visible);
				AssertEquals(1, menuItems[0].MenuItems.Count);
				AssertEquals("Latest Response", menuItems[0].MenuItems[0].Text);
				Assert(menuItems[0].MenuItems[0].Visible);

				provider.SetMessagesGridExtraMenuItemsVisibility(menuItems, Factory.New<CUSDECEDIMessage>());
				AssertEquals(1, menuItems.Length);
				AssertEquals("Request Customs Resend of Responses", menuItems[0].Text);
				Assert(!menuItems[0].Visible);
				AssertEquals(1, menuItems[0].MenuItems.Count);
				AssertEquals("Latest Response", menuItems[0].MenuItems[0].Text);
				Assert(!menuItems[0].MenuItems[0].Visible);
			}
		}

		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[] { typeof(AsycudaPackUserControl) };

		[TestDate(2019, 8, 19)]
		public void TestLatestResponse()
		{
			var header = (AsycudaManifestHeader)AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, ZaManifestTypes.Codes.ALM, ApplicationCodeTypeList.Codes.ShippingLine);
			header.FillWithValidTestData();
			header.AMA_ManifestType = ZaManifestTypes.Codes.ALM;
			var message = Factory.New<CUSCAREDIMessage>();
			message.EM_MessageText = @"UNH+360+CUSCAR:D:16A:UN:RCG001'BGM+85:::ALM+4702A6AD067441149AAF7809692B4C2C+9'RFF+LO:MAN0000014'NAD+MS+12342342'NAD+DEG'TDT+20++++:172:20'LOC+60'CNI+1+123434:BOL:123434'RFF+BM:11111'LOC+8'LOC+9'GID+1+0'FTX+AAA++9'MEA+AAE+AAB+KGM:0'PCI+24'UNT+16+360'";
			((IEDIFACTMessageAttachee)header).AddMessage(message);
			Factory.Save();

			using (var form = new ManifestForm(header))
			{
				form.Show();
				((IFormPlugInsProvider)form).TopLevelTabControl.SelectTab("MainTabPage");
				var mainTabControl = form.FindSingle<ZTabControl>(x => x.Name == "mainTabControl");
				var messagesTabPage = mainTabControl.FindSingle<ZTabPage>("mainTabControl_TabPage_MessagesUserControl");
				mainTabControl.SelectedTab = messagesTabPage;

				var messagesGrid = form.FindSingle<ZGrid>(x => x.Name == "messagesGrid");
				messagesGrid.SelectSingleElementByPK(message.PK);
				messagesGrid.ContextMenu.ShowPopupMenu();
				var requestCustomsResendOfResponsesMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText("Request Customs Resend of Responses");
				var latestResponse = requestCustomsResendOfResponsesMenuItem.MenuItems[0];
				latestResponse.PerformClick();
				var req = header.Messages.Cast<EDIMessage>().Single(x => x.EM_MessageType == "REQ");
				AssertEquals("UNH+2+REQDOC:D:99B:UN:ZZZ01'BGM+785+4702A6AD067441149AAF7809692B4C2C+9'DOC+704+123434::::ALM'DTM+318:20190819:102'NAD+MS+12342342'LIN+1'UNT+7+2'", req.EM_MessageText);
				AssertEquals("REQDOC message for 123434 queued for sending", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMasterBOLCaption()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_ManifestType = nameof(Enterprise.Customs.Universal.Messaging.CUSCAR.ManifestDocumentType.ALH);
			manifest.Bills.AddNew();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<DynamicLayoutPanel>("dynamicManifestDetailsPanel");
				var masterBOLTextBox = asycudaManifestUserControl.FindSingle<ZTextBox>("MasterBOLTextBox");
				AssertEquals("Master BOL", masterBOLTextBox.GetExtension<ILabelCaptionRenderer>().Caption);

				manifest.AMA_AgentType = Core.Constants.AgentType.CoLoad;
				AssertEquals("Parent Bill", masterBOLTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestPersonsGridVisibility()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;
				manifest.AMA_TransportMode = Core.Constants.TransportModes.Road;
				var personsTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "personsTabPage");
				mainTabControl.SelectedTab = personsTabPage;
				var personsGrid = personsTabPage.FindSingle<ZGrid>(c => c.Name == "personsGrid");
				var occupationInZAStyle = personsGrid.GetColumnStyle(CusPerson.Schema.OccupationInZA);
				AssertEquals("OccupationInZAStyle.Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(77), occupationInZAStyle.Width);
				var travellerTypeInZAStyle = personsGrid.GetColumnStyle(CusPerson.Schema.TravellerTypeInZA);
				AssertEquals("TravellerTypeInZAStyle.Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(92), travellerTypeInZAStyle.Width);
				var reasonForMovementInZAStyle = personsGrid.GetColumnStyle(CusPerson.Schema.ReasonForMovementInZA);
				AssertEquals("ReasonForMovementInZAStyle.Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(129), reasonForMovementInZAStyle.Width);
				var travelDocumentTypeInZAStyle = personsGrid.GetColumnStyle(CusPerson.Schema.TravelDocumentTypeInZA);
				AssertEquals("TravelDocumentTypeInZAStyle.Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(133), travelDocumentTypeInZAStyle.Width);
			}
		}
	}
}
