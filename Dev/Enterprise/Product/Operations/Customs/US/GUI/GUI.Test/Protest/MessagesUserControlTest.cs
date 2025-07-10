using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.GUI.Protest.Testing
{
	using Enterprise.Customs.US.Business.Protest;
	using Moq.Protected;

	sealed class MessagesUserControlTest : TestCaseWithFactory
	{
		public void TestStatusesErrorsTabHasUserControl()
		{
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock
				.Protected()
				.Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var message = mock.Object;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Protest.Messages.Add(message);
			using (var form = new ProtestForm(Protest))
			{
				form.Show();
				AssertNotNull(form.messagesUserControl.messagesStatusErrorsUserControl);
			}
		}

		Protest protest;
		Protest Protest => protest ?? (protest = new Protest(Factory.New<JobDeclaration>()));
	}
}
