using System.Linq;
using System.Windows.Forms;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public partial class HVLVCancelLastMileCarrierSelectItemForm : HVLVSelectConsignmentItemForm
	{
		public HVLVCancelLastMileCarrierSelectItemForm(HVLVConsignment consignment)
			: base(consignment)
		{
			CustomiseForm();
		}

		void CustomiseForm()
		{
			ButtonOk.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 390, true);
			ButtonOk.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 25, true);
			ButtonOk.Text = ResString.GetMultilingualString("58485608-509d-440c-be5e-df7341e726df", "Cancel Booking");
			CaptionResourceString = Res.GetData("54641bb9-8af3-430c-bd4e-15c08561e98c", "Cancel Last Mile Carrier Booking");
			Name = "HVLVCancelLastMileCarrierSelectItemForm";
		}

		protected override TreeNode BuildTreeNodes(HVLVConsignment consignment)
		{
			var root = new TreeNode(consignment.HumanReadableName);
			var bookedItems = consignment.Items.OfType<HVLVItem>().Where(item => item.IsLastMileCarrierBooked);
			foreach (var item in bookedItems)
			{
				var itemLabel = ResString.GetMultilingualString("de93eb6b-122f-4608-b6a0-ad5869b641d7", "Last Mile Carrier Booking for Item ID: {0}", item.HVI_ItemId);
				if (!item.HVI_ShipperReference.IsEmpty)
				{
					itemLabel = ResString.GetMultilingualString("c636757e-bba9-4fd8-98d1-bfd3edce6a76", "Last Mile Carrier Booking for Item Shipper Reference: {0}", item.HVI_ShipperReference);
				}
				else if (!item.HVI_CurrentBarcode.IsEmpty)
				{
					itemLabel = ResString.GetMultilingualString("da623db5-aa8d-4230-a2fe-4a494ba17299", "Last Mile Carrier Booking for Item Barcode: {0}", item.HVI_CurrentBarcode);
				}

				root.Nodes.Add(new ZBusinessObjectTreeNode(item, itemLabel));
			}

			return root;
		}
	}
}
