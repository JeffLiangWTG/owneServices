using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(EIDOMessage))]
	class EIDOMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestNew()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();

			EIDOMessage message1 = EIDOMessage.New(container, EIDOMessageFunction.Original, "OriginalMessageText");
			AssertEquals("Sub Type", EIDOMessageTypes.Codes.Original, message1.EM_MessageSubType);
			AssertEquals("Message Text", "OriginalMessageText", message1.EM_MessageText);

			EIDOMessage message2 = EIDOMessage.New(container, EIDOMessageFunction.Cancelation, "CancelationMessageText");
			AssertEquals("Sub Type", EIDOMessageTypes.Codes.Cancellation, message2.EM_MessageSubType);
			AssertEquals("Message Text", "CancelationMessageText", message2.EM_MessageText);
		}

		[ExpectNoExceptions]
		public void TestSaving_MessageIsSavedFirstTime_CreateLicenceConsumptionLog()
		{
			var mocksRepository = new MockRepository(MockBehavior.Strict);
			var licenceConsumptionLogCreatorMock = mocksRepository.Create<ILicenceConsumptionLogCreator>();

			licenceConsumptionLogCreatorMock.Setup(l => l.CreateLog(Env.Licence.ShippingManagerEIDOMessagingPerTransaction, true, default));
			using (ObjectFactory.Substitute(licenceConsumptionLogCreatorMock.Object))
			{
				var shipment = Factory.New<AgencyShipment>();
				var container = shipment.RealContainers.AddNew();

				var messageToTest = EIDOMessage.New(container, EIDOMessageFunction.Original, "McLaren <<MSGNO PLACEHOLDER>>");

				Factory.Save();
			}
			licenceConsumptionLogCreatorMock.Verify(l => l.CreateLog(Env.Licence.ShippingManagerEIDOMessagingPerTransaction, true, default), Times.Exactly(1));
		}

		[ExpectNoExceptions]
		public void TestSaving_MessageIsAlreadySaved_DoNotCreateLicenceConsumptionLog()
		{
			var mocksRepository = new MockRepository(MockBehavior.Strict);
			var licenceConsumptionLogCreatorMock = mocksRepository.Create<ILicenceConsumptionLogCreator>(MockBehavior.Strict);

			licenceConsumptionLogCreatorMock.Setup(l => l.CreateLog(Env.Licence.ShippingManagerEIDOMessagingPerTransaction, true, default));

			using (ObjectFactory.Substitute(licenceConsumptionLogCreatorMock.Object))
			{
				var shipment = Factory.New<AgencyShipment>();
				var container = shipment.RealContainers.AddNew();

				var messageToTest = EIDOMessage.New(container, EIDOMessageFunction.Original, "McLaren <<MSGNO PLACEHOLDER>>");

				Factory.Save();

				messageToTest.EM_MessageText = "Manchester United <<MSGNO PLACEHOLDER>>";

				Factory.Save();
			}
			licenceConsumptionLogCreatorMock.Verify(l => l.CreateLog(Env.Licence.ShippingManagerEIDOMessagingPerTransaction, true, default), Times.Exactly(1));
		}

		[ExpectNoExceptions]
		public void TestSaving_OnlyOriginalsAreSaved()
		{
			var mocksRepository = new MockRepository(MockBehavior.Strict);
			var licenceConsumptionLogCreatorMock = mocksRepository.Create<ILicenceConsumptionLogCreator>(MockBehavior.Strict);

			licenceConsumptionLogCreatorMock.Setup(l => l.CreateLog(Env.Licence.ShippingManagerEIDOMessagingPerTransaction, true, default));

			using (ObjectFactory.Substitute(licenceConsumptionLogCreatorMock.Object))
			{
				var shipment = Factory.New<AgencyShipment>();
				var container = shipment.RealContainers.AddNew();

				var message1 = EIDOMessage.New(container, EIDOMessageFunction.Original, "Message 1");
				Factory.Save();

				var message2 = EIDOMessage.New(container, EIDOMessageFunction.Cancelation, "Message 2");
				Factory.Save();
			}
			licenceConsumptionLogCreatorMock.Verify(l => l.CreateLog(Env.Licence.ShippingManagerEIDOMessagingPerTransaction, true, default), Times.Exactly(1));
		}
	}
}
