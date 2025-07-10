using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.EventProcessors;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using ParameterCodes = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class DocumentEventProcessorTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			var lrn = "123456";
			var caseNo = "654321";
			var processor = new DocumentEventProcessor(new DummyLogger());

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = lrn;
			var instr = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instr.PK;
			var caseNumber = instr.CaseNumbers.AddNew();
			caseNumber.CY_Data = caseNo;

			processor.Process(declaration, null);
			AssertEquals(ZString.Empty, caseNumber.Document_Status);

			var helper = new SupportingDocumentStatusHelperTest.Helper(Factory, declaration, entryHeader, caseNo);
			var fileName = "test.pdf";
			helper.CreateNewEvent(Events.DocumentSent, helper.CreateNewDSNEDIMessage(fileName));
			SupportingDocumentStatusHelper.UpdateStatus(entryHeader, caseNumber);
			AssertEquals(DocumentStatusCodes.Codes.PND, caseNumber.Document_Status);

			var helper2 = new SupportingDocumentStatusHelperTest.Helper(Factory, declaration, entryHeader, "123123");
			var ediMessage = helper2.CreateNewDDVEDIMessage(fileName);
			UniversalEvent universalEvent;

			using (universalEvent = ediMessage?.GetEM_MessageTextReader()?.Parse<UniversalEvent>())
			{
				processor.Process(declaration, universalEvent);
				AssertEquals("DDV for different Case Number should not update status", DocumentStatusCodes.Codes.PND, caseNumber.Document_Status);
				helper2.CreateNewEvent(Events.DocumentDelivered, ediMessage);

				ediMessage = helper.CreateNewDDVEDIMessage(fileName);
			}

			using (universalEvent = ediMessage?.GetEM_MessageTextReader()?.Parse<UniversalEvent>())
			{
				processor.Process(declaration, universalEvent);
				AssertEquals("DDV for same Case Number should update status", DocumentStatusCodes.Codes.SNT, caseNumber.Document_Status);
				helper.CreateNewEvent(Events.DocumentDelivered, ediMessage);

				fileName = "test2.pdf";
				helper.CreateNewEvent(Events.DocumentSent, helper.CreateNewDSNEDIMessage(fileName));
				SupportingDocumentStatusHelper.UpdateStatus(entryHeader, caseNumber);
				AssertEquals("Sending new Document after SNT should update to PND", DocumentStatusCodes.Codes.PND, caseNumber.Document_Status);

				ediMessage = helper.CreateNewDNDEDIMessage(fileName);
			}

			using (universalEvent = ediMessage?.GetEM_MessageTextReader()?.Parse<UniversalEvent>())
			{
				processor.Process(declaration, universalEvent);
				AssertEquals(DocumentStatusCodes.Codes.FAL, caseNumber.Document_Status);
				helper.CreateNewEvent(Events.DocumentNotDelivered, ediMessage);

				fileName = "test3.pdf";
				helper.CreateNewEvent(Events.DocumentSent, helper.CreateNewDSNEDIMessage(fileName));
				SupportingDocumentStatusHelper.UpdateStatus(entryHeader, caseNumber);
				AssertEquals("Sending new Document after FAL should update to PND", DocumentStatusCodes.Codes.PND, caseNumber.Document_Status);

				ediMessage = helper.CreateNewDDVEDIMessage(fileName);
			}

			using (universalEvent = ediMessage?.GetEM_MessageTextReader()?.Parse<UniversalEvent>())
			{
				processor.Process(declaration, universalEvent);
				AssertEquals("DDV for new Document after FAL should update to SNT", DocumentStatusCodes.Codes.SNT, caseNumber.Document_Status);
				helper.CreateNewEvent(Events.DocumentDelivered, ediMessage);

				fileName = "test4.pdf";
				helper.CreateNewEvent(Events.DocumentSent, helper.CreateNewDSNEDIMessage(fileName));
				SupportingDocumentStatusHelper.UpdateStatus(entryHeader, caseNumber);
				AssertEquals("Should update to PND despite prior FAL status", DocumentStatusCodes.Codes.PND, caseNumber.Document_Status);

				ediMessage = helper.CreateNewDDVEDIMessage(fileName);
			}

			using (universalEvent = ediMessage?.GetEM_MessageTextReader()?.Parse<UniversalEvent>())
			{
				processor.Process(declaration, universalEvent);
				AssertEquals("Should update to SNT despite prior FAL status", DocumentStatusCodes.Codes.SNT, caseNumber.Document_Status);
			}
		}
	}

	sealed class XmlEventValueObject_ForTest : IXmlEventValueObject
	{
		public XmlEventValueObject_ForTest(ZString lrn, ZString caseNo)
		{
			var parameters = new Dictionary<string, string>();
			parameters.Add(ParameterCodes.ReferenceNumber, lrn);
			parameters.Add(ParameterCodes.RequestNumber, caseNo);
			eventReference = StmALog.GenerateEventReference(ZString.Empty, parameters);
		}
		readonly ZString eventReference;

		public IXmlEventValueObjectContextValueList Context => throw new NotImplementedException();

		public ZDateTimeOffset CreatedTime => throw new NotImplementedException();

		public IDataContextDataObject DataContext
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString EventReference => eventReference;

		public ZDateTimeOffset EventTime => throw new NotImplementedException();

		public ZString EventType { get; set; }

		public ZBool IsCancelled => throw new NotImplementedException();

		public ZBool IsEstimate => throw new NotImplementedException();

		public IEnumerable<IMessageNumber> MessageNumberCollection => throw new NotImplementedException();

		public void Dispose()
		{
		}

		public void SetWriterStrategy(IDataObjectWriterStrategy strategy) => throw new NotImplementedException();

		public void SetMessageNumber(MessageNumberType type, ZString value)
		{
			throw new NotImplementedException();
		}
	}
}
