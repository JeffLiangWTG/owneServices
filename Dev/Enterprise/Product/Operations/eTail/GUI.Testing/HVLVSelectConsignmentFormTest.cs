using System.Linq;
using System.Windows.Forms;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.GUI.Testing
{
	[TestedType(typeof(HVLVSelectConsignmentForm))]
	public class HVLVSelectConsignmentFormTest : ZFormBasherTest
	{
		public void TestMergeButtonEnable()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = BuildImportConsignment(bookingHeader);
			BuildImportConsignment(bookingHeader);

			using (var form = new HVLVSelectConsignmentForm(consignment.ConsignmentsBelongToSameConsigneeExcludingParent))
			{
				form.Show();

				var buttonMerge = form.Controls.Find("btnMerge", true)[0] as ZButton;

				AssertEquals("pre condition", false, buttonMerge.Enabled);

				var gridConsignments = form.Controls.Find("gridConsignments", true)[0] as ZGrid;
				gridConsignments.PerformMouseDownForTest(0, 1);
				Application.DoEvents();

				AssertEquals("pre condition: seleted one consignment", true, form.SelectedConsignments.Any());
				AssertEquals("Merge button enabled", true, buttonMerge.Enabled);
			}
		}

		public void TestClickMergeButton()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = BuildImportConsignment(bookingHeader);
			BuildImportConsignment(bookingHeader);

			using (var form = new HVLVSelectConsignmentForm(consignment.ConsignmentsBelongToSameConsigneeExcludingParent))
			{
				form.Show();

				var gridConsignments = form.Controls.Find("gridConsignments", true)[0] as ZGrid;
				gridConsignments.PerformMouseDownForTest(0, 1);
				Application.DoEvents();

				var buttonMerge = form.Controls.Find("btnMerge", true)[0] as ZButton;
				buttonMerge.PerformClick();

				AssertEquals("Dialog result should be OK", DialogResult.OK, form.DialogResult);
			}
		}

		public void TestClickCancelButton()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();

			using (var form = new HVLVSelectConsignmentForm(consignment.ConsignmentsBelongToSameConsigneeExcludingParent))
			{
				form.Show();

				var buttonCancel = form.Controls.Find("btnIgnore", true)[0] as ZButton;
				buttonCancel.PerformClick();

				AssertEquals("Dialog result should be Cancel", DialogResult.Cancel, form.DialogResult);
			}
		}

		#region Implementations

		protected override Form GetFormToBashCore()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			return new HVLVSelectConsignmentForm(new HVLVCommonConsigneeConsignmentCollection(consignment));
		}

		HVLVConsignment BuildImportConsignment(HVLVBookingHeader header)
		{
			var consignment = header.Consignments.AddNew();
			consignment.HVC_RN_NKShipperCountryCode = "NZ";
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			return consignment;
		}

		#endregion
	}
}
