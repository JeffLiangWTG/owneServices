using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
#if NETFRAMEWORK
using System.Net.Http;
using Enterprise.Services.ServiceHost;
using Enterprise.ZArchitecture.Web.Business;
#endif
using InvoiceHeaderRefsTypeList = Enterprise.Customs.Business.InvoiceHeaderRefsTypeList;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	partial class DeclarationDataObjectWriterTest : OrganizationAddressTestHelper
	{
#if NETFRAMEWORK // Disable this UT in .NET 8 for now, because Enterprise.ZArchitecture.Web.Business is not multi-targeted -->
		public void TestWhenOrganizationAddressCollectionIsNull()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "DUS";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BUS";
			branch.GB_WebAddress = "TEST WEB ADDRESS";
			Factory.SaveForTesting();
			Enterprise.ZArchitecture.Environment.DataRegistry.Instance.WebBranch = branch.PK.ToGuid();

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				WebAppEnvironment.Setup(controller.Config.WebConfig);
				var shipment = Factory.New<ForwardingShipment>();
				var declaratioin = Factory.New<JobDeclaration>();
				declaratioin.JE_JS = shipment.PK;
				Factory.SaveForTesting();

				var xml = @"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <ShipmentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>S00001000</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
	<MessageProfile>
		<SchemaFilter>
			<Type>Include</Type>
			<FilterCollection>
				<Filter>
					<ElementName>PackingLineCollection</ElementName>
				</Filter>
				<Filter>
					<ElementName>DateCollection</ElementName>
				</Filter>
				<Filter>
					<ElementName>NOTACollection</ElementName>
				</Filter>
				<Filter>
					<ElementName></ElementName>
				</Filter>
				<Filter>
				</Filter>
			</FilterCollection>
		</SchemaFilter>
	</MessageProfile>
  </ShipmentRequest>
</UniversalShipmentRequest>";

				request.Content = new StringContent(xml);
				controller.Request = request;
				AssertNoExceptionThrown(() =>
				{
					controller.Post().Dispose();
				});
			}
		}
#endif

		public void TestExportStandaloneCommercialInvoiceData()
		{
			var supplier = CreateOrganisation("BOB", "AB1@#");
			var importer = CreateOrganisation("JACK", "AB2@#");
			var buyer = CreateOrganisation("JOE", "AB3@#");
			var manufacturer = CreateOrganisation("JAY", "AB4@#");
			var intermediateConsignee = CreateOrganisation("MARY", "AB5@#");
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_OH_Buyer = importer.PK;
			invoice.BuyerOrgPK = buyer.PK;
			invoice.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoice.JZ_OA_FDAShipperAddress = ZGuid.Empty;
			invoice.JZ_OH_Consignee = intermediateConsignee.PK;
			invoice.JZ_StandAloneInvoiceDirection = Customs.Business.JobMessageTypeList.Codes.Import;
			invoice.JZ_OA_ConsigneeAddress = ZGuid.Empty;
			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice.JZ_InvoiceNumber = "INV234ABC";
			invoice.JZ_InvoiceAmount = 1500m;
			var charge1 = invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 200m);
			var charge2 = invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 300m);
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1010101010";
			var invoiceLine1Charge1 = invoiceLine1.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.Discount, 50m);
			var invoiceLine1Charge2 = invoiceLine1.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.Commission, 20m);
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2020202020";
			var invoiceLine2Charge1 = invoiceLine2.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.Discount, 70m);
			var container1 = SetupInvoiceHeaderRefs(invoice.InvoiceHeaderRefs.AddNew(), InvoiceHeaderRefsTypeList.Codes.CN, "CONT123ABC");
			var container2 = SetupInvoiceHeaderRefs(invoice.InvoiceHeaderRefs.AddNew(), InvoiceHeaderRefsTypeList.Codes.CN, "CONT456DEF");
			var masterBill = SetupInvoiceHeaderRefs(invoice.InvoiceHeaderRefs.AddNew(), InvoiceHeaderRefsTypeList.Codes.MB, "MB123ABC");
			var houseBill = SetupInvoiceHeaderRefs(invoice.InvoiceHeaderRefs.AddNew(), InvoiceHeaderRefsTypeList.Codes.HB, "HB123ABC");
			var subHouseBill = SetupInvoiceHeaderRefs(invoice.InvoiceHeaderRefs.AddNew(), InvoiceHeaderRefsTypeList.Codes.SH, "SH123ABC");
			var transport1 = invoice.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "USLAX";
			var transport2 = invoice.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "USLAX";
			transport2.JW_RL_NKDiscPort = "USCHI";
			var note1 = invoice.Notes.AddNew(true, "SILLY DATA 2", "GOODBYE WORLD");
			var note2 = invoice.Notes.AddNew(true, "SILLY DATA 1", "HELLO WORLD");
			var writer = new StandaloneCommercialInvoiceDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)));
			var declarationData = writer.GetDataObject(invoice);
			CombineAssertions(delegate
			{
				AssertEquals("declarationData.MessageType.Code", Customs.Business.JobMessageTypeList.Codes.Import, declarationData.MessageType.Code);
				AssertEquals("declarationData.MessageType.Description", USJobMessageTypeList.Descriptions.Import, declarationData.MessageType.Description);
				AssertEquals("declarationData.Branch.Code", GlbBranch.CurrentBranch.GB_Code, declarationData.Branch.Code);
				AssertEquals("declarationData.Branch.Name", GlbBranch.CurrentBranch.GB_BranchName, declarationData.Branch.Name);
				AssertNull("declarationData.WayBillNumber", declarationData.WayBillNumber);
				AssertNull("declarationData.WayBillNumber", declarationData.WayBillType);
			});
			AssertEquals("declarationData.TransportLegCollection.Count", 2, declarationData.TransportLegCollection.Count);
			AssertNotNull(declarationData.TransportLegCollection.FirstOrDefault(x => x.PortOfLoading.Code.GetValueOrDefault() == "AUSYD" && x.PortOfDischarge.Code.GetValueOrDefault() == "USLAX"));
			AssertNotNull(declarationData.TransportLegCollection.FirstOrDefault(x => x.PortOfLoading.Code.GetValueOrDefault() == "AUSYD" && x.PortOfDischarge.Code.GetValueOrDefault() == "USLAX"));
			AssertEquals("declarationData.ContainerCollection.Count", 2, declarationData.ContainerCollection.Count);
			AssertNotNull(declarationData.ContainerCollection.FirstOrDefault(x => x.ContainerNumber.GetValueOrDefault() == "CONT123ABC" && !x.Seal.HasValue));
			AssertNotNull(declarationData.ContainerCollection.FirstOrDefault(x => x.ContainerNumber.GetValueOrDefault() == "CONT456DEF" && !x.Seal.HasValue));
			AssertEquals("declarationData.AdditionalBillCollection.Count", 3, declarationData.AdditionalBillCollection.Count);
			AssertNotNull(declarationData.AdditionalBillCollection.FirstOrDefault(x => x.BillNumber.GetValueOrDefault() == "MB123ABC" && x.BillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.Master));
			AssertNotNull(declarationData.AdditionalBillCollection.FirstOrDefault(x => x.BillNumber.GetValueOrDefault() == "HB123ABC" && x.BillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.House));
			AssertNotNull(declarationData.AdditionalBillCollection.FirstOrDefault(x => x.BillNumber.GetValueOrDefault() == "SH123ABC" && x.BillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.SubHouse));
			AssertNull(declarationData.CommercialInfo.CommercialChargeCollection);
			AssertEquals("declarationData.CommercialInfo.CommercialInvoiceCollection.Count", 1, declarationData.CommercialInfo.CommercialInvoiceCollection.Count);
			var invoiceData = declarationData.CommercialInfo.CommercialInvoiceCollection.FirstOrDefault(x => x.InvoiceNumber.GetValueOrDefault() == "INV234ABC" && x.InvoiceAmount.GetValueOrDefault() == 1500m && x.IncoTerm.Code.GetValueOrDefault() == Core.Constants.IncoTerms.CostInsuranceAndFreight);
			AssertEquals(invoiceData.Supplier.AddressType, AddressTypes.Supplier, invoiceData.Supplier.AddressType);
			AssertEquals("invoiceData.Supplier.CompanyName", "BOB", invoiceData.Supplier.CompanyName);
			AssertEquals(invoiceData.Buyer.AddressType, nameof(DocAddressType.BuyerDocumentaryAddress), invoiceData.Buyer.AddressType);
			AssertEquals("invoiceData.Buyer.CompanyName", "JOE", invoiceData.Buyer.CompanyName);
			AssertEquals("invoiceData.OrganizationAddressCollection.Count", 3, invoiceData.OrganizationAddressCollection.Count);
			AssertNotNull(invoiceData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == AddressTypes.Importer && x.CompanyName.GetValueOrDefault() == "JACK"));
			AssertNotNull(invoiceData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.Manufacturer) && x.CompanyName.GetValueOrDefault() == "JAY"));
			AssertNotNull(invoiceData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == Constants.AddressType.FDAShipper && x.CompanyName.GetValueOrDefault() == "BOB"));
			AssertNotNull("invoiceData.NoteCollection", invoiceData.NoteCollection);
			AssertEquals("invoiceData.NoteCollection.Count", 2, invoiceData.NoteCollection.Count);
			AssertNotNull(invoiceData.NoteCollection.FirstOrDefault(x => x.IsCustomDescription.GetValueOrDefault() && x.Description.GetValueOrDefault() == "SILLY DATA 1" && x.NoteText.GetValueOrDefault() == "HELLO WORLD"));
			AssertNotNull(invoiceData.NoteCollection.FirstOrDefault(x => x.IsCustomDescription.GetValueOrDefault() && x.Description.GetValueOrDefault() == "SILLY DATA 2" && x.NoteText.GetValueOrDefault() == "GOODBYE WORLD"));
			AssertEquals("invoiceData.CommercialChargeCollection.Count", 2, invoiceData.CommercialChargeCollection.Count);
			AssertNotNull(invoiceData.CommercialChargeCollection.FirstOrDefault(x => x.ChargeType.GetCodeAsUpperCase() == Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight && x.Amount.GetValueOrDefault() == 200m));
			AssertNotNull(invoiceData.CommercialChargeCollection.FirstOrDefault(x => x.ChargeType.GetCodeAsUpperCase() == Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance && x.Amount.GetValueOrDefault() == 300m));
			AssertEquals("invoiceData.CommercialInvoiceLineCollection.Count", 2, invoiceData.CommercialInvoiceLineCollection.Count);
			var invoiceLineData1 = invoiceData.CommercialInvoiceLineCollection.FirstOrDefault(x => x.HarmonisedCode.GetValueOrDefault() == "1010101010");
			AssertEquals("invoiceLineData1.CommercialChargeCollection.Count", 2, invoiceLineData1.CommercialChargeCollection.Count);
			AssertNotNull(invoiceLineData1.CommercialChargeCollection.FirstOrDefault(x => x.ChargeType.GetCodeAsUpperCase() == Customs.Business.CustomsChargeTypeList.Codes.Discount && x.Amount.GetValueOrDefault() == 50m));
			AssertNotNull(invoiceLineData1.CommercialChargeCollection.FirstOrDefault(x => x.ChargeType.GetCodeAsUpperCase() == Customs.Business.CustomsChargeTypeList.Codes.Commission && x.Amount.GetValueOrDefault() == 20m));
			var invoiceLineData2 = invoiceData.CommercialInvoiceLineCollection.FirstOrDefault(x => x.HarmonisedCode.GetValueOrDefault() == "2020202020");
			AssertEquals("invoiceLineData2.CommercialChargeCollection.Count", 1, invoiceLineData2.CommercialChargeCollection.Count);
			AssertNotNull(invoiceLineData2.CommercialChargeCollection.FirstOrDefault(x => x.ChargeType.GetCodeAsUpperCase() == Customs.Business.CustomsChargeTypeList.Codes.Discount && x.Amount.GetValueOrDefault() == 70m));
		}

		public void TestGetGenPivotFetchHintQueryOfFDA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = US.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.DecEntryNumber = "0012414588";

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));

			var methodInfo = writer.GetType().GetMethod("GetGenPivotFetchHintQueryOfFDA", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			var result = methodInfo.Invoke(writer, new object[] { ZGuid.NewZGuid() }) as ZQuery;
			CombineAssertions("Please make sure the sql script of fetch hint on GenPiovt hint the index 'NR_RX__XX_RelationType_XX_Relation2ID' ([XX_RelationType] ASC, [XX_Relation2ID] ASC)", () =>
			{
				AssertNotNull(result);
				Assert("The sql script should contains 'XX_Relation2ID' and 'XX_RelationType'", result.FilterString.Contains(GenPivot.Schema.XX_Relation2ID));
				Assert("The sql script should contains 'XX_Relation2ID' and 'XX_RelationType'", result.FilterString.Contains(GenPivot.Schema.XX_RelationType));
			});
		}

		public void TestPopulatetAddressForEBond()
		{
			DeclarationTestHelper.SetProcessingDistrictPortCode("SQMZ");
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetBRecordOfficeCode("HZ");

			var declaration = DeclarationTestHelper.GetDeclarationForeBond(Factory.BOFactory);

			CombineAssertions(() =>
			{
				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				var declarationData = writer.GetDataObject(declaration);

				var bondContactAddress = declarationData.OrganizationAddressCollection.FirstOrDefault(c => c.AddressType.GetValueOrDefault() == Constants.AddressType.BondContact);

				AssertNotNull("Should create a bond contact address when the eBond function is enable.", bondContactAddress);
				AssertEquals("Contact", Env.CurrentUser.FullName, bondContactAddress.Contact);
				AssertEquals("Email", Env.CurrentUser.EmailAddress, bondContactAddress.Email);
				AssertEquals("Phone", Env.CurrentUser.WorkPhone, bondContactAddress.Phone);

				var secondaryNotifyParty = bondContactAddress.RegistrationNumberCollection.Find(c => c.Type.Code.GetValueOrDefault() == "SNP");

				AssertNotNull("Should create a registration number for the Secondary Notify Party.", secondaryNotifyParty);
				AssertEquals("Description of Type", "Secondary Notify Party", secondaryNotifyParty.Type.Description);
				AssertEquals("CountryOfIssue", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, secondaryNotifyParty.CountryOfIssue.Code);
				AssertEquals("Secondary Notify Party", "SQMZXJ5HZ", secondaryNotifyParty.Value);
			});
		}

		public void TestPopulateCusDisposition()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = US.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.DecEntryNumber = "0012414588";

			var dispositions1 = declaration.EntryPGACusDispositions.AddNew();
			dispositions1.CDI_StatusKey = "FDA";
			dispositions1.CDI_Status = "01";
			dispositions1.CDI_StatusDate = ZDateTime.BrettsBirthday;

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var declarationData = writer.GetDataObject(declaration);

			var dispositions = declarationData.EntryNumberCollection.Where(x => x.Type.Code.Value == "FDA").ToList();
			Assert(dispositions.Count == 1);

			Assert(dispositions[0].Type.Code.Value == "FDA");
			Assert(dispositions[0].Number.Value == "XJ5-0012414588");
			Assert(dispositions[0].EntryIsSystemGenerated.Value);
			Assert(dispositions[0].EntryStatus.Code.Value == "01");
			Assert(dispositions[0].IssueDate.Value == ZDateTime.BrettsBirthday);
		}

		public void TestExtraParentDetailsAreExportedInAdditionalBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = US.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			var mb1 = declaration.Bills.AddNew();
			mb1.CU_BillNum = "MB1";
			mb1.CU_BillType = BillTypeList.Codes.MasterBill;
			mb1.CU_NoOfPacks = 10m;
			mb1.US_UI_NKBillIssuerSCAC = "OMB1";
			var mb1hb1 = mb1.ChildBills.AddNew();
			mb1hb1.CU_BillNum = "HB1";
			mb1hb1.CU_NoOfPacks = 10m;
			mb1hb1.US_UI_NKBillIssuerSCAC = "OHB1";
			var mb1hb1sb1 = mb1hb1.ChildBills.AddNew();
			mb1hb1sb1.CU_BillNum = "SB1";
			mb1hb1sb1.CU_NoOfPacks = 10m;
			mb1hb1sb1.US_UI_NKBillIssuerSCAC = "OSB1";
			var mb2 = declaration.Bills.AddNew();
			mb2.CU_BillNum = "MB2";
			mb2.CU_BillType = BillTypeList.Codes.MasterBill;
			mb2.CU_NoOfPacks = 15m;
			mb2.US_UI_NKBillIssuerSCAC = "OMB2";
			var mb2hb1 = mb2.ChildBills.AddNew();
			mb2hb1.CU_BillNum = "HB1";
			mb2hb1.CU_NoOfPacks = 15m;
			mb2hb1.US_UI_NKBillIssuerSCAC = "OHB1";
			var mb2hb1sb1 = mb2hb1.ChildBills.AddNew();
			mb2hb1sb1.CU_BillNum = "SB1";
			mb2hb1sb1.CU_NoOfPacks = 15m;
			mb2hb1sb1.US_UI_NKBillIssuerSCAC = "OSB1";
			Factory.SaveForTesting();
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var declarationData = writer.GetDataObject(declaration);
			AssertEquals(6, declarationData.AdditionalBillCollection.Count);
			var mb1Data = declarationData.AdditionalBillCollection.First(x => x.BillNumber.Value == "MB1" && x.BillType.Code.Value == WayBillTypeList.Codes.Master && x.NoOfPacks == 10m && x.AddInfoCollection.GetZStringValue(US.Business.Bill.Schema.US_UI_NKBillIssuerSCAC.Substring(3)).Value == "OMB1");
			var mb1hb1Data = declarationData.AdditionalBillCollection.First(x => x.BillNumber.Value == "HB1" && x.BillType.Code.Value == WayBillTypeList.Codes.House && x.NoOfPacks == 10m && x.AddInfoCollection.GetZStringValue(US.Business.Bill.Schema.US_UI_NKBillIssuerSCAC.Substring(3)).Value == "OHB1" && x.ParentBillNumber.Value == "MB1" && x.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.AdditionalBill.ParentBillIssuerSCAC).Value == "OMB1");
			var mb1hb1sb1Data = declarationData.AdditionalBillCollection.First(x => x.BillNumber.Value == "SB1" && x.BillType.Code.Value == WayBillTypeList.Codes.SubHouse && x.NoOfPacks == 10m && x.AddInfoCollection.GetZStringValue(US.Business.Bill.Schema.US_UI_NKBillIssuerSCAC.Substring(3)).Value == "OSB1"
				&& x.ParentBillNumber.Value == "HB1"
				&& x.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.AdditionalBill.ParentMasterBillNumber).Value == "MB1"
				&& x.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.AdditionalBill.ParentMasterBillIssuerSCAC).Value == "OMB1"
				&& x.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.AdditionalBill.ParentBillIssuerSCAC).Value == "OHB1");
			var mb2Data = declarationData.AdditionalBillCollection.First(x => x.BillNumber.Value == "MB2" && x.BillType.Code.Value == WayBillTypeList.Codes.Master && x.NoOfPacks == 15m && x.AddInfoCollection.GetZStringValue(US.Business.Bill.Schema.US_UI_NKBillIssuerSCAC.Substring(3)).Value == "OMB2");
			var mb2hb1Data = declarationData.AdditionalBillCollection.First(x => x.BillNumber.Value == "HB1" && x.BillType.Code.Value == WayBillTypeList.Codes.House && x.NoOfPacks == 15m && x.AddInfoCollection.GetZStringValue(US.Business.Bill.Schema.US_UI_NKBillIssuerSCAC.Substring(3)).Value == "OHB1" && x.ParentBillNumber.Value == "MB2" && x.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.AdditionalBill.ParentBillIssuerSCAC).Value == "OMB2");
			var mb2hb1sb1Data = declarationData.AdditionalBillCollection.First(x => x.BillNumber.Value == "SB1" && x.BillType.Code.Value == WayBillTypeList.Codes.SubHouse && x.NoOfPacks == 15m && x.AddInfoCollection.GetZStringValue(US.Business.Bill.Schema.US_UI_NKBillIssuerSCAC.Substring(3)).Value == "OSB1"
				&& x.ParentBillNumber.Value == "HB1"
				&& x.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.AdditionalBill.ParentMasterBillNumber).Value == "MB2"
				&& x.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.AdditionalBill.ParentMasterBillIssuerSCAC).Value == "OMB2"
				&& x.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.AdditionalBill.ParentBillIssuerSCAC).Value == "OHB1");
		}

		public void TestWriteContainsDispositions()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Import;
			var dispositiondata = dec.DispositionCodes.AddNew();
			dispositiondata.B7_AddInfoData = "Code=06*DispositionDate=2010-06-04 10:20:00.000*Order=1";

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dec)));
			var declarationData = writer.GetDataObject(dec);

			var addInfoGroup = declarationData.AddInfoGroupCollection.Find(x => x.Type.Code.Value == CusAddInfoTypeAttribute.Codes.USDisposition);
			AssertNotNull(addInfoGroup);

			var addInfo = addInfoGroup.AddInfoCollection.Find(x => x.Key.Value == "Code");
			AssertNotNull(addInfo);
			AssertEquals("06", addInfo.Value);
		}

		public void TestNoOfPacksAndPackType()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Export;
			dec.JE_TotalNoOfPacks = 12;
			dec.JE_TotalNoOfPacksPackType = "AG";
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dec)));
			var declarationData = writer.GetDataObject(dec);
			AssertEquals(12, declarationData.OuterPacks);
			AssertEquals("AG", declarationData.OuterPacksPackageType.Code);
			dec.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Import;
			writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dec)));
			declarationData = writer.GetDataObject(dec);
			AssertEquals(12, declarationData.TotalNoOfPacks);
			AssertEquals("AG", declarationData.TotalNoOfPacksPackageType.Code);
		}

		public void TestDeclarationExtraOrganisation()
		{
			var org1 = CreateOrganisation("BOB", "ABC!@#1");
			var org2 = CreateOrganisation("JACK", "ABC!@#2");
			var org3 = CreateOrganisation("JANE", "ABC!@#3");
			var org4 = CreateOrganisation("PETE", "ABC!@#4");
			var org5 = CreateOrganisation("MARY", "ABC!@#5");
			var org7 = CreateOrganisation("JOE", "ABC!@#7");
			var org8 = CreateOrganisation("MARK", "ABC!@#8");
			var org11 = CreateOrganisation("CATE", "ABC!@#11");
			var org12 = CreateOrganisation("TIM", "ABC!@#12");
			var org13 = CreateOrganisation("GARY", "ABC!@#13");
			var org14 = CreateOrganisation("ILYA", "ABC!@#14");
			var org15 = CreateOrganisation("DAVID", "ABC!@#15");
			var org16 = CreateOrganisation("HUYE", "ABC!@#16");
			var org17 = CreateOrganisation("BENQ", "ABC!@#17");
			var org18 = CreateOrganisation("RICH", "ABC!@#18");
			var org19 = CreateOrganisation("INC", "ABC!@#19");
			var org20 = CreateOrganisation("MJG", "ABC!@#20");
			var org21 = CreateOrganisation("BSH", "ABC!@#21");
			var org22 = CreateOrganisation("JEG", "ABC!@#22");
			var org23 = CreateOrganisation("HXU", "ABC!@#23");
			var dec = Factory.New<JobDeclaration>();
			dec.JE_OA_InvoicerAddress = org1.MainAddress.PK;
			dec.JE_OA_ManufacturerAddress = org2.MainAddress.PK;
			dec.JE_OH_Buyer = org3.PK;
			dec.JE_OH_BuyingAgent = org4.PK;

			dec.CBPBrokerDocumentaryAddress.E2_AddressType = "CBP";
			dec.CBPBrokerDocumentaryAddress.E2_ParentID = dec.PK;
			dec.CBPBrokerDocumentaryAddress.E2_ParentTableCode = "JE";
			dec.CBPBrokerDocumentaryAddress.E2_OA_Address = org5.Addresses[0].PK;

			dec.JE_OH_Exporter = org7.PK;
			dec.JE_OH_FDASubmitter = org8.PK;
			dec.JE_OH_Consignee = org11.PK;
			dec.IOROrgPK = org12.PK;
			dec.JE_OH_NotifyParty = org13.PK;
			dec.JE_OA_SellerAddress = org14.MainAddress.PK;
			dec.JE_OH_SellingAgent = org15.PK;
			dec.JE_OA_SoldToPartyAddress = org16.MainAddress.PK;
			dec.JE_OA_ConsigneeAddress = org17.MainAddress.PK;
			dec.US_DRWTransferee = org18.PK;
			dec.JE_OA_ShipToPartyAddress = org19.MainAddress.PK;
			dec.JE_OH_ExternalBroker = org20.PK;
			dec.JE_OH_ControllingAgent = org21.PK;
			dec.JE_OH_ControllingCustomer = org22.PK;
			dec.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Export;
			dec.USD_OH_ForeignPrincipalParty = org23.PK;
			Factory.SaveForTesting();

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dec)));

			var declarationData = writer.GetDataObject(dec);
			AssertEquals(12, declarationData.OrganizationAddressCollection.Count);

			var externalBroker = declarationData.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ExternalBroker));
			AssertEquals("Should export External Broker directly in US.", org20.OH_FullName, externalBroker.CompanyName);
			var foreignPrincipalPartyInInterest = declarationData.OrganizationAddressCollection.FirstOrDefault(Constants.AddressType.ForeignPrincipalPartyInInterest);
			AssertEquals(org23.OH_FullName, foreignPrincipalPartyInInterest.CompanyName);

			if (declarationData.OrganizationAddressCollection[0].AddressType.ToString() == Enterprise.Customs.DataTransfer.Universal.Constants.AddressTypes.IntermediateConsignee)
			{
				AssertEquals(Enterprise.Customs.DataTransfer.Universal.Constants.AddressTypes.IntermediateConsignee, declarationData.OrganizationAddressCollection[0].AddressType);
				AssertEquals(org11.OH_FullName, declarationData.OrganizationAddressCollection[0].CompanyName);
				AssertEquals(Constants.AddressType.ShipToParty, declarationData.OrganizationAddressCollection[1].AddressType);
			}
			else if (declarationData.OrganizationAddressCollection[1].AddressType.ToString() == Enterprise.Customs.DataTransfer.Universal.Constants.AddressTypes.IntermediateConsignee)
			{
				AssertEquals(Enterprise.Customs.DataTransfer.Universal.Constants.AddressTypes.IntermediateConsignee, declarationData.OrganizationAddressCollection[1].AddressType);
				AssertEquals(org11.OH_FullName, declarationData.OrganizationAddressCollection[1].CompanyName);
				AssertEquals(Constants.AddressType.ShipToParty, declarationData.OrganizationAddressCollection[0].AddressType);
			}

			dec.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			Factory.SaveForTesting();

			AssertEquals(false, dec.US_EnableAII);

			declarationData = writer.GetDataObject(dec);

			void AssertAddressData(OrgHeader orgHeader, string addressType)
			{
				var addressData = declarationData.OrganizationAddressCollection.FirstOrDefault(addressType);
				AssertEquals(addressType, orgHeader.OH_FullName, addressData.CompanyName);
			}

			CombineAssertions(() =>
			{
				AssertEquals(21, declarationData.OrganizationAddressCollection.Count);

				AssertAddressData(org1, Constants.AddressType.Invoicer);
				AssertAddressData(org1, nameof(DocAddressType.InvoicerAddress));
				AssertAddressData(org2, nameof(DocAddressType.Manufacturer));
				AssertAddressData(org3, nameof(DocAddressType.BuyerDocumentaryAddress));
				AssertAddressData(org4, Customs.DataTransfer.Universal.Constants.AddressTypes.BuyingAgent);
				AssertAddressData(org5, Constants.AddressType.CBPBroker);
				AssertAddressData(org7, Constants.AddressType.Exporter);
				AssertAddressData(org8, Constants.AddressType.FDASubmitter);
				AssertAddressData(org12, Constants.AddressType.ImporterOfRecord);
				AssertAddressData(org13, nameof(DocAddressType.NotifyParty));
				AssertAddressData(org14, Constants.AddressType.Seller);
				AssertAddressData(org15, Customs.DataTransfer.Universal.Constants.AddressTypes.SellingAgent);
				AssertAddressData(org16, Constants.AddressType.SoldToParty);
				AssertAddressData(org17, nameof(DocAddressType.UltimateConsignee));
				AssertAddressData(org19, Constants.AddressType.ShipToParty);
				AssertAddressData(org20, nameof(DocAddressType.ExternalBroker));
				AssertAddressData(org21, nameof(DocAddressType.ControllingAgent));
				AssertAddressData(org22, nameof(DocAddressType.ControllingCustomer));
			});

			dec.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Drawback;
			Factory.SaveForTesting();

			AssertEquals(false, dec.IsACE);
			AssertEquals(true, dec.IsDrawback);
			declarationData = writer.GetDataObject(dec);
			AssertEquals(12, declarationData.OrganizationAddressCollection.Count);

			CombineAssertions(() =>
			{
				AssertAddressData(org18, Constants.AddressType.Transferee);
				AssertAddressData(org20, nameof(DocAddressType.ExternalBroker));
			});
		}

		public void TestWayBillIssuerSCAC()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MasterBill = "MB1";
			dec.JE_MasterBillIssuerSCAC = "OMB1";
			dec.JE_HouseBill = "HB1";
			dec.JE_HouseBillIssuerSCAC = "OHB1";

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dec)));
			var declarationData = writer.GetDataObject(dec);
			AssertEquals("OHB1", declarationData.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.Declaration.WayBillIssuerSCAC));
			dec.JE_HouseBill = ZString.Empty;
			declarationData = writer.GetDataObject(dec);
			AssertEquals("OMB1", declarationData.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.Declaration.WayBillIssuerSCAC));
			dec.JE_MasterBill = ZString.Empty;
			declarationData = writer.GetDataObject(dec);
			AssertNull(declarationData.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.Declaration.WayBillIssuerSCAC));
		}

		public void TestBondDesignationCode()
		{
			var dec = Factory.New<JobDeclaration>();
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dec)));
			var declarationData = writer.GetDataObject(dec);
			AssertEquals(BondDesignationCodeList.Codes.BasicBond, declarationData.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.Declaration.BondDesignationCode));
			dec.US_BondDesignationCode = BondDesignationCodeList.Codes.SubstitutionBond;
			declarationData = writer.GetDataObject(dec);
			AssertEquals(BondDesignationCodeList.Codes.BasicBond, declarationData.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.Declaration.BondDesignationCode));
		}

		public void TestExportingInBondData()
		{
			CombineAssertions("InBondData does exist", () =>
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Import;

				var inBondHeader = Factory.BOFactory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
				inBondHeader.BH_ParentID = dec.PK;
				inBondHeader.BH_ParentTableCode = dec.TablePrefix;

				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dec)));
				var declarationData = writer.GetDataObject(dec);

				AssertNotNull("declarationData.SubShipmentCollection", declarationData.SubShipmentCollection);
				AssertNotNull("declarationData.SubShipmentCollection should contain InBond", declarationData.SubShipmentCollection.FirstOrDefault(x => x.GetMatchingDataSource(DataContextType.InBond) != null));
			});

			CombineAssertions("InBondData doesnt exist", () =>
			{
				var shipment = Factory.New<ForwardingShipment>();

				var dec = Factory.New<JobDeclaration>();
				dec.JE_JS = shipment.PK;
				dec.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Import;

				var inBondHeader = Factory.BOFactory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
				inBondHeader.BH_ParentID = shipment.PK;
				inBondHeader.BH_ParentTableCode = shipment.TablePrefix;

				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dec)));
				var declarationData = writer.GetDataObject(dec);

				AssertNull("declarationData.SubShipmentCollection", declarationData.SubShipmentCollection);
			});
		}

		public void TestDeclarationStatementDetails()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Import;
			dec.US_EntryFilerCode = "XJ5";
			dec.ImportEntryNumber = "3423422";
			Factory.SaveForTesting();

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dec)));
			var declarationData = writer.GetDataObject(dec);
			AssertNull("No Statement Detail", declarationData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.Declaration.StatementNumber));
			AssertNull("No Statement Detail", declarationData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.Declaration.StatementStatus));
			AssertNull("No Statement Detail", declarationData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.Declaration.StatementPaidDate));
			AssertNull("No Statement Detail", declarationData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.Declaration.StatementPaymentStatus));

			var statementHeader1 = Factory.New<CusStatementHeader>();
			statementHeader1.B2_EntryFilerCode = "XJ6";
			statementHeader1.B2_StatementNumber = "STATENO1";
			statementHeader1.B2_Status = StatementHeaderStatusList.Codes.Final;
			statementHeader1.B2_PaymentAuthorizationDate = new ZDateTime(2012, 2, 12);
			statementHeader1.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			var line1 = statementHeader1.StatementLines.AddNew();
			line1.B3_EntryFilerCode = "XJ6";
			line1.B3_EntryNum = "3423422";

			var statementHeader2 = Factory.New<CusStatementHeader>();
			statementHeader2.B2_EntryFilerCode = "XJ5";
			statementHeader2.B2_StatementNumber = "STATENO2";
			statementHeader2.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			statementHeader2.B2_PaymentAuthorizationDate = new ZDateTime(2012, 3, 15);
			statementHeader2.B2_PaymentStatus = PaymentStatusList.Codes.PaymentInProgress;
			var line2 = statementHeader2.StatementLines.AddNew();
			line2.B3_EntryFilerCode = "XJ5";
			line2.B3_EntryNum = "3423422";
			Factory.SaveForTesting();

			declarationData = writer.GetDataObject(dec);
			AssertEquals("StatementNumber", "STATENO2", declarationData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.Declaration.StatementNumber).Value.GetValueOrDefault());
			AssertEquals("StatementStatus", StatementHeaderStatusList.Codes.Preliminary, declarationData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.Declaration.StatementStatus).Value.GetValueOrDefault());
			AssertEquals("StatementPaidDate", AutoUSAddInfo.GetStringRepresentation(new ZDateTime(2012, 3, 15)), declarationData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.Declaration.StatementPaidDate).Value.GetValueOrDefault());
			AssertEquals("StatementPaymentStatus", PaymentStatusList.Codes.PaymentInProgress, declarationData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.Declaration.StatementPaymentStatus).Value.GetValueOrDefault());
		}

		public void TestPolpulateITNumbers()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MasterBill = "MB1";
			dec.JE_MasterBillIssuerSCAC = "OMB1";
			dec.JE_HouseBill = "HB1";
			dec.JE_HouseBillIssuerSCAC = "OHB1";
			dec.JE_PrimaryITNumber = "12333333";
			Factory.SaveForTesting();

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dec)));
			var declarationData = writer.GetDataObject(dec);
			var billData = declarationData.AdditionalBillCollection.GetAdditionalBill("HB1", WayBillTypeList.Codes.House);
			var itNumberData = billData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == "ITN");
			AssertNotNull(itNumberData);
			var itNumberDataAddInfo = itNumberData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "ITNumber");
			AssertNotNull(itNumberDataAddInfo);
			AssertEquals("12333333", itNumberDataAddInfo.Value.GetValueOrDefault());

			dec.JE_HouseBill = ZString.Empty;
			Factory.SaveForTesting();

			writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dec)));
			declarationData = writer.GetDataObject(dec);
			billData = declarationData.AdditionalBillCollection.GetAdditionalBill(ZString.Empty, WayBillTypeList.Codes.House);
			itNumberData = billData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == "ITN");
			AssertNotNull(itNumberData);
			itNumberDataAddInfo = itNumberData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "ITNumber");
			AssertNotNull(itNumberDataAddInfo);
			AssertEquals("12333333", itNumberDataAddInfo.Value.GetValueOrDefault());
		}

		public void TestAddInfoDescAttributes()
		{
			var org = Factory.New<OrgHeader>();
			var declaration = Factory.New<DummyJobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Import;
			declaration.US_SchDExport = "Test";
			declaration.JE_AddInfo = string.Format("{0}=61*{1}=IMP234*SchDExport=Test", USAddInfoSchema.Constants.US_InbondType.Substring(3), USAddInfoSchema.Constants.US_ImportEntryNo.Substring(3));
			var indexer = (IColumnIndexer)declaration;
			var collection = Enterprise.Customs.DataTransfer.Universal.AddInfoCollectionCreator.CreateCollection(indexer, JobDeclarationSchema.JE_AddInfo);
			var matches = collection.FindAll(x => x.Key.GetValueOrDefault() == "SchDExportDescription");
			AssertEquals(1, matches.Count);
			AssertEquals("TestAddInfoDesc", matches[0].Value.GetValueOrDefault());
		}

		public void TestGetAddInfoDescAttribute()
		{
			var declaration = Factory.New<DummyJobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Import;
			declaration.US_SchDExport = "Test";
			declaration.JE_AddInfo = string.Format("{0}=61*{1}=IMP234*{2}=Test", USAddInfoSchema.Constants.US_InbondType.Substring(3), USAddInfoSchema.Constants.US_ImportEntryNo.Substring(3), USAddInfoSchema.Constants.US_SchDExport.Substring(3));
			var dictionary = AddInfoExtensions.GetAddInfoDescAttribute(declaration);
			AssertNotNull("PreConditiona: there is at least AddInfoDesc attribute", dictionary);
			AddInfoDescAttribute attribute;
			dictionary.TryGetValue(USAddInfoSchema.Constants.US_SchDExport.Substring(3), out attribute);
			AssertNotNull("PreConditiona: there is at least AddInfoDesc attribute", attribute);
			var desc = attribute.GetDescription(declaration);
			AssertEquals("TestAddInfoDesc", desc);
		}

		protected override void SetUp()
		{
			setupCreator = ((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			base.SetUp();
		}
		IDisposable setupCreator;

		protected override void TearDown()
		{
			base.TearDown();
			if (setupCreator != null)
			{
				setupCreator.Dispose();
				setupCreator = null;
			}
		}

		JobComInvoiceHeaderRefs SetupInvoiceHeaderRefs(JobComInvoiceHeaderRefs reference, ZString type, ZString number)
		{
			reference.J2_ReferenceType = type;
			reference.J2_ReferenceNumber = number;
			return reference;
		}

		OrgHeader CreateOrganisation(ZString name, ZString code)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = name;
			org.OH_Code = code;
			org.MainAddress.OA_Address1 = name + " ADDRESS 1";
			return org;
		}

		sealed class DummyJobDeclaration : JobDeclaration
		{
			public DummyJobDeclaration(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
			{
			}

			[AddInfoDesc(nameof(TestAddInfoDesc))]
			public override ZString US_SchDExport
			{
				get => base.US_SchDExport;
				set => base.US_SchDExport = value;
			}

			public ZString TestAddInfoDesc => "TestAddInfoDesc";
		}
	}
}
