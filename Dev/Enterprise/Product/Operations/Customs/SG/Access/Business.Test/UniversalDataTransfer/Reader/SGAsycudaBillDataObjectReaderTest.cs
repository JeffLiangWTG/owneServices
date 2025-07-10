using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer.Testing
{
	sealed class SGAsycudaBillDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportOrganizations()
		{
			var readerHelper = new SGAsycudaManifestDataObjectReaderHelper(Factory.BOFactory);
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			header.AMA_ManifestType = Constants.ManifestType.Import;
			var helper = new AsycudaManifestDataObjectReaderTestHelper();
			var fjAirLocalPort1 = helper.GetAirLocalPort1(Core.Constants.CountryCodes.Singapore);
			var fjAirLocalPort2 = helper.GetAirLocalPort2(fjAirLocalPort1.PK);
			var portOfLoading = new UNLOCO() { Code = fjAirLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = fjAirLocalPort2.RL_Code };
			var bill = helper.SetupBill("BIL00001", portOfLoading, portOfDischarge, 300m, "Goods Desc", 3m, "Carrier Reference", "STD", "PRE");
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "CCC";
			orgHeader.OH_FullName = "Consignee";
			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "SGUEN00001", Core.Constants.CountryCodes.Singapore);
			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.PartyStatusType, "A", Core.Constants.CountryCodes.Singapore);
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Consignee Address1";
			orgAddress.OA_City = "SYD";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_PostCode = "1000";
			orgAddress.OA_Phone = "12345678";
			orgAddress.OA_RN_NKCountryCode = "AU";

			var consigneeAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress),
				CompanyName = "Consignee",
				Address1 = "Consignee Address1",
				City = "SYD",
				State = "NSW",
				Postcode = "1000",
				Phone = "12345678",
				Country = new Country() { Code = "AU" }
			};

			bill.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			bill.OrganizationAddressCollection.AddSafe(consigneeAddress);
			Factory.SaveForTesting();

			var billBO = new SGAsycudaBillDataObjectReader(bill, Logger, Factory, header, readerHelper, false).ReadIntoBusinessObject() as AsycudaBill;
			AssertNotNull(billBO);
			AssertEquals("billBO.ABL_OA_Consignee", orgAddress.PK, billBO.ABL_OA_Consignee);
			AssertEquals("billBO.ABL_ConsigneeName", orgAddress.Header.OH_FullName, billBO.ABL_ConsigneeName);
			AssertEquals("billBO.ABL_ConsigneeStreet1", orgAddress.OA_Address1, billBO.ABL_ConsigneeStreet1);
			AssertEquals("billBO.ABL_ConsigneeStreet2", orgAddress.OA_Address2, billBO.ABL_ConsigneeStreet2);
			AssertEquals("billBO.ABL_ConsigneeCity", orgAddress.OA_City, billBO.ABL_ConsigneeCity);
			AssertEquals("billBO.ABL_ConsigneeState", orgAddress.OA_State, billBO.ABL_ConsigneeState);
			AssertEquals("billBO.ABL_ConsigneePostcode", orgAddress.OA_PostCode, billBO.ABL_ConsigneePostcode);
			AssertEquals("billBO.ABL_ConsigneePhone", orgAddress.PhoneNumber.FormattedForBinding, billBO.ABL_ConsigneePhone);
			AssertEquals("billBO.ABL_RN_NKConsigneeCountry", orgAddress.OA_RN_NKCountryCode, billBO.ABL_RN_NKConsigneeCountry);
			AssertEquals("SGUEN00001", billBO.SG_PartyID);
			AssertEquals("PRE", billBO.ABL_PrepaidCollect);

			var sgEntryInstruction = new EntryInstruction()
			{
				Link = 1,
				AddInfoCollection = new List<AddInfo>(new[] { new AddInfo() { Key = Constants.EventContext.PartyIndicator, Value = "1" } })
			};

			bill.SetEntryHeaderCollection(() => new List<EntryHeader>(new[]
			{
				new EntryHeader()
				{
					Type = ListHelper.GetWithDescription<EntryType>(Core.Constants.CountryCodes.SouthAfrica, new CodeDescriptionPairList()),
					EntryInstructionLink = 1
				},
				new EntryHeader()
				{
					Type = ListHelper.GetWithDescription<EntryType>(Core.Constants.CountryCodes.Singapore, new CodeDescriptionPairList()),
					EntryInstructionLink = 2
				}
			}));

			bill.SetEntryInstructionCollection(() => new List<EntryInstruction>(new[]
			{
				new EntryInstruction()
				{
					Link = 1,
					AddInfoCollection = new List<AddInfo>(new[] { new AddInfo() { Key = Constants.EventContext.PartyIndicator, Value = "2" } })
				},
				new EntryInstruction()
				{
					Link = 2,
					AddInfoCollection = new List<AddInfo>(new[]
					{
						new AddInfo() { Key = Constants.EventContext.PartyIndicator, Value = "1" },
					})
				}
			}));

			bill.AddInfoCollection.Add(new AddInfo { Key = AddInfoConstants.BillCountry.GSTNReferenceNo, Value = "A1B2C3" });

			bill.SetPackingLineCollection(() =>
			{
				var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
				var packingLineItem = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
				var addInfoGroup = new AddInfoGroup(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Type = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = ASYCUDA.Business.AddInfoConstants.Pack.PackAddInfoType, Description = ASYCUDA.Business.AddInfoConstants.Pack.PackAddInfoTypeDescription },
				};
				var addInfo = new AddInfo() { Key = AddInfoConstants.PackedItem.GSTPaymentIndicator, Value = "Y" };
				var addInfo1 = new AddInfo() { Key = ASYCUDA.Business.AddInfoConstants.PackedItem.Country, Value = "SG" };

				addInfoGroup.SetAddInfoCollection(() => new List<AddInfo>() { addInfo, addInfo1 });
				packingLineItem.SetAddInfoGroupCollection(() => new List<AddInfoGroup>() { addInfoGroup });
				packingLine.SetPackingLineCollection(() => new List<PackingLine>() { packingLineItem });
				return new DataObjectList<PackingLine>() { packingLine };
			});

			Factory.SaveForTesting();

			billBO = new SGAsycudaBillDataObjectReader(bill, Logger, Factory, header, readerHelper, false).ReadIntoBusinessObject() as AsycudaBill;
			AssertEquals("1", billBO.SG_PartyID);
			AssertEquals("A1B2C3", billBO.GSTNReferenceNo);
			var pack = billBO.Packs[0];
			var packLine = (AsycudaPackedItem)pack.PackedItems[0].PackedItem;
			AssertEquals("Y", packLine.GSTPaid);
		}
	}
}
