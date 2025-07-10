using System;
using System.Drawing;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(JobDeclarationDrawbackSupporter))]
	class JobDeclarationDrawbackSupporterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestExpAndRevDate()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			var invoice = drawback.Invoices.AddNew();

			var date = ZDateTime.UtcToday.AddDays(-3);
			var expExpDateType = Factory.New<RefSysConfigType>();
			expExpDateType.ZRT_ConfigCode = "CBP7553ED";
			expExpDateType.ZRT_Description = "Test CBP7553ED";
			expExpDateType.ZRT_LongDescription = "Test CBP7553ED";
			var expExpDate = Factory.New<RefSysConfig>();
			expExpDate.ZRC_ZRT_NKConfigCode = expExpDateType.ZRT_ConfigCode;
			expExpDate.ZRC_DecimalValue = 0m;
			expExpDate.ZRC_StartDate = date.AddDays(-10);
			expExpDate.ZRC_EndDate = date.AddDays(10);
			expExpDate.ZRC_StringValue = "20210601";

			var expRevDateType = Factory.New<RefSysConfigType>();
			expRevDateType.ZRT_ConfigCode = "CBP7553RD";
			expRevDateType.ZRT_Description = "Test CBP7553RD";
			expRevDateType.ZRT_LongDescription = "Test CBP7553RD";
			var expRevDate = Factory.New<RefSysConfig>();
			expRevDate.ZRC_ZRT_NKConfigCode = expRevDateType.ZRT_ConfigCode;
			expRevDate.ZRC_DecimalValue = 0m;
			expRevDate.ZRC_StartDate = date.AddDays(-10);
			expRevDate.ZRC_EndDate = date.AddDays(10);
			expRevDate.ZRC_StringValue = "20210602";
			Factory.Save();

			var print = new JobDeclarationDrawbackSupporter(drawback, UpdateActionCode.Add);
			AssertEquals("20210601", print.CBP7553ExpDate);
			AssertEquals("20210602", print.CBP7553RevDate);
		}

		public void TestExportSectionInvoiceLinesOrder()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			var invoice = drawback.Invoices.AddNew();

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.US_DRWIsForImportSection = true;
			line1.US_DRWIsForExportSection = true;
			line1.JI_LineNo = 1;
			line1.US_DRWEntryDate = ZDateTime.Today;

			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.US_DRWIsForImportSection = true;
			line2.US_DRWIsForExportSection = true;
			line2.JI_LineNo = 2;
			line2.US_DRWEntryDate = ZDateTime.Today;

			var line3 = invoice.JobComInvoiceLines.AddNew();
			line3.US_DRWIsForImportSection = true;
			line3.US_DRWIsForExportSection = true;
			line3.JI_LineNo = 3;
			line3.US_DRWEntryDate = ZDateTime.Today.AddDays(-1);

			var line4 = invoice.JobComInvoiceLines.AddNew();
			line4.US_DRWIsForImportSection = true;
			line4.US_DRWIsForExportSection = true;
			line4.JI_LineNo = 4;
			line4.US_DRWEntryDate = ZDateTime.Today.AddDays(-1);

			Factory.Save();

			var supporter = new JobDeclarationDrawbackSupporter(drawback, UpdateActionCode.Add);
			AssertEquals(line3, supporter.ExportSectionInvoiceLines[0]);
			AssertEquals(line4, supporter.ExportSectionInvoiceLines[1]);
			AssertEquals(line1, supporter.ExportSectionInvoiceLines[2]);
			AssertEquals(line2, supporter.ExportSectionInvoiceLines[3]);
		}

		public void TestChronologicalSummaryInvoiceLinesOrder()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			var invoice = drawback.Invoices.AddNew();

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.US_DRWIsForImportSection = true;
			line1.US_DRWIsForExportSection = true;
			line1.JI_LineNo = 1;
			line1.US_DRWExportDate = ZDateTime.Today;

			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.US_DRWIsForImportSection = true;
			line2.US_DRWIsForExportSection = true;
			line2.JI_LineNo = 2;
			line2.US_DRWExportDate = ZDateTime.Today.AddDays(1);

			var line3 = invoice.JobComInvoiceLines.AddNew();
			line3.US_DRWIsForImportSection = true;
			line3.US_DRWIsForExportSection = true;
			line3.JI_LineNo = 3;
			line3.US_DRWExportDate = ZDateTime.Today.AddDays(-2);

			var line4 = invoice.JobComInvoiceLines.AddNew();
			line4.US_DRWIsForImportSection = true;
			line4.US_DRWIsForExportSection = true;
			line4.JI_LineNo = 4;
			line4.US_DRWExportDate = ZDateTime.Today.AddDays(-1);

			Factory.Save();

			var supporter = new JobDeclarationDrawbackSupporter(drawback, UpdateActionCode.Add);
			AssertEquals(line3, supporter.ChronologicalSummaryInvoiceLines[0]);
			AssertEquals(line4, supporter.ChronologicalSummaryInvoiceLines[1]);
			AssertEquals(line1, supporter.ChronologicalSummaryInvoiceLines[2]);
			AssertEquals(line2, supporter.ChronologicalSummaryInvoiceLines[3]);
		}

		public void TestDrawbackNoticeOfIntentMembers()
		{
			GlbStaff.CurrentUser.GS_IsSystemAccount = false;
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;

			var supporter = new JobDeclarationDrawbackSupporter(drawback, UpdateActionCode.Add);
			drawback.US_EntryType = EntryTypeList.Codes.OtherDrawback;
			drawback.US_DRWSection = "3333";
			AssertEquals("Y", supporter.IsFilledAsSameConditionUnderNAFTA);
			AssertEquals(ZString.Empty, supporter.IsFilledAsDistilledSpiritsWineBeers);
			drawback.US_DRWSection = "5062";
			AssertEquals(ZString.Empty, supporter.IsFilledAsSameConditionUnderNAFTA);
			AssertEquals("Y", supporter.IsFilledAsDistilledSpiritsWineBeers);

			var exporter = Factory.NewWithValidTestData<OrgHeader>();
			exporter.OH_FullName = "Exporter";
			exporter.MainAddress.OA_Address1 = "Exporter Address";
			exporter.MainAddress.OA_Address2 = "Exporter Address2";

			exporter.MainAddress.OA_State = "NewYork";
			exporter.MainAddress.OA_City = "NewYork";
			exporter.MainAddress.OA_PostCode = "2233";
			exporter.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-12345", Core.Constants.CountryCodes.UnitedStates);

			var exportingCarrier = Factory.NewWithValidTestData<USCarrierCombined>();
			exportingCarrier.UI_Code = "ECC";
			exportingCarrier.UI_Name = "Exporting Carrier Company";

			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "TV";
			broker.GS_FullName = "TIM VAN";
			broker.GS_Title = "Mr";
			broker.SignatureImage = new Bitmap(2, 1);
			drawback.JE_GS_NKCusAgent = "TV";

			USCustomsDataRegistry.Instance.EntryDeclarant.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			USCustomsDataRegistry.Instance.IsAttorneyInFact.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var invoice = drawback.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();
			line.US_DRWIsForExportSection = true;
			line.US_DRWIsForImportSection = true;
			line.ExporterOrDestroyer.OrganisationPK = exporter.PK;
			var locationOfMerchandies = Factory.NewWithValidTestData<OrgHeader>();
			locationOfMerchandies.OH_FullName = "Merchandise Location";
			locationOfMerchandies.MainAddress.OA_Address1 = "Merchandise address";
			var locationOfDestruction = Factory.NewWithValidTestData<OrgHeader>();
			locationOfDestruction.OH_FullName = "Destruction Location";
			locationOfDestruction.MainAddress.OA_Address1 = "Destruction address";
			line.US_DRWMethodOfDestruction = "Destruction Method";
			line.LocationOfDestruction.OrganisationPK = locationOfDestruction.PK;
			line.LocationOfMerchandise.OrganisationPK = locationOfMerchandies.PK;
			line.US_DRWClaimAmountOverriden_New = true;
			line.US_DRWDeclaredMPF = 0m;
			line.US_DRWDeclaredHMF = 0m;
			line.US_DRWDeclaredOtherFees = 0m;
			line.US_DRWWeightedRatio = 1m;
			line.US_DRWMPFWeightedRatio = 1m;
			line.DRWImportQuantity = 100m;
			line.DRWExportQuantity = 100m;
			line.DeclaredVFD = 10m;
			line.DeclaredTax = 20m;
			line.LineDuty = line.Claims.DutyClaim.DeclaredAmount * 0.1m;
			line.US_UI_NKCarrierSCAC = "ECC";

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1###", "A port", startDate, endDate);
			newFactory.Save();

			line.US_DRWIntendedPortOfExport = "1###";
			line.US_DRWTENo = "123456";
			Factory.Save();

			supporter = new JobDeclarationDrawbackSupporter(drawback, UpdateActionCode.Add);
			var noticeOfIntentLine = supporter.DrawbackNoticeOfIntentDocLines[0];
			AssertEquals("Exporter", noticeOfIntentLine["US_DRWExporterName"]);
			AssertEquals("Exporter Address", noticeOfIntentLine["ExporterOrDestroyerAddress"]);
			AssertEquals("Exporter Address2", noticeOfIntentLine["ExporterOrDestroyerAddressTwo"]);
			AssertEquals("NewYork", noticeOfIntentLine["ExporterOrDestroyerCity"]);
			AssertEquals("NewYork", noticeOfIntentLine["ExporterOrDestroyerState"]);
			AssertEquals("2233", noticeOfIntentLine["ExporterOrDestroyerZip"]);
			AssertEquals("12-12345", noticeOfIntentLine["ExporterOrDestroyerIDNumber"]);
			AssertEquals("Merchandise Location\nMERCHANDISE ADDRESS", noticeOfIntentLine["LocationOfMerchandise"]);
			AssertEquals("Destruction Location\nDESTRUCTION ADDRESS", noticeOfIntentLine["LocationOfDestruction"]);
			AssertEquals("Destruction Method", noticeOfIntentLine["MethodOfDestruction"]);
			AssertEquals(20.99m, noticeOfIntentLine["DrawbackAmount"]);
			AssertEquals("1### - A port", noticeOfIntentLine["IntendedPortOfExport"]);
			AssertEquals("123456", noticeOfIntentLine["TENo"]);
			AssertEquals("Exporting Carrier Company", noticeOfIntentLine["ExportingCarrierName"]);

			AssertEquals("TIM VAN", supporter.Preparer);
			AssertEquals("ATTY-IN-FACT", supporter.Title);
			AssertEquals(ZDateTime.Today.Date, supporter.PrintDate.Date);
			AssertNotNull(supporter.BrokerSignatureImage);

			AssertEquals(20.99m, supporter.DrawbackTotalAmount);

			drawback.JE_GS_NKCusAgent = ZString.Empty;
			Factory.Save();

			USCustomsDataRegistry.Instance.PrintSignatureOnEntryDocsBroker.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, broker.PK.ToGuid());
			supporter = new JobDeclarationDrawbackSupporter(drawback, UpdateActionCode.Add);
			AssertEquals("TIM VAN", supporter.Preparer);
			AssertEquals("ATTY-IN-FACT", supporter.Title);
			AssertEquals(ZDateTime.Today.Date, supporter.PrintDate.Date);
			AssertNotNull(supporter.BrokerSignatureImage);

			USCustomsDataRegistry.Instance.EntryDeclarant.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			USCustomsDataRegistry.Instance.PrintSignatureOnEntryDocsBroker.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			supporter = new JobDeclarationDrawbackSupporter(drawback, UpdateActionCode.Add);
			AssertEquals("CargoWise Support", supporter.Preparer);
			AssertEquals("ATTY-IN-FACT", supporter.Title);
			AssertNull(supporter.BrokerSignatureImage);

			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			drawback.US_EntryType = ACEDrawbackProvisionsList.Codes._03;
			AssertEquals("IsRejectedMerchandiseDrawback", true, supporter.IsRejectedMerchandiseDrawback);
			AssertEquals("IsUnusedMerchandiseDrawback_J1", false, supporter.IsUnusedMerchandiseDrawback_J1);
			AssertEquals("IsUnusedMerchandiseDrawback_J2", false, supporter.IsUnusedMerchandiseDrawback_J2);
			AssertEquals("IsManufacturingDrawback", false, supporter.IsManufacturingDrawback);
			drawback.US_EntryType = ACEDrawbackProvisionsList.Codes._08;
			AssertEquals("IsRejectedMerchandiseDrawback", false, supporter.IsRejectedMerchandiseDrawback);
			AssertEquals("IsUnusedMerchandiseDrawback_J1", true, supporter.IsUnusedMerchandiseDrawback_J1);
			AssertEquals("IsUnusedMerchandiseDrawback_J2", false, supporter.IsUnusedMerchandiseDrawback_J2);
			AssertEquals("IsManufacturingDrawback", false, supporter.IsManufacturingDrawback);
			drawback.US_EntryType = ACEDrawbackProvisionsList.Codes._09;
			AssertEquals("IsRejectedMerchandiseDrawback", false, supporter.IsRejectedMerchandiseDrawback);
			AssertEquals("IsUnusedMerchandiseDrawback_J1", false, supporter.IsUnusedMerchandiseDrawback_J1);
			AssertEquals("IsUnusedMerchandiseDrawback_J2", true, supporter.IsUnusedMerchandiseDrawback_J2);
			AssertEquals("IsManufacturingDrawback", false, supporter.IsManufacturingDrawback);
		}

		public void TestDrawbackNoticeIntentDocLineSupressSSN()
		{
			TestDrawbackNoticeSupressSSNFromOtherDeclaratoinTypesCore("ExporterOrDestroyerIDNumber",
				(drawback, exporter) =>
				{
					var invoice = drawback.Invoices.AddNew();
					var line = invoice.InvoiceLines.AddNew();
					line.US_DRWIsForExportSection = true;
					line.US_DRWIsForImportSection = true;
					line.ExporterOrDestroyer.OrganisationPK = exporter.PK;
				},
				supporter => supporter.DrawbackNoticeOfIntentDocLines[0]);
		}

		public void TestDrawbackNoticeClaimantIdentificationSupressSSN()
		{
			TestDrawbackNoticeSupressSSNFromOtherDeclaratoinTypesCore("ClaimantIdentification4Print",
				(drawback, exporter) =>
				{
					drawback.JE_OH_Importer = exporter.PK;
				},
				supporter => supporter
				);
		}

		void TestDrawbackNoticeSupressSSNFromOtherDeclaratoinTypesCore(string propertyName, Action<JobDeclaration, OrgHeader> fillInDrawbackProperty, Func<JobDeclarationDrawbackSupporter, BusinessObject> findObject2Check)
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;

			var supporter = new JobDeclarationDrawbackSupporter(drawback, UpdateActionCode.Add);

			var exporter = Factory.NewWithValidTestData<OrgHeader>();
			exporter.OH_FullName = "Exporter";
			exporter.MainAddress.OA_Address1 = "Exporter Address";
			exporter.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "999-99-9999", Core.Constants.CountryCodes.UnitedStates);

			fillInDrawbackProperty(drawback, exporter);
			Factory.Save();

			var noticeOfIntentLine = findObject2Check(supporter);
			AssertEquals(propertyName + ", SSN only then registration number print empty.", string.Empty, noticeOfIntentLine[propertyName]);

			supporter = new JobDeclarationDrawbackSupporter(drawback, UpdateActionCode.Update);
			var sinNumber = "999-99-8888";
			exporter.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, sinNumber, Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();
			noticeOfIntentLine = findObject2Check(supporter);
			AssertEquals(propertyName + ", SIN&SSN exist then registration number print SIN.", sinNumber, noticeOfIntentLine[propertyName]);

			supporter = new JobDeclarationDrawbackSupporter(drawback, UpdateActionCode.Update);
			var cbnNumber = "999-99-7777";
			exporter.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, cbnNumber, Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();
			noticeOfIntentLine = findObject2Check(supporter);
			Assert(propertyName + " SIN&SSN&CBN, exist then registration number print SIN or CBN.", noticeOfIntentLine[propertyName].EqualsAny(sinNumber, cbnNumber));
		}

		public void TestCalculationExhibitsLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			declaration.JE_OwnerRef = "REF";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var jobDeclarationDrawbackSummary = new JobDeclarationDrawbackSupporter(declaration, UpdateActionCode.Add);
			AssertEquals("Client Reference", "REF", jobDeclarationDrawbackSummary.ClientReference);
			AssertEquals("0 duty line", 0, jobDeclarationDrawbackSummary.DutyCalculationLines.Count);
			AssertEquals("0 tax line", 0, jobDeclarationDrawbackSummary.TaxCalculationLines.Count);
			AssertEquals("0 MPF line", 0, jobDeclarationDrawbackSummary.MPFCalculationLines.Count);
			AssertEquals("0 HMF line", 0, jobDeclarationDrawbackSummary.HMFCalculationLines.Count);
			AssertEquals("0 other fees line", 0, jobDeclarationDrawbackSummary.OtherFeesCalculationLines.Count);
			AssertEquals("Has no duty line", "N", jobDeclarationDrawbackSummary.HasDutyCalculationLines);
			AssertEquals("Has no tax line", "N", jobDeclarationDrawbackSummary.HasDutyCalculationLines);
			AssertEquals("Has no MPF line", "N", jobDeclarationDrawbackSummary.HasDutyCalculationLines);
			AssertEquals("Has no HMF line", "N", jobDeclarationDrawbackSummary.HasDutyCalculationLines);
			AssertEquals("Has no other fees line", "N", jobDeclarationDrawbackSummary.HasDutyCalculationLines);
			AssertEquals("No MPF refund requested", ZString.Empty, jobDeclarationDrawbackSummary.HasMPFRefundRequested);

			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWClaimAmountOverriden_New = true;
			invoiceLine.US_DRWWeightedRatio = 1m;
			invoiceLine.US_DRWMPFWeightedRatio = 1m;
			invoiceLine.DRWImportQuantity = 1m;
			invoiceLine.DRWImportUQ = ABIUnitOfMeasureList.Codes.Barrels;
			invoiceLine.DRWExportQuantity = 1m;
			invoiceLine.DeclaredVFD = 100m;
			invoiceLine.DeclaredTax = 200m;
			invoiceLine.DeclaredHMF = 300m;
			invoiceLine.DeclaredMPF = 400m;
			invoiceLine.DeclaredOtherFees = 500m;
			invoiceLine.LineDuty = invoiceLine.Claims.DutyClaim.DeclaredAmount * 0.5m;

			Factory.Save();

			jobDeclarationDrawbackSummary = new JobDeclarationDrawbackSupporter(declaration, UpdateActionCode.Add);
			AssertEquals("1 duty line", 1, jobDeclarationDrawbackSummary.DutyCalculationLines.Count);
			AssertEquals("1 tax line", 1, jobDeclarationDrawbackSummary.TaxCalculationLines.Count);
			AssertEquals("1 MPF line", 1, jobDeclarationDrawbackSummary.MPFCalculationLines.Count);
			AssertEquals("1 HMF line", 1, jobDeclarationDrawbackSummary.HMFCalculationLines.Count);
			AssertEquals("1 other fees line", 1, jobDeclarationDrawbackSummary.OtherFeesCalculationLines.Count);
			AssertEquals("Has duty line", ZString.Empty, jobDeclarationDrawbackSummary.HasDutyCalculationLines);
			AssertEquals("Has tax line", ZString.Empty, jobDeclarationDrawbackSummary.HasDutyCalculationLines);
			AssertEquals("Has MPF line", ZString.Empty, jobDeclarationDrawbackSummary.HasDutyCalculationLines);
			AssertEquals("Has HMF line", ZString.Empty, jobDeclarationDrawbackSummary.HasDutyCalculationLines);
			AssertEquals("Has Other fees line", ZString.Empty, jobDeclarationDrawbackSummary.HasDutyCalculationLines);
			AssertEquals("No MPF refund requested", "Y", jobDeclarationDrawbackSummary.HasMPFRefundRequested);
		}

		public void TestDrawbackSummaryDocumentMember()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			var invoice = drawback.Invoices.AddNew();

			var line1 = invoice.InvoiceLines.AddNew();
			line1.US_DRWIsForImportSection = true;
			line1.US_DRWIsForExportSection = true;
			line1.JI_Tariff = "1000000";
			line1.US_ImportEntryNo = "ENRTY1";
			line1.US_DRWImportEntryLine = 1;
			line1.US_DRWPort = "1011";
			line1.US_DRWClaimAmountOverriden_New = true;
			line1.US_DRWImportQuantity = 1m;
			line1.US_DRWExportQuantity = 1m;
			line1.DeclaredVFD = 100m;
			var iMPtariff = line1.DrawbackAdditionalImportTariffNumbers.AddNew();
			iMPtariff.US_Tariff = "1000001";
			iMPtariff = line1.DrawbackAdditionalImportTariffNumbers.AddNew();
			iMPtariff.US_Tariff = "1000002";
			iMPtariff = line1.DrawbackAdditionalImportTariffNumbers.AddNew();
			iMPtariff.US_Tariff = "1000003";
			iMPtariff = line1.DrawbackAdditionalImportTariffNumbers.AddNew();
			iMPtariff.US_Tariff = "1000004";

			line1.LineDuty = line1.Claims.DutyClaim.DeclaredAmount * 1m;

			var line2 = invoice.InvoiceLines.AddNew();
			line2.US_DRWIsForImportSection = true;
			line2.US_DRWIsForExportSection = true;
			line2.US_ImportEntryNo = "ENRTY1";
			line2.JI_Tariff = "1000000";
			line2.US_DRWImportEntryLine = 2;
			line2.US_DRWPort = "1011";
			line2.US_DRWClaimAmountOverriden_New = true;
			line2.US_DRWImportQuantity = 1m;
			line2.US_DRWExportQuantity = 1m;
			line2.DeclaredVFD = 200m;
			iMPtariff = line2.DrawbackAdditionalImportTariffNumbers.AddNew();
			iMPtariff.US_Tariff = "1000005";
			iMPtariff = line2.DrawbackAdditionalImportTariffNumbers.AddNew();
			iMPtariff.US_Tariff = "1000006";
			iMPtariff = line2.DrawbackAdditionalImportTariffNumbers.AddNew();
			iMPtariff.US_Tariff = "1000007";
			line2.LineDuty = line2.Claims.DutyClaim.DeclaredAmount * 1m;

			var line3 = invoice.InvoiceLines.AddNew();
			line3.US_ImportEntryNo = ZString.Empty;
			line3.US_DRWIsForImportSection = true;
			line3.US_DRWIsForExportSection = true;
			line3.JI_Tariff = "1000007";
			line3.US_DRWCertOfManufacture = "CM1234";
			line3.US_DRWPort = "2022";
			line3.US_DRWClaimAmountOverriden_New = true;
			line3.US_DRWImportQuantity = 1m;
			line3.US_DRWExportQuantity = 1m;
			line3.DeclaredVFD = 300m;
			var eXPtariff = line3.DrawbackAdditionalExportTariffNumbers.AddNew();
			eXPtariff.CY_Data = "2000000";
			eXPtariff = line3.DrawbackAdditionalExportTariffNumbers.AddNew();
			eXPtariff.CY_Data = "2000004";
			eXPtariff = line3.DrawbackAdditionalExportTariffNumbers.AddNew();
			eXPtariff.CY_Data = "2000005";
			eXPtariff = line3.DrawbackAdditionalExportTariffNumbers.AddNew();
			eXPtariff.CY_Data = "2000006";
			line3.LineDuty = line3.Claims.DutyClaim.DeclaredAmount * 1m;

			var line4 = invoice.InvoiceLines.AddNew();
			line4.US_ImportEntryNo = ZString.Empty;
			line4.US_DRWIsForImportSection = true;
			line4.US_DRWIsForExportSection = true;
			line4.JI_Tariff = "1000003";
			line4.US_DRWCertOfManufacture = "CM1234";
			line4.US_DRWPort = "2022";
			line4.US_DRWClaimAmountOverriden_New = true;
			line4.US_DRWImportQuantity = 1m;

			line4.US_DRWExportQuantity = 1m;
			line4.DeclaredVFD = 400m;
			eXPtariff = line3.DrawbackAdditionalExportTariffNumbers.AddNew();
			eXPtariff.CY_Data = "2000001";
			line4.LineDuty = line4.Claims.DutyClaim.DeclaredAmount * 1m;

			var line5 = invoice.InvoiceLines.AddNew();
			line5.US_ImportEntryNo = ZString.Empty;
			line5.US_DRWCertOfManufacture = ZString.Empty;
			line5.US_DRWIsForImportSection = true;
			line5.US_DRWIsForExportSection = true;
			line5.JI_Tariff = "1000007";
			line5.US_ExportTariff = "2000002";
			line5.US_DRWPort = "3033";
			line5.US_DRWClaimAmountOverriden_New = true;
			line5.US_DRWImportQuantity = 1m;
			line5.US_DRWExportQuantity = 1m;
			line5.DeclaredVFD = 500m;
			line5.LineDuty = line5.Claims.DutyClaim.DeclaredAmount * 1m;

			var line6 = invoice.InvoiceLines.AddNew();
			line6.US_DRWIsForImportSection = true;
			line6.US_ImportEntryNo = ZString.Empty;
			line6.US_DRWCertOfManufacture = ZString.Empty;
			line6.US_DRWIsForExportSection = true;
			line6.JI_Tariff = "1000007";
			line6.US_ExportTariff = "2000003";
			line6.US_DRWPort = "4044";
			line6.US_DRWClaimAmountOverriden_New = true;
			line6.US_DRWImportQuantity = 1m;
			line6.US_DRWExportQuantity = 1m;
			line6.DeclaredVFD = 600m;
			line6.LineDuty = line6.Claims.DutyClaim.DeclaredAmount * 1m;

			Factory.Save();

			var jobDeclarationDrawbackSummary = new JobDeclarationDrawbackSupporter(drawback, UpdateActionCode.Add);
			AssertEquals("No. of doc line", 4, jobDeclarationDrawbackSummary.DocSummaryLineCollection.Count);
			var docLine = jobDeclarationDrawbackSummary.DocSummaryLineCollection.Where(l => (ZString)l["ImportEntryOrCMDNumber"] == "ENRTY1");
			AssertEquals(1, docLine.Count());
			AssertEquals("1011", docLine.ElementAt(0)["Port"]);
			AssertEquals(297m, docLine.ElementAt(0)["_99Duty"]);
			docLine = jobDeclarationDrawbackSummary.DocSummaryLineCollection.Where(l => (ZString)l["ImportEntryOrCMDNumber"] == "CM1234");
			AssertEquals(1, docLine.Count());
			AssertEquals("2022", docLine.ElementAt(0)["Port"]);
			AssertEquals(693m, docLine.ElementAt(0)["_99Duty"]);
			docLine = jobDeclarationDrawbackSummary.DocSummaryLineCollection.Where(l => (ZString)l["Port"] == "3033");
			AssertEquals(1, docLine.Count());
			AssertEquals(ZString.Empty, docLine.ElementAt(0)["ImportEntryOrCMDNumber"]);
			AssertEquals(495m, docLine.ElementAt(0)["_99Duty"]);

			AssertEquals("No. of imp tariff", 2, jobDeclarationDrawbackSummary.DocSummaryImportTariffCollection.Count);
			AssertEquals("No. of exp tariff", 1, jobDeclarationDrawbackSummary.DocSummaryExportTariffCollection.Count);
		}

		public void TestADRW89And90HaveSumOfCalculatedAmountAndAdjustedAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWClaimAmountOverriden_New = true;
			invoiceLine.US_DRWWeightedRatio = 1m;
			invoiceLine.US_DRWMPFWeightedRatio = 1m;
			invoiceLine.DRWImportQuantity = 1m;
			invoiceLine.DRWImportUQ = ABIUnitOfMeasureList.Codes.Barrels;
			invoiceLine.DRWExportQuantity = 1m;
			invoiceLine.DeclaredVFD = 100m;
			invoiceLine.DeclaredTax = 200m;
			invoiceLine.DeclaredHMF = 300m;
			invoiceLine.DeclaredMPF = 400m;
			invoiceLine.DeclaredOtherFees = 500m;
			invoiceLine.AdjClaimDuty = 50m;
			invoiceLine.AdjClaimHMF = 10m;
			invoiceLine.AdjClaimMPF = 2m;
			invoiceLine.AdjClaimTax = 3m;

			invoiceLine.DrawbackOtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.OilSpillTax, 450m).AdjClaimAmount = 0.10m;
			invoiceLine.DrawbackOtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.DomesticTax, 70m).AdjClaimAmount = 0.20m;
			invoiceLine.DrawbackOtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.DrawbackSuperfundTax, 80m).AdjClaimAmount = 0.30m;
			invoiceLine.DrawbackOtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.PRDrawbackDuty, 90m).AdjClaimAmount = 0.40m;
			invoiceLine.LineDuty = invoiceLine.Claims.DutyClaim.DeclaredAmount * 0.5m;

			var jobDeclarationDrawbackSummary = new JobDeclarationDrawbackSupporter(declaration, UpdateActionCode.Add);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			CombineAssertions(() =>
			{
				AssertEquals("TotalDrawbackClaimed", 1623.60m, jobDeclarationDrawbackSummary.TotalDrawbackClaimed);
				AssertEquals("TotalClaimDuty", 49.5m, jobDeclarationDrawbackSummary.TotalClaimDuty);
				AssertEquals("TotalClaimTax", 198m, jobDeclarationDrawbackSummary.TotalClaimTax);
				AssertEquals("TotalHMF", 297m, jobDeclarationDrawbackSummary.TotalHMF);
				AssertEquals("TotalMPF", 396m, jobDeclarationDrawbackSummary.TotalMPF);
				AssertEquals("TotalAdjDutyClaimAmount", 50m, jobDeclarationDrawbackSummary.TotalAdjDutyClaimAmount);
				AssertEquals("TotalAdjHMFClaimAmount", 10m, jobDeclarationDrawbackSummary.TotalAdjHMFClaimAmount);
				AssertEquals("TotalAdjMPFClaimAmount", 2m, jobDeclarationDrawbackSummary.TotalAdjMPFClaimAmount);
				AssertEquals("TotalAdjTaxClaimAmount", 3m, jobDeclarationDrawbackSummary.TotalAdjTaxClaimAmount);
				AssertEquals("TotalPuertoRicoDrawback", 89.1m, jobDeclarationDrawbackSummary.TotalPuertoRicoDrawback);

				var aceDrawbackSummary = jobDeclarationDrawbackSummary as IACEDrawbackSummary;
				AssertEquals("GrandTotalDuty", 189m, aceDrawbackSummary.GrandTotalDuty);
				AssertEquals("GrandTotalUserFee", 705.00m, aceDrawbackSummary.GrandTotalUserFee);
				AssertEquals("GrandTotalIRTax", 795.60m, aceDrawbackSummary.GrandTotalIRTax);
			});
		}

		public void TestTotalDrawbackClaimed()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			invoiceLine1.US_DRWIsForImportSection = true;
			invoiceLine1.US_DRWClaimAmountOverriden_New = true;
			invoiceLine1.US_DRWWeightedRatio = 1m;
			invoiceLine1.US_DRWMPFWeightedRatio = 1m;
			invoiceLine1.DRWImportQuantity = 1m;
			invoiceLine1.DRWImportUQ = ABIUnitOfMeasureList.Codes.Barrels;
			invoiceLine1.DRWExportQuantity = 1m;
			invoiceLine1.DeclaredVFD = 100m;
			invoiceLine1.DeclaredTax = 200m;
			invoiceLine1.DeclaredHMF = 300m;
			invoiceLine1.DeclaredMPF = 400m;
			invoiceLine1.DeclaredOtherFees = 500m;
			invoiceLine1.DrawbackOtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.OilSpillTax, 450m);
			invoiceLine1.LineDuty = invoiceLine1.Claims.DutyClaim.DeclaredAmount * 0.5m;

			invoiceLine2.US_DRWIsForImportSection = true;
			invoiceLine2.US_DRWClaimAmountOverriden_New = true;
			invoiceLine2.US_DRWWeightedRatio = 1m;
			invoiceLine2.US_DRWMPFWeightedRatio = 1m;
			invoiceLine2.DRWImportQuantity = 1m;
			invoiceLine2.DRWImportUQ = ABIUnitOfMeasureList.Codes.Barrels;
			invoiceLine2.DRWExportQuantity = 1m;
			invoiceLine2.DeclaredVFD = 1000m;
			invoiceLine2.DeclaredTax = 2000m;
			invoiceLine2.DeclaredHMF = 3000m;
			invoiceLine2.DeclaredMPF = 4000m;
			invoiceLine2.DrawbackOtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.OilSpillTax, 4500m);
			invoiceLine2.DeclaredOtherFees = 5000m;
			invoiceLine2.LineDuty = invoiceLine2.Claims.DutyClaim.DeclaredAmount * 0.5m;

			JobDeclarationDrawbackSupporter jobDeclarationDrawbackSummary = new JobDeclarationDrawbackSupporter(declaration, UpdateActionCode.Add);
			AssertEquals(15790.5m, jobDeclarationDrawbackSummary.TotalDrawbackClaimed);

			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CD;
			AssertEquals(15950m, jobDeclarationDrawbackSummary.TotalDrawbackClaimed);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			AssertEquals(15246m, jobDeclarationDrawbackSummary.TotalDrawbackClaimed);

			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CD;
			AssertEquals(15400m, jobDeclarationDrawbackSummary.TotalDrawbackClaimed);
		}

		public void TestCusAddInfoFetchHints()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			drawback.US_EntryType = ACEDrawbackProvisionsList.Codes._51;

			var invoice = drawback.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.DrawbackOtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.OilSpillTax, 888);

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.US_DRWIsForImportSection = true;
			invoiceLine2.DrawbackOtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.DomesticTax, 777m);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.EnableTableHitQueryCollection(new string[] { CusAddInfoSchema.Constants.TableName });
			var declarationLoaded = factory2.Load<JobDeclaration>(drawback.PK);
			var accessed = new JobDeclarationDrawbackSupporter(declarationLoaded, UpdateActionCode.Add).TotalsCalculated;
			var cusAddInfoTableHit = factory2.TableSelects.FirstOrDefault(x => x.TableName == CusAddInfoSchema.Constants.TableName);

			var query = factory2.TableSelects.FirstOrDefault(x => x.TableName == CusAddInfoSchema.Constants.TableName && x.Queries.Any(hitQuery => hitQuery.Query.Contains(invoiceLine.PK.ToString()) && hitQuery.Query.Contains(invoiceLine2.PK.ToString())));
			AssertEquals("Access to CusAddInfo containing two invoice line PKs", 1, query.Value);
		}

		public void TestJobDeclarationDrawbackSupporterForSummaryMessage()
		{
			OrgHeader claimant = Factory.New<OrgHeader>();
			claimant.OH_FullName = "MR CLAIMANT";
			claimant.MainAddress.OA_Address1 = "CLAIMANT ADDRESS 1";
			OrgCusCode cusCode = claimant.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			cusCode.OK_CustomsRegNo = "061234";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.US_DRWIsForImportSection = true;
			invoiceLine2.US_DRWIsForImportSection = true;
			invoiceLine1.US_DRWIsForExportSection = true;
			invoiceLine2.US_DRWIsForExportSection = true;
			invoiceLine1.US_DRWClaimAmountOverriden_New = true;
			invoiceLine2.US_DRWClaimAmountOverriden_New = true;
			USCustomsDataRegistry.Instance.ARecordOfficeCode.SetValue(declaration.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, "89");
			declaration.Branch.GB_Code = "XXX";
			var drawbackFiler = Factory.New<OrgHeader>();
			drawbackFiler.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "364-331 434");
			declaration.JE_OH_NotifyParty = drawbackFiler.PK;
			declaration.JE_OH_Importer = claimant.PK;
			declaration.JE_MessageStatus = "AAA";
			declaration.JE_EntryStatus = "BBB";
			declaration.US_EntryType = "41";
			declaration.US_ClaimPort = "8888";
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 1);
			declaration.US_TeamNo = "CCC";
			declaration.US_BondType = "8";
			declaration.US_SuretyCode = "12";
			declaration.JE_DeclarationReference = "B12345678";
			declaration.US_PreparerDistrictPort = "3901";
			declaration.JE_GoodsDescription = "XXXXXXXXXX";
			declaration.US_EarliestExportDate = new ZDateTime(2007, 1, 1);
			declaration.US_NAFTADrawbackCountry = "CA";
			declaration.US_EntryFilerCode = "SV9";
			declaration.SetDefaultValuesForDrawback();

			JobDeclarationDrawbackSupporter jobDeclarationDrawbackSummary = new JobDeclarationDrawbackSupporter(declaration, UpdateActionCode.Delete);
			AssertEquals("DeleteCode", "D", jobDeclarationDrawbackSummary.DeleteCode);
			jobDeclarationDrawbackSummary = new JobDeclarationDrawbackSupporter(declaration, UpdateActionCode.Add);
			AssertEquals("DeleteCode", "", jobDeclarationDrawbackSummary.DeleteCode);
			AssertEquals("EntryFilerCode", "SV9", jobDeclarationDrawbackSummary.EntryFilerCode);
			AssertEquals("ProcessingOfficeCode", "89", jobDeclarationDrawbackSummary.ProcessingOfficeCode);
			AssertEquals("MessageStatus", "AAA", jobDeclarationDrawbackSummary.MessageStatus);
			AssertEquals("EntryStatus", "BBB", jobDeclarationDrawbackSummary.EntryStatus);
			AssertEquals("No messages", 0, declaration.Messages.Count);
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			jobDeclarationDrawbackSummary.AddMessage(message);
			AssertEquals("AddMessage", 1, declaration.Messages.Count);
			AssertEquals("ClaimNumber", MQEDIMessage.USEntryFilerEntryNumberPlaceHolder, jobDeclarationDrawbackSummary.ClaimNumber);
			AssertEquals("ClaimantName", claimant.OH_FullName, jobDeclarationDrawbackSummary.ClaimantName);
			AssertEquals("ClaimantAddress", claimant.MainAddress.AddressAsASingleLine, jobDeclarationDrawbackSummary.ClaimantAddress);
			AssertNotContains("ContactDetails", GlbBranch.CurrentBranch.OrgProxy.OH_FullName + ", " + GlbBranch.CurrentBranch.OrgProxy.OH_FullName, jobDeclarationDrawbackSummary.ContactDetails);
			AssertEquals("ClaimType", 41, jobDeclarationDrawbackSummary.ClaimType);
			AssertEquals("ClaimPort", "8888", jobDeclarationDrawbackSummary.ClaimPort);
			AssertEquals("CodeOfBranchOnDeclaration", "XXX", jobDeclarationDrawbackSummary.CodeOfBranchOnDeclaration);
			AssertEquals("EstimatedClaimDate", new ZDate(2008, 1, 1), jobDeclarationDrawbackSummary.EstimatedClaimDate);
			AssertEquals("ClaimantIdentification", "061234", jobDeclarationDrawbackSummary.ClaimantIdentification);
			AssertEquals("DrawbackTeam", "CCC", jobDeclarationDrawbackSummary.DrawbackTeam);
			AssertEquals("BondType", "8", jobDeclarationDrawbackSummary.BondType);
			AssertEquals("SuretyCode", "12", jobDeclarationDrawbackSummary.SuretyCode);
			AssertEquals("NAFTAClaimIndicator", "", jobDeclarationDrawbackSummary.NAFTAClaimIndicator);
			declaration.US_NAFTAClaimInd = true;
			AssertEquals("NAFTAClaimIndicator", "1", jobDeclarationDrawbackSummary.NAFTAClaimIndicator);
			AssertEquals("GovernmentClaimIndicator", "", jobDeclarationDrawbackSummary.GovernmentClaimIndicator);
			AssertEquals("AcceleratedClaimIndicator", "N", jobDeclarationDrawbackSummary.AcceleratedClaimIndicator);
			declaration.US_AcceleratedClaimInd = true;
			AssertEquals("AcceleratedClaimIndicator", "Y", jobDeclarationDrawbackSummary.AcceleratedClaimIndicator);
			AssertEquals("ExporterSummaryProcedureIndicator", "", jobDeclarationDrawbackSummary.ExporterSummaryProcedureIndicator);
			declaration.US_ExporterSummaryInd = true;
			AssertEquals("ExporterSummaryProcedureIndicator", "1", jobDeclarationDrawbackSummary.ExporterSummaryProcedureIndicator);
			AssertEquals("WaiverOfPriorNoticeIndicator", "", jobDeclarationDrawbackSummary.WaiverOfPriorNoticeIndicator);
			declaration.US_WaiverNoticeInd = true;
			AssertEquals("ExporterSummaryProcedureIndicator", "1", jobDeclarationDrawbackSummary.ExporterSummaryProcedureIndicator);
			AssertEquals("PreInspectionIndicator", "", jobDeclarationDrawbackSummary.PreInspectionIndicator);
			declaration.US_PreInspectionInd = true;
			AssertEquals("PreInspectionIndicator", "1", jobDeclarationDrawbackSummary.PreInspectionIndicator);
			AssertEquals("PetroleumClaimIndicator", "", jobDeclarationDrawbackSummary.PetroleumClaimIndicator);
			declaration.US_PetroleumClaimInd = true;
			AssertEquals("PetroleumClaimIndicator", "1", jobDeclarationDrawbackSummary.PetroleumClaimIndicator);
			AssertEquals("AgentBroker", "364-331 434", jobDeclarationDrawbackSummary.AgentBroker);
			AssertEquals("BrokerReferenceNumber", "B12345678", jobDeclarationDrawbackSummary.BrokerReferenceNumber);
			AssertEquals("LicensePort", "3901", jobDeclarationDrawbackSummary.LicensePort);
			AssertEquals("Description", "XXXXXXXXXX", jobDeclarationDrawbackSummary.Description);
			AssertEquals("EarliestExportDate", new ZDate(2007, 1, 1), jobDeclarationDrawbackSummary.EarliestExportDate);
			AssertEquals("PetroleumClaimIndicator", "1", jobDeclarationDrawbackSummary.PetroleumClaimIndicator);
			AssertEquals("NAFTADrawbackCountryCode", "CA", jobDeclarationDrawbackSummary.NAFTADrawbackCountryCode);
			AssertEquals("FirstContractNumber", "", jobDeclarationDrawbackSummary.FirstContractNumber);
			int j = 0;
			foreach (IDrawbackContractNumber contractNumber in jobDeclarationDrawbackSummary.ExtraContractNumbers)
			{
				j++;
			}
			AssertEquals("ExtraContractNumbers", 0, j);
			ContractNumber contactNumber1 = declaration.ContractNumbers.AddNew();
			contactNumber1.CY_Data = "C1";
			AssertEquals("FirstContractNumber", "C1", jobDeclarationDrawbackSummary.FirstContractNumber);
			j = 0;
			foreach (IDrawbackContractNumber contractNumber in jobDeclarationDrawbackSummary.ExtraContractNumbers)
			{
				j++;
			}
			AssertEquals("ContractNumbers", 0, j);
			ContractNumber contactNumber2 = declaration.ContractNumbers.AddNew();
			contactNumber2.CY_Data = "C2";
			AssertEquals("FirstContractNumber", "C1", jobDeclarationDrawbackSummary.FirstContractNumber);
			j = 0;
			ZString c1 = "";
			foreach (IDrawbackContractNumber contractNumber in jobDeclarationDrawbackSummary.ExtraContractNumbers)
			{
				j++;
				c1 = contractNumber.ContractNumberCode;
			}
			AssertEquals("ExtraContractNumbers", 1, j);
			AssertEquals("ExtraContractNumber", "C2", c1);
			ContractNumber contactNumber3 = declaration.ContractNumbers.AddNew();
			contactNumber3.CY_Data = "C3";
			AssertEquals("FirstContractNumber", "C1", jobDeclarationDrawbackSummary.FirstContractNumber);
			j = 0;
			foreach (IDrawbackContractNumber contractNumber in jobDeclarationDrawbackSummary.ExtraContractNumbers)
			{
				j++;
				if (j == 1)
				{
					AssertEquals("ExtraContractNumber", "C2", contractNumber.ContractNumberCode);
				}
				else if (j == 2)
				{
					AssertEquals("ExtraContractNumber", "C3", contractNumber.ContractNumberCode);
				}
			}
			AssertEquals("ExtraContractNumbers", 2, j);

			j = 0;
			foreach (IDrawbackTrailerTariff tariff in jobDeclarationDrawbackSummary.TrailerTariffs)
			{
				j++;
			}
			AssertEquals("TrailerTariffs", 0, j);
			invoiceLine1.JI_Tariff = "1";
			j = 0;
			foreach (IDrawbackTrailerTariff tariff in jobDeclarationDrawbackSummary.TrailerTariffs)
			{
				j++;
				AssertEquals("FirstTariffNumber", "1", tariff.FirstTariffNumber);
				AssertEquals("AdditionalTariffNumber", "", tariff.AdditionalTariffNumber);
				AssertEquals("AdditionalTariffNumber1", "", tariff.AdditionalTariffNumber1);
				AssertEquals("AdditionalTariffNumber2", "", tariff.AdditionalTariffNumber2);
				AssertEquals("AdditionalTariffNumber3", "", tariff.AdditionalTariffNumber3);
			}
			AssertEquals("TrailerTariffs", 1, j);
			invoiceLine2.JI_Tariff = "1";
			j = 0;
			foreach (IDrawbackTrailerTariff tariff in jobDeclarationDrawbackSummary.TrailerTariffs)
			{
				j++;
				AssertEquals("FirstTariffNumber", "1", tariff.FirstTariffNumber);
				AssertEquals("AdditionalTariffNumber", "", tariff.AdditionalTariffNumber);
				AssertEquals("AdditionalTariffNumber1", "", tariff.AdditionalTariffNumber1);
				AssertEquals("AdditionalTariffNumber2", "", tariff.AdditionalTariffNumber2);
				AssertEquals("AdditionalTariffNumber3", "", tariff.AdditionalTariffNumber3);
			}
			AssertEquals("TrailerTariffs", 1, j);
			invoiceLine2.JI_Tariff = "2";
			j = 0;
			foreach (IDrawbackTrailerTariff tariff in jobDeclarationDrawbackSummary.TrailerTariffs)
			{
				j++;
				AssertEquals("FirstTariffNumber", "1", tariff.FirstTariffNumber);
				AssertEquals("AdditionalTariffNumber", "2", tariff.AdditionalTariffNumber);
				AssertEquals("AdditionalTariffNumber1", "", tariff.AdditionalTariffNumber1);
				AssertEquals("AdditionalTariffNumber2", "", tariff.AdditionalTariffNumber2);
				AssertEquals("AdditionalTariffNumber3", "", tariff.AdditionalTariffNumber3);
			}
			AssertEquals("TrailerTariffs", 1, j);
			DrawbackAdditionalImportTariffNumber at1 = invoiceLine1.DrawbackAdditionalImportTariffNumbers.AddNew();
			at1.US_Tariff = "3";
			DrawbackAdditionalImportTariffNumber at2 = invoiceLine1.DrawbackAdditionalImportTariffNumbers.AddNew();
			at2.US_Tariff = "4";
			DrawbackAdditionalImportTariffNumber at3 = invoiceLine1.DrawbackAdditionalImportTariffNumbers.AddNew();
			at3.US_Tariff = "5";
			j = 0;
			foreach (IDrawbackTrailerTariff tariff in jobDeclarationDrawbackSummary.TrailerTariffs)
			{
				j++;
				AssertEquals("FirstTariffNumber", "1", tariff.FirstTariffNumber);
				AssertEquals("AdditionalTariffNumber", "3", tariff.AdditionalTariffNumber);
				AssertEquals("AdditionalTariffNumber1", "4", tariff.AdditionalTariffNumber1);
				AssertEquals("AdditionalTariffNumber2", "5", tariff.AdditionalTariffNumber2);
				AssertEquals("AdditionalTariffNumber3", "2", tariff.AdditionalTariffNumber3);
			}
			AssertEquals("TrailerTariffs", 1, j);
			DrawbackAdditionalImportTariffNumber at4 = invoiceLine2.DrawbackAdditionalImportTariffNumbers.AddNew();
			at4.US_Tariff = "6";
			j = 0;
			foreach (IDrawbackTrailerTariff tariff in jobDeclarationDrawbackSummary.TrailerTariffs)
			{
				j++;
				if (j == 1)
				{
					AssertEquals("FirstTariffNumber", "1", tariff.FirstTariffNumber);
					AssertEquals("AdditionalTariffNumber", "3", tariff.AdditionalTariffNumber);
					AssertEquals("AdditionalTariffNumber1", "4", tariff.AdditionalTariffNumber1);
					AssertEquals("AdditionalTariffNumber2", "5", tariff.AdditionalTariffNumber2);
					AssertEquals("AdditionalTariffNumber3", "2", tariff.AdditionalTariffNumber3);
				}
				else if (j == 2)
				{
					AssertEquals("FirstTariffNumber", "6", tariff.FirstTariffNumber);
					AssertEquals("AdditionalTariffNumber", "", tariff.AdditionalTariffNumber);
					AssertEquals("AdditionalTariffNumber1", "", tariff.AdditionalTariffNumber1);
					AssertEquals("AdditionalTariffNumber2", "", tariff.AdditionalTariffNumber2);
					AssertEquals("AdditionalTariffNumber3", "", tariff.AdditionalTariffNumber3);
				}
			}
			AssertEquals("TrailerTariffs", 2, j);

			j = 0;
			foreach (IDrawbackTrailerScheduleBNumber tariff in jobDeclarationDrawbackSummary.TrailerScheduleBNumbers)
			{
				j++;
			}
			AssertEquals("TrailerScheduleBNumbers", 0, j);
			invoiceLine1.US_ExportTariff = "11";
			j = 0;
			foreach (IDrawbackTrailerScheduleBNumber tariff in jobDeclarationDrawbackSummary.TrailerScheduleBNumbers)
			{
				j++;
				AssertEquals("FirstScheduleBNumber", "11", tariff.FirstScheduleBNumber);
				AssertEquals("AdditionalScheduleBNumber", "", tariff.AdditionalScheduleBNumber);
				AssertEquals("AdditionalScheduleBNumber1", "", tariff.AdditionalScheduleBNumber1);
				AssertEquals("AdditionalScheduleBNumber2", "", tariff.AdditionalScheduleBNumber2);
				AssertEquals("AdditionalScheduleBNumber3", "", tariff.AdditionalScheduleBNumber3);
			}
			AssertEquals("TrailerScheduleBNumbers", 1, j);
			invoiceLine2.US_ExportTariff = "11";
			j = 0;
			foreach (IDrawbackTrailerScheduleBNumber tariff in jobDeclarationDrawbackSummary.TrailerScheduleBNumbers)
			{
				j++;
				AssertEquals("FirstScheduleBNumber", "11", tariff.FirstScheduleBNumber);
				AssertEquals("AdditionalScheduleBNumber", "", tariff.AdditionalScheduleBNumber);
				AssertEquals("AdditionalScheduleBNumber1", "", tariff.AdditionalScheduleBNumber1);
				AssertEquals("AdditionalScheduleBNumber2", "", tariff.AdditionalScheduleBNumber2);
				AssertEquals("AdditionalScheduleBNumber3", "", tariff.AdditionalScheduleBNumber3);
			}
			AssertEquals("TrailerScheduleBNumbers", 1, j);
			invoiceLine2.US_ExportTariff = "22";
			j = 0;
			foreach (IDrawbackTrailerScheduleBNumber tariff in jobDeclarationDrawbackSummary.TrailerScheduleBNumbers)
			{
				j++;
				AssertEquals("FirstScheduleBNumber", "11", tariff.FirstScheduleBNumber);
				AssertEquals("AdditionalScheduleBNumber", "22", tariff.AdditionalScheduleBNumber);
				AssertEquals("AdditionalScheduleBNumber1", "", tariff.AdditionalScheduleBNumber1);
				AssertEquals("AdditionalScheduleBNumber2", "", tariff.AdditionalScheduleBNumber2);
				AssertEquals("AdditionalScheduleBNumber3", "", tariff.AdditionalScheduleBNumber3);
			}
			AssertEquals("TrailerScheduleBNumbers", 1, j);
			DrawbackAdditionalExportTariffNumber axt1 = invoiceLine1.DrawbackAdditionalExportTariffNumbers.AddNew();
			axt1.CY_Data = "33";
			DrawbackAdditionalExportTariffNumber axt2 = invoiceLine1.DrawbackAdditionalExportTariffNumbers.AddNew();
			axt2.CY_Data = "44";
			DrawbackAdditionalExportTariffNumber axt3 = invoiceLine1.DrawbackAdditionalExportTariffNumbers.AddNew();
			axt3.CY_Data = "55";
			j = 0;
			foreach (IDrawbackTrailerScheduleBNumber tariff in jobDeclarationDrawbackSummary.TrailerScheduleBNumbers)
			{
				j++;
				AssertEquals("FirstScheduleBNumber", "11", tariff.FirstScheduleBNumber);
				AssertEquals("AdditionalScheduleBNumber", "33", tariff.AdditionalScheduleBNumber);
				AssertEquals("AdditionalScheduleBNumber1", "44", tariff.AdditionalScheduleBNumber1);
				AssertEquals("AdditionalScheduleBNumber2", "55", tariff.AdditionalScheduleBNumber2);
				AssertEquals("AdditionalScheduleBNumber3", "22", tariff.AdditionalScheduleBNumber3);
			}
			AssertEquals("TrailerScheduleBNumbers", 1, j);
			DrawbackAdditionalExportTariffNumber axt4 = invoiceLine2.DrawbackAdditionalExportTariffNumbers.AddNew();
			axt4.CY_Data = "66";
			j = 0;
			foreach (IDrawbackTrailerScheduleBNumber tariff in jobDeclarationDrawbackSummary.TrailerScheduleBNumbers)
			{
				j++;
				if (j == 1)
				{
					AssertEquals("FirstScheduleBNumber", "11", tariff.FirstScheduleBNumber);
					AssertEquals("AdditionalScheduleBNumber", "33", tariff.AdditionalScheduleBNumber);
					AssertEquals("AdditionalScheduleBNumber1", "44", tariff.AdditionalScheduleBNumber1);
					AssertEquals("AdditionalScheduleBNumber2", "55", tariff.AdditionalScheduleBNumber2);
					AssertEquals("AdditionalScheduleBNumber3", "22", tariff.AdditionalScheduleBNumber3);
				}
				else if (j == 2)
				{
					AssertEquals("FirstScheduleBNumber", "66", tariff.FirstScheduleBNumber);
					AssertEquals("AdditionalScheduleBNumber", "", tariff.AdditionalScheduleBNumber);
					AssertEquals("AdditionalScheduleBNumber1", "", tariff.AdditionalScheduleBNumber1);
					AssertEquals("AdditionalScheduleBNumber2", "", tariff.AdditionalScheduleBNumber2);
					AssertEquals("AdditionalScheduleBNumber3", "", tariff.AdditionalScheduleBNumber3);
				}
			}
			AssertEquals("TrailerScheduleBNumbers", 2, j);

			invoiceLine1.US_ImportEntryNo = "123456";
			invoiceLine2.US_ImportEntryNo = "123456";

			invoiceLine1.DRWImportQuantity = 1m;
			invoiceLine1.DRWExportQuantity = 1m;
			invoiceLine1.DRWImportUQ = ABIUnitOfMeasureList.Codes.Barrels;
			invoiceLine1.DeclaredVFD = 12.34m;
			invoiceLine1.DeclaredTax = 1.23m;
			invoiceLine1.LineDuty = invoiceLine1.Claims.DutyClaim.DeclaredAmount * 1m;

			invoiceLine2.DRWImportQuantity = 1m;
			invoiceLine2.DRWExportQuantity = 1m;
			invoiceLine2.DRWImportUQ = ABIUnitOfMeasureList.Codes.Barrels;
			invoiceLine2.DeclaredVFD = 2m;
			invoiceLine2.DeclaredTax = 1m;
			invoiceLine2.LineDuty = invoiceLine2.Claims.DutyClaim.DeclaredAmount * 1m;
			j = 0;
			foreach (IDrawbackImportClaim importClaim in jobDeclarationDrawbackSummary.ImportClaims)
			{
				j++;
				AssertEquals("Total line claim duty", 14.19m, importClaim.DrawbackClaimDuty);
				AssertEquals("Total line claim tax", 2.20m, importClaim.DrawbackClaimTax);
			}
			AssertEquals("ImportClaims", 1, j);
			j = 0;
			foreach (IDrawbackManufactureClaim manufactureClaim in jobDeclarationDrawbackSummary.ManufactureClaims)
			{
				j++;
			}
			AssertEquals("ManufactureClaims", 0, j);

			invoiceLine2.US_ImportEntryNo = "123457";
			j = 0;
			foreach (IDrawbackImportClaim importClaim in jobDeclarationDrawbackSummary.ImportClaims)
			{
				j++;
			}
			AssertEquals("ImportClaims", 2, j);

			invoiceLine2.US_ImportEntryNo = "";
			invoiceLine2.US_DRWCertOfManufacture = "CM1";

			j = 0;
			foreach (IDrawbackImportClaim importClaim in jobDeclarationDrawbackSummary.ImportClaims)
			{
				j++;
				AssertEquals("DrawbackImportEntry", "123456", importClaim.DrawbackImportEntry);
			}
			AssertEquals("ImportClaims", 1, j);
			j = 0;
			foreach (IDrawbackManufactureClaim manufactureClaim in jobDeclarationDrawbackSummary.ManufactureClaims)
			{
				j++;
				AssertEquals("", "CM1", manufactureClaim.CertificateOfManufactureNumber);
			}
			AssertEquals("ManufactureClaims", 1, j);

			invoiceLine1.US_ImportEntryNo = "";
			invoiceLine1.US_DRWCertOfManufacture = "CM2";
			j = 0;
			foreach (IDrawbackImportClaim importClaim in jobDeclarationDrawbackSummary.ImportClaims)
			{
				j++;
			}
			AssertEquals("ImportClaims", 0, j);
			j = 0;
			foreach (IDrawbackManufactureClaim manufactureClaim in jobDeclarationDrawbackSummary.ManufactureClaims)
			{
				j++;
			}
			AssertEquals("ManufactureClaims", 2, j);

			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CD;

			invoiceLine1.US_DRWCertOfManufacture = "CM1";
			invoiceLine1.JI_CustomsQuantity = 1M;
			invoiceLine2.JI_CustomsQuantity = 2m;

			invoiceLine1.DRWImportQuantity = 1m;
			invoiceLine1.DRWExportQuantity = 1m;
			invoiceLine1.DRWImportUQ = ABIUnitOfMeasureList.Codes.Barrels;
			invoiceLine1.DeclaredVFD = 12.34m;
			invoiceLine1.DeclaredTax = 1.23m;
			invoiceLine1.LineDuty = invoiceLine1.Claims.DutyClaim.DeclaredAmount * 1m;

			invoiceLine2.DRWImportQuantity = 1m;
			invoiceLine2.DRWExportQuantity = 1m;
			invoiceLine2.DRWImportUQ = ABIUnitOfMeasureList.Codes.Barrels;
			invoiceLine2.DeclaredVFD = 2m;
			invoiceLine2.DeclaredTax = 1m;
			invoiceLine2.LineDuty = invoiceLine2.Claims.DutyClaim.DeclaredAmount * 1m;

			j = 0;
			foreach (IDrawbackImportClaim importClaim in jobDeclarationDrawbackSummary.ImportClaims)
			{
				j++;
			}
			AssertEquals("ImportClaims", 0, j);
			j = 0;
			foreach (IDrawbackManufactureClaim manufactureClaim in jobDeclarationDrawbackSummary.ManufactureClaims)
			{
				j++;
				AssertEquals("", "CM1", manufactureClaim.CertificateOfManufactureNumber);
				AssertEquals("Total line claim duty", 14.34m, manufactureClaim.DrawbackClaimDuty);
				AssertEquals("Total line claim tax", 2.23m, manufactureClaim.DrawbackClaimTax);
				AssertEquals("Total line mfg qty, DrawbackManufactureQuantity comes from DRWExportQuantity now", 2m, manufactureClaim.DrawbackManufactureQuantity);
			}
			AssertEquals("ManufactureClaims", 1, j);

			AssertEquals("TotalClaimDuty", 14.34m, jobDeclarationDrawbackSummary.TotalClaimDuty);
			AssertEquals("TotalClaimTax", 2.23m, jobDeclarationDrawbackSummary.TotalClaimTax);
			AssertEquals("TotalNAFTACountryImportDuty", 0m, jobDeclarationDrawbackSummary.TotalNAFTACountryImportDuty);
			AssertEquals("TotalUSDollarEquivalentOfNAFTACountryDuty", 0m, jobDeclarationDrawbackSummary.TotalUSDollarEquivalentOfNAFTACountryDuty);
			invoiceLine1.US_ImportEntryNo = "123456";
			invoiceLine1.US_DRWCertOfManufacture = "";

			invoiceLine1.DRWImportQuantity = 1m;
			invoiceLine1.DRWExportQuantity = 1m;
			invoiceLine1.DRWImportUQ = ABIUnitOfMeasureList.Codes.Barrels;
			invoiceLine1.DeclaredVFD = 100m;
			invoiceLine1.DeclaredTax = 25m;
			invoiceLine1.LineDuty = invoiceLine1.Claims.DutyClaim.DeclaredAmount * 1m;

			invoiceLine2.DRWImportQuantity = 1m;
			invoiceLine2.DRWExportQuantity = 1m;
			invoiceLine2.DRWImportUQ = ABIUnitOfMeasureList.Codes.Barrels;
			invoiceLine2.DeclaredVFD = 200m;
			invoiceLine2.DeclaredTax = 50m;
			invoiceLine2.LineDuty = invoiceLine2.Claims.DutyClaim.DeclaredAmount * 1m;

			j = 0;
			foreach (IDrawbackNAFTATariff nAFTAtariff in jobDeclarationDrawbackSummary.NAFTATariffs)
			{
				j++;
			}
			AssertEquals("DrawbackNAFTATariffs", 0, j);
			DrawbackNAFTA n1 = invoiceLine1.DrawbackNAFTAs.AddNew();
			DrawbackNAFTA n2 = invoiceLine1.DrawbackNAFTAs.AddNew();
			DrawbackNAFTA n3 = invoiceLine2.DrawbackNAFTAs.AddNew();
			n1.US_DRWNAFTACountryImportEntry = "1";
			n1.US_DRWNAFTACountryImportDuty = 100m;
			n1.US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty = 110m;
			n2.US_DRWNAFTACountryImportEntry = "2";
			n2.US_DRWNAFTACountryImportDuty = 200m;
			n2.US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty = 220m;
			n3.US_DRWNAFTACountryImportDuty = 300m;
			n3.US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty = 330m;
			n3.US_DRWNAFTACountryImportEntry = "3";
			j = 0;
			foreach (IDrawbackNAFTATariff nAFTAtariff in jobDeclarationDrawbackSummary.NAFTATariffs)
			{
				j++;
				if (j == 1)
				{
					AssertEquals("DrawbackNAFTATariff", "1", nAFTAtariff.NAFTACountryImportEntry);
				}
				else if (j == 2)
				{
					AssertEquals("DrawbackNAFTATariff", "2", nAFTAtariff.NAFTACountryImportEntry);
				}
				else if (j == 3)
				{
					AssertEquals("DrawbackNAFTATariff", "3", nAFTAtariff.NAFTACountryImportEntry);
				}
			}
			AssertEquals("TrailerTariffs", 3, j);
			AssertEquals("TotalClaimDuty", 300m, jobDeclarationDrawbackSummary.TotalClaimDuty);
			AssertEquals("TotalClaimTax", 75m, jobDeclarationDrawbackSummary.TotalClaimTax);
			AssertEquals("TotalNAFTACountryImportDuty", 600m, jobDeclarationDrawbackSummary.TotalNAFTACountryImportDuty);
			AssertEquals("TotalUSDollarEquivalentOfNAFTACountryDuty", 660m, jobDeclarationDrawbackSummary.TotalUSDollarEquivalentOfNAFTACountryDuty);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("ClaimantIdentification", "061234", jobDeclarationDrawbackSummary.ClaimantIdentification);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.OtherDrawback;
			declaration.US_NAFTADrawbackCountry = "CA";
			AssertEquals("NFTA", "", jobDeclarationDrawbackSummary.SectionVNAFTASameCondition);
			AssertEquals("Unused", "", jobDeclarationDrawbackSummary.SectionVUnused);
			AssertEquals("Interchangeable", "", jobDeclarationDrawbackSummary.SectionVInterchangeable);
			AssertEquals("SameKind", "", jobDeclarationDrawbackSummary.SectionVSameKind);
			AssertEquals("Manufacture", "", jobDeclarationDrawbackSummary.SectionVManufacture);
			AssertEquals("SectionVPOA", "", jobDeclarationDrawbackSummary.SectionVPOA);
			AssertEquals("SectionVOfficer", "", jobDeclarationDrawbackSummary.SectionVOfficer);
			AssertEquals("SectionVBroker", "", jobDeclarationDrawbackSummary.SectionVBroker);
			declaration.US_EntryType = EntryTypeList.Codes.DirectIdentificationUnusedMerchandiseDrawback;
			AssertEquals("NFTA", "X", jobDeclarationDrawbackSummary.SectionVNAFTASameCondition);
			AssertEquals("Unused", "", jobDeclarationDrawbackSummary.SectionVUnused);
			declaration.US_NAFTADrawbackCountry = "";
			AssertEquals("NFTA", "", jobDeclarationDrawbackSummary.SectionVNAFTASameCondition);
			AssertEquals("Unused", "X", jobDeclarationDrawbackSummary.SectionVUnused);
			declaration.US_EntryType = EntryTypeList.Codes.RejectedMerchandiseDrawback;
			AssertEquals("Interchangeable", "X", jobDeclarationDrawbackSummary.SectionVInterchangeable);
			AssertEquals("NonConform", "", jobDeclarationDrawbackSummary.SectionVNonConform);
			AssertEquals("Defective", "", jobDeclarationDrawbackSummary.SectionVDefective);
			AssertEquals("NoConcent", "", jobDeclarationDrawbackSummary.SectionVNoConcent);
			declaration.US_DRWRejectedMerchandiseReason = DrawbackRejectedMerchandiseReasonList.Codes.NCS;
			AssertEquals("NonConform", "X", jobDeclarationDrawbackSummary.SectionVNonConform);
			declaration.US_DRWRejectedMerchandiseReason = DrawbackRejectedMerchandiseReasonList.Codes.DTI;
			AssertEquals("Defective", "X", jobDeclarationDrawbackSummary.SectionVDefective);
			declaration.US_DRWRejectedMerchandiseReason = DrawbackRejectedMerchandiseReasonList.Codes.SWC;
			AssertEquals("NoConcent", "X", jobDeclarationDrawbackSummary.SectionVNoConcent);
			declaration.US_EntryType = EntryTypeList.Codes.SubstitutionManufacturerDrawback;
			AssertEquals("SameKind", "X", jobDeclarationDrawbackSummary.SectionVSameKind);
			declaration.US_EntryType = EntryTypeList.Codes.DirectIdentificationManufacturingDrawback;
			AssertEquals("SameKind", "X", jobDeclarationDrawbackSummary.SectionVSameKind);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._01;
			declaration.US_NAFTADrawbackCountry = "CA";
			AssertEquals("NFTA", "", jobDeclarationDrawbackSummary.SectionVNAFTASameCondition);
			AssertEquals("Unused", "", jobDeclarationDrawbackSummary.SectionVUnused);
			AssertEquals("Interchangeable", "", jobDeclarationDrawbackSummary.SectionVInterchangeable);
			AssertEquals("NonConform", "", jobDeclarationDrawbackSummary.SectionVNonConform);
			AssertEquals("Defective", "", jobDeclarationDrawbackSummary.SectionVDefective);
			AssertEquals("NoConcent", "", jobDeclarationDrawbackSummary.SectionVNoConcent);
			AssertEquals("SameKind", "X", jobDeclarationDrawbackSummary.SectionVSameKind);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._08;
			AssertEquals("NFTA", "X", jobDeclarationDrawbackSummary.SectionVNAFTASameCondition);
			AssertEquals("Unused", "", jobDeclarationDrawbackSummary.SectionVUnused);
			AssertEquals("Interchangeable", "", jobDeclarationDrawbackSummary.SectionVInterchangeable);
			AssertEquals("SameKind", "", jobDeclarationDrawbackSummary.SectionVSameKind);
			declaration.US_NAFTADrawbackCountry = "";
			AssertEquals("NFTA", "", jobDeclarationDrawbackSummary.SectionVNAFTASameCondition);
			AssertEquals("Unused", "X", jobDeclarationDrawbackSummary.SectionVUnused);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._03;
			declaration.US_DRWRejectedMerchandiseReason = ZString.Empty;
			AssertEquals("Interchangeable", "X", jobDeclarationDrawbackSummary.SectionVInterchangeable);
			AssertEquals("NonConform", "", jobDeclarationDrawbackSummary.SectionVNonConform);
			AssertEquals("Defective", "", jobDeclarationDrawbackSummary.SectionVDefective);
			AssertEquals("NoConcent", "", jobDeclarationDrawbackSummary.SectionVNoConcent);
			declaration.US_DRWRejectedMerchandiseReason = DrawbackRejectedMerchandiseReasonList.Codes.NCS;
			AssertEquals("NonConform", "X", jobDeclarationDrawbackSummary.SectionVNonConform);
			declaration.US_DRWRejectedMerchandiseReason = DrawbackRejectedMerchandiseReasonList.Codes.DTI;
			AssertEquals("Defective", "X", jobDeclarationDrawbackSummary.SectionVDefective);
			declaration.US_DRWRejectedMerchandiseReason = DrawbackRejectedMerchandiseReasonList.Codes.SWC;
			AssertEquals("NoConcent", "X", jobDeclarationDrawbackSummary.SectionVNoConcent);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._02;
			AssertEquals("SameKind", "X", jobDeclarationDrawbackSummary.SectionVSameKind);
		}

		public void TestDrawbackUsesCustomsAddressOfRecord()
		{
			var claimant = Factory.New<OrgHeader>();
			claimant.OH_Code = "CLA001";
			claimant.OH_FullName = "MR CLAIMANT";
			claimant.MainAddress.OA_Address1 = "CLAIMANT ADDRESS 1";
			var cusCode = claimant.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			cusCode.OK_CustomsRegNo = "061234";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.US_DRWIsForImportSection = true;
			invoiceLine2.US_DRWIsForImportSection = true;
			invoiceLine1.US_DRWIsForExportSection = true;
			invoiceLine2.US_DRWIsForExportSection = true;
			USCustomsDataRegistry.Instance.ARecordOfficeCode.SetValue(declaration.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, "89");
			var filer = Factory.NewWithValidTestData<OrgHeader>();
			filer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "364-331 434");
			declaration.JE_OH_NotifyParty = filer.PK;
			declaration.JE_OH_Importer = claimant.PK;
			declaration.JE_MessageStatus = "AAA";
			declaration.JE_EntryStatus = "BBB";
			declaration.US_EntryType = "41";
			declaration.US_ClaimPort = "8888";
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 1);
			declaration.US_TeamNo = "CCC";
			declaration.US_BondType = "8";
			declaration.US_SuretyCode = "12";
			declaration.JE_DeclarationReference = "B12345678";
			declaration.US_PreparerDistrictPort = "3901";
			declaration.JE_GoodsDescription = "XXXXXXXXXX";
			declaration.US_EarliestExportDate = new ZDateTime(2007, 1, 1);
			declaration.US_NAFTADrawbackCountry = "CA";
			declaration.US_EntryFilerCode = "SV9";
			declaration.SetDefaultValuesForDrawback();

			var jobDeclarationDrawbackSummary = new JobDeclarationDrawbackSupporter(declaration, UpdateActionCode.Add);
			var expectedResult = "CargoWise Support, EDI CUSTOMS BROKERS, 10 HUTCHESON STREET ALBION QLD 4010 AUSTRALIA\r\n PH PH 07 3268 2903  FAX FX 07 3868 1274 EML ";
			AssertEquals("ContactDetails", expectedResult, jobDeclarationDrawbackSummary.ContactDetails);

			DbConnection connection = ((IDbConnected)Factory).Connection;
			try
			{
				connection.BeginTransaction();
				var customsAddrOfRecord = Factory.Load<OrgHeader>(declaration.Branch.GB_OH_OrgProxy);
				customsAddrOfRecord.OH_RL_NKClosestPort = "USNAS";
				customsAddrOfRecord.Addresses[0].OA_CompanyNameOverride = "Customs Brokers and Logistics Worldwide";
				customsAddrOfRecord.Addresses[0].OA_Address1 = "Nashville Branch Office";
				customsAddrOfRecord.Addresses[0].OA_Address2 = "115 JonesTown Street";
				customsAddrOfRecord.Addresses[0].OA_City = "Nashville";
				customsAddrOfRecord.Addresses[0].OA_PostCode = "55555";
				customsAddrOfRecord.Addresses[0].OA_State = "TN";
				customsAddrOfRecord.Addresses[0].OA_Phone = "+1 22 456 789";
				customsAddrOfRecord.Addresses[0].OA_Fax = "+1 22 456 799";
				customsAddrOfRecord.Addresses[0].OA_Email = "support@support.com";
				customsAddrOfRecord.Addresses[0].AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.CustomsAddressOfRecord);
				Factory.Save();

				jobDeclarationDrawbackSummary = new JobDeclarationDrawbackSupporter(declaration, UpdateActionCode.Add);
				expectedResult = "CargoWise Support, Customs Brokers and Logistics Worldwide, NASHVILLE BRANCH OFFICE 115 JONESTOWN STREET NASHVILLE TN 55555\r\n PH +1 22 456 789 FX +1 22 456 799 EML support@support.com";
				AssertEquals("Contact Name and Address", "CargoWise Support, Customs Brokers and Logistics Worldwide, NASHVILLE BRANCH OFFICE 115 JONESTOWN STREET NASHVILLE TN 55555", jobDeclarationDrawbackSummary.ContactNameAndAddress);
				AssertEquals("Contact Phone Number", "+1 22 456 789", jobDeclarationDrawbackSummary.ContactPhoneNumber);
				AssertEquals("+1 22 456 799", jobDeclarationDrawbackSummary.ContactFAXNumber);
				AssertEquals("ContactDetails for Company values should use Customs Address of Record", expectedResult, jobDeclarationDrawbackSummary.ContactDetails);
				AssertEquals("Company Name", "Customs Brokers and Logistics Worldwide", jobDeclarationDrawbackSummary.CompanyName);
			}
			finally
			{
				connection.RollbackTransaction();
			}
		}
		public void TestBrokerContactFieldsFor7553Form()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CD;
			var jobDeclarationDrawback7553Form = new JobDeclarationDrawbackSupporter(declaration, UpdateActionCode.Add);

			var addressOfR = Factory.Load<OrgHeader>(declaration.Branch.GB_OH_OrgProxy);
			addressOfR.OH_RL_NKClosestPort = "USNAS";
			addressOfR.Addresses[0].OA_CompanyNameOverride = "Customs Brokers and Logistics Worldwide";
			addressOfR.Addresses[0].OA_Address1 = "Nashville Branch Office";
			addressOfR.Addresses[0].OA_Address2 = "115 JonesTown Street";
			addressOfR.Addresses[0].OA_City = "Nashville";
			addressOfR.Addresses[0].OA_PostCode = "55555";
			addressOfR.Addresses[0].OA_State = "TN";
			addressOfR.Addresses[0].OA_Phone = "+1 22 456 789";
			addressOfR.Addresses[0].OA_Fax = "+1 22 456 799";
			addressOfR.Addresses[0].OA_Email = "test@example.com";
			addressOfR.Addresses[0].AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.CustomsAddressOfRecord);
			Factory.Save();

			AssertEquals("CargoWise Support", GlbStaff.CurrentUser.GS_FullName, jobDeclarationDrawback7553Form.BrokerContactName);
			AssertEquals("Address1", addressOfR.Addresses[0].OA_Address1, jobDeclarationDrawback7553Form.BrokerContactAddressOne);
			AssertEquals("Address3", addressOfR.Addresses[0].OA_Address2, jobDeclarationDrawback7553Form.BrokerContactAddressTwo);
			Assert("State", jobDeclarationDrawback7553Form.BrokerContactState.EqualsAny("TN", "Tennessee"));
			AssertEquals("City", addressOfR.Addresses[0].OA_City, jobDeclarationDrawback7553Form.BrokerContactCity);
			AssertEquals("Postcode", addressOfR.Addresses[0].OA_PostCode, jobDeclarationDrawback7553Form.BrokerContactZip);
			AssertEquals("CompanyName", addressOfR.Addresses[0].OA_CompanyNameOverride, jobDeclarationDrawback7553Form.BrokerContactCompanyName);
		}

		public void TestDeliveryCerfiticateForDrawbackPurposeMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CD;
			var jobDeclarationDrawback7553new = new JobDeclarationDrawbackSupporter(declaration, UpdateActionCode.Add);
			AssertEquals("CertificateOfDelivery", "X", jobDeclarationDrawback7553new.CertificateOfDelivery);
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CM;
			AssertEquals("CertificateOfManufactureAndDelivery", "X", jobDeclarationDrawback7553new.CertificateOfManufactureAndDelivery);

			OrgHeader transferee = Factory.New<OrgHeader>();
			transferee.OH_FullName = "MR CLAIMANT";
			transferee.MainAddress.OA_Address1 = "CLAIMANT ADDRESS 1";
			declaration.US_DRWTransferee = transferee.PK;

			AssertEquals("ClaimantAddress", transferee.MainAddress.AddressAsASingleLine, jobDeclarationDrawback7553new.TransfereeAddress);
		}

		public void TestSortedInvoiceLines()
		{
			var claimant = Factory.New<OrgHeader>();
			claimant.OH_Code = "CLA001";
			claimant.OH_FullName = "MR CLAIMANT";
			claimant.MainAddress.OA_Address1 = "CLAIMANT ADDRESS 1";
			var cusCode = claimant.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			cusCode.OK_CustomsRegNo = "061234";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			USCustomsDataRegistry.Instance.ARecordOfficeCode.SetValue(declaration.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, "89");
			var filer = Factory.NewWithValidTestData<OrgHeader>();
			filer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "364-331 434");
			declaration.JE_OH_NotifyParty = filer.PK;
			declaration.JE_OH_Importer = claimant.PK;
			declaration.JE_MessageStatus = "AAA";
			declaration.JE_EntryStatus = "BBB";
			declaration.US_EntryType = "41";
			declaration.US_ClaimPort = "8888";
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 1);
			declaration.US_TeamNo = "CCC";
			declaration.US_BondType = "8";
			declaration.US_SuretyCode = "12";
			declaration.JE_DeclarationReference = "B12345678";
			declaration.US_PreparerDistrictPort = "3901";
			declaration.JE_GoodsDescription = "XXXXXXXXXX";
			declaration.US_EarliestExportDate = new ZDateTime(2007, 1, 1);
			declaration.US_NAFTADrawbackCountry = "CA";
			declaration.US_EntryFilerCode = "SV9";
			declaration.SetDefaultValuesForDrawback();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.US_DRWIsForImportSection = true;
			invoiceLine2.US_DRWIsForImportSection = true;
			invoiceLine3.US_DRWIsForImportSection = true;
			invoiceLine4.US_DRWIsForImportSection = true;
			invoiceLine1.US_DRWEntryDate = new ZDateTime(2011, 10, 04);
			invoiceLine2.US_DRWEntryDate = new ZDateTime(2011, 09, 30);
			invoiceLine3.US_DRWEntryDate = new ZDateTime(2011, 10, 05);
			invoiceLine4.US_DRWEntryDate = new ZDateTime(2011, 10, 06);

			invoiceLine1.JI_LineNo = 1;
			invoiceLine2.JI_LineNo = 2;
			invoiceLine3.JI_LineNo = 3;
			invoiceLine4.JI_LineNo = 4;

			invoiceLine1.US_DRWPort = "l1";
			invoiceLine2.US_DRWPort = "l2";
			invoiceLine3.US_DRWPort = "l3";
			invoiceLine4.US_DRWPort = "l4";

			Factory.Save();

			var jobDeclarationDrawbackSummary = new JobDeclarationDrawbackSupporter(declaration, UpdateActionCode.Add);
			AssertEquals("invoiceLine2 is the first", "l2", jobDeclarationDrawbackSummary.DrawbackImportSectionLines[0].US_DRWPort);
			AssertEquals("invoiceLine1 is the first", "l1", jobDeclarationDrawbackSummary.DrawbackImportSectionLines[1].US_DRWPort);
			AssertEquals("invoiceLine3 is the first", "l3", jobDeclarationDrawbackSummary.DrawbackImportSectionLines[2].US_DRWPort);
			AssertEquals("invoiceLine4 is the first", "l4", jobDeclarationDrawbackSummary.DrawbackImportSectionLines[3].US_DRWPort);
		}

		public void TestIACEDrawbackSummary()
		{
			var claimant = Factory.New<OrgHeader>();
			claimant.OH_Code = "CLA001";
			claimant.OH_FullName = "MR CLAIMANT";
			claimant.MainAddress.OA_Address1 = "CLAIMANT ADDRESS 1";
			var cusCode = claimant.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			cusCode.OK_CustomsRegNo = "061234";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._01;
			var filer = Factory.NewWithValidTestData<OrgHeader>();
			filer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "364-331 434");
			declaration.JE_OH_NotifyParty = filer.PK;
			declaration.JE_OH_Importer = claimant.PK;
			declaration.US_EntryFilerCode = "SV9";
			declaration.JE_DeclarationReference = "B12345678";
			declaration.US_ClaimPort = "8888";
			declaration.US_PreparerDistrictPort = "9900";
			declaration.US_EntryType = "01";
			declaration.US_BondType = BondTypeList.Codes.NoBondRequired;
			declaration.US_BondWaiverCode = "999";
			declaration.US_AcceleratedClaimInd = true;
			declaration.US_DRWOneTimeWaiverInd = true;
			declaration.US_WaiverNoticeInd = true;
			declaration.US_DRWCommRuling = "2";
			declaration.US_DRWElectPetroleumCert = true;
			declaration.US_DRWElectManufPetroleumCert = true;
			declaration.US_DRWOilSpillTaxCert = true;
			declaration.US_NAFTAClaimInd = true;
			declaration.US_USMCAClaimInd = true;
			declaration.US_DRWIntendedPortOfExport = "1101";
			declaration.US_DRWExamWitness = true;
			declaration.US_DRWLocatOfDest = "ABCD";
			declaration.US_DRWUnUsedWine = true;
			declaration.US_DRWBillOfFormula = true;
			declaration.US_DRWDestroyedValuation = true;
			declaration.US_DRWDestructionResult = DrawbackDestructionResultCodes.Codes.Discrepant;
			declaration.USD_RetailSalesSubstitutionIndicator = true;
			declaration.US_DRWSuperfundInd = true;

			var invoice = declaration.Invoices.AddNew();
			var importSectionInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			importSectionInvoiceLine.US_DRWIsForImportSection = true;
			var exportSectionInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			exportSectionInvoiceLine.US_DRWIsForExportSection = true;

			var jobDeclarationDrawbackSummary = new JobDeclarationDrawbackSupporter(declaration, UpdateActionCode.Add);
			var drawbackSummary = (IACEDrawbackSummary)jobDeclarationDrawbackSummary;
			AssertEquals(false, drawbackSummary.IsTFTEARequired);
			AssertEquals("A", drawbackSummary.ActionRequestCode);
			AssertEquals("SV9", drawbackSummary.EntryFilerCode);
			AssertEquals("B12345678", drawbackSummary.BrokerReferenceNumber);
			AssertEquals("8888", drawbackSummary.ClaimPort);
			AssertEquals("9900", drawbackSummary.ProcessingPort);
			AssertEquals("01", drawbackSummary.ClaimType);
			AssertEquals("0", drawbackSummary.BondWaiverIndicator);
			AssertEquals("999", drawbackSummary.BondWaiverReasonCode);
			AssertEquals("Y", drawbackSummary.AcceleratedClaimIndicator);
			AssertEquals("Y", drawbackSummary.OneTimeWaiverIndicator);
			AssertEquals("Y", drawbackSummary.WavierOfPriorNoticeIndicator);
			AssertEquals("2", drawbackSummary.CommercialInterchangeability);
			AssertEquals("X", drawbackSummary.ElectronicPetroleumCertification);
			AssertEquals("X", drawbackSummary.ElectronicManufacturingPetroleumCertification);
			AssertEquals("X", drawbackSummary.OilSpillTaxCertification);
			AssertEquals("X", drawbackSummary.NAFTADrawbackClaimIndicator);
			AssertEquals("X", drawbackSummary.USMCADrawbackClaimIndicator);
			AssertEquals("X", drawbackSummary.SubstitutedUnusedWineCertification);
			AssertEquals("X", drawbackSummary.BillOfMaterialsFormulaCertification);
			AssertEquals("X", drawbackSummary.CertificationForValuationOfDestroyedMerchandise);
			AssertEquals("X", drawbackSummary.RetailSalesSubstitution);
			AssertEquals("X", drawbackSummary.SuperfundTaxCertification);
			AssertEquals("061234", drawbackSummary.ImporterOfRecordNumber);
			AssertEquals("364-331 434", drawbackSummary.NotifyParty4811Number);
			AssertEquals("1101", drawbackSummary.IntendedPort);
			AssertEquals("X", drawbackSummary.ExaminationWitnessIndicator);
			AssertEquals("ABCD", drawbackSummary.LocationOfDestruction);
			AssertEquals(DrawbackDestructionResultCodes.Codes.Discrepant, drawbackSummary.ResultsOfExamination);
			AssertEquals(0, drawbackSummary.BondDetails.Count());
			AssertEquals(0, drawbackSummary.TFTEADetails.Count());

			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._51;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			AssertEquals(true, drawbackSummary.IsTFTEARequired);
			AssertEquals(1, drawbackSummary.BondDetails.Count());
			AssertEquals(1, drawbackSummary.TFTEADetails.Count());

			declaration.US_DRWSuperfundInd = false;
			AssertEquals("", drawbackSummary.SuperfundTaxCertification);
		}

		public void TestExaminationWitnessIndicator()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.US_DRWExamWitness = true;
			declaration.US_DRWDestructionResult = DrawbackDestructionResultCodes.Codes.Waived;
			var jobDeclarationDrawbackSummary = new JobDeclarationDrawbackSupporter(declaration, UpdateActionCode.Add);
			var drawbackSummary = (IACEDrawbackSummary)jobDeclarationDrawbackSummary;
			AssertEquals("", drawbackSummary.ExaminationWitnessIndicator);
		}

		public void TestIACEDrawbackSummaryResultsOfExamination()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = USJobMessageTypeList.Codes.Drawback;
			var jobDeclarationDrawbackSummary = new JobDeclarationDrawbackSupporter(jobDeclaration, UpdateActionCode.Add);
			var drawbackSummary = (IACEDrawbackSummary)jobDeclarationDrawbackSummary;

			jobDeclaration.US_DRWDestructionResult = DrawbackDestructionResultCodes.Codes.Discrepant;
			AssertEquals(DrawbackDestructionResultCodes.Codes.Discrepant, drawbackSummary.ResultsOfExamination);

			jobDeclaration.US_DRWDestructionResult = DrawbackDestructionResultCodes.Codes.NonDiscrepant;
			AssertEquals(DrawbackDestructionResultCodes.Codes.NonDiscrepant, drawbackSummary.ResultsOfExamination);

			jobDeclaration.US_DRWDestructionResult = DrawbackDestructionResultCodes.Codes.Waived;
			AssertEquals(ZString.Empty, drawbackSummary.ResultsOfExamination);
		}

		public void TestIVisualizerNoteSupporterMembers()
		{
			var declarationSupporter = GetNewBusinessObject();

			var supporter = declarationSupporter as IVisualizerNoteSupporter;
			AssertNotNull("DeclarationDataPrint should implement IVisualizerNoteSupporter", supporter);
			if (supporter != null)
			{
				AssertEquals("supporter.PK", Declaration.PK, supporter.PK);
				AssertEquals("supporter.TableCode", JobDeclarationSchema.Constants.Prefix, supporter.TableCode);
				AssertEquals("supporter.ChildBusinessObjectPK", ZGuid.Empty, supporter.ChildBusinessObjectPK);
			}
		}

		public void TestDrawbackSection()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "Drawback Provision Codes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "01", "1313(A) - Direct Identification Manufacturing Drawback (Articles made from imported merchandise)", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "64", "TFTEA 5062(C) - TFTEA Distilled spirits, wines, or beer which are unmerchantable or do not conform to sample or specifications", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			Factory.Save();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			Declaration.US_EntryType = EntryTypeList.Codes.DirectIdentificationUnusedMerchandiseDrawback;
			var supporter = new JobDeclarationDrawbackSupporter(Declaration, UpdateActionCode.Add);
			AssertEquals(ZString.Empty, supporter.DrawbackSection);
			Declaration.US_DRWSection = "1313(D)";
			AssertEquals("1313(D)", supporter.DrawbackSection);

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._01;
			AssertEquals("1313(A)", supporter.DrawbackSection);
			Declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._64;
			AssertEquals("TFTEA 5062(C)", supporter.DrawbackSection);
			Declaration.US_DRWSection = "ABC123";
			AssertEquals("ABC123", supporter.DrawbackSection);
		}

		public void TestISourceIdentifierProvider()
		{
			var declarationSupporter = GetNewBusinessObject();

			var supporter = declarationSupporter as ISourceIdentifierProvider;
			AssertNotNull("DrawBackJobDeclarationSupporter should implement ISourceIdentifierProvider", supporter);	
			AssertEquals("supporter.SourceIdentifier", Declaration.PK, supporter?.SourceIdentifier);
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				}
				return declaration;
			}
		}

		JobDeclaration declaration;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new JobDeclarationDrawbackSupporter(Declaration, UpdateActionCode.Add);
		}
	}
}
