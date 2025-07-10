using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

#if !WINZOR
using Enterprise.ZArchitecture.GUI.BrowserInterop;
#endif

namespace Enterprise.Rating.GUI
{
	internal class ContractNumberFindBoxPopup : IFindBoxPopup
	{
		public event EventHandler Closed;

		public void Dispose()
		{
		}

		public SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, EmbeddedModulePopup popup)
		{
			return SilentSelectResult.None;
		}

		public void SelectRowByPK(ZGuid pK) { }

		public void ShowModal(IFindBox findBox, Form parentForm)
		{
			var contractInfo = findBox as IContractNumberFindBoxPopupSupport;
			var showCW1popup = ObjectFactory.Get<IContractPermissions>().IsAllocationsVisible();

			if (showCW1popup && contractInfo.RateEntry.IsCosting())
			{
				ShowCW1Popup(contractInfo);
			}
			else
			{
				ShowGlowPopup(contractInfo);
			}
			Closed?.Invoke(this, EventArgs.Empty);
		}

		void ShowCW1Popup(IContractNumberFindBoxPopupSupport contractInfo)
		{
			var simulation = new ContractNumberFindBoxContractAndAllocationSimulation(contractInfo);

			simulation.ShowPopup();
		}

		void ShowGlowPopup(IContractNumberFindBoxPopupSupport contractInfo)
		{
#if !WINZOR
			ContractNumberGlowPopupProxy proxy = default;

			if (contractInfo.RateEntry.IsCosting())
			{
				proxy = new CarrierContractNumberGlowPopupProxy(contractInfo);
			}
			else if (contractInfo.RateEntry.IsClientRate())
			{
				proxy = new ClientContractNumberGlowPopupProxy(contractInfo);
			}

			if (proxy.TryGenerateURL(out var url))
			{
				var browserWindowFactory = ObjectFactory.Get<IBrowserInteropWindowFactory>();
				var windowTitle = proxy.WindowTitle;
				var browserWindow = browserWindowFactory.CreateBrowserInteropWindow(windowTitle, url);

				BrowserInteropHelperExtensions.AddBrowserCloseCommandHandler(browserWindow, data =>
				{
					if (!string.IsNullOrEmpty(data))
					{
						proxy.ContractNumber = data;
					}
				});
				BrowserInteropHelperExtensions.AddBrowserListenerCommandHandler(browserWindow, () => proxy.GetUpdateFilterData());

				browserWindow.ShowDialog();
			}
#endif
		}
	}

	public interface IContractNumberFindBoxPopupSupport
	{
		/// <summary>
		/// This contract number represents the number visible on the textbox
		/// in the findbox.This can be a number different to the one committed
		/// into the RateEntry bizo. The RateEntry.TI_ContractNumber is only set
		/// *after* the user tabs away from the findbox. 
		/// </summary>

		string ContractNumber { get; set; }

		/// <summary>
		/// This is the rate entry of the row it belongs to.
		/// Should not be used to update the RateEntry directly. Instead, set
		/// the ContractNumber above
		/// </summary>
		IRateEntry RateEntry { get; set; }
	}
}
