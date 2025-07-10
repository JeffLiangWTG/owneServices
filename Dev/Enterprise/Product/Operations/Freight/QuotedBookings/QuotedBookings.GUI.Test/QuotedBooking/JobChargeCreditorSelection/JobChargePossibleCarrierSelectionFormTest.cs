using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	[TestedType(typeof(JobChargePossibleCarrierSelectionForm))]
	public class JobChargePossibleCarrierSelectionFormTest : ZFormBasherTest
	{
		public void TestPotentialCarriersGrid_Structure()
		{
			var (charge, testForm) = CreateChargeAndForm();
			testForm.Show();

			var potentialCarrierGrid = testForm
				.Controls
				.Find("PotentialCarriersGrid", true)
				.Cast<ZGrid>()
				.Single();

			var seenColumns = potentialCarrierGrid
				.Columns
				.Select(x => new { x.ColumnName, x.ColumnStyle.ReadOnly })
				.ToArray();
			var expectColumns = new[] {
				new { ColumnName = "TTC_OH_Carrier", ReadOnly = true },
				new { ColumnName = "CarrierName", ReadOnly = true },
				new { ColumnName = "TTC_OH_Creditor", ReadOnly = true },
			};

			AssertContainsExactElementsInAnyOrder(expectColumns, seenColumns);
			testForm.Dispose();
		}

		public void TestCancelButton_SelectedCharge_NotModified()
		{
			var (charge, testForm) = CreateChargeAndForm();
			testForm.Show();

			var cancelButton = (ZButton)testForm.Controls.Find("BtnCancel", true).Single();
			cancelButton.PerformClick();

			charge.VerifySet(x => x.JR_Calc_CostRatingBehavior = It.IsAny<ZString>(), Times.Never);
			charge.VerifySet(x => x.JR_OH_CostAccount = It.IsAny<ZGuid>(), Times.Never);
			Assert(true);

			testForm.Dispose();
		}

		public void TestOKButton_SelectedCharge_Modified()
		{
			var (charge, testForm) = CreateChargeAndForm();
			testForm.Show();

			var okButton = (ZButton)testForm.Controls.Find("BtnOK", true).Single();
			okButton.PerformClick();

			charge.VerifySet(x => x.JR_Calc_CostRatingBehavior = It.IsAny<ZString>(), Times.Once);
			charge.VerifySet(x => x.JR_OH_CostAccount = It.IsAny<ZGuid>(), Times.Once);
			Assert(true);

			testForm.Dispose();
		}

		public void TestTabIndexOrder()
		{
			var (_, testForm) = CreateChargeAndForm();

			var potentialCarrierGrid = testForm
				.Controls
				.Find("PotentialCarriersGrid", true)
				.Cast<ZGrid>()
				.Single();
			var okButton = (ZButton)testForm.Controls.Find("BtnOK", true).Single();
			var cancelButton = (ZButton)testForm.Controls.Find("BtnCancel", true).Single();

			AssertEquals(1, potentialCarrierGrid.TabIndex);
			AssertEquals(2, okButton.TabIndex);
			AssertEquals(3, cancelButton.TabIndex);

			testForm.Dispose();
		}

		(Mock<ICharge> Charge, JobChargePossibleCarrierSelectionForm Form) CreateChargeAndForm()
		{
			var charge = new Mock<ICharge>();
			var shipment = Factory.NewWithValidTestData<RateOneOffShipment>();

			var possibleCarriers = new RateOneOffCarrierCollection(shipment);
			var possibleCarrier = possibleCarriers.AddNew();
			possibleCarrier.TTC_OH_Creditor = Factory.NewWithValidTestData<OrgHeader>().PK;
			possibleCarrier.TTC_OH_Creditor = Factory.NewWithValidTestData<OrgHeader>().PK;

			var testForm = new JobChargePossibleCarrierSelectionForm(new JobChargePossibleCarrierSelection(charge.Object, possibleCarriers));

			return (charge, testForm);
		}

		protected override Form GetFormToBashCore()
		{
			var (_, testForm) = CreateChargeAndForm();
			return testForm;
		}
	}
}
