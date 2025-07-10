using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public partial class HVLVConsignmentACASUserControl : ZUserControl
	{
		public HVLVConsignmentACASUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			const string IsVisibleForBindingString = "IsVisibleForBinding";

			if (dataSource != null)
			{
				zSendACASAmendmentButton.DataBindings.RemoveBinding(IsVisibleForBindingString);
			}

			base.SetDataBinding(dataSource, dataMember);

			if (dataSource != null)
			{
				zSendACASAmendmentButton.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, GetBindingMemberString(dataSource, "NeedACASAmendment"), false, DataSourceUpdateMode.Never));
			}
		}

		string GetBindingMemberString(object dataSource, string dataMember)
		{
			var result = dataMember;
			if (dataSource is IHVLVConsignmentCollectionParent)
			{
				result = "ConsignmentsFilteredView." + dataMember;
			}

			return result;
		}

		void SendACASAmendment(object sender, EventArgs e)
		{
			if (CurrentConsignment.HasChanges)
			{
				Globals.Message.Show(Res.GetString("4fa6b9f8-b944-46bf-bf93-0d4c5fde78e3", "Please save the form before sending ACAS Amendment."));
			}
			else
			{
				var acasSender = ObjectFactory.Get<IHVLVAirCargoAdvanceScreeningMessageSender>(nameof(IHVLVAirCargoAdvanceScreeningMessageSender), CurrentConsignment.ManifestedOnShipment);
				var validationErrorMessage = acasSender.ValidateACASReportBasicRequirments();

				if (!string.IsNullOrEmpty(validationErrorMessage))
				{
					Globals.Message.Show(validationErrorMessage);
				}
				else
				{
					var wrapper = new HVLVConsignmentForACASWrapper(CurrentConsignment);
					wrapper.RunPreSaveValidation();

					if (wrapper.HasMessageErrors)
					{
						Globals.Message.ShowError(Res.GetString("54c53f7b-ad7d-4157-a951-f0de5585e64a", "There are message errors that need to be corrected before sending ACAS Amendment."));
					}
					else
					{
						if (acasSender.TrySendACASReport(CurrentConsignment, ACASReportAction.SendAmendment, out var errorMessage))
						{
							Globals.Message.ShowInformation(Res.GetString("31a7201f-b1de-44a2-9b8e-57606bb957b9", "ACAS amendment message successfully sent."),
															Res.GetString("188f57d1-8abb-43d5-9042-528b1acb4e26", "ACAS Amendment Sent"));
						}
						else
						{
							Globals.Message.Show(errorMessage);
						}
					}
				}
			}
		}

		HVLVConsignment CurrentConsignment => CurrentDataItem as HVLVConsignment;
	}
}
