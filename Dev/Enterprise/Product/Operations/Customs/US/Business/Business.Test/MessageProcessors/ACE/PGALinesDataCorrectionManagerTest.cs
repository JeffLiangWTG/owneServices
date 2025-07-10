using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class PGALinesDataCorrectionManagerTest : TestCaseWithFactory
	{
		public void TestUpdatePGALinesIfMessageUpdatedFailed()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_PGAReplaceUpdateNeeded = YesNoDefaultList.Codes.Yes;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			invoiceLine.SetTrackingID();
			AssertEquals("8471704065_1__0_0", invoiceLine.GetTrackingID());

			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCACertification = TSCAIndicatorList.Codes.TSCAPositive;
			invoiceLine.US_TSCATrackingStatus = PGATrackingStatusList.Codes.Adding;

			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			var pstLine = invoiceLine.PSTLines.AddNew();
			pstLine.US_ProductType = PSTProductTypeList.Codes.PS1;
			pstLine.US_TrackingStatus = PGATrackingStatusList.Codes.Updating;

			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
			var nhtsaLine = invoiceLine.NHTSALines.AddNew();
			nhtsaLine.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			nhtsaLine.US_TrackingStatus = PGATrackingStatusList.Codes.Deleting;

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseAdd;

			new PGALinesDataCorrectionManager(message, declaration).UpdatePGALines(true);
			AssertEquals(ZString.Empty, invoiceLine.US_TSCATrackingStatus);
			AssertEquals(PGATrackingStatusList.Codes.ToBeUpdated, pstLine.US_TrackingStatus);
			AssertEquals(PGATrackingStatusList.Codes.ToBeDeleted, nhtsaLine.US_TrackingStatus);
			AssertEquals(YesNoDefaultList.Codes.Yes, declaration.US_PGAReplaceUpdateNeeded);
			AssertEquals("8471704065_1__0_0", invoiceLine.GetTrackingID());

			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(false, nhtsaLine.HasTrackingIDChanged());
		}

		public void TestUpdatePGALinesIfMessageUpdatedSuccessfully()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_PGAReplaceUpdateNeeded = YesNoDefaultList.Codes.Yes;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine0 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine0.JI_Tariff = "8471704065";
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8471704066";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			invoiceLine0.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine0.US_TSCACertification = TSCAIndicatorList.Codes.TSCAPositive;
			invoiceLine0.US_TSCATrackingStatus = PGATrackingStatusList.Codes.Adding;

			invoiceLine0.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			var pstLine = invoiceLine0.PSTLines.AddNew();
			pstLine.US_ProductType = PSTProductTypeList.Codes.PS1;
			pstLine.US_TrackingStatus = PGATrackingStatusList.Codes.Updating;

			invoiceLine0.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
			var nhtsaLine = invoiceLine0.NHTSALines.AddNew();
			nhtsaLine.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			nhtsaLine.US_TrackingStatus = PGATrackingStatusList.Codes.Deleting;

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseAdd;

			new PGALinesDataCorrectionManager(message, declaration).UpdatePGALines(false);
			AssertEquals(PGATrackingStatusList.Codes.Added, invoiceLine0.US_TSCATrackingStatus);
			AssertEquals(PGATrackingStatusList.Codes.Added, pstLine.US_TrackingStatus);
			AssertEquals(PGATrackingStatusList.Codes.Deleted, nhtsaLine.US_TrackingStatus);
			AssertEquals(ZString.Empty, declaration.US_PGAReplaceUpdateNeeded);
			AssertEquals("8471704065_1__0_0", invoiceLine0.GetTrackingID());
			AssertEquals("8471704066_2__0_0", invoiceLine1.GetTrackingID());
		}

		public void TestUpdatePGALinesIfDeleteMessageAccepted()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_PGAReplaceUpdateNeeded = YesNoDefaultList.Codes.Yes;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			invoiceLine.SetTrackingID();
			AssertEquals("8471704065_1__0_0", invoiceLine.GetTrackingID());

			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCACertification = TSCAIndicatorList.Codes.TSCAPositive;
			invoiceLine.US_TSCATrackingStatus = PGATrackingStatusList.Codes.Adding;

			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			var pstLine = invoiceLine.PSTLines.AddNew();
			pstLine.US_ProductType = PSTProductTypeList.Codes.PS1;
			pstLine.US_TrackingStatus = PGATrackingStatusList.Codes.Updating;

			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
			var nhtsaLine = invoiceLine.NHTSALines.AddNew();
			nhtsaLine.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			nhtsaLine.US_TrackingStatus = PGATrackingStatusList.Codes.Deleting;

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseDelete;

			new PGALinesDataCorrectionManager(message, declaration).UpdatePGALines(false);
			AssertEquals(ZString.Empty, invoiceLine.US_TSCATrackingStatus);
			AssertEquals(ZString.Empty, pstLine.US_TrackingStatus);
			AssertEquals(ZString.Empty, nhtsaLine.US_TrackingStatus);
			AssertEquals(ZString.Empty, declaration.US_PGAReplaceUpdateNeeded);
			AssertEquals(ZString.Empty, invoiceLine.GetTrackingID());
		}

		public void TestUpdatePGALinesWithPGADeletingMessageAccepted()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_PGAReplaceUpdateNeeded = YesNoDefaultList.Codes.Yes;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_DDTCLicenseNo = "132133";
			invoiceLine.US_DDTCTrackingStatus = PGATrackingStatusList.Codes.Deleting;

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseAdd;

			new PGALinesDataCorrectionManager(message, declaration).UpdatePGALines(false, true);
			AssertEquals(PGATrackingStatusList.Codes.Deleted, invoiceLine.US_DDTCTrackingStatus);
			AssertEquals(ZString.Empty, declaration.US_PGAReplaceUpdateNeeded);
			AssertEquals(ZString.Empty, invoiceLine.GetTrackingID());
		}
	}
}
