using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	static class AttachOrderToShipmentHelper
	{
		public static void OrderLineToPackLineConversion(object sender, OrderLineToPackLineConversionEventArgs e)
		{
			var dialogCaption = ResString.GetMultilingualString("158BE79B-DC9F-46f1-8E50-7421ACB62A48", "Link order lines to pack-lines");

			if (string.IsNullOrEmpty(e.ReasonForNotAbleToConvert))
			{
				var dialogContext = new DialogDefaultContext(
					new ZGuid("3c1844c0-e366-4aca-a50e-4861c3a5adce"),
					dialogCaption,
					ZMessageBoxButtons.YesNo,
					ZMessageBoxIcon.Question,
					null,
					showCheckboxOnly: true);

				var dialogResult = Globals.Message.ShowOrDefault(dialogContext, ResString.GetMultilingualString("43E6F96A-2620-43aa-AF2E-48C17C38ACB9", "Do you want to link the order lines from the recently attached orders to pack-lines on this shipment?"));
				if (dialogResult == ZDialogResult.Yes)
				{
					e.ShouldCreatePacklines = true;
					ZFormModaliser.ShowDialogAndDispose(new OrderLineToPackLineConversionForm(e.Helper));
				}
				else
				{
					e.ShouldCreatePacklines = false;
				}
			}
			else
			{
				Globals.Message.Show(e.ReasonForNotAbleToConvert, dialogCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}
	}
}
