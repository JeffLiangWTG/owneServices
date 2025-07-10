using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.Business.Testing
{
	public sealed class ACEFDAJobDocAddressLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOrgHeader_List()
		{
			docAddress.E2_AddressType = DocAddressTypes.Codes.GoodsDeliveredTo;
			AssertType<ConsigneeCollection>(lookups.OrgHeader_List);

			docAddress.E2_AddressType = DocAddressTypes.Codes.Consolidator;
			AssertType<ConsignorCollection>(lookups.OrgHeader_List);

			docAddress.E2_AddressType = DocAddressTypes.Codes.Grower;
			AssertType<ConsignorCollection>(lookups.OrgHeader_List);

			docAddress.E2_AddressType = DocAddressTypes.Codes.Manufacturer;
			AssertType<ConsignorCollection>(lookups.OrgHeader_List);

			docAddress.E2_AddressType = DocAddressTypes.Codes.Shipper;
			AssertType<OrganisationsFindBoxCollection>(lookups.OrgHeader_List);

			docAddress.E2_AddressType = DocAddressTypes.Codes.ThirdPartyLaboratory;
			AssertType<OrganisationsFindBoxCollection>(lookups.OrgHeader_List);
		}

		public void TestAddressTypeList()
		{
			var addressTypes = lookups.AddressTypeList;
			AssertEquals(13, addressTypes.Count);

			Assert(addressTypes.ContainsCode(DocAddressTypes.Codes.Shipper));
			Assert(addressTypes.ContainsCode(DocAddressTypes.Codes.GoodsDeliveredTo));
			Assert(addressTypes.ContainsCode(DocAddressTypes.Codes.ImporterDocumentaryAddress));
			Assert(addressTypes.ContainsCode(DocAddressTypes.Codes.FSVPImporter));
			Assert(addressTypes.ContainsCode(DocAddressTypes.Codes.GoodsOwner));
			Assert(addressTypes.ContainsCode(DocAddressTypes.Codes.Manufacturer));
			Assert(addressTypes.ContainsCode(DocAddressTypes.Codes.GoodsLocation));
			Assert(addressTypes.ContainsCode(DocAddressTypes.Codes.InitialImporter));
			Assert(addressTypes.ContainsCode(DocAddressTypes.Codes.Sponsor));
			Assert(addressTypes.ContainsCode(DocAddressTypes.Codes.Consolidator));
			Assert(addressTypes.ContainsCode(DocAddressTypes.Codes.Grower));
			Assert(addressTypes.ContainsCode(DocAddressTypes.Codes.Laboratory));
			Assert(addressTypes.ContainsCode(DocAddressTypes.Codes.ThirdPartyLaboratory));

			AssertHasCustomDescription(addressTypes[DocAddressTypes.Codes.GoodsDeliveredTo], "Delivery To Party");
			AssertHasCustomDescription(addressTypes[DocAddressTypes.Codes.ImporterDocumentaryAddress], "FDA Importer");
			AssertHasCustomDescription(addressTypes[DocAddressTypes.Codes.GoodsOwner], "Owner");
			AssertHasCustomDescription(addressTypes[DocAddressTypes.Codes.Manufacturer], "Manufacturer");
			AssertHasCustomDescription(addressTypes[DocAddressTypes.Codes.Consolidator], "Consolidator");
			AssertHasCustomDescription(addressTypes[DocAddressTypes.Codes.ThirdPartyLaboratory], "Third Party Laboratory");
		}

		void AssertHasCustomDescription(ICodeDescription code, string customDescription)
		{
			AssertEquals($"{code} custom description", customDescription, code.Description);
		}

		public void TestAddressTypeList_ProgramCode()
		{
			TestCase("BIO", new string[] { DocAddressTypes.Codes.Manufacturer, DocAddressTypes.Codes.Shipper, DocAddressTypes.Codes.ImporterDocumentaryAddress, DocAddressTypes.Codes.GoodsDeliveredTo });
			TestCase("COS", new string[] { DocAddressTypes.Codes.Manufacturer, DocAddressTypes.Codes.Shipper, DocAddressTypes.Codes.ImporterDocumentaryAddress, DocAddressTypes.Codes.GoodsDeliveredTo });
			TestCase("RAD", new string[] { DocAddressTypes.Codes.Manufacturer, DocAddressTypes.Codes.Shipper, DocAddressTypes.Codes.ImporterDocumentaryAddress, DocAddressTypes.Codes.GoodsDeliveredTo });
			TestCase("VME", new string[] { DocAddressTypes.Codes.Manufacturer, DocAddressTypes.Codes.Shipper, DocAddressTypes.Codes.ImporterDocumentaryAddress, DocAddressTypes.Codes.GoodsDeliveredTo });
			TestCase("DEV", new string[] { DocAddressTypes.Codes.Manufacturer, DocAddressTypes.Codes.Shipper, DocAddressTypes.Codes.ImporterDocumentaryAddress, DocAddressTypes.Codes.GoodsDeliveredTo, DocAddressTypes.Codes.InitialImporter });
			TestCase("DRU", new string[] { DocAddressTypes.Codes.Manufacturer, DocAddressTypes.Codes.Shipper, DocAddressTypes.Codes.ImporterDocumentaryAddress, DocAddressTypes.Codes.GoodsDeliveredTo, DocAddressTypes.Codes.Sponsor });
			TestCase("FOO", new string[] { DocAddressTypes.Codes.Manufacturer, DocAddressTypes.Codes.Shipper, DocAddressTypes.Codes.ImporterDocumentaryAddress, DocAddressTypes.Codes.GoodsDeliveredTo, DocAddressTypes.Codes.GoodsOwner, DocAddressTypes.Codes.GoodsLocation, DocAddressTypes.Codes.FSVPImporter, DocAddressTypes.Codes.Consolidator, DocAddressTypes.Codes.Grower });
			TestCase("TOB", new string[] { DocAddressTypes.Codes.Manufacturer, DocAddressTypes.Codes.Shipper, DocAddressTypes.Codes.ImporterDocumentaryAddress, DocAddressTypes.Codes.GoodsDeliveredTo, DocAddressTypes.Codes.ThirdPartyLaboratory, DocAddressTypes.Codes.Laboratory });
			void TestCase(string programCode, string[] expectedCodes)
			{
				fda.US_ProgramCode = programCode;
				var actualCodes = lookups.AddressTypeList.Cast<ICodeDescription>().Select(x => x.Code).ToArray();
				AssertContainsExactElementsInExactOrder(expectedCodes, actualCodes);
			}
		}

		public void TestGovRegNumTypes()
		{
			docAddress.DocAddressType = DocAddressType.Manufacturer;
			var list = lookups.GovRegNumTypes;
			AssertEquals(2, list.Count);
			Assert(list.ContainsCode(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier));
			AssertEquals(lookups.FDAEstablishmentIdentifierDescription, list.GetDescriptionFromCode(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier));
			Assert(list.ContainsCode(OrgCusCode.CodeTypes.DataUniversalNumberingSystem));
			AssertEquals(lookups.DataUniversalNumberingSystemDescription, list.GetDescriptionFromCode(OrgCusCode.CodeTypes.DataUniversalNumberingSystem));

			docAddress.DocAddressType = DocAddressType.FSVPImporter;
			list = lookups.GovRegNumTypes;
			AssertEquals(1, list.Count);
			Assert(list.ContainsCode(OrgCusCode.CodeTypes.DataUniversalNumberingSystem));
			AssertEquals(lookups.DataUniversalNumberingSystemDescription, list.GetDescriptionFromCode(OrgCusCode.CodeTypes.DataUniversalNumberingSystem));
		}

		protected override void SetUp()
		{
			base.SetUp();
			fda = Factory.New<ACEFDA>();
			docAddress = fda.DocAddresses.AddNew();
			lookups = docAddress.Lookups;
		}

		ACEFDA fda;
		ACEFDAJobDocAddress docAddress;
		ACEFDAJobDocAddressLookups lookups;
	}
}
