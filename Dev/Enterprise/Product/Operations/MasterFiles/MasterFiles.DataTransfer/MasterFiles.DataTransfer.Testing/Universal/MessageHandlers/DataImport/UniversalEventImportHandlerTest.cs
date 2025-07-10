using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	public class UniversalEventImportHandlerTest : TestCaseWithFactory
	{
		public void TestImportEDocWhenDocTypeDescriptionIsEmpty()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00009001";
			Factory.Save();

			var requestXml =
$@"<UniversalEvent version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Event>

    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>S00009001</Key>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
    </DataContext>

    <EventTime>2016-07-19T13:36:39.707</EventTime>
    <EventType>DDI</EventType>
    <IsEstimate>false</IsEstimate>

    <AttachedDocumentCollection>
      <AttachedDocument>
        <FileName>hello1.txt</FileName>
        <ImageData>SGVsbG8sIFdvcmxkIQ==</ImageData>
        <Type>
          <Code>TSR</Code>
        </Type>
        <IsPublished>true</IsPublished>
        <VisibleCompanyCode></VisibleCompanyCode>
        <VisibleBranchCode></VisibleBranchCode>
        <VisibleDepartmentCode></VisibleDepartmentCode>
      </AttachedDocument>
	  <AttachedDocument>
        <FileName>hello2.txt</FileName>
        <ImageData>SGVsbG8sIFdvcmxkIQ==</ImageData>
        <Type>
          <Code>MSC</Code>
        </Type>
        <IsPublished>true</IsPublished>
        <VisibleCompanyCode></VisibleCompanyCode>
        <VisibleBranchCode></VisibleBranchCode>
        <VisibleDepartmentCode></VisibleDepartmentCode>
      </AttachedDocument>
    </AttachedDocumentCollection>

  </Event>
</UniversalEvent>";

			var handler = new UniversalEventImportHandler(new XmlSessionTracker(new SimpleLogger()));
			var request = handler.CreateRequestMessage();
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new StreamWriter(stream) { AutoFlush = true }.Write(requestXml);
				request.SetMessageTextSource(stream);
				request.Save();
			}

			using ((request as BusinessObject).Factory.AddDisposableService())
			using (var processingResult = handler.Process(request))
			{
				request.Save();
				AssertNotNull(processingResult);
				AssertEquals("PRS", processingResult.Status);
			}

			var reloadedShipment = new BusinessObjectFactory().Load<Forwarding.IForwardingShipment>(shipment.PK);
			var reloadedDocManagerInfo = ((IDocManagerSupport)reloadedShipment).DocManagerInfo;

			AssertEquals(2, reloadedDocManagerInfo.AllEDocs.Count);

			var hello1 = reloadedDocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(eDoc => eDoc.FileName == "hello1.txt");
			AssertNotNull(hello1);
			AssertEquals("TSR", hello1.DocType);
			AssertEquals("use system doc type description if not provided.", "Time Slot Request", hello1.Description);

			var hello2 = reloadedDocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(eDoc => eDoc.FileName == "hello2.txt");
			AssertNotNull(hello2);
			AssertEquals("MSC", hello2.DocType);
			AssertEquals("msc code as desc if not provided.", "MSC", hello2.Description);
		}

		public void TestProcess()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00009001";

			var docManagerInfo = ((IDocManagerSupport)shipment).DocManagerInfo;
			var eDoc1 = CreateEDocForTest(docManagerInfo, "File1", "INV", true, new ZDateTime(2016, 7, 1, 12, 15, 00));

			docManagerInfo.MasterFactory.Save();
			Factory.Save();

			var requestXml =
$@"<UniversalEvent version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>S00009001</Key>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}EDI</DataProvider>
      <EnterpriseID>{GlbCompany.CurrentCompany.LicenceEnterpriseCode}</EnterpriseID>
      <ServerID>{GlbCompany.CurrentCompany.LicenceServerID}</ServerID>
    </DataContext>

    <EventTime>2016-07-19T13:36:39.707</EventTime>
    <EventType>DDI</EventType>
    <IsEstimate>false</IsEstimate>

    <AttachedDocumentCollection>
      <AttachedDocument>
        <FileName>hello.txt</FileName>
        <ImageData>SGVsbG8sIFdvcmxkIQ==</ImageData>
        <Type>
          <Code>TSR</Code>
          <Description>Time Slot Request</Description>
        </Type>
        <IsPublished>true</IsPublished>
        <VisibleCompanyCode></VisibleCompanyCode>
        <VisibleBranchCode>{GlbBranch.CurrentBranch.GB_Code}</VisibleBranchCode>
        <VisibleDepartmentCode></VisibleDepartmentCode>
      </AttachedDocument>
    </AttachedDocumentCollection>

  </Event>
</UniversalEvent>";

			var handler = new UniversalEventImportHandler(new XmlSessionTracker(new SimpleLogger()));
			var request = handler.CreateRequestMessage();
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new StreamWriter(stream) { AutoFlush = true }.Write(requestXml);
				request.SetMessageTextSource(stream);
				request.Save();
			}

			using ((request as BusinessObject).Factory.AddDisposableService())
			using (var processingResult = handler.Process(request))
			{
				request.Save();
				AssertNotNull(processingResult);
				AssertEquals("PRS", processingResult.Status);

				processingResult.ResponseMessageText.Position = 0;
				var responseMessageText = new StreamReader(processingResult.ResponseMessageText).ReadToEnd();

				AssertContains(
	@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00009001</Key>
        </DataSource>
      </DataSourceCollection>", responseMessageText);

				AssertContains("<EventType>DIM</EventType>", responseMessageText);
				AssertNotContains("<AttachedDocumentCollection>", responseMessageText);
			}

			var reloadedShipment = new BusinessObjectFactory().Load<Forwarding.IForwardingShipment>(shipment.PK);
			var reloadedDocManagerInfo = ((IDocManagerSupport)reloadedShipment).DocManagerInfo;

			AssertEquals(2, reloadedDocManagerInfo.AllEDocs.Count);

			var newDoc = reloadedDocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(eDoc => eDoc.FileName == "hello.txt");
			AssertNotNull(newDoc);
			AssertEquals("TSR", newDoc.DocType);
			AssertEquals(true, newDoc.IsPublished);
			AssertEquals(GlbBranch.CurrentBranch.GB_Code, newDoc.VisibleBranchCode);

			using (var streamReader = new StreamReader(newDoc.GetImageDataReader(), Encoding.UTF8))
			{
				AssertEquals("Hello, World!", streamReader.ReadToEnd()); // <ImageData>SGVsbG8sIFdvcmxkIQ==</ImageData>
			}
		}

		public void TestImportDocumentSizeLargerThanMaximumFileSize()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00009001";
			Factory.Save();

			var currentCompany = GlbCompany.CurrentCompany;
			var xml = FormattableString.Invariant($@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
				  <Event>
				    <DataContext>
				      <DataTargetCollection>
				        <DataTarget>
				          <Type>ForwardingShipment</Type>
				          <Key>{shipment.JS_UniqueConsignRef}</Key>
				        </DataTarget>
				      </DataTargetCollection>
				      <EnterpriseID>{currentCompany.LicenceEnterpriseCode}</EnterpriseID>
				      <ServerID>{currentCompany.LicenceServerID}</ServerID>
				      <Company>
				        <Code>{currentCompany.GC_Code}</Code>
				      </Company>
				    </DataContext>
				    <EventTime>2016-06-09T12:32:12.42</EventTime>
				    <EventType>DDI</EventType>
				    <EventReference>PAL</EventReference>
				    <IsEstimate>false</IsEstimate>
				    <AttachedDocumentCollection>
				      <AttachedDocument>
				        <FileName>Test.txt</FileName>
				        <ImageData>SGVsbG8sIFdvcmxkIQ==</ImageData>
				        <Type>
				          <Code>PAL</Code>
				          <Description>Pre Alert</Description>
				        </Type>
				        <IsPublished>true</IsPublished>
				        <VisibleCompanyCode>EDI</VisibleCompanyCode>
				        <VisibleBranchCode>BNE</VisibleBranchCode>
				      </AttachedDocument>
				    </AttachedDocumentCollection>
				  </Event>
				</UniversalEvent>");

			var handler = new UniversalEventImportHandler(new XmlSessionTracker(new SimpleLogger()));
			var request = handler.CreateRequestMessage();
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new StreamWriter(stream) { AutoFlush = true }.Write(xml);
				request.SetMessageTextSource(stream);
				request.Save();
			}

			SystemDataRegistry.Instance.eDocsMaximumFilesize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			using ((request as BusinessObject).Factory.AddDisposableService())
			using (var processingResult = handler.Process(request))
			{
				request.Save();
				var responseMessageText = "";
				if (processingResult != null)
				{
					processingResult.ResponseMessageText.Position = 0;
					responseMessageText = new StreamReader(processingResult.ResponseMessageText).ReadToEnd();
				}

				CombineAssertions(() =>
				{
					AssertNotNull(processingResult);
					AssertEquals("PRS", processingResult.Status);
					AssertNotEquals(0, responseMessageText.Length);
					AssertContains("<Value>Warning - The file size of 'Test.txt' exceeds the 0MB maximum file size allowed for eDocs.", responseMessageText);
				});
			}
		}

		IeDoc CreateEDocForTest(DocManagerInfo docManagerInfo, string fileName, string docType, bool isPublished, ZDateTime docDateTime)
		{
			var eDoc = docManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, fileName, docType);
			eDoc.IsPublished = isPublished;
			eDoc.SetValuesForTest(docDateTime, docType);

			return eDoc;
		}
	}
}
