using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.Forwarding.Documents.GUI
{
	public sealed class AirlineCommodityFindBoxPopup : IFindBoxPopup, IAirlineCommodityFindBoxPopup
	{
		public event EventHandler Closed
		{
			add
			{
				closed += value;
			}
			remove
			{
				closed -= value;
			}
		}

		event EventHandler closed;

		public void Dispose()
		{
			if (closed != null)
			{
				foreach (var handler in closed.GetInvocationList().OfType<EventHandler>())
				{
					closed -= handler;
				}
			}
		}

		public SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, EmbeddedModulePopup popup)
		{
			return SilentSelectResult.None;
		}

		public void SelectRowByPK(ZGuid pK)
		{
		}

		public void ShowModal(IFindBox findBox, Form parentForm)
		{
			if (findBox != null && parentForm != null)
			{
				var form = new AirlineCommodityPopupForm(findBox);
				ZFormModaliser.ShowDialogAndDispose(form, parentForm);
			}

			closed?.Invoke(this, EventArgs.Empty);
		}
	}
}
