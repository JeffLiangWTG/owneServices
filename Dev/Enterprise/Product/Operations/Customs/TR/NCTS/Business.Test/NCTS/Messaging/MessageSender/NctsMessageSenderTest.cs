using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.DataTransfer.Phase4.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.NCTS.Business.Messaging;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	public class NctsMessageSenderTest : TestCaseWithFactory
	{
		public void TestCreateNCTSMessage()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			NctsHeaderDataObjectWriterTest.SetupNctsHeaderForDeparture(Factory, nctsHeader);
			var sender = new NctsMessageSender();

			using (TRCustomsDataRegistry.Instance.EnableTRNCTS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				sender.CreateMessage(nctsHeader, new SendsMessagesToCustomsGUI(), new NctsMessageFunctionSet.DeclarationDataMessage());
				AssertContains("It should show error message when 'Enable Turkey NCTS' is set to false", "NCTS has not been implemented for the country of departure or destination selected.\r\nPlease select a departure or destination office in another country.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (TRCustomsDataRegistry.Instance.EnableTRNCTS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				sender.CreateMessage(nctsHeader, new SendsMessagesToCustomsGUI(), new NctsMessageFunctionSet.DeclarationDataMessage());
				AssertEquals(1, nctsHeader.Messages.Count);

				CombineAssertions(() =>
				{
					var message = nctsHeader.Messages[0];
					AssertEquals(ApplicationCodes.TRCustoms, message.EM_ApplicationCode);
					AssertEquals(TRMessageTypes.Codes.TRN, message.EM_MessageType);
					AssertEquals(ZString.Empty, message.EM_MessageSubType);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
		}

		NctsHeader nctsHeader;
	}
}
