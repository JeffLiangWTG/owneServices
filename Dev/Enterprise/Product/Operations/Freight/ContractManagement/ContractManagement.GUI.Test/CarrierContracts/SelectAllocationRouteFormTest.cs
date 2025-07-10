using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ContractManagement.GUI.Testing
{
	[TestedType(typeof(SelectAllocationRouteForm))]
	public sealed class SelectAllocationRouteFormForConsolTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			consol = Factory.New<IForwardingConsol>();
			allocationRoutes = new RatingContractAllocationLine[] {
				Factory.NewWithValidTestData<RatingContractAllocationLine>(),
				Factory.NewWithValidTestData<RatingContractAllocationLine>()
			};

			return new SelectAllocationRouteForm(consol as IAllocationRouteAssignable, new ViewMultiAllocationSelectionManager(Factory, allocationRoutes));
		}

		public void TestAllocationRouteLinksToConsol()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var allocationRouteGrid = form.Controls.Find("selectionGrid", searchAllChildren: true).FirstOrDefault() as ZGrid;
				allocationRouteGrid.Select(0);

				var okButton = form.Controls.Find("okButton", searchAllChildren: true).FirstOrDefault() as ZButton;
				okButton.PerformClick();

				AssertEquals(consol.JK_CarrierContractNumber, allocationRoutes[0].ParentContractNumber);
				AssertEquals(consol.JK_RCA_AllocationLine, allocationRoutes[0].PK);
			}
		}

		IForwardingConsol consol;
		RatingContractAllocationLine[] allocationRoutes;
	}

	[TestedType(typeof(SelectAllocationRouteForm))]
	public sealed class SelectAllocationRouteFormForQuotedBookingTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			quotedBooking = ObjectFactory
				.Get<IQuotedBookingBuilder>()
				.CreateNew(QuoteBookingType.QuickBooking, Factory);
			allocationRoutes = new RatingContractAllocationLine[] {
				Factory.NewWithValidTestData<RatingContractAllocationLine>(),
				Factory.NewWithValidTestData<RatingContractAllocationLine>()
			};

			return new SelectAllocationRouteForm(quotedBooking as IAllocationRouteAssignable, new ViewMultiAllocationSelectionManager(Factory, allocationRoutes));
		}

		public void TestAllocationRouteLinksToQuotedBooking()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var allocationRouteGrid = form.Controls.Find("selectionGrid", searchAllChildren: true).FirstOrDefault() as ZGrid;
				allocationRouteGrid.Select(1);

				var okButton = form.Controls.Find("okButton", searchAllChildren: true).FirstOrDefault() as ZButton;
				okButton.PerformClick();

				AssertEquals(quotedBooking.GetPropertyValue("CarrierContractNumber"), allocationRoutes[1].ParentContractNumber);
				AssertEquals(quotedBooking.GetPropertyValue("AllocationLinePK"), allocationRoutes[1].PK);
			}
		}

		IQuotedBooking quotedBooking;
		RatingContractAllocationLine[] allocationRoutes;
	}
}
