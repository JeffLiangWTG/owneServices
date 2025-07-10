using System.IO;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.DocumentSending.Testing
{
	sealed class SupportingDocUniversalEventBuilderTests : TestCaseWithFactory
	{
		[TestDate(2020, 01, 01, 0, 0, 0)]
		public void TestBuildUniversalEvent()
		{
			CombineAssertions("Catch Error in case pre-send check were missed", () =>
			{
				var sendingObject = SupportingDocSendingObject.New(declaration) as SupportingDocSendingObject;
				sendingObject.EDoc = eDoc.UniqueKey;

				var eventBuilder = new ZASupportingDocUniversalEventBuilder(sendingObject);
				using (var universalEvent = eventBuilder.BuildUniversalEvent(new SupportingDocSendingObject[] { sendingObject }).Single())
				{
					string xml;

					using (var stream = (SubStreamableStream)new MemoryStream())
					using (var reader = new StreamReader(stream))
					{
						new XmlWriter().WriteXML(universalEvent, stream, false);
						stream.Flush();
						stream.Position = 0;
						xml = reader.ReadToEnd();
					}

					AssertMultilineASCIIEquals("Universal Event", ExpectedResult(universalEvent, declaration), ReplaceEventTime(xml, "2016-08-16T00:28:45.837"));
				}
			});
		}

		protected override void SetUp()
		{
			Enterprise.Customs.ZA.Business.Testing.ZAUniversalReferenceTestDataHelper.SetupRefCusMapAndRefCusMapType(Factory);
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			byte[] imageBytes = new byte[1];
			eDoc = declaration.DocManagerInfo.AddFileOrDocument(imageBytes, "Invoice.pdf", "CIV");

			entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "12341234";
			instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			caseNumber1 = instruction.CaseNumbers.AddNew();
			caseNumber1.CY_Data = "1234567";

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var agentOrg = Factory.New<OrgHeader>();
			agentOrg.OH_Code = "AGENTCODE";
			agentOrg.OH_FullName = "AgentName";
			declaration.JE_OH_AgentOverride = agentOrg.PK;
			agentOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "AGT7634", Core.Constants.CountryCodes.SouthAfrica);
			agentOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, "CDPC", Core.Constants.CountryCodes.SouthAfrica);

			declaration.JE_CustomsOffice = "123";

			Factory.Save();
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		CusEntryInstruction instruction;
		CaseNumber caseNumber1;
		IeDoc eDoc;

		string ReplaceEventTime(string text, string dateString)
		{
			string startTag = "<EventTime>";
			string endTag = "</EventTime>";
			int startPos = text.IndexOf(startTag);
			int endPos = text.IndexOf(endTag);

			return text.Substring(0, startPos + startTag.Length) + dateString + text.Substring(endPos, text.Length - endPos);
		}

		string ExpectedResult(UniversalEvent universalEvent, JobDeclaration declaration)
		{
			var company = declaration.Branch.Company;
			var server = universalEvent.DataContext.GetEnterpriseServerAndCompanyIDs();
			return @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>CustomsDeclaration</Type>
          <Key>" + universalEvent.DataContext.DataSourceCollection.First().Key + @"</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>" + company.GC_Code + @"</Code>
        <Country>
          <Code>" + company.Country.RN_Code + @"</Code>
          <Name>" + company.Country.RN_Desc + @"</Name>
        </Country>
        <Name>" + company.GC_Name + @"</Name>
      </Company>
      <DataProvider>" + universalEvent.DataContext.DataProviderForCodeMapping + @"</DataProvider>
      <EnterpriseID>" + server.EnterpriseID + @"</EnterpriseID>
      <ServerID>" + server.ServerID + @"</ServerID>
    </DataContext>

    <EventTime>2016-08-16T00:28:45.837</EventTime>
    <EventType>DSN</EventType>
    <EventParameters>
      <ExternalDocumentType>INV</ExternalDocumentType>
      <ReferenceNumber>AGT763412320200101000001</ReferenceNumber>
      <RequestNumber></RequestNumber>
    </EventParameters>
    <EventReference>|EXT=INV|RFN=AGT763412320200101000001</EventReference>

    <AttachedDocumentCollection>
      <AttachedDocument>
        <FileName>Invoice.pdf</FileName>
        <ImageData>AA==</ImageData>
        <Type>
          <Code>INV</Code>
          <Description>CustomsAuthority</Description>
        </Type>
      </AttachedDocument>
    </AttachedDocumentCollection>

    <ContextCollection>
      <Context>
        <Type>TradingPartyID</Type>
        <Value>AGT7634</Value>
      </Context>
      <Context>
        <Type>DualProfileCode</Type>
        <Value>CDPC</Value>
      </Context>
      <Context>
        <Type>FileName</Type>
        <Value>Invoice.pdf</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
		}
	}
}
