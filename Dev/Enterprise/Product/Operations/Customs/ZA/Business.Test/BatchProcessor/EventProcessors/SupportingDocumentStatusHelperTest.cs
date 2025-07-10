using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.EventProcessors;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Integration;
using ParameterCodes = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using UniversalContext = Enterprise.UniversalDataBuss.DataObjects.Universal.Context;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class SupportingDocumentStatusHelperTest : TestCaseWithFactory
	{
		public void TestUpdateStatus()
		{
			var lrn = "123456";
			var caseNo = "654321";
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = lrn;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var caseNumber = entryInstruction.CaseNumbers.AddNew();
			caseNumber.CY_Data = caseNo;
			SupportingDocumentStatusHelper.UpdateStatus(entryHeader, caseNumber);
			AssertEquals(ZString.Empty, caseNumber.Document_Status);
			var helper = new Helper(Factory, declaration, entryHeader, caseNo);
			var fileName = "test.pdf";
			helper.CreateNewEvent(Events.DocumentSent, helper.CreateNewDSNEDIMessage(fileName));
			SupportingDocumentStatusHelper.UpdateStatus(entryHeader, caseNumber);
			AssertEquals(DocumentStatusCodes.Codes.PND, caseNumber.Document_Status);
			var helper2 = new Helper(Factory, declaration, entryHeader, "123123");
			helper2.CreateNewEvent(Events.DocumentDelivered, helper2.CreateNewDDVEDIMessage(fileName));
			Factory.Save();
			SupportingDocumentStatusHelper.UpdateStatus(entryHeader, caseNumber);
			AssertEquals("DDV for different Case Number should not update status", DocumentStatusCodes.Codes.PND, caseNumber.Document_Status);
			var ediMessage = helper.CreateNewDDVEDIMessage(fileName);
			var universalEvent = ediMessage?.GetEM_MessageTextReader()?.Parse<UniversalEvent>();
			SupportingDocumentStatusHelper.UpdateStatus(entryHeader, caseNumber, universalEvent);
			AssertEquals("DDV for same Case Number should update status", DocumentStatusCodes.Codes.SNT, caseNumber.Document_Status);
			helper.CreateNewEvent(Events.DocumentDelivered, ediMessage);
			fileName = "test2.pdf";
			helper.CreateNewEvent(Events.DocumentSent, helper.CreateNewDSNEDIMessage(fileName));
			SupportingDocumentStatusHelper.UpdateStatus(entryHeader, caseNumber);
			AssertEquals("Sending new Document after SNT should update to PND", DocumentStatusCodes.Codes.PND, caseNumber.Document_Status);
			ediMessage = helper.CreateNewDNDEDIMessage(fileName);
			universalEvent = ediMessage?.GetEM_MessageTextReader()?.Parse<UniversalEvent>();
			SupportingDocumentStatusHelper.UpdateStatus(entryHeader, caseNumber, universalEvent);
			AssertEquals(DocumentStatusCodes.Codes.FAL, caseNumber.Document_Status);
			helper.CreateNewEvent(Events.DocumentNotDelivered, ediMessage);
			fileName = "test3.pdf";
			helper.CreateNewEvent(Events.DocumentSent, helper.CreateNewDSNEDIMessage(fileName));
			SupportingDocumentStatusHelper.UpdateStatus(entryHeader, caseNumber);
			AssertEquals("Sending new Document after FAL should update to PND", DocumentStatusCodes.Codes.PND, caseNumber.Document_Status);
			ediMessage = helper.CreateNewDDVEDIMessage(fileName);
			universalEvent = ediMessage?.GetEM_MessageTextReader()?.Parse<UniversalEvent>();
			SupportingDocumentStatusHelper.UpdateStatus(entryHeader, caseNumber, universalEvent);
			AssertEquals("DDV for new Document after FAL should update to SNT", DocumentStatusCodes.Codes.SNT, caseNumber.Document_Status);
			helper.CreateNewEvent(Events.DocumentDelivered, ediMessage);
			fileName = "test4.pdf";
			helper.CreateNewEvent(Events.DocumentSent, helper.CreateNewDSNEDIMessage(fileName));
			SupportingDocumentStatusHelper.UpdateStatus(entryHeader, caseNumber);
			AssertEquals("Should update to PND despite prior FAL status", DocumentStatusCodes.Codes.PND, caseNumber.Document_Status);
			ediMessage = helper.CreateNewDDVEDIMessage(fileName);
			universalEvent = ediMessage?.GetEM_MessageTextReader()?.Parse<UniversalEvent>();
			SupportingDocumentStatusHelper.UpdateStatus(entryHeader, caseNumber, universalEvent);
			AssertEquals("Should update to SNT despite prior FAL status", DocumentStatusCodes.Codes.SNT, caseNumber.Document_Status);
		}

		public void TestIsEventRelatedToCase()
		{
			var lrn = "1111";
			var caseNo = "2222";
			var parameters = new Dictionary<string, string>();
			parameters.Add(ParameterCodes.ReferenceNumber, lrn);
			parameters.Add(ParameterCodes.RequestNumber, caseNo);
			var evnt = Factory.New<StmALog>();
			using (evnt.LockForUpdatingKeyFieldsForTesting())
			{
				evnt.SL_Reference = StmALog.GenerateEventReference(ZString.Empty, parameters);
			}

			Assert(SupportingDocumentStatusHelper.IsEventRelatedToCase(evnt, lrn, caseNo));
		}

		public void TestGetFileName()
		{
			var fileName = "test.pdf";
			var universalEvent = new UniversalEvent();
			universalEvent.ContextCollection = new List<UniversalContext>();
			universalEvent.ContextCollection.Add(new UniversalContext()
			{ Type = "FileName", Value = fileName });
			AssertEquals(fileName, SupportingDocumentStatusHelper.GetFileName(universalEvent));
		}

		internal class Helper
		{
			readonly BusinessObjectFactory factory;
			readonly JobDeclaration declaration;
			readonly CusEntryHeader entryHeader;
			readonly ZString caseNumber;
			public Helper(BusinessObjectFactory factory, JobDeclaration declaration, CusEntryHeader entryHeader, ZString caseNumber)
			{
				this.factory = factory;
				this.declaration = declaration;
				this.entryHeader = entryHeader;
				this.caseNumber = caseNumber;
			}

			public void CreateNewEvent(Event evnt, EDIMessage message)
			{
				var parameters = new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>(ParameterCodes.RequestNumber, caseNumber),
					new KeyValuePair<string, string>(ParameterCodes.ReferenceNumber, entryHeader.CH_BGMReference),
				};
				var log = declaration.Logs.AddNew(evnt, parameters);
				var pivot = factory.New<IGenPivot>();
				pivot.XX_RelationType = Core.Constants.GenPivotTypes.XmlEdiMessage;
				pivot.XX_Relation1ID = log.PK;
				pivot.XX_Relation1TableCode = "SL";
				pivot.XX_Relation2ID = message.PK;
				pivot.XX_Relation2TableCode = "EM";
			}

			public EDIMessage CreateNewDSNEDIMessage(ZString fileName)
			{
				var message = factory.NewWithValidTestData<XmlEDIMessage>();
				message.Content = XElement.Parse(GetDSNUniversalEvent(ZDateTime.Now, fileName));
				factory.Save();
				return message;
			}

			ZString GetDSNUniversalEvent(ZDateTime eventTime, ZString fileName)
			{
				return ZString.Format(DSNUniveralEvent, declaration.JE_DeclarationReference, entryHeader.CH_BGMReference, caseNumber, fileName, eventTime);
			}

			const string DSNUniveralEvent = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11\"" version=""1.1"">
	  <Event>
		<DataContext>
		  <DataSourceCollection>
			<DataSource>
			  <Type>CustomsDeclaration</Type>
			  <Key>{0}</Key>
			</DataSource>
		  </DataSourceCollection>
		  <Company>
			<Code>DZA</Code>
			<Country>
			  <Code>ZA</Code>
			  <Name>South Africa</Name>
			</Country>
			<Name>EDI Demonstration System ZA</Name>
		  </Company>
		  <DataProvider>HYEBP1DZA</DataProvider>
		  <EnterpriseID>HYE</EnterpriseID>
		  <ServerID>BP1</ServerID>
		</DataContext>
		<EventTime>{4}</EventTime>
		<EventType>DSN</EventType>
		<EventParameters>
		  <ExternalDocumentType>ACW</ExternalDocumentType>
		  <ReferenceNumber>{1}</ReferenceNumber>
		  <RequestNumber>{2}</RequestNumber>
		</EventParameters>
		<EventReference>|EXT=ACW|RFN={1}|RQN={2}</EventReference>
		<AttachedDocumentCollection>
		  <AttachedDocument>
			<FileName>{3}</FileName>
			<ImageData></ImageData>
			<Type>
			  <Code>ACW</Code>
			  <Description>CustomsAuthority</Description>
			</Type>
		  </AttachedDocument>
		</AttachedDocumentCollection>
		<ContextCollection>
		  <Context>
			<Type>TradingPartyID</Type>
			<Value>87654321</Value>
		  </Context>
		  <Context>
			<Type>DualProfileCode</Type>
			<Value></Value>
		  </Context>
		  <Context>
			<Type>FileName</Type>
			<Value>{3}</Value>
		  </Context>
		</ContextCollection>
	  </Event></UniversalEvent>";
			public EDIMessage CreateNewDDVEDIMessage(ZString fileName)
			{
				var message = factory.NewWithValidTestData<XmlEDIMessage>();
				message.Content = XElement.Parse(GetDDVUniversalEvent(ZDateTime.Now, fileName));
				return message;
			}

			ZString GetDDVUniversalEvent(ZDateTime eventTime, ZString fileName)
			{
				return ZString.Format(DDVUniveralEvent, declaration.JE_DeclarationReference, eventTime, entryHeader.CH_BGMReference, caseNumber, fileName);
			}

			const string DDVUniveralEvent = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
		  <Event>
			<DataContext>
			  <DataTargetCollection>
				<DataTarget>
				  <Type>CustomsDeclaration</Type>
				  <Key>{0}</Key>
				</DataTarget>
			  </DataTargetCollection>
			</DataContext>
			<EventTime>{1}</EventTime>
			<EventType>DDV</EventType>
			<EventReference>|EXT=INV|RFN={2}|RQN={3}|RCP=00505655TST-e7d7b313-2824-4445-87a3-c3565ed53174</EventReference>
			<ContextCollection>
			  <Context>
				<Type>FileName</Type>
				<Value>{4}</Value>
			  </Context>   
			</ContextCollection>
		  </Event>
		</UniversalEvent>";
			public EDIMessage CreateNewDNDEDIMessage(ZString fileName)
			{
				var message = factory.NewWithValidTestData<XmlEDIMessage>();
				message.Content = XElement.Parse(GetDNDUniversalEvent(ZDateTime.Now, fileName));
				return message;
			}

			ZString GetDNDUniversalEvent(ZDateTime eventTime, ZString fileName)
			{
				return ZString.Format(DNDUniveralEvent, declaration.JE_DeclarationReference, eventTime, entryHeader.CH_BGMReference, caseNumber, fileName);
			}

			const string DNDUniveralEvent = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
		  <Event>
			<DataContext>
			  <DataTargetCollection>
				<DataTarget>
				  <Type>CustomsDeclaration</Type>
				  <Key>{0}</Key>
				</DataTarget>
			  </DataTargetCollection>
			</DataContext>
			<EventTime>{1}</EventTime>
			<EventType>DND</EventType>
			<EventReference>|EXT=INV|RFN={2}|RQN={3}|RES=Some Reason</EventReference>
			<ContextCollection>
			  <Context>
				<Type>FileName</Type>
				<Value>{4}</Value>
			  </Context>   
			</ContextCollection>
		  </Event>
		</UniversalEvent>";
		}
	}
}
