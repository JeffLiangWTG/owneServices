using System;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.SG.Access.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;
using AsycudaUniversalEventMessageFailureProcessor = Enterprise.Customs.SG.Access.Business.UniversalDataTransfer.AsycudaUniversalEventMessageFailureProcessor;
using AsycudaUniversalEventMessagePinProcessor = Enterprise.Customs.SG.Access.Business.UniversalDataTransfer.AsycudaUniversalEventMessagePinProcessor;
using AsycudaUniversalEventMessageSuccessProcessor = Enterprise.Customs.SG.Access.Business.UniversalDataTransfer.AsycudaUniversalEventMessageSuccessProcessor;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	[TestedType(typeof(ApplicationBusinessProvider))]
	sealed class ApplicationBusinessProviderTest : ASYCUDA.Business.Testing.ApplicationBusinessProviderAbstractTest<ApplicationBusinessProvider, AsycudaManifestHeader>
	{
		public override void TestGetNewAsycudaUniversalEventMessageProcessor()
		{
			var header = CreateNewManifest();
			var eventXML = new Event();
			var dataContext = DataContextFactory.New();
			var actionPurpose = new CodeDescriptionPair()
			{ Code = Constants.ActionPurpose.ERR };
			var workflowInfo = new WorkflowInfo { ActionPurpose = actionPurpose };
			dataContext.SetWorkflowInfo(workflowInfo);
			eventXML.DataContext = dataContext;
			var ediMessage = Factory.New<AsycudaEDIMessage>();
			AssertEquals("ERR", typeof(AsycudaUniversalEventMessageFailureProcessor), header.ApplicationBusinessProvider.GetNewAsycudaUniversalEventMessageProcessor(new XmlSessionTracker(new ServiceTaskLogForTesting()), eventXML, ediMessage, header).GetType());
			actionPurpose.Code = Constants.ActionPurpose.AEP;
			AssertEquals("AEP", typeof(AsycudaUniversalEventMessageSuccessProcessor), header.ApplicationBusinessProvider.GetNewAsycudaUniversalEventMessageProcessor(new XmlSessionTracker(new ServiceTaskLogForTesting()), eventXML, ediMessage, header).GetType());
			actionPurpose.Code = Constants.ActionPurpose.PIN;
			AssertEquals("PIN", typeof(AsycudaUniversalEventMessagePinProcessor), header.ApplicationBusinessProvider.GetNewAsycudaUniversalEventMessageProcessor(new XmlSessionTracker(new ServiceTaskLogForTesting()), eventXML, ediMessage, header).GetType());
			actionPurpose.Code = null;
			AssertNull("Null code", header.ApplicationBusinessProvider.GetNewAsycudaUniversalEventMessageProcessor(new XmlSessionTracker(new ServiceTaskLogForTesting()), eventXML, ediMessage, header));
			actionPurpose.Code = "#@#";
			AssertNull("Invalid code", header.ApplicationBusinessProvider.GetNewAsycudaUniversalEventMessageProcessor(new XmlSessionTracker(new ServiceTaskLogForTesting()), eventXML, ediMessage, header));
			eventXML.DataContext = null;
			AssertNull("Null DataContext", header.ApplicationBusinessProvider.GetNewAsycudaUniversalEventMessageProcessor(new XmlSessionTracker(new ServiceTaskLogForTesting()), eventXML, ediMessage, header));
		}

		public override void TestManifestTypes()
		{
			ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			base.TestManifestTypes();
		}

		protected override IEnumerable<IManifestType> ExpectedManifestTypes => new SGManifestTypes().All;

		protected override Type ExpectedMessagingProviderType => typeof(MessagingProvider);

		protected override Type ExpectedFeatureProviderType => typeof(FeatureProvider);

		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterType => typeof(SGAsycudaManifestHeaderDataObjectWriter);

		protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => typeof(SGAsycudaForCustomsDeclarationDataObjectWriter);

		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterHelperType => typeof(SGAsycudaManifestHeaderDataObjectWriterHelper);

		protected override Type ExpectedGetAsycudaManifestDataObjectReaderHelperType => typeof(SGAsycudaManifestDataObjectReaderHelper);
	}
}
