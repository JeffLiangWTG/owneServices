using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.DocumentScanning.Business;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.eTail.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public class CancelLastMileCarrierBookingMenuItem(Func<HVLVConsignment> consignmentGetter)
		: BaseLastMileCarrierBookingMenuItem(ResString.GetMultilingualString("da4fe816-586f-473c-9b5c-29bf040c2451", "Cancel Last Mile Carrier Booking"), consignmentGetter)
	{
		public override void UpdateVisibilityAndCaption()
		{
			base.UpdateVisibilityAndCaption();
			Visible = Visible && Consignment.Items.OfType<HVLVItem>().Any(i => i.IsLastMileCarrierBooked);
		}

		protected override Action MenuAction => () =>
		{
			using (var selectItemForm = new HVLVCancelLastMileCarrierSelectItemForm(Consignment))
			{
				ZFormModaliser.ShowDialogWithoutDispose(selectItemForm);
				if (selectItemForm.DialogResult == DialogResult.OK)
				{
					CancelLastMileCarrierBookingCore(selectItemForm.SelectedItems);
				}
			}
		};

		void CancelLastMileCarrierBookingCore(IEnumerable<HVLVItem> itemsToCancel)
		{
			try
			{
				var hvlvItemRTUSBookingProvider = ObjectFactory.Get<ILastMileCarrierBookingService>(nameof(HVLVItemRTUSBookingProvider), Consignment.Factory);
				var anyItemsCancelledSuccessfully = false;
				foreach (var item in itemsToCancel)
				{
					var lastMileCarrierBookingResponseCollection = (IEnumerable<ILastMileCarrierBookingResponse>)hvlvItemRTUSBookingProvider.CancelBooking(item.PK.ToGuid());
					foreach (var response in lastMileCarrierBookingResponseCollection)
					{
						if (response.Successful)
						{
							anyItemsCancelledSuccessfully = true;
							DeleteLabelFileIfExists(item);
						}
						else
						{
							Globals.Message.ShowError(response.ErrorMessage);
						}
					}
				}

				if (anyItemsCancelledSuccessfully)
				{
					Globals.Message.ShowInformation(Res.GetString("f6e35dc0-8059-462c-b4d8-4d2b1beb7176", "Last Mile Carrier Booking(s) have been canceled."));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		void DeleteLabelFileIfExists(HVLVItem item)
		{
			var allEDocsOfConsignment = ((IDocManagerSupport)item.Consignment).DocManagerInfo.AllEDocs.Cast<IeDoc>();
			var file = allEDocsOfConsignment.SingleOrDefault(n => n.FileNameOnly == item.HVI_ItemId);
			if (file != null && !file.IsDeleted)
			{
				(file as StorageFile).DeleteQuietly();
			}
		}
	}
}
