using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class CustomsEntryIssueAndExpiryDateControlTest : TestCaseWithFactory
	{
		public void TestIsIExtendedControl()
		{
			using (CustomsEntryIssueAndExpiryDateControl control = new CustomsEntryIssueAndExpiryDateControl())
			{
				AssertEquals(control, ((IExtendedControl)control).Host);
				AssertNotNull(control.GetExtension<LabelCaptionRenderer>());
			}
		}

		[RequiresSTA]
		public void TestControlsVisibility()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			using (TestForm form = new TestForm(shipment))
			{
				form.Show();

				AssertEquals(true, form.CustomsEntryIssueAndExpiryDateControl.CustomsEntryNameIssueDateEdit.Visible);
				AssertEquals(true, form.CustomsEntryIssueAndExpiryDateControl.CustomsEntryNameExpiryDateEdit.Visible);

				AssertEquals("Issue Date", form.CustomsEntryIssueAndExpiryDateControl.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals(null, form.CustomsEntryIssueAndExpiryDateControl.GetExtension<LabelCaptionRenderer>().LabelSeparator);
			}

			shipment.CusEntryNumbers.AddNew();

			using (TestForm form = new TestForm(shipment))
			{
				form.Show();

				AssertEquals(true, form.CustomsEntryIssueAndExpiryDateControl.CustomsEntryNameIssueDateEdit.Visible);
				AssertEquals(true, form.CustomsEntryIssueAndExpiryDateControl.CustomsEntryNameExpiryDateEdit.Visible);

				AssertEquals("Issue Date", form.CustomsEntryIssueAndExpiryDateControl.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals(null, form.CustomsEntryIssueAndExpiryDateControl.GetExtension<LabelCaptionRenderer>().LabelSeparator);
			}

			shipment.CusEntryNumbers.AddNew();

			using (TestForm form = new TestForm(shipment))
			{
				form.Show();

				AssertEquals(false, form.CustomsEntryIssueAndExpiryDateControl.CustomsEntryNameIssueDateEdit.Visible);
				AssertEquals(false, form.CustomsEntryIssueAndExpiryDateControl.CustomsEntryNameExpiryDateEdit.Visible);

				AssertEquals(string.Empty, form.CustomsEntryIssueAndExpiryDateControl.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals(string.Empty, form.CustomsEntryIssueAndExpiryDateControl.GetExtension<LabelCaptionRenderer>().LabelSeparator);
			}
		}

		#region Implementation

		class TestForm : ZForm
		{
			public TestForm(ForwardingShipment shipment)
				: base(shipment)
			{
				InitializeComponent();
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing && (CustomsEntryIssueAndExpiryDateControl != null))
				{
					CustomsEntryIssueAndExpiryDateControl.Dispose();
				}
				base.Dispose(disposing);
			}

			new void InitializeComponent()
			{
				CustomsEntryIssueAndExpiryDateControl = new CustomsEntryIssueAndExpiryDateControlForTest();
				Controls.Add(CustomsEntryIssueAndExpiryDateControl);
			}

			public CustomsEntryIssueAndExpiryDateControlForTest CustomsEntryIssueAndExpiryDateControl { get; private set; }
		}

		class CustomsEntryIssueAndExpiryDateControlForTest : CustomsEntryIssueAndExpiryDateControl
		{
			public ZDateEdit CustomsEntryNameIssueDateEdit { get { return (ZDateEdit)Controls.Find("customsEntryNameIssueDateEdit", true)[0]; } }
			public ZDateEdit CustomsEntryNameExpiryDateEdit { get { return (ZDateEdit)Controls.Find("customsEntryNameExpiryDateEdit", true)[0]; } }
		}

		#endregion
	}
}
