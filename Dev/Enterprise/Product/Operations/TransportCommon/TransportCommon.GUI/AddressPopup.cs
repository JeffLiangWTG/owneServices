using System;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportCommon.GUI
{
	public class AddressPopup : IDisposable
	{
		public void ShowPopupAddress(ZForm form, DocAddressType docAddressType)
		{
			ShowPopupAddress((BusinessObject)form.BusinessEntity, null, form, docAddressType);
		}

		public void ShowPopupAddress(ZGrid gridWithAddress, ZString bizOHumanReadableName, DocAddressType docAddressType)
		{
			var selectedBizO = gridWithAddress.GetCurrent();
			if (selectedBizO != null)
			{
				var listMgr = gridWithAddress.ListManager;
				if (listMgr != null)
				{
					listMgr.EndCurrentEdit(); // handle case when editing a new row
				}

				ShowPopupAddress(selectedBizO, gridWithAddress, gridWithAddress.FindForm(), docAddressType);
			}
			else
			{
				var message = Res.GetString("b1285322-7dbd-4219-a343-41b8f82c0e6c", "Please select a {0}.", bizOHumanReadableName);
				var caption = Res.GetString("6b5f7b51-591f-4f22-b15b-120471df69dd", "Warning");
				Globals.Message.ShowWarning(message, caption);
			}
		}

		void ShowPopupAddress(BusinessObject selectedBizO, Control control, Form form, DocAddressType docAddressType)
		{
			var selectedAddress = selectedBizO as IAddress
				?? throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Business Object {0} must implement {1} before using the {2}.",
					selectedBizO.GetType().Name, nameof(IAddress), this.GetType().Name));

			var address = selectedAddress.GetAddress(docAddressType);
			var host = new DocAddressCreatorHost(address, address.Factory);
			quickAddressForm = new QuickAddressForm(host, control);
			ZFormModaliser.Show(quickAddressForm, form);
		}

		QuickAddressForm quickAddressForm;

		#region Dispose

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (quickAddressForm != null)
			{
				quickAddressForm.Dispose();
			}
		}

		#endregion
	}
}
