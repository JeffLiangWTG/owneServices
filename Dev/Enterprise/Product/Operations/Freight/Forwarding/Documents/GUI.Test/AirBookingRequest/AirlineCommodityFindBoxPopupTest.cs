using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Documents.GUI.Testing
{
	sealed class AirlineCommodityFindBoxPopupTest : TestCaseWithFactory
	{
		public void TestObjectFactoryRegistration()
		{
			var findBoxPopup = ObjectFactory.Get<IAirlineCommodityFindBoxPopup>();

			AssertType<AirlineCommodityFindBoxPopup>(findBoxPopup);
		}

		public void TestShowModal()
		{
			using (var form = new Form())
			{
				var onPopupClosedInvoked = false;

				void OnPopupClosed(object sender, EventArgs args)
				{
					onPopupClosedInvoked = true;
				}

				var findBoxPopup = new AirlineCommodityFindBoxPopup();
				findBoxPopup.Closed += OnPopupClosed;

				findBoxPopup.ShowModal(new DummyFindBox(), form);

				AssertEquals("AirlineCommodityPopupForm shown", typeof(AirlineCommodityPopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				Assert("Popup Closed handler has been invoked", onPopupClosedInvoked);
			}
		}
	}
}
