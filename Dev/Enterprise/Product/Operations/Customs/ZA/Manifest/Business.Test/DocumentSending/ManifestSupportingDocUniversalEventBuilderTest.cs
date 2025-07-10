using System.IO;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.MasterFiles.Integration;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	class ManifestSupportingDocUniversalEventBuilderTests : TestCaseWithFactory
	{
		public void TestBuildUniversalEvent()
		{
			CombineAssertions("Catch Error in case pre-send check were missed", () =>
			{
				var sendingObject = SupportingDocSendingObject.New(manifest);
				sendingObject.EDoc = eDoc.UniqueKey;
				sendingObject.DocumentType = "INV";

				var eventBuilder = new ManifestSupportingDocUniversalEventBuilder(sendingObject);
				using (var universalEvent = eventBuilder.BuildUniversalEvent(new ISupportingDocumentMessageDataProvider[] { sendingObject }).Single())
				{
					string xml;

					using (var stream = new CargoWise.IO.Shim.SubStreamableStream())
					using (var reader = new StreamReader(stream))
					{
						new UniversalDataBuss.XmlIO.XmlWriting.XmlWriter().WriteXML(universalEvent, stream, false);
						stream.Flush();
						stream.Position = 0;
						xml = reader.ReadToEnd();
					}

					AssertMultilineASCIIEquals("Universal Event", ExpectedResult(universalEvent, manifest), ReplaceEventTime(xml, "2016-08-16T00:28:45.837"));
				}
			});
		}

		string ReplaceEventTime(string text, string dateString)
		{
			string startTag = "<EventTime>";
			string endTag = "</EventTime>";
			int startPos = text.IndexOf(startTag);
			int endPos = text.IndexOf(endTag);

			return text.Substring(0, startPos + startTag.Length) + dateString + text.Substring(endPos, text.Length - endPos);
		}

		string ExpectedResult(UniversalEvent universalEvent, AsycudaManifestHeader manifest)
		{
			var company = manifest.Branch.Company;
			var server = universalEvent.DataContext.GetEnterpriseServerAndCompanyIDs();
			return @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AsycudaManifest</Type>
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
      <ReferenceNumber></ReferenceNumber>
      <RequestNumber></RequestNumber>
    </EventParameters>
    <EventReference>|EXT=INV</EventReference>

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
        <Type>FileName</Type>
        <Value>Invoice.pdf</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			eDoc = manifest.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			Factory.Save();
		}

		AsycudaManifestHeader manifest;
		IeDoc eDoc;

		#endregion
	}
}
