using System.Linq;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal.Writer.Testing
{
	sealed class ISFLineDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestISFLineMapping()
		{
			var lineBO = SetupISFLine();
			var helper = new ISFDataObjectHelper(lineBO.Header);
			var writer = new ISFLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, lineBO)), helper);
			var lineDataObject = writer.GetDataObject(lineBO);
			AssertEquals(lineBO.BL_TextProductCode, lineDataObject.PartNo);
			AssertEquals(Core.Constants.CountryCodes.China, lineDataObject.CountryOfOrigin.GetCodeAsUpperCase());
			AssertEquals("9201.10.00", lineDataObject.HarmonisedCode);
			AssertEquals("lineDataObject.OrganizationAddressCollection.Count", 1, lineDataObject.OrganizationAddressCollection.Count);
			var manufacturerData = lineDataObject.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.Manufacturer));
			AssertNotNull(manufacturerData);
			AssertEquals("INCTESTMANU", manufacturerData.OrganizationCode);
			AssertEquals("IAN TEST ADDRESS1", manufacturerData.Address1);
			var customAttribOne = lineDataObject.AddInfoCollection.GetZStringValue(ISFConstants.CustomizedFieldConstants.CustomAttribOne);
			AssertEquals(lineBO.CustomAttribute1, customAttribOne);
			var customAttribTwo = lineDataObject.AddInfoCollection.GetZStringValue(ISFConstants.CustomizedFieldConstants.CustomAttribTwo);
			AssertEquals(lineBO.CustomAttribute2, customAttribTwo);
		}

		CusISFLine SetupISFLine()
		{
			var header = Factory.New<CusISFHeader>();
			var manufacturer = header.DocAddresses.CreateWithAddressType(DocAddressType.Manufacturer);
			var address = Factory.New<OrgAddress>();
			address.OA_Address1 = "IAN TEST ADDRESS1";
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "INCTESTMANU";
			address.OA_OH = orgHeader.PK;
			manufacturer.E2_OA_Address = address.PK;
			var line = header.Lines.AddNew();
			line.BL_ManufacturerDocAddressPK = manufacturer.PK;
			line.BL_TextProductCode = "TEST PRODUCT";
			line.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.China;
			line.BL_FormattedHarmonisedNum = "9201.10.00";
			line.CustomAttribute1 = "123445";
			line.CustomAttribute2 = "233456";
			return line;
		}
	}
}
