using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusEntryInstruction))]
	sealed class CusEntryInstructionTest : Customs.Business.Testing.CusEntryInstructionAbstractTest
	{
		[ExpectNoExceptions]
		public void TestAdditionalBizoCaptionsAndDescriptions()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			CombineAssertions(() =>
			{
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.CEI_CustomsOfficeInfo, "Office of Receipt", "The Office of Receipt of the declaration. It's used to generate the first 2 digits of the entry number.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.CEI_ExamModeInfo, "Exam. Mode", "The examination mode of the declaration. The default can be set against the Importer's/Exporter's Organization > Details > Configuration > Taiwan.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.CEI_StyleInfo, "Declaration Type", "The declaration type of entry.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.CEI_DateForDutyInfo, "Declaration Date", "When the declaration is created, the declaration date is defaulted to today.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.CEI_PackageDescriptionInfo, "Package Description", "The package description of the shipment.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.CEI_PrintDutyMemoInfo, "Print Duty Memo", "Tick the box if it is required to print the duty memo for declaration.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.CEI_WaiverOfExemptionInfo, "Waiver of Exemption", "Tick the box if De Minimis exemption is waived for declaration.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.CEI_WHSTradeReferenceNoInfo, "Trade Reference No.", "The reference number of the bonded goods that are reported to customs monthly.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.CEI_WHSMonthInfo, "Month", "The month of the bonded goods From/To bonded warehouse.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.CEI_ReasonForDutyInfo, "Reason for Added Duty", "The reason code of duty to be made up for the bonded goods.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.CEI_DaysOfDelayedDeclarationInfo, "Days of Delayed Declaration", "Days of Delay", "Calculating the days for late declaration fee.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.CEI_OA_WarehouseInfo, "Address", "The organization code of From Bonded Warehouse.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.CEI_OA_Warehouse2Info, "Address", "The organization code of To Bonded warehouse.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.CEI_DutyRefundInfo, "Request Duty Refund", "Indicates if the duty refund is requested for the imported raw materials.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.CEI_IsCoPackagedInfo, "Co-Packaged", "The package is composed of multiple packages.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.CEI_BillOfMaterialsInfo, "Attach Bill of Materials", "Indicates if the list of raw materials used in export products and the information of their suppliers is attached.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.CEI_BOMPageCountInfo, "BoM Page Count", "The page count of the attached list of raw materials used in export products and the information of their suppliers.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.CEI_BoxNumberInfo, "Box Number", "The unique Box Number issued by customs to the customs broker.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.TW_ICIExamLocationInfo, "Examination Zone", "The code of examination zone on declaration.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.TW_ICIExamTimeInfo, "Examination Date", "The cargo examination date requested for the declaration.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.TW_TradersRemarksInfo, "Traders Remarks", "Trader's remarks on declaration.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.AttachedDocumentNumbersAsStringInfo, "Attached Document No.", "The attached document numbers for the declaration. You can use commas (,) to separate multiple document numbers (up to three numbers). Or you can click \"More…\" to open the pop-up window.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.UCRNumberInfo, "UCR Number", "The unique tracking number of the shipment on declaration.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.FromWarehouseCodeTypeInfo, "Bonded ID", "The goods location code issued by customs to From Bonded warehouse.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.FromWarehouseVATCodeInfo, "VAT", "The VAT number of From Bonded warehouse.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.ToWarehouseCodeTypeInfo, "Bonded ID", "The goods location code issued by customs to To Bonded warehouse.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.ToWarehouseVATCodeInfo, "VAT", "The VAT number of the To Bonded warehouse.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryInstruction.CEI_RORPaymentMethodInfo, "ROR Payment Method", "Indicate the payment method of all ROR duty treatment.");
			});
		}

		[ExpectNoExceptions]
		public void TestResetCalculateDaysOfDelayedDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DateAtFinalDestination = new ZDateTime(2019, 03, 01);
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_DaysOfDelayedDeclaration = 1;
			NUnit.Framework.Assert.That(entryInstruction.CEI_DaysOfDelayedDeclaration, NUnit.Framework.Is.EqualTo(1).Using(CustomComparers.TypeComparison));

			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 03, 20);
			entryInstruction.ResetCalculateDaysOfDelayedDeclaration();
			NUnit.Framework.Assert.That(entryInstruction.CEI_DaysOfDelayedDeclaration, NUnit.Framework.Is.EqualTo(4).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDaysOfDelayed()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DateAtFinalDestination = new ZDateTime(2019, 03, 01);
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 03, 20);
			NUnit.Framework.Assert.That(entryInstruction.DaysOfDelayed, NUnit.Framework.Is.EqualTo(DutyCalculationHelper.CalculateDaysDelayedDeclaration(entryInstruction.CEI_DateForDuty, declaration.JE_DateAtFinalDestination)));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123456";
			var mergedLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = mergedLine.PK;
			var entryInstruction = declaration.CusEntryInstruction;
			invoiceLine.JI_CEI = entryInstruction.PK;
			return entryInstruction;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return Factory.NewWithValidTestData<CusEntryInstruction>();
		}

		[ExpectNoExceptions]
		public void TestFromWarehouseVATCode()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryInstruction = jobDeclaration.CusEntryInstruction;
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg1MainAddress = testOrg1.MainAddress;
			testOrg1MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			testOrg1MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "50873964", Core.Constants.CountryCodes.Taiwan);
			var org1Address2 = testOrg1.Addresses.AddNew();
			org1Address2.OA_OH = testOrg1.PK;
			org1Address2.OA_Address1 = "11 notplaceholder avenue";
			org1Address2.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			org1Address2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "96944490", Core.Constants.CountryCodes.Taiwan);
			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg2MainAddress = testOrg2.MainAddress;
			testOrg2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "42523557", Core.Constants.CountryCodes.Taiwan);
			cusEntryInstruction.CEI_OA_Warehouse = testOrg1MainAddress.PK;
			NUnit.Framework.Assert.That(cusEntryInstruction.FromWarehouseVATCode, NUnit.Framework.Is.EqualTo("96944490").Using(CustomComparers.TypeComparison));
			cusEntryInstruction.CEI_OA_Warehouse = org1Address2.PK;
			NUnit.Framework.Assert.That(cusEntryInstruction.FromWarehouseVATCode, NUnit.Framework.Is.EqualTo("96944490").Using(CustomComparers.TypeComparison));
			cusEntryInstruction.CEI_OA_Warehouse = testOrg2MainAddress.PK;
			NUnit.Framework.Assert.That(cusEntryInstruction.FromWarehouseVATCode, NUnit.Framework.Is.EqualTo("42523557").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPropertiesToExcludeFromCloning()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CusEntryInstruction;
				entryInstruction.CEI_DaysOfDelayedDeclaration = 1;
				Factory.Save();
				var templateCopy = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy, Factory).Clone();
				NUnit.Framework.Assert.That(templateCopy.CusEntryInstruction.CEI_DaysOfDelayedDeclaration, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison), "Template Copy");
				var countryToCountryCopy = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.CountryToCountryCopy, Factory).Clone();
				NUnit.Framework.Assert.That(countryToCountryCopy.CusEntryInstruction.CEI_DaysOfDelayedDeclaration, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison), "Country to Country Copy");
			});
		}

		[ExpectNoExceptions]
		public void TestToWarehouseVATCode()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryInstruction = jobDeclaration.CusEntryInstruction;
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg1MainAddress = testOrg1.MainAddress;
			testOrg1MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			testOrg1MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "50873964", Core.Constants.CountryCodes.Taiwan);
			var org1Address2 = testOrg1.Addresses.AddNew();
			org1Address2.OA_OH = testOrg1.PK;
			org1Address2.OA_Address1 = "11 notplaceholder avenue";
			org1Address2.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			org1Address2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "96944490", Core.Constants.CountryCodes.Taiwan);
			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg2MainAddress = testOrg2.MainAddress;
			testOrg2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "42523557", Core.Constants.CountryCodes.Taiwan);
			cusEntryInstruction.CEI_OA_Warehouse2 = testOrg1MainAddress.PK;
			NUnit.Framework.Assert.That(cusEntryInstruction.ToWarehouseVATCode, NUnit.Framework.Is.EqualTo("96944490").Using(CustomComparers.TypeComparison));
			cusEntryInstruction.CEI_OA_Warehouse2 = org1Address2.PK;
			NUnit.Framework.Assert.That(cusEntryInstruction.ToWarehouseVATCode, NUnit.Framework.Is.EqualTo("96944490").Using(CustomComparers.TypeComparison));
			cusEntryInstruction.CEI_OA_Warehouse2 = testOrg2MainAddress.PK;
			NUnit.Framework.Assert.That(cusEntryInstruction.ToWarehouseVATCode, NUnit.Framework.Is.EqualTo("42523557").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestIsBillOfMaterialsEnable()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			NUnit.Framework.Assert.That(!entryInstruction.IsBillOfMaterialsEnable, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Should be disable");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			entryInstruction = declaration.CusEntryInstruction;
			NUnit.Framework.Assert.That(!entryInstruction.IsBillOfMaterialsEnable, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Should be disable");
			entryInstruction.CEI_BillOfMaterials = true;
			NUnit.Framework.Assert.That(entryInstruction.IsBillOfMaterialsEnable, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Should be enable");
			entryInstruction.CEI_BillOfMaterials = false;
			NUnit.Framework.Assert.That(!entryInstruction.IsBillOfMaterialsEnable, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Should be disable");
			declaration.JE_MessageType = "IMP";
			entryInstruction.CEI_BillOfMaterials = true;
			NUnit.Framework.Assert.That(!entryInstruction.IsBillOfMaterialsEnable, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Should be disable");
		}

		[ExpectNoExceptions]
		public void TestTW_ICIExamLocation_Get()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			invoiceLine.JI_CEI = entryInstruction.PK;
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamLocation, NUnit.Framework.Is.EqualTo(ZString.Empty));
			var service = declaration.DocsAndCartage.Services.AddNew();
			service.ES_ServiceCode = ServiceTypes.CommodityInspection;
			service.ES_ServiceNote = "123456";
			service.ES_SubLocation = "123";
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamLocation, NUnit.Framework.Is.EqualTo(ZString.Empty));
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123456";
			var mergedLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = mergedLine.PK;
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamLocation, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
			service.ES_ServiceCode = "AAA";
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamLocation, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestTW_ICIExamLocation_Set()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			invoiceLine.JI_CEI = entryInstruction.PK;
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamLocation, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamLocation_ReadOnly, NUnit.Framework.Is.True);
			entryInstruction.TW_ICIExamLocation = "123";
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamLocation, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(declaration.DocsAndCartage.Services.Count, NUnit.Framework.Is.EqualTo(0));
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123456";
			var mergedLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = mergedLine.PK;
			NUnit.Framework.Assert.That(!entryInstruction.TW_ICIExamLocation_ReadOnly, NUnit.Framework.Is.True);
			entryInstruction.TW_ICIExamLocation = "123";
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamLocation, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(declaration.DocsAndCartage.Services.Count, NUnit.Framework.Is.EqualTo(1));
			var service = declaration.DocsAndCartage.Services[0];
			NUnit.Framework.Assert.That(service.ES_ServiceCode, NUnit.Framework.Is.EqualTo(ServiceTypes.CommodityInspection).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(service.ES_ServiceNote, NUnit.Framework.Is.EqualTo("123456").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(service.ES_SubLocation, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(service.ES_Booked, NUnit.Framework.Is.EqualTo(ZDateTime.Empty));
		}

		[ExpectNoExceptions]
		public void TestTW_ICIExamLocation_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			invoiceLine.JI_CEI = entryInstruction.PK;
			NUnit.Framework.Assert.That(entryInstruction.EntryHeader, NUnit.Framework.Is.EqualTo(default(Customs.Business.CusEntryHeader)));
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamLocation_ReadOnly, NUnit.Framework.Is.True);
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var mergedLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = mergedLine.PK;
			NUnit.Framework.Assert.That(entryInstruction.EntryHeader, NUnit.Framework.Is.Not.EqualTo(default(Customs.Business.CusEntryHeader)));
			NUnit.Framework.Assert.That(entryInstruction.EntryHeader.EntryNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamLocation_ReadOnly, NUnit.Framework.Is.True);
			entryHeader.EntryNumber = "123456";
			NUnit.Framework.Assert.That(!entryInstruction.TW_ICIExamLocation_ReadOnly, NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestTW_ICIExamTime_Get()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			invoiceLine.JI_CEI = entryInstruction.PK;
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamTime, NUnit.Framework.Is.EqualTo(ZDateTime.Empty));
			var service = declaration.DocsAndCartage.Services.AddNew();
			service.ES_ServiceCode = ServiceTypes.CommodityInspection;
			service.ES_ServiceNote = "123456";
			service.ES_Booked = ZDateTime.BrettsBirthday;
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamTime, NUnit.Framework.Is.EqualTo(ZDateTime.Empty));
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123456";
			var mergedLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = mergedLine.PK;
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamTime, NUnit.Framework.Is.EqualTo(ZDateTime.BrettsBirthday));
			service.ES_ServiceCode = "AAA";
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamTime, NUnit.Framework.Is.EqualTo(ZDateTime.Empty));
		}

		[ExpectNoExceptions]
		public void TestTW_ICIExamTime_Set()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			invoiceLine.JI_CEI = entryInstruction.PK;
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamTime, NUnit.Framework.Is.EqualTo(ZDateTime.Empty));
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamTime_ReadOnly, NUnit.Framework.Is.True);
			entryInstruction.TW_ICIExamTime = ZDateTime.BrettsBirthday;
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamTime, NUnit.Framework.Is.EqualTo(ZDateTime.Empty));
			NUnit.Framework.Assert.That(declaration.DocsAndCartage.Services.Count, NUnit.Framework.Is.EqualTo(0));
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123456";
			var mergedLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = mergedLine.PK;
			NUnit.Framework.Assert.That(!entryInstruction.TW_ICIExamTime_ReadOnly, NUnit.Framework.Is.True);
			entryInstruction.TW_ICIExamTime = ZDateTime.BrettsBirthday;
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamTime, NUnit.Framework.Is.EqualTo(ZDateTime.BrettsBirthday));
			NUnit.Framework.Assert.That(declaration.DocsAndCartage.Services.Count, NUnit.Framework.Is.EqualTo(1));
			var service = declaration.DocsAndCartage.Services[0];
			NUnit.Framework.Assert.That(service.ES_ServiceCode, NUnit.Framework.Is.EqualTo(ServiceTypes.CommodityInspection).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(service.ES_ServiceNote, NUnit.Framework.Is.EqualTo("123456").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(service.ES_Booked, NUnit.Framework.Is.EqualTo(ZDateTime.BrettsBirthday));
			NUnit.Framework.Assert.That(service.ES_SubLocation, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestTW_ICIExamTime_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			invoiceLine.JI_CEI = entryInstruction.PK;
			NUnit.Framework.Assert.That(entryInstruction.EntryHeader, NUnit.Framework.Is.EqualTo(default(Customs.Business.CusEntryHeader)));
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamTime_ReadOnly, NUnit.Framework.Is.True);
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var mergedLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = mergedLine.PK;
			NUnit.Framework.Assert.That(entryInstruction.EntryHeader, NUnit.Framework.Is.Not.EqualTo(default(Customs.Business.CusEntryHeader)));
			NUnit.Framework.Assert.That(entryInstruction.EntryHeader.EntryNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamTime_ReadOnly, NUnit.Framework.Is.True);
			entryHeader.EntryNumber = "123456";
			NUnit.Framework.Assert.That(!entryInstruction.TW_ICIExamTime_ReadOnly, NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestExamServiceCache()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			invoiceLine.JI_CEI = entryInstruction.PK;
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamLocation, NUnit.Framework.Is.EqualTo(ZString.Empty));
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123456";
			var mergedLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = mergedLine.PK;
			NUnit.Framework.Assert.That(declaration.DocsAndCartage.Services.Count, NUnit.Framework.Is.EqualTo(0));
			entryInstruction.TW_ICIExamLocation = ZString.Empty;
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamLocation, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(declaration.DocsAndCartage.Services.Count, NUnit.Framework.Is.EqualTo(0), "Setting an Empty location does not create a service");
			entryInstruction.TW_ICIExamLocation = "123";
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamLocation, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(declaration.DocsAndCartage.Services.Count, NUnit.Framework.Is.EqualTo(1), "Setting a location creates a service");
			entryInstruction.TW_ICIExamLocation = "456";
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamLocation, NUnit.Framework.Is.EqualTo("456").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(declaration.DocsAndCartage.Services.Count, NUnit.Framework.Is.EqualTo(1), "Changing the location does not create another service");
			var service = declaration.DocsAndCartage.Services[0];
			service.ES_ServiceCode = "AAA";
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamLocation, NUnit.Framework.Is.EqualTo(ZString.Empty), "Changed Code on Service invalidates cache");
			entryInstruction.TW_ICIExamLocation = "789";
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamLocation, NUnit.Framework.Is.EqualTo("789").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(declaration.DocsAndCartage.Services.Count, NUnit.Framework.Is.EqualTo(2), "Setting a location creates a new service");
			entryHeader.EntryNumber = "654321";
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamLocation, NUnit.Framework.Is.EqualTo(ZString.Empty), "Changed Entry Number on EntryHeader invalidates cache");
			entryInstruction.TW_ICIExamLocation = "255";
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamLocation, NUnit.Framework.Is.EqualTo("255").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(declaration.DocsAndCartage.Services.Count, NUnit.Framework.Is.EqualTo(3), "Setting a location creates a new service for the new entry number");
			declaration.DocsAndCartage.Services[2].Delete();
			NUnit.Framework.Assert.That(entryInstruction.TW_ICIExamLocation, NUnit.Framework.Is.EqualTo(ZString.Empty), "Deleted Service invalidates cache");
		}

		[TestDate(2017, 12, 26)]
		[ExpectNoExceptions]
		public void TestTW_GoodsLocation()
		{
			new TestTWCreator(Factory).CreateRegistryItemCusGoodsLocation();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "CC";
			NUnit.Framework.Assert.That(entryInstruction.CEI_GoodsLocation, NUnit.Framework.Is.EqualTo("ANP0060D").Using(CustomComparers.TypeComparison));
			entryInstruction.CEI_CustomsOffice = "DD";
			NUnit.Framework.Assert.That(entryInstruction.CEI_GoodsLocation, NUnit.Framework.Is.EqualTo("ANP0062D").Using(CustomComparers.TypeComparison));
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			NUnit.Framework.Assert.That(entryInstruction.CEI_CustomsOffice, NUnit.Framework.Is.EqualTo("DD").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryInstruction.CEI_GoodsLocation, NUnit.Framework.Is.EqualTo("ANP0061D").Using(CustomComparers.TypeComparison));
		}

		[TestDate(2017, 12, 26)]
		[ExpectNoExceptions]
		public void TestTW_CustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "49", "49 DESC", new ZDateTime(2017, 12, 25), new ZDateTime(2017, 12, 30));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "6", "6 DESC", new ZDateTime(2017, 12, 25), new ZDateTime(2017, 12, 25));
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "49";
			NUnit.Framework.Assert.That(entryInstruction.Lookups.CustomsOfficeList.GetDescriptionFromCode(entryInstruction.CEI_CustomsOffice), NUnit.Framework.Is.EqualTo("49 DESC"));
			entryInstruction.CEI_CustomsOffice = "6";
			NUnit.Framework.Assert.That(entryInstruction.Lookups.CustomsOfficeList.GetDescriptionFromCode(entryInstruction.CEI_CustomsOffice), NUnit.Framework.Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestEntryNumberComponetsNotReadOnlyWhenIsWaitingForResponseOrHasBeenLodgedAtCustoms()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var heard = declaration.CustomsEntryHeaders.AddNew();
			heard.CH_CEI_Instruction = entryInstruction.PK;
			heard.CH_EntryStatus = EntryStatusCodeList.Codes.RFM;
			CombineAssertions("entryStatus=RFM", () =>
			{
				NUnit.Framework.Assert.That(heard.IsWaitingForResponseOrHasBeenLodgedAtCustoms, NUnit.Framework.Is.True, "IsWaitingForResponseOrHasBeenLodgedAtCustoms");
				NUnit.Framework.Assert.That(!entryInstruction.CEI_CustomsOfficeInfo.ReadOnly, NUnit.Framework.Is.True, "CEI_CustomsOfficeInfo ReadOnly");
				NUnit.Framework.Assert.That(!entryInstruction.CEI_StyleInfo.ReadOnly, NUnit.Framework.Is.True, "CEI_StyleInfo ReadOnly");
				NUnit.Framework.Assert.That(!entryInstruction.CEI_OA_WarehouseInfo.ReadOnly, NUnit.Framework.Is.True, "CEI_OA_WarehouseInfo ReadOnly");
				NUnit.Framework.Assert.That(!entryInstruction.CEI_OA_Warehouse2Info.ReadOnly, NUnit.Framework.Is.True, "CEI_OA_Warehouse2Info ReadOnly");
				NUnit.Framework.Assert.That(!entryInstruction.CEI_BoxNumberInfo.ReadOnly, NUnit.Framework.Is.True, "CEI_BoxNumberInfo ReadOnly");
				NUnit.Framework.Assert.That(!entryInstruction.CEI_DateForDutyInfo.ReadOnly, NUnit.Framework.Is.True, "CEI_DateForDutyInfo ReadOnly");
			});

			heard.CH_EntryStatus = EntryStatusCodeList.Codes.ARM;
			var disposition = heard.CusDispositions.AddNew();
			disposition.CDI_StatusKey = EntryStatusCodeList.Codes.ARM;
			disposition.CDI_Type = Common.CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			disposition.CDI_Status = "A01";
			CombineAssertions("entryStatus=ARM and dispositionStatus=A01", () =>
			{
				NUnit.Framework.Assert.That(!heard.IsWaitingForResponseOrHasBeenLodgedAtCustoms, NUnit.Framework.Is.True, "IsWaitingForResponseOrHasBeenLodgedAtCustoms");
				NUnit.Framework.Assert.That(!entryInstruction.CEI_CustomsOfficeInfo.ReadOnly, NUnit.Framework.Is.True, "CEI_CustomsOfficeInfo ReadOnly");
				NUnit.Framework.Assert.That(!entryInstruction.CEI_StyleInfo.ReadOnly, NUnit.Framework.Is.True, "CEI_StyleInfo ReadOnly");
				NUnit.Framework.Assert.That(!entryInstruction.CEI_OA_WarehouseInfo.ReadOnly, NUnit.Framework.Is.True, "CEI_OA_WarehouseInfo ReadOnly");
				NUnit.Framework.Assert.That(!entryInstruction.CEI_OA_Warehouse2Info.ReadOnly, NUnit.Framework.Is.True, "CEI_OA_Warehouse2Info ReadOnly");
				NUnit.Framework.Assert.That(!entryInstruction.CEI_BoxNumberInfo.ReadOnly, NUnit.Framework.Is.True, "CEI_BoxNumberInfo ReadOnly");
				NUnit.Framework.Assert.That(!entryInstruction.CEI_DateForDutyInfo.ReadOnly, NUnit.Framework.Is.True, "CEI_DateForDutyInfo ReadOnly");
			});

			heard.CH_EntryStatus = ZString.Empty;
			CombineAssertions("entryStatus=''", () =>
			{
				NUnit.Framework.Assert.That(!heard.IsWaitingForResponseOrHasBeenLodgedAtCustoms, NUnit.Framework.Is.True, "IsWaitingForResponseOrHasBeenLodgedAtCustoms");
				NUnit.Framework.Assert.That(!entryInstruction.CEI_CustomsOfficeInfo.ReadOnly, NUnit.Framework.Is.True, "CEI_CustomsOfficeInfo ReadOnly");
				NUnit.Framework.Assert.That(!entryInstruction.CEI_StyleInfo.ReadOnly, NUnit.Framework.Is.True, "CEI_StyleInfo ReadOnly");
				NUnit.Framework.Assert.That(!entryInstruction.CEI_OA_WarehouseInfo.ReadOnly, NUnit.Framework.Is.True, "CEI_OA_WarehouseInfo ReadOnly");
				NUnit.Framework.Assert.That(!entryInstruction.CEI_OA_Warehouse2Info.ReadOnly, NUnit.Framework.Is.True, "CEI_OA_Warehouse2Info ReadOnly");
				NUnit.Framework.Assert.That(!entryInstruction.CEI_BoxNumberInfo.ReadOnly, NUnit.Framework.Is.True, "CEI_BoxNumberInfo ReadOnly");
				NUnit.Framework.Assert.That(!entryInstruction.CEI_DateForDutyInfo.ReadOnly, NUnit.Framework.Is.True, "CEI_DateForDutyInfo ReadOnly");
			});
		}

		[ExpectNoExceptions]
		public void TestGoodsLocation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var facilityCodeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			var facility = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "XXXX0123", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(facility.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "AT");
			Factory.Save();
			var cusEntryInstruction = Factory.New<CusEntryInstruction>();
			cusEntryInstruction.CEI_GoodsLocation = "XXXX0123";
			var c1 = (ICodeDescription)cusEntryInstruction.GoodsLocation;
			NUnit.Framework.Assert.That(c1.Code, NUnit.Framework.Is.EqualTo("XXXX0123"));
			NUnit.Framework.Assert.That(c1.Description, NUnit.Framework.Is.EqualTo("XXXXXX"));
			NUnit.Framework.Assert.That(cusEntryInstruction.GoodsLocation.Attributes.HasAttribute(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "AT"), NUnit.Framework.Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestCEI_StyleDescription()
		{
			new TestTWCreator(Factory).CreateRefCusCodeForDeclarationType();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.B6;
			NUnit.Framework.Assert.That(entryInstruction.CEI_StyleDescription, NUnit.Framework.Is.EqualTo("保稅廠輸入貨物(原料)").Using(CustomComparers.TypeComparison));
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.G3;
			NUnit.Framework.Assert.That(entryInstruction.CEI_StyleDescription, NUnit.Framework.Is.EqualTo("外貨復出口").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCEI_StyleWithSetterSuspender()
		{
			var universalDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var cusCodeType = universalDataHelper.CreateNewOrGetExistingCusCodeType("FAC", "Facility Code");
			var cusCodeList = universalDataHelper.CreateNewOrGetExistingCusCodeList("TW", "FAC", "ZZZZZ", "保稅廠貨物卸存地點", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var cusCodeListAttribute1 = universalDataHelper.CreateNewOrGetExistingCusCodeListAttribute(cusCodeList.PK, "CUSTOMSOFFICE", "BG");
			var cusCodeListAttribute2 = universalDataHelper.CreateNewOrGetExistingCusCodeListAttribute(cusCodeList.PK, "CUSTOMSOFFICE", "AA");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_DutyRefund = true;
			entryInstruction.CEI_WHSMonth = "02";
			entryInstruction.CEI_CustomsOffice = "BG";
			entryInstruction.CEI_DaysOfDelayedDeclaration = 2;
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_CusValueConvRatio = 1M;
			using (entryInstruction.SetterSuspender.SuspendSetting(CusEntryInstruction.Schema.CEI_Style))
			{
				CombineAssertions(() =>
				{
					entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F2;
					NUnit.Framework.Assert.That(invoiceLine.JI_CusValueConvRatio, NUnit.Framework.Is.Not.EqualTo(ZDecimal.Zero), "JI_CusValueConvRatio");
					entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F5;
					NUnit.Framework.Assert.That(entryInstruction.CEI_DutyRefund, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "CEI_DutyRefund");
					entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F3;
					NUnit.Framework.Assert.That(entryInstruction.CEI_GoodsLocation, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "CEI_GoodsLocation");
					entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D2;
					NUnit.Framework.Assert.That(entryInstruction.CEI_DaysOfDelayedDeclaration, NUnit.Framework.Is.Not.EqualTo(0).Using(CustomComparers.TypeComparison), "CEI_DaysOfDelayedDeclaration");
				}

				);
			}
		}

		public void TestICusCodeDataTypeSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			ICusCodeDataTypeSupporter supporter = instruction;
			supporter.AssertType(typeof(DeclarationDuplicate), CusCodeDataTypeList.Codes.DeclarationDuplicate);
			supporter.AssertType(null, "XXX");
			var declarationDuplicate = instruction.DeclarationDuplicates.AddNew();
			declarationDuplicate.CY_Data = "XXX";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var codeData = newFactory.Load<CusCodeData>(declarationDuplicate.PK);
			NUnit.Framework.Assert.That(codeData.GetType(), NUnit.Framework.Is.EqualTo(typeof(DeclarationDuplicate)));
		}

		[ExpectNoExceptions]
		public void TestCEI_OH_Owner()
		{
			var declaration = Factory.New<JobDeclaration>();
			var owner1 = Factory.New<OrgHeader>();
			var instruction = declaration.CusEntryInstruction;
			var jobComInvoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			jobComInvoiceLine.JI_CEI = instruction.PK;
			instruction.CEI_OH_Owner = owner1.PK;
			jobComInvoiceLine.JI_NewOwnerPartNo = "XXX";
			NUnit.Framework.Assert.That(!jobComInvoiceLine.JI_NewOwnerPartNo.IsEmpty, NUnit.Framework.Is.True);
			instruction.CEI_OH_Owner = ZGuid.Empty;
			NUnit.Framework.Assert.That(jobComInvoiceLine.JI_NewOwnerPartNo.IsEmpty, NUnit.Framework.Is.True);
			jobComInvoiceLine.JI_NewOwnerPartNo = "XXX";
			var owner2 = Factory.New<OrgHeader>();
			instruction.CEI_OH_Owner = owner2.PK;
			NUnit.Framework.Assert.That(jobComInvoiceLine.JI_NewOwnerPartNo.IsEmpty, NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestTW_DutyRefundReadOnly()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			jobDeclaration.JE_RL_NKFinalDestination = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Taiwan)).RL_Code;
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			entryInstruction.CEI_DutyRefund = true;
			entryInstruction.CEI_Style = "B1";
			NUnit.Framework.Assert.That(entryInstruction.CEI_DutyRefundInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false));
			entryInstruction.CEI_Style = "F5";
			NUnit.Framework.Assert.That(entryInstruction.CEI_DutyRefundInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false));
			jobDeclaration.JE_RL_NKFinalDestination = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Italy)).RL_Code;
			NUnit.Framework.Assert.That(entryInstruction.CEI_DutyRefundInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true));
			NUnit.Framework.Assert.That(entryInstruction.CEI_DutyRefund, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTW_GoodsLocationValueAndReadOnly()
		{
			var universalDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var cusCodeType = universalDataHelper.CreateNewOrGetExistingCusCodeType("FAC", "Facility Code");
			Factory.Save();
			var cusCodeList = universalDataHelper.CreateNewOrGetExistingCusCodeList("TW", "FAC", "ZZZZZ", "保稅廠貨物卸存地點", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var cusCodeListAttribute1 = universalDataHelper.CreateNewOrGetExistingCusCodeListAttribute(cusCodeList.PK, "CUSTOMSOFFICE", "BG");
			var cusCodeListAttribute2 = universalDataHelper.CreateNewOrGetExistingCusCodeListAttribute(cusCodeList.PK, "CUSTOMSOFFICE", "AA");
			Factory.Save();
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			entryInstruction.CEI_GoodsLocation = "LOCATION";
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B1;
			entryInstruction.CEI_WHSMonth = "04";
			NUnit.Framework.Assert.That(entryInstruction.CEI_GoodsLocationInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false));
			NUnit.Framework.Assert.That(entryInstruction.CEI_GoodsLocation, NUnit.Framework.Is.EqualTo("LOCATION").Using(CustomComparers.TypeComparison));
			jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F3;
			entryInstruction.CEI_WHSMonth = "02";
			entryInstruction.CEI_CustomsOffice = "BG";
			NUnit.Framework.Assert.That(entryInstruction.CEI_GoodsLocationInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false));
			NUnit.Framework.Assert.That(entryInstruction.CEI_GoodsLocation, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestIsInvoiceLineMerginAllowed()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			NUnit.Framework.Assert.That(entryInstruction.IsInvoiceLinesMergingAllowed, NUnit.Framework.Is.EqualTo(false));
			entryInstruction.CEI_ExamMode = ExamModeList.Codes.FactoryInspection;
			NUnit.Framework.Assert.That(entryInstruction.IsInvoiceLinesMergingAllowed, NUnit.Framework.Is.EqualTo(false));
			for (int i = 0; i < 50; i++)
			{
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
			}

			NUnit.Framework.Assert.That(entryInstruction.IsInvoiceLinesMergingAllowed, NUnit.Framework.Is.EqualTo(false));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G1;
			NUnit.Framework.Assert.That(entryInstruction.IsInvoiceLinesMergingAllowed, NUnit.Framework.Is.EqualTo(false));
			entryInstruction.CEI_ExamMode = ExamModeList.Codes.WrittenReview;
			NUnit.Framework.Assert.That(entryInstruction.IsInvoiceLinesMergingAllowed, NUnit.Framework.Is.EqualTo(false));
			var invoiceLine51 = declaration.InvoiceLines.AddNew();
			invoiceLine51.JI_CEI = entryInstruction.PK;
			NUnit.Framework.Assert.That(entryInstruction.IsInvoiceLinesMergingAllowed, NUnit.Framework.Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestDefaultExamModeFromOrgImpAddInfo()
		{
			var organization1 = Factory.New<OrgHeader>();
			var organization2 = Factory.New<OrgHeader>();
			TWOrgImpAddInfo.Get(organization1).ZO_TWDefaultExamMode = "2";
			TWOrgImpAddInfo.Get(organization2).ZO_TWDefaultExamMode = "3";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_ExamMode = ZString.Empty;
			declaration.JE_OH_Importer = organization1.PK;
			NUnit.Framework.Assert.That(entryInstruction.CEI_ExamMode, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison));
			declaration.JE_OH_Importer = organization2.PK;
			NUnit.Framework.Assert.That(entryInstruction.CEI_ExamMode, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison));
			entryInstruction.CEI_ExamMode = ZString.Empty;
			declaration.JE_OH_Supplier = organization1.PK;
			NUnit.Framework.Assert.That(entryInstruction.CEI_ExamMode, NUnit.Framework.Is.EqualTo("3").Using(CustomComparers.TypeComparison));
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Supplier = organization2.PK;
			NUnit.Framework.Assert.That(entryInstruction.CEI_ExamMode, NUnit.Framework.Is.EqualTo("3").Using(CustomComparers.TypeComparison));
			declaration.JE_OH_Supplier = organization1.PK;
			NUnit.Framework.Assert.That(entryInstruction.CEI_ExamMode, NUnit.Framework.Is.EqualTo("3").Using(CustomComparers.TypeComparison));
			entryInstruction.CEI_ExamMode = ZString.Empty;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Importer = organization2.PK;
			NUnit.Framework.Assert.That(entryInstruction.CEI_ExamMode, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestHasWarehouse2CCPAddress()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var warehouseAddress1 = orgHeader1.MainAddress;
			warehouseAddress1.Address1 = "Address1";
			warehouseAddress1.Address2 = "Address2";
			warehouseAddress1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "00612348", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var org2Code2 = orgHeader2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.AccountsPayableSuppliersReference, "00612348", Core.Constants.CountryCodes.Taiwan);
			var warehouseAddress2 = orgHeader2.MainAddress;
			warehouseAddress2.Address1 = "Address1";
			warehouseAddress2.Address2 = "Address2";
			org2Code2.OK_OA_PremisesAddress = orgHeader2.MainAddress.PK;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_OA_Warehouse2 = orgHeader1.MainAddress.PK;
			NUnit.Framework.Assert.That(entryInstruction.HasWarehouse2CCPAddress, NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_OA_Warehouse2 = orgHeader2.MainAddress.PK;
			NUnit.Framework.Assert.That(entryInstruction.HasWarehouse2CCPAddress, NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestTW_DaysOfDelayedDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D8;
			entryInstruction.CEI_DaysOfDelayedDeclaration = 3;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			NUnit.Framework.Assert.That(entryInstruction.CEI_DaysOfDelayedDeclaration, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison));
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			entryInstruction.CEI_DaysOfDelayedDeclaration = 2;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D2;
			NUnit.Framework.Assert.That(entryInstruction.CEI_DaysOfDelayedDeclaration, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTW_DaysOfDelayedDeclarationReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D8;
			NUnit.Framework.Assert.That(entryInstruction.TW_DaysOfDelayedDeclarationReadOnly, NUnit.Framework.Is.True);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			NUnit.Framework.Assert.That(!entryInstruction.TW_DaysOfDelayedDeclarationReadOnly, NUnit.Framework.Is.True);
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D2;
			NUnit.Framework.Assert.That(entryInstruction.TW_DaysOfDelayedDeclarationReadOnly, NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestUCR()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var cei = declaration.CusEntryInstruction;
			NUnit.Framework.Assert.That(!cei.UCRNumber_ReadOnly, NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(cei.UCROverride, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(cei.UCROverride_ReadOnly, NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(!cei.UCRNumberInfo.ReadOnly, NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(cei.UCROverrideInfo.ReadOnly, NUnit.Framework.Is.True);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			NUnit.Framework.Assert.That(cei.UCRNumber_ReadOnly, NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(!cei.UCROverride, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(!cei.UCROverride_ReadOnly, NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(cei.UCRNumberInfo.ReadOnly, NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(!cei.UCROverrideInfo.ReadOnly, NUnit.Framework.Is.True);
			cei.UCROverride = true;
			NUnit.Framework.Assert.That(!cei.UCRNumber_ReadOnly, NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(!cei.UCROverride_ReadOnly, NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(!cei.UCRNumberInfo.ReadOnly, NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(!cei.UCROverrideInfo.ReadOnly, NUnit.Framework.Is.True);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			cei.UCRNumber = "123";
			NUnit.Framework.Assert.That(cei.UCRNumber, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			NUnit.Framework.Assert.That(cei.UCRNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(cei.UCROverride, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			cei.UCROverride = true;
			NUnit.Framework.Assert.That(cei.UCRNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			cei.UCRNumber = "123";
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			NUnit.Framework.Assert.That(cei.UCRNumber, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(cei.UCROverride, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[TestDate(2019, 05, 16, 4, 33, 20, 1)]
		[ExpectNoExceptions]
		public void TestUpdateUCR()
		{
			var dec = Factory.New<JobDeclaration>();
			var cei = dec.CusEntryInstruction;
			var entry = dec.ActiveEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = cei.PK;
			entry.CH_EntrySubmittedDate = TestDateAttribute.Date;
			cei.CEI_DateForDuty = TestDateAttribute.Date;
			var cusNum = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum.CE_ParentID = entry.PK;
			cusNum.CE_Category = "CUS";
			cusNum.CE_EntryType = "IMP";
			cusNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum.CE_EntryNum = "1111";
			var supOrg = Factory.New<OrgHeader>();
			supOrg.OH_Code = "DEF";
			var invoice = dec.Invoices.AddNew();
			var invLine = dec.InvoiceLines.AddNew();
			invLine.JI_JZ = invoice.PK;
			var entryLine1 = Factory.NewWithValidTestData<CusEntryLine>();
			entryLine1.CL_CH = entry.PK;
			invLine.JI_CL = entryLine1.PK;
			dec.JE_OH_Supplier = supOrg.PK;
			supOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.PID, "PID001", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			cei.UCRNumber = ZString.Empty;
			dec.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			cei.UpdateUCR();
			NUnit.Framework.Assert.That(cei.UCRNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			dec.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			cei.UCROverride = false;
			cei.UpdateUCR();
			NUnit.Framework.Assert.That(cei.UCRNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			cei.UCROverride = true;
			cei.UpdateUCR();
			NUnit.Framework.Assert.That(cei.UCRNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			supOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "VAT001", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			cei.UCRNumber = ZString.Empty;
			dec.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			cei.UpdateUCR();
			NUnit.Framework.Assert.That(cei.UCRNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			dec.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			cei.UCROverride = true;
			cei.UpdateUCR();
			NUnit.Framework.Assert.That(cei.UCRNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			cei.UCROverride = false;
			cei.CEI_DateForDuty = TestDateAttribute.Date.AddYears(-1);
			cei.UpdateUCR();
			NUnit.Framework.Assert.That(cei.UCRNumber, NUnit.Framework.Is.EqualTo("8TWVAT00120190516043320001").Using(CustomComparers.TypeComparison));
			cei.CEI_DateForDuty = TestDateAttribute.Date.AddYears(1);
			cei.UpdateUCR();
			NUnit.Framework.Assert.That(cei.UCRNumber, NUnit.Framework.Is.EqualTo("0TWVAT00120190516043320001").Using(CustomComparers.TypeComparison));
			cei.CEI_DateForDuty = ZDateTime.Empty;
			cei.UpdateUCR();
			NUnit.Framework.Assert.That(cei.UCRNumber, NUnit.Framework.Is.EqualTo("9TWVAT00120190516043320001").Using(CustomComparers.TypeComparison));
			entry.CH_EntryStatus = EntryStatusCodeList.Codes.RFM;
			cusNum.CE_IssueDate = ZDateTime.Empty;
			cei.UCRNumber = ZString.Empty;
			cei.UpdateUCR();
			NUnit.Framework.Assert.That(cei.UCRNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			cusNum.CE_IssueDate = ZDateTime.Now;
			entry.CH_Status = MessageStatusList.Codes.AwaitingReplace;
			cei.UpdateUCR();
			NUnit.Framework.Assert.That(cei.UCRNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestControllingMessageHeaders()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var controllingMessageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			NUnit.Framework.Assert.That(controllingMessageHeader.TW1_CEI, NUnit.Framework.Is.EqualTo(entryInstruction.PK));
			NUnit.Framework.Assert.That(controllingMessageHeaders.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(entryInstruction.IsRegisteredEditableChildObject(controllingMessageHeaders), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestHasValidControllingMsgHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			NUnit.Framework.Assert.That(!entryInstruction.HasValidControllingMsgHeader, NUnit.Framework.Is.True);
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			NUnit.Framework.Assert.That(controllingMessageHeaders.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(!entryInstruction.HasValidControllingMsgHeader, NUnit.Framework.Is.True);
			controllingMessageHeader = controllingMessageHeaders.AddNew();
			NUnit.Framework.Assert.That(controllingMessageHeaders.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(!entryInstruction.HasValidControllingMsgHeader, NUnit.Framework.Is.True);
			controllingMessageHeader.PermitNumber = "1";
			NUnit.Framework.Assert.That(entryInstruction.HasValidControllingMsgHeader, NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestIsShippingFromFactoryToDutyLevyingArea()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.FinishedProductDomesticSales;
			NUnit.Framework.Assert.That(entryInstruction.IsShippingFromFactoryToDutyLevyingArea, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "When CEI_ReasonForDuty equal 01 should be ");
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.PortEnterpriseSupplementary;
			NUnit.Framework.Assert.That(entryInstruction.IsShippingFromFactoryToDutyLevyingArea, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "When CEI_ReasonForDuty equal 13 should be ");
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.Other;
			NUnit.Framework.Assert.That(entryInstruction.IsShippingFromFactoryToDutyLevyingArea, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "When CEI_ReasonForDuty equal 99 should be ");
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.OriginalDomesticSales;
			NUnit.Framework.Assert.That(!entryInstruction.IsShippingFromFactoryToDutyLevyingArea, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "When CEI_ReasonForDuty equal 03 should be ");
		}

		[ExpectNoExceptions]
		public void TestIsShippedFromFreeTradeZoneToDutyLevyingArea()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.SupplementaryEquipment;
			NUnit.Framework.Assert.That(entryInstruction.IsShippedFromFreeTradeZoneToDutyLevyingArea, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "When CEI_ReasonForDuty equal 04 should be ");
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.PoorDiscSupplementary;
			NUnit.Framework.Assert.That(entryInstruction.IsShippedFromFreeTradeZoneToDutyLevyingArea, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "When CEI_ReasonForDuty equal 05 should be ");
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.StolenFinishedGoodsSupplementary;
			NUnit.Framework.Assert.That(entryInstruction.IsShippedFromFreeTradeZoneToDutyLevyingArea, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "When CEI_ReasonForDuty equal 06 should be ");
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.StolenEquipmentSupplementary;
			NUnit.Framework.Assert.That(entryInstruction.IsShippedFromFreeTradeZoneToDutyLevyingArea, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "When CEI_ReasonForDuty equal 07 should be ");
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.PortEnterpriseSupplementary;
			NUnit.Framework.Assert.That(entryInstruction.IsShippedFromFreeTradeZoneToDutyLevyingArea, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "When CEI_ReasonForDuty equal 13 should be ");
			entryInstruction.CEI_ReasonForDuty = ReasonforDutyList.Codes.OriginalDomesticSales;
			NUnit.Framework.Assert.That(!entryInstruction.IsShippedFromFreeTradeZoneToDutyLevyingArea, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "When CEI_ReasonForDuty equal 03 should be ");
		}

		[ExpectNoExceptions]
		public void TestFromWarehouse()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryInstruction = jobDeclaration.CusEntryInstruction;
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg1MainAddress = testOrg1.MainAddress;
			testOrg1MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			testOrg1MainAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.EPZ, "11111", Core.Constants.CountryCodes.Taiwan);
			var orgAddress2 = testOrg1.Addresses.AddNew();
			orgAddress2.OA_OH = testOrg1.PK;
			orgAddress2.OA_Address1 = "11 notplaceholder avenue";
			orgAddress2.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			orgAddress2.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "22222", Core.Constants.CountryCodes.Taiwan);
			var orgAddress3 = testOrg1.Addresses.AddNew();
			orgAddress3.OA_OH = testOrg1.PK;
			orgAddress3.OA_Address1 = "22 notplaceholder avenue";
			orgAddress3.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			orgAddress3.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.FTZ, "PAA01", Core.Constants.CountryCodes.Taiwan);
			var orgAddress4 = testOrg1.Addresses.AddNew();
			orgAddress4.OA_OH = testOrg1.PK;
			orgAddress4.OA_Address1 = "33 notplaceholder avenue";
			orgAddress4.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			orgAddress4.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "33333", Core.Constants.CountryCodes.Taiwan);
			var orgAddress5 = testOrg1.Addresses.AddNew();
			orgAddress5.OA_OH = testOrg1.PK;
			orgAddress5.OA_Address1 = "44 notplaceholder avenue";
			orgAddress5.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			orgAddress5.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "44444", Core.Constants.CountryCodes.Taiwan);
			cusEntryInstruction.CEI_OA_Warehouse = testOrg1MainAddress.PK;
			NUnit.Framework.Assert.That(cusEntryInstruction.FromWarehouseCodeType.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(cusEntryInstruction.FromWarehouseCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			cusEntryInstruction.CEI_OA_Warehouse = orgAddress2.PK;
			NUnit.Framework.Assert.That(cusEntryInstruction.FromWarehouseCodeType.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(cusEntryInstruction.FromWarehouseCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			cusEntryInstruction.CEI_OA_Warehouse = orgAddress3.PK;
			NUnit.Framework.Assert.That(cusEntryInstruction.FromWarehouseCodeType.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(cusEntryInstruction.FromWarehouseCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			cusEntryInstruction.CEI_OA_Warehouse = orgAddress4.PK;
			NUnit.Framework.Assert.That(cusEntryInstruction.FromWarehouseCodeType, NUnit.Framework.Is.EqualTo("CPW").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(cusEntryInstruction.FromWarehouseCode, NUnit.Framework.Is.EqualTo("33333").Using(CustomComparers.TypeComparison));
			cusEntryInstruction.CEI_OA_Warehouse = orgAddress5.PK;
			NUnit.Framework.Assert.That(cusEntryInstruction.FromWarehouseCodeType, NUnit.Framework.Is.EqualTo("CCP").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(cusEntryInstruction.FromWarehouseCode, NUnit.Framework.Is.EqualTo("44444").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestToWarehouse()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryInstruction = jobDeclaration.CusEntryInstruction;
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg1MainAddress = testOrg1.MainAddress;
			testOrg1MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			testOrg1MainAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.EPZ, "11111", Core.Constants.CountryCodes.Taiwan);
			var orgAddress2 = testOrg1.Addresses.AddNew();
			orgAddress2.OA_OH = testOrg1.PK;
			orgAddress2.OA_Address1 = "11 notplaceholder avenue";
			orgAddress2.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			orgAddress2.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "22222", Core.Constants.CountryCodes.Taiwan);
			var orgAddress3 = testOrg1.Addresses.AddNew();
			orgAddress3.OA_OH = testOrg1.PK;
			orgAddress3.OA_Address1 = "22 notplaceholder avenue";
			orgAddress3.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			orgAddress3.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.FTZ, "PAA01", Core.Constants.CountryCodes.Taiwan);
			var orgAddress4 = testOrg1.Addresses.AddNew();
			orgAddress4.OA_OH = testOrg1.PK;
			orgAddress4.OA_Address1 = "33 notplaceholder avenue";
			orgAddress4.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			orgAddress4.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "33333", Core.Constants.CountryCodes.Taiwan);
			var orgAddress5 = testOrg1.Addresses.AddNew();
			orgAddress5.OA_OH = testOrg1.PK;
			orgAddress5.OA_Address1 = "44 notplaceholder avenue";
			orgAddress5.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			orgAddress5.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "44444", Core.Constants.CountryCodes.Taiwan);
			cusEntryInstruction.CEI_OA_Warehouse2 = testOrg1MainAddress.PK;
			NUnit.Framework.Assert.That(cusEntryInstruction.ToWarehouseCodeType.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(cusEntryInstruction.ToWarehouseCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			cusEntryInstruction.CEI_OA_Warehouse2 = orgAddress2.PK;
			NUnit.Framework.Assert.That(cusEntryInstruction.ToWarehouseCodeType.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(cusEntryInstruction.ToWarehouseCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			cusEntryInstruction.CEI_OA_Warehouse2 = orgAddress3.PK;
			NUnit.Framework.Assert.That(cusEntryInstruction.ToWarehouseCodeType.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(cusEntryInstruction.ToWarehouseCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			cusEntryInstruction.CEI_OA_Warehouse2 = orgAddress4.PK;
			NUnit.Framework.Assert.That(cusEntryInstruction.ToWarehouseCodeType, NUnit.Framework.Is.EqualTo("CPW").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(cusEntryInstruction.ToWarehouseCode, NUnit.Framework.Is.EqualTo("33333").Using(CustomComparers.TypeComparison));
			cusEntryInstruction.CEI_OA_Warehouse2 = orgAddress5.PK;
			NUnit.Framework.Assert.That(cusEntryInstruction.ToWarehouseCodeType, NUnit.Framework.Is.EqualTo("CCP").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(cusEntryInstruction.ToWarehouseCode, NUnit.Framework.Is.EqualTo("44444").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDefaultWarehouse()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.FillWithValidTestData();
			var testCpwAddress = testOrg.MainAddress;
			testCpwAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			testCpwAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "11111", Core.Constants.CountryCodes.Taiwan);
			var testCcpAddress = testOrg.Addresses.AddNew();
			testCcpAddress.FillWithValidTestData();
			testCcpAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			testCcpAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "22222", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryInstruction = jobDeclaration.CusEntryInstruction;
			NUnit.Framework.Assert.That(cusEntryInstruction.CEI_OA_Warehouse.IsEmpty, NUnit.Framework.Is.True);
			jobDeclaration.SupplierDocumentaryAddress.E2_OA_Address = testCpwAddress.PK;
			NUnit.Framework.Assert.That(cusEntryInstruction.CEI_OA_Warehouse.IsEmpty, NUnit.Framework.Is.True);
			AssertDefaultWarehouse(Constants.DeclarationTypes.Import.D2);
			AssertDefaultWarehouse(Constants.DeclarationTypes.Export.D5);
			AssertDefaultWarehouse(Constants.DeclarationTypes.Import.D7);
			void AssertDefaultWarehouse(ZString declarationType)
			{
				jobDeclaration.SupplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
				cusEntryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
				cusEntryInstruction.CEI_Style = declarationType;
				jobDeclaration.SupplierDocumentaryAddress.E2_OA_Address = testCpwAddress.PK;
				NUnit.Framework.Assert.That(cusEntryInstruction.CEI_OA_Warehouse, NUnit.Framework.Is.EqualTo(testCpwAddress.PK));
				jobDeclaration.SupplierDocumentaryAddress.E2_OA_Address = testCcpAddress.PK;
				NUnit.Framework.Assert.That(cusEntryInstruction.CEI_OA_Warehouse, NUnit.Framework.Is.EqualTo(testCcpAddress.PK));
			}
		}

		[ExpectNoExceptions]
		public void TestDefaultWarehouse2()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.FillWithValidTestData();
			var testCpwAddress = testOrg.MainAddress;
			testCpwAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			testCpwAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "11111", Core.Constants.CountryCodes.Taiwan);
			var testCcpAddress = testOrg.Addresses.AddNew();
			testCcpAddress.FillWithValidTestData();
			testCcpAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			testCcpAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "22222", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryInstruction = jobDeclaration.CusEntryInstruction;
			NUnit.Framework.Assert.That(cusEntryInstruction.CEI_OA_Warehouse2.IsEmpty, NUnit.Framework.Is.True);
			jobDeclaration.ImporterDocumentaryAddress.E2_OA_Address = testCpwAddress.PK;
			NUnit.Framework.Assert.That(cusEntryInstruction.CEI_OA_Warehouse2.IsEmpty, NUnit.Framework.Is.True);
			AssertDefaultWarehouse2(Constants.DeclarationTypes.Export.D1);
			AssertDefaultWarehouse2(Constants.DeclarationTypes.Import.D8);
			AssertDefaultWarehouse2(Constants.DeclarationTypes.Export.B2);
			AssertDefaultWarehouse2(Constants.DeclarationTypes.Import.D7);
			void AssertDefaultWarehouse2(ZString declarationType)
			{
				jobDeclaration.ImporterDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
				cusEntryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				cusEntryInstruction.CEI_Style = declarationType;
				jobDeclaration.ImporterDocumentaryAddress.E2_OA_Address = testCpwAddress.PK;
				NUnit.Framework.Assert.That(cusEntryInstruction.CEI_OA_Warehouse2, NUnit.Framework.Is.EqualTo(testCpwAddress.PK));
				jobDeclaration.ImporterDocumentaryAddress.E2_OA_Address = testCcpAddress.PK;
				NUnit.Framework.Assert.That(cusEntryInstruction.CEI_OA_Warehouse2, NUnit.Framework.Is.EqualTo(testCcpAddress.PK));
			}
		}

		[ExpectNoExceptions]
		public void TestSetDefaultCEI_Style()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			var instruction = declaration.CusEntryInstruction;
			instruction.SetDefaultCEI_Style();
			NUnit.Framework.Assert.That(instruction.CEI_Style, NUnit.Framework.Is.EqualTo(Constants.DeclarationTypes.Export.G5).Using(CustomComparers.TypeComparison));
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			instruction.SetDefaultCEI_Style();
			NUnit.Framework.Assert.That(instruction.CEI_Style, NUnit.Framework.Is.EqualTo(Constants.DeclarationTypes.Import.G1).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValueForBoxNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			new TestTWCreator(Factory).CreateRegistryItemCusBrokerageBoxNumber();
			entryInstruction.CEI_BoxNumber = ZString.Empty;
			entryInstruction.CEI_CustomsOffice = "AA";
			NUnit.Framework.Assert.That(entryInstruction.CEI_BoxNumber, NUnit.Framework.Is.EqualTo("600").Using(CustomComparers.TypeComparison));
			entryInstruction.CEI_CustomsOffice = "BA";
			NUnit.Framework.Assert.That(entryInstruction.CEI_BoxNumber, NUnit.Framework.Is.EqualTo("100").Using(CustomComparers.TypeComparison));
			entryInstruction.CEI_CustomsOffice = "XX";
			NUnit.Framework.Assert.That(entryInstruction.CEI_BoxNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[TestDate(2020, 05, 15)]
		[ExpectNoExceptions]
		public void TestTW_TradersRemarks()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var doc = testOrg.RequiredDocuments.AddNew();
			doc.EQ_DocCategory = "CSR";
			doc.EQ_DocType = "POA";
			doc.EQ_DocPeriod = "PER";
			doc.EQ_DateReceived = new ZDateTimeOffset(2020, 7, 17);
			doc.EQ_ValidToDate = new ZDateTime(2025, 5, 15);
			doc.EQ_DocNumber = "236547";
			doc.EQ_DocUsage = "BRK";
			doc.EQ_RN_NKRelatedCountry = "TW";
			var docAtt = doc.Attributes[JobRequiredDocAttribTypeList.Codes.CustomsDistrict];
			docAtt.D0_AttribValue = "A";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "AA";
			NUnit.Framework.Assert.That(!entryInstruction.TW_OverrideTradersRemarks, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryInstruction.TW_TradersRemarks, NUnit.Framework.Is.EqualTo(ZString.Empty));
			declaration.JE_OH_Supplier = testOrg.PK;
			NUnit.Framework.Assert.That(!entryInstruction.TW_OverrideTradersRemarks, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryInstruction.TW_TradersRemarks, NUnit.Framework.Is.EqualTo("常年(長期)委任報關核准文號：236547\r\n起：109年07月17日\r\n迄：114年05月15日").Using(CustomComparers.TypeComparison));
			entryInstruction.TW_TradersRemarks += " Test 1234";
			NUnit.Framework.Assert.That(entryInstruction.TW_OverrideTradersRemarks, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryInstruction.TW_TradersRemarks, NUnit.Framework.Is.EqualTo("常年(長期)委任報關核准文號：236547\r\n起：109年07月17日\r\n迄：114年05月15日 Test 1234").Using(CustomComparers.TypeComparison));
			entryInstruction.TW_OverrideTradersRemarks = false;
			NUnit.Framework.Assert.That(!entryInstruction.TW_OverrideTradersRemarks, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryInstruction.TW_TradersRemarks, NUnit.Framework.Is.EqualTo("常年(長期)委任報關核准文號：236547\r\n起：109年07月17日\r\n迄：114年05月15日").Using(CustomComparers.TypeComparison));
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_MessageType = "IMP";
			NUnit.Framework.Assert.That(!entryInstruction.TW_OverrideTradersRemarks, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryInstruction.TW_TradersRemarks, NUnit.Framework.Is.EqualTo(ZString.Empty));
			declaration.JE_OH_Importer = testOrg.PK;
			NUnit.Framework.Assert.That(!entryInstruction.TW_OverrideTradersRemarks, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryInstruction.TW_TradersRemarks, NUnit.Framework.Is.EqualTo("常年(長期)委任報關核准文號：236547\r\n起：109年07月17日\r\n迄：114年05月15日").Using(CustomComparers.TypeComparison));
			entryInstruction.TW_TradersRemarks += " Test 1234";
			NUnit.Framework.Assert.That(entryInstruction.TW_OverrideTradersRemarks, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryInstruction.TW_TradersRemarks, NUnit.Framework.Is.EqualTo("常年(長期)委任報關核准文號：236547\r\n起：109年07月17日\r\n迄：114年05月15日 Test 1234").Using(CustomComparers.TypeComparison));
			entryInstruction.TW_OverrideTradersRemarks = false;
			NUnit.Framework.Assert.That(!entryInstruction.TW_OverrideTradersRemarks, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryInstruction.TW_TradersRemarks, NUnit.Framework.Is.EqualTo("常年(長期)委任報關核准文號：236547\r\n起：109年07月17日\r\n迄：114年05月15日").Using(CustomComparers.TypeComparison));
		}

		[TestDate(2020, 05, 15)]
		[ExpectNoExceptions]
		public void TestTW_TradersRemarks_RefreshAutomatically()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = testOrg.Addresses.AddNew();
			var address2 = testOrg.Addresses.AddNew();
			var warehouseOrg = Factory.NewWithValidTestData<OrgHeader>();
			var warehouseMainAddress = warehouseOrg.MainAddress;
			warehouseMainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CusEntryInstruction;

			var tradersRemarksRefreshBindinged = false;
			entryInstruction.TW_TradersRemarksInfo.ValueChanged += (sender, e) =>
			{
				tradersRemarksRefreshBindinged = true;
			};
			CombineAssertions(() =>
			{
				tradersRemarksRefreshBindinged = false;
				entryInstruction.CEI_OA_Warehouse = warehouseMainAddress.PK;
				NUnit.Framework.Assert.That(tradersRemarksRefreshBindinged, NUnit.Framework.Is.True, "TradersRemarks should be refresh after changing CusEntryInstruction CEI_OA_Warehouse");

				tradersRemarksRefreshBindinged = false;
				declaration.JE_OH_Supplier = testOrg.PK;
				NUnit.Framework.Assert.That(tradersRemarksRefreshBindinged, NUnit.Framework.Is.True, "TradersRemarks should be refresh after changing declaration supplier");

				declaration.SupplierDocumentaryAddress.E2_OA_Address = address1.PK;
				tradersRemarksRefreshBindinged = false;
				declaration.SupplierDocumentaryAddress.E2_OA_Address = address2.PK;
				NUnit.Framework.Assert.That(tradersRemarksRefreshBindinged, NUnit.Framework.Is.True, "TradersRemarks should be refresh after changing declaration supplier address");

				tradersRemarksRefreshBindinged = false;
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(tradersRemarksRefreshBindinged, NUnit.Framework.Is.True, "TradersRemarks should be refresh after changing declaration JE_MessageType");

				tradersRemarksRefreshBindinged = false;
				entryInstruction.CEI_OA_Warehouse2 = warehouseMainAddress.PK;
				NUnit.Framework.Assert.That(tradersRemarksRefreshBindinged, NUnit.Framework.Is.True, "TradersRemarks should be refresh after changing CusEntryInstruction CEI_OA_Warehouse2");

				tradersRemarksRefreshBindinged = false;
				declaration.JE_OH_Importer = testOrg.PK;
				NUnit.Framework.Assert.That(tradersRemarksRefreshBindinged, NUnit.Framework.Is.True, "TradersRemarks should be refresh after changing declaration importer");

				declaration.ImporterDocumentaryAddress.E2_OA_Address = address1.PK;
				tradersRemarksRefreshBindinged = false;
				declaration.ImporterDocumentaryAddress.E2_OA_Address = address2.PK;
				NUnit.Framework.Assert.That(tradersRemarksRefreshBindinged, NUnit.Framework.Is.True, "TradersRemarks should be refresh after changing declaration importer address");

				tradersRemarksRefreshBindinged = false;
				entryInstruction.CEI_CustomsOffice = "A";
				NUnit.Framework.Assert.That(tradersRemarksRefreshBindinged, NUnit.Framework.Is.True, "TradersRemarks should be refresh after changing CusEntryInstruction CEI_CustomsOffice");

				tradersRemarksRefreshBindinged = false;
				entryInstruction.CEI_BoxNumber = "999";
				NUnit.Framework.Assert.That(tradersRemarksRefreshBindinged, NUnit.Framework.Is.True, "TradersRemarks should be refresh after changing CusEntryInstruction CEI_BoxNumber");

				tradersRemarksRefreshBindinged = false;
				entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 5, 4);
				NUnit.Framework.Assert.That(tradersRemarksRefreshBindinged, NUnit.Framework.Is.True, "TradersRemarks should be refresh after changing CusEntryInstruction CEI_DateForDuty");

				tradersRemarksRefreshBindinged = false;
				entryInstruction.TW_OverrideTradersRemarks = true;
				NUnit.Framework.Assert.That(tradersRemarksRefreshBindinged, NUnit.Framework.Is.True, "TradersRemarks should be refresh after changing CusEntryInstruction TW_OverrideTradersRemarks");

				entryInstruction.Delete();

				tradersRemarksRefreshBindinged = false;
				NUnit.Framework.Assert.That(entryInstruction.IsDeleted, NUnit.Framework.Is.True, "checking entryInstruction.IsDeleted");

				declaration.JE_OH_Importer = ZGuid.Empty;
				NUnit.Framework.Assert.That(!tradersRemarksRefreshBindinged, NUnit.Framework.Is.True, "TradersRemarks should not be refresh when CusEntryInstruction deleted[declaration importer]");
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				NUnit.Framework.Assert.That(!tradersRemarksRefreshBindinged, NUnit.Framework.Is.True, "TradersRemarks should not be refresh when CusEntryInstruction deleted[declaration JE_MessageType]");
				declaration.JE_OH_Supplier = ZGuid.Empty;
				NUnit.Framework.Assert.That(!tradersRemarksRefreshBindinged, NUnit.Framework.Is.True, "TradersRemarks should not be refresh when CusEntryInstruction deleted[declaration supplier]");
			});
		}

		[TestDate(2020, 05, 15)]
		[ExpectNoExceptions]
		public void TestTW_TradersRemarks_Priority()
		{
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = testOrg1.MainAddress;
			address1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "99999", Core.Constants.CountryCodes.Taiwan);
			var poaDoc1 = testOrg1.RequiredDocuments.AddNew();
			poaDoc1.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			poaDoc1.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			poaDoc1.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			poaDoc1.EQ_DateReceived = new ZDateTimeOffset(2020, 7, 17);
			poaDoc1.EQ_ValidToDate = new ZDateTime(2025, 5, 15);
			poaDoc1.EQ_DocNumber = "189657";
			poaDoc1.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			poaDoc1.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			var poaCustomsDistrictAtt1 = poaDoc1.Attributes[JobRequiredDocAttribTypeList.Codes.CustomsDistrict];
			poaCustomsDistrictAtt1.D0_AttribValue = "C";
			var poaBoxNumberAtt1 = poaDoc1.Attributes[JobRequiredDocAttribTypeList.Codes.BoxNumber];
			poaBoxNumberAtt1.D0_AttribValue = "123";

			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			var address2 = testOrg2.MainAddress;
			address2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "99999", Core.Constants.CountryCodes.Taiwan);

			var poaDoc2 = testOrg2.RequiredDocuments.AddNew();
			poaDoc2.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			poaDoc2.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			poaDoc2.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			poaDoc2.EQ_DateReceived = new ZDateTimeOffset(2020, 7, 17);
			poaDoc2.EQ_ValidToDate = new ZDateTime(2025, 5, 15);
			poaDoc2.EQ_DocNumber = "236547";
			poaDoc2.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			poaDoc2.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			var poaCustomsDistrictAtt2 = poaDoc2.Attributes[JobRequiredDocAttribTypeList.Codes.CustomsDistrict];
			poaCustomsDistrictAtt2.D0_AttribValue = "C";
			var poaBoxNumberAtt2 = poaDoc2.Attributes[JobRequiredDocAttribTypeList.Codes.BoxNumber];
			poaBoxNumberAtt2.D0_AttribValue = "123";
			var poaBondedIDAtt2 = poaDoc2.Attributes[JobRequiredDocAttribTypeList.Codes.BondedID];
			poaBondedIDAtt2.D0_AttribValue = "99999";

			var testOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			var address3 = testOrg3.MainAddress;
			address3.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "883", Core.Constants.CountryCodes.Taiwan);
			var poaDoc3 = testOrg3.RequiredDocuments.AddNew();
			poaDoc3.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			poaDoc3.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			poaDoc3.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			poaDoc3.EQ_DateReceived = new ZDateTimeOffset(2020, 7, 17);
			poaDoc3.EQ_ValidToDate = new ZDateTime(2025, 5, 15);
			poaDoc3.EQ_DocNumber = "226549";
			poaDoc3.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			poaDoc3.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			var poaCustomsDistrictAtt3 = poaDoc3.Attributes[JobRequiredDocAttribTypeList.Codes.CustomsDistrict];
			poaCustomsDistrictAtt3.D0_AttribValue = "C";
			var poaBoxNumberAtt3 = poaDoc3.Attributes[JobRequiredDocAttribTypeList.Codes.BoxNumber];
			poaBoxNumberAtt3.D0_AttribValue = "123";
			var poaBondedIDAtt3 = poaDoc3.Attributes[JobRequiredDocAttribTypeList.Codes.BondedID];
			poaBondedIDAtt3.D0_AttribValue = "883";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D1;
			entryInstruction.CEI_CustomsOffice = "CA";
			entryInstruction.CEI_BoxNumber = "123";
			declaration.JE_OH_Supplier = testOrg1.PK;

			NUnit.Framework.Assert.That(entryInstruction.TW_TradersRemarks, NUnit.Framework.Is.EqualTo("常年(長期)委任報關核准文號：189657\r\n起：109年07月17日\r\n迄：114年05月15日").Using(CustomComparers.TypeComparison));

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D5;
			NUnit.Framework.Assert.That(entryInstruction.TW_TradersRemarks, NUnit.Framework.Is.EqualTo("常年(長期)委任報關核准文號：189657\r\n起：109年07月17日\r\n迄：114年05月15日").Using(CustomComparers.TypeComparison));

			declaration.JE_OH_Supplier = ZGuid.Empty;
			entryInstruction.CEI_OA_Warehouse = address2.PK;
			NUnit.Framework.Assert.That(entryInstruction.TW_TradersRemarks, NUnit.Framework.Is.EqualTo("常年(長期)委任報關核准文號：236547\r\n起：109年07月17日\r\n迄：114年05月15日").Using(CustomComparers.TypeComparison));

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.G3;
			NUnit.Framework.Assert.That(entryInstruction.TW_TradersRemarks, NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison));

			declaration.JE_OH_Supplier = testOrg3.PK;
			NUnit.Framework.Assert.That(entryInstruction.TW_TradersRemarks, NUnit.Framework.Is.EqualTo("常年(長期)委任報關核准文號：226549\r\n起：109年07月17日\r\n迄：114年05月15日").Using(CustomComparers.TypeComparison));

			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G7;
			entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
			declaration.JE_OH_Importer = testOrg1.PK;

			NUnit.Framework.Assert.That(entryInstruction.TW_TradersRemarks, NUnit.Framework.Is.EqualTo("常年(長期)委任報關核准文號：189657\r\n起：109年07月17日\r\n迄：114年05月15日").Using(CustomComparers.TypeComparison));

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D2;
			NUnit.Framework.Assert.That(entryInstruction.TW_TradersRemarks, NUnit.Framework.Is.EqualTo("常年(長期)委任報關核准文號：189657\r\n起：109年07月17日\r\n迄：114年05月15日").Using(CustomComparers.TypeComparison));

			declaration.JE_OH_Importer = ZGuid.Empty;
			entryInstruction.CEI_OA_Warehouse = address2.PK;
			NUnit.Framework.Assert.That(entryInstruction.TW_TradersRemarks, NUnit.Framework.Is.EqualTo("常年(長期)委任報關核准文號：236547\r\n起：109年07月17日\r\n迄：114年05月15日").Using(CustomComparers.TypeComparison));

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D7;
			NUnit.Framework.Assert.That(entryInstruction.TW_TradersRemarks, NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison));

			declaration.JE_OH_Importer = testOrg3.PK;
			NUnit.Framework.Assert.That(entryInstruction.TW_TradersRemarks, NUnit.Framework.Is.EqualTo("常年(長期)委任報關核准文號：226549\r\n起：109年07月17日\r\n迄：114年05月15日").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAttachedDocumentNumbersAsString()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryInstruction = declaration.CusEntryInstruction;
			NUnit.Framework.Assert.That(cusEntryInstruction.TW_AttachedDoc1, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(cusEntryInstruction.TW_AttachedDoc2, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(cusEntryInstruction.TW_AttachedDoc3, NUnit.Framework.Is.EqualTo(ZString.Empty));
			cusEntryInstruction.AttachedDocumentNumbersAsString = "中文1,B,中文2";
			NUnit.Framework.Assert.That(cusEntryInstruction.TW_AttachedDoc1, NUnit.Framework.Is.EqualTo("中文1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(cusEntryInstruction.TW_AttachedDoc2, NUnit.Framework.Is.EqualTo("B").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(cusEntryInstruction.TW_AttachedDoc3, NUnit.Framework.Is.EqualTo("中文2").Using(CustomComparers.TypeComparison));
			cusEntryInstruction.DocumentNumbers[1].Delete();
			NUnit.Framework.Assert.That(cusEntryInstruction.TW_AttachedDoc1, NUnit.Framework.Is.EqualTo("中文1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(cusEntryInstruction.TW_AttachedDoc2, NUnit.Framework.Is.EqualTo("中文2").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(cusEntryInstruction.TW_AttachedDoc3, NUnit.Framework.Is.EqualTo(ZString.Empty));
			cusEntryInstruction.DocumentNumbers.AddNew().CY_Data = "中文3";
			NUnit.Framework.Assert.That(cusEntryInstruction.TW_AttachedDoc1, NUnit.Framework.Is.EqualTo("中文1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(cusEntryInstruction.TW_AttachedDoc2, NUnit.Framework.Is.EqualTo("中文2").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(cusEntryInstruction.TW_AttachedDoc3, NUnit.Framework.Is.EqualTo("中文3").Using(CustomComparers.TypeComparison));
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var cusEntryInstruction2 = factory2.Load<CusEntryInstruction>(cusEntryInstruction.PK);
			NUnit.Framework.Assert.That(cusEntryInstruction2.AttachedDocumentNumbersAsString, NUnit.Framework.Is.EqualTo("中文1,中文2,中文3").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(cusEntryInstruction2.TW_AttachedDoc1, NUnit.Framework.Is.EqualTo("中文1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(cusEntryInstruction2.TW_AttachedDoc2, NUnit.Framework.Is.EqualTo("中文2").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(cusEntryInstruction2.TW_AttachedDoc3, NUnit.Framework.Is.EqualTo("中文3").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTW_BillOfMaterials()
		{
			var cusEntryInstruction = Factory.New<CusEntryInstruction>();
			cusEntryInstruction.CEI_BillOfMaterials = true;
			cusEntryInstruction.CEI_BOMPageCount = 5;
			NUnit.Framework.Assert.That(cusEntryInstruction.CEI_BOMPageCount, NUnit.Framework.Is.EqualTo(5).Using(CustomComparers.TypeComparison));
			cusEntryInstruction.CEI_BillOfMaterials = false;
			NUnit.Framework.Assert.That(cusEntryInstruction.CEI_BOMPageCount, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison));
		}

		protected override RefCusProcedure CreateRefCusProcedure()
		{
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "40";
			procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			procedure.ZZ6_Description = "DESCRIPTION";
			procedure.ZZ6_ShipmentType = "IMP";
			return procedure;
		}
	}
}
