using System.Windows.Forms;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public partial class HVLVBookLastMileCarrierSelectItemForm : HVLVSelectConsignmentItemForm
	{
		public HVLVBookLastMileCarrierSelectItemForm(HVLVConsignment consignment)
			: base(consignment)
		{
			CustomiseForm();
		}

		void CustomiseForm()
		{
			ButtonOk.Text = ResString.GetMultilingualString("a86d02ff-93e1-4d5e-a888-0a066265c0e3", "Book");
			CaptionResourceString = Res.GetData("5699a9b1-a3a7-44d9-b811-38dd116b63ed", "Book Last Mile Carrier");
			Name = "HVLVBookLastMileCarrierSelectItemForm";
		}

		protected override TreeNode BuildTreeNodes(HVLVConsignment consignment)
		{
			var root = new TreeNode(consignment.HumanReadableName);

			foreach (HVLVItem item in consignment.Items)
			{
				var itemLabel = ResString.GetMultilingualString("764e06f2-35c1-4500-8067-a67f8856ab32", "Last Mile Carrier Booking for Item ID: {0}", item.HVI_ItemId);
				if (!item.HVI_ShipperReference.IsEmpty)
				{
					itemLabel = ResString.GetMultilingualString("231732d7-59aa-4789-b30c-56343c34f3b2", "Last Mile Carrier Booking for Item Shipper Reference: {0}", item.HVI_ShipperReference);
				}
				else if (!item.HVI_CurrentBarcode.IsEmpty)
				{
					itemLabel = ResString.GetMultilingualString("24ee0f8d-69ee-40f9-afb4-dc9675443e92", "Last Mile Carrier Booking for Item Barcode: {0}", item.HVI_CurrentBarcode);
				}

				root.Nodes.Add(new ZBusinessObjectTreeNode(item, itemLabel));
			}

			return root;
		}
	}
}
