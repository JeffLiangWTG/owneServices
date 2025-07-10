using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using IStowPlanMessage = Enterprise.Integration.Customs.US.USAMS.IStowPlanMessage;

namespace Enterprise.Freight.Business.Testing
{
	sealed class StowPlanLicenceLoggingExtensionsTest : TestCaseWithFactory
	{
		public void TestLogSTWLicense()
		{
			var voyage = Factory.New<JobVoyage>();
			var destination = voyage.Destinations.AddNew();
			var msg1 = Factory.New<DummyStowPlanMessage>();
			destination.Messages.Add(msg1);
			msg1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			msg1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-3);
			msg1.AcceptedContainersWhichPreviouslyNotAccepted = 6;
			var msg2 = Factory.New<DummyStowPlanMessage>();
			destination.Messages.Add(msg2);
			msg2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			msg2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-2);
			msg2.AcceptedContainersWhichPreviouslyNotAccepted = 4;
			var msg3 = Factory.New<DummyStowPlanMessage>();
			destination.Messages.Add(msg3);
			msg3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			msg3.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-1);
			msg3.AcceptedContainersWhichPreviouslyNotAccepted = 5;
			var msg4 = Factory.New<DummyStowPlanMessage>();
			destination.Messages.Add(msg4);
			msg4.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			msg4.EM_SystemCreateTimeUtc = ZDateTime.Now;
			msg4.AcceptedContainersWhichPreviouslyNotAccepted = 7;

			destination.LogSTWLicense();

			AssertEquals(7, Factory.Load<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_FormCaption, Env.Licence.StowPlanReporting.Name)).Length);
		}

		public class DummyStowPlanMessage : EDIMessage, IStowPlanMessage
		{
			public DummyStowPlanMessage(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public ZInt CountAcceptedContainersWhichPreviouslyNotAccepted()
			{
				return AcceptedContainersWhichPreviouslyNotAccepted;
			}

			public ZInt AcceptedContainersWhichPreviouslyNotAccepted;
		}
	}
}
