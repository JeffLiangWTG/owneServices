using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Moq.Protected;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class OrganisationPlugInUserControlTest : TestCaseWithFactory
	{
		public void TestStatusesErrorsTabHasUserControl()
		{
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock
				.Protected()
				.Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var message = mock.Object;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var orgWrapper = OrgHeaderWrapper.New(organisation);
			orgWrapper.Messages.Add(message);
			using (var plugIn = new OrganisationPlugIn(organisation))
			{
				AssertEquals(typeof(OrganisationPlugInUserControl), plugIn.UserControl.GetType());
				var userControl = plugIn.UserControl as OrganisationPlugInUserControl;
				AssertNotNull(userControl.messagesStatusErrorsUserControl);
			}
		}
	}
}
