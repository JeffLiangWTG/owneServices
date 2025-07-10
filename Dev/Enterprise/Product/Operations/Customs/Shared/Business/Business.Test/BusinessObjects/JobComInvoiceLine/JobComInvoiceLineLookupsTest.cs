using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	class JobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCachedListOfAdditionalCodeDescriptions()
		{
			SetupCusCodeList();

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var additionalCodesList = invoiceLine.Lookups.CachedListOfAdditionalCodeDescriptions;
			AssertContainsExactElementsInAnyOrder(new ZString[] { "add11", "add12", "add21", "add22", "add31" }, additionalCodesList.GetAllCodesZString());
			AssertEquals("add11 Descriptions", additionalCodesList.GetDescriptionFromCode("add11"));
			AssertEquals("add12 Descriptions", additionalCodesList.GetDescriptionFromCode("add12"));
			AssertEquals("add21 Descriptions", additionalCodesList.GetDescriptionFromCode("add21"));
			AssertEquals("add22 Descriptions", additionalCodesList.GetDescriptionFromCode("add22"));
			AssertEquals("add31 Descriptions", additionalCodesList.GetDescriptionFromCode("add31"));

			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_WorkingLanguage = "FR";
			Factory.Save();
			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var invoice2 = declaration.Invoices.AddNew();
				var invoiceLine2 = invoice.InvoiceLines.AddNew();
				var additionalCodesList2 = invoiceLine2.Lookups.CachedListOfAdditionalCodeDescriptions;
				AssertContainsExactElementsInAnyOrder(new ZString[] { "add11", "add12", "add21", "add22", "add31" }, additionalCodesList2.GetAllCodesZString());
				AssertEquals("add11 La description", additionalCodesList2.GetDescriptionFromCode("add11"));
				AssertEquals("add12 La description", additionalCodesList2.GetDescriptionFromCode("add12"));
				AssertEquals("add21 La description", additionalCodesList2.GetDescriptionFromCode("add21"));
				AssertEquals("add22 La description", additionalCodesList2.GetDescriptionFromCode("add22"));
				AssertEquals("add31 La description", additionalCodesList2.GetDescriptionFromCode("add31"));
			}
		}

		public void TestOrderNumbersList()
		{
			SetupTariffAndRate();

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2019, 12, 10, 0, 0, 0);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "DUMMYTRF";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Botswana;
			invoiceLine.JI_PrimaryPreference = "";

			CombineAssertions("OrderNumbersList when preference and additionalCode and rateType and rateCode is empty ", () =>
			{
				var orderNumbersList = invoiceLine.Lookups.OrderNumbersList;
				AssertEquals(5, orderNumbersList.Count);
				Assert(orderNumbersList.ContainsCode("ord11"));
				Assert(orderNumbersList.ContainsCode("ord12"));
				Assert(orderNumbersList.ContainsCode("ord21"));
				Assert(orderNumbersList.ContainsCode("ord22"));
				Assert(orderNumbersList.ContainsCode("ord31"));
			});

			invoiceLine.JI_PrimaryPreference = "STD";
			CombineAssertions("OrderNumbersList when preference='STD'", () =>
			{
				var orderNumbersList = invoiceLine.Lookups.OrderNumbersList;
				AssertEquals(2, orderNumbersList.Count);
				Assert(orderNumbersList.ContainsCode("ord11"));
				Assert(orderNumbersList.ContainsCode("ord12"));
			});
		}

		public void TestAdditionalCodesList_RateAdditionalCodesList()
		{
			SetupTariffAndRate();
			SetupCusCodeList();

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2019, 12, 10, 0, 0, 0);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "DUMMYTRF";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Botswana;
			invoiceLine.JI_PrimaryPreference = "";
			invoiceLine.JI_ConcessionOrder = "";

			CombineAssertions("AdditionalCodesList when preference and ordernumber and rateType and rateCode is empty ", () =>
			{
				var additionalCodesList = invoiceLine.Lookups.AdditionalCodesList;
				AssertContainsExactElementsInAnyOrder(new ZString[] { "add11", "add12", "add21", "add22", "add31" }, additionalCodesList.GetAllCodesZString());
				AssertEquals("add11 Descriptions", additionalCodesList.GetDescriptionFromCode("add11"));
				AssertEquals("add12 Descriptions", additionalCodesList.GetDescriptionFromCode("add12"));
				AssertEquals("add21 Descriptions", additionalCodesList.GetDescriptionFromCode("add21"));
				AssertEquals("add22 Descriptions", additionalCodesList.GetDescriptionFromCode("add22"));
				AssertEquals("add31 Descriptions", additionalCodesList.GetDescriptionFromCode("add31"));
			});

			invoiceLine.JI_PrimaryPreference = "STD";
			CombineAssertions("AdditionalCodesList when preference='STD' and ordernumber is empty", () =>
			{
				var additionalCodesList = invoiceLine.Lookups.AdditionalCodesList;
				AssertContainsExactElementsInAnyOrder(new ZString[] { "add11", "add12" }, additionalCodesList.GetAllCodesZString());
				AssertEquals("add11 Descriptions", additionalCodesList.GetDescriptionFromCode("add11"));
				AssertEquals("add12 Descriptions", additionalCodesList.GetDescriptionFromCode("add12"));
			});

			invoiceLine.JI_ConcessionOrder = "ord11";
			CombineAssertions("AdditionalCodesList when preference='STD' and ordernumber='ord11'", () =>
			{
				var additionalCodesList = invoiceLine.Lookups.AdditionalCodesList;
				AssertContainsExactElementsInAnyOrder(new ZString[] { "add11" }, additionalCodesList.GetAllCodesZString());
				AssertEquals("add11 Descriptions", additionalCodesList.GetDescriptionFromCode("add11"));
			});
		}

		public void TestAdditionalCodesList_ConditionAdditionalCodesList()
		{
			SetupTariffAndCondition();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2019, 12, 10, 0, 0, 0);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "DUMMYTRF";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Botswana;

			var additionalCodesList = invoiceLine.Lookups.AdditionalCodesList;
			AssertContainsExactElementsInAnyOrder(new ZString[] { "add11", "add14" }, additionalCodesList.GetAllCodesZString());
		}

		public void TestAdditionalCodesList_ConditionAdditionalCodesList_TariffDataGrouping()
		{
			SetupTariffAndCondition("ABC");

			var declaration = Factory.New<TariffDataGroupingDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2019, 12, 10, 0, 0, 0);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "DUMMYTRF";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Botswana;
			declaration.TariffDataGrouping = "ABC";

			var additionalCodesList = invoiceLine.Lookups.AdditionalCodesList;
			AssertContainsExactElementsInAnyOrder(new ZString[] { "add11", "add14" }, additionalCodesList.GetAllCodesZString());
		}

		public void TestAdditionalCodesList_VATAdditionalCodeList()
		{
			SetupVAT();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2019, 12, 10, 0, 0, 0);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "DUMMYTRF";
			invoiceLine.JI_ZZF_NKTaxType = "tax1";
			var additionalCodesList = invoiceLine.Lookups.AdditionalCodesList;
			AssertContainsExactElementsInAnyOrder(new ZString[] { "add91" }, additionalCodesList.GetAllCodesZString());
		}

		public void TestAdditionalCodesList_TariffAdditionalCodeList()
		{
			SetUpTariffAdditionalCodes();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2019, 12, 10, 0, 0, 0);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "DUMMYTRF";
			var additionalCodesList = invoiceLine.Lookups.AdditionalCodesList;
			AssertContainsExactElementsInAnyOrder("Line criteria returns null, so there shouldn't be any result for TariffAdditionalCodes in base Customs solution", Array.Empty<ZString>(), additionalCodesList.GetAllCodesZString());
		}

		public void TestConsignees()
		{
			AssertNotNull("Consignees", invoiceLine.Lookups.Consignees);
			AssertEquals("Consignees type", typeof(ConsigneeCollection), invoiceLine.Lookups.Consignees.GetType());
		}

		public void TestVolumeUQList()
		{
			AssertEquals("Volume list count", 11, invoiceLine.Lookups.VolumeUQList.Count);
		}

		public void TestRelatedIndicatorList()
		{
			AssertEquals("RelatedIndicatorList", Factory.GetCachedValue<RelatedIndicatorList>(), invoiceLine.Lookups.RelatedIndicatorList);
		}

		public void TestPartsList()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var org3 = Factory.New<OrgHeader>();
			var org4 = Factory.New<OrgHeader>();

			declaration.JE_OH_Supplier = org1.PK;
			declaration.JE_OH_Importer = org2.PK;

			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();

			invoiceHeader2.JZ_OH_Supplier = org3.PK;
			invoiceHeader2.JZ_OH_Buyer = org4.PK;

			invoiceLine.JI_PartNo = "AAA";
			var parts = invoiceLine.Lookups.PartsList;
			AssertEquals("OP_PartNum is set", true, parts.FilterBusinessObjectDefaults.ContainsDefaultFor("Product Code:Property"));
			AssertEquals("Supplier is set", true, parts.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer/Supplier:Property1"));
			AssertEquals("Buyer is set", true, parts.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer/Supplier:Property2"));
			AssertEquals("Correct Supplier", org2.PK, (ZGuid)parts.FilterBusinessObjectDefaults["Importer/Supplier:Property1"].Value);
			AssertEquals("Correct Importer", org1.PK, (ZGuid)parts.FilterBusinessObjectDefaults["Importer/Supplier:Property2"].Value);

			var parts2 = invoiceLine2.Lookups.PartsList;
			AssertEquals("OP_PartNum is not set", false, parts2.FilterBusinessObjectDefaults.ContainsDefaultFor("Product Code:Property"));
			AssertEquals("Supplier is set", true, parts2.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer/Supplier:Property1"));
			AssertEquals("Buyer is set", true, parts2.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer/Supplier:Property2"));
			AssertEquals("Correct Supplier", org4.PK, (ZGuid)parts2.FilterBusinessObjectDefaults["Importer/Supplier:Property1"].Value);
			AssertEquals("Correct Importer", org3.PK, (ZGuid)parts2.FilterBusinessObjectDefaults["Importer/Supplier:Property2"].Value);

			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Importer = ZGuid.Empty;
			parts2 = invoiceLine2.Lookups.PartsList;
			AssertEquals("Supplier is set", true, parts2.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer/Supplier:Property1"));
			AssertEquals("Buyer is set", true, parts2.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer/Supplier:Property2"));
			AssertEquals("Correct Supplier", org4.PK, (ZGuid)parts2.FilterBusinessObjectDefaults["Importer/Supplier:Property1"].Value);
			AssertEquals("Correct Importer", org3.PK, (ZGuid)parts2.FilterBusinessObjectDefaults["Importer/Supplier:Property2"].Value);
		}

		public void TestPartsListGetsTheRightConstructor()
		{
			var newFactory = new BusinessObjectFactory();
			var importer = newFactory.New<OrgHeader>();
			importer.FillWithValidTestData();

			var supplier = newFactory.New<OrgHeader>();
			supplier.FillWithValidTestData();

			var anotherSupplier = newFactory.New<OrgHeader>();

			anotherSupplier.FillWithValidTestData();
			var provider = invoiceLine.GetClassificationTypeProvider();

			var part1 = newFactory.New<OrgSupplierPart>();
			part1.FillWithValidTestData();
			part1.OP_PartNum = "AAA";
			var part1Relation1 = part1.RelatedOrganisations.AddNew();
			part1Relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			part1Relation1.OU_OH = importer.PK;
			var part1Relation2 = part1.RelatedOrganisations.AddNew();
			part1Relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			part1Relation2.OU_OH = supplier.PK;
			var part1Pivot = part1.PivotsForBinding.AddNew();
			part1Pivot.CI_ChildType = provider.HTECode;
			part1Pivot.CI_TariffNum = "10";

			var part2 = newFactory.New<OrgSupplierPart>();
			part2.FillWithValidTestData();
			part2.OP_PartNum = "AAA";
			var part2Relation1 = part2.RelatedOrganisations.AddNew();
			part2Relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			part2Relation1.OU_OH = importer.PK;
			var part2Relation2 = part2.RelatedOrganisations.AddNew();
			part2Relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			part2Relation2.OU_OH = anotherSupplier.PK;
			var part2Pivot = part2.PivotsForBinding.AddNew();
			part2Pivot.CI_ChildType = provider.HTECode;
			part2Pivot.CI_TariffNum = "10";

			using (DuplicateProductTriggerSuspenderForTest.Suspend())
			{
				newFactory.Save();
			}

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			invoiceHeader.JZ_OH_Buyer = importer.PK;
			invoiceLine.JI_PartNo = "AAA";

			Factory.Save();

			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var parts = invoiceLine.Lookups.PartsList;
			var additionalFilter = ((IBusinessObjectCollectionTestingMembers)parts).GetAdditionalFilter();
			var collection = new OrgSupplierPartCollection(Factory);
			collection.Load(additionalFilter);
			AssertEquals("collection.Contains(part1)", true, collection.Contains(part1));
			AssertEquals("collection.Contains(part2)", false, collection.Contains(part2));

			CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			parts = invoiceLine.Lookups.PartsList;
			additionalFilter = ((IBusinessObjectCollectionTestingMembers)parts).GetAdditionalFilter();
			collection = new OrgSupplierPartCollection(Factory);
			collection.Load(additionalFilter);
			AssertEquals("collection.Contains(part1)", true, collection.Contains(part1));
			AssertEquals("collection.Contains(part2)", true, collection.Contains(part2));

			AssertEquals("part should not have warning", false, invoiceLine.JI_PartNoInfo.HasWarnings());
			AssertEquals("parts collection should not have force search", false, parts.ForceSearchOnEnteringModule);
			invoiceLine.JI_PartNo = "INVALID";
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertEquals("part should now have warning", true, invoiceLine.JI_PartNoInfo.HasWarnings());
			parts = invoiceLine.Lookups.PartsList;
			AssertEquals("parts collection should now have force search", true, parts.ForceSearchOnEnteringModule);
		}

		public void TestPartsListAddNewSetsCorrectRealationships()
		{
			BaseCusClassification classification = Factory.New<BaseCusClassification>();
			classification.CC_Description = "LOOKUP DESCRIPTION";
			classification.CC_LookupCode = "LKCODE";
			OrgHeader supplier = Factory.New<OrgHeader>();
			OrgHeader owner = Factory.New<OrgHeader>();
			declaration.JE_OH_Supplier = supplier.PK;
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = owner.PK;
			invoiceLine.JI_PartNo = "PART";
			invoiceLine.JI_CC = classification.PK;
			invoiceLine.JI_Description = "LINE DESCRIPTION";

			string sup = OrgPartRelation.RelationshipTypes.Supplier;
			string own = OrgPartRelation.RelationshipTypes.Owner;
			string bth = OrgPartRelation.RelationshipTypes.Both;

			RunPartsListAddNewSetsCorrectRealationshipsTest(false, JobMessageTypeList.Codes.Import, supplier, owner, false, false, sup, own, true);
			RunPartsListAddNewSetsCorrectRealationshipsTest(false, JobMessageTypeList.Codes.Import, supplier, owner, false, true, sup, bth, true);
			RunPartsListAddNewSetsCorrectRealationshipsTest(false, JobMessageTypeList.Codes.Import, supplier, owner, true, false, bth, own, true);
			RunPartsListAddNewSetsCorrectRealationshipsTest(false, JobMessageTypeList.Codes.Import, supplier, owner, true, true, bth, bth, true);

			RunPartsListAddNewSetsCorrectRealationshipsTest(false, JobMessageTypeList.Codes.Export, supplier, owner, false, false, sup, own, false);
			RunPartsListAddNewSetsCorrectRealationshipsTest(false, JobMessageTypeList.Codes.Export, supplier, owner, false, true, sup, bth, false);
			RunPartsListAddNewSetsCorrectRealationshipsTest(false, JobMessageTypeList.Codes.Export, supplier, owner, true, false, bth, own, false);
			RunPartsListAddNewSetsCorrectRealationshipsTest(false, JobMessageTypeList.Codes.Export, supplier, owner, true, true, bth, bth, false);

			RunPartsListAddNewSetsCorrectRealationshipsTest(true, JobMessageTypeList.Codes.Import, supplier, owner, false, false, sup, bth, true);
			RunPartsListAddNewSetsCorrectRealationshipsTest(true, JobMessageTypeList.Codes.Import, supplier, owner, false, true, sup, bth, true);
			RunPartsListAddNewSetsCorrectRealationshipsTest(true, JobMessageTypeList.Codes.Import, supplier, owner, true, false, bth, bth, true);
			RunPartsListAddNewSetsCorrectRealationshipsTest(true, JobMessageTypeList.Codes.Import, supplier, owner, true, true, bth, bth, true);

			RunPartsListAddNewSetsCorrectRealationshipsTest(true, JobMessageTypeList.Codes.Export, supplier, owner, false, false, bth, own, false);
			RunPartsListAddNewSetsCorrectRealationshipsTest(true, JobMessageTypeList.Codes.Export, supplier, owner, false, true, bth, bth, false);
			RunPartsListAddNewSetsCorrectRealationshipsTest(true, JobMessageTypeList.Codes.Export, supplier, owner, true, false, bth, own, false);
			RunPartsListAddNewSetsCorrectRealationshipsTest(true, JobMessageTypeList.Codes.Export, supplier, owner, true, true, bth, bth, false);
		}

		public void TestContainerModeListSameAsInDeclaration()
		{
			AssertEquals("Container Mode list same as in the parent declaration", declaration.Lookups.CargoIdTypeList, invoiceLine.Lookups.ContainerModeList);
		}

		public void TestSortedInvoiceListSameAsInDeclaration()
		{
			AssertEquals("Sorted Invoice list same as in the parent declaration", declaration.Lookups.SortedInvoiceList, invoiceLine.Lookups.SortedInvoiceList);
		}

		public void TestTaxOrFeeType()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertEquals("TaxOrFeeType should be empty in base", string.Empty, invoiceLine.Lookups.TaxOrFeeType);
		}

		public void TestTaxOrFeeCodeList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var today = ZDateTime.Today;

				RefDataHelper.CreateTaxOrFee("VZR", 0, Core.Constants.CountryCodes.SouthAfrica, today.AddDays(-1), today.AddDays(1), "VAT Zero Rated");
				RefDataHelper.CreateTaxOrFee("VAT", 0.14, Core.Constants.CountryCodes.SouthAfrica, today.AddDays(-1), today.AddDays(1), "VAT Normal");
				RefDataHelper.CreateTaxOrFee("VEX", 0, Core.Constants.CountryCodes.SouthAfrica, today.AddDays(-1), today.AddDays(1), "VAT Exempt");

				RefDataHelper.CreateTaxOrFee("ZA1", 0, Core.Constants.CountryCodes.SouthAfrica, today.AddDays(-3), today.AddDays(-2), "DESC1");
				RefDataHelper.CreateTaxOrFee("ZA2", 0, Core.Constants.CountryCodes.SouthAfrica, today.AddDays(-1), today.AddDays(1), "DESC2");
				RefDataHelper.CreateTaxOrFee("ZA3", 0, Core.Constants.CountryCodes.SouthAfrica, today.AddDays(-1), today.AddDays(1), "DESC3");
				RefDataHelper.CreateTaxOrFee("ZA4", 0, Core.Constants.CountryCodes.SouthAfrica, today.AddDays(2), today.AddDays(3), "DESC4");
				RefDataHelper.CreateTaxOrFee("AU1", 0, Core.Constants.CountryCodes.Italy, today.AddDays(-1), today.AddDays(1), "AU1");
				Factory.Save();

				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				var list = invoiceLine.Lookups.TaxOrFeeCodeList;

				AssertContainsExactElementsInAnyOrder(
					new ZString[]
					{
						"ZA2 - DESC2",
						"ZA3 - DESC3",
						"VAT - VAT Normal",
						"VEX - VAT Exempt",
						"VZR - VAT Zero Rated",
					},
					list.ToArray().Select(x => $"{x.Code} - {x.Description}")
					);
			}
		}

		public void TestTaxOrFeeCodeList_CN()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var today = ZDateTime.Today;

				var tariffType = RefDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem);
				RefDataHelper.CreateTaxOrFee("CN1", 0, Core.Constants.CountryCodes.China, today.AddDays(-3), today.AddDays(-2), "DESC1");
				RefDataHelper.CreateTaxOrFee("CN2", 0, Core.Constants.CountryCodes.China, today.AddDays(-1), today.AddDays(1), "DESC2");
				RefDataHelper.CreateTaxOrFee("CN3", 0, Core.Constants.CountryCodes.China, today.AddDays(-1), today.AddDays(1), "DESC3");
				RefDataHelper.CreateTaxOrFee("CN4", 0, Core.Constants.CountryCodes.China, today.AddDays(2), today.AddDays(3), "DESC4");
				RefDataHelper.CreateTaxOrFee("CN5", 0, Core.Constants.CountryCodes.China, today.AddDays(-1), today.AddDays(1), "DESC5");
				RefDataHelper.CreateTaxOrFee("AU1", 0, Core.Constants.CountryCodes.Italy, today.AddDays(-1), today.AddDays(1), "AU1");
				Factory.Save();

				var tariff = RefDataHelper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today, "Alpha Bravo", compositeKey: "99...99.99", taxOrFeeCode: "CN1");
				RefDataHelper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.China, "CN1", startDate: today.AddDays(-3), endDate: today.AddDays(-2));
				RefDataHelper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.China, "CN2", startDate: today.AddDays(-1), endDate: today.AddDays(1));
				RefDataHelper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.China, "CN3", startDate: today.AddDays(2), endDate: today.AddDays(3));
				RefDataHelper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.China, "CN4", startDate: today.AddDays(-1), endDate: today.AddDays(1));
				RefDataHelper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.China, "CN5", startDate: today.AddDays(2), endDate: today.AddDays(3));

				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "99999999";
				var list = invoiceLine.Lookups.TaxOrFeeCodeList;

				AssertContainsExactElementsInAnyOrder(
					new ZString[]
					{
						"CN2 - DESC2"
					},
					list.ToArray().Select(x => $"{x.Code} - {x.Description}")
				);
			}
		}

		public void TestTaxOrFeeCodeList_US()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var today = ZDateTime.Today;

				var s1p1TariffType = RefDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "1P1");
				RefDataHelper.CreateTaxOrFee("US1", 0, Core.Constants.CountryCodes.UnitedStates, today.AddDays(-3), today.AddDays(-2), "DESC1");
				RefDataHelper.CreateTaxOrFee("US2", 0, Core.Constants.CountryCodes.UnitedStates, today.AddDays(-1), today.AddDays(1), "DESC2");
				RefDataHelper.CreateTaxOrFee("US3", 0, Core.Constants.CountryCodes.UnitedStates, today.AddDays(-1), today.AddDays(1), "DESC3");
				RefDataHelper.CreateTaxOrFee("US4", 0, Core.Constants.CountryCodes.UnitedStates, today.AddDays(2), today.AddDays(3), "DESC4");
				RefDataHelper.CreateTaxOrFee("US5", 0, Core.Constants.CountryCodes.UnitedStates, today.AddDays(-1), today.AddDays(1), "DESC5");
				RefDataHelper.CreateTaxOrFee("AU1", 0, Core.Constants.CountryCodes.Italy, today.AddDays(-1), today.AddDays(1), "AU1");
				Factory.Save();

				var tariff = RefDataHelper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, s1p1TariffType.PK, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today, "Alpha Bravo", compositeKey: "99...99.99", taxOrFeeCode: "CN1");
				RefDataHelper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.UnitedStates, "US1", startDate: today.AddDays(-3), endDate: today.AddDays(-2));
				RefDataHelper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.UnitedStates, "US2", startDate: today.AddDays(-1), endDate: today.AddDays(1));
				RefDataHelper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.UnitedStates, "US3", startDate: today.AddDays(2), endDate: today.AddDays(3));
				RefDataHelper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.UnitedStates, "US4", startDate: today.AddDays(-1), endDate: today.AddDays(1));
				RefDataHelper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.UnitedStates, "US5", startDate: today.AddDays(2), endDate: today.AddDays(3));

				var dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var invoice = dec.Invoices.AddNew();
				var invoiceline = invoice.InvoiceLines.AddNew();
				var list = invoiceline.Lookups.TaxOrFeeCodeList;

				AssertEquals(0, list.Count);
			}
		}

		public void TestPrimaryPreferenceList_UseUniversalTariff()
		{
			SetupTariffAndRate();

			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Botswana;
			AssertContainsExactElementsInAnyOrder("UseUniversalTariff but tariff is empty", new string[] { "STD", "RED", "MFN" }, (invoiceLine.Lookups.PrimaryPreferenceList as CodeDescriptionPairList).GetAllCodes());

			invoiceLine.JI_Tariff = "DUMMYTRF";
			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			AssertContainsExactElementsInAnyOrder("UseUniversalTariff but CountryOfOrigin  is empty", new string[] { "STD", "RED", "MFN" }, (invoiceLine.Lookups.PrimaryPreferenceList as CodeDescriptionPairList).GetAllCodes());

			invoiceLine.JI_Tariff = "DUMMYTRF";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Botswana;
			AssertContainsExactElementsInAnyOrder("match country using univiersal tariff", new string[] { "STD", "RED" }, (invoiceLine.Lookups.PrimaryPreferenceList as CodeDescriptionPairList).GetAllCodes());

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.AlandIslands;
			AssertContainsExactElementsInAnyOrder("no match country using univiersal tariff", Array.Empty<string>(), (invoiceLine.Lookups.PrimaryPreferenceList as CodeDescriptionPairList).GetAllCodes());

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Botswana;
			invoiceLine.JI_ConcessionOrder = "ord12";
			AssertContainsExactElementsInAnyOrder("match ordernumber using univiersal tariff", new string[] { "STD" }, (invoiceLine.Lookups.PrimaryPreferenceList as CodeDescriptionPairList).GetAllCodes());
			invoiceLine.JI_ConcessionOrder = "ord52";
			AssertContainsExactElementsInAnyOrder("no match ordernumber using univiersal tariff", Array.Empty<string>(), (invoiceLine.Lookups.PrimaryPreferenceList as CodeDescriptionPairList).GetAllCodes());

			invoiceLine.JI_ConcessionOrder = "";
			invoiceLine.JI_PrimaryPreference = "RED";
			AssertContainsExactElementsInAnyOrder("get preference lookup list regardless selected preference value", new string[] { "STD", "RED" }, (invoiceLine.Lookups.PrimaryPreferenceList as CodeDescriptionPairList).GetAllCodes());
		}

		public void TestPrimaryPreferenceList_UseUniversalTariff_TariffDataGroup()
		{
			RefDataHelper.CreateNewOrGetExistingDataGrouping("ABC");
			SetupTariffAndRate("ABC");

			var declaration = Factory.New<TariffDataGroupingDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Botswana;
			AssertContainsExactElementsInAnyOrder("UseUniversalTariff but tariff is empty", Array.Empty<string>(), (invoiceLine.Lookups.PrimaryPreferenceList as CodeDescriptionPairList).GetAllCodes());
			declaration.TariffDataGrouping = "ABC";
			AssertContainsExactElementsInAnyOrder("UseUniversalTariff but tariff is empty", new string[] { "STD", "RED", "MFN" }, (invoiceLine.Lookups.PrimaryPreferenceList as CodeDescriptionPairList).GetAllCodes());
		}

		public void TestPrimaryPreferenceList_NotUseUniversalTariff()
		{
			var eunId = RefDataHelper.CreateNewOrGetExistingDataGrouping("EUN");
			RefDataHelper.CreateNewOrGetExistingDataGrouping(Env.CurrentCompany.Country.Code, parent: eunId);
			var wcoId = RefDataHelper.CreateNewOrGetExistingDataGrouping("WCO");
			RefDataHelper.CreateNewOrGetExistingDataGrouping("JP", parent: wcoId);

			var cusPref1 = RefDataHelper.CreatePreferenceForCountry("140", "Exemption for End-Use Resulting from the CCT", "EUN");
			var cusPref2 = RefDataHelper.CreatePreferenceForCountry("200", "GSP Rate Without Conditions Or Limits (Including Ceilings)", "EUN");
			var cusPref3 = RefDataHelper.CreatePreferenceForCountry("123", "123Description", "WCO");

			Factory.Save();

			var invoiceLine = Factory.New<NonUniversalTariffJobComInvoiceLine_ForTest>();
			AssertNull("Expecting invoice country code to be null to force using ENV.Country", invoiceLine?.InvoiceHeader?.CountryCode);

			var preferencesList = invoiceLine.Lookups.PrimaryPreferenceList;

			AssertNotNull(preferencesList);
			AssertCollectionContains(cusPref1, preferencesList);
			AssertCollectionContains(cusPref2, preferencesList);
			AssertCollectionNotContains(cusPref3, preferencesList);

			var company = Factory.New<GlbCompany>();
			company.SetCountry("JP");
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			var declaration = (BaseJobDeclaration)Factory.New(ObjectFactory.GetType<Integration.Customs.JP.IJobDeclaration>());
			declaration.JE_GB = branch.PK;
			var header = declaration.Invoices.AddNew();
			invoiceLine.JI_JZ = header.PK;

			AssertEquals("JP", invoiceLine?.InvoiceHeader?.CountryCode);

			var preferencesListJP = invoiceLine.Lookups.PrimaryPreferenceList;

			AssertNotNull(preferencesListJP);
			AssertCollectionContains(cusPref3, preferencesListJP);
			AssertCollectionNotContains(cusPref1, preferencesListJP);
			AssertCollectionNotContains(cusPref2, preferencesListJP);
		}

		public void TestPrimaryPreferenceList_NotUseUniversalTariff_TariffDataGrouping()
		{
			var eunId = RefDataHelper.CreateNewOrGetExistingDataGrouping("EUN");
			RefDataHelper.CreateNewOrGetExistingDataGrouping(Env.CurrentCompany.Country.Code, parent: eunId);
			RefDataHelper.CreateNewOrGetExistingDataGrouping("ABC");

			var cusPref1 = RefDataHelper.CreatePreferenceForCountry("123", "Tariff DataGrouping Test", "ABC");
			var cusPref2 = RefDataHelper.CreatePreferenceForCountry("456", "Country pref", "EUN");

			Factory.Save();

			var declaration = Factory.New<TariffDataGroupingDeclaration>();
			var header = declaration.Invoices.AddNew();
			invoiceLine.JI_JZ = header.PK;

			var preferencesList = invoiceLine.Lookups.PrimaryPreferenceList;

			AssertNotNull(preferencesList);
			AssertCollectionNotContains(cusPref1, preferencesList);
			AssertCollectionContains(cusPref2, preferencesList);

			declaration.TariffDataGrouping = "ABC";

			preferencesList = invoiceLine.Lookups.PrimaryPreferenceList;

			AssertNotNull(preferencesList);
			AssertCollectionContains(cusPref1, preferencesList);
			AssertCollectionNotContains(cusPref2, preferencesList);
		}

		public void TestSerialList()
		{
			AssertEquals(0, invoiceLine.Lookups.SerialList.Count);
		}

		public void TestGoodsCatalogList()
		{
			Factory.NewWithValidTestData<BaseCusGoodsCatalog>();
			Factory.NewWithValidTestData<BaseCusGoodsCatalog>().CGC_GC_Company = Factory.New<GlbCompany>().PK;
			var goodsCatalogList = invoiceLine.Lookups.GoodsCatalogList;
			goodsCatalogList.Load();

			AssertEquals(1, goodsCatalogList.Count);
			AssertEquals(GlbCompany.CurrentCompany.PK, goodsCatalogList[0].CGC_GC_Company);
		}

		public void TestCustomsEntryInstructions()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "AAA";

			var cei2 = declaration.CustomsEntryInstructions.AddNew();
			cei2.CEI_Style = "BBB";

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var styles = invoiceLine.Lookups.CustomsEntryInstructions.Select(c => c.CEI_Style);
			AssertContainsExactElementsInAnyOrder(new [] { "AAA", "BBB" }, styles);
		}

		BaseJobDeclaration declaration;
		BaseJobComInvoiceHeader invoiceHeader;
		BaseJobComInvoiceLine invoiceLine;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = BaseJobDeclaration.New(Factory);
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = declaration.FilteredInvoiceLines.AddNew();
		}

		UniversalReferenceTestDataHelper RefDataHelper => refDataHelper ?? (refDataHelper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper refDataHelper;

		void RunPartsListAddNewSetsCorrectRealationshipsTest(bool regSetting, string messagetype, OrgHeader supplier, OrgHeader owner, bool supplierBoth, bool ownerBoth, string expectedSupplierRelationship,
			string expectedOwnerRelationship, bool expectRelatedOrgToBeImport)
		{
			CustomsDataRegistry.Instance.AlwaysAssumeLocalOrganisationIsBothImporterAndExporter.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, regSetting);
			declaration.JE_MessageType = messagetype;

			supplier.CountryData.OV_MakePartsBothImportAndExport = supplierBoth;
			owner.CountryData.OV_MakePartsBothImportAndExport = ownerBoth;

			OrgSupplierPart newPart = (OrgSupplierPart)invoiceLine.Lookups.PartsList.AddNew();
			AssertEquals("DESCRIPTION", "LINE DESCRIPTION", newPart.OP_Desc);
			AssertEquals("1 OrgPart Realtionsips", 1, newPart.RelatedOrganisations.Count);

			OrgPartRelation relatedOrg = newPart.RelatedOrganisations[0];
			if (expectRelatedOrgToBeImport)
			{
				AssertEquals("Owner PK", owner.PK, relatedOrg.OU_OH);
				AssertEquals("Owner RelationshipPK", expectedOwnerRelationship, relatedOrg.OU_Relationship);
			}
			else
			{
				AssertEquals("Supplier PK", supplier.PK, relatedOrg.OU_OH);
				AssertEquals("Supplier RelationshipPK", expectedSupplierRelationship, relatedOrg.OU_Relationship);
			}
		}

		void SetupTariffAndRate(string overrideDataGrouping = "")
		{
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);
			var dataGrouping = string.IsNullOrEmpty(overrideDataGrouping) ? GlbCompany.CurrentCompany.Country.Code : new ZString(overrideDataGrouping);

			var tradeGroupStandard = RefDataHelper.CreateTradeGroup(dataGrouping, "STANDARD", date1, date4);
			RefDataHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Botswana, date1, date4);
			Factory.Save();

			var hsnTariffType = RefDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, "HSN");
			Factory.Save();
			var dutyRateType = RefDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);
			var addRateType = RefDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.AntiDumping, "ADD");
			var rateCode2 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, "AD1", addRateType.PK);
			Factory.Save();
			var preferenceSTD = RefDataHelper.CreatePreferenceForCountry("STD", "Standard", dataGrouping);
			var preferenceRED = RefDataHelper.CreatePreferenceForCountry("RED", "Reduced", dataGrouping);
			RefDataHelper.CreatePreferenceForCountry("MFN", "Most-favored Nation Duty", dataGrouping);
			Factory.Save();

			var cusTariff = RefDataHelper.CreateTariff(dataGrouping, hsnTariffType.PK, "DUMMYTRF", date1, date4, "dummy Description 0");
			Factory.Save();

			var testRate1 = RefDataHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceSTD.PK);
			RefDataHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, "add11", "ord11");
			RefDataHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, "add12", "ord12");

			var testRate2 = RefDataHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceRED.PK);
			RefDataHelper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date4, "add21", "ord21");
			RefDataHelper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date4, "add22", "ord22");

			var testRate3 = RefDataHelper.CreateRate(cusTariff, rateCode2.PK, date1, date4, "0", preferencePk: preferenceRED.PK);
			RefDataHelper.CreateCusApplicability(testRate3, tradeGroupStandard, date1, date4, "add31", "ord31");
			Factory.Save();
		}

		void SetupTariffAndCondition(string overrideDataGrouping = "")
		{
			var date1 = new ZDate(2010, 12, 10);
			var date2 = new ZDate(2079, 06, 06);
			var dataGrouping = string.IsNullOrEmpty(overrideDataGrouping) ? GlbCompany.CurrentCompany.Country.Code : new ZString(overrideDataGrouping);

			var tradeGroupStandard = RefDataHelper.CreateTradeGroup(dataGrouping, "STANDARD", date1, date2);
			RefDataHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Botswana, date1, date2);
			var tradeGroupAU = RefDataHelper.CreateTradeGroup("AU", "STANDARD", date1, date2);
			RefDataHelper.AddCountry(tradeGroupAU, Core.Constants.CountryCodes.Australia, date1, date2);
			Factory.Save();

			var hsnTariffType = RefDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, "HSN");
			Factory.Save();
			var cusTariff = RefDataHelper.CreateTariff(dataGrouping, hsnTariffType.PK, "DUMMYTRF", date1, date2, "dummy Description 0");
			Factory.Save();

			var conditionType = RefDataHelper.CreateOrGetExistingRefCusConditionType(dataGrouping, "CTRL", "TY1");
			var conditionCode1 = RefDataHelper.CreateOrGetExistingRefCusCondition(dataGrouping, conditionType.PK, cusTariff.PK, "", true, false, date1, date2);
			var conditionCode2 = RefDataHelper.CreateOrGetExistingRefCusCondition(dataGrouping, conditionType.PK, cusTariff.PK, "", false, true, date1, date2);
			Factory.Save();

			RefDataHelper.CreateCusApplicability(conditionCode1, tradeGroupStandard, date1, date2);
			RefDataHelper.CreateCusApplicability(conditionCode1, tradeGroupStandard, date1, date2, "add11");
			RefDataHelper.CreateCusApplicability(conditionCode1, tradeGroupStandard, date2.AddDays(-1), date2, "add12");
			RefDataHelper.CreateCusApplicability(conditionCode1, tradeGroupAU, date1, date2, "add13");
			RefDataHelper.CreateCusApplicability(conditionCode1, tradeGroupStandard, date1, date2, "add14");

			RefDataHelper.CreateCusApplicability(conditionCode2, tradeGroupStandard, date1, date2, "add21");

			Factory.Save();
		}

		void SetupVAT()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date2 = new ZDate(2079, 06, 06);
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;

			var hsnTariffType = RefDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, "HSN");
			Factory.Save();

			var tradeGroupStandard = RefDataHelper.CreateTradeGroup(dataGrouping, "STANDARD", date1, date2, "STANDARD DEC");
			//Helper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Australia, startDate, endDate2);
			Factory.Save();

			var cusTariff = RefDataHelper.CreateTariff(dataGrouping, hsnTariffType.PK, "DUMMYTRF", date1, date2, "dummy Description 0");
			RefDataHelper.CreateVATApplicabilityView(cusTariff, dataGrouping, "tax1", date1, date2, additionalCode: "add91", tradeGroup: tradeGroupStandard.PK);

			Factory.Save();
		}

		void SetUpTariffAdditionalCodes()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date2 = new ZDate(2079, 06, 06);
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			var tradeGroup = RefDataHelper.CreateTradeGroup(dataGrouping, "STANDARD", date1, date2, "STANDARD DEC");
			var hsnTariffType = RefDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, "HSN");
			Factory.Save();

			var cusTariff = RefDataHelper.CreateTariff(dataGrouping, hsnTariffType.PK, "DUMMYTRF", date1, date2, "dummy Description 0");
			var additionalCode1 = RefDataHelper.CreateTariffAdditionalCodeView(cusTariff, "EXP", "add98");
			RefDataHelper.CreateCusApplicability(additionalCode1, tradeGroup, date1, date2);
			var additionalCode2 = RefDataHelper.CreateTariffAdditionalCodeView(cusTariff, "IMP", "add99");
			RefDataHelper.CreateCusApplicability(additionalCode2, tradeGroup, date1, date2);

			Factory.Save();
		}

		void SetupCusCodeList()
		{
			const string additionalCodes = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes;
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var datetime1 = ZDateTime.Today.AddDays(-1);
			var datetime2 = ZDateTime.Today.AddDays(1);
			RefDataHelper.CreateCusCodeType(additionalCodes, "Additional Codes", currentCountry);
			RefDataHelper.CreateCusCodeType(additionalCodes, "Additional Codes", Core.Constants.CountryCodes.Italy);
			RefDataHelper.CreateCusCodeType(additionalCodes, "Additional Codes", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			RefDataHelper.CreateOrGetLanguage(Core.Constants.CountryCodes.France, "French");
			RefDataHelper.CreateOrGetLanguage(Core.Constants.CountryCodes.Italy, "Italian");
			Factory.Save();

			var addcdCodeList1 = RefDataHelper.CreateCusCodeList(currentCountry, additionalCodes, "add11", "add11 Descriptions", datetime1, datetime2);
			RefDataHelper.CreateCusCodeListLanguage(addcdCodeList1, Core.Constants.CountryCodes.France, "add11 La description");
			var addcdCodeList2 = RefDataHelper.CreateCusCodeList(currentCountry, additionalCodes, "add12", "add12 Descriptions", datetime1, datetime2);
			RefDataHelper.CreateCusCodeListLanguage(addcdCodeList2, Core.Constants.CountryCodes.France, "add12 La description");
			var addcdCodeList3 = RefDataHelper.CreateCusCodeList(currentCountry, additionalCodes, "add21", "add21 Descriptions", datetime1, datetime2);
			RefDataHelper.CreateCusCodeListLanguage(addcdCodeList3, Core.Constants.CountryCodes.France, "add21 La description");
			var addcdCodeList4 = RefDataHelper.CreateCusCodeList(currentCountry, additionalCodes, "add22", "add22 Descriptions", datetime1, datetime2);
			RefDataHelper.CreateCusCodeListLanguage(addcdCodeList4, Core.Constants.CountryCodes.France, "add22 La description");
			var addcdCodeList5 = RefDataHelper.CreateCusCodeList(currentCountry, additionalCodes, "add31", "add31 Descriptions", datetime1, datetime2);
			RefDataHelper.CreateCusCodeListLanguage(addcdCodeList5, Core.Constants.CountryCodes.France, "add31 La description");
			var addcdCodeList6 = RefDataHelper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, "ADDCD", "add31", "add31 Descriptions", datetime1, datetime2);
			RefDataHelper.CreateCusCodeListLanguage(addcdCodeList6, Core.Constants.CountryCodes.Italy, "add31 Descrizione");
			var addcdCodeList7 = RefDataHelper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "ADDCD", "add11", "add11 Descriptions", datetime1, datetime2);
			RefDataHelper.CreateCusCodeListLanguage(addcdCodeList7, Core.Constants.CountryCodes.Italy, "add11 Descrizione");
			Factory.Save();
		}
	}

	class NonUniversalTariffJobComInvoiceLine_ForTest : BaseJobComInvoiceLine
	{
		public NonUniversalTariffJobComInvoiceLine_ForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
		}

		protected internal override bool UseUniversalTariffCore => false;
	}
}
