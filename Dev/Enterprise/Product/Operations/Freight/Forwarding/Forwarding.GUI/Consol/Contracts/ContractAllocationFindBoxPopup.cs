using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class ContractAllocationFindBoxPopup : IFindBoxPopup
	{
		public ContractAllocationFindBoxPopup(IContractSimulationFormConfiguration configurationFactory)
		{
			this.configurationFactory = configurationFactory;
		}

		readonly IContractSimulationFormConfiguration configurationFactory;

		public SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, EmbeddedModulePopup popup)
		{
			return SilentSelectResult.None;
		}

		public void SelectRowByPK(ZGuid pK)
		{
		}

		public void ShowModal(IFindBox findBox, Form parentForm)
		{
			if (findBox == null || parentForm == null)
			{
				return;
			}

			if (parentForm is ICarrierContractAssignableJobForm form)
			{
				var parentJob = form.ParentJob;
				if (parentJob.TransportMode != Core.Constants.TransportModes.Sea)
				{
					var dialogContext = new DialogDefaultContext(
						new ZGuid("ce3c5d7f-7c95-4753-9efe-7ac02b0ce371"),
						Res.GetString("60b6e30a-8dfc-49f2-985e-c5d062b1bb2e", "Carrier Contract & Allocations"),
						ZMessageBoxButtons.OK,
						ZMessageBoxIcon.Error,
						null);

					var dialogResString = Res.GetString("36bec5c6-8f4c-7a84-4335-9ea6ea55ba76", "Allocations are only available for Sea {0} Type.", parentJob.Name);
					Globals.Message.ShowOrDefault(dialogContext, dialogResString);

					return;
				}
			}

			var formFactory = ObjectFactory.Get<IContractAndAllocationsAttachFormFactory>();
			var modalForm = formFactory.CreateForm(configurationFactory) as ZForm;
			modalForm.FormClosed += (s, e) => Closed?.Invoke(s, e);

			ZFormModaliser.ShowDialogAndDispose(modalForm, parentForm);
		}

		public event EventHandler Closed;
		public void Dispose() { }
	}
}
