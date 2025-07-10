using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class AllocationIDFindBoxTest : TestCaseWithFactory
	{
		public void TestGetNewPopupForm_ShowsCorrectAllocationModal()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var container = consol.Containers.AddNew();

			using (var parentForm = new ConsolForm(consol))
			using (var findBox = new AllocationIDFindBoxDummy())
			{
				findBox.SetDataBinding(container, nameof(ForwardingContainer.JC_RCA_AllocationLine));
				parentForm.Controls.Add(findBox);

				ZFormModaliser.ShowDialogsInTest = true;

				var allocationPopupForm = findBox.GetNewPopupForm_ForTest();
				allocationPopupForm.ShowModal(findBox, parentForm);
				Assert("The ContractAndAllocationPopup Modal should be displayed", ZFormModaliser.LastFormShownDialogForTest.Name.Equals(ContractAllocationFindBoxPopupTest.ContractAndAllocationsAttachForm));
			}
		}

		public void TestGetNewPopupForm_SetsCorrectFindBoxTextOnClose()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var allocationRoute = Factory.New<RatingContractAllocationLine>();
			allocationRoute.RCA_AllocationLineID = "ABC5678";

			var container = consol.Containers.AddNew();
			container.JC_RCA_AllocationLine = allocationRoute.PK;

			using (var parentForm = new ConsolForm(consol))
			using (var findBoxDummy = new AllocationIDFindBoxDummy())
			{
				findBoxDummy.SetDataBinding(container, nameof(ForwardingContainer.JC_RCA_AllocationLine));
				parentForm.Controls.Add(findBoxDummy);

				ZFormModaliser.ShowDialogsInTest = true;

				var allocationPopupForm = findBoxDummy.GetNewPopupForm_ForTest();
				allocationPopupForm.ShowModal(findBoxDummy, parentForm);
				AssertEquals("Findbox Text should have been set when modal form was closed.", allocationRoute.RCA_AllocationLineID, findBoxDummy.Text);
			}
		}

		[RequiresSTA]
		public void TestGetNewPopupForm_WhenParentIsQuotedBooking()
		{
			var quotedBooking = ObjectFactory
				.Get<IQuotedBookingBuilder>()
				.CreateNew(QuoteBookingType.QuickBooking, Factory);

			var allocationRoute = Factory.New<RatingContractAllocationLine>();
			allocationRoute.RCA_AllocationLineID = "ABC5678";

			var container = Factory.New<ForwardingContainer>();
			container.JC_JS_FCLBookingOnlyLink = quotedBooking.ForwardingShipment.PK;
			container.JC_RCA_AllocationLine = allocationRoute.PK;

			using (var form = new ZDummyForm(quotedBooking as BusinessObject))
			using (var findBox = new AllocationIDFindBoxDummy())
			{
				findBox.SetDataBinding(container, nameof(ForwardingContainer.JC_RCA_AllocationLine));
				form.Controls.Add(findBox);

				ZFormModaliser.ShowDialogsInTest = true;

				var popupForm = findBox.GetNewPopupForm_ForTest();
				popupForm.ShowModal(findBox, form);
				AssertEquals("Findbox Text should have been set when modal form was closed.", allocationRoute.RCA_AllocationLineID, findBox.Text);
			}
		}

		class AllocationIDFindBoxDummy : AllocationIDFindBox
		{
			public AllocationIDFindBoxDummy() : base()
			{
			}

			public IFindBoxPopup GetNewPopupForm_ForTest() => GetNewPopupForm();
		}
	}
}
