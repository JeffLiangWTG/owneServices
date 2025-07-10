using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(ContractAllocationFindBoxPopup))]
	public class ContractAllocationFindBoxPopupTest : TestCaseWithFactory
	{
		public const string ContractAndAllocationsAttachForm = "ContractAndAllocationsAttachForm";

		public void TestShowModalDisplaysAttachForm()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			using (var parentForm = new ConsolForm(consol))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				var findBox = new AllocationIDFindBox();
				parentForm.Controls.Add(findBox);

				var formConfiguration = new Mock<IContractSimulationFormConfiguration>();

				var popup = new ContractAllocationFindBoxPopup(formConfiguration.Object);
				popup.ShowModal(findBox, parentForm);

				Assert(ZFormModaliser.LastFormShownDialogForTest.Name.Equals(ContractAndAllocationsAttachForm));
			}
		}

		public void TestInterfaceClosedEventIsRaisedByModalFormClosed()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var formConfiguration = new Mock<IContractSimulationFormConfiguration>();
			var form = new Mock<ZForm>() { CallBase = true };

			using (var parentForm = new ConsolForm(consol))
			using (var findBox = new AllocationIDFindBox())
			using (var popup = new ContractAllocationFindBoxPopup(formConfiguration.Object))
			{
				var mockFormFactory = new Mock<IContractAndAllocationsAttachFormFactory>();
				mockFormFactory.Setup(x => x.CreateForm(It.IsAny<IContractSimulationFormConfiguration>())).Returns(form.Object);

				ObjectFactory.Substitute("IContractAndAllocationsAttachFormFactory", mockFormFactory.Object);
				parentForm.Controls.Add(findBox);

				var eventWasRaised = false;
				(popup as IFindBoxPopup).Closed += (sender, args) =>
				{
					eventWasRaised = true;
				};
				popup.ShowModal(findBox, parentForm);
				form.Object.Close();
				Assert(eventWasRaised.Equals(true));
			}
		}
	}
}
