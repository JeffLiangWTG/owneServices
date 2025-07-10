using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Windows.UI;
using Enterprise.eTail.DataTransfer;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	class HVLVCustomsMenuGroupTest : TestCaseWithFactory
	{
		public void TestSubMenuItems()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_HouseBill = "HouseBill001";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.JS_RL_NKOrigin = "USLAX";

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_MasterBillNum = "MAWB1234";
				consol.Transports.MostInterestingTransport.JW_VoyageFlight = "Flight100";
				consol.Shipments.Add(shipment);

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();
					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var menuItem = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					menuItem.OnPopup(EventArgs.Empty);

					AssertEquals(7, menuItem.MenuItems.Count);

					var menuCollection = menuItem.MenuItems.OfType<ZMenuItem>().Select(x => x.GetType()).ToList();
					AssertCollectionContains(typeof(CommandJobTypeMenuGroup<AUAirCargoReportCommand>), menuCollection);
					AssertCollectionContains(typeof(DeclarationMenuGroup), menuCollection);
					AssertCollectionContains(typeof(CreateDA306MenuItem), menuCollection);
					AssertCollectionContains(typeof(USISFMenuGroup), menuCollection);
					AssertCollectionContains(typeof(SendACASMenuGroup), menuCollection);
					Assert("This missing one menu Item is a test command class", true);
				}
			}
		}

		public void TestPopupCustomsMenuGroup_RefreshSubMenuGroups()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_HouseBill = "HouseBill001";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.JS_RL_NKOrigin = "USLAX";

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_MasterBillNum = "MAWB1234";
				consol.Transports.MostInterestingTransport.JW_VoyageFlight = "Flight100";
				consol.Shipments.Add(shipment);

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var menuItem = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					menuItem.OnPopup(EventArgs.Empty);

					var menuCollection = menuItem.MenuItems.OfType<ZMenuItem>().Select(x => x.GetType()).ToList();
					AssertCollectionContains(typeof(CommandJobTypeMenuGroup<AUAirCargoReportCommand>), menuCollection);
					AssertCollectionNotContains(typeof(CommandJobTypeMenuGroup<AUSeaCargoReportCommand>), menuCollection);

					shipment.JS_TransportMode = TransportModes.Sea;
					menuItem.OnPopup(EventArgs.Empty);

					menuCollection = menuItem.MenuItems.OfType<ZMenuItem>().Select(x => x.GetType()).ToList();
					AssertCollectionContains(typeof(CommandJobTypeMenuGroup<AUSeaCargoReportCommand>), menuCollection);
					AssertCollectionNotContains(typeof(CommandJobTypeMenuGroup<AUAirCargoReportCommand>), menuCollection);
				}
			}
		}

		public void TestPopupCustomsMenuGroup_ShouldNotLoadGenPivot()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_HouseBill = "HouseBill001";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.JS_RL_NKOrigin = "USLAX";

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_MasterBillNum = "MAWB1234";
				consol.Transports.MostInterestingTransport.JW_VoyageFlight = "Flight100";
				consol.Shipments.Add(shipment);

				Factory.Save();

				var expectedDbHitsWithFetchHint = new Dictionary<string, int>()
				{
					{ GenPivotSchema.Constants.TableName, 0 }
				};

				var newFactory = new BusinessObjectFactory();
				shipment = newFactory.Load<ForwardingShipment>(shipment.PK);

				using (var form = new ZForm(shipment))
				using (AssertDbHitsWithUsefulQueryInformation(expectedDbHitsWithFetchHint, newFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);
				}
			}
		}

		public void TestH7MenuItemForFeatureControlForEU()
		{
			AssertH7MenuItemForFeatureControl(CountryCodes.Spain, typeof(CommandJobTypeMenuGroup<ESH7DeclarationCommand>));
		}

		public void TestH7MenuItemForFeatureControlForGB()
		{
			AssertH7MenuItemForFeatureControl(CountryCodes.UnitedKingdom, typeof(CommandJobTypeMenuGroup<GBH7DeclarationCommand>));
		}

		public void TestH7MenuItemForFeatureControlForIT()
		{
			AssertH7MenuItemForFeatureControl(CountryCodes.Italy, typeof(CommandJobTypeMenuGroup<ITH7DeclarationCommand>));
		}

		public void TestH7MenuItemForFeatureControlForFR()
		{
			AssertH7MenuItemForFeatureControl(CountryCodes.France, typeof(CommandJobTypeMenuGroup<FRH7DeclarationCommand>));
		}

		public void AssertH7MenuItemForFeatureControl(string countryCode, Type h7JobMenuGroupType)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_HouseBill = "HouseBill001";
				shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.HomePort.Code;
				shipment.JS_RL_NKOrigin = "USLAX";

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_MasterBillNum = "MAWB1234";
				consol.Transports.MostInterestingTransport.JW_VoyageFlight = "Flight100";
				consol.Shipments.Add(shipment);

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var menuItem = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					menuItem.OnPopup(EventArgs.Empty);

					var menuCollection = menuItem.MenuItems.OfType<ZMenuItem>().Select(x => x.GetType()).ToList();
					AssertCollectionNotContains("By default the H7 job is not loaded", h7JobMenuGroupType, menuCollection);

					var h7FeatureCode = CargoWise.Definitions.LicenceFeatureCodeList.Codes.EcommerceH7Feature;
					var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
					using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
					{
						MockIFeatureControlManager(mockIFeatureControlManager, GetMockFeatureData(h7FeatureCode), h7FeatureCode);
						menuItem.OnPopup(EventArgs.Empty);
						menuCollection = menuItem.MenuItems.OfType<ZMenuItem>().Select(x => x.GetType()).ToList();
						AssertCollectionContains("Feature code exists but parameter is null, load H7 job", h7JobMenuGroupType, menuCollection);

						var parameters = "{\"AuthorizedCountries\": [],  \"AuthorizedCompanies\": []}";
						MockIFeatureControlManager(mockIFeatureControlManager, GetMockFeatureData(h7FeatureCode, parameters), h7FeatureCode);
						menuItem.OnPopup(EventArgs.Empty);
						menuCollection = menuItem.MenuItems.OfType<ZMenuItem>().Select(x => x.GetType()).ToList();
						AssertCollectionNotContains("Parameter contains AuthorizedCountries and AuthorizedCompanies but both empty, don't load H7 job", h7JobMenuGroupType, menuCollection);

						parameters = $"{{\"AuthorizedCountries\": [\"$$\", \"{countryCode}\"],  \"AuthorizedCompanies\": []}}";
						MockIFeatureControlManager(mockIFeatureControlManager, GetMockFeatureData(h7FeatureCode, parameters), h7FeatureCode);
						menuItem.OnPopup(EventArgs.Empty);
						menuCollection = menuItem.MenuItems.OfType<ZMenuItem>().Select(x => x.GetType()).ToList();
						AssertCollectionContains("AuthorizedCountries contains login country, load H7 job", h7JobMenuGroupType, menuCollection);

						parameters = $"{{\"AuthorizedCountries\": [],  \"AuthorizedCompanies\": [{{\"CountryCode\": \"{countryCode}\",\"CompanyCodes\": [\"{GlbCompany.CurrentCompany.GC_Code}\"]}}]}}";
						MockIFeatureControlManager(mockIFeatureControlManager, GetMockFeatureData(h7FeatureCode, parameters), h7FeatureCode);
						menuItem.OnPopup(EventArgs.Empty);
						menuCollection = menuItem.MenuItems.OfType<ZMenuItem>().Select(x => x.GetType()).ToList();
						AssertCollectionContains("AuthorizedCompanies contains login country and login company, load H7 job", h7JobMenuGroupType, menuCollection);

						parameters = $"{{\"AuthorizedCountries\": [\"%%\"],  \"AuthorizedCompanies\": [{{\"CountryCode\": \"{countryCode}\",\"CompanyCodes\": [\"$$$\"]}}]}}";
						MockIFeatureControlManager(mockIFeatureControlManager, GetMockFeatureData(h7FeatureCode, parameters), h7FeatureCode);
						menuItem.OnPopup(EventArgs.Empty);
						menuCollection = menuItem.MenuItems.OfType<ZMenuItem>().Select(x => x.GetType()).ToList();
						AssertCollectionNotContains("Neither AuthorizedCountries nor AuthorizedCompanies contains the current login country and company, don't load H7 job", h7JobMenuGroupType, menuCollection);
					}
				}
			}
		}

		IFeatureData GetMockFeatureData(string featureControlCode, string parameters = null)
		{
			var featureControlRule = new FeatureControlRule
			{
				FCM_FeatureControlCode = featureControlCode,
				FCR_Parameters = parameters
			};

			return new FeatureData(featureControlRule);
		}

		void MockIFeatureControlManager(Mock<IFeatureControlManager> mockIFeatureControlManager, IFeatureData featureData, string h7FeatureCode)
		{
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(h7FeatureCode, CancellationToken.None)).Returns(Task.FromResult(featureData));
		}
	}
}
