using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.DataTransfer.Universal.Helpers;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.DataTransfer.Test
{
	public class DocumentHelperTest : TestCaseWithFactory
	{
		const string SampleResponseBody = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>PRS</Status>
  <Data>
    <UniversalShipment xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
      <Shipment>
        <CartageWaybillNumber>111JD7570023</CartageWaybillNumber>
        <JobCosting>
          <TotalCost>22.76</TotalCost>
        </JobCosting>
        <UniqueConsignmentReference>MoEK0E0JhVgAAAGQ2xcCDsRa</UniqueConsignmentReference>
        <AddInfoCollection>
          <AddInfo>
            <Key>LABEL</Key>
            <Value>111JD757002301000961503</Value>
          </AddInfo>
        </AddInfoCollection>
        <AttachedDocumentCollection>
          <AttachedDocument>
            <FileName>labels.pdf</FileName>
            <ImageData>d3Rn</ImageData>
            <Type>
              <Code>LBL</Code>
              <Description>Carrier Labels</Description>
            </Type>
          </AttachedDocument>
        </AttachedDocumentCollection>
      </Shipment>
    </UniversalShipment>
  </Data>
</UniversalResponse>";

		[TestDate(2024, 8, 14, 14, 5, 45)]
		public void TestGeneratedLabelName_NoSuffix()
		{
			AssertEquals($"LBL_{ZDateTime.Now:yyyyMMdd_HHmmss}.pdf", DocumentHelper.GenerateLabelName("LBL", ""));
		}

		[TestDate(2024, 8, 14, 14, 5, 45)]
		public void TestGeneratedLabelName_WithSuffix()
		{
			AssertEquals($"LBL_{ZDateTime.Now:yyyyMMdd_HHmmss}_1.pdf", DocumentHelper.GenerateLabelName("LBL", "1"));
		}

		[TestDate(2024, 8, 14, 14, 5, 45)]
		public void TestAddDocumentToBooking()
		{
			var carrierBooking = Factory.New<DtbConsignment>();
			var label = DocumentHelper.GenerateLabelName("LBL", "");
			DocumentHelper.AddDocumentFromUniversalResponse(SampleResponseBody, carrierBooking, label);
			var docManager = carrierBooking.DocManagerInfo();
			AssertEquals(1, docManager.AllEDocs.Count);
			AssertEquals(label, docManager.AllEDocs[0].FileName);
		}
	}
}

