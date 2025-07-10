using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.eTail.Integration;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.eTail.GUI.Testing
{
	public abstract class BaseLastMileCarrierBookingMenuItemTest : TestCaseWithFactory
	{
		protected void PerformLMCBookingAction(ILastMileCarrierBookingService testBookingProvider, BaseLastMileCarrierBookingMenuItem menuItemToTest)
		{
			using (ObjectFactory.Substitute("HVLVItemRTUSBookingProvider", testBookingProvider))
			{
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					if (dialog is HVLVSelectConsignmentItemForm itemSelectionForm)
					{
						var consignmentTreeView = itemSelectionForm.Controls.Find("ConsignmentItemTreeView", true)[0] as ZTreeView;
						consignmentTreeView.Nodes[0].Nodes[0].Checked = true;

						itemSelectionForm.DialogResult = DialogResult.OK;
					}
				});

				menuItemToTest.PerformClick();
			}
		}

		protected ILastMileCarrierBookingResponseCollection CreateMockLMCBookingResponseCollection(IEnumerable<ILastMileCarrierBookingResponse> responses)
		{
			var responseCollection = new Mock<ILastMileCarrierBookingResponseCollection>();
			responseCollection.Setup(mock => mock.GetEnumerator()).Returns(responses.GetEnumerator());
			return responseCollection.Object;
		}
	}
}
