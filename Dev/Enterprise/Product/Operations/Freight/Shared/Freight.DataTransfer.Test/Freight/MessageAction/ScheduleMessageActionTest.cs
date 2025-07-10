using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.ServiceManager.Tasks.StandardXMLProcessor;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Testing
{
	sealed class ScheduleMessageActionTest : TestCaseWithFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEndToEnd()
		{
			Assert("Ensure database is clean", Factory.Load<JobVoyage>(new ZQuery(JobVoyageSchema.JV_VoyageFlight, "917")).IsNullOrEmpty());

			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.XMS;
			message.EM_MessageType = EDIMessage.ApplicationCodes.XMS;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.Schedules;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageNum = "00000000000000000101";

			var path = string.Format(@"{0}Enterprise\Product\Operations\Freight\Shared\Freight.DataTransfer.Test\Freight\MessageAction\Testing\Schedules.xml", BaseSourcePath);
			message.EM_MessageText = System.IO.File.ReadAllText(path);
			Factory.Save();

			var processor = new StandardXMLMessageServiceTask();
			processor.ServiceLogger = new DummyLogger();
			processor.RunTask(CancellationToken.None);

			Assert("Schedule(JobVoyage) created from Native XML", Factory.Load<JobVoyage>(new ZQuery(JobVoyageSchema.JV_VoyageFlight, "917")).Any());
		}
	}
}
