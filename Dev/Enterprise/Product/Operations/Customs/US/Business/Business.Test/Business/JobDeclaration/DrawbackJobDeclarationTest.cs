using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DrawbackJobDeclarationTest : TestCaseWithFactory
	{
		public void TestGoodsDescriptionMaxlength()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("Goods description max length is 35 for a drawback", 35, declaration.JE_GoodsDescriptionInfo.MaxLength);
			AssertNotEquals(JobDeclaration.Schema.JE_GoodsDescriptionMaxLength, declaration.JE_GoodsDescriptionInfo.MaxLength);

			declaration.JE_GoodsDescription = "12345678901234567890123456789012345";
			AssertEquals("12345678901234567890123456789012345", declaration.JE_GoodsDescription);

			declaration.JE_GoodsDescription = "123456789012345678901234567890123456";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(JobDeclaration.Schema.JE_GoodsDescriptionMaxLength, declaration.JE_GoodsDescriptionInfo.MaxLength);

			declaration.JE_GoodsDescription = "1234567890123456789012345678901234567890";
			AssertEquals("1234567890123456789012345678901234567890", declaration.JE_GoodsDescription);
		}

		public void TestIsNotRejectedMerchandiseDrawback()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			drawback.US_EntryType = EntryTypeList.Codes.RejectedMerchandiseDrawback;
			drawback.US_DRWRejectedMerchandiseReason = DrawbackRejectedMerchandiseReasonList.Codes.SWC;
			Assert(!drawback.IsNotRejectedMerchandiseDrawback);
			AssertEquals(DrawbackRejectedMerchandiseReasonList.Codes.SWC, drawback.US_DRWRejectedMerchandiseReason);
			drawback.US_EntryType = EntryTypeList.Codes.SubstitutionUnusedMerchandiseDrawback;
			Assert(drawback.IsNotRejectedMerchandiseDrawback);
			AssertEquals(ZString.Empty, drawback.US_DRWRejectedMerchandiseReason);

			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			drawback.US_EntryType = ACEDrawbackProvisionsList.Codes._01;
			Assert(drawback.IsNotRejectedMerchandiseDrawback);
			drawback.US_EntryType = ACEDrawbackProvisionsList.Codes._03;
			Assert(!drawback.IsNotRejectedMerchandiseDrawback);
			drawback.US_EntryType = ACEDrawbackProvisionsList.Codes._04;
			Assert(!drawback.IsNotRejectedMerchandiseDrawback);
			drawback.US_EntryType = ACEDrawbackProvisionsList.Codes._05;
			Assert(!drawback.IsNotRejectedMerchandiseDrawback);
			drawback.US_EntryType = ACEDrawbackProvisionsList.Codes._06;
			Assert(!drawback.IsNotRejectedMerchandiseDrawback);
		}

		public void TestIsHMF_MPFClaimable()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			Declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CM;
			Assert(!Declaration.IsHMF_MPFClaimable);
			Declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			Declaration.US_EntryType = EntryTypeList.Codes.SubstitutionUnusedMerchandiseDrawback;
			Assert(Declaration.IsHMF_MPFClaimable);
			Declaration.US_EntryType = EntryTypeList.Codes.DirectIdentificationUnusedMerchandiseDrawback;
			Assert(Declaration.IsHMF_MPFClaimable);
			Declaration.US_EntryType = EntryTypeList.Codes.SubstitutionManufacturerDrawback;
			Assert(!Declaration.IsHMF_MPFClaimable);

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CM;
			Assert(!Declaration.IsHMF_MPFClaimable);

			Declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			Declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._08;
			Assert(Declaration.IsHMF_MPFClaimable);
			Declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._09;
			Assert(Declaration.IsHMF_MPFClaimable);
		}

		public void TestTotalDrawbackClaimAmount()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.US_DRWIsForImportSection = true;
			invoiceLine1.US_DRWClaimAmountOverriden_New = true;
			invoiceLine1.US_DRWWeightedRatio = 1m;
			invoiceLine1.US_DRWMPFWeightedRatio = 1m;
			invoiceLine1.DRWImportQuantity = 1m;
			invoiceLine1.DRWExportQuantity = 1m;

			invoiceLine1.DeclaredVFD = 12.34m;
			invoiceLine1.DeclaredTax = 1m;
			invoiceLine1.DeclaredMPF = 10m;
			invoiceLine1.DeclaredHMF = 20m;
			invoiceLine1.DrawbackOtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.OilSpillTax, 21m);
			invoiceLine1.DrawbackOtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.DomesticTax, 23m);
			invoiceLine1.DrawbackOtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.PRDrawbackDuty, 25m);
			invoiceLine1.DrawbackOtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.DrawbackSuperfundTax, 26m);
			invoiceLine1.DrawbackOtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.CottonFee, 27m);
			invoiceLine1.DeclaredOtherFees = 30m;
			invoiceLine1.LineDuty = invoiceLine1.Claims.DutyClaim.DeclaredAmount * 0.5m;

			invoiceLine2.US_DRWIsForImportSection = true;
			invoiceLine2.US_DRWClaimAmountOverriden_New = true;
			invoiceLine2.US_DRWWeightedRatio = 1m;
			invoiceLine2.US_DRWMPFWeightedRatio = 1m;
			invoiceLine2.DRWImportQuantity = 1m;
			invoiceLine2.DRWExportQuantity = 1m;
			invoiceLine2.DeclaredVFD = 2m;
			invoiceLine2.DeclaredTax = 0.10m;
			invoiceLine2.DeclaredMPF = 100m;
			invoiceLine2.DeclaredHMF = 200m;
			invoiceLine2.DrawbackOtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.OilSpillTax, 210m);
			invoiceLine2.DrawbackOtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.DomesticTax, 230m);
			invoiceLine2.DrawbackOtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.PRDrawbackDuty, 250m);
			invoiceLine2.DrawbackOtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.DrawbackSuperfundTax, 23m);
			invoiceLine2.DrawbackOtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.MangoFee, 11m);
			invoiceLine2.DeclaredOtherFees = 300m;
			invoiceLine2.LineDuty = invoiceLine2.Claims.DutyClaim.DeclaredAmount * 0.5m;

			invoiceLine3.US_DRWIsForImportSection = true;
			invoiceLine3.US_DRWClaimAmountOverriden_New = true;
			invoiceLine3.US_DRWWeightedRatio = 1m;
			invoiceLine3.US_DRWMPFWeightedRatio = 1m;
			invoiceLine3.DRWImportQuantity = 1m;
			invoiceLine3.DRWExportQuantity = 1m;
			invoiceLine3.DeclaredVFD = 0.20m;
			invoiceLine3.DeclaredTax = 4m;
			invoiceLine3.DeclaredMPF = 1000m;
			invoiceLine3.DeclaredHMF = 2000m;
			invoiceLine3.DeclaredOtherFees = 3000m;
			invoiceLine3.AdjClaimMPF = 20m;
			invoiceLine3.AdjClaimHMF = 40m;
			invoiceLine3.AdjClaimTax = 1.5m;
			invoiceLine3.AdjClaimDuty = 3m;
			invoiceLine3.LineDuty = invoiceLine3.Claims.DutyClaim.DeclaredAmount * 0.5m;

			CombineAssertions(() =>
			{
				AssertEquals("Total Drawback Claim Duty", 7.18m, Declaration.TotalDutyClaimAmount);
				AssertEquals("Total Drawback Claim Tax", 5.04m, Declaration.TotalTaxClaimAmount);
				AssertEquals("Total Drawback Claim MPF", 1098.9m, Declaration.TotalMPFClaimAmount);
				AssertEquals("Total Drawback Claim HMF", 2197.8m, Declaration.TotalHMFClaimAmount);
				AssertEquals("Total Drawback Claim Purtro Rico", 0m, Declaration.US_DRWTotalPRDC);
				AssertEquals("Total Drawback Claim Other fees", 3296.7m, Declaration.TotalOtherClaimAmount);
				AssertEquals("Total Drawback Claim", 6605.62m, Declaration.TotalClaimAmount);
			});

			Declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CD;
			CombineAssertions(() =>
			{
				AssertEquals("Total Drawback Claim Duty", 7.27m, Declaration.TotalDutyClaimAmount);
				AssertEquals("Total Drawback Claim Tax", 5.1m, Declaration.TotalTaxClaimAmount);
				AssertEquals("Total Drawback Claim MPF", 1110m, Declaration.TotalMPFClaimAmount);
				AssertEquals("Total Drawback Claim HMF", 2220m, Declaration.TotalHMFClaimAmount);
				AssertEquals("Total Drawback Claim Purtro Rico", 0m, Declaration.US_DRWTotalPRDC);
				AssertEquals("Total Drawback Claim Other fees", 3330m, Declaration.TotalOtherClaimAmount);
				AssertEquals("Total Drawback Claim", 6672.37m, Declaration.TotalClaimAmount);
			});

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_DRWPurpose = ZString.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("Total Drawback Claim Duty", 7.18m, Declaration.TotalDutyClaimAmount);
				AssertEquals("Total Drawback Claim Tax", 5.04m, Declaration.TotalTaxClaimAmount);
				AssertEquals("Total Drawback Claim MPF", 1098.9m, Declaration.TotalMPFClaimAmount);
				AssertEquals("Total Drawback Claim HMF", 2197.8m, Declaration.TotalHMFClaimAmount);
				AssertEquals("Total Drawback Claim Purtro Rico", 272.25m, Declaration.US_DRWTotalPRDC);
				AssertEquals("Total Drawback Claim Other fees", 565.29m, Declaration.TotalOtherClaimAmount);
				AssertEquals("Total Drawback Claim", 4146.46m, Declaration.TotalClaimAmount);

				AssertEquals("Total Drawback Claim Duty", 3m, Declaration.TotalAdjDutyClaimAmount);
				AssertEquals("Total Drawback Claim Tax", 1.5m, Declaration.TotalAdjTaxClaimAmount);
				AssertEquals("Total Drawback Claim MPF", 40m, Declaration.TotalAdjHMFClaimAmount);
				AssertEquals("Total Drawback Claim HMF", 20m, Declaration.TotalAdjMPFClaimAmount);
			});
		}

		public void TestSettingClaimPortSetsTeamNo()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;

			Assert("pre-condition", Declaration.US_TeamNo.IsEmpty);
			Declaration.US_ClaimPort = "9999";
			AssertEquals("Invalid port should not set team no", "", Declaration.US_TeamNo);
			Declaration.US_ClaimPort = "1001";
			AssertEquals("TeamNo is now set", "2DB", Declaration.US_TeamNo);
			Declaration.US_ClaimPort = "3901";
			AssertEquals("TeamNo should not change", "2DB", Declaration.US_TeamNo);
		}

		public void TestSettingClaimPortSetsProcessingPort()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertEquals(ZString.Empty, Declaration.US_ClaimPort);
			AssertEquals(ZString.Empty, Declaration.US_PreparerDistrictPort);

			Declaration.US_ClaimPort = "~";
			AssertEquals("~", Declaration.US_ClaimPort);
			AssertEquals(ZString.Empty, Declaration.US_PreparerDistrictPort);

			Declaration.US_ClaimPort = "1001";
			AssertEquals("1001", Declaration.US_ClaimPort);
			AssertEquals("1001", Declaration.US_PreparerDistrictPort);
		}

		public void TestOnSavingAndDeclarationNumber()
		{
			var companyStmNums = DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
			companyStmNums.SetNextNumber(8976546);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			AssertEquals("No DeclarationNumber", "", declaration.DeclarationNumber);
			Factory.Save();
			AssertEquals("No DeclarationNumber", "", declaration.DeclarationNumber);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.SetDefaultValuesForDrawback();
			AssertEquals("No DeclarationNumber", "", declaration.DeclarationNumber);
			var nextNumber = companyStmNums.GenerateCustomsNumber(8976546);
			Factory.Save();
			AssertEquals("Merge by changed", OrgConstants.MergeInvoiceLines.NotMerge, Declaration.JE_MergeBy);
			Assert("DeclarationNumber", !declaration.DeclarationNumber.IsEmpty);
			AssertEquals("DeclarationNumber", nextNumber, declaration.DeclarationNumber);
			AssertEquals("US_DRWFilingMethod", "A", declaration.US_DRWFilingMethod);
			companyStmNums.SetNextNumber(companyStmNums.SN_MaximumValue);
			companyStmNums.TryGetNumberFountain().GetNext(Factory);

			var companyMessage = string.Format(ACEEntryStmNumsSetting.NotEnoughAvailableEntryNumbersForCompany, GlbCompany.CurrentCompany.GC_Code, "XJ5");
			AssertEquals("PreCondition", companyMessage, Declaration.DisallowAllocateImportEntryNumber);
			Declaration.DeclarationNumber = ZString.Empty;
			Factory.Save();
			AssertEquals("Should not have generated another dec number", ZString.Empty, Declaration.DeclarationNumber);
		}

		public void TestDefaultEmptyJE_RS_NKServiceLevel()
		{
			AssertEquals("Declaration initially defaults JE_RS_NKServiceLevel", Env.Registry.ServiceLevel, Declaration.ServiceLevel.PK);

			Declaration.SetDefaultValuesForDrawback();
			AssertEquals("No Default value on TransportMode", "", Declaration.JE_TransportMode);
			AssertEquals("JE_RS_NKServiceLevel defaulted to empty for Drawback", ZString.Empty, Declaration.JE_RS_NKServiceLevel);
		}

		public void TestDrawbackIsNonTransportDeclarationType()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert("Export is TransportDeclarationType", !Declaration.IsNonTransportDeclarationType);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("Import is TransportDeclarationType", !Declaration.IsNonTransportDeclarationType);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Assert("Drawback is NonTransportDeclarationType", Declaration.IsNonTransportDeclarationType);
		}

		public void TestDrawbackMessageAttachees()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			IMessageActionHeader actionHeader = declaration;
			int j = 0;
			foreach (IMessageAttacheeInDeclaration messageAttachee in actionHeader.MessageAttachees)
			{
				j++;
				AssertEquals("Is JobDeclaration", typeof(JobDeclaration), messageAttachee.GetType());
			}
			AssertEquals("only 1 attachee", 1, j);
		}

		public void TestUS_ClaimPortName()
		{
			AssertEquals("[PRE-CONDITION] US_ClaimPortName.IsEmpty?", true, Declaration.US_ClaimPortName.IsEmpty);
			Declaration.US_ClaimPort = "2809";
			AssertEquals("US_ClaimPortName", "San Francisco", Declaration.US_ClaimPortName);
			Declaration.US_ClaimPort = "3901";
			AssertEquals("US_ClaimPortName", "Chicago", Declaration.US_ClaimPortName);
		}

		public void TestUS_PreparerDistrictPortName()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1704", "ATLANTA, GA", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3303", "SALT LAKE CITY, UT", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			AssertEquals("[PRE-CONDITION] US_PreparerDistrictPortName.IsEmpty?", true, Declaration.US_PreparerDistrictPortName.IsEmpty);
			Declaration.US_PreparerDistrictPort = "1704";
			AssertEquals("US_PreparerDistrictPortName", "ATLANTA, GA", Declaration.US_PreparerDistrictPortName);
			Declaration.US_PreparerDistrictPort = "3303";
			AssertEquals("US_PreparerDistrictPortName", "SALT LAKE CITY, UT", Declaration.US_PreparerDistrictPortName);
		}

		public void TestUS_ConcatenatedContractList()
		{
			AssertEquals("[PRE-CONDITION] US_ConcatenatedContracts.IsEmpty?", true, Declaration.US_ConcatenatedContracts.IsEmpty);

			var contract1 = Factory.New<ContractNumber>();
			contract1.CY_Data = "CT1";
			Declaration.ContractNumbers.Add(contract1);
			AssertEquals("US_ConcatenatedContracts", "CT1", Declaration.US_ConcatenatedContracts);

			var contract2 = Factory.New<ContractNumber>();
			contract2.CY_Data = "CT2";
			Declaration.ContractNumbers.Add(contract2);
			AssertEquals("US_ConcatenatedContracts", "CT1, CT2", Declaration.US_ConcatenatedContracts);
		}

		public void TestIs7552()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Not a drawback", false, Declaration.Is7552);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			AssertEquals("Not a 7552", false, Declaration.Is7552);
			Declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CD;
			AssertEquals("Not a 7552", true, Declaration.Is7552);
			Declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CM;
			AssertEquals("Not a 7552", true, Declaration.Is7552);
		}

		[TestDate(2016, 9, 22)]
		public void TestUS_DRWPurpose()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("Default to DRW", DrawbackDeclarationPurposeList.Codes.DRW, Declaration.US_DRWPurpose);
		}

		public void TestUS_DRWIsForImportSection()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			AssertEquals("For 7551", true, invoiceLine.US_DRWIsForImportSection);

			invoiceLine.US_DRWIsForImportSection = false;
			AssertEquals("For 7551", false, invoiceLine.US_DRWIsForImportSection);

			Declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CD;
			AssertEquals("For 7552, it's alway true", true, invoiceLine.US_DRWIsForImportSection);
		}

		public void TestIsManufacturingDrawbackSupported()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			Assert(Declaration.IsManufacturingDrawbackSupported);

			Declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CM;
			Assert(Declaration.IsManufacturingDrawbackSupported);

			Declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CD;
			Assert(!Declaration.IsManufacturingDrawbackSupported);
		}

		public void TestCustomsLastEntryStatusDate()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var firstDate = new ZDateTime(2022, 10, 22);
			var lastDate = new ZDateTime(2022, 11, 22);

			Declaration.Logs.AddNew(Events.CustomsEntryStatus, firstDate.ToOffset());
			AssertEquals("CustomsLastEntryStatusDate", firstDate, Declaration.CustomsLastEntryStatusDate);

			Declaration.Logs.AddNew(Events.CustomsEntryStatus, lastDate.ToOffset());
			AssertEquals("CustomsLastEntryStatusDate", lastDate, Declaration.CustomsLastEntryStatusDate);
		}

		public void TestIsACEDrawback()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertEquals(false, Declaration.IsACEDrawback);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertEquals(true, Declaration.IsACEDrawback);

			Declaration.JE_ApplicationCode = ZString.Empty;
			AssertEquals(false, Declaration.IsACEDrawback);
		}

		public void TestWorkflowTemplateMilestoneForDrawbackDeclaration()
		{
			var drawbackTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			drawbackTemplate.P0_ProcessType = JobMessageTypeList.Codes.Drawback;
			drawbackTemplate.P0_SubType2 = JobMessageTypeList.Codes.Drawback;

			var task = drawbackTemplate.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "TEST TASK 33";
			var milestone = drawbackTemplate.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "TEST MILESTONE 33";
			Factory.Save();

			var job = Factory.New<JobDeclaration>();
			job.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Factory.Save();

			var matchedTask = job.WorkflowItems.Tasks.OfType<ProcessTask>().FirstOrDefault(t => t.P9_Description == "TEST TASK 33");
			var matchedMilestone = job.WorkflowItems.Milestones.OfType<ProcessTask>().FirstOrDefault(t => t.P9_Description == "TEST MILESTONE 33");
			AssertNotNull(matchedTask);
			AssertNotNull(matchedMilestone);
		}

		public void TestWorkflowTemplateCustomFieldForDrawbackDeclaration()
		{
			var drawbackTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			drawbackTemplate.P0_ProcessType = "DRW";

			var drawbackColumn = drawbackTemplate.GenCustomColumnDefinitions.AddNew();
			drawbackColumn.XC_Name = "Drawback Field 1";
			drawbackColumn.XC_Type = MasterFiles.Business.CustomValues.AddOnColumnDataType.Codes.String;

			Factory.Save();

			var job = Factory.New<JobDeclaration>();
			job.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var customBusinessObject = ((ICustomFieldProvider)job).GetCustomBusinessObject() as IDynamicBusinessObject;
			AssertContainsExactElementsInAnyOrder(new[] { "__DRAWBACK FIELD 1__prop__ZString", "__DRAWBACK FIELD 1__prop__ZStringInfo" }, customBusinessObject.PropertyNames);
		}

		public void TestJE_RS_NKServiceLevelNotSupportedForDrawback()
		{
			AssertEquals(true, Declaration.IsPropertySupported(Declaration.JE_RS_NKServiceLevelInfo));
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals(false, Declaration.IsPropertySupported(Declaration.JE_RS_NKServiceLevelInfo));
		}

		public void TestSuspendOtherFeeCalculationWhenAPTicked()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var importDec = Factory.New<JobDeclaration>();
			importDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var importInvoice = importDec.Invoices.AddNew();
			importInvoice.JZ_InvoiceNumber = "INV1234";

			var importInvoiceLine = importInvoice.JobComInvoiceLines.AddNew();
			importInvoiceLine.JI_CustomsUnitQty = "NO";
			importInvoiceLine.JI_CustomsQuantity = 2000m;
			importInvoiceLine.US_PayableMPF = 100m;
			importInvoiceLine.US_CustomsValue = 1000m;

			var importEntryHeader = importDec.CustomsEntryHeaders.AddNew();
			importEntryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			importEntryHeader.EntryNumber = "12345678";
			importEntryHeader.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 100m);
			importEntryHeader.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.HMF, 400m);

			var importEntryLine = importEntryHeader.MergedLines.AddNew();
			importEntryLine.CL_LineNumber = 1;
			importEntryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 200m);
			importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 100m);
			importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.HMF, 400m);
			importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Cotton, 500m);
			importEntryLine.CL_CustomsValue = 1000m;
			importInvoiceLine.JI_CL = importEntryLine.PK;
			Factory.Save();

			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			drawback.US_EntryType = ACEDrawbackProvisionsList.Codes._58;
			drawback.US_AcceleratedClaimInd = true;

			var drawbackInvoice = drawback.Invoices.AddNew();
			var drawbackInvoiceLine = drawbackInvoice.InvoiceLines.AddNew();
			drawbackInvoiceLine.US_DRWIsForImportSection = true;
			drawbackInvoiceLine.US_ImportEntryNo = "XJ512345678-1";
			drawbackInvoiceLine.DRWExportQuantity = 500m;
			CombineAssertions("Acc Payment Indicator ticked", () =>
			{
				AssertEquals("drawbackInvoiceLine.DeclaredMPF", 100m, drawbackInvoiceLine.DeclaredMPF);
				AssertEquals("drawbackInvoiceLine.CalculatedMPF", 24.75m, drawbackInvoiceLine.CalculatedMPF);
				AssertEquals("drawbackInvoiceLine.DeclaredHMF", 400m, drawbackInvoiceLine.DeclaredHMF);
				AssertEquals("drawbackInvoiceLine.CalculatedHMF", 99m, drawbackInvoiceLine.CalculatedHMF);
				AssertEquals("drawbackInvoiceLine.DrawbackOtherFees.Count", 1, drawbackInvoiceLine.DrawbackOtherFees.Count);
				var cottonFee = drawbackInvoiceLine.DrawbackOtherFees[0];
				AssertEquals("cottonFee.US_FeeType", "056", cottonFee.US_FeeType);
				AssertEquals("cottonFee.DeclaredAmount", 0m, cottonFee.DeclaredAmount);
				AssertEquals("cottonFee.CalculatedAmount", 0m, cottonFee.CalculatedAmount);
			});

			drawback.US_AcceleratedClaimInd = false;
			CombineAssertions("Acc Payment Indicator unticked", () =>
			{
				AssertEquals("drawbackInvoiceLine.DeclaredMPF", 100m, drawbackInvoiceLine.DeclaredMPF);
				AssertEquals("drawbackInvoiceLine.CalculatedMPF", 24.75m, drawbackInvoiceLine.CalculatedMPF);
				AssertEquals("drawbackInvoiceLine.DeclaredHMF", 400m, drawbackInvoiceLine.DeclaredHMF);
				AssertEquals("drawbackInvoiceLine.CalculatedHMF", 99m, drawbackInvoiceLine.CalculatedHMF);
				AssertEquals("drawbackInvoiceLine.DrawbackOtherFees.Count", 1, drawbackInvoiceLine.DrawbackOtherFees.Count);
				var cottonFee = drawbackInvoiceLine.DrawbackOtherFees[0];
				AssertEquals("cottonFee.US_FeeType", "056", cottonFee.US_FeeType);
				AssertEquals("cottonFee.DeclaredAmount", 500m, cottonFee.DeclaredAmount);
				AssertEquals("cottonFee.CalculatedAmount", 123.75m, cottonFee.CalculatedAmount);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
	}
}
