using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Address;
using Enterprise.Freight.Forwarding.Documents.GUI.CertificateOfOrigin;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Documents.Testing.GUI.CertificateOfOrigin.Supporters.Address
{
	class AddressFormControllerTest : TestCaseWithFactory
	{
		public void TestObjectFactoryRegistration()
		{
			var formController = ObjectFactory.Get<IAddressFormPopup>();

			AssertType<AddressFormController>(formController);
		}

		public void TestShowModal_Opens_AddressFormPopup()
		{
			using var form = new Form();

			var onPopupClosedInvoked = false;

			void OnPopupClosed(object sender, EventArgs args)
			{
				onPopupClosedInvoked = true;
			}

			using var formController = new AddressFormController();
			formController.Closed += OnPopupClosed;

			var findBox = new DummyFindBox { ListProvider = new AddressBusinessObjectConfiguration(certificateName: "NZCFTA", enableUnknown: true) { new AddressBusinessObject() } };

			formController.ShowModal(findBox, form);

			AssertEquals("AddressFormPopup shown", typeof(AddressFormPopup), ZFormModaliser.LastFormShownDialogForTest.GetType());
			Assert("Popup Closed handler has been invoked", onPopupClosedInvoked);
		}
	}
}
