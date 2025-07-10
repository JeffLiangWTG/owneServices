using System.IO;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Integration;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.Testing
{
	public class UniversalXmlImportHandlerTest : TestCaseWithFactory
	{
		const string InboundXml = @"<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1""><TransactionInfo></TransactionInfo></UniversalTransaction>";

		public class DummyImportHandler : UniversalXmlImportHandler<TransactionInfo>
		{
			public DummyImportHandler(IXmlSessionTracker xmlSessionTracker) : this(xmlSessionTracker, new DefaultProcessingConfig())
			{
			}

			public DummyImportHandler(IXmlSessionTracker xmlSessionTracker, IHttpXmlProcessingConfig processingConfig) : base(xmlSessionTracker, processingConfig)
			{
			}

			protected override string RequestMessageSubType => EDIMessageSubTypeList.Codes.XmlUniversalTransaction;
		}

		public void TestProcessingDoesNotFailMessageAndBubblesOutInCaseOfCriticalExceptions()
		{
			var handler = new DummyImportHandler(new XmlSessionTracker(new SimpleLogger()));
			var request = handler.CreateRequestMessage();

			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new StreamWriter(stream) { AutoFlush = true }.Write(InboundXml);
				request.SetMessageTextSource(stream);
				request.Save();
			}

			var factory = (request as BusinessObject).Factory;
			using (factory.AddDisposableService())
			{
				UniversalXmlWorkflowProcessor.SetPreProcessActionHook(() => throw new DatabaseUpgradedException());

				var previousRetriedCount = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_Status, EDIMessageStatusList.Codes.Failed)).Length;

				AssertExceptionThrown<DatabaseUpgradedException>("Critical exception should be bubbled out of Process", () => handler.Process(request));
				AssertEquals("No attempt should be done to fail the message in case of critical exceptions", previousRetriedCount, Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_Status, EDIMessageStatusList.Codes.Failed)).Length);
			}
		}
	}
}
