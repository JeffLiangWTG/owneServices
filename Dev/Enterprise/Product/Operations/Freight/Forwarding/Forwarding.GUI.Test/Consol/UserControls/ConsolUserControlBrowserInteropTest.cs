#if !WINZOR
using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.BrowserInterop;
using Enterprise.ZArchitecture.GUI.BrowserInterop.Tests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ConsolUserControlBrowserInteropTest : TestCaseWithFactory
	{
		#region BrowserInterop

		public void TestCarrierContractLookup()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_OA_ShippingLineAddress = new Guid();
			consol.JK_CarrierContractNumber = "CarrierNumber";
			consol.JK_TransportMode = "AIR";
			var transport = consol.Transports[0];
			transport.JW_ETD = new DateTime(2021, 1, 1);

			Factory.Save();

			using (var form = new ConsolForm(consol))
			{
				ShowFormAndClickCarrierContractImportButton(form);

				dummyBrowserInteropWindowFactory.MessageTransportLayer.HandleObjectFromBrowser<JToken>(WebViewCommands.Ready);

				var expectedMessage = new BrowserMessageEventArgs<CarrierUpdateFilterData>
				{
					Kind = WebViewCommands.SetFilters,
					Payload = new CarrierUpdateFilterData()
					{
						serviceProviderPK = consol.JK_OA_ShippingLineAddress,
						contractID = "CarrierNumber",
						transportMode = "AIR",
						startDate = new DateTime(2021, 1, 1),
						expiryDate = new DateTime(2021, 1, 1)
					}
				};
				var expectedMessageAsString = JsonConvert.SerializeObject(expectedMessage);
				dummyBrowserInteropWindowFactory.SendMessageToBrowserMock.Verify(v => v.SendToBrowserAsync(expectedMessageAsString));
			}

			Assert("to avoid empty test", true);
		}

		public void TestCarrierContractLookup_ShouldUseJWATDIfNotEmpty()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_OA_ShippingLineAddress = new Guid();
			consol.JK_CarrierContractNumber = "CarrierNumber";
			consol.JK_TransportMode = "AIR";
			var transport = consol.Transports[0];
			transport.JW_ETD = new DateTime(2021, 1, 1);
			var atdDateTime = new DateTime(2021, 1, 2);
			transport.JW_ATD = atdDateTime;

			Factory.Save();

			using (var form = new ConsolForm(consol))
			{
				ShowFormAndClickCarrierContractImportButton(form);

				dummyBrowserInteropWindowFactory.MessageTransportLayer.HandleObjectFromBrowser<JToken>(WebViewCommands.Ready);

				var expectedMessage = new BrowserMessageEventArgs<CarrierUpdateFilterData>
				{
					Kind = WebViewCommands.SetFilters,
					Payload = new CarrierUpdateFilterData()
					{
						serviceProviderPK = consol.JK_OA_ShippingLineAddress,
						contractID = "CarrierNumber",
						transportMode = "AIR",
						startDate = atdDateTime,
						expiryDate = atdDateTime
					}
				};

				var expectedMessageAsString = JsonConvert.SerializeObject(expectedMessage);
				dummyBrowserInteropWindowFactory.SendMessageToBrowserMock.Verify(v => v.SendToBrowserAsync(expectedMessageAsString));
			}

			Assert("to avoid empty test", true);
		}

		public void TestCarrierContractLookup_CloseCommand()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();

			using (var form = new ConsolForm(consol))
			{
				ShowFormAndClickCarrierContractImportButton(form);

				dummyBrowserInteropWindowFactory.MessageTransportLayer.HandleObjectFromBrowser(WebViewCommands.SelectRow, "CCA001");
				AssertEquals("consol carrier number is set", "CCA001", consol.JK_CarrierContractNumber);
			}
		}

		public void TestCarrierContractLookup_CloseCommand_Null()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_CarrierContractNumber = "HOWDTHATGETINTHERE";
			Factory.Save();

			using (var form = new ConsolForm(consol))
			{
				ShowFormAndClickCarrierContractImportButton(form);

				dummyBrowserInteropWindowFactory.MessageTransportLayer.HandleObjectFromBrowser<JToken>(WebViewCommands.SelectRow);
				AssertEquals("consol carrier number is not changed or set to blank", "HOWDTHATGETINTHERE", consol.JK_CarrierContractNumber);
			}
		}

		#endregion

		#region Implementation

		IDisposable[] disposables;
		DummyBrowserInteropWindowFactory dummyBrowserInteropWindowFactory;
		protected override void SetUp()
		{
			base.SetUp();
			dummyBrowserInteropWindowFactory = new DummyBrowserInteropWindowFactory();
			disposables = new[]
			{
				GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glowdev/Portals"),
				ObjectFactory.Substitute<IBrowserInteropWindowFactory>(dummyBrowserInteropWindowFactory),
				FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true),
				FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false),
			};
		}

		protected override void TearDown()
		{
			disposables.ForEach(t => t.Dispose());
			base.TearDown();
		}

		void ShowFormAndClickCarrierContractImportButton(ConsolForm form)
		{
			form.Show();

			var preallocationTabPage = form.Controls.Find("PreAllocationTabPage", true).FirstOrDefault();
			preallocationTabPage.Show();

			var button = form.Controls.Find("CarrierContractImportButton", true)[0] as ZButton.Bare;
			button.PerformClick();
		}

		#endregion
	}
}

#endif
