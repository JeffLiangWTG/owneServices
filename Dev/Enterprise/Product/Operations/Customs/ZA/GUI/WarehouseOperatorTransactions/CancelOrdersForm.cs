using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public class CancelOrdersForm : TransactionSelectionForm
	{
		public CancelOrdersForm(BusinessObjectFactory factory, ZGuid warehousePK, ZGuid productOwnerPK, string ownerReference)
			: this(new CancelOrdersController(factory, warehousePK, productOwnerPK, ownerReference))
		{
		}

		public CancelOrdersForm(CancelOrdersController businessObject) : base(businessObject)
		{
			CaptionResourceString = Res.GetData("5A9F5D9D-7C79-48A6-BB9B-59034A93A0CE", "Cancel Unallocated Order");
		}

		protected override bool IsConfirmedToProcess
		{
			get
			{
				var result = false;
				var selected = Controller.SelectedRecords;
				if (selected.Count == 0)
				{
					Globals.Message.ShowError(Res.GetString("12588D2F-F9D2-4414-A5FD-0EB59200207E", "No order is selected!"));
				}
				else
				{
					var message = Res.GetString("FA291772-A6E0-4817-BBF5-DA4A4D64CD27", "{0} Orders will be canceled, type \"yes\" to continue", selected.Count);
					result = Globals.Message.ShowConfirmation(message, "Confirmation", "yes", ZMessageBoxIcon.Question) == ZDialogResult.OK;
				}
				return result;
			}
		}

		public static void ShowDialog(BusinessObjectFactory factory, ZGuid warehousePK, ZGuid productOwnerPK)
		{
			var argument = new UserResponseArgument
			{
				Caption = Res.GetString("5A9F5D9D-7C79-48A6-BB9B-59034A93A0CE", "Cancel Unallocated Order"),
				Message = Res.GetString("4F8C29A5-A1EF-4E4D-9C97-1951E30E942F", "Owner Reference"),
				MinimumResponseLength = 1,
				MaximumResponseLength = 35
			};
			var ownerReference = Globals.Message.QueryUserResponse(argument);
			if (!string.IsNullOrEmpty(ownerReference))
			{
				using (var ordersForm = new CancelOrdersForm(factory, warehousePK, productOwnerPK, ownerReference))
				{
					if (ordersForm.Controller.CanProceed)
					{
						ZFormModaliser.ShowDialogWithoutDispose(ordersForm);
					}
					else
					{
						Globals.Message.ShowInformation(Res.GetString("07634535-610C-47BF-A57A-06B0AF60D1B9", "Cannot find unallocated orders for \"{0}\"!", ownerReference));
					}
				}
			}
		}
	}
}
