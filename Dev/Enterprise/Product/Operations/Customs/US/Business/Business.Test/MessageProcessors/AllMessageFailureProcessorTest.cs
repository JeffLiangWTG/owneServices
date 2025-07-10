using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class AllMessageFailureProcessorTest : MessageFailureProcessorTest<AllMessageFailureProcessor, APLA, APLB, APLY>
	{
		//CS00078664
		public void TestExtractReferenceFilesFailure()
		{
			ProcessAndAssert(ApplicationIdentifierCodeList.Codes.ExtractReferenceFiles, ApplicationIdentifierCodeList.Descriptions.ExtractReferenceFiles);
		}

		//CS00078664
		protected override void EndToEndCore()
		{
			ProcessAndAssert(ApplicationIdentifierCodeList.Codes.ExtractReferenceFiles, ApplicationIdentifierCodeList.Descriptions.ExtractReferenceFiles);
		}

		public void TestProcessConsigneeNameAddressQuery()
		{
			ProcessAndAssertForOrg(ApplicationIdentifierCodeList.Codes.ConsigneeNameAddressQuery, ApplicationIdentifierCodeList.Descriptions.ConsigneeNameAddressQuery);
		}

		public void TestManufacturerNameandAddressQueryFailure()
		{
			ProcessAndAssertForOrg(ApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressQuery, ApplicationIdentifierCodeList.Descriptions.ManufacturerNameandAddressQuery);
		}

		[ExpectNoExceptions]
		void ProcessAndAssertForOrg(string applicationIdentifier, string subject)
		{
			OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(org);
			ProcessAndAssert(applicationIdentifier, subject, delegate(MQEDIMessage sendingMessage)
			{
				sendingMessage.EM_LinkedObject = org;
			});
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 1; }
		}
	}
}
