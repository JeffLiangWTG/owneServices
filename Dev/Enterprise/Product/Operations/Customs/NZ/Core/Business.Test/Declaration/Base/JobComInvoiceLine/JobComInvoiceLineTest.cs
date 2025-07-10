using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators;
using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators.Testing;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.Customs.NZ.Business.TariffValidation;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using DefaultOptions = Enterprise.Core.Constants.Customs.ASNRefreshDefaultsOptions;
using ECB = Enterprise.Customs.Business;
using OrgSupplierPart = Enterprise.Customs.NZ.Business.MasterFiles.OrgSupplierPart;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceLine))]
	class JobComInvoiceLineTest : BaseJobComInvoiceLineAbstractTest
	{
		public void TestFetchForValidateCore()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var tariffType = helper.CreateNewOrGetExistingTariffType("NZ", "HSN");
				var rateType = helper.CreateNewOrGetExistingRateType("NZ", "DTY");
				var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", rateType.PK);
				var tariff = helper.CreateTariff("NZ", tariffType.PK, "123456789", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Tariff Desc1");
				helper.CreateTariff("NZ", tariffType.PK, "987654321", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Tariff Desc2");
				var rate = helper.CreateRate(tariff, rateCode.PK, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
				var tradeGroup1 = helper.LoadOrCreateTradeGroup("NZ", "AU", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Australia");
				var tradeGroup2 = helper.LoadOrCreateTradeGroup("NZ", "US", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "United States");
				var applicability = helper.CreateCusApplicability(rate, tradeGroup1, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "", "100001A");
				helper.CreateCusApplicability(rate, tradeGroup2, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "", "200002A");

				InvoiceLine.JI_Tariff = "123456789";
				InvoiceLine.JI_PartsOfClassification = "987654321";
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var declarationInNewFactory = newFactory.Load<JobDeclaration>(Declaration.PK);
				var lineInNewFactory = declarationInNewFactory.InvoiceLines[0];

				((IBusiness)lineInNewFactory).RunPreSaveValidationFetch(true);
				CombineAssertions("FetchForValidate Hints should be added for Invoice Line", () =>
				{
					AssertEquals("Tariff table hit count", 1, newFactory.GetTableHitCount(TariffViewSchema.Constants.TableName));
					AssertEquals("Rate table hit count", 1, newFactory.GetTableHitCount(RateViewSchema.Constants.TableName));
					AssertEquals("Applicability table hit count", 1, newFactory.GetTableHitCount(CusRefApplicabilityViewSchema.Constants.TableName));
				});

				CombineAssertions("Should not have extra db hits", () =>
				{
					newFactory.Load<TariffView>(tariff.PK);
					newFactory.Load<RateView>(rate.PK);
					newFactory.Load<CusRefApplicabilityView>(applicability.PK);
					AssertEquals("Tariff table hit count", 1, newFactory.GetTableHitCount(TariffViewSchema.Constants.TableName));
					AssertEquals("Rate table hit count", 1, newFactory.GetTableHitCount(RateViewSchema.Constants.TableName));
					AssertEquals("Applicability table hit count", 1, newFactory.GetTableHitCount(CusRefApplicabilityViewSchema.Constants.TableName));
				});
			}
		}

		public void TestJI_PartsOfClassification()
		{
			AssertEquals("Caption", "\"Parts Of\" Classification", DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_PartsOfClassification)).Caption);

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				InvoiceLine.JI_PartsOfClassification = "1234.56.78.9";
				AssertEquals("1234.56.78.9", InvoiceLine.JI_PartsOfClassification);
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				InvoiceLine.JI_PartsOfClassification = "987654321";
				AssertEquals("987654321", InvoiceLine.JI_PartsOfClassification);
			}
		}

		public void TestJI_ConcessionCode()
		{
			AssertEquals("Caption", "Concession Code", DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_ConcessionCode)).Caption);
		}

		public void TestJI_Tariff()
		{
			AssertEquals("Caption", "Tariff Code", DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_Tariff)).Caption);

			var tariffQuery = new ZQuery(JobComInvoiceLineSchema.JI_Tariff, "1234.56.78.9");
			AssertNull("Precondition - no existing line with this tariff", Factory.LoadTop1<JobComInvoiceLine>(tariffQuery));

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				InvoiceLine.JI_Tariff = "1234.56.78.9";
				AssertEquals("1234.56.78.9", InvoiceLine.JI_Tariff);
				Factory.Save();
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();
				var line = newFactory.LoadTop1<JobComInvoiceLine>(tariffQuery);
				AssertNotNull("line was stored with a dotted tariff", line);
				AssertEquals("Dots are removed in getter", "123456789", line.JI_Tariff);

				line.JI_Tariff = "987654321";
				line.JI_Tariff = "123456789";  // force write back of new value
				newFactory.Save();

				var newFactory2 = new BusinessObjectFactory();
				line = newFactory2.LoadTop1<JobComInvoiceLine>(tariffQuery);
				AssertNotNull("line was stored with a dotted tariff", line);
				AssertEquals("Dots are removed in getter", "123456789", line.JI_Tariff);
			}
		}

		public void TestIInvoiceLinePartDetailsMembers()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
				AssertEquals(Core.Constants.CountryCodes.NewZealand, partDetails.CustomsCountryCode);
				AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
			}
		}

		public void TestIsTabaccoOrAlcoholic()
		{
			InvoiceLine.JI_Tariff = "2402";
			Assert(InvoiceLine.IsTabaccoOrAlcoholic);
			InvoiceLine.JI_Tariff = "2403";
			Assert(InvoiceLine.IsTabaccoOrAlcoholic);
			InvoiceLine.JI_Tariff = "2203";
			Assert(InvoiceLine.IsTabaccoOrAlcoholic);
			InvoiceLine.JI_Tariff = "2204";
			Assert(InvoiceLine.IsTabaccoOrAlcoholic);
			InvoiceLine.JI_Tariff = "2205";
			Assert(InvoiceLine.IsTabaccoOrAlcoholic);
			InvoiceLine.JI_Tariff = "2206";
			Assert(InvoiceLine.IsTabaccoOrAlcoholic);
			InvoiceLine.JI_Tariff = "2207";
			Assert(!InvoiceLine.IsTabaccoOrAlcoholic);
			InvoiceLine.JI_Tariff = "2208";
			Assert(InvoiceLine.IsTabaccoOrAlcoholic);
		}

		public void TestIsDutyOnlyGST()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_DateOfArrival = new ZDateTime(2019, 08, 01);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			InvoiceHeader.JZ_IsGSTPrePaid = YesNoList.Codes.Yes;

			Assert("InvoiceLine.IsDutyOnlyGST", InvoiceLine.IsDutyOnlyGST);

			InvoiceHeader.JZ_IsGSTPrePaid = YesNoList.Codes.No;
			Assert("InvoiceLine.IsDutyOnlyGST", !InvoiceLine.IsDutyOnlyGST);

			InvoiceHeader.JZ_IsGSTPrePaid = YesNoList.Codes.Yes;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			Assert("InvoiceLine.IsDutyOnlyGST", !InvoiceLine.IsDutyOnlyGST);
		}

		public void TestASNRefresh()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			var collection = new ASNRefreshOptionsConfigCollection(fallbackLevel, Factory);
			var config1 = collection.AddNew();
			config1.FieldType = DefaultOptions.Codes.Classification;
			var config2 = collection.AddNew();
			config2.FieldType = DefaultOptions.Codes.CountryOfOrigin;

			CustomsDataRegistry.Instance.ASNRefreshOptions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			var invoice = Factory.New<JobComInvoiceHeader>();
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPTEST";
			invoice.JZ_OH_Buyer = importer.PK;
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUPTEST";
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_StandAloneInvoiceDirection = Customs.Business.JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.MoreCodes.AdvanceShippingNotice;

			var line = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var classification = Factory.New<CusClassification>();
			classification.FillWithValidTestData();

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "AMYTEST";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_CC = classification.PK;
			pivot.CI_TariffNum = "1010101010";
			pivot.CI_RN_NKCountryOfOrigin = "CA";
			var relatedOrg1 = product.RelatedOrganisations.AddNew();
			relatedOrg1.OU_OH = importer.PK;
			relatedOrg1.OU_Relationship = "OWN";
			var relatedOrg2 = product.RelatedOrganisations.AddNew();
			relatedOrg2.OU_OH = supplier.PK;
			relatedOrg2.OU_Relationship = "SUP";

			line.JI_PartNo = product.OP_PartNum;
			line.JI_OP = product.PK;
			AssertEquals(pivot, line.Pivot);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			invoice.JZ_JE = declaration.PK;
			AssertEquals("Refreshed by the registry", pivot.CI_CC, line.JI_CC);
			AssertEquals("Refreshed by the registry", pivot.CI_RN_NKCountryOfOrigin, line.JI_CountryOfOrigin);
			AssertEquals("Refreshed by the registry", ZString.Empty, line.JI_Tariff);

			var companyData = importer.CompanyData;
			companyData.ImporterOverride = true;
			companyData.OB_IMProductValueDefaultOptions = "OVR,TAR,PREFF";

			invoice.JZ_JE = ZGuid.Empty;
			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_StandAloneInvoiceDirection = Customs.Business.JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			line.JI_CC = ZGuid.Empty;
			line.JI_CountryOfOrigin = ZString.Empty;
			invoice.JZ_JE = declaration.PK;
			AssertEquals("Refreshed by the consignee config", ZGuid.Empty, line.JI_CC);
			AssertEquals("Refreshed by the consignee config", ZString.Empty, line.JI_CountryOfOrigin);
			AssertEquals("Refreshed by the consignee config", pivot.CI_TariffNum, line.JI_Tariff);
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			ECB.ICommonInvoice commonInvoice = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			var customsChargeTypeList = new CustomsChargeTypeList();
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);

			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			chargeTypeList1 = commonInvoice.ChargeTypeList;
			chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);

			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			customsChargeTypeList = new CustomsChargeTypeList(true);
			customsChargeTypeList.Sort();
			chargeTypeList1 = commonInvoice.ChargeTypeList;
			chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);

			commonInvoice = Factory.New<JobComInvoiceHeader>().JobComInvoiceLines.AddNew();
			customsChargeTypeList = new CustomsChargeTypeList();
			customsChargeTypeList.Sort();
			chargeTypeList1 = commonInvoice.ChargeTypeList;
			chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		}

		public void TestDissectionReportLineDuty()
		{
			var currencyNZD = RefCurrency.LoadFromCurrencyCode(Factory, "NZD");
			var drawbackDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			drawbackDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			drawbackDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			var ddInvoice = drawbackDeclaration.Invoices.AddNew();
			ddInvoice.JZ_InvoiceNumber = "INV-WI00084488";
			ddInvoice.JZ_RX_NKInvoice_Currency = currencyNZD.RX_Code;
			ddInvoice.JZ_InvoiceAmount = 5785.00m;
			ddInvoice.JZ_IncoTerm = "CIF";

			var invoiceLine1 = ddInvoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 3000.00m;
			invoiceLine1.JI_Tariff = "7007.21.02.01K";
			invoiceLine1.JI_DutyCreditAmount = 228.73m;
			AssertEquals("DissectionReportLineDuty", 228.73m, invoiceLine1.DissectionReportLineDuty);

			var invoiceLine2 = ddInvoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 2785.00m;
			invoiceLine2.JI_Tariff = "7007.21.02.01K";
			invoiceLine2.JI_DutyCreditAmount = 191.30m;
			AssertEquals("DissectionReportLineDuty", 191.30m, invoiceLine2.DissectionReportLineDuty);

			drawbackDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			drawbackDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var cusEntryLine = drawbackDeclaration.CusEntryHeader.MergedLines[0];
			AssertEquals("DissectionReportDutyAmount", 420.03m, cusEntryLine.DissectionReportDutyAmount);

			ZString menuPath = "";
			ZString filterList = "";
			Guid branchGuid = (drawbackDeclaration.JE_GB.IsEmpty ? GlbBranch.CurrentBranch.PK : drawbackDeclaration.JE_GB).ToGuid();
			Guid departmentGuid = Guid.Empty;

			ZGuid printQueuePK = new ZGuid(NZCustomsDataRegistry.Instance.CustomsCertificatePrinter.GetFallBackValueAtAllLevels(Guid.Empty, branchGuid, departmentGuid));
			ZInt copies = new ZInt(NZCustomsDataRegistry.Instance.CustomsCertificateCopies.GetValueWithoutFallback(Guid.Empty, branchGuid, Guid.Empty));
			ZBool copyToEDocs = new ZBool(NZCustomsDataRegistry.Instance.CustomsCertificateCopyToEDocs.GetValueWithoutFallback(Guid.Empty, branchGuid, Guid.Empty));
			SilentDocumentPrinter documentPrinter = new SilentDocumentPrinter(Factory, drawbackDeclaration, "Dissection Report", menuPath, filterList);
			documentPrinter.Print(printQueuePK, copies, copyToEDocs);

			var print = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.Contains, "Dissection Report"));
			AssertNotNull("A print was created, we did not crash on duty values", print);
			ExcelContentTest.AssertPrintJobContainsAndNotContainsText(new string[] { "{U}-[LAMINATED SAFETY GLASS FOR]   {AK}-[0]   {AO}-[3000.00 NZD]   {AU}-[228.73]" }, Array.Empty<string>(), print);
			ExcelContentTest.AssertPrintJobContainsAndNotContainsText(new string[] { "{U}-[LAMINATED SAFETY GLASS FOR]   {AK}-[0]   {AO}-[2785.00 NZD]   {AU}-[191.3]" }, Array.Empty<string>(), print);
		}

		public void TestOverseasFreightIncludeAllNonDutiableGSTApplicableCharges()
		{
			TaxOrFeeTestHelper.SetUp(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.TopGroupInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400m, "NZD");

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = "NZD";
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;

			var otherCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, "NZD");
			otherCharge.J7_IsDutiable = false;
			otherCharge.J7_IsIncludedInITOT = false;

			var inlandFreightCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 10m, "NZD");
			inlandFreightCharge.J7_IsDutiable = false;
			inlandFreightCharge.J7_IsIncludedInITOT = false;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			declaration.ResumeApportionment();

			AssertEquals(510m, invoiceLine.JI_OverseasFreight.Amount);

			AssertEquals("GST", 1576.50m, new GSTCalculator(invoiceLine).GSTAmount);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals(510m, invoiceLine.CusEntryLine.OverseasFreight.Amount);

			var entry = declaration.CusEntryHeader;
			AssertEquals("GST should include all the other non-dutiable & gst applicable charges", 1576.50m, entry.GSTAmount);
		}

		public override void TestPivot()
		{
			Declaration.JE_MessageType = "EXP";

			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";

			OrgPartRelation supplierRelation1 = part.RelatedOrganisations.AddNew();
			supplierRelation1.OU_OH = Factory.New<OrgHeader>().PK;
			supplierRelation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			OrgPartRelation supplierRelation2 = part.RelatedOrganisations.AddNew();
			supplierRelation2.OU_OH = Factory.New<OrgHeader>().PK;
			supplierRelation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			var classification1 = Factory.New<CusClassification>();
			classification1.CC_ClassificationType = Enterprise.Customs.Business.BaseCusClassification.ClassificationType.IMP;
			var classification2 = Factory.New<CusClassification>();
			classification2.CC_ClassificationType = Enterprise.Customs.Business.BaseCusClassification.ClassificationType.Both;

			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ECB.ClassificationTypeList.Codes.HTI;
			pivot1.CI_CC = classification1.PK;
			pivot1.CI_OH = supplierRelation1.OU_OH;
			var pivot2 = part.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ECB.ClassificationTypeList.Codes.HTE;
			pivot2.CI_CC = classification1.PK;
			pivot2.CI_OH = supplierRelation1.OU_OH;
			var pivot3 = part.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = ECB.ClassificationTypeList.Codes.HTE;
			pivot3.CI_CC = classification1.PK;
			pivot3.CI_OH = supplierRelation2.OU_OH;

			InvoiceLine.InvoiceHeader.JZ_OH_Supplier = supplierRelation1.OU_OH;
			InvoiceLine.JI_PartNo = part.OP_PartNum;

			AssertNotNull(InvoiceLine.Part);
			AssertEquals(pivot2.PK, InvoiceLine.Pivot.PK);

			Declaration.JE_MessageType = DeclarationImportMessageType;
			AssertEquals(pivot1.PK, InvoiceLine.Pivot.PK);
		}

		public void TestPivotTariff()
		{
			Declaration.JE_MessageType = "EXP";

			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";

			OrgPartRelation supplierRelation1 = part.RelatedOrganisations.AddNew();
			supplierRelation1.OU_OH = Factory.New<OrgHeader>().PK;
			supplierRelation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			NZCClassification classificationForTariff = Factory.New<NZCClassification>();
			classificationForTariff.U0_Tariff = "0000.00.00.01A";
			classificationForTariff.U0_DateActiveFrom = ZDate.Today.AddDays(-1);
			classificationForTariff.U0_DateActiveTo = ZDate.Today.AddDays(1);
			classificationForTariff.U0_IsManual = false;

			NZCClassification classificationForPartsOfTariff = Factory.New<NZCClassification>();
			classificationForPartsOfTariff.U0_Tariff = "0000.00.00.01B";
			classificationForPartsOfTariff.U0_DateActiveFrom = ZDate.Today.AddDays(-1);
			classificationForPartsOfTariff.U0_DateActiveTo = ZDate.Today.AddDays(1);
			classificationForPartsOfTariff.U0_IsManual = true;

			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ECB.ClassificationTypeList.Codes.HTE;
			pivot.CI_TariffNum = "0000.00.00.01A";
			pivot.CI_ConcessionCode = "ABC123";
			pivot.CI_PartsOfClassification = "0000.00.00.01B";
			pivot.CI_OH = supplierRelation1.OU_OH;

			PermitCode permit = pivot.PermitCodes.AddNew();
			permit.ZO_Code = "CUD";
			permit.ZO_Data = "147981H";

			InvoiceLine.InvoiceHeader.JZ_OH_Supplier = supplierRelation1.OU_OH;
			InvoiceLine.JI_PartNo = part.OP_PartNum;

			AssertNotNull(InvoiceLine.Part);
			AssertEquals(pivot.PK, InvoiceLine.Pivot.PK);

			AssertEquals("Tariff number", pivot.CI_TariffNum, InvoiceLine.JI_Tariff);
			AssertEquals("PartsOfClassification", pivot.CI_PartsOfClassification, InvoiceLine.JI_PartsOfClassification);
			AssertEquals("Concession Code", pivot.CI_ConcessionCode, InvoiceLine.JI_ConcessionCode);
			AssertEquals("Permit code", 1, InvoiceLine.PermitCodes.Count);
			AssertEquals("Permit code", "CUD", InvoiceLine.PermitCodes[0].ZO_Code);
		}

		public override void TestJI_FormattedTariff()
		{
			ZString tariff = "1234567890K";
			InvoiceLine.JI_Tariff = tariff;
			AssertEquals("JI_FormattedTariff", "1234.56.78.90K", InvoiceLine.JI_FormattedTariff);
			tariff = "9876.54.32.10K";
			InvoiceLine.JI_FormattedTariff = tariff;
			AssertEquals("JI_FormattedTariff", tariff, InvoiceLine.JI_FormattedTariff);
		}

		public void TestJI_EffectivePreferentialCountryGroupWithPreferentialCountryGroupCodeDefaultingSetToFalse()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfArrival = new ZDateTime(2009, 11, 21);

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.PapuaNewGuinea;
			invoiceLine.JI_Tariff = "0203.11.00.02C";
			invoiceLine.JI_LinePrice = 1000;
			invoiceLine.JI_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.NonQualifying;
			AssertEquals("invoiceLine.JI_EffectivePreferentialCountryGroup", "", invoiceLine.JI_EffectivePreferentialCountryGroup);
			AssertEquals("invoiceLine.JI_EffectivePreferentialCountryGroupName", "", invoiceLine.PrefGroupDescription);

			invoiceLine.JI_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.Qualifies;
			AssertEquals("invoiceLine.JI_EffectivePreferentialCountryGroup", "", invoiceLine.JI_EffectivePreferentialCountryGroup);
			AssertEquals("invoiceLine.JI_EffectivePreferentialCountryGroupName", "", invoiceLine.PrefGroupDescription);

			invoiceLine.JI_PreferentialCountryGroup = "LDC";
			AssertEquals("invoiceLine.JI_EffectivePreferentialCountryGroup", "LDC", invoiceLine.JI_EffectivePreferentialCountryGroup);
			AssertEquals("invoiceLine.JI_EffectivePreferentialCountryGroupName", "LESS DEVELOPED COUNTRIES", invoiceLine.PrefGroupDescription);

			invoiceLine.JI_PreferentialCountryGroup = "PAC";
			AssertEquals("invoiceLine.JI_EffectivePreferentialCountryGroup", "PAC", invoiceLine.JI_EffectivePreferentialCountryGroup);
			AssertEquals("invoiceLine.JI_EffectivePreferentialCountryGroupName", "PACIFICA", invoiceLine.PrefGroupDescription);

			invoiceLine.JI_PreferentialCountryGroup = ZString.Empty;
			AssertEquals("invoiceLine.JI_EffectivePreferentialCountryGroup", "", invoiceLine.JI_EffectivePreferentialCountryGroup);
			AssertEquals("invoiceLine.JI_EffectivePreferentialCountryGroupName", "", invoiceLine.PrefGroupDescription);

			invoice.JZ_DefaultPreferentialCountryGroup = "LDC";
			AssertEquals("invoiceLine.JI_EffectivePreferentialCountryGroup", "LDC", invoiceLine.JI_EffectivePreferentialCountryGroup);
			AssertEquals("invoiceLine.JI_EffectivePreferentialCountryGroupName", "LESS DEVELOPED COUNTRIES", invoiceLine.PrefGroupDescription);

			invoice.JZ_DefaultPreferentialCountryGroup = "PAC";
			AssertEquals("invoiceLine.JI_EffectivePreferentialCountryGroup", "PAC", invoiceLine.JI_EffectivePreferentialCountryGroup);
			AssertEquals("invoiceLine.JI_EffectivePreferentialCountryGroupName", "PACIFICA", invoiceLine.PrefGroupDescription);

			invoiceLine.JI_PreferentialCountryGroup = "LDC";
			AssertEquals("invoiceLine.JI_EffectivePreferentialCountryGroup", "LDC", invoiceLine.JI_EffectivePreferentialCountryGroup);
			AssertEquals("invoiceLine.JI_EffectivePreferentialCountryGroupName", "LESS DEVELOPED COUNTRIES", invoiceLine.PrefGroupDescription);
		}

		public void TestBarrierDate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			AssertEquals("Pre-condition: invoiceLine.DateForDutyRate", declaration.DateForDutyRate, invoiceLine.DateForDutyRate);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfArrival = new ZDateTime(2009, 11, 21);
			AssertEquals("invoiceLine.DateForDutyRate", declaration.DateForDutyRate, invoiceLine.DateForDutyRate);
			AssertEquals("invoiceLine.DateForDutyRate", declaration.JE_DateOfArrival, invoiceLine.DateForDutyRate);
		}

		public void TestEffectiveMAF_Measurement()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("EffectiveMAF_MeasurementValue", ZInt.Zero, invoiceLine.EffectiveMAF_MeasurementValue);
			AssertEquals("EffectiveMAF_MeasurementUQ", ZString.Empty, invoiceLine.EffectiveMAF_MeasurementUQ);

			invoiceLine.JI_Weight = 123;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("EffectiveMAF_MeasurementValue", 123, invoiceLine.EffectiveMAF_MeasurementValue);
			AssertEquals("EffectiveMAF_MeasurementUQ", MeasurementUQList.Codes.kilograms, invoiceLine.EffectiveMAF_MeasurementUQ);

			invoiceLine.JI_MAF_MeasurementValue = 43;
			invoiceLine.JI_MAF_MeasurementUQ = MeasurementUQList.Codes.egg;
			AssertEquals("EffectiveMAF_MeasurementValue", 43, invoiceLine.EffectiveMAF_MeasurementValue);
			AssertEquals("EffectiveMAF_MeasurementUQ", MeasurementUQList.Codes.egg, invoiceLine.EffectiveMAF_MeasurementUQ);
		}

		public void TestEffectiveMAF_GoodsTypeDescription()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("invoiceLine.EffectiveMAF_GoodsTypeDescription", GoodsTypeList.Descriptions.Miscellaneous, invoiceLine.EffectiveMAF_GoodsTypeDescription);
			invoiceLine.JI_Tariff = "0101.34.32.00A";
			AssertEquals("invoiceLine.EffectiveMAF_GoodsTypeDescription", GoodsTypeList.Descriptions.Animals, invoiceLine.EffectiveMAF_GoodsTypeDescription);
			invoiceLine.JI_MAF_GoodsType = GoodsTypeList.Codes.Tyres;
			AssertEquals("invoiceLine.EffectiveMAF_GoodsTypeDescription", GoodsTypeList.Descriptions.Tyres, invoiceLine.EffectiveMAF_GoodsTypeDescription);
		}

		public void TestEffectiveMAF_NewGoodsDescription()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("invoiceLine.EffectiveMAF_NewGoodsDescription", Enterprise.Customs.Business.YesNoList.Descriptions.Yes, invoiceLine.EffectiveMAF_NewGoodsDescription);
			invoiceLine.ProhibitedCodes.AddNew(ProhibitedCodeList.Codes.MpiUsedGoods, "");
			AssertEquals("invoiceLine.EffectiveMAF_NewGoodsDescription", Enterprise.Customs.Business.YesNoList.Descriptions.No, invoiceLine.EffectiveMAF_NewGoodsDescription);
			invoiceLine.JI_MAF_NewGoods = Enterprise.Customs.Business.YesNoList.Codes.Yes;
			AssertEquals("invoiceLine.EffectiveMAF_NewGoodsDescription", Enterprise.Customs.Business.YesNoList.Descriptions.Yes, invoiceLine.EffectiveMAF_NewGoodsDescription);
		}

		public void TestAddRoyaltyEnteredAgainstOwnerProductHasOwnerAndSupplier()
		{
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_PartNum = "BEER";

			OrgHeader orgOwner = Factory.NewWithValidTestData<OrgHeader>();
			orgOwner.OH_IsConsignee = true;
			orgOwner.OH_IsConsignor = true;

			OrgPartRelation relationOwner = part.RelatedOrganisations.AddNew();
			relationOwner.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relationOwner.OU_RoyaltyPercent = 10m;
			relationOwner.OU_OH = orgOwner.PK;

			OrgHeader orgSupplier = Factory.NewWithValidTestData<OrgHeader>();
			orgSupplier.OH_IsConsignee = true;
			orgSupplier.OH_IsConsignor = true;

			OrgPartRelation relationSupplier = part.RelatedOrganisations.AddNew();
			relationSupplier.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relationSupplier.OU_RoyaltyPercent = 0m;
			relationSupplier.OU_OH = orgSupplier.PK;

			InvoiceLine.JI_LinePrice = 200m;

			Declaration.Invoices.Add(InvoiceHeader);
			InvoiceHeader.JobComInvoiceLines.Add(InvoiceLine);
			Declaration.JE_OH_Supplier = orgSupplier.PK;
			Declaration.JE_OH_Importer = orgOwner.PK;

			Factory.Save();

			InvoiceLine.JI_PartNo = part.OP_PartNum;
			InvoiceLine.JI_OP = part.PK;

			AssertEquals("InvoiceLine.Charges.Count", 1, InvoiceLine.Charges.Count);

			AssertEquals("InvoiceLine.Charges[0].J7_Percentage", 10.0m, InvoiceLine.Charges[0].J7_Percentage);
			AssertEquals("InvoiceLine.Charges[0].J7_Amount", 20.0m, InvoiceLine.Charges[0].J7_Amount);
			AssertEquals("InvoiceLine.Charges[0].J7_ChargeType", CustomsChargeTypeList.Codes.Commission, InvoiceLine.Charges[0].J7_ChargeType);
		}

		public void TestAddRoyaltyEnteredAgainstSupplierProductHasOwnerAndSupplier()
		{
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_PartNum = "BEER";

			OrgHeader orgOwner = Factory.NewWithValidTestData<OrgHeader>();
			orgOwner.OH_IsConsignee = true;
			orgOwner.OH_IsConsignor = true;

			OrgPartRelation relationOwner = part.RelatedOrganisations.AddNew();
			relationOwner.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relationOwner.OU_RoyaltyPercent = 0m;
			relationOwner.OU_OH = orgOwner.PK;

			OrgHeader orgSupplier = Factory.NewWithValidTestData<OrgHeader>();
			orgSupplier.OH_IsConsignee = true;
			orgSupplier.OH_IsConsignor = true;

			OrgPartRelation relationSupplier = part.RelatedOrganisations.AddNew();
			relationSupplier.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relationSupplier.OU_RoyaltyPercent = 10m;
			relationSupplier.OU_OH = orgSupplier.PK;

			InvoiceLine.JI_LinePrice = 200m;

			Declaration.Invoices.Add(InvoiceHeader);
			InvoiceHeader.JobComInvoiceLines.Add(InvoiceLine);
			Declaration.JE_OH_Supplier = orgSupplier.PK;
			Declaration.JE_OH_Importer = orgOwner.PK;

			Factory.Save();

			InvoiceLine.JI_PartNo = part.OP_PartNum;
			InvoiceLine.JI_OP = part.PK;

			AssertEquals("InvoiceLine.Charges.Count", 1, InvoiceLine.Charges.Count);

			AssertEquals("InvoiceLine.Charges[0].J7_Percentage", 10.0m, InvoiceLine.Charges[0].J7_Percentage);
			AssertEquals("InvoiceLine.Charges[0].J7_Amount", 20.0m, InvoiceLine.Charges[0].J7_Amount);
			AssertEquals("InvoiceLine.Charges[0].J7_ChargeType", CustomsChargeTypeList.Codes.Commission, InvoiceLine.Charges[0].J7_ChargeType);
		}

		public void TestAddRoyaltyAllocatedToOwnerOwnerAndSupplierArePseudoBoth()
		{
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_PartNum = "BEER";

			OrgHeader orgOwner = Factory.NewWithValidTestData<OrgHeader>();
			orgOwner.OH_IsConsignee = true;
			orgOwner.OH_IsConsignor = true;

			OrgPartRelation relationOwner = part.RelatedOrganisations.AddNew();
			relationOwner.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relationOwner.OU_RoyaltyPercent = 10m;
			relationOwner.OU_OH = orgOwner.PK;

			OrgHeader orgSupplier = Factory.NewWithValidTestData<OrgHeader>();
			orgSupplier.OH_IsConsignee = true;
			orgSupplier.OH_IsConsignor = true;

			OrgPartRelation relationSupplier = part.RelatedOrganisations.AddNew();
			relationSupplier.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relationSupplier.OU_RoyaltyPercent = 0m;
			relationSupplier.OU_OH = orgSupplier.PK;

			InvoiceLine.JI_LinePrice = 200m;

			Declaration.Invoices.Add(InvoiceHeader);
			InvoiceHeader.JobComInvoiceLines.Add(InvoiceLine);
			Declaration.JE_OH_Supplier = orgSupplier.PK;
			Declaration.JE_OH_Importer = orgOwner.PK;

			Factory.Save();

			InvoiceLine.JI_PartNo = part.OP_PartNum;
			InvoiceLine.JI_OP = part.PK;

			AssertEquals("InvoiceLine.Charges.Count", 1, InvoiceLine.Charges.Count);

			AssertEquals("InvoiceLine.Charges[0].J7_Percentage", 10.0m, InvoiceLine.Charges[0].J7_Percentage);
			AssertEquals("InvoiceLine.Charges[0].J7_Amount", 20.0m, InvoiceLine.Charges[0].J7_Amount);
			AssertEquals("InvoiceLine.Charges[0].J7_ChargeType", CustomsChargeTypeList.Codes.Commission, InvoiceLine.Charges[0].J7_ChargeType);
		}

		public void TestAddRoyaltyAllocatedToSupplierOwnerAndSupplierArePseudoBoth()
		{
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_PartNum = "BEER";

			OrgHeader orgOwner = Factory.NewWithValidTestData<OrgHeader>();
			orgOwner.OH_IsConsignee = true;
			orgOwner.OH_IsConsignor = true;

			OrgPartRelation relationOwner = part.RelatedOrganisations.AddNew();
			relationOwner.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relationOwner.OU_RoyaltyPercent = 0m;
			relationOwner.OU_OH = orgOwner.PK;

			OrgHeader orgSupplier = Factory.NewWithValidTestData<OrgHeader>();
			orgSupplier.OH_IsConsignee = true;
			orgSupplier.OH_IsConsignor = true;

			OrgPartRelation relationSupplier = part.RelatedOrganisations.AddNew();
			relationSupplier.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relationSupplier.OU_RoyaltyPercent = 10m;
			relationSupplier.OU_OH = orgSupplier.PK;

			InvoiceLine.JI_LinePrice = 200m;

			Declaration.Invoices.Add(InvoiceHeader);
			InvoiceHeader.JobComInvoiceLines.Add(InvoiceLine);
			Declaration.JE_OH_Supplier = orgSupplier.PK;
			Declaration.JE_OH_Importer = orgOwner.PK;

			Factory.Save();

			InvoiceLine.JI_PartNo = part.OP_PartNum;
			InvoiceLine.JI_OP = part.PK;

			AssertEquals("InvoiceLine.Charges.Count", 1, InvoiceLine.Charges.Count);

			AssertEquals("InvoiceLine.Charges[0].J7_Percentage", 10.0m, InvoiceLine.Charges[0].J7_Percentage);
			AssertEquals("InvoiceLine.Charges[0].J7_Amount", 20.0m, InvoiceLine.Charges[0].J7_Amount);
			AssertEquals("InvoiceLine.Charges[0].J7_ChargeType", CustomsChargeTypeList.Codes.Commission, InvoiceLine.Charges[0].J7_ChargeType);
		}

		public void TestOtherImportersAndSuppliersDontAffectTheDefaultingOfRoyalties()
		{
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_PartNum = "BEER";

			OrgHeader orgBothOther = Factory.NewWithValidTestData<OrgHeader>();
			orgBothOther.OH_IsConsignee = true;
			orgBothOther.OH_IsConsignor = true;

			OrgPartRelation relationBothOther = part.RelatedOrganisations.AddNew();
			relationBothOther.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relationBothOther.OU_OH = orgBothOther.PK;

			OrgHeader orgBoth = Factory.NewWithValidTestData<OrgHeader>();
			orgBoth.OH_IsConsignee = true;
			orgBoth.OH_IsConsignor = true;

			OrgPartRelation relationBoth = part.RelatedOrganisations.AddNew();
			relationBoth.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relationBoth.OU_OH = orgBoth.PK;

			OrgHeader orgBothBunkus = Factory.NewWithValidTestData<OrgHeader>();
			orgBothBunkus.OH_IsConsignee = true;
			orgBothBunkus.OH_IsConsignor = true;

			OrgPartRelation relationBothBunkus = part.RelatedOrganisations.AddNew();
			relationBothBunkus.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relationBothBunkus.OU_OH = orgBothBunkus.PK;

			InvoiceLine.JI_LinePrice = 200m;

			Declaration.Invoices.Add(InvoiceHeader);
			InvoiceHeader.JobComInvoiceLines.Add(InvoiceLine);
			Declaration.JE_OH_Supplier = orgBoth.PK;
			Declaration.JE_OH_Importer = orgBoth.PK;

			Factory.Save();

			AssertEquals("Precondition: InvoiceLine.Charges.Count", 0, InvoiceLine.Charges.Count);

			relationBothOther.OU_RoyaltyPercent = 10m;
			relationBoth.OU_RoyaltyPercent = 5m;
			relationBothBunkus.OU_RoyaltyPercent = 15m;

			SetPartNoAndAssertJI_OPIsPartPK(part);

			AssertEquals("InvoiceLine.Charges.Count", 1, InvoiceLine.Charges.Count);

			AssertEquals("InvoiceLine.Charges[0].J7_Percentage", 5.0m, InvoiceLine.Charges[0].J7_Percentage);
			AssertEquals("InvoiceLine.Charges[0].J7_Amount", 10.0m, InvoiceLine.Charges[0].J7_Amount);
		}

		public void TestRoyaltyFlatAmountIsProperlySetWhenPartNoIsSet()
		{
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_PartNum = "BEER";

			OrgHeader orgBoth = Factory.NewWithValidTestData<OrgHeader>();
			orgBoth.OH_IsConsignee = true;
			orgBoth.OH_IsConsignor = true;

			OrgPartRelation relationBoth = part.RelatedOrganisations.AddNew();
			relationBoth.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relationBoth.OU_OH = orgBoth.PK;

			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			currency.RX_Code = "ZZZ";

			InvoiceLine.JI_LinePrice = 200m;

			Declaration.Invoices.Add(InvoiceHeader);
			InvoiceHeader.JobComInvoiceLines.Add(InvoiceLine);
			Declaration.JE_OH_Supplier = orgBoth.PK;
			Declaration.JE_OH_Importer = orgBoth.PK;

			Factory.Save();

			AssertEquals("Precondition: InvoiceLine.Charges.Count", 0, InvoiceLine.Charges.Count);

			relationBoth.OU_RoyaltyFlatAmount = 35m;
			relationBoth.OU_RX_NKRoyaltyCurrency = currency.RX_Code;

			SetPartNoAndAssertJI_OPIsPartPK(part);

			AssertEquals("InvoiceLine.Charges.Count", 1, InvoiceLine.Charges.Count);

			AssertEquals("InvoiceLine.Charges[0].J7_Percentage", 0.0m, InvoiceLine.Charges[0].J7_Percentage);
			AssertEquals("InvoiceLine.Charges[0].J7_Amount", 35.0m, InvoiceLine.Charges[0].J7_Amount);
			AssertEquals("InvoiceLine.Charges[0].J7_RX_NKCurrency", "ZZZ", InvoiceLine.Charges[0].J7_RX_NKCurrency);
		}

		public void TestRoyaltyPercentIsProperlySetWhenPartNoIsSet()
		{
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_PartNum = "BEER";

			OrgHeader orgBoth = Factory.NewWithValidTestData<OrgHeader>();
			orgBoth.OH_IsConsignee = true;
			orgBoth.OH_IsConsignor = true;

			OrgPartRelation relationBoth = part.RelatedOrganisations.AddNew();
			relationBoth.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relationBoth.OU_OH = orgBoth.PK;

			InvoiceLine.JI_LinePrice = 200m;

			Declaration.Invoices.Add(InvoiceHeader);
			InvoiceHeader.JobComInvoiceLines.Add(InvoiceLine);
			Declaration.JE_OH_Supplier = orgBoth.PK;
			Declaration.JE_OH_Importer = orgBoth.PK;

			Factory.Save();

			AssertEquals("Precondition: InvoiceLine.Charges.Count", 0, InvoiceLine.Charges.Count);

			relationBoth.OU_RoyaltyPercent = 5m;

			SetPartNoAndAssertJI_OPIsPartPK(part);

			AssertEquals("InvoiceLine.Charges.Count", 1, InvoiceLine.Charges.Count);

			AssertEquals("InvoiceLine.Charges[0].J7_Percentage", 5.0m, InvoiceLine.Charges[0].J7_Percentage);
			AssertEquals("InvoiceLine.Charges[0].J7_Amount", 10.0m, InvoiceLine.Charges[0].J7_Amount);
		}

		public void TestHeaderDataIsUpdatedProperlyWhenHeaderIsChangedOriginCountry()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfArrival = new ZDateTime(2009, 11, 21);

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 5;
			invoiceHeader.JZ_RN_NKDefaultOrigin = "AU";

			JobComInvoiceLine invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.PapuaNewGuinea;
			invoiceLine.JI_LinePrice = 1000;

			//1
			invoiceLine.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.NewZealand;
			AssertNotEquals("Header=AU Line=NZ", invoiceHeader.JZ_RN_NKDefaultOrigin, invoiceLine.JI_CountryOfOrigin);
			invoiceLine.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.Australia;
			AssertEquals("Header=AU Line=AU", invoiceHeader.JZ_RN_NKDefaultOrigin, invoiceLine.JI_CountryOfOrigin);
			invoiceHeader.JZ_RN_NKDefaultOrigin = Enterprise.Core.Constants.CountryCodes.KoreaSouth;
			AssertEquals("Header=KR Line=KR", invoiceHeader.JZ_RN_NKDefaultOrigin, invoiceLine.JI_CountryOfOrigin);

			//2
			invoiceHeader.JZ_RN_NKDefaultOrigin = Enterprise.Core.Constants.CountryCodes.Australia;
			invoiceLine.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.Australia;
			AssertEquals("Header=AU Line=AU", invoiceHeader.JZ_RN_NKDefaultOrigin, invoiceLine.JI_CountryOfOrigin);

			invoiceLine.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.KoreaSouth;
			AssertNotEquals("Header=AU Line=KR", invoiceHeader.JZ_RN_NKDefaultOrigin, invoiceLine.JI_CountryOfOrigin);

			invoiceHeader.JZ_RN_NKDefaultOrigin = Enterprise.Core.Constants.CountryCodes.SaudiArabia;
			AssertNotEquals("Header=SA Line=KR", invoiceHeader.JZ_RN_NKDefaultOrigin, invoiceLine.JI_CountryOfOrigin);

			invoiceLine.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.SaudiArabia;
			AssertEquals("Header=SA Line=SA", invoiceHeader.JZ_RN_NKDefaultOrigin, invoiceLine.JI_CountryOfOrigin);

			invoiceHeader.JZ_RN_NKDefaultOrigin = Enterprise.Core.Constants.CountryCodes.Japan;
			AssertEquals("Header=JP Line=JP", invoiceHeader.JZ_RN_NKDefaultOrigin, invoiceLine.JI_CountryOfOrigin);
		}

		public void TestHeaderDataIsUpdatedProperlyWhenHeaderIsChangedTest1()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfArrival = new ZDateTime(2009, 11, 21);

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 5;

			JobComInvoiceLine invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.PapuaNewGuinea;
			invoiceLine.JI_LinePrice = 1000;
			invoiceLine.JI_CountryOfOrigin = "";

			#region TEST1

			invoiceHeader.JZ_RN_NKDefaultOrigin = Enterprise.Core.Constants.CountryCodes.Australia;
			invoiceHeader.JZ_DefaultOriginRegion = "NSW";
			invoiceHeader.JZ_RN_NKDefaultExport = Enterprise.Core.Constants.CountryCodes.NewZealand;
			invoiceHeader.JZ_DefaultQualifiesForPreferentialDuty = "Q";
			invoiceHeader.JZ_DefaultPreferentialCountryGroup = "AAN";
			invoiceHeader.JZ_IsZeroRatedDuty = "Y";
			invoiceHeader.JZ_IsZeroRatedExcise = "Y";
			invoiceHeader.JZ_IsZeroRatedLevies = "Y";
			invoiceHeader.JZ_IsZeroRatedGST = "Y";

			AssertEquals("Origion     		 Header=AU	 Line=AU", invoiceHeader.JZ_RN_NKDefaultOrigin, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("OrigionRegion		 Header=NSW	 Line=NSW", invoiceHeader.JZ_DefaultOriginRegion, invoiceLine.JI_OriginRegion);
			AssertEquals("Export					 Header=NZ	 Line=NZ", invoiceHeader.JZ_RN_NKDefaultExport, invoiceLine.JI_RN_NKCountryOfExport);
			AssertEquals("Preference			 Header=Q		 Line=Q", invoiceHeader.JZ_DefaultQualifiesForPreferentialDuty, invoiceLine.JI_QualifiesForPreferentialDuty);
			AssertEquals("PreferenceGroup	 Header=AAN	 Line=AAN", invoiceHeader.JZ_DefaultPreferentialCountryGroup, invoiceLine.JI_PreferentialCountryGroup);
			AssertEquals("Duty		Header=Y	 Line=Y", invoiceHeader.JZ_IsZeroRatedDuty, invoiceLine.JI_IsZeroRatedDuty);
			AssertEquals("Excise	Header=Y	 Line=Y", invoiceHeader.JZ_IsZeroRatedExcise, invoiceLine.JI_IsZeroRatedExcise);
			AssertEquals("Levies	Header=Y	 Line=Y", invoiceHeader.JZ_IsZeroRatedLevies, invoiceLine.JI_IsZeroRatedLevies);
			AssertEquals("GST			Header=Y	 Line=Y", invoiceHeader.JZ_IsZeroRatedGST, invoiceLine.JI_IsZeroRatedGST);

			#endregion

			#region TEST3

			invoiceHeader.JZ_RN_NKDefaultOrigin = Enterprise.Core.Constants.CountryCodes.Australia;
			invoiceHeader.JZ_DefaultOriginRegion = "VIC";
			invoiceHeader.JZ_RN_NKDefaultExport = Enterprise.Core.Constants.CountryCodes.Australia;
			invoiceHeader.JZ_DefaultQualifiesForPreferentialDuty = "N";
			invoiceHeader.JZ_DefaultPreferentialCountryGroup = "";
			invoiceHeader.JZ_IsZeroRatedDuty = "N";
			invoiceHeader.JZ_IsZeroRatedExcise = "N";
			invoiceHeader.JZ_IsZeroRatedLevies = "N";
			invoiceHeader.JZ_IsZeroRatedGST = "N";

			AssertEquals("Origion     		 Header=AU	 Line=AU", invoiceHeader.JZ_RN_NKDefaultOrigin, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("OrigionRegion		 Header=VIC	 Line=VIC", invoiceHeader.JZ_DefaultOriginRegion, invoiceLine.JI_OriginRegion);
			AssertEquals("Export					 Header=AU	 Line=AU", invoiceHeader.JZ_RN_NKDefaultExport, invoiceLine.JI_RN_NKCountryOfExport);
			AssertEquals("Preference			 Header=N		 Line=N", invoiceHeader.JZ_DefaultQualifiesForPreferentialDuty, invoiceLine.JI_QualifiesForPreferentialDuty);
			AssertEquals("PreferenceGroup	 Header= 		 Line= ", invoiceHeader.JZ_DefaultPreferentialCountryGroup, invoiceLine.JI_PreferentialCountryGroup);
			AssertEquals("Duty		Header=N	 Line=N", invoiceHeader.JZ_IsZeroRatedDuty, invoiceLine.JI_IsZeroRatedDuty);
			AssertEquals("Excise	Header=N	 Line=N", invoiceHeader.JZ_IsZeroRatedExcise, invoiceLine.JI_IsZeroRatedExcise);
			AssertEquals("Levies	Header=N	 Line=N", invoiceHeader.JZ_IsZeroRatedLevies, invoiceLine.JI_IsZeroRatedLevies);
			AssertEquals("GST			Header=N	 Line=N", invoiceHeader.JZ_IsZeroRatedGST, invoiceLine.JI_IsZeroRatedGST);

			#endregion

			#region TEST4

			invoiceLine.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.Singapore;
			invoiceLine.JI_OriginRegion = "NSW";
			invoiceLine.JI_RN_NKCountryOfExport = Enterprise.Core.Constants.CountryCodes.Singapore;
			invoiceLine.JI_QualifiesForPreferentialDuty = "Q";
			invoiceLine.JI_PreferentialCountryGroup = "SG";

			invoiceHeader.JZ_IsZeroRatedDuty = "";
			invoiceHeader.JZ_IsZeroRatedExcise = "";
			invoiceHeader.JZ_IsZeroRatedLevies = "";
			invoiceHeader.JZ_IsZeroRatedGST = "";

			AssertEquals("AU", invoiceHeader.JZ_RN_NKDefaultOrigin);
			AssertEquals("SG", invoiceLine.JI_CountryOfOrigin);
			AssertNotEquals("Origion     		 Header=AU	 Line=SG", invoiceHeader.JZ_RN_NKDefaultOrigin, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("VIC", invoiceHeader.JZ_DefaultOriginRegion);
			AssertNotEquals("OrigionRegion		 Header=VIC	 Line=NSW", invoiceHeader.JZ_DefaultOriginRegion, invoiceLine.JI_OriginRegion);
			AssertEquals("AU", invoiceHeader.JZ_RN_NKDefaultExport);
			AssertNotEquals("Export					 Header=AU	 Line=SG", invoiceHeader.JZ_RN_NKDefaultExport, invoiceLine.JI_RN_NKCountryOfExport);
			AssertEquals("N", invoiceHeader.JZ_DefaultQualifiesForPreferentialDuty);
			AssertNotEquals("Preference			 Header=N		 Line=Q", invoiceHeader.JZ_DefaultQualifiesForPreferentialDuty, invoiceLine.JI_QualifiesForPreferentialDuty);
			AssertNotEquals("PreferenceGroup	 Header=  	 Line=SG", invoiceHeader.JZ_DefaultPreferentialCountryGroup, invoiceLine.JI_PreferentialCountryGroup);
			AssertEquals("Duty		Header=\"	 Line=\"", invoiceHeader.JZ_IsZeroRatedDuty, invoiceLine.JI_IsZeroRatedDuty);
			AssertEquals("Excise	Header=\"	 Line=\"", invoiceHeader.JZ_IsZeroRatedExcise, invoiceLine.JI_IsZeroRatedExcise);
			AssertEquals("Levies	Header=\"	 Line=\"", invoiceHeader.JZ_IsZeroRatedLevies, invoiceLine.JI_IsZeroRatedLevies);
			AssertEquals("GST			Header=\"	 Line=\"", invoiceHeader.JZ_IsZeroRatedGST, invoiceLine.JI_IsZeroRatedGST);

			#endregion

			#region TEST5

			invoiceHeader.JZ_RN_NKDefaultOrigin = Enterprise.Core.Constants.CountryCodes.Singapore;
			invoiceHeader.JZ_DefaultOriginRegion = "VIC";
			invoiceHeader.JZ_RN_NKDefaultExport = Enterprise.Core.Constants.CountryCodes.Singapore;
			invoiceHeader.JZ_DefaultQualifiesForPreferentialDuty = "Q";
			invoiceHeader.JZ_DefaultPreferentialCountryGroup = "AAN";

			AssertEquals("Origion     		 Header=SG	 Line=SG", invoiceHeader.JZ_RN_NKDefaultOrigin, invoiceLine.JI_CountryOfOrigin);
			AssertNotEquals("OrigionRegion		 Header=VIC	 Line=NSW", invoiceHeader.JZ_DefaultOriginRegion, invoiceLine.JI_OriginRegion);
			AssertEquals("Export					 Header=SG	 Line=SG", invoiceHeader.JZ_RN_NKDefaultExport, invoiceLine.JI_RN_NKCountryOfExport);
			AssertEquals("Preference			 Header=Q		 Line=Q", invoiceHeader.JZ_DefaultQualifiesForPreferentialDuty, invoiceLine.JI_QualifiesForPreferentialDuty);
			AssertNotEquals("PreferenceGroup	 Header=AAN  	 Line=SG", invoiceHeader.JZ_DefaultPreferentialCountryGroup, invoiceLine.JI_PreferentialCountryGroup);

			#endregion

			#region TEST6

			invoiceLine.JI_PreferentialCountryGroup = "SG";
			AssertNotEquals("PreferenceGroup	 Header=AAN  	 Line=SG", invoiceHeader.JZ_DefaultPreferentialCountryGroup, invoiceLine.JI_PreferentialCountryGroup);

			#endregion
		}

		public void TestHeaderDataIsUpdatedProperlyWhenHeaderIsChangedTest2()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			declaration.JE_DateOfArrival = new ZDateTime(2009, 11, 21);

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 5;

			JobComInvoiceLine invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.PapuaNewGuinea;
			invoiceLine.JI_LinePrice = 1000;

			invoiceHeader.JZ_RN_NKDefaultOrigin = Enterprise.Core.Constants.CountryCodes.Australia;
			invoiceHeader.JZ_DefaultOriginRegion = "NSW";
			invoiceHeader.JZ_RN_NKDefaultExport = Enterprise.Core.Constants.CountryCodes.NewZealand;
			invoiceHeader.JZ_DefaultQualifiesForPreferentialDuty = "Q";
			invoiceHeader.JZ_DefaultPreferentialCountryGroup = "AAN";
			invoiceHeader.JZ_IsZeroRatedDuty = "Y";
			invoiceHeader.JZ_IsZeroRatedExcise = "Y";
			invoiceHeader.JZ_IsZeroRatedLevies = "Y";
			invoiceHeader.JZ_IsZeroRatedGST = "Y";

			AssertEquals("OrigionRegion		 Header=NSW	 Line=NSW", invoiceHeader.JZ_DefaultOriginRegion, invoiceLine.JI_OriginRegion);
			AssertEquals("Export					 Header=NZ	 Line=NZ", invoiceHeader.JZ_RN_NKDefaultExport, invoiceLine.JI_RN_NKCountryOfExport);
			AssertEquals("Preference			 Header=Q		 Line=Q", invoiceHeader.JZ_DefaultQualifiesForPreferentialDuty, invoiceLine.JI_QualifiesForPreferentialDuty);
			AssertEquals("PreferenceGroup	 Header=AAN	 Line=AAN", invoiceHeader.JZ_DefaultPreferentialCountryGroup, invoiceLine.JI_PreferentialCountryGroup);
			AssertEquals("Duty		Header=Y	 Line=Y", invoiceHeader.JZ_IsZeroRatedDuty, invoiceLine.JI_IsZeroRatedDuty);
			AssertEquals("Excise	Header=Y	 Line=Y", invoiceHeader.JZ_IsZeroRatedExcise, invoiceLine.JI_IsZeroRatedExcise);
			AssertEquals("Levies	Header=Y	 Line=Y", invoiceHeader.JZ_IsZeroRatedLevies, invoiceLine.JI_IsZeroRatedLevies);
			AssertEquals("GST			Header=Y	 Line=Y", invoiceHeader.JZ_IsZeroRatedGST, invoiceLine.JI_IsZeroRatedGST);
		}

		public void TestCountryOfOriginIsSetWhenPartNoIsSet()
		{
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_PartNum = "CERVESA - CORONA";
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			pivot.CI_TariffNum = "10203040";

			OrgHeader orgBoth = Factory.NewWithValidTestData<OrgHeader>();
			orgBoth.OH_IsConsignee = true;
			orgBoth.OH_IsConsignor = true;

			OrgPartRelation relationBoth = part.RelatedOrganisations.AddNew();
			relationBoth.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relationBoth.OU_OH = orgBoth.PK;

			InvoiceLine.JI_LinePrice = 200m;
			Declaration.Invoices.Add(InvoiceHeader);
			InvoiceHeader.JobComInvoiceLines.Add(InvoiceLine);
			Declaration.JE_OH_Supplier = orgBoth.PK;
			Declaration.JE_OH_Importer = orgBoth.PK;
			Factory.Save();

			AssertEquals("Precondition: InvoiceLine Country/Region of Origin", "", InvoiceLine.JI_CountryOfOrigin);

			InvoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("InvoiceLine Country/Region of Origin should be defaulted from Part", "MX", InvoiceLine.JI_CountryOfOrigin);
		}

		public void TestWhenRoyaltyPercentageAndFlatAmountAreSetTwoChargeLinesAreCreated()
		{
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_PartNum = "BEER";

			OrgHeader orgBoth = Factory.NewWithValidTestData<OrgHeader>();
			orgBoth.OH_IsConsignee = true;
			orgBoth.OH_IsConsignor = true;
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			currency.RX_Code = "ZZZ";

			OrgPartRelation relationBoth = part.RelatedOrganisations.AddNew();
			relationBoth.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relationBoth.OU_OH = orgBoth.PK;

			relationBoth.OU_RoyaltyPercent = 5.0m;
			relationBoth.OU_RoyaltyFlatAmount = 35m;
			relationBoth.OU_RX_NKRoyaltyCurrency = currency.RX_Code;

			InvoiceLine.JI_LinePrice = 200m;

			Declaration.Invoices.Add(InvoiceHeader);
			InvoiceHeader.JobComInvoiceLines.Add(InvoiceLine);
			Declaration.JE_OH_Supplier = orgBoth.PK;
			Declaration.JE_OH_Importer = orgBoth.PK;

			Factory.Save();

			AssertEquals("Precondition: InvoiceLine.Charges.Count", 0, InvoiceLine.Charges.Count);
			SetPartNoAndAssertJI_OPIsPartPK(part);

			AssertEquals("InvoiceLine.Charges.Count", 2, InvoiceLine.Charges.Count);

			AssertEquals("InvoiceLine.Charges[0].J7_Percentage", 5.0m, InvoiceLine.Charges[0].J7_Percentage);
			AssertEquals("InvoiceLine.Charges[0].J7_Amount", 10.0m, InvoiceLine.Charges[0].J7_Amount);
			AssertNotEquals("InvoiceLine.Charges[0].J7_RX_NKCurrency", "ZZZ", InvoiceLine.Charges[0].J7_RX_NKCurrency);
			AssertEquals("InvoiceLine.Charges[1].J7_Percentage", 0m, InvoiceLine.Charges[1].J7_Percentage);
			AssertEquals("InvoiceLine.Charges[1].J7_Amount", 35m, InvoiceLine.Charges[1].J7_Amount);
			AssertEquals("InvoiceLine.Charges[1].J7_RX_NKCurrency", "ZZZ", InvoiceLine.Charges[1].J7_RX_NKCurrency);
		}

		public void TestPreferentialDetailsGetForcedToUpper()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_Tariff = "1010.10.10.10z";
			invoiceLine.JI_Description = "lower case";
			invoiceLine.JI_CustomsUnitQty = "aa";
			invoiceLine.JI_SupplementaryUQ = "bb";
			invoiceLine.JI_CountryOfOrigin = "cc";
			invoiceLine.JI_RN_NKCountryOfExport = "dd";
			invoiceLine.JI_QualifiesForPreferentialDuty = "e";
			invoiceLine.JI_ConcessionCode = "ff";

			AssertEquals("invoiceLine.JI_CustomsUnitQty", "AA", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("invoiceLine.JI_SupplementaryUQ", "BB", invoiceLine.JI_SupplementaryUQ);
			AssertEquals("invoiceLine.JI_CountryOfOrigin", "CC", invoiceLine.JI_CountryOfOrigin);
			AssertEquals("invoiceLine.JI_RN_NKCountryOfExport", "DD", invoiceLine.JI_RN_NKCountryOfExport);
			AssertEquals("invoiceLine.JI_QualifiesForPreferentialDuty", "E", invoiceLine.JI_QualifiesForPreferentialDuty);
			AssertEquals("invoiceLine.JI_ConcessionCode", "FF", invoiceLine.JI_ConcessionCode);
			AssertEquals("invoiceLine.JI_Tariff", "1010.10.10.10Z", invoiceLine.JI_Tariff);
			AssertEquals("invoiceLine.JI_Description", "LOWER CASE", invoiceLine.JI_Description);
		}

		public void TestPartType()
		{
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			OrgHeader importer = factory2.New<OrgHeader>();
			importer.FillWithValidTestData();
			Enterprise.MasterFiles.Business.OrgSupplierPart product = (Enterprise.MasterFiles.Business.OrgSupplierPart)factory2.New<Integration.Customs.AU.IOrgSupplierPart>();
			product.OP_PartNum = "TestTEST";
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			factory2.Save();

			GlbCompany nzCompany = Factory.New<GlbCompany>();
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			GlbBranch nzBranch = nzCompany.Branches.AddNew();
			nzBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.NewZealand)).RL_Code;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_GB = nzBranch.PK;
			JobComInvoiceLine invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "TestTEST";
			AssertEquals("Product type gets changed depending on who is requesting", typeof(OrgSupplierPart), invoiceLine.Part.GetType());

			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry("AU");
			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			JobDeclaration declarationLoaded = factory3.Load<JobDeclaration>(declaration.PK);
			AssertEquals("product type still the right type type", typeof(OrgSupplierPart), declarationLoaded.InvoiceLines[0].Part.GetType());
		}

		public override void TestMergedLineNumber()
		{
			AssertEquals("InvoiceLine.MergedLineNumber When Not Merged", InvoiceLine.MergedLineNumber, JobComInvoiceLine.MergedLineNumberWhenLineIsNotMerged);
			CusEntryLine entryLine = Declaration.CusEntryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 2;
			InvoiceLine.JI_CL = entryLine.PK;
			AssertEquals("InvoiceLine.MergedLineNumber When Merged", InvoiceLine.MergedLineNumber, "   2");
		}

		public void TestPetrolClassificationShowsGPBInDutyRateButNotAsSupplementaryQuantity()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_DateOfArrival = new ZDateTime(2005, 1, 1);
			InvoiceLine.JI_Tariff = "2710.19.29.11B";
			AssertEquals(StatisticalUQList.Codes.Litres, InvoiceLine.JI_CustomsUnitQty);
			AssertEquals("", InvoiceLine.JI_SupplementaryUQ);
			AssertEquals("2710.19.29.11B @ NML = 7.00% $0.3337/LTR $0.0800/GPB ACC:$0.0508/LTR", InvoiceLine.JI_DutyRateComplete);

			InvoiceLine.JI_Tariff = "2203.00.39.02K";
			AssertEquals(StatisticalUQList.Codes.LitresOfPureAlcohol, InvoiceLine.JI_CustomsUnitQty);
			AssertEquals(StatisticalUQList.Codes.Litres, InvoiceLine.JI_SupplementaryUQ);
			AssertEquals("2203.00.39.02K @ NML = $21.9820/LPA ALAC:$0.0117/LTR", InvoiceLine.JI_DutyRateComplete);
		}

		public void TestDutyRateOnApportionedCharges()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("DEM", 10000m, "NZ", new ZDateTime(2006, 1, 1), new ZDateTime(2079, 6, 6), "Deminimus");
			Factory.Save();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Excise;
			Declaration.JE_DateOfArrival = new ZDateTime(2006, 1, 1);

			CusEntryHeader entryHeader = Declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1100m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = "FOB";

			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine.PK;
			line1.JI_LinePrice = 1000m;
			line1.JI_Tariff = "2204.21.18.11A";
			line1.JI_CustomsQuantity = 100m;
			line1.JI_CustomsUnitQty = "LTR";
			line1.JI_QualifiesForPreferentialDuty = "N";
			line1.JI_CountryOfOrigin = "US";
			line1.JI_RN_NKCountryOfExport = "US";

			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_CL = entryLine.PK;
			line2.JI_LinePrice = 100m;
			line2.JI_Tariff = "2204.21.18.11A";
			line2.JI_CustomsQuantity = 1000m;
			line2.JI_CustomsUnitQty = "LTR";
			line2.JI_QualifiesForPreferentialDuty = "N";
			line2.JI_CountryOfOrigin = "US";
			line2.JI_RN_NKCountryOfExport = "US";

			entryLine.CalculateDutyAndGST();

			AssertEquals(295.92m, line1.JI_Calc_DutyAmount);
			AssertEquals(2266.2m, line2.JI_Calc_DutyAmount);

			entryLine.Fees.AddOrUpdate("GST", 50m);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			AssertEquals(ZDecimal.Zero, line1.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals(ZDecimal.Zero, line1.JI_Calc_GSTVATAmountIncludingWHEstimate);

			var otherInfo = line1.OtherInfos.AddNew();
			otherInfo.ZO_Code = "LVX";
			AssertEquals(295.92m, line1.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals(45.45454545m, line1.JI_Calc_GSTVATAmountIncludingWHEstimate);
		}

		public void TestBestPreferentialCountryGroup()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_DateOfArrival = new ZDateTime(2006, 1, 1);

			CusEntryHeader entryHeader = Declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1100m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = "FOB";

			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine.PK;
			line1.JI_LinePrice = 1000m;
			line1.JI_Tariff = "8413.81.19.00J";
			line1.JI_CustomsQuantity = 100m;
			line1.JI_CustomsUnitQty = "LTR";
			line1.JI_QualifiesForPreferentialDuty = "Q";
			line1.JI_CountryOfOrigin = "AU";
			line1.JI_RN_NKCountryOfExport = "AU";
			AssertEquals("BestPreferentialCountryGroup for line 1 should default from best rate search", "AU", line1.JI_BestPreferentialCountryGroup);

			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_CL = entryLine.PK;
			line2.JI_LinePrice = 100m;
			line2.JI_Tariff = "8413.81.19.00J";
			line2.JI_CustomsQuantity = 1000m;
			line2.JI_CustomsUnitQty = "LTR";
			line2.JI_QualifiesForPreferentialDuty = "N";
			line2.JI_CountryOfOrigin = "AU";
			line2.JI_RN_NKCountryOfExport = "AU";
			AssertEquals("BestPreferentialCountryGroup for line 2 should be Normal rate", "NML", line2.JI_BestPreferentialCountryGroup);
		}

		public void TestJI_LevyCreditAmountCode()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("JI_LevyCreditAmountCode", LevyCodesList.Codes.ALAC, InvoiceLine.JI_LevyCreditAmountCode);
			AssertEquals("JI_LevyCreditAmountCode should not be read only", false, InvoiceLine.JI_LevyCreditAmountCodeInfo.ReadOnly);

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			AssertEquals("JI_LevyCreditAmountCode", LevyCodesList.Codes.ALAC, InvoiceLine.JI_LevyCreditAmountCode);
			AssertEquals("JI_LevyCreditAmountCode should be read only", true, InvoiceLine.JI_LevyCreditAmountCodeInfo.ReadOnly);
		}

		public void TestIsExportDrawbackOrCompletion()
		{
			AssertEquals("Pre-condition: IsExportDrawbackOrCompletion", false, InvoiceLine.IsExportDrawbackOrCompletion);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("IsExportDrawbackOrCompletion", false, InvoiceLine.IsExportDrawbackOrCompletion);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			AssertEquals("IsExportDrawbackOrCompletion", true, InvoiceLine.IsExportDrawbackOrCompletion);
		}

		public void TestRequiresPackingLine()
		{
			AssertEquals("Pre-condition: RequiresPackingLine", false, InvoiceLine.RequiresPackingLine);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("TSW Export RequiresPackingLine", true, InvoiceLine.RequiresPackingLine);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Periodic;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("TSW Import Periodic entry now RequiresPackingLine - WI00091070", true, InvoiceLine.RequiresPackingLine);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("TSW Export write-off entry RequiresPackingLine", false, InvoiceLine.RequiresPackingLine);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("TSW Import entry RequiresPackingLine", true, InvoiceLine.RequiresPackingLine);

			var container = Declaration.CusContainers.AddNew();
			AssertEquals("TSW Import entry RequiresPackingLine", true, InvoiceLine.RequiresPackingLine);
			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			AssertEquals("TSW Import empty container entry RequiresPackingLine", false, InvoiceLine.RequiresPackingLine);
		}

		public void TestHasPackingLineIfRequired()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			AssertEquals("Pre-condition: RequiresPackingLine", false, InvoiceLine.RequiresPackingLine);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			var invLine = Declaration.InvoiceLines.AddNew();
			AssertEquals("TSW Export RequiresPackingLine", true, invLine.RequiresPackingLine);
			invLine.JI_InvoiceQuantity = 10;
			AssertEquals("ItemPackages for invoice line has been created", true, invLine.ItemPackages.Count > 0);
		}

		public void TestNumberOfPackages1()
		{
			InvoiceLine.ItemPackages.AddNew();
			AssertEquals("NumberOfPackages1 default", 0, InvoiceLine.NumberOfPackages1);
			InvoiceLine.NumberOfPackages1 = 25;
			AssertEquals("NumberOfPackages1 entered", 25, InvoiceLine.NumberOfPackages1);
		}

		public void TestPackages1UQ()
		{
			InvoiceLine.ItemPackages.AddNew();
			AssertEquals("Packages1UQ default", "PK", InvoiceLine.Packages1UQ);
			InvoiceLine.Packages1UQ = "CT";
			AssertEquals("Packages1UQ entered", "CT", InvoiceLine.Packages1UQ);
		}

		public void TestPackagesVolume1()
		{
			InvoiceLine.ItemPackages.AddNew();
			AssertEquals("PackagesVolume1 default", 0M, InvoiceLine.PackagesVolume1);
			InvoiceLine.PackagesVolume1 = 2.875m;
			AssertEquals("PackagesVolume1 entered", 2.875M, InvoiceLine.PackagesVolume1);
		}

		public void TestPackagingMarks1()
		{
			InvoiceLine.ItemPackages.AddNew();
			AssertEquals("PackagingMarks1 should default in first instance", "Unknown", InvoiceLine.PackagingMarks1);
			InvoiceLine.PackagingMarks1 = "L-3992377/XP";
			AssertEquals("PackagingMarks1 default value can be overriden", "L-3992377/XP", InvoiceLine.PackagingMarks1);
		}

		public void TestPackagingMaterial1()
		{
			InvoiceLine.ItemPackages.AddNew();
			AssertEquals("PackagingMaterial1 defaults as empty", "", InvoiceLine.PackagingMaterial1);
			InvoiceLine.PackagingMaterial1 = "STRAW";
			AssertEquals("PackagingMaterial1 can be overriden", "STRAW", InvoiceLine.PackagingMaterial1);
		}

		public void TestIsTSWCRE()
		{
			AssertEquals("Pre-condition: default set up - IsTSWCRE", false, InvoiceLine.IsTSWCRE);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsTSWCRE", false, InvoiceLine.IsTSWCRE);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsTSWCRE", true, InvoiceLine.IsTSWCRE);
		}

		public void TestCustomsWeight()
		{
			InvoiceLine.JI_CustomsQuantity = 23.00m;

			InvoiceLine.JI_CustomsUnitQty = StatisticalUQList.Codes.Grams;
			AssertEquals(new ZWeight(23.00m, Core.Constants.Weight.Grams), InvoiceLine.CustomsWeight);

			InvoiceLine.JI_CustomsUnitQty = StatisticalUQList.Codes.Kilograms;
			AssertEquals(new ZWeight(23.00m, Core.Constants.Weight.Kilograms), InvoiceLine.CustomsWeight);

			InvoiceLine.JI_CustomsUnitQty = StatisticalUQList.Codes.Tonnes;
			AssertEquals(new ZWeight(23.00m, Core.Constants.Weight.Tonnes), InvoiceLine.CustomsWeight);

			InvoiceLine.JI_CustomsUnitQty = StatisticalUQList.Codes.KilogramsOfPureTobaccoContent;
			AssertEquals(ZWeight.Empty, InvoiceLine.CustomsWeight);
		}

		public void TestITariffValidationData()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_DateOfArrival = new ZDateTime(2002, 1, 2);
			InvoiceLine.JI_Tariff = Tariff1.U0_Tariff;
			InvoiceLine.JI_PartsOfClassification = Tariff2.U0_Tariff;
			PermitCode permit1 = InvoiceLine.PermitCodes.AddNew();
			PermitCode permit2 = InvoiceLine.PermitCodes.AddNew();
			PermitCode permit3 = Declaration.PermitCodes.AddNew();
			ITariffValidationData tariffValidation = InvoiceLine;
			AssertNotNull("Classification as ITariffValidationData", tariffValidation);
			AssertEquals("tariffValidation.AllowableTariffCodeTypes.Import", true, tariffValidation.AllowableTariffCodeTypes.Import);
			AssertEquals("tariffValidation.AllowableTariffCodeTypes.Export", false, tariffValidation.AllowableTariffCodeTypes.Export);
			AssertEquals("tariffValidation.AllowableTariffCodeTypes.Exc", false, tariffValidation.AllowableTariffCodeTypes.Excise);
			AssertEquals("tariffValidation.DateForDutyRate", new ZDateTime(2002, 1, 2), tariffValidation.DateForDutyRate);
			AssertEquals("tariffValidation.EmptyTariffIsFullError", false, tariffValidation.EmptyTariffIsFullError);
			AssertEquals("tariffValidation.EmptyTariffIsAllowed", false, tariffValidation.EmptyTariffIsAllowed);
			AssertEquals("tariffValidation.Factory", InvoiceLine.Factory, tariffValidation.Factory);
			AssertEquals("tariffValidation.PartsOfTariffBO", Tariff2, tariffValidation.PartsOfTariffBO);
			AssertEquals("tariffValidation.PartsOfTariffCode", Tariff2.U0_Tariff, tariffValidation.PartsOfTariffCode);
			AssertEquals("tariffValidation.PartsOfTariffCodeInfo", InvoiceLine.JI_PartsOfClassificationInfo, tariffValidation.PartsOfTariffCodeInfo);
			AssertEquals("tariffValidation.PermitCodeCount", 3, tariffValidation.PermitCodeCount);
			AssertEquals("tariffValidation.TariffBO", Tariff1, tariffValidation.TariffBO);
			AssertEquals("tariffValidation.TariffCode", Tariff1.U0_Tariff, tariffValidation.TariffCode);
			AssertEquals("tariffValidation.TariffCodeInfo", InvoiceLine.JI_TariffInfo, tariffValidation.TariffCodeInfo);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			tariffValidation = InvoiceLine;
			AssertEquals("EmptyTariffIsAllowed for TSW CRE entries", true, tariffValidation.EmptyTariffIsAllowed);
		}

		public void TestPartsOfTariffBO()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_DateOfArrival = new ZDateTime(2004, 12, 12);
				JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
				JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

				NZCClassification manualClass = Factory.New<NZCClassification>();
				manualClass.U0_Tariff = "0000.00.00.00Y";
				manualClass.U0_DateActiveFrom = new ZDateTime(2004, 1, 1);
				manualClass.U0_IsManual = true;

				NZCClassification normalClass = Factory.New<NZCClassification>();
				normalClass.U0_Tariff = "0000.00.00.00N";
				normalClass.U0_DateActiveFrom = new ZDateTime(2004, 1, 1);
				normalClass.U0_IsManual = false;

				AssertEquals("InvoiceLine.PartsOfTariffBO", null, ((ITariffValidationData)invoiceLine).PartsOfTariffBO);

				invoiceLine.JI_PartsOfClassification = "1212";
				AssertEquals("InvoiceLine.PartsOfTariffBO", null, ((ITariffValidationData)invoiceLine).PartsOfTariffBO);

				invoiceLine.JI_PartsOfClassification = manualClass.U0_Tariff;
				AssertEquals("InvoiceLine.PartsOfTariffBO", manualClass, ((ITariffValidationData)invoiceLine).PartsOfTariffBO);

				invoiceLine.JI_PartsOfClassification = normalClass.U0_Tariff;
				AssertEquals("InvoiceLine.PartsOfTariffBO", normalClass, ((ITariffValidationData)invoiceLine).PartsOfTariffBO);

				invoiceLine.JI_PartsOfClassification = "";
				AssertEquals("InvoiceLine.PartsOfTariffBO", null, ((ITariffValidationData)invoiceLine).PartsOfTariffBO);

				declaration.JE_DateOfArrival = new ZDateTime(2002, 1, 1);
				invoiceLine.JI_PartsOfClassification = normalClass.U0_Tariff;
				AssertEquals("InvoiceLine.PartsOfTariffBO", null, ((ITariffValidationData)invoiceLine).PartsOfTariffBO);
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var tariff = UniversalTariffHelperTest.SetupTariffData(Factory);

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_DateOfArrival = ZDateTime.Today;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

				AssertEquals("InvoiceLine.PartsOfTariffBO", null, ((ITariffValidationData)invoiceLine).PartsOfTariffBO);

				invoiceLine.JI_PartsOfClassification = "1212";
				AssertEquals("InvoiceLine.PartsOfTariffBO", null, ((ITariffValidationData)invoiceLine).PartsOfTariffBO);

				invoiceLine.JI_PartsOfClassification = "123456789";
				AssertEquals("InvoiceLine.PartsOfTariffBO", tariff, ((ITariffValidationData)invoiceLine).PartsOfTariffBO);

				invoiceLine.JI_PartsOfClassification = "1234.56.78.9";
				AssertEquals("InvoiceLine.PartsOfTariffBO", tariff, ((ITariffValidationData)invoiceLine).PartsOfTariffBO);

				invoiceLine.JI_PartsOfClassification = "";
				AssertEquals("InvoiceLine.PartsOfTariffBO", null, ((ITariffValidationData)invoiceLine).PartsOfTariffBO);

				declaration.JE_DateOfArrival = new ZDateTime(2002, 1, 1);
				invoiceLine.JI_PartsOfClassification = "123456789";
				AssertEquals("InvoiceLine.PartsOfTariffBO", null, ((ITariffValidationData)invoiceLine).PartsOfTariffBO);
			}
		}

		public void TestAdditionalDataForBorderWise()
		{
			Declaration.JE_MessageType = Business.JobMessageTypeList.Codes.Import;
			Declaration.JE_DateOfArrival = new ZDateTime(2005, 12, 11);
			IHaveAdditionalDataForBorderWise additionalDataSource = InvoiceLine;
			AdditionalDataForBorderWise additionalData = additionalDataSource.GetAdditionalDataForBorderWise("");
			AssertEquals("AdditionalData.ParameterForBorderWise", "I", additionalData.ParameterForBorderWise);
			AssertEquals("AdditionalData.DateForDutyRate", new ZDateTime(2005, 12, 11), additionalData.DateForDutyRate);
		}

		public void TestEffectiveCountryOfExport()
		{
			RefCountry australia = RefCountry.LoadFromCountryCode(Factory, "AU");
			RefCountry singapore = RefCountry.LoadFromCountryCode(Factory, "SG");

			AssertEquals("Precondition: InvoiceLine.JI_RN_NKEffectiveCountryOfExport", ZString.Empty, InvoiceLine.JI_RN_NKEffectiveCountryOfExport);
			AssertEquals("Precondition: InvoiceLine.EffectiveCountryOfExport", null, InvoiceLine.EffectiveCountryOfExport);

			InvoiceHeader.JZ_RN_NKDefaultExport = australia.RN_Code;
			AssertEquals("InvoiceLine.JI_RN_NKEffectiveCountryOfExport", australia.RN_Code, InvoiceLine.JI_RN_NKEffectiveCountryOfExport);
			AssertEquals("InvoiceLine.EffectiveCountryOfExport", australia, InvoiceLine.EffectiveCountryOfExport);

			InvoiceLine.JI_RN_NKCountryOfExport = singapore.RN_Code;
			AssertEquals("InvoiceLine.JI_RN_NKEffectiveCountryOfExport", singapore.RN_Code, InvoiceLine.JI_RN_NKEffectiveCountryOfExport);
			AssertEquals("InvoiceLine.EffectiveCountryOfExport", singapore, InvoiceLine.EffectiveCountryOfExport);

			InvoiceLine.JI_RN_NKCountryOfExport = "XL";
			AssertEquals("Even if the line value is valid, it should still take precedence.", "XL", InvoiceLine.JI_RN_NKEffectiveCountryOfExport);
			AssertEquals("InvoiceLine.EffectiveCountryOfExport", null, InvoiceLine.EffectiveCountryOfExport);

			InvoiceHeader.JZ_RN_NKDefaultExport = "XH";
			AssertEquals("Should not change when the Header value changes if it was filled in on the Line.", "XL", InvoiceLine.JI_RN_NKEffectiveCountryOfExport);
			AssertEquals("InvoiceLine.EffectiveCountryOfExport", null, InvoiceLine.EffectiveCountryOfExport);

			InvoiceLine.JI_RN_NKCountryOfExport = ZString.Empty;
			AssertEquals("Should drop back to the Header level when Line value is removed, even if the Header value is invalid.", "XH", InvoiceLine.JI_RN_NKEffectiveCountryOfExport);
			AssertEquals("InvoiceLine.EffectiveCountryOfExport", null, InvoiceLine.EffectiveCountryOfExport);

			InvoiceLine.JI_RN_NKCountryOfExport = australia.RN_Code;
			AssertEquals("InvoiceLine.JI_RN_NKEffectiveCountryOfExport", australia.RN_Code, InvoiceLine.JI_RN_NKEffectiveCountryOfExport);
			AssertEquals("InvoiceLine.EffectiveCountryOfExport", australia, InvoiceLine.EffectiveCountryOfExport);
		}

		public new void TestEffectiveCountryOfOrigin()
		{
			RefCountry australia = RefCountry.LoadFromCountryCode(Factory, "AU");
			RefCountry singapore = RefCountry.LoadFromCountryCode(Factory, "SG");

			AssertEquals("Precondition: InvoiceLine.JI_RN_NKEffectiveCountryOfOrigin", ZString.Empty, InvoiceLine.JI_RN_NKEffectiveCountryOfOrigin);
			AssertEquals("Precondition: InvoiceLine.EffectiveCountryOfOrigin", null, InvoiceLine.EffectiveCountryOfOriginRefCountry);

			InvoiceHeader.JZ_RN_NKDefaultOrigin = australia.RN_Code;
			AssertEquals("InvoiceLine.JI_RN_NKEffectiveCountryOfOrigin", australia.RN_Code, InvoiceLine.JI_RN_NKEffectiveCountryOfOrigin);
			AssertEquals("InvoiceLine.EffectiveCountryOfOrigin", australia, InvoiceLine.EffectiveCountryOfOriginRefCountry);

			InvoiceLine.JI_CountryOfOrigin = singapore.RN_Code;
			AssertEquals("InvoiceLine.JI_RN_NKEffectiveCountryOfOrigin", singapore.RN_Code, InvoiceLine.JI_RN_NKEffectiveCountryOfOrigin);
			AssertEquals("InvoiceLine.EffectiveCountryOfOrigin", singapore, InvoiceLine.EffectiveCountryOfOriginRefCountry);

			InvoiceLine.JI_CountryOfOrigin = "XL";
			AssertEquals("Even if the line value is valid, it should still take precedence.", "XL", InvoiceLine.JI_RN_NKEffectiveCountryOfOrigin);
			AssertEquals("InvoiceLine.EffectiveCountryOfOrigin", null, InvoiceLine.EffectiveCountryOfOriginRefCountry);

			InvoiceHeader.JZ_RN_NKDefaultOrigin = "XH";
			AssertEquals("Should not change when the Header value changes if it was filled in on the Line.", "XL", InvoiceLine.JI_RN_NKEffectiveCountryOfOrigin);
			AssertEquals("InvoiceLine.EffectiveCountryOfOrigin", null, InvoiceLine.EffectiveCountryOfOriginRefCountry);

			InvoiceLine.JI_CountryOfOrigin = ZString.Empty;
			AssertEquals("Should drop back to the Header level when Line value is removed, even if the Header value is invalid.", "XH", InvoiceLine.JI_RN_NKEffectiveCountryOfOrigin);
			AssertEquals("InvoiceLine.EffectiveCountryOfOrigin", null, InvoiceLine.EffectiveCountryOfOriginRefCountry);

			InvoiceLine.JI_CountryOfOrigin = australia.RN_Code;
			AssertEquals("InvoiceLine.JI_RN_NKEffectiveCountryOfOrigin", australia.RN_Code, InvoiceLine.JI_RN_NKEffectiveCountryOfOrigin);
			AssertEquals("InvoiceLine.EffectiveCountryOfOrigin", australia, InvoiceLine.EffectiveCountryOfOriginRefCountry);
		}

		public void TestEffectOriginRegion()
		{
			AssertEquals("Precondition: InvoiceLine.JI_EffectiveOriginRegion", ZString.Empty, InvoiceLine.JI_EffectiveOriginRegion);

			InvoiceHeader.JZ_DefaultOriginRegion = "XXXXXXXXX";
			AssertEquals("InvoiceLine.JI_EffectiveOriginRegion", "XXXXXXXXX", InvoiceLine.JI_EffectiveOriginRegion);

			InvoiceLine.JI_OriginRegion = "YYYYYYYYY";
			AssertEquals("InvoiceLine.JI_EffectiveOriginRegion", "YYYYYYYYY", InvoiceLine.JI_EffectiveOriginRegion);

			InvoiceLine.JI_OriginRegion = "XL";
			AssertEquals("Even if the line value is valid, it should still take precedence.", "XL", InvoiceLine.JI_EffectiveOriginRegion);

			InvoiceHeader.JZ_DefaultOriginRegion = "XH";
			AssertEquals("Should not change when the Header value changes if it was filled in on the Line.", "XL", InvoiceLine.JI_EffectiveOriginRegion);

			InvoiceLine.JI_OriginRegion = ZString.Empty;
			AssertEquals("Should drop back to the Header level when Line value is removed, even if the Header value is invalid.", "XH", InvoiceLine.JI_EffectiveOriginRegion);

			InvoiceLine.JI_OriginRegion = "XXXXXXXXX";
			AssertEquals("InvoiceLine.JI_EffectiveOriginRegion", "XXXXXXXXX", InvoiceLine.JI_EffectiveOriginRegion);
		}

		public void TestEffectiveQualifiesForPreferentialDuty()
		{
			AssertEquals("Precondition: InvoiceLine.JI_EffectiveQualifiesForPreferentialDuty", ZString.Empty, InvoiceLine.JI_EffectiveQualifiesForPreferentialDuty);
			InvoiceHeader.JZ_DefaultQualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.NonQualifying;
			AssertEquals("InvoiceLine.JI_EffectiveQualifiesForPreferentialDuty", QualifiesForPreferentialDutyList.Codes.NonQualifying, InvoiceLine.JI_EffectiveQualifiesForPreferentialDuty);
			InvoiceLine.JI_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.Qualifies;
			AssertEquals("InvoiceLine.JI_EffectiveQualifiesForPreferentialDuty", QualifiesForPreferentialDutyList.Codes.Qualifies, InvoiceLine.JI_EffectiveQualifiesForPreferentialDuty);
			InvoiceLine.JI_QualifiesForPreferentialDuty = "Z";
			AssertEquals("Even if the line value is valid, it should still take precedence.", "Z", InvoiceLine.JI_EffectiveQualifiesForPreferentialDuty);
			InvoiceHeader.JZ_DefaultQualifiesForPreferentialDuty = "X";
			AssertEquals("Should not change when the Header value changes if it was filled in on the Line.", "Z", InvoiceLine.JI_EffectiveQualifiesForPreferentialDuty);
			InvoiceLine.JI_QualifiesForPreferentialDuty = ZString.Empty;
			AssertEquals("Should drop back to the Header level when Line value is removed, even if the Header value is invalid.", "X", InvoiceLine.JI_EffectiveQualifiesForPreferentialDuty);
		}

		public void TestTariffDescription()
		{
			InvoiceLine.JI_Tariff = Tariff1.U0_Tariff;
			AssertEquals("InvoiceLine.TariffDescription", Tariff1.U0_Description, InvoiceLine.TariffDescription);
			InvoiceLine.JI_Tariff = Tariff2.U0_Tariff;
			AssertEquals("InvoiceLine.TariffDescription", Tariff2.U0_Description, InvoiceLine.TariffDescription);
		}

		[TestDate(2006, 1, 1)]
		public void TestIUltimateDistributeeTestNZ()
		{
			TaxOrFeeTestHelper.SetUp(Factory);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_DateOfArrival = new ZDateTime(2006, 1, 1);

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 20000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = "FOB";

			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 15000m;
			line1.JI_Tariff = "2204.21.18.11A";
			line1.JI_CustomsQuantity = 1000m;
			line1.JI_CustomsUnitQty = "LTR";
			line1.JI_QualifiesForPreferentialDuty = "N";
			line1.JI_CountryOfOrigin = "US";
			line1.JI_RN_NKCountryOfExport = "US";

			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 5000m;
			line2.JI_Tariff = "2204.21.18.11A";
			line2.JI_CustomsQuantity = 10m;
			line2.JI_CustomsUnitQty = "LTR";
			line2.JI_QualifiesForPreferentialDuty = "N";
			line2.JI_CountryOfOrigin = "US";
			line2.JI_RN_NKCountryOfExport = "US";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("one entry", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("one entry line", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			line1.JI_AntiDumpingDutyAmount = 150m;
			line1.JI_CountervailingDutyAmount = 250m;

			DutyTaxEntryFee line1Result = ((IUltimateDistributee)line1).LineDutyTaxEntryFeeItems;
			DutyTaxEntryFee line2Result = ((IUltimateDistributee)line2).LineDutyTaxEntryFeeItems;

			CombineAssertions(delegate
			{
				AssertEquals("IUltimateDistributee.EntryFee", 21.75m, line1Result["ENT"]);
				AssertEquals("IUltimateDistributee.EntryFee", 7.25m, line2Result["ENT"]);
				AssertEquals("IUltimateDistributee.TotalDuty taken from JI_Calc_DutyAmount", 3309.20m, line1Result["TDT"]);
				AssertEquals("IUltimateDistributee.TotalDuty taken from JI_Calc_DutyAmount", 372.59m, line2Result["TDT"]);
				AssertEquals("IUltimateDistributee.SpecialTax1", 49.30m, line1Result["ST1"]);
				AssertEquals("IUltimateDistributee.SpecialTax1", 0.49m, line2Result["ST1"]);
			});
		}

		[TestDate(2006, 1, 1)]
		public void TestSGGIsIncludedInLandedCosting()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_DateOfArrival = new ZDateTime(2013, 1, 1);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 20000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = "FOB";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5000m;
			var tariff = NZCClassificationTest.CreateSGGTariff(Factory);
			invoiceLine.JI_Tariff = tariff.U0_Tariff;
			invoiceLine.JI_CustomsQuantity = 20m;
			invoiceLine.JI_CustomsUnitQty = tariff.U0_StatisticalUnit;
			invoiceLine.JI_QualifiesForPreferentialDuty = "N";
			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLine.JI_RN_NKCountryOfExport = "US";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var invoiceLineResult = ((IUltimateDistributee)invoiceLine).LineDutyTaxEntryFeeItems;
			AssertEquals("IUltimateDistributee.OtherDuties", 171m, invoiceLineResult["OTH"]);
		}

		public override void TestMakeCustomsQuantityReadOnly()
		{
			InvoiceLine.JI_Tariff = "";
			AssertEquals("There is no tariff and CustomsQuantity should be readonly", true, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);

			InvoiceLine.JI_Tariff = "00000000 00";
			AssertEquals("Invalid tariff and there is no Customs UQ involved", true, InvoiceLine.JI_CustomsUnitQty.IsEmpty);

			InvoiceLine.JI_CustomsUnitQty = "NO";
			AssertEquals("Customs unit qty exists and Qty field should be open", false, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		public override void TestCustomsQuantityIsReadonlyWhenUnitQtyEmpty()
		{
			InvoiceLine.JI_Tariff = "";
			AssertEquals("", InvoiceLine.JI_CustomsUnitQty);
			AssertEquals("Customs UQ is readonly", true, InvoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
			AssertEquals("Customs Quantity is readonly", true, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);

			InvoiceLine.JI_Tariff = "6103.41.00.11K";//Statistical Unit = "NMB"
			AssertEquals("NMB", InvoiceLine.JI_CustomsUnitQty);
			AssertEquals("Customs UQ is readonly", true, InvoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
			AssertEquals("Customs Quantity is readonly", false, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		public void TestDateForDutyRate()
		{
			ZDateTime testDate = new ZDateTime(2004, 12, 12);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("Precondition: InvoiceLine.EffectiveDateForDutyRate different from Test Date being used", InvoiceLine.EffectiveDateForDutyRate != testDate);
			Declaration.JE_DateOfArrival = testDate;
			AssertEquals("InvoiceLine.EffectiveDateForDutyRate", testDate, InvoiceLine.EffectiveDateForDutyRate);
		}

		public void TestJI_DescriptionIsUpperCase_NZCustomsRequirement()
		{
			InvoiceLine.JI_Description = "test";
			AssertEquals("TEST", InvoiceLine.JI_Description);
		}

		public void TestCustomsValueInLocalCurrencyRounded()
		{
			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceHeader.JZ_IncoTerm = "FOB";
			InvoiceHeader.JZ_InvoiceAmount = 10000.00m;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = "AUD";
			InvoiceLine.JI_LinePrice = 10000.00m;
			AssertEquals("Resulting Customs Value", 11111.00m, InvoiceLine.CustomsValueInLocalCurrencyRounded);
		}

		public void TestOverseasFreightInLocalCurrencyRounded()
		{
			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceHeader.JZ_IncoTerm = "FOB";
			InvoiceHeader.JZ_InvoiceAmount = 10000.00m;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = "AUD";
			InvoiceCharge oSFrtCharge = InvoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000m, "AUD");
			InvoiceLine.JI_LinePrice = 10000.00m;
			Declaration.ResumeApportionment();

			AssertEquals("Resulting Freight Value", 1111.00m, InvoiceLine.OverseasFreightInLocalCurrencyRounded);
		}

		public void TestOverseasInsuranceInLocalCurrencyRounded()
		{
			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceHeader.JZ_IncoTerm = "FOB";
			InvoiceHeader.JZ_InvoiceAmount = 10000.00m;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = "AUD";
			InvoiceCharge oSInsCharge = InvoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10m, "AUD");
			InvoiceLine.JI_LinePrice = 10000.00m;
			Declaration.ResumeApportionment();
			AssertEquals("Resulting Insurance Value", 11.00m, InvoiceLine.OverseasInsuranceInLocalCurrencyRounded);
		}

		public void TestPermitCodes()
		{
			AssertNotNull(InvoiceLine.PermitCodes);
		}

		public void TestHasFSAPermit()
		{
			var permitCode = InvoiceLine.PermitCodes.AddNew();
			AssertEquals(false, InvoiceLine.HasFSAPermit);

			permitCode.ZO_Code = PermitCodeList.Codes.NZFoodSafetyAuthorityNew;
			AssertEquals("Only has FSA permit when data is entered", false, InvoiceLine.HasFSAPermit);

			permitCode.ZO_Data = "Y93287";
			AssertEquals(true, InvoiceLine.HasFSAPermit);
		}

		public void TestOtherInfos()
		{
			AssertNotNull(InvoiceLine.OtherInfos);
		}

		public void TestProhibitedCodes()
		{
			AssertNotNull(InvoiceLine.ProhibitedCodes);
		}

		public void TestConcreteInvoiceHeader()
		{
			AssertEquals(InvoiceHeader, InvoiceLine.InvoiceHeader);
		}

		public void TestIsContainerLinkMandatory()
		{
			AssertEquals("IsContainerLinkMandatory", false, InvoiceLine.IsContainerLinkMandatory);
		}

		public void TestAddInfoOnSaving()
		{
			AssertEquals("JI_AddInfo should be empty", ZString.Empty, InvoiceLine.JI_AddInfo);
			InvoiceLine.JI_AntiDumpingDutyAmount = 500;
			Factory.Save();
			AssertEquals("JI_AddInfo", "AntiDumpingDuty=500", InvoiceLine.JI_AddInfo);
		}

		public void TestAddInfoOnLoaded()
		{
			InvoiceLine.JI_AntiDumpingDutyAmount = 500;
			Factory.Save();

			var invoiceLineLoaded = Factory.Load<JobComInvoiceLine>(InvoiceLine.PK);
			AssertEquals("InvoiceLine addinfo fields loaded", 500m, invoiceLineLoaded.JI_AntiDumpingDutyAmount);
		}

		public void TestAddInfoHasChanges()
		{
			InvoiceLine.JI_AntiDumpingDutyAmount = 500;
			AssertEquals("HasChanges", true, InvoiceLine.HasChanges);
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobComInvoiceLine invoiceLineLoaded = newFactory.Load<JobComInvoiceLine>(InvoiceLine.PK);
			AssertEquals("HasChanges", false, invoiceLineLoaded.HasChanges);
		}

		public void TestGetChildLines()
		{
			JobComInvoiceLine line2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			line2.JI_JI_ParentLine = InvoiceLine.PK;
			AssertEquals("Line 1's child", line2, InvoiceLine.ChildLines[0]);
		}

		public void TestGetParentLine()
		{
			JobComInvoiceLine line2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			line2.JI_JI_ParentLine = InvoiceLine.PK;
			AssertEquals("Line 2's parent", InvoiceLine, line2.ParentLine);
			AssertEquals("Line 2's parent", true, InvoiceLine.ChildLines.Contains(line2));
		}

		public void TestParentLineOnLoaded()
		{
			JobComInvoiceLine line2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			InvoiceLine.JI_ParentLineNo = line2.JI_LineNoString;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobComInvoiceLine lineLoaded = newFactory.Load<JobComInvoiceLine>(InvoiceLine.PK);
			AssertEquals("Parent Line no initialsed on loaded", line2.JI_LineNoString, lineLoaded.JI_ParentLineNo);
		}

		public void TestFormatJI_PartsOfClassification()
		{
			InvoiceLine.JI_PartsOfClassification = "4016997900J";
			AssertEquals("Formatted", "4016.99.79.00J", InvoiceLine.JI_PartsOfClassification);
		}

		public void TestValidateJI_PartsOfClassification()
		{
			InvoiceLine.JI_PartsOfClassification = "0101.10.00.11E";
			AssertEquals("This is not a parts of classification", true, InvoiceLine.JI_PartsOfClassificationInfo.HasMessageErrors());
		}

		public void TestSettingClassificationWithPartsOfSetsInInvoiceLine()
		{
			CusClassification @class = Factory.New<CusClassification>();
			@class.CC_TariffNum = "8431.49.01.00B";
			@class.CC_PartsOfClassification = "8431.49.19.00J";
			@class.CC_Description = "lol hay guys";

			InvoiceLine.JI_CC = @class.PK;

			AssertEquals("Tariff has been set", @class.CC_TariffNum, InvoiceLine.JI_Tariff);
			AssertEquals("Parts of classification set", @class.CC_PartsOfClassification, InvoiceLine.JI_PartsOfClassification);
			AssertEquals("LOL HAY GUYS", InvoiceLine.JI_Description);
		}

		public void TestSettingClassificationWithPartsOfSetsInInvoiceLineWithExistingDescription()
		{
			CusClassification class1 = Factory.New<CusClassification>();
			class1.CC_TariffNum = "8431.49.01.00B";
			class1.CC_PartsOfClassification = "8431.49.19.00J";
			class1.CC_Description = "lol hay guys";

			InvoiceLine.JI_Description = "OMG WTF";
			InvoiceLine.JI_CC = class1.PK;

			AssertEquals("Tariff has been set", class1.CC_TariffNum, InvoiceLine.JI_Tariff);
			AssertEquals("Parts of classification set", class1.CC_PartsOfClassification, InvoiceLine.JI_PartsOfClassification);
			AssertEquals("OMG WTF", InvoiceLine.JI_Description);
		}

		public void TestSettingClassificationWithPartsOfSetsInInvoiceLineWithExistingDescriptionFromOldTariff()
		{
			CusClassification class1 = Factory.New<CusClassification>();
			class1.CC_TariffNum = "8431.49.01.00B";
			class1.CC_PartsOfClassification = "8431.49.19.00J";
			class1.CC_Description = "lol hay guys";

			InvoiceLine.JI_Tariff = Tariff1.U0_Tariff;
			AssertEquals("InvoiceLine.TariffDescription", Tariff1.U0_Description, InvoiceLine.TariffDescription);

			InvoiceLine.JI_CC = class1.PK;

			AssertEquals("Tariff has been set", class1.CC_TariffNum, InvoiceLine.JI_Tariff);
			AssertEquals("Parts of classification set", class1.CC_PartsOfClassification, InvoiceLine.JI_PartsOfClassification);
			AssertEquals("LOL HAY GUYS", InvoiceLine.JI_Description);
		}

		public void TestSettingDescriptionWhenChangingTariff()
		{
			NZCClassification tariff1 = NZCClassification.New(Factory);
			tariff1.U0_Tariff = "1111.11.11.11K";
			tariff1.U0_Description = "Tariff_Description_1";
			tariff1.U0_DateActiveFrom = new ZDateTime(1980, 1, 1, 0, 0, 0);
			tariff1.U0_DateActiveTo = new ZDateTime(3000, 12, 31, 0, 0, 0);

			NZCClassification tariff2 = NZCClassification.New(Factory);
			tariff2.U0_Tariff = "2222.22.22.22K";
			tariff2.U0_Description = "Tariff_Description_2";
			tariff2.U0_DateActiveFrom = new ZDateTime(1980, 2, 2, 0, 0, 0);
			tariff2.U0_DateActiveTo = new ZDateTime(3000, 12, 31, 0, 0, 0);

			InvoiceLine.JI_Tariff = tariff1.U0_Tariff;
			AssertEquals("InvoiceLine.TariffDescription", "TARIFF_DESCRIPTION_1", InvoiceLine.JI_Description);
			InvoiceLine.JI_Tariff = tariff2.U0_Tariff;
			AssertEquals("InvoiceLine.TarrifDescription", "TARIFF_DESCRIPTION_2", InvoiceLine.JI_Description);
		}

		public void TestSettingDescriptionWhenChangingClassificationWithExistingTariff()
		{
			NZCClassification tariff1 = NZCClassification.New(Factory);
			tariff1.U0_Tariff = "1111.11.11.11K";
			tariff1.U0_Description = "Tariff_Description_1";
			tariff1.U0_DateActiveFrom = new ZDateTime(1980, 1, 1, 0, 0, 0);
			tariff1.U0_DateActiveTo = new ZDateTime(3000, 12, 31, 0, 0, 0);

			CusClassification class1 = Factory.New<CusClassification>();
			class1.CC_TariffNum = "8431.49.01.00B";
			class1.CC_PartsOfClassification = "8431.49.19.00J";
			class1.CC_Description = "lol hay guys";

			InvoiceLine.JI_Tariff = tariff1.U0_Tariff;
			AssertEquals("InvoiceLine.TariffDescription", "TARIFF_DESCRIPTION_1", InvoiceLine.JI_Description);
			InvoiceLine.JI_CC = class1.PK;
			AssertEquals("InvoiceLine.TarrifDescription", "LOL HAY GUYS", InvoiceLine.JI_Description);
		}

		public void TestClassificationInfoBroughtToInvoiceLine()
		{
			CusClassification @class = Factory.New<CusClassification>();
			@class.CC_LookupCode = "Machinery Parts";
			@class.CC_TariffNum = "4016.99.79.00J";
			@class.CC_Description = "Machinery parts";
			@class.CC_PartsOfClassification = "4011.62.00.00K";
			@class.CC_ConcessionCode = "987686D";
			PermitCode permit = @class.PermitCodes.AddNew();
			permit.ZO_Code = "CUD";
			permit.ZO_Data = "147981H";

			AssertEquals("Not classified", ZGuid.Empty, InvoiceLine.JI_CC);
			AssertEquals("AddInfo is empty", ZString.Empty, InvoiceLine.JI_AddInfo);
			AssertEquals("PartsOfClassification", ZString.Empty, InvoiceLine.JI_PartsOfClassification);
			AssertEquals("Concession Code", ZString.Empty, InvoiceLine.JI_ConcessionCode);
			AssertEquals("PermitCodes", 0, InvoiceLine.PermitCodes.Count);

			InvoiceLine.JI_CC = @class.PK;
			AssertEquals("Tariff number", @class.CC_TariffNum, InvoiceLine.JI_Tariff);
			AssertEquals("PartsOfClassification", @class.CC_PartsOfClassification, InvoiceLine.JI_PartsOfClassification);
			AssertEquals("Concession Code", @class.CC_ConcessionCode, InvoiceLine.JI_ConcessionCode);
			AssertEquals("Permit code", 1, InvoiceLine.PermitCodes.Count);
			AssertEquals("Permit code", "CUD", InvoiceLine.PermitCodes[0].ZO_Code);

			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			JobComInvoiceLine lineLoaded = anotherFactory.Load<JobComInvoiceLine>(InvoiceLine.PK);

			AssertEquals("PartsOfClassification persisted", @class.CC_PartsOfClassification, lineLoaded.JI_PartsOfClassification);
			AssertEquals("Concession Code persisted", @class.CC_ConcessionCode, lineLoaded.JI_ConcessionCode);
			AssertEquals("Permit code persisted", 1, lineLoaded.PermitCodes.Count);
			AssertEquals("Permit code persisted", "CUD", lineLoaded.PermitCodes[0].ZO_Code);
		}

		public void TestLockCustomsSuppQty()
		{
			InvoiceLine.JI_SupplementaryUQ = "KG";
			InvoiceLine.RefreshBinding();
			AssertEquals("Supp Qty open", false, InvoiceLine.JI_SupplementaryQtyInfo.ReadOnly);

			InvoiceLine.JI_SupplementaryUQ = "";
			InvoiceLine.RefreshBinding();
			AssertEquals("Supp Qty locked", true, InvoiceLine.JI_SupplementaryQtyInfo.ReadOnly);
		}

		public void TestSupplementaryQtyDecimalPlaces()
		{
			Assert(
				"JI_SupplementaryQty.DecimalPlaces should be <= 3, but instead is " + NZJobComInvoiceLineSchema.JI_SupplementaryQty.Scale.ToString(),
				NZJobComInvoiceLineSchema.JI_SupplementaryQty.Scale <= JobComInvoiceLine.CustomsQtyMaxDecimalPlaces);
		}

		public void TestJI_Calc_ALACLevyAmountIsReadOnly()
		{
			InvoiceLine.Declaration.JE_MessageType = "IMP";
			AssertEquals("ALAC is readonly", true, InvoiceLine.JI_Calc_ALACLevyAmountInfo.ReadOnly);
		}

		public void TestPrefDutyDescription()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceHeader.JZ_DefaultQualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.Qualifies;
			AssertEquals("Header Qualifies for Preferential Duty shown on line should be same as InvoiceHeader", QualifiesForPreferentialDutyList.Descriptions.Qualifies, invoiceLine.PrefDutyDescription);
			invoiceLine.JI_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.NonQualifying;
			AssertEquals("Header Qualifies for Preferential Duty shown on line should be overriden line value", QualifiesForPreferentialDutyList.Descriptions.NonQualifying, invoiceLine.PrefDutyDescription);
		}

		public void TestCountryOfOriginDescription()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceHeader.JZ_RN_NKDefaultOrigin = "US";
			AssertEquals("Header Country/Region of Origin shown on line should be same as InvoiceHeader", "UNITED STATES", invoiceLine.CountryOfOriginDescription.ToUpper());
			invoiceLine.JI_CountryOfOrigin = "CA";
			AssertEquals("Header Country/Region of Origin shown on line should be overriden line value", "CANADA", invoiceLine.CountryOfOriginDescription.ToUpper());
		}

		public void TestCountryOfExportDescription()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceHeader.JZ_RN_NKDefaultExport = "ZA";
			AssertEquals("Header Country/Region of Export shown on line should be same as InvoiceHeader", "SOUTH AFRICA", invoiceLine.CountryOfExportDescription.ToUpper());
			invoiceLine.JI_RN_NKCountryOfExport = "AU";
			AssertEquals("Header Country/Region of Export shown on line should be overriden line value", "AUSTRALIA", invoiceLine.CountryOfExportDescription.ToUpper());
		}

		public void TestCanSetManufacturer()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_RL_NKClosestPort = "KRBUS";
			org1.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "ABC");
			var org2 = Factory.New<OrgHeader>();
			org2.OH_RL_NKClosestPort = "JPTYO";
			org2.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "DEF");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.ManufacturerOrgPK = org1.PK;
			AssertEquals(org1.PK, invoiceLine.ManufacturerOrgPK);
			AssertEquals(org1.MainAddress.PK, invoiceLine.JI_OA_ManufacturerAddress);
			invoiceLine.ManufacturerOrgPK = org2.PK;
			invoiceLine.JI_OA_ManufacturerAddress = org2.MainAddress.PK;
			AssertEquals(org2.PK, invoiceLine.ManufacturerOrgPK);
			AssertEquals(org2.MainAddress.PK, invoiceLine.JI_OA_ManufacturerAddress);
		}

		public void TestCanSetManufacturerAddress()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_RL_NKClosestPort = "KRBUS";
			org1.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "ABC");
			var org2 = Factory.New<OrgHeader>();
			org2.OH_RL_NKClosestPort = "JPTYO";
			org2.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "DEF");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			//invoiceLine.ManufacturerOrgPK = org1.PK;
			invoiceLine.JI_OA_ManufacturerAddress = org1.MainAddress.PK;
			AssertEquals(org1.PK, invoiceLine.ManufacturerOrgPK);
			AssertEquals(org1.MainAddress.PK, invoiceLine.JI_OA_ManufacturerAddress);
			//invoiceLine.ManufacturerOrgPK = org2.PK;
			invoiceLine.JI_OA_ManufacturerAddress = org2.MainAddress.PK;
			AssertEquals(org2.PK, invoiceLine.ManufacturerOrgPK);
			AssertEquals(org2.MainAddress.PK, invoiceLine.JI_OA_ManufacturerAddress);
		}

		public override void TestWipeNKTaxType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			Assert(!invoiceLine.ShouldWipeNKTaxType);
		}

		public void TestSetDefaultValuesForNewPackableItem()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var line1 = invoice.InvoiceLines.AddNew();
			line1.JI_Description = "DESC LINE 1";
			line1.JI_InvoiceQuantity = 2;
			line1.JI_InvoiceUQ = "BAG";
			var line2 = invoice.InvoiceLines.AddNew();
			line2.JI_Description = "DESC LINE 2";
			line2.JI_InvoiceQuantity = 3;
			line2.JI_InvoiceUQ = "BBG";

			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			var package = packingList.PackageJob.Packages.AddNew();
			var packableItemRelations = package.PackableItemRelataions;
			var items = packingList.PackableItems;
			AssertEquals(2, items.Count);
			var item = items.First();
			AssertEquals("DESC LINE 1", item.CUI_GoodsDescription);
			AssertEquals(2m, item.CUI_PackableQty);
			AssertEquals("BAG", item.CUI_PackableUQ);
			item = items.ElementAt(1);
			AssertEquals("DESC LINE 2", item.CUI_GoodsDescription);
			AssertEquals(3m, item.CUI_PackableQty);
			AssertEquals("BBG", item.CUI_PackableUQ);
		}

		public void TestInitialDefaultCodesCopyFromProduct_IfEntryTypeIsImport_WhenPartsOfSetsInInvoiceLine()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsImport, "NZPERIMP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsImport, "MAF", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsImport, "NZPROIMP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsImport, "BEF", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, "NZOTHIMPLN");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, "ATF", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "NZPEREXP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "APA", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsExport, "NZPROEXP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsExport, "ANT", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, "NZOTHEXPLN");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, "EMP", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "ABC";

			var supplierPart1 = Factory.New<OrgSupplierPart>();
			supplierPart1.FillWithValidTestData();
			supplierPart1.OP_PartNum = "Part1";
			supplierPart1.OP_Desc = "Part1 Desc";
			supplierPart1.RelatedOrganisations.RemoveAndDeleteAll();
			supplierPart1.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

			var classificationPart = Factory.New<BaseCusClassification>();
			classificationPart.FillWithValidTestData();
			classificationPart.CC_LookupCode = "CCPart1";
			classificationPart.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;

			var partPivot = Factory.New<CusClassPartPivot>();
			partPivot.CI_CC = classificationPart.PK;
			partPivot.CI_OP = supplierPart1.PK;
			partPivot.CI_ChildType = ECB.ClassificationTypeList.Codes.HTB;
			partPivot.CI_RN_NKCountry = Core.Constants.CountryCodes.NewZealand;

			var permitCode = partPivot.PermitCodes.AddNew();
			var prohibitedCode = partPivot.ProhibitedCodes.AddNew();
			var otherInfo = partPivot.OtherInfos.AddNew();
			permitCode.ZO_Code = "MAF";
			permitCode.ZO_Data = "MAF Data";
			prohibitedCode.ZO_Code = "BEF";
			prohibitedCode.ZO_Data = "BEF Data";
			otherInfo.ZO_Code = "ATF";
			otherInfo.ZO_Data = "ATF Data";

			var permitCode1 = partPivot.PermitCodes.AddNew();
			var prohibitedCode1 = partPivot.ProhibitedCodes.AddNew();
			var otherInfo1 = partPivot.OtherInfos.AddNew();
			permitCode1.ZO_Code = "APA";
			permitCode1.ZO_Data = "APA Data";
			prohibitedCode1.ZO_Code = "ANT";
			prohibitedCode.ZO_Data = "ANT Data";
			otherInfo1.ZO_Code = "EMP";
			otherInfo1.ZO_Data = "EMP Data";

			var declarationImport = Factory.NewWithValidTestData<JobDeclaration>();
			declarationImport.JE_OH_Importer = importer.PK;
			declarationImport.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declarationImport.Invoices.AddNew();
			var lineImport1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			lineImport1.JI_PartNo = "Part1";

			AssertEquals("Import Permit Codes", 1, lineImport1.PermitCodes.Count);
			AssertEquals("MAF", lineImport1.PermitCodes[0].ZO_Code);
			AssertEquals("Import Prohibited Codes", 1, lineImport1.ProhibitedCodes.Count);
			AssertEquals("BEF", lineImport1.ProhibitedCodes[0].ZO_Code);
			AssertEquals("Import OtherInfos", 1, lineImport1.OtherInfos.Count);
			AssertEquals("ATF", lineImport1.OtherInfos[0].ZO_Code);
		}

		public void TestInitialDefaultCodesCopyFromProduct_IfEntryTypeIsExport_WhenPartsOfSetsInInvoiceLine()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsImport, "NZPERIMP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsImport, "MAF", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsImport, "NZPROIMP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsImport, "BEF", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, "NZOTHIMPLN");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, "ATF", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "NZPEREXP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "APA", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsExport, "NZPROEXP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsExport, "ANT", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, "NZOTHEXPLN");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, "EMP", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "ABC";

			var supplierPart1 = Factory.New<OrgSupplierPart>();
			supplierPart1.FillWithValidTestData();
			supplierPart1.OP_PartNum = "Part1";
			supplierPart1.OP_Desc = "Part1 Desc";
			supplierPart1.RelatedOrganisations.RemoveAndDeleteAll();
			supplierPart1.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

			var classificationPart = Factory.New<BaseCusClassification>();
			classificationPart.FillWithValidTestData();
			classificationPart.CC_LookupCode = "CCPart1";
			classificationPart.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;

			var partPivot = Factory.New<CusClassPartPivot>();
			partPivot.CI_CC = classificationPart.PK;
			partPivot.CI_OP = supplierPart1.PK;
			partPivot.CI_ChildType = ECB.ClassificationTypeList.Codes.HTB;
			partPivot.CI_RN_NKCountry = Core.Constants.CountryCodes.NewZealand;

			var permitCode = partPivot.PermitCodes.AddNew();
			var prohibitedCode = partPivot.ProhibitedCodes.AddNew();
			var otherInfo = partPivot.OtherInfos.AddNew();
			permitCode.ZO_Code = "MAF";
			permitCode.ZO_Data = "MAF Data";
			prohibitedCode.ZO_Code = "BEF";
			prohibitedCode.ZO_Data = "BEF Data";
			otherInfo.ZO_Code = "ATF";
			otherInfo.ZO_Data = "ATF Data";

			var permitCode1 = partPivot.PermitCodes.AddNew();
			var prohibitedCode1 = partPivot.ProhibitedCodes.AddNew();
			var otherInfo1 = partPivot.OtherInfos.AddNew();
			permitCode1.ZO_Code = "APA";
			permitCode1.ZO_Data = "APA Data";
			prohibitedCode1.ZO_Code = "ANT";
			prohibitedCode.ZO_Data = "ANT Data";
			otherInfo1.ZO_Code = "EMP";
			otherInfo1.ZO_Data = "EMP Data";

			var declarationExport = Factory.NewWithValidTestData<JobDeclaration>();
			declarationExport.JE_OH_Importer = importer.PK;
			declarationExport.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoice = declarationExport.Invoices.AddNew();
			var lineExport1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			lineExport1.JI_PartNo = "Part1";

			AssertEquals("Export Permit Codes", 1, lineExport1.PermitCodes.Count);
			AssertEquals("APA", lineExport1.PermitCodes[0].ZO_Code);
			AssertEquals("Export Prohibited Codes", 1, lineExport1.ProhibitedCodes.Count);
			AssertEquals("ANT", lineExport1.ProhibitedCodes[0].ZO_Code);
			AssertEquals("Export OtherInfos", 1, lineExport1.OtherInfos.Count);
			AssertEquals("EMP", lineExport1.OtherInfos[0].ZO_Code);
		}

		public void TestEffectiveZeroRatedDuty()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			declaration.JE_IsZeroRatedAll = "Y";
			invoiceLine.JI_IsZeroRatedDuty = "";
			AssertEquals(true, invoiceLine.EffectiveIsZeroRatedDuty);

			declaration.JE_IsZeroRatedAll = "N";
			invoiceLine.JI_IsZeroRatedDuty = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedDuty);

			invoiceHeader.JZ_IsZeroRatedDuty = "N";
			invoiceLine.JI_IsZeroRatedDuty = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedDuty);

			declaration.JE_IsZeroRatedAll = "";
			invoiceHeader.JZ_IsZeroRatedDuty = "";
			invoiceLine.JI_IsZeroRatedDuty = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedDuty);
		}

		public void TestEffectiveZeroRatedExcise()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			declaration.JE_IsZeroRatedAll = "Y";
			invoiceLine.JI_IsZeroRatedExcise = "";
			AssertEquals(true, invoiceLine.EffectiveIsZeroRatedExcise);

			declaration.JE_IsZeroRatedAll = "N";
			invoiceLine.JI_IsZeroRatedExcise = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedExcise);

			invoiceHeader.JZ_IsZeroRatedExcise = "N";
			invoiceLine.JI_IsZeroRatedExcise = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedExcise);

			declaration.JE_IsZeroRatedAll = "";
			invoiceHeader.JZ_IsZeroRatedExcise = "";
			invoiceLine.JI_IsZeroRatedExcise = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedExcise);
		}

		public void TestEffectiveZeroRatedGST()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			declaration.JE_IsZeroRatedAll = "Y";
			invoiceLine.JI_IsZeroRatedGST = "";
			AssertEquals(true, invoiceLine.EffectiveIsZeroRatedGST);

			declaration.JE_IsZeroRatedAll = "N";
			invoiceLine.JI_IsZeroRatedGST = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedGST);

			invoiceHeader.JZ_IsZeroRatedGST = "N";
			invoiceLine.JI_IsZeroRatedGST = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedGST);

			declaration.JE_IsZeroRatedAll = "";
			invoiceHeader.JZ_IsZeroRatedGST = "";
			invoiceLine.JI_IsZeroRatedGST = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedGST);
		}

		public void TestEffectiveZeroRatedLevies()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			declaration.JE_IsZeroRatedAll = "Y";
			invoiceLine.JI_IsZeroRatedLevies = "";
			AssertEquals(true, invoiceLine.EffectiveIsZeroRatedLevies);

			declaration.JE_IsZeroRatedAll = "N";
			invoiceLine.JI_IsZeroRatedLevies = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedLevies);

			invoiceHeader.JZ_IsZeroRatedLevies = "N";
			invoiceLine.JI_IsZeroRatedLevies = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedLevies);

			declaration.JE_IsZeroRatedAll = "";
			invoiceHeader.JZ_IsZeroRatedLevies = "";
			invoiceLine.JI_IsZeroRatedLevies = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedLevies);
		}

		public void TestSerialiseCollectionsOnFactorySaving()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			Assert("PreCondition:Class addinfo is empty", invoiceLine.JI_AddInfo.IsEmpty);
			OtherInfo otherInfo = invoiceLine.OtherInfos.AddNew();
			otherInfo.ZO_Code = "ZZZ";
			otherInfo.ZO_Data = "111";
			Factory.Save();

			AssertEquals("Other info collection serialised", "ZZZ=111", invoiceLine.JI_OtherInfos);
			AssertEquals("Class Addinfo", "OtherInfos=ZZZ=111", invoiceLine.JI_AddInfo);
		}

		public void TestAddInfoWithOtherInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AssertEquals("PreCondition:AddInfo empty", string.Empty, invoiceLine.JI_AddInfo.ToString());
			invoiceLine.JI_AddInfo = "OtherInfos=APE^APD=ZZZZ*ConcessionCode=123";
			AssertEquals("Concession", "123", invoiceLine.JI_ConcessionCode);
			AssertEquals("OtherInfo", "APE^APD=ZZZZ", invoiceLine.JI_OtherInfos);
		}

		public void TestOtherInfosCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.JI_OtherInfos = "APE^APD=ZZZZ";
			AssertEquals("Otherinfos count", 2, invoiceLine.OtherInfos.Count);

			AssertEquals("APE", invoiceLine.OtherInfos[0].ZO_Code);
			AssertEquals(ZString.Empty, invoiceLine.OtherInfos[0].ZO_Data);
			AssertEquals("APD", invoiceLine.OtherInfos[1].ZO_Code);
			AssertEquals("ZZZZ", invoiceLine.OtherInfos[1].ZO_Data);
		}

		public void TestAddInfoPropertyUpdate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AssertEquals("JI_AddInfo is empty", ZString.Empty, invoiceLine.JI_AddInfo);
			invoiceLine.JI_SupplementaryUQ = "NMB";
			Factory.Save();
			AssertEquals("JI_AddInfo", "SupplementaryUQ=NMB", invoiceLine.JI_AddInfo);
		}

		public void TestCodePropertiesUpdatedOnCollectionChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			PermitCode code = invoiceLine.PermitCodes.AddNew();
			code.ZO_Code = "ZZZ";

			Factory.Save();
			AssertEquals("AddInfo JI_PermitCodes is updated", "PermitCodes=ZZZ", invoiceLine.JI_AddInfo);
		}

		protected override bool UseUniversalTariff => false;

		#region Implementation

		void SetPartNoAndAssertJI_OPIsPartPK(OrgSupplierPart part)
		{
			InvoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("Precondition: InvoiceLine.JI_OP (pk assigned from entry of partNo code)", part.PK, InvoiceLine.JI_OP);
		}

		protected new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		protected new JobComInvoiceHeader InvoiceHeader
		{
			get { return (JobComInvoiceHeader)base.InvoiceHeader; }
		}

		protected new JobComInvoiceLine InvoiceLine
		{
			get { return (JobComInvoiceLine)base.InvoiceLine; }
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			return declaration;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return InvoiceLine;
		}

		protected NZCClassification Tariff1
		{
			get
			{
				if (fTariff1 == null)
				{
					fTariff1 = NZCClassification.New(Factory);
					fTariff1.U0_Tariff = "1111.11.11.11K";
					fTariff1.U0_Description = "TARIFF_DESCRIPTION_1";
					fTariff1.U0_DateActiveFrom = new ZDateTime(1980, 1, 1, 0, 0, 0);
					fTariff1.U0_DateActiveTo = new ZDateTime(3000, 12, 31, 0, 0, 0);
				}
				return fTariff1;
			}
		}
		NZCClassification fTariff1;

		protected NZCClassification Tariff2
		{
			get
			{
				if (fTariff2 == null)
				{
					fTariff2 = NZCClassification.New(Factory);
					fTariff2.U0_Tariff = "2222.22.22.22K";
					fTariff2.U0_Description = "TARIFF_DESCRIPTION_2";
					fTariff2.U0_DateActiveFrom = new ZDateTime(1980, 1, 1, 0, 0, 0);
					fTariff2.U0_DateActiveTo = new ZDateTime(3000, 12, 31, 0, 0, 0);
				}
				return fTariff2;
			}
		}
		NZCClassification fTariff2;
		#endregion

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceLineCharge>);
	}

	public class JobComInvoiceLineCustomsChargesProxiedBackFromEntryLineTest : TestCaseWithFactory
	{
		void AssertSetNonLVXInvoiceLine(Func<ZDecimal> getValue)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("DEM", 1000m, "NZ", new ZDateTime(2010, 1, 1), new ZDateTime(2079, 6, 6), "Deminimus");
			Factory.Save();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfArrival = new ZDateTime(2019, 08, 01);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			AssertEquals(ZDecimal.Zero, getValue());
			var lvx = invoiceLine1.OtherInfos.AddNew();
			lvx.ZO_Code = "LVX";
			AssertNotEquals(ZDecimal.Zero, getValue());
		}

		public void TestJI_Calc_DutyRateFlatRate()
		{
			SetDutyCalculationData("2208.60.19.09J", "", qualifiesForPreferentialDuty: false, "US", 20000m, "LTR", 0m, "", 20000m, new ZDateTime(2005, 12, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals(41.146m, invoiceLine1.JI_Calc_DutyRateFlatRate);
		}

		public void TestJI_Calc_DutyRatePercent()
		{
			SetDutyCalculationData("7007.21.02.01K", "", qualifiesForPreferentialDuty: false, "US", 2000m, "LTR", 0m, "", 4000m, new ZDateTime(2005, 12, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: true, isZeroRatedLevies: false);
			AssertEquals(17.5m, invoiceLine1.JI_Calc_DutyRatePercent);
			AssertSetNonLVXInvoiceLine(() => invoiceLine1.JI_Calc_DutyRatePercent);
		}

		public void TestJI_Calc_ALACLevyAmount()
		{
			SetDutyCalculationData("2204.21.18.19G", "", qualifiesForPreferentialDuty: false, "US", 2000m, "LTR", 0m, "", 4000m, new ZDateTime(2005, 12, 12), "", isZeroRatedDuty: true, isZeroRatedExcise: true, isZeroRatedLevies: false);
			AssertEquals(98.60m, invoiceLine1.JI_Calc_ALACLevyAmount);
			AssertSetNonLVXInvoiceLine(() => invoiceLine1.JI_Calc_ALACLevyAmount);
		}

		public void TestJI_Calc_FuelLevyAmount1()
		{
			SetDutyCalculationData("2710.19.21.10C", "", qualifiesForPreferentialDuty: false, "US", 20000m, "LMS", 20000m, "LTR", 20000m, new ZDateTime(2008, 10, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals(1877.00m, invoiceLine1.JI_Calc_FuelLevyAmount);
			AssertSetNonLVXInvoiceLine(() => invoiceLine1.JI_Calc_FuelLevyAmount);
		}

		public void TestJI_Calc_FuelLevyAmount2()
		{
			SetDutyCalculationData("2710.19.13.00C", "", qualifiesForPreferentialDuty: false, "US", 20000m, "LMS", 20000m, "LTR", 20000m, new ZDateTime(2008, 10, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals(9.00m, invoiceLine1.JI_Calc_FuelLevyAmount);
			AssertSetNonLVXInvoiceLine(() => invoiceLine1.JI_Calc_FuelLevyAmount);
		}

		public void TestJI_Calc_SyntheticGreenhouseGasesLevyAmount()
		{
			var tariff = NZCClassificationTest.CreateSGGTariff(Factory);
			SetDutyCalculationData(tariff.U0_Tariff, "", qualifiesForPreferentialDuty: false, "US", 20000m, tariff.U0_StatisticalUnit, 0m, "", 20000m, new ZDateTime(2012, 7, 7), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals(171000.00m, invoiceLine1.JI_Calc_SyntheticGreenhouseGasesLevyAmount);
			AssertSetNonLVXInvoiceLine(() => invoiceLine1.JI_Calc_SyntheticGreenhouseGasesLevyAmount);
		}

		public void TestJI_Calc_HERALevyAmount()
		{
			SetDutyCalculationData("7210.70.01.01K", "", qualifiesForPreferentialDuty: false, "US", 20000m, StatisticalUQList.Codes.Kilograms, 0m, "", 20000m, new ZDateTime(2005, 7, 7), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			AssertEquals(100.00m, invoiceLine1.JI_Calc_HERALevyAmount);
			AssertSetNonLVXInvoiceLine(() => invoiceLine1.JI_Calc_HERALevyAmount);
		}

		public void TestJI_Calc_LevyTypeDescriptionAndValueInNZD()
		{
			AssertEquals("InvoiceLine1.JI_Calc_LevyTypeDescription", JobComInvoiceLine.NoParticularLevyDescription, invoiceLine1.JI_Calc_LevyTypeDescription);
			AssertEquals("InvoiceLine1.JI_Calc_LevyValueInNZD", 0m, invoiceLine1.JI_Calc_LevyValueInNZD);

			SetDutyCalculationData("2204.21.18.19G", "", qualifiesForPreferentialDuty: false, "US", 2000m, "LTR", 0m, "", 4000m, new ZDateTime(2005, 12, 12), "", isZeroRatedDuty: true, isZeroRatedExcise: true, isZeroRatedLevies: false);
			Assert("JI_DutyCalculationComplete accessed", !invoiceLine1.JI_DutyRateComplete.IsEmpty);
			AssertEquals(98.60m, invoiceLine1.JI_Calc_ALACLevyAmount);
			AssertEquals("InvoiceLine1.JI_Calc_LevyTypeDescription", JobComInvoiceLine.ALACLevyDescription, invoiceLine1.JI_Calc_LevyTypeDescription);
			AssertEquals("InvoiceLine1.JI_Calc_LevyValueInNZD", 98.60m, invoiceLine1.JI_Calc_LevyValueInNZD);

			SetDutyCalculationData("2710.19.11.11F", "", qualifiesForPreferentialDuty: false, "US", 20000m, "LTR", 0m, "", 20000m, new ZDateTime(2004, 12, 12), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			Assert("JI_DutyCalculationComplete accessed", !invoiceLine1.JI_DutyRateComplete.IsEmpty);
			AssertEquals(1016.00m, invoiceLine1.JI_Calc_FuelLevyAmount);
			AssertEquals("InvoiceLine1.JI_Calc_LevyTypeDescription", JobComInvoiceLine.FuelLevyDescription, invoiceLine1.JI_Calc_LevyTypeDescription);
			AssertEquals("InvoiceLine1.JI_Calc_LevyValueInNZD", 1016.00m, invoiceLine1.JI_Calc_LevyValueInNZD);

			SetDutyCalculationData("7210.70.01.01K", "", qualifiesForPreferentialDuty: false, "US", 20000m, StatisticalUQList.Codes.Kilograms, 0m, "", 20000m, new ZDateTime(2005, 7, 7), "", isZeroRatedDuty: false, isZeroRatedExcise: false, isZeroRatedLevies: false);
			Assert("JI_DutyCalculationComplete accessed", !invoiceLine1.JI_DutyRateComplete.IsEmpty);
			AssertEquals(100.00m, invoiceLine1.JI_Calc_HERALevyAmount);
			AssertEquals("InvoiceLine1.JI_Calc_LevyTypeDescription", JobComInvoiceLine.HERALevyDescription, invoiceLine1.JI_Calc_LevyTypeDescription);
			AssertEquals("InvoiceLine1.JI_Calc_LevyValueInNZD", 100.00m, invoiceLine1.JI_Calc_LevyValueInNZD);
		}

		void SetDutyCalculationData(ZString classificationCode,
			ZString concessionCode,
			ZBool qualifiesForPreferentialDuty,
			ZString countryOfOrigin,
			ZDecimal statQty,
			ZString statUQ,
			ZDecimal suppQty,
			ZString suppUQ,
			ZDecimal valueForDuty,
			ZDateTime barrierDate,
			ZString partsOfClassification,
			ZBool isZeroRatedDuty,
			ZBool isZeroRatedExcise,
			ZBool isZeroRatedLevies)
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfArrival = barrierDate;

			invoiceLine1.JI_Tariff = classificationCode;
			invoiceLine1.JI_ConcessionCode = concessionCode;
			invoiceLine1.JI_QualifiesForPreferentialDuty = qualifiesForPreferentialDuty ? QualifiesForPreferentialDutyList.Codes.Qualifies : "N";
			invoiceLine1.JI_CountryOfOrigin = countryOfOrigin;
			invoiceLine1.JI_CustomsQuantity = statQty;
			invoiceLine1.JI_CustomsUnitQty = statUQ;
			invoiceLine1.JI_SupplementaryQty = suppQty;
			invoiceLine1.JI_SupplementaryUQ = suppUQ;
			invoiceLine1.JI_LinePrice = valueForDuty;
			invoiceLine1.JI_PartsOfClassification = partsOfClassification;
			invoiceLine1.JI_IsZeroRatedDuty = isZeroRatedDuty ? "Y" : "N";
			invoiceLine1.JI_IsZeroRatedExcise = isZeroRatedExcise ? "Y" : "N";
			invoiceLine1.JI_IsZeroRatedLevies = isZeroRatedLevies ? "Y" : "N";
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			declaration = JobDeclaration.New(Factory);

			entryLine = declaration.CusEntryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 1000m;

			invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "NZD";
			invoiceHeader.JZ_InvoiceAmount = 1000m;

			invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_LinePrice = 400m;

			invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_LinePrice = 600m;
		}
		JobDeclaration declaration;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine1;
		JobComInvoiceLine invoiceLine2;
		#endregion
	}

	public class JobComInvoiceLineDutyCalculatorTest : TestCaseWithFactory
	{
		public void TestJI_Tariff_Changed()
		{
			testObjects.InvoiceLine.JI_Tariff = "0101.01.01.01J";
			Assert("InvoiceLine.JI_Tariff Changes DutyRateComplete", testObjects.InvoiceLine.JI_DutyRateComplete != ExpectedNormalValue);
		}

		public void TestJI_ConcessionCode_Changed()
		{
			testObjects.InvoiceLine.JI_ConcessionCode = "100101J";
			Assert("InvoiceLine.JI_ConcessionCode Changes DutyRateComplete", testObjects.InvoiceLine.JI_DutyRateComplete != ExpectedNormalValue);
		}

		public void TestCountryOfOriginAndPrefDutyChanged()
		{
			NZCustomsDataRegistry.Instance.PreferentialCountryGroupCodeDefaulting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			string saveJI_CountryOfOrigin = testObjects.InvoiceLine.JI_CountryOfOrigin;
			testObjects.InvoiceLine.JI_CountryOfOrigin = "AU";
			testObjects.InvoiceLine.JI_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.Qualifies;
			Assert("InvoiceLine.JI_CountryOfOrigin Changes DutyRateComplete", testObjects.InvoiceLine.JI_DutyRateComplete != ExpectedNormalValue);
			testObjects.InvoiceLine.JI_CountryOfOrigin = saveJI_CountryOfOrigin;
			AssertEquals("InvoiceLine.JI_CountryOfOrigin Changes DutyRateComplete Back", ExpectedNormalValue, testObjects.InvoiceLine.JI_DutyRateComplete);
		}

		public void TestCustomsValueAndCustomsQuantityChanged()
		{
			testObjects.InvoiceLine.JI_Tariff = "6207.11.01.00J";
			testObjects.InvoiceLine.JI_CustomsQuantity = 10000m;
			testObjects.InvoiceLine.JI_CustomsUnitQty = "NMB";
			testObjects.InvoiceLine.JI_LinePrice = 20000m;
			testObjects.InvoiceHeader.JZ_InvoiceAmount = 20000m;
			string saveDutyRate = testObjects.InvoiceLine.JI_DutyRateComplete;
			Assert("Duty Rate Contains 6207.11.01.00J", saveDutyRate.IndexOf("6207.11.01.00J") != -1);

			testObjects.InvoiceLine.JI_LinePrice = 2000m;
			testObjects.InvoiceHeader.JZ_InvoiceAmount = 2000m;
			string newDutyRate = testObjects.InvoiceLine.JI_DutyRateComplete;
			Assert("Duty Rate now Different", newDutyRate != saveDutyRate);
			Assert("Duty Rate Contains 6207.11.09.00K", newDutyRate.IndexOf("6207.11.09.00K") != -1);
		}

		public void TestJI_PartsOfClassificationChanged()
		{
			string saveJI_PartsOfClassification = testObjects.InvoiceLine.JI_PartsOfClassification;
			testObjects.InvoiceLine.JI_PartsOfClassification = "8426.99.00.09A";
			Assert("InvoiceLine.JI_PartsOfClassification Changes DutyRateComplete", testObjects.InvoiceLine.JI_DutyRateComplete != ExpectedNormalValue);
			testObjects.InvoiceLine.JI_PartsOfClassification = saveJI_PartsOfClassification;
			AssertEquals("InvoiceLine.JI_PartsOfClassification Changes DutyRateComplete Back", ExpectedNormalValue, testObjects.InvoiceLine.JI_DutyRateComplete);
		}

		public void TestJI_IsZeroRatedChanged()
		{
			testObjects.InvoiceLine.JI_IsZeroRatedDuty = "Y";
			Assert("InvoiceLine.JI_PartsOfClassification Changes DutyRateComplete", testObjects.InvoiceLine.JI_DutyRateComplete != ExpectedNormalValue);
			testObjects.InvoiceLine.JI_IsZeroRatedDuty = "N";
			AssertEquals("InvoiceLine.JI_PartsOfClassification Changes DutyRateComplete Back", ExpectedNormalValue, testObjects.InvoiceLine.JI_DutyRateComplete);
		}

		protected DutyCalculatorTestObjects testObjects;
		protected const string ExpectedNormalValue = "7007.21.02.01K @ NML = 17.50%";

		protected override void SetUp()
		{
			base.SetUp();
			testObjects = new DutyCalculatorTestObjects(Factory);
			AssertEquals("Precondition", ExpectedNormalValue, testObjects.InvoiceLine.JI_DutyRateComplete);
		}

		public void TestEffectiveIsZeroRatedDuty()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			declaration.JE_IsZeroRatedAll = "Y";
			invoiceHeader.JZ_IsZeroRatedDuty = "";
			invoiceLine.JI_IsZeroRatedDuty = "";
			AssertEquals(true, invoiceLine.EffectiveIsZeroRatedDuty);
			AssertEquals("Yes", invoiceLine.ZeroRatedDutyDescription);

			declaration.JE_IsZeroRatedAll = "";
			invoiceHeader.JZ_IsZeroRatedDuty = "Y";
			invoiceLine.JI_IsZeroRatedDuty = "";
			AssertEquals(true, invoiceLine.EffectiveIsZeroRatedDuty);
			AssertEquals("Yes", invoiceLine.ZeroRatedDutyDescription);

			declaration.JE_IsZeroRatedAll = "N";
			invoiceHeader.JZ_IsZeroRatedDuty = "";
			invoiceLine.JI_IsZeroRatedDuty = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedDuty);
			AssertEquals("No", invoiceLine.ZeroRatedDutyDescription);

			declaration.JE_IsZeroRatedAll = "";
			invoiceHeader.JZ_IsZeroRatedDuty = "N";
			invoiceLine.JI_IsZeroRatedDuty = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedDuty);
			AssertEquals("No", invoiceLine.ZeroRatedDutyDescription);

			declaration.JE_IsZeroRatedAll = "";
			invoiceHeader.JZ_IsZeroRatedDuty = "";
			invoiceLine.JI_IsZeroRatedDuty = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedDuty);
			AssertEquals("", invoiceLine.ZeroRatedDutyDescription);
		}

		public void TestEffectiveIsZeroRatedExcise()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			declaration.JE_IsZeroRatedAll = "Y";
			invoiceHeader.JZ_IsZeroRatedExcise = "";
			invoiceLine.JI_IsZeroRatedExcise = "";
			AssertEquals(true, invoiceLine.EffectiveIsZeroRatedExcise);
			AssertEquals("Yes", invoiceLine.ZeroRatedExciseDescription);

			declaration.JE_IsZeroRatedAll = "";
			invoiceHeader.JZ_IsZeroRatedExcise = "Y";
			invoiceLine.JI_IsZeroRatedExcise = "";
			AssertEquals(true, invoiceLine.EffectiveIsZeroRatedExcise);
			AssertEquals("Yes", invoiceLine.ZeroRatedExciseDescription);

			declaration.JE_IsZeroRatedAll = "N";
			invoiceHeader.JZ_IsZeroRatedExcise = "";
			invoiceLine.JI_IsZeroRatedExcise = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedExcise);
			AssertEquals("No", invoiceLine.ZeroRatedExciseDescription);

			declaration.JE_IsZeroRatedAll = "";
			invoiceHeader.JZ_IsZeroRatedExcise = "N";
			invoiceLine.JI_IsZeroRatedExcise = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedExcise);
			AssertEquals("No", invoiceLine.ZeroRatedExciseDescription);

			declaration.JE_IsZeroRatedAll = "";
			invoiceHeader.JZ_IsZeroRatedExcise = "";
			invoiceLine.JI_IsZeroRatedExcise = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedExcise);
			AssertEquals("", invoiceLine.ZeroRatedExciseDescription);
		}

		public void TestEffectiveIsZeroRatedGST()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			declaration.JE_IsZeroRatedAll = "Y";
			invoiceHeader.JZ_IsZeroRatedGST = "";
			invoiceLine.JI_IsZeroRatedGST = "";
			AssertEquals(true, invoiceLine.EffectiveIsZeroRatedGST);
			AssertEquals("Yes", invoiceLine.ZeroRatedGSTDescription);

			declaration.JE_IsZeroRatedAll = "";
			invoiceHeader.JZ_IsZeroRatedGST = "Y";
			invoiceLine.JI_IsZeroRatedGST = "";
			AssertEquals(true, invoiceLine.EffectiveIsZeroRatedGST);
			AssertEquals("Yes", invoiceLine.ZeroRatedGSTDescription);

			declaration.JE_IsZeroRatedAll = "N";
			invoiceHeader.JZ_IsZeroRatedGST = "";
			invoiceLine.JI_IsZeroRatedGST = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedGST);
			AssertEquals("No", invoiceLine.ZeroRatedGSTDescription);

			declaration.JE_IsZeroRatedAll = "";
			invoiceHeader.JZ_IsZeroRatedGST = "N";
			invoiceLine.JI_IsZeroRatedGST = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedGST);
			AssertEquals("No", invoiceLine.ZeroRatedGSTDescription);

			declaration.JE_IsZeroRatedAll = "";
			invoiceHeader.JZ_IsZeroRatedGST = "";
			invoiceLine.JI_IsZeroRatedGST = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedGST);
			AssertEquals("", invoiceLine.ZeroRatedGSTDescription);
		}

		public void TestEffectiveIsZeroRatedLevies()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			declaration.JE_IsZeroRatedAll = "Y";
			invoiceHeader.JZ_IsZeroRatedLevies = "";
			invoiceLine.JI_IsZeroRatedLevies = "";
			AssertEquals(true, invoiceLine.EffectiveIsZeroRatedLevies);
			AssertEquals("Yes", invoiceLine.ZeroRatedLeviesDescription);

			declaration.JE_IsZeroRatedAll = "";
			invoiceHeader.JZ_IsZeroRatedLevies = "Y";
			invoiceLine.JI_IsZeroRatedLevies = "";
			AssertEquals(true, invoiceLine.EffectiveIsZeroRatedLevies);
			AssertEquals("Yes", invoiceLine.ZeroRatedLeviesDescription);

			declaration.JE_IsZeroRatedAll = "N";
			invoiceHeader.JZ_IsZeroRatedLevies = "";
			invoiceLine.JI_IsZeroRatedLevies = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedLevies);
			AssertEquals("No", invoiceLine.ZeroRatedLeviesDescription);

			declaration.JE_IsZeroRatedAll = "";
			invoiceHeader.JZ_IsZeroRatedLevies = "N";
			invoiceLine.JI_IsZeroRatedLevies = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedLevies);
			AssertEquals("No", invoiceLine.ZeroRatedLeviesDescription);

			declaration.JE_IsZeroRatedAll = "";
			invoiceHeader.JZ_IsZeroRatedLevies = "";
			invoiceLine.JI_IsZeroRatedLevies = "";
			AssertEquals(false, invoiceLine.EffectiveIsZeroRatedLevies);
			AssertEquals("", invoiceLine.ZeroRatedLeviesDescription);
		}
	}

	public class JobComInvoiceLinePartTest : TestCaseWithFactory
	{
		public void TestSetPartAggregateCodeInfo()
		{
			CusClassification @class = Factory.New<CusClassification>();
			@class.CC_LookupCode = "Machinery Parts";
			@class.CC_TariffNum = "4016.99.79.00J";
			@class.CC_Description = "Machinery parts";
			@class.CC_PartsOfClassification = "4011.62.00.00K";
			@class.CC_ConcessionCode = "987686D";
			PermitCode permit = @class.PermitCodes.AddNew();
			permit.ZO_Code = "CUD";
			permit.ZO_Data = "147981H";
			product.PivotsForBinding.RemoveAndDeleteAll();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_CC = @class.PK;
			pivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTB;

			invoiceLine.JI_PartNo = product.OP_PartNum;

			AssertEquals("Tariff number", @class.CC_TariffNum, invoiceLine.JI_Tariff);
			AssertEquals("PartsOfClassification", @class.CC_PartsOfClassification, invoiceLine.JI_PartsOfClassification);
			AssertEquals("Concession Code", @class.CC_ConcessionCode, invoiceLine.JI_ConcessionCode);
			AssertEquals("Permit code", 1, invoiceLine.PermitCodes.Count);
			AssertEquals("Permit code", "CUD", invoiceLine.PermitCodes[0].ZO_Code);

			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			JobComInvoiceLine lineLoaded = anotherFactory.Load<JobComInvoiceLine>(invoiceLine.PK);

			AssertEquals("PartsOfClassification persisted", @class.CC_PartsOfClassification, lineLoaded.JI_PartsOfClassification);
			AssertEquals("Concession Code persisted", @class.CC_ConcessionCode, lineLoaded.JI_ConcessionCode);
			AssertEquals("Permit code persisted", 1, lineLoaded.PermitCodes.Count);
			AssertEquals("Permit code persisted", "CUD", lineLoaded.PermitCodes[0].ZO_Code);
		}

		public void TestSetPartSetsClassificationTariff()
		{
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals("Classification", classification.PK, invoiceLine.JI_CC);
			AssertEquals("Tariff", classification.CC_TariffNum, invoiceLine.JI_Tariff);
		}

		public void TestCodesAreDefaultedFromOrgSupplierPartIfEmptyonInvoiceLine()
		{
			product.PivotsForBinding.RemoveAndDeleteAll();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "10203040";
			pivot.PermitCodes.AddNew();
			var pivot1 = product.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTE;
			pivot1.CI_TariffNum = "10203050";
			pivot1.PermitCodes.AddNew();
			JobComInvoiceLine line = declaration.InvoiceLines.AddNew();
			line.JI_PartNo = product.OP_PartNum;
			AssertEquals("JobComInvoiceLine.Count", 1, line.PermitCodes.Count);
		}

		public void TestSetClassificationDoesNotClearPart()
		{
			invoiceLine.JI_PartNo = product.OP_PartNum;

			CusClassification anotherClass = Factory.New<CusClassification>();
			anotherClass.CC_TariffNum = "2208.50.09.01D";

			invoiceLine.JI_CC = anotherClass.PK;
			AssertEquals("Part is not cleared", product.OP_PartNum, invoiceLine.JI_PartNo);
			AssertEquals("Part is not cleared", product.PK, invoiceLine.JI_OP);
			AssertEquals("Lookup is changed", anotherClass.PK, invoiceLine.JI_CC);
			AssertEquals("Tariff is Changed", anotherClass.CC_TariffNum, invoiceLine.JI_Tariff);
		}

		public void TestSetTariffSetCustomsQuantityReadOnlyIfTheresNoStatQtyInTheTariffAndNoQuantityEntered()
		{
			invoiceLine.JI_CustomsUnitQty = "NMB";
			AssertEquals("InvoiceLine.JI_CustomsQuantityInfo.ReadOnly", false, invoiceLine.JI_CustomsQuantityInfo.ReadOnly);
			invoiceLine.JI_CustomsUnitQty = "";
			AssertEquals("InvoiceLine.JI_CustomsQuantityInfo.ReadOnly", true, invoiceLine.JI_CustomsQuantityInfo.ReadOnly);
			invoiceLine.JI_CustomsQuantity = 1.10m;
			AssertEquals("InvoiceLine.JI_CustomsQuantityInfo.ReadOnly", false, invoiceLine.JI_CustomsQuantityInfo.ReadOnly);
			invoiceLine.JI_CustomsQuantity = 0.00m;
			AssertEquals("InvoiceLine.JI_CustomsQuantityInfo.ReadOnly", true, invoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		public void TestCustomsQuantityConverter()
		{
			invoiceLine.JI_InvoiceQuantity = 20;
			invoiceLine.JI_InvoiceUQ = "NMB";
			invoiceLine.JI_CustomsUnitQty = "NMB";
			AssertEquals("Customs Quantity", 20m, invoiceLine.JI_CustomsQuantity);
		}

		public void TestCustomsQuantityConverterUsingPart()
		{
			invoiceLine.JI_PartNo = product.OP_PartNum;
			invoiceLine.JI_InvoiceQuantity = 10;
			invoiceLine.JI_InvoiceUQ = "CTN";
			AssertEquals("PreCondition:CustomsUQ", "NMB", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("Customs Quantity in NMB", 2000m, invoiceLine.JI_CustomsQuantity);
		}

		public void TestSetTariffSetCustomsSecondUQ()
		{
			invoiceLine.JI_Tariff = "2208.20.29.01D";//SecondUQ = "LTR";
			AssertEquals("Customs UQ", "LPA", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("Customs UQ2", "LTR", invoiceLine.JI_SupplementaryUQ);
		}

		public void TestCalculateSecondQuantity()
		{
			CusClassification classWithSecondUQ = Factory.New<CusClassification>();
			classWithSecondUQ.CC_TariffNum = "2208.20.29.01D";
			classWithSecondUQ.CC_LookupCode = "ClassWithSecondUQ";
			classWithSecondUQ.CC_ClassificationType = CusClassification.ClassificationType.Both;
			classWithSecondUQ.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			product.PivotsForBinding.RemoveAndDeleteAll();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_CC = classWithSecondUQ.PK;
			pivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTB;

			OrgPartUnit cTN_BOT = product.PartUnits.AddNew();
			cTN_BOT.OF_QuantityInParent = 20;
			cTN_BOT.OF_PackType = "BOT";
			cTN_BOT.OF_ParentPackType = "CTN";

			OrgPartUnit bOT_LTR = product.PartUnits.AddNew();
			bOT_LTR.OF_QuantityInParent = 0.3m;
			bOT_LTR.OF_PackType = "LTR";
			bOT_LTR.OF_ParentPackType = "BOT";

			OrgPartUnit lTR_LPA = product.PartUnits.AddNew();
			lTR_LPA.OF_QuantityInParent = 0.01m;
			lTR_LPA.OF_PackType = "LPA";
			lTR_LPA.OF_ParentPackType = "LTR";

			Factory.Save();

			invoiceLine.JI_PartNo = product.OP_PartNum;
			invoiceLine.JI_InvoiceUQ = "CTN";
			invoiceLine.JI_InvoiceQuantity = 20;
			AssertEquals("PreCondition:Second UQ", "LTR", invoiceLine.JI_SupplementaryUQ);
			AssertEquals("PreCondition:SecondUQ readonly", true, invoiceLine.JI_SupplementaryUQInfo.ReadOnly);
			AssertEquals("PreCondition:Second UQ", "LPA", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("CustomsQuantity", 1.2m, invoiceLine.JI_CustomsQuantity);
			AssertEquals("Second Quantity", 120m, invoiceLine.JI_SupplementaryQty);
		}

		public void TestAggregateAddInfoFromClassification()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = new DummyInvoiceLineCollection(invoice).AddNew();

			AssertEquals(0, invoiceLine.PermitCodes.Count);
			AssertEquals(0, invoiceLine.ProhibitedCodes.Count);
			AssertEquals(0, invoiceLine.OtherInfos.Count);

			invoiceLine.AggregateAddInfoFromClassificationForTesting();

			AssertEquals(0, invoiceLine.PermitCodes.Count);
			AssertEquals(0, invoiceLine.ProhibitedCodes.Count);
			AssertEquals(0, invoiceLine.OtherInfos.Count);

			invoiceLine.CreateClassification();

			PermitCode permitCode = invoiceLine.PermitCodes.AddNew();
			permitCode.ZO_Code = "ABC";
			permitCode.ZO_Data = "tv";
			(invoiceLine.Classification).CC_PermitCodes = "ABC=tv^DEF=leppard^GHI=jklmnop";

			ProhibitedCode prohibitedCode = invoiceLine.ProhibitedCodes.AddNew();
			prohibitedCode.ZO_Code = "DEF";
			prohibitedCode.ZO_Data = "leppard";
			(invoiceLine.Classification).CC_ProhibitedCodes = "ABC=tv^ACB=tv^DEF=leppard^HIG=jklmnop";

			LineOtherInfo lineOtherInfo = invoiceLine.OtherInfos.AddNew();
			lineOtherInfo.ZO_Code = "GHI";
			lineOtherInfo.ZO_Data = "jklmnop";
			(invoiceLine.Classification).CC_OtherInfos = "DEF=leppard^GHI=jklmnop^FED=leppard^HIG=jklmnop";

			invoiceLine.AggregateAddInfoFromClassificationForTesting();

			AssertEquals(3, invoiceLine.PermitCodes.Count);
			AssertEquals(4, invoiceLine.ProhibitedCodes.Count);
			AssertEquals(4, invoiceLine.OtherInfos.Count);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var lineInOtherFactory = otherFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
			AssertEquals(3, lineInOtherFactory.PermitCodes.Count);
			AssertEquals(4, lineInOtherFactory.ProhibitedCodes.Count);
			AssertEquals(4, lineInOtherFactory.OtherInfos.Count);
		}

		public void TestNoExceptionWhenInvoiceLineRefreshAfterOrgPartDetailsHasBeenUpdated()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPTEST";
			invoice.JZ_OH_Buyer = importer.PK;
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUPTEST";
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;

			var line = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var classification = Factory.New<CusClassification>();
			classification.FillWithValidTestData();

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "AMYTEST";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_CC = classification.PK;
			pivot.CI_TariffNum = "1010101010";
			pivot.CI_RN_NKCountryOfOrigin = "AU";
			var relatedOrg1 = product.RelatedOrganisations.AddNew();
			relatedOrg1.OU_OH = importer.PK;
			relatedOrg1.OU_Relationship = "OWN";
			var relatedOrg2 = product.RelatedOrganisations.AddNew();
			relatedOrg2.OU_OH = supplier.PK;
			relatedOrg2.OU_Relationship = "SUP";

			line.JI_PartNo = product.OP_PartNum;
			line.JI_OP = product.PK;
			AssertEquals(pivot, line.Pivot);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var companyData = importer.CompanyData;
			companyData.ImporterOverride = true;
			companyData.OB_IMProductValueDefaultOptions = "OVR,TAR,PREFF";
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;

			line.JI_CC = ZGuid.Empty;
			line.JI_CountryOfOrigin = ZString.Empty;
			invoice.JZ_JE = declaration.PK;
			AssertEquals("Line Classification refreshed ", pivot.CI_CC, line.JI_CC);
			AssertEquals("Line Country of Origin refreshed ", pivot.CI_RN_NKCountryOfOrigin, line.JI_CountryOfOrigin);
			AssertEquals("Line Tariff refreshed ", pivot.CI_TariffNum, line.JI_Tariff);
		}

		#region Implementation

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		CusClassification classification;
		OrgSupplierPart product;
		OrgHeader supplier;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());

			invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_OH_Supplier = supplier.PK;

			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = CusClassification.ClassificationType.Both;
			classification.CC_TariffNum = "6103.41.00.11K";
			classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TESTPART";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_CC = classification.PK;

			OrgPartRelation relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = invoiceHeader.JZ_OH_Supplier;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			OrgPartUnit cTN_BOX = product.PartUnits.AddNew();
			cTN_BOX.OF_PackType = "BOX";
			cTN_BOX.OF_ParentPackType = "CTN";
			cTN_BOX.OF_QuantityInParent = 20;

			OrgPartUnit bOX_NMB = product.PartUnits.AddNew();
			bOX_NMB.OF_PackType = "NMB";
			bOX_NMB.OF_ParentPackType = "BOX";
			bOX_NMB.OF_QuantityInParent = 10;
			Factory.Save();
		}

		class DummyInvoiceLineCollection : DependentBusinessObjectCollection<JobComInvoiceLinesForTesting, JobComInvoiceHeader>
		{
			public DummyInvoiceLineCollection(JobComInvoiceHeader invoice)
				: base(invoice)
			{
			}
		}

		class JobComInvoiceLinesForTesting : JobComInvoiceLine
		{
			public JobComInvoiceLinesForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public void CreateClassification()
			{
				JI_CC = Factory.NewWithValidTestData<CusClassification>().PK;
			}

			public void AggregateAddInfoFromClassificationForTesting()
			{
				AggregateAddInfoFromClassification();
			}
		}

		#endregion
	}

	public class Test : TestCaseWithFactory
	{
		public void TestLineHasErrorResponseOverride()
		{
			invoiceLine.JI_HadErrorInLastResponse = true;
			AssertEquals("InvoiceLine.LineHasErrorResponse", true, invoiceLine.JI_HadErrorInLastResponse);
			invoiceLine.JI_HadErrorInLastResponse = false;
			AssertEquals("InvoiceLine.LineHasErrorResponse", false, invoiceLine.JI_HadErrorInLastResponse);
		}

		public void TestLoadPropertiesFromAddInfoWithGuidType()
		{
			var guid = Guid.NewGuid();
			((IAddInfoManager)invoiceLine).AddInfo.UpdateAddInfoFromString($"JI_ParentLine={guid}");
			AssertEquals("Guid property loaded", guid, invoiceLine.JI_JI_ParentLine);
		}

		public void TestParentLineSetsParentLineGuidInAddInfo()
		{
			JobComInvoiceLine line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ParentLineNo = line2.JI_LineNoString;
			AssertEquals("ParentLineGuid set", line2.PK, invoiceLine.JI_JI_ParentLine);
		}

		public void TestDeleteParentLineFreeChildLines()
		{
			JobComInvoiceLine line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ParentLineNo = line2.JI_LineNoString;
			AssertEquals("PreCondition:ParentLineGuid set", line2.PK, invoiceLine.JI_JI_ParentLine);

			line2.Delete();//ParentLine deleted
			AssertEquals("ParentLine Guid cleared", ZGuid.Empty, invoiceLine.JI_JI_ParentLine);
		}

		public void TestInvalidParentLineClearsParentLineGuid()
		{
			JobComInvoiceLine line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ParentLineNo = line2.JI_LineNoString;
			AssertEquals("PreCondition:ParentLineGuid set", line2.PK, invoiceLine.JI_JI_ParentLine);

			invoiceLine.JI_ParentLineNo = "5";
			AssertEquals("Invalid line no", ZGuid.Empty, invoiceLine.JI_JI_ParentLine);

			invoiceLine.JI_ParentLineNo = line2.JI_LineNoString;
			invoiceLine.JI_ParentLineNo = "";
			AssertEquals("Invalid line no", ZGuid.Empty, invoiceLine.JI_JI_ParentLine);
		}

		#region Implementation

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoiceHeader.PK;
		}

		#endregion
	}

	public class InvoiceLinePartClassificationTariffDescriptionSyncroniserTest : InvoiceLinePartClassificationTariffDescriptionSyncroniserAbstractTest
	{
		protected override ZString TariffCode
		{
			get { return Tariff.U0_Tariff; }
		}

		protected override ZString TariffDescription
		{
			get { return Tariff.U0_Description; }
		}

		protected override ZString TariffCode2
		{
			get { return Tariff2.U0_Tariff; }
		}

		protected override ZString TariffDescription2
		{
			get { return Tariff2.U0_Description; }
		}

		protected override Type DeclarationTypeForTest
		{
			get { return typeof(JobDeclaration); }
		}

		protected NZCClassification Tariff
		{
			get
			{
				if (fTariff == null)
				{
					fTariff = NZCClassification.New(Factory);
					fTariff.U0_Tariff = "0000.00.00.00K";
					fTariff.U0_Description = "Tariff_Description";
					fTariff.U0_DateActiveFrom = new ZDateTime(1980, 1, 1, 0, 0, 0);
					fTariff.U0_DateActiveTo = new ZDateTime(3000, 12, 31, 0, 0, 0);
				}
				return fTariff;
			}
		}
		NZCClassification fTariff;

		protected NZCClassification Tariff2
		{
			get
			{
				if (fTariff2 == null)
				{
					fTariff2 = NZCClassification.New(Factory);
					fTariff2.U0_Tariff = "0000.00.00.02K";
					fTariff2.U0_Description = "Tariff_Description2";
					fTariff2.U0_DateActiveFrom = new ZDateTime(1980, 1, 1, 0, 0, 0);
					fTariff2.U0_DateActiveTo = new ZDateTime(3000, 12, 31, 0, 0, 0);
				}
				return fTariff;
			}
		}
		NZCClassification fTariff2;
		protected override ZString ExpectedDescriptionFromMergeOfMultipleLines
		{
			get { return base.ExpectedDescriptionFromMergeOfMultipleLines.ToUpper(); }
		}

		protected override ZString ExpectedDescriptionFromMergeOfOneLine
		{
			get { return base.ExpectedDescriptionFromMergeOfOneLine.ToUpper(); }
		}
	}
}
