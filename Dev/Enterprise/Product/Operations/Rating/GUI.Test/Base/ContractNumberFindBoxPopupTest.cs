using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.BrowserInterop;
using Enterprise.ZArchitecture.GUI.BrowserInterop.Tests;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	internal class ContractNumberFindBoxPopupTest : RatingTestCase
	{
#if !WINZOR
		#region GLOW popup tests

		#region Client contract number

		public void TestGlow_ClientContractNumber_UpdateFilters_LCLTransportModesResolveToSEA() => AssertClientRate_TransportModeAppearsAsSEA(Core.Constants.RateMode.LCL);
		public void TestGlow_ClientContractNumber_UpdateFilters_FCLTransportModesResolveToSEA() => AssertClientRate_TransportModeAppearsAsSEA(Core.Constants.RateMode.FCL);
		public void TestGlow_ClientContractNumber_UpdateFilters_SEATransportModesResolveToSEA() => AssertClientRate_TransportModeAppearsAsSEA(Core.Constants.RateMode.SEA);

		void AssertClientRate_TransportModeAppearsAsSEA(string transportMode)
		{
			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			// EnableCarrierContractAllocations can remain True since the CW1 popup should not interfere
			// with the GLOW popup when it is a client rate.
			FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var orgHeader = NewOrganization();
			var client = Helper.NewClientRate(orgHeader);
			var contractNumberStyleInfo = GetContractNumberFindboxStyleInfo<ClientRate>();

			var entry = GetEntry(client);
			entry.TI_Mode = transportMode;

			using (var contractNumberStyle = new ContractNumberFindBoxColumnStyle(contractNumberStyleInfo))
			using (var form = new Form())
			using (var findBox = contractNumberStyle.FindBox)
			{
				findBox.ContractNumber = "Thisone!";

				ShowPopup(form, findBox, entry);
				dummyBrowserInteropWindowFactory.MessageTransportLayer.HandleObjectFromBrowser<JToken>(WebViewCommands.Ready);
				var expectedMessage = new BrowserMessageEventArgs<ClientUpdateFilterData>
				{
					Kind = WebViewCommands.SetFilters,
					Payload = new ClientUpdateFilterData()
					{
						clientPK = orgHeader.PK,
						contractID = "Thisone!",
						startDate = ZDateTime.BrettsBirthday,
						expiryDate = ZDateTime.BrettsBirthday.AddYears(1)
					}
				};
				var expectedMessageAsString = JsonConvert.SerializeObject(expectedMessage);
				dummyBrowserInteropWindowFactory.SendMessageToBrowserMock.Verify(v => v.SendToBrowserAsync(expectedMessageAsString));
			}

			Assert("If it had failed, the Verify would have failed", true);
		}

		[GuiTest]
		public void TestGlow_ClientContractNumber_LookupSelectionMade()
		{
			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			// EnableCarrierContractAllocations can remain True since the CW1 popup should not interfere
			// with the GLOW popup when it is a client rate.
			FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var orgHeader = NewOrganization();
			var client = Helper.NewClientRate(orgHeader);
			var contractNumberStyleInfo = GetContractNumberFindboxStyleInfo<ClientRate>();

			var entry = GetEntry(client);
			entry.TI_ContractNumber = "first";

			using (var contractNumberStyle = new ContractNumberFindBoxColumnStyle(contractNumberStyleInfo))
			using (var form = new Form())
			using (var findBox = contractNumberStyle.FindBox)
			{
				findBox.ContractNumber = "second";

				ShowPopup(form, findBox, entry);
				dummyBrowserInteropWindowFactory.MessageTransportLayer.HandleObjectFromBrowser(WebViewCommands.SelectRow, "new-contract-number");

				// TI_ContractNumber changing cannot be tested directly since the findBox would need to be in a ZGrid for it to work.
				// this unit test is simple and does not have that.
				AssertEquals("rate Client contract number is not changed.", "first", entry.TI_ContractNumber);
				AssertEquals("codebox text box is updated", "new-contract-number", findBox.ContractNumber);
			}
		}

		#endregion

		#region Carrier contract number

		public void TestGlow_CarrierContractNumber_UpdateFilters_LCLTransportModesResolveToSEA() => AssertCosting_TransportModeAppearsAsSEA(Core.Constants.RateMode.LCL);
		public void TestGlow_CarrierContractNumber_UpdateFilters_FCLTransportModesResolveToSEA() => AssertCosting_TransportModeAppearsAsSEA(Core.Constants.RateMode.FCL);
		public void TestGlow_CarrierContractNumber_UpdateFilters_SEATransportModesResolveToSEA() => AssertCosting_TransportModeAppearsAsSEA(Core.Constants.RateMode.SEA);

		void AssertCosting_TransportModeAppearsAsSEA(string transportMode)
		{
			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var orgHeader = NewOrganization();
			var costing = Helper.NewCosting(orgHeader);
			var contractNumberStyleInfo = GetContractNumberFindboxStyleInfo<Costing>();
			var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR");
			AssertEquals("Precondition", "FLT", container.RC_ContainerType);

			var entry = GetEntry(costing);
			entry.TI_Mode = transportMode;
			entry.TI_RC = container.PK;
			entry.TI_RH_NKCommodityCode = "HAZ";

			using (var contractNumberStyle = new ContractNumberFindBoxColumnStyle(contractNumberStyleInfo))
			using (var form = new Form())
			using (var findBox = contractNumberStyle.FindBox)
			{
				findBox.ContractNumber = "Thisone!";

				ShowPopup(form, findBox, entry);
				dummyBrowserInteropWindowFactory.MessageTransportLayer.HandleObjectFromBrowser<JToken>(WebViewCommands.Ready);
				var expectedMessage = new BrowserMessageEventArgs<CarrierUpdateFilterData>
				{
					Kind = WebViewCommands.SetFilters,
					Payload = new CarrierUpdateFilterData()
					{
						serviceProviderPK = orgHeader.PK,
						contractID = "Thisone!",
						transportMode = "SEA",
						startDate = ZDateTime.BrettsBirthday,
						isCommodityHazardousRequired = true,
						expiryDate = ZDateTime.BrettsBirthday.AddYears(1),
						containerType = "FLT"
					}
				};
				var expectedMessageAsString = JsonConvert.SerializeObject(expectedMessage);
				dummyBrowserInteropWindowFactory.SendMessageToBrowserMock.Verify(v => v.SendToBrowserAsync(expectedMessageAsString));
			}

			Assert("If it had failed, the Verify would have failed", true);
			ExceptionReporterTestListener.Instance.Clear();
		}

		[GuiTest]
		public void TestGlow_CarrierContractNumber_LookupSelectionMade()
		{
			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var orgHeader = NewOrganization();
			var costing = Helper.NewCosting(orgHeader);
			var contractNumberStyleInfo = GetContractNumberFindboxStyleInfo<Costing>();

			var entry = GetEntry(costing);
			entry.TI_ContractNumber = "first";

			using (var contractNumberStyle = new ContractNumberFindBoxColumnStyle(contractNumberStyleInfo))
			using (var form = new Form())
			using (var findBox = contractNumberStyle.FindBox)
			{
				findBox.ContractNumber = "second";

				ShowPopup(form, findBox, entry);
				dummyBrowserInteropWindowFactory.MessageTransportLayer.HandleObjectFromBrowser(WebViewCommands.SelectRow, "new-contract-number");

				// TI_ContractNumber changing cannot be tested directly since the findBox would need to be in a ZGrid for it to work.
				// this unit test is simple and does not have that.
				AssertEquals("rate carrier contract number is not changed.", "first", entry.TI_ContractNumber);
				AssertEquals("codebox text box is updated", "new-contract-number", findBox.ContractNumber);
			}
		}

		#endregion

		#endregion
#endif
		#region CW1 popup tests

		[GuiTest]
		public void TestCW1_CarrierContractNumber_SetContractButtonVisible_SetAllocationButtonNotVisible()
		{
			TestContractNumberPopup(enableClientCarrierContractRegistry: true, enableContractAllocationRegistry: true, expectFormSeen: true, (shownForm) =>
			{
				if (shownForm is ZForm form)
				{
					var contractButton = form.Controls.Find("SelectContractButton", true).Single() as ZButton;
					var allocationButton = form.Controls.Find("SelectAllocationRouteButton", true).Single() as ZButton;

					AssertEquals(true, contractButton.Visible);
					AssertEquals(false, allocationButton.Visible);
				}
			});
		}

		#endregion

		#region Glow/CW1 Popup decision tests

		[GuiTest]
		public void TestPopup_WhenContractAllocationModuleNo_AndIsClientCarrierContractYes_ShowGlowPopup()
		{
			TestContractNumberPopup(enableClientCarrierContractRegistry: true, enableContractAllocationRegistry: false, expectFormSeen: false);
			AssertGlowPopup(expectPopup: !IsWinzor);
		}

		[GuiTest]
		public void TestPopup_WhenContractAllocationModuleNo_AndIsClientCarrierContractNo_NoPopupSHown()
		{
			TestContractNumberPopup(enableClientCarrierContractRegistry: false, enableContractAllocationRegistry: false, expectFormSeen: false);
			AssertGlowPopup(expectPopup: false);
		}

		[GuiTest]
		public void TestPopup_WhenContractAllocationModuleYes_AndIsClientCarrierContractYes_ShowCW1Popup()
		{
			TestContractNumberPopup(enableClientCarrierContractRegistry: true, enableContractAllocationRegistry: true, expectFormSeen: true, (shownForm) =>
			{
				AssertEquals("Shown form must be the attachment form", "ContractAndAllocationsAttachForm", shownForm.Name);
			});
			AssertGlowPopup(expectPopup: false);
		}

		[GuiTest]
		public void TestPopup_WhenContractAllocationModuleYes_AndIsClientCarrierContractNo_NoPopupShown()
		{
			TestContractNumberPopup(enableClientCarrierContractRegistry: false, enableContractAllocationRegistry: true, expectFormSeen: false);
			AssertGlowPopup(expectPopup: false);
		}

		void AssertGlowPopup(bool expectPopup)
		{
			var popupShown = dummyBrowserInteropWindowFactory.windowsCreated > 0;
			AssertEquals("UrlIncludeList being populated hints at the GLOW popup appearing", expectPopup, popupShown);
		}

		#endregion

		void TestContractNumberPopup(bool enableClientCarrierContractRegistry, bool enableContractAllocationRegistry, bool expectFormSeen)
			=> TestContractNumberPopup(enableClientCarrierContractRegistry, enableContractAllocationRegistry, expectFormSeen, (form) => { });

		void TestContractNumberPopup(bool enableClientCarrierContractRegistry, bool enableContractAllocationRegistry, bool expectFormSeen, Action<Form> testFunction)
		{
			var formSeen = false;

			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableContractAllocationRegistry))
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableClientCarrierContractRegistry))
			{
				var orgHeader = NewOrganization();
				var costing = Helper.NewCosting(orgHeader);
				var contractNumberStyleInfo = GetContractNumberFindboxStyleInfo<Costing>();
				var entry = GetEntry(costing);

				if (contractNumberStyleInfo != null)
				{
					ZFormModaliser.ShowDialogsInTest = true;
					using (var contractNumberStyle = new ContractNumberFindBoxColumnStyle(contractNumberStyleInfo))
					using (var form = new Form())
					using (var findBox = contractNumberStyle.FindBox)
					{
						findBox.ContractNumber = "second";

						ZFormModaliser.SetDelegateToCallOnFormShown((shownForm) =>
						{
							testFunction((Form)shownForm);
							formSeen = true;
						});

						ShowPopup(form, findBox, entry);
					}
				}
			}

			AssertEquals(expectFormSeen, formSeen);
		}

		#region Setup/Tear down/helpers

		IDisposable[] disposables;
		DummyBrowserInteropWindowFactory dummyBrowserInteropWindowFactory;

		protected override void SetUp()
		{
			base.SetUp();
			dummyBrowserInteropWindowFactory = new DummyBrowserInteropWindowFactory();
			disposables = new[]
			{
				GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glowdev/Portals"),
				ObjectFactory.Substitute<IBrowserInteropWindowFactory>(dummyBrowserInteropWindowFactory)
			};
		}

		protected override void TearDown()
		{
			disposables.ForEach(t => t.Dispose());
			base.TearDown();
		}

		static ContractNumberFindBoxColumnStyleInfo GetContractNumberFindboxStyleInfo<T>() where T : RatingHeader
		{
			var uiCollection = RateEntryCollectionGUIInfo.GetInfo(RatingConstants.RateCategory.FCL, typeof(T), false);
			var contractNumberStyleInfo = uiCollection.Columns.Single(c => c.ColumnName == RateEntry.Schema.TI_ContractNumber) as ContractNumberFindBoxColumnStyleInfo;
			return contractNumberStyleInfo;
		}

		static void ShowPopup(Form form, ContractNumberGridFindBox findBox, RateEntry entry)
		{
			findBox.RateEntry = entry;
			findBox.Visible = true;
			findBox.Size = new System.Drawing.Size(50, 50);
			form.Controls.Add(findBox);

			form.Show();
			Application.DoEvents();

			findBox.PopupButton.PerformClick();
			Application.DoEvents();
		}

		static RateEntry GetEntry(RatingHeader costing)
		{
			var entryCollection = costing.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.FCL);
			var entry = entryCollection.AddNew();
			entry.TI_ContractNumber = "not-this-number";
			entry.TI_Mode = "SEA";
			entry.TI_RateStartDate = ZDate.BrettsBirthday;
			entry.TI_RateEndDate = ZDate.BrettsBirthday.AddYears(1);
			return entry;
		}

		OrgHeader NewOrganization()
		{
			var orgHeader = Helper.NewOrgHeader();
			orgHeader.OH_FullName = "CONSIGNOR";
			orgHeader.OH_Code = "CON";
			orgHeader.OH_IsConsignor = true;
			return orgHeader;
		}

#if WINZOR
		protected bool IsWinzor => true;
#else
		protected bool IsWinzor => false;
#endif

		#endregion
	}
}
