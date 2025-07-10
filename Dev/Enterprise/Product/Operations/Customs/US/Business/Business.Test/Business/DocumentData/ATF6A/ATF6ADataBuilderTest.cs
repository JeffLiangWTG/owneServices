using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.US.Business.Testing
{
	class ATF6ADataBuilderTest : TestCaseWithFactory
	{
		public void TestForDeclaration()
		{
			CombineAssertions(() =>
			{
				var parameters = new Mock<IDocDataObjectParameters>();
				parameters.Setup(x => x.Data).Returns(new ZString[] { "P1", "P2", "P3", "P4", "P5" });
				var builder = new ATF6ADataBuilder(declaration, parameters.Object);
				var wrapper = builder.Build();
				AssretWrapper(wrapper, declaration.JE_TotalNoOfPacks.ToString(), declaration.Lookups.JE_TotalNoOfPacksPackType_List.GetDescriptionFromCode(declaration.JE_TotalNoOfPacksPackType));
				var atfGroups = wrapper.ATFGroups.ToArray();
				AssertEquals(5, atfGroups.Length);

				var group1 = atfGroups[0];
				AssertATFGroup(group1, "P1", "Seller Org", "Seller Address 1, Seller Address 2, Seller City, Seller State, China", "Exporter Org", "Exporter Address 1, Exporter Address 2, Exporter City, Exporter State, Australia", "FFL: F1   AECA: A1", "FFL: 03-01-2025   AECA: 03-02-2025", true, true, false);
				var atfDetails1 = group1.ATFDetails.ToArray();
				AssertEquals(4, atfDetails1.Length);
				var atfDetail11 = atfDetails1[0];
				AssertATFDetails(atfDetail11, "P1", "INV01", 1, "Manufacturer Org 1", "Product 1", "C1", "CA1", "1", "11.1", "111.1", MunitionsCategoryList.Codes.I, "M1");
				var atfDetail12 = atfDetails1[1];
				AssertATFDetails(atfDetail12, "P1", "INV01", 2, "Manufacturer Org 1", "Product 1", "C2", "CA2", "2", "22.2", "222.2", MunitionsCategoryList.Codes.II, "M2");
				var atfdetail13 = atfDetails1[2];
				AssertATFDetails(atfdetail13, "P1", "INV01", 3, "Manufacturer Org 2", "Product 3", "", "", "", "", "", MunitionsCategoryList.Codes.VIII, "");
				var atfDetail14 = atfDetails1[3];
				AssertATFDetails(atfDetail14, "P1", "INV02", 1, "", "", "", "", "", "", "", MunitionsCategoryList.Codes.XIV, "");

				var group2 = atfGroups[1];
				AssertATFGroup(group2, "P2", "Seller Org", "Seller Address 1, Seller Address 2, Seller City, Seller State, China", "Exporter Org", "Exporter Address 1, Exporter Address 2, Exporter City, Exporter State, Australia", "FFL: F3", "FFL: 03-05-2025", false, true, false);
				var atfDetails2 = group2.ATFDetails.ToArray();
				AssertEquals(3, atfDetails2.Length);
				var atfDetail21 = atfDetails2[0];
				AssertATFDetails(atfDetail21, "P2", "INV01", 1, "", "Product 2", "", "", "", "", "", MunitionsCategoryList.Codes.VII, "");
				var atfDetail22 = atfDetails2[1];
				AssertATFDetails(atfDetail22, "P2", "INV01", 2, "Manufacturer Org 2", "Product 3", "", "", "", "", "", MunitionsCategoryList.Codes.XIV, "");
				var atfDetail23 = atfDetails2[2];
				AssertATFDetails(atfDetail23, "P2", "INV02", 1, "", "", "", "", "", "", "", MunitionsCategoryList.Codes.XIV, "");

				var group3 = atfGroups[2];
				AssertATFGroup(group3, "P3", "", "", "", "", "AECA: A4", "AECA: 03-06-2025", false, false, true);
				var atfDetails3 = group3.ATFDetails.ToArray();
				AssertEquals(1, atfDetails3.Length);
				var atfDetail31 = atfDetails3[0];
				AssertATFDetails(atfDetail31, "P3", "INV02", 1, "", "", "", "", "", "", "", MunitionsCategoryList.Codes.III, "");

				var group4 = atfGroups[3];
				AssertATFGroup(group4, "P4", "", "", "", "", "FFL: F5", "AECA: 03-07-2025", false, true, false);
				var atfDetails4 = group4.ATFDetails.ToArray();
				AssertEquals(1, atfDetails4.Length);
				var atfDetail41 = atfDetails4[0];
				AssertATFDetails(atfDetail41, "P4", "INV02", 1, "", "", "", "", "", "", "", MunitionsCategoryList.Codes.VI, "");

				var group5 = atfGroups[4];
				AssertATFGroup(group5, "P5", "", "", "", "", "AECA: A6", "FFL: 03-08-2025", false, true, false);
				var atfDetails5 = group5.ATFDetails.ToArray();
				AssertEquals(1, atfDetails5.Length);
				var atfDetail51 = atfDetails5[0];
				AssertATFDetails(atfDetail51, "P5", "INV02", 1, "", "", "", "", "", "", "", MunitionsCategoryList.Codes.XX, "");
			});

			CombineAssertions(() =>
			{
				var parameters = new Mock<IDocDataObjectParameters>();
				parameters.Setup(x => x.Data).Returns(new ZString[] { "P1", "P5" });
				var builder = new ATF6ADataBuilder(declaration, parameters.Object);
				var wrapper = builder.Build();
				var atfGroups = wrapper.ATFGroups.ToArray();
				AssertEquals(2, atfGroups.Length);

				parameters.Setup(x => x.Data).Returns(new ZString[] { "P1", "P3", "P5" });
				builder = new ATF6ADataBuilder(declaration, parameters.Object);
				wrapper = builder.Build();
				atfGroups = wrapper.ATFGroups.ToArray();
				AssertEquals(3, atfGroups.Length);

				declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
				builder = new ATF6ADataBuilder(declaration, parameters.Object);
				wrapper = builder.Build();
				Assert("IsWarehouse", wrapper.IsWarehouse);

				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
				builder = new ATF6ADataBuilder(declaration, parameters.Object);
				wrapper = builder.Build();
				Assert("IsConsumption", wrapper.IsConsumption);
			});
		}

		public void TestForShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TotalPackageCount = 125;
			shipment.JS_F3_NKTotalCountPackType = "BG";
			declaration.JE_JS = shipment.PK;

			CombineAssertions(() =>
			{
				var parameters = new Mock<IDocDataObjectParameters>();
				parameters.Setup(x => x.Data).Returns(new ZString[] { "P1", "P2", "P3", "P4", "P5" });
				var builder = new ATF6ADataBuilder();
				var wrapper = builder.Build(shipment.DeclarationForDocuments, parameters.Object) as ATF6ADocDataObject;
				AssretWrapper(wrapper, shipment.JS_TotalPackageCount.ToString(), shipment.Lookups.JS_PackType_List.GetDescriptionFromCode(shipment.JS_F3_NKTotalCountPackType));
				var atfGroups = wrapper.ATFGroups.ToArray();
				AssertEquals(5, atfGroups.Length);

				var group1 = atfGroups[0];
				AssertATFGroup(group1, "P1", "Seller Org", "Seller Address 1, Seller Address 2, Seller City, Seller State, China", "Exporter Org", "Exporter Address 1, Exporter Address 2, Exporter City, Exporter State, Australia", "FFL: F1   AECA: A1", "FFL: 03-01-2025   AECA: 03-02-2025", true, true, false);
				var atfDetails1 = group1.ATFDetails.ToArray();
				AssertEquals(4, atfDetails1.Length);
				var atfDetail11 = atfDetails1[0];
				AssertATFDetails(atfDetail11, "P1", "INV01", 1, "Manufacturer Org 1", "Product 1", "C1", "CA1", "1", "11.1", "111.1", MunitionsCategoryList.Codes.I, "M1");
				var atfDetail12 = atfDetails1[1];
				AssertATFDetails(atfDetail12, "P1", "INV01", 2, "Manufacturer Org 1", "Product 1", "C2", "CA2", "2", "22.2", "222.2", MunitionsCategoryList.Codes.II, "M2");
				var atfdetail13 = atfDetails1[2];
				AssertATFDetails(atfdetail13, "P1", "INV01", 3, "Manufacturer Org 2", "Product 3", "", "", "", "", "", MunitionsCategoryList.Codes.VIII, "");
				var atfDetail14 = atfDetails1[3];
				AssertATFDetails(atfDetail14, "P1", "INV02", 1, "", "", "", "", "", "", "", MunitionsCategoryList.Codes.XIV, "");

				var group2 = atfGroups[1];
				AssertATFGroup(group2, "P2", "Seller Org", "Seller Address 1, Seller Address 2, Seller City, Seller State, China", "Exporter Org", "Exporter Address 1, Exporter Address 2, Exporter City, Exporter State, Australia", "FFL: F3", "FFL: 03-05-2025", false, true, false);
				var atfDetails2 = group2.ATFDetails.ToArray();
				AssertEquals(3, atfDetails2.Length);
				var atfDetail21 = atfDetails2[0];
				AssertATFDetails(atfDetail21, "P2", "INV01", 1, "", "Product 2", "", "", "", "", "", MunitionsCategoryList.Codes.VII, "");
				var atfDetail22 = atfDetails2[1];
				AssertATFDetails(atfDetail22, "P2", "INV01", 2, "Manufacturer Org 2", "Product 3", "", "", "", "", "", MunitionsCategoryList.Codes.XIV, "");
				var atfDetail23 = atfDetails2[2];
				AssertATFDetails(atfDetail23, "P2", "INV02", 1, "", "", "", "", "", "", "", MunitionsCategoryList.Codes.XIV, "");

				var group3 = atfGroups[2];
				AssertATFGroup(group3, "P3", "", "", "", "", "AECA: A4", "AECA: 03-06-2025", false, false, true);
				var atfDetails3 = group3.ATFDetails.ToArray();
				AssertEquals(1, atfDetails3.Length);
				var atfDetail31 = atfDetails3[0];
				AssertATFDetails(atfDetail31, "P3", "INV02", 1, "", "", "", "", "", "", "", MunitionsCategoryList.Codes.III, "");

				var group4 = atfGroups[3];
				AssertATFGroup(group4, "P4", "", "", "", "", "FFL: F5", "AECA: 03-07-2025", false, true, false);
				var atfDetails4 = group4.ATFDetails.ToArray();
				AssertEquals(1, atfDetails4.Length);
				var atfDetail41 = atfDetails4[0];
				AssertATFDetails(atfDetail41, "P4", "INV02", 1, "", "", "", "", "", "", "", MunitionsCategoryList.Codes.VI, "");

				var group5 = atfGroups[4];
				AssertATFGroup(group5, "P5", "", "", "", "", "AECA: A6", "FFL: 03-08-2025", false, true, false);
				var atfDetails5 = group5.ATFDetails.ToArray();
				AssertEquals(1, atfDetails5.Length);
				var atfDetail51 = atfDetails5[0];
				AssertATFDetails(atfDetail51, "P5", "INV02", 1, "", "", "", "", "", "", "", MunitionsCategoryList.Codes.XX, "");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2407", "2407 Desc", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);

			var ior = Factory.NewWithValidTestData<OrgHeader>();
			ior.OH_FullName = "Importer of Record";
			var iorAddress = ior.MainAddress;
			iorAddress.OA_Address1 = "IOR Address 1";
			iorAddress.OA_Address2 = "IOR Address 2";
			iorAddress.OA_RL_NKRelatedPortCode = "USNYC";
			iorAddress.OA_City = "IOR City";
			iorAddress.OA_State = "IOR State";
			iorAddress.OA_PostCode = "IOR123";
			iorAddress.OA_RN_NKCountryCode = "US";
			iorAddress.OA_Fax = "123";
			iorAddress.OA_Phone = "234";
			iorAddress.OA_Email = "345";

			var seller = Factory.NewWithValidTestData<OrgHeader>();
			seller.OH_FullName = "Seller Org";
			var sellerAddress = seller.MainAddress;
			sellerAddress.OA_Address1 = "Seller Address 1";
			sellerAddress.OA_Address2 = "Seller Address 2";
			sellerAddress.OA_RL_NKRelatedPortCode = "CNSHA";
			sellerAddress.OA_City = "Seller City";
			sellerAddress.OA_State = "Seller State";

			var exporter = Factory.NewWithValidTestData<OrgHeader>();
			exporter.OH_FullName = "Exporter Org";
			var exporterAddress = exporter.MainAddress;
			exporterAddress.OA_Address1 = "Exporter Address 1";
			exporterAddress.OA_Address2 = "Exporter Address 2";
			exporterAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			exporterAddress.OA_City = "Exporter City";
			exporterAddress.OA_State = "Exporter State";

			var manufacturer1 = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer1.OH_FullName = "Manufacturer Org 1";
			var manufacturer2 = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer2.OH_FullName = "Manufacturer Org 2";
			var manufacturer3 = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer3.OH_FullName = "Manufacturer Org 3";
			Factory.Save();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OA_DeclarantAddress = iorAddress.PK;
			declaration.US_SchDEntry = "2407";
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2025, 3, 15);
			declaration.JE_OwnerRef = "456";
			declaration.US_EntryFilerCode = "SV9";
			declaration.ImportEntryNumber = "567";
			declaration.JE_TotalNoOfPacks = 67;
			declaration.JE_TotalNoOfPacksPackType = "AE";
			declaration.JE_DeclarationReference = "B00001000";

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV01";
			var invoiceline11 = invoice1.InvoiceLines.AddNew();
			invoiceline11.JI_PartNo = "Product 1";
			invoiceline11.JI_OA_Seller = sellerAddress.PK;
			invoiceline11.JI_OA_ExporterAddress = exporterAddress.PK;
			invoiceline11.JI_OA_ManufacturerAddress = manufacturer1.MainAddress.PK;
			invoiceline11.JI_LinePrice = 11.11f;
			invoiceline11.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceline11.US_ATFInd = OGAIndicatorList.Codes.Declared;
			CreateATF(invoiceline11, "P1", "C1", "CA1", 1f, 11.1f, 111.1f, MunitionsCategoryList.Codes.I, "M1", "F1", "A1", new ZDateTime(2025, 3, 1), new ZDateTime(2025, 3, 2));
			CreateATF(invoiceline11, "P1", "C2", "CA2", 2f, 22.2f, 222.2f, MunitionsCategoryList.Codes.II, "M2", "F2", "A2", new ZDateTime(2025, 3, 3), new ZDateTime(2025, 3, 4));

			var invoiceline12 = invoice1.InvoiceLines.AddNew();
			invoiceline12.JI_PartNo = "Product 2";
			invoiceline12.JI_LinePrice = 22.22f;
			invoiceline12.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceline12.US_ATFInd = OGAIndicatorList.Codes.Declared;
			CreateATF(invoiceline12, "P2", "", "", 0, 0, 0, MunitionsCategoryList.Codes.VII, "", "F3", "", new ZDateTime(2025, 3, 5), null);

			var invoiceline13 = invoice1.InvoiceLines.AddNew();
			invoiceline13.JI_PartNo = "Product 3";
			invoiceline13.JI_OA_Seller = sellerAddress.PK;
			invoiceline13.JI_OA_ExporterAddress = exporterAddress.PK;
			invoiceline13.JI_OA_ManufacturerAddress = manufacturer2.MainAddress.PK;
			invoiceline13.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;
			invoiceline13.JI_LinePrice = 33.33f;
			invoiceline12.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceline13.US_ATFInd = OGAIndicatorList.Codes.Declared;
			CreateATF(invoiceline13, "P1", "", "", 0, 0, 0, MunitionsCategoryList.Codes.VIII, "", "", "", null, null);
			CreateATF(invoiceline13, "P2", "", "", 0, 0, 0, MunitionsCategoryList.Codes.XIV, "", "", "", null, null);

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV02";
			var invoiceline21 = invoice2.InvoiceLines.AddNew();
			invoiceline21.JI_LinePrice = 44.44f;
			invoiceline21.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceline21.US_ATFInd = OGAIndicatorList.Codes.Declared;
			CreateATF(invoiceline21, "P1", "", "", 0, 0, 0, MunitionsCategoryList.Codes.XIV, "", "", "", null, null);
			CreateATF(invoiceline21, "P2", "", "", 0, 0, 0, MunitionsCategoryList.Codes.XIV, "", "", "", null, null);
			CreateATF(invoiceline21, "P3", "", "", 0, 0, 0, MunitionsCategoryList.Codes.III, "", "", "A4", null, new ZDateTime(2025, 3, 6));
			CreateATF(invoiceline21, "P4", "", "", 0, 0, 0, MunitionsCategoryList.Codes.VI, "", "F5", "", null, new ZDateTime(2025, 3, 7));
			CreateATF(invoiceline21, "P5", "", "", 0, 0, 0, MunitionsCategoryList.Codes.XX, "", "", "A6", new ZDateTime(2025, 3, 8), null);
		}
		JobDeclaration declaration;

		void CreateATF(JobComInvoiceLine line, ZString permitNumber, ZString categoryCode, ZString caliberGaugeSize, ZDecimal quantity, ZDecimal barrelLength, ZDecimal overallLength, ZString munitionsListCategory, ZString model, ZString fflNumber, ZString aecaNumber, ZDateTime? fflExpirationDate, ZDateTime? aecaExpirationDate)
		{
			var atf = line.ATFLines.AddNew();
			atf.US_PermitNumber = permitNumber;
			atf.US_CategoryCode = categoryCode;
			atf.US_CaliberGaugeSize = caliberGaugeSize;
			atf.US_Quantity = quantity;
			atf.US_BarrelLength = barrelLength;
			atf.US_OverallLength = overallLength;
			atf.US_MunitionsListCategory = munitionsListCategory;
			atf.US_Model = model;
			atf.US_FFLNumber = fflNumber;
			atf.US_AECANumber = aecaNumber;
			if (fflExpirationDate.HasValue)
			{
				atf.US_FFLExpirationDate = fflExpirationDate.Value;
			}
			if(aecaExpirationDate.HasValue)
			{
				atf.US_AECAExpirationDate = aecaExpirationDate.Value;
			}
		}

		void AssretWrapper(ATF6ADocDataObject wrapper, ZString totalNoOfPacks, ZString totalNoOfPacksPackageTypeDescription)
		{
			var ior = wrapper.ImporterOfRecord;
			AssertNotNull(ior);
			AssertEquals("Importer of Record", ior.CompanyName);
			AssertEquals("IOR Address 1", ior.AddressLine1);
			AssertEquals("IOR Address 2", ior.AddressLine2);
			AssertEquals("IOR City", ior.City);
			AssertEquals("IOR State", ior.State);
			AssertEquals("IOR123", ior.Postcode);
			AssertEquals("US", ior.Country.Code);
			AssertEquals("USNYC", ior.Unloco.Code);
			AssertEquals("123", ior.Fax);
			AssertEquals("234", ior.Phone);
			AssertEquals("345", ior.Email);

			AssertEquals("456", wrapper.OwnerRef);
			AssertEquals(totalNoOfPacks, wrapper.TotalNoOfPacks);
			AssertEquals(totalNoOfPacksPackageTypeDescription, wrapper.TotalNoOfPacksPackageTypeDescription);
			AssertEquals("SV9 - 567", wrapper.EntryNumber);
			AssertEquals("B00001000", wrapper.DeclarationNumber);
			AssertEquals("US, CN, AU", wrapper.CountryManufactured);
			AssertEquals("77.77", wrapper.EntryValue);
			AssertEquals("2407 - 2407 Desc", wrapper.PortOfEntryAndDesc);
			Assert(!wrapper.IsWarehouse);
			Assert(wrapper.IsInformal);
			Assert(!wrapper.IsConsumption);
			AssertEquals("15-Mar-25", wrapper.EntryReleaseDate);
		}

		void AssertATFGroup(ATFGroupDocDataObject group, ZString permitNumber, ZString sellerName, ZString sellerDetails, ZString exporterName, ZString exporterDetails, ZString fflNoAndAECANo, ZString fflAndAECAExpirationDates, ZBool isFirearms, ZBool isImplementsOfWar, ZBool isAmmunition)
		{
			AssertEquals($"{permitNumber} PermitNumber", permitNumber, group.PermitNumber);
			AssertEquals($"{permitNumber} SellerName", sellerName, group.SellerName);
			AssertEquals($"{permitNumber} SellerDetails", sellerDetails, group.SellerDetails);
			AssertEquals($"{permitNumber} ForeignExporterName", exporterName, group.ForeignExporterName);
			AssertEquals($"{permitNumber} ForeignExporterDetails", exporterDetails, group.ForeignExporterDetails);
			AssertEquals($"{permitNumber} FFLNoAndAECANo", fflNoAndAECANo, group.FFLNoAndAECANo);
			AssertEquals($"{permitNumber} FFLAndAECAExpirationDates", fflAndAECAExpirationDates, group.FFLAndAECAExpirationDates);
			AssertEquals($"{permitNumber} IsFirearms", isFirearms, group.IsFirearms);
			AssertEquals($"{permitNumber} IsImplementsOfWar", isImplementsOfWar, group.IsImplementsOfWar);
			AssertEquals($"{permitNumber} IsAmmunition", isAmmunition, group.IsAmmunition);
			Assert($"{permitNumber} IsCargoAsDescribed", !group.IsCargoAsDescribed);
			Assert($"{permitNumber} IsContainedOthers", !group.IsContainedOthers);
			AssertEquals($"{permitNumber} Discrepancies", ZString.Empty, group.Discrepancies);
		}

		void AssertATFDetails(ATFDetailDocDataObject details, ZString permitNumber, ZString invoiceNumber, ZInt lineNumber, ZString manufacturerName, ZString productCode, ZString categoryCode, ZString caliberGaugeSize, ZString quantity, ZString barrelLength, ZString overallLength, ZString munitionsListCategory, ZString model)
		{
			AssertEquals($"{permitNumber}-Invoice Number: {invoiceNumber} - Line Number: {lineNumber} InvoiceNumber", invoiceNumber, details.InvoiceNumber);
			AssertEquals($"{permitNumber}-Invoice Number: {invoiceNumber} - Line Number: {lineNumber} LineNumber", lineNumber, details.LineNumber);
			AssertEquals($"{permitNumber}-Invoice Number: {invoiceNumber} - Line Number: {lineNumber} ManufacturerName", manufacturerName, details.ManufacturerName);
			AssertEquals($"{permitNumber}-Invoice Number: {invoiceNumber} - Line Number: {lineNumber} ProductCode", productCode, details.ProductCode);
			AssertEquals($"{permitNumber}-Invoice Number: {invoiceNumber} - Line Number: {lineNumber} CategoryCode", categoryCode, details.CategoryCode);
			AssertEquals($"{permitNumber}-Invoice Number: {invoiceNumber} - Line Number: {lineNumber} CaliberGaugeSize", caliberGaugeSize, details.CaliberGaugeSize);
			AssertEquals($"{permitNumber}-Invoice Number: {invoiceNumber} - Line Number: {lineNumber} Quantity", quantity, details.Quantity);
			AssertEquals($"{permitNumber}-Invoice Number: {invoiceNumber} - Line Number: {lineNumber} BarrelLength", barrelLength, details.BarrelLength);
			AssertEquals($"{permitNumber}-Invoice Number: {invoiceNumber} - Line Number: {lineNumber} OverallLength", overallLength, details.OverallLength);
			AssertEquals($"{permitNumber}-Invoice Number: {invoiceNumber} - Line Number: {lineNumber} MunitionsListCategory", munitionsListCategory, details.MunitionsListCategory);
			AssertEquals($"{permitNumber}-Invoice Number: {invoiceNumber} - Line Number: {lineNumber} Model", model, details.Model);
			AssertEquals($"{permitNumber}-Invoice Number: {invoiceNumber} - Line Number: {lineNumber} SerialNumber", ZString.Empty, details.SerialNumber);
		}
	}
}
