using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Customs.TW.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusEntryLine))] 
	sealed class CusEntryLineTest : CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
	{
		[ExpectNoExceptions]
		public override void TestDescriptionWhenMergedByClassification()
		{
			CombineAssertions(() =>
			{
				const string tarrifCode = "0000.00.00.00Y";
				var classification = Factory.New<CusClassification>();

				classification.CC_LookupCode = "CKSQKS";

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var line1 = invoice.JobComInvoiceLines.AddNew();
				line1.JI_Tariff = tarrifCode;
				line1.JI_CC = classification.PK;
				line1.JI_CEI = instruction.PK;
				NUnit.Framework.Assert.That(line1.JI_Description, NUnit.Framework.Is.EqualTo(ZString.Empty), "Line1 Description should have not defaulted from classification");

				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
				var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
				NUnit.Framework.Assert.That(entryLine.Description, NUnit.Framework.Is.EqualTo(ZString.Empty), "Description should be empty when merge by " + declaration.JE_MergeBy);
			});
		}

		[ExpectNoExceptions]
		public void TestGetPropertiesFromFirstInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var assignedNumber1 = invoiceLine.AssignedJobComInvLineRefsCollection.AddNew();
			assignedNumber1.JG_ReferenceNumber = "R1";
			var assignedNumber2 = invoiceLine.AssignedJobComInvLineRefsCollection.AddNew();
			assignedNumber2.JG_ReferenceNumber = "R2";
			invoiceLine.JI_Model = "model test";
			invoiceLine.JI_BrandName = "brand test";
			var permit1 = invoiceLine.PermitCusSupportingCollection.AddNew();
			permit1.CSI_LineNo = 1;
			permit1.CSI_ReferenceNumber = "P1";
			var permit2 = invoiceLine.PermitCusSupportingCollection.AddNew();
			permit2.CSI_LineNo = 2;
			permit2.CSI_ReferenceNumber = "P2";
			invoiceLine.JI_HazMatCode = "XX01";
			invoiceLine.JI_Compositions = "CX01";
			invoiceLine.JI_CustomsSupplierPartNo = "CSP91";
			invoiceLine.JI_CustomsOwnerPartNo = "COP99";
			invoiceLine.JI_CountryOfOrigin = "TW";
			invoiceLine.CertificateOfOriginNumber = "CER01";
			invoiceLine.CertificateOfOriginNumberItemNumber = 3;
			invoiceLine.JI_PreviousEntryNumber = "PE01";
			invoiceLine.JI_PreviousEntryLineNumber = 2;
			invoiceLine.PreviousBondedEntryNumber = "PBE01";
			invoiceLine.PreviousBondedEntryLineNumber = 5;
			invoiceLine.JI_NDescription = "中文測試.";
			invoiceLine.JI_Description = "description test.";
			invoiceLine.HighTechLicense = "htl1";
			invoiceLine.CitesPermit = "cp001";
			invoiceLine.JI_PrimaryPreference = "29";
			invoiceLine.JI_HasCatalystConverter = "1";
			invoiceLine.JI_EquipmentPrintMode = "2";
			invoiceLine.JI_CarType = "8";
			invoiceLine.JI_Transmission = "A";
			invoiceLine.JI_EngineType = "x0";
			invoiceLine.JI_LHD = "x";
			invoiceLine.JI_CarCondition = "c";
			invoiceLine.JI_ModelYear = 2016;
			invoiceLine.JI_Displacement = "2000";
			invoiceLine.JI_NumberOfDoor = 4;
			invoiceLine.JI_Seats = 7;
			invoiceLine.JI_Cylinders = 3;
			invoiceLine.JI_Gears = 8;

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			CombineAssertions(() =>
			{
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_AssignedNumberInfo, "Assigned Number");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_ModelInfo, "Model");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_BrandNameInfo, "Brand");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_PermitInfo, "Permit");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_DGCodeInfo, "DG Code");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_SpecificationInfo, "Specification");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_SupplierPartNumberInfo, "Supplier Part Number");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_CustomsOwnerPartNoInfo, "Owner Part Number");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_GoodsOriginInfo, "Goods Origin");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_CertificateOfOriginNumberInfo, "Certification of Origin");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_PreviousEntryNumberInfo, "Previous Entry Number");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_PreviousBondedEntryNumberInfo, "Previous Bounded Entry Number");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_ChineseDescriptionInfo, "Chinese Description");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_EnglishDescriptionInfo, "English Description");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_SHTCImportPermitInfo, "SHTC Import Permit");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_CITESImportPermitInfo, "CITES Import Permit");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_PreferenceInfo, "Preference");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_CatalyticConverterInfo, "Catalytic Converter?");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_StandardEquipmentInfo, "Standard Equipment");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_CarTypeInfo, "Car Type");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_TransmissionInfo, "Transmission");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_EngineTypeInfo, "Engine Type");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_LeftSideSteeringInfo, "Left Side Steering");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_CarConditionInfo, "Condition");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_ModelYearInfo, "Model Year");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_DisplacementInfo, "Displacement(cc)");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_NumberOfDoorInfo, "Number of Door");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_NumberOfSeatInfo, "Number of Seat");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_NumberOfCylinderInfo, "Number of Cylinder");
				BusinessObjectCaptionTestHelper.AssertCaptions(entryLine.CL_NumberOfGearInfo, "Number of Gear");

				NUnit.Framework.Assert.That(entryLine.CL_Model, NUnit.Framework.Is.EqualTo("model test").Using(CustomComparers.TypeComparison), "entryLine.CL_Model");
				NUnit.Framework.Assert.That(entryLine.CL_BrandName, NUnit.Framework.Is.EqualTo("brand test").Using(CustomComparers.TypeComparison), "entryLine.CL_BrandName");
				NUnit.Framework.Assert.That(entryLine.CL_AssignedNumber, NUnit.Framework.Is.EqualTo("R1|R2").Using(CustomComparers.TypeComparison), "entryLine.CL_AssignedNumber");
				NUnit.Framework.Assert.That(entryLine.CL_Permit, NUnit.Framework.Is.EqualTo("P1-1|P2-2").Using(CustomComparers.TypeComparison), "entryLine.CL_Permit");
				NUnit.Framework.Assert.That(entryLine.CL_DGCode, NUnit.Framework.Is.EqualTo("XX01").Using(CustomComparers.TypeComparison), "entryLine.CL_DGCode");
				NUnit.Framework.Assert.That(entryLine.CL_Specification, NUnit.Framework.Is.EqualTo("CX01").Using(CustomComparers.TypeComparison), "entryLine.CL_Specification");
				NUnit.Framework.Assert.That(entryLine.CL_SupplierPartNumber, NUnit.Framework.Is.EqualTo("CSP91").Using(CustomComparers.TypeComparison), "entryLine.CL_SupplierPartNumber");
				NUnit.Framework.Assert.That(entryLine.CL_CustomsOwnerPartNo, NUnit.Framework.Is.EqualTo("COP99").Using(CustomComparers.TypeComparison), "entryLine.CL_CustomsOwnerPartNo");
				NUnit.Framework.Assert.That(entryLine.CL_GoodsOrigin, NUnit.Framework.Is.EqualTo("TW").Using(CustomComparers.TypeComparison), "entryLine.CL_GoodsOrigin");
				NUnit.Framework.Assert.That(entryLine.CL_CertificateOfOriginNumber, NUnit.Framework.Is.EqualTo("CER01-3").Using(CustomComparers.TypeComparison), "entryLine.CL_CertificateOfOriginNumber");
				NUnit.Framework.Assert.That(entryLine.CL_PreviousEntryNumber, NUnit.Framework.Is.EqualTo("PE01-2").Using(CustomComparers.TypeComparison), "entryLine.CL_PreviousEntryNumber");
				NUnit.Framework.Assert.That(entryLine.CL_PreviousBondedEntryNumber, NUnit.Framework.Is.EqualTo("PBE01-5").Using(CustomComparers.TypeComparison), "entryLine.CL_PreviousBondedEntryNumber");
				NUnit.Framework.Assert.That(entryLine.CL_ChineseDescription, NUnit.Framework.Is.EqualTo("中文測試.").Using(CustomComparers.TypeComparison), "entryLine.CL_ChineseDescription");
				NUnit.Framework.Assert.That(entryLine.CL_EnglishDescription, NUnit.Framework.Is.EqualTo("description test.").Using(CustomComparers.TypeComparison), "entryLine.CL_EnglishDescription");
				NUnit.Framework.Assert.That(entryLine.CL_SHTCImportPermit, NUnit.Framework.Is.EqualTo("htl1").Using(CustomComparers.TypeComparison), "entryLine.CL_SHTCImportPermit");
				NUnit.Framework.Assert.That(entryLine.CL_CITESImportPermit, NUnit.Framework.Is.EqualTo("cp001").Using(CustomComparers.TypeComparison), "entryLine.CL_CITESImportPermit");
				NUnit.Framework.Assert.That(entryLine.CL_Preference, NUnit.Framework.Is.EqualTo("29").Using(CustomComparers.TypeComparison), "entryLine.CL_Preference");
				NUnit.Framework.Assert.That(entryLine.CL_CatalyticConverter, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison), "entryLine.CL_CatalyticConverter");
				NUnit.Framework.Assert.That(entryLine.CL_StandardEquipment, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison), "entryLine.CL_StandardEquipment");
				NUnit.Framework.Assert.That(entryLine.CL_CarType, NUnit.Framework.Is.EqualTo("8").Using(CustomComparers.TypeComparison), "entryLine.CL_CarType");
				NUnit.Framework.Assert.That(entryLine.CL_Transmission, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison), "entryLine.CL_Transmission");
				NUnit.Framework.Assert.That(entryLine.CL_EngineType, NUnit.Framework.Is.EqualTo("x0").Using(CustomComparers.TypeComparison), "entryLine.CL_EngineType");
				NUnit.Framework.Assert.That(entryLine.CL_LeftSideSteering, NUnit.Framework.Is.EqualTo("x").Using(CustomComparers.TypeComparison), "entryLine.CL_LeftSideSteering");
				NUnit.Framework.Assert.That(entryLine.CL_CarCondition, NUnit.Framework.Is.EqualTo("c").Using(CustomComparers.TypeComparison), "entryLine.CL_CarCondition");
				NUnit.Framework.Assert.That(entryLine.CL_ModelYear, NUnit.Framework.Is.EqualTo((ZShort)2016), "entryLine.CL_ModelYear");
				NUnit.Framework.Assert.That(entryLine.CL_Displacement, NUnit.Framework.Is.EqualTo("2000").Using(CustomComparers.TypeComparison), "entryLine.CL_Displacement");
				NUnit.Framework.Assert.That(entryLine.CL_NumberOfDoor, NUnit.Framework.Is.EqualTo((ZShort)4), "entryLine.CL_NumberOfDoor");
				NUnit.Framework.Assert.That(entryLine.CL_NumberOfSeat, NUnit.Framework.Is.EqualTo((ZShort)7), "entryLine.CL_NumberOfSeat");
				NUnit.Framework.Assert.That(entryLine.CL_NumberOfCylinder, NUnit.Framework.Is.EqualTo((ZShort)3), "entryLine.CL_NumberOfCylinder");
				NUnit.Framework.Assert.That(entryLine.CL_NumberOfGear, NUnit.Framework.Is.EqualTo((ZShort)8), "entryLine.CL_NumberOfGear");
			});
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_AdditionalDutyAmount()
		{
			AssertDutiesAndTaxesFeesAmount(203M, x => x.CL_Calc_AdditionalDutyAmount, UniversalReferenceConstants.RefCusRateCodes.ADT, 101M, 102M);
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_AdditionalDutyAmount_Caption()
		{
			var entryLine = Factory.New<CusEntryLine>();
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(entryLine.CL_Calc_AdditionalDutyAmountInfo, "Additional Duty Amount", "ADT Amt.");
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_AntiDumpingDutyAmount()
		{
			AssertDutiesAndTaxesFeesAmount(203M, x => x.CL_Calc_AntiDumpingDutyAmount, UniversalReferenceConstants.RefCusRateCodes.ADD, 101M, 102M);
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_AntiDumpingDutyAmount_Caption()
		{
			var entryLine = Factory.New<CusEntryLine>();
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(entryLine.CL_Calc_AntiDumpingDutyAmountInfo, "Anti-Dumping Duty Amount", "ADD Amt.");
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_BusinessTaxAmount()
		{
			AssertDutiesAndTaxesFeesAmount(203M, x => x.CL_Calc_BusinessTaxAmount, RefCusTaxOrFeeCodes.VAT, 101M, 102M);
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_BusinessTaxAmount_Caption()
		{
			var entryLine = Factory.New<CusEntryLine>();
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(entryLine.CL_Calc_BusinessTaxAmountInfo, "Business Tax", "VAT Amt.");
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_CommodityTaxCashAmount()
		{
			AssertDutiesAndTaxesFeesAmount(402M, x => x.CL_Calc_CommodityTaxCashAmount, UniversalReferenceConstants.RefCusRateCodes.CTA, UniversalReferenceConstants.RefCusRateCodes.CTS, 101M, 102M, 201M, 202M);
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_CommodityTaxCashAmount_Caption()
		{
			var entryLine = Factory.New<CusEntryLine>();
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(entryLine.CL_Calc_CommodityTaxCashAmountInfo, "Commodity Tax (CASH)", "CT Amt. (CASH)");
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_CommodityTaxNonCashAmount()
		{
			AssertDutiesAndTaxesFeesAmount(304M, x => x.CL_Calc_CommodityTaxNonCashAmount, UniversalReferenceConstants.RefCusRateCodes.CTA, UniversalReferenceConstants.RefCusRateCodes.CTS, 101M, 102M, 201M, 202M);
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_CommodityTaxNonCashAmount_Caption()
		{
			var entryLine = Factory.New<CusEntryLine>();
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(entryLine.CL_Calc_CommodityTaxNonCashAmountInfo, "Commodity Tax (NON-CASH)", "CT Amt. (NON-CASH)");
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_CountervailingDutyAmount()
		{
			AssertDutiesAndTaxesFeesAmount(203M, x => x.CL_Calc_CountervailingDutyAmount, UniversalReferenceConstants.RefCusRateCodes.CVD, 101M, 102M);
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_CountervailingDutyAmount_Caption()
		{
			var entryLine = Factory.New<CusEntryLine>();
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(entryLine.CL_Calc_CountervailingDutyAmountInfo, "Countervailing Duty", "CVD Amt.");
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_HealthAndWelfareSurchargeAmount()
		{
			AssertDutiesAndTaxesFeesAmount(203M, x => x.CL_Calc_HealthAndWelfareSurchargeAmount, UniversalReferenceConstants.RefCusRateCodes.HWS, 101M, 102M);
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_HealthAndWelfareSurchargeAmount_Caption()
		{
			var entryLine = Factory.New<CusEntryLine>();
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(entryLine.CL_Calc_HealthAndWelfareSurchargeAmountInfo, "Health and Welfare Surcharge", "HWS Amt.");
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_ImportDutyCashAmount()
		{
			AssertDutiesAndTaxesFeesAmount(302M, x => x.CL_Calc_ImportDutyCashAmount, UniversalReferenceConstants.RefCusRateCodes.DTA, UniversalReferenceConstants.RefCusRateCodes.DTS, 101M, 102M, 201M, 202M);
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_ImportDutyCashAmount_Caption()
		{
			var entryLine = Factory.New<CusEntryLine>();
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(entryLine.CL_Calc_ImportDutyCashAmountInfo, "Import Duty (CASH)", "DT Amt. (CASH)");
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_ImportDutyNonCashAmount()
		{
			AssertDutiesAndTaxesFeesAmount(304M, x => x.CL_Calc_ImportDutyNonCashAmount, UniversalReferenceConstants.RefCusRateCodes.DTA, UniversalReferenceConstants.RefCusRateCodes.DTS, 101M, 102M, 201M, 202M);
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_ImportDutyNonCashAmount_Caption()
		{
			var entryLine = Factory.New<CusEntryLine>();
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(entryLine.CL_Calc_ImportDutyNonCashAmountInfo, "Import Duty (NON-CASH)", "DT Amt. (NON-CASH)");
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_RetaliatoryDutyAmount()
		{
			AssertDutiesAndTaxesFeesAmount(203M, x => x.CL_Calc_RetaliatoryDutyAmount, UniversalReferenceConstants.RefCusRateCodes.RTD, 101M, 102M);
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_RetaliatoryDutyAmount_Caption()
		{
			var entryLine = Factory.New<CusEntryLine>();
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(entryLine.CL_Calc_RetaliatoryDutyAmountInfo, "Retaliatory Duty", "RTD Amt.");
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_SpecificallySelectedGoodsAndServicesTaxAmount()
		{
			AssertDutiesAndTaxesFeesAmount(203M, x => x.CL_Calc_SpecificallySelectedGoodsAndServicesTaxAmount, UniversalReferenceConstants.RefCusRateCodes.SSG, 101M, 102M);
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_SpecificallySelectedGoodsAndServicesTaxAmount_Caption()
		{
			var entryLine = Factory.New<CusEntryLine>();
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(entryLine.CL_Calc_SpecificallySelectedGoodsAndServicesTaxAmountInfo, "Specifically Selected Goods and Services Tax", "SSG Amt.");
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_TobaccoAndAlcoholTaxAmount()
		{
			AssertDutiesAndTaxesFeesAmount(403M, x => x.CL_Calc_TobaccoAndAlcoholTaxAmount, UniversalReferenceConstants.RefCusRateCodes.TAT, 101M, 102M);
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_TobaccoAndAlcoholTaxAmount_Caption()
		{
			var entryLine = Factory.New<CusEntryLine>();
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(entryLine.CL_Calc_TobaccoAndAlcoholTaxAmountInfo, "Tobacco and Alcohol Tax", "TAT Amt.");
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_TradePromotionFeeAmount()
		{
			AssertDutiesAndTaxesFeesAmount(203M, x => x.CL_Calc_TradePromotionFeeAmount, RefCusTaxOrFeeCodes.TPF, 101M, 102M);
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_TradePromotionFeeAmount_Caption()
		{
			var entryLine = Factory.New<CusEntryLine>();
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(entryLine.CL_Calc_TradePromotionFeeAmountInfo, "Trade Promotion Fee", "TPF Amt.");
		}

		[ExpectNoExceptions]
		void AssertDutiesAndTaxesFeesAmount(decimal expectedAmount, Func<CusEntryLine, decimal> clacPropertySelector, string chargeType, params decimal[] inputAmouts)
		{
			AssertDutiesAndTaxesFeesAmount(expectedAmount, clacPropertySelector, chargeType, string.Empty, inputAmouts);
		}

		[ExpectNoExceptions]
		void AssertDutiesAndTaxesFeesAmount(decimal expectedAmount, Func<CusEntryLine, decimal> clacPropertySelector, string chargeType1, string chargeType2, params decimal[] inputAmouts)
		{
			var entryLine = CreateEntryLine();
			var lineFee1 = entryLine.Fees.AddNew();
			lineFee1.CF_ChargeType = chargeType1;
			lineFee1.CF_ChargeAmount = inputAmouts[0];
			lineFee1.CF_MethodOfPayment = DutyTaxPaymentMethodList.Codes.CashPayment;

			var lineFee2 = entryLine.Fees.AddNew();
			lineFee2.CF_ChargeType = chargeType1;
			lineFee2.CF_ChargeAmount = inputAmouts[1];
			lineFee2.CF_MethodOfPayment = DutyTaxPaymentMethodList.Codes.NonCashPayment;

			if (!string.IsNullOrEmpty(chargeType2))
			{
				if (inputAmouts[2] > 0)
				{
					var lineFee3 = entryLine.Fees.AddNew();
					lineFee3.CF_ChargeType = chargeType2;
					lineFee3.CF_ChargeAmount = inputAmouts[2];
					lineFee3.CF_MethodOfPayment = DutyTaxPaymentMethodList.Codes.CashPayment;
				}

				if (inputAmouts[3] > 0)
				{
					var lineFee4 = entryLine.Fees.AddNew();
					lineFee4.CF_ChargeType = chargeType2;
					lineFee4.CF_ChargeAmount = inputAmouts[3];
					lineFee4.CF_MethodOfPayment = DutyTaxPaymentMethodList.Codes.NonCashPayment;
				}
			}

			NUnit.Framework.Assert.That(clacPropertySelector.Invoke(entryLine), NUnit.Framework.Is.EqualTo(expectedAmount));
		}

		[ExpectNoExceptions]
		public void TestTaiwanTariffFormatter()
		{
			var tariff = "87149990905";
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Description = "InvoiceLineDescription";
			invoiceLine.JI_Tariff = tariff;
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			NUnit.Framework.Assert.That(invoiceLine.JI_FormattedTariff, NUnit.Framework.Is.EqualTo("8714.99.90.90-5").Using(CustomComparers.TypeComparison), "JI_FormattedTariff");
			NUnit.Framework.Assert.That(entryLine.FormattedTariff, NUnit.Framework.Is.EqualTo(invoiceLine.JI_FormattedTariff), "FormattedTariff");
		}

		[TestDate(2005, 6, 2)]
		[ExpectNoExceptions]
		public override void TestMoneyInLocalCurrency()
		{
			var newCurrency = RefCurrency.New(Factory);
			newCurrency.RX_Code = "MDD";
			newCurrency.SetCustomsRate(new ZDateTime(2005, 6, 1), new ZDateTime(2005, 6, 5), 0.5m);
			var declaration = ImportJobDeclaration;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = newCurrency.RX_Code;
			invoiceHeader.JZ_InvoiceAmount = 300m;
			var line1 = (JobComInvoiceLine)invoiceHeader.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "2203.10.10 10";
			line1.JI_InvoiceQuantity = 1m;
			line1.JI_EnteredUnitPrice = 100.0m;
			var line2 = (JobComInvoiceLine)invoiceHeader.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "2203.10.10 10";
			line2.JI_InvoiceQuantity = 1m;
			line2.JI_EnteredUnitPrice = 200.0m;
			var line1ONS = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
			line1ONS.J7_Amount = 5.0m;
			var line1OFT = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
			line1OFT.J7_Amount = 10.0m;
			var line2ONS = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
			line2ONS.J7_Amount = 10.0m;
			var line2OFT = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
			line2OFT.J7_Amount = 20.0m;
			DoMerge(declaration);
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryLine.FOBInLocalCurrency.Amount, NUnit.Framework.Is.EqualTo(690.0m).Using(CustomComparers.TypeComparison), "FOB");
				NUnit.Framework.Assert.That(entryLine.CIFInLocalCurrency.Amount, NUnit.Framework.Is.EqualTo(690.0m).Using(CustomComparers.TypeComparison), "CIF");
				NUnit.Framework.Assert.That(entryLine.OverseasFreightInLocalCurrency.Amount, NUnit.Framework.Is.EqualTo(60.0m).Using(CustomComparers.TypeComparison), "Overseas Freight");
				NUnit.Framework.Assert.That(entryLine.OverseasInsuranceInLocalCurrency.Amount, NUnit.Framework.Is.EqualTo(30.0m).Using(CustomComparers.TypeComparison), "Overseas Insurance");
				NUnit.Framework.Assert.That(entryLine.TAndIInLocalCurrency.Amount, NUnit.Framework.Is.EqualTo(90.0m).Using(CustomComparers.TypeComparison), "T and I");
			}

			);
		}

		[ExpectNoExceptions]
		public override void TestFirstLine()
		{
			const string tarrifCode = "0000.00.00.00Y";
			const string tarrifCode1 = "A";
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = tarrifCode;
			line1.JI_Description = ZString.Empty;
			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = tarrifCode;
			line2.JI_Description = ZString.Empty;
			var line3 = invoice.JobComInvoiceLines.AddNew();
			line3.JI_Description = ZString.Empty;
			var line4 = invoice.JobComInvoiceLines.AddNew();
			line4.JI_Tariff = tarrifCode;
			line4.JI_Description = ZString.Empty;
			var line5 = invoice.JobComInvoiceLines.AddNew();
			line5.JI_Tariff = tarrifCode;
			line5.JI_Description = ZString.Empty;
			var line6 = invoice.JobComInvoiceLines.AddNew();
			line6.JI_Tariff = tarrifCode;
			line6.JI_Description = ZString.Empty;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var entryLine1 = declaration.CustomsEntryHeaders[0].MergedLines[0];
			NUnit.Framework.Assert.That(entryLine1.FirstLine.PK, NUnit.Framework.Is.EqualTo(line1.PK), "Get FirstLine");
			var entryLine2 = declaration.CustomsEntryHeaders[0].MergedLines[1];
			NUnit.Framework.Assert.That(entryLine2.FirstLine.PK, NUnit.Framework.Is.EqualTo(line2.PK), "Get FirstLine");
			var entryLine3 = declaration.CustomsEntryHeaders[0].MergedLines[2];
			NUnit.Framework.Assert.That(entryLine3.FirstLine.PK, NUnit.Framework.Is.EqualTo(line3.PK), "Get FirstLine");
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			line3.JI_Tariff = tarrifCode1;
			line3.JI_Description = ZString.Empty;
			line4.JI_Tariff = tarrifCode1;
			line4.JI_Description = ZString.Empty;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].MergedLines[0].FirstLine.PK, NUnit.Framework.Is.EqualTo(line1.PK), "Get FirstLine");
			NUnit.Framework.Assert.That(declaration.CustomsEntryHeaders[0].MergedLines[1].FirstLine.PK, NUnit.Framework.Is.EqualTo(line3.PK), "Get FirstLine");
		}

		[ExpectNoExceptions]
		public void TestCommodityTax()
		{
			var entryLine = CreateEntryLine();
			NUnit.Framework.Assert.That(entryLine.InvoiceQuantity, NUnit.Framework.Is.EqualTo(200M).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryLine.UnitCommodityTax, NUnit.Framework.Is.EqualTo(100M / 200M).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestUnitCustomsQuantityInKG()
		{
			var entryLine = CreateEntryLine();
			NUnit.Framework.Assert.That(entryLine.InvoiceQuantity, NUnit.Framework.Is.EqualTo(200M).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryLine.UnitCustomsQuantityInKG, NUnit.Framework.Is.EqualTo(100M / 200M).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAdditionalDocuments()
		{
			var entryLine = CreateEntryLine();
			var additionalDocuments = entryLine.AdditionalDocuments;
			NUnit.Framework.Assert.That(additionalDocuments.Count, NUnit.Framework.Is.EqualTo(0));
			var invoiceLine = entryLine.InvoiceLines[0] as JobComInvoiceLine;
			var cusSupporting = invoiceLine.PermitCusSupportingCollection.AddNew();
			cusSupporting.CSI_ReferenceNumber = "ref1";
			cusSupporting.CSI_LineNo = 1;
			invoiceLine.PermitCusSupportingCollection.AddNew();
			cusSupporting = invoiceLine.PermitCusSupportingCollection.AddNew();
			cusSupporting.CSI_ReferenceNumber = "ref1";
			cusSupporting = invoiceLine.PermitCusSupportingCollection.AddNew();
			cusSupporting.CSI_ReferenceNumber = "ref2";
			cusSupporting.CSI_LineNo = 2;
			cusSupporting = invoiceLine.PermitCusSupportingCollection.AddNew();
			cusSupporting.CSI_ReferenceNumber = "ref2";
			cusSupporting.CSI_LineNo = 2;
			additionalDocuments = entryLine.AdditionalDocuments;
			NUnit.Framework.Assert.That(additionalDocuments.Count, NUnit.Framework.Is.EqualTo(3));
			NUnit.Framework.Assert.That(additionalDocuments, NUnit.Framework.Has.Some.EqualTo("ref1-1"));
			NUnit.Framework.Assert.That(additionalDocuments, NUnit.Framework.Has.Some.EqualTo("ref1-0"));
			NUnit.Framework.Assert.That(additionalDocuments, NUnit.Framework.Has.Some.EqualTo("ref2-2"));
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_UnitPriceQuantityAndUQ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = MergeByCodeList.Codes.CondensedDeclaration;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndInsurance;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_InvoiceQuantity = 3;
			invoiceLine1.JI_InvoiceUQ = "ACR";
			invoiceLine1.JI_EnteredUnitPrice = 2004.89m;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_InvoiceQuantity = 4;
			invoiceLine2.JI_InvoiceUQ = "AML";
			invoiceLine2.JI_EnteredUnitPrice = 2426.68m;
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			NUnit.Framework.Assert.That(entryLine.CL_EntryLineQty, NUnit.Framework.Is.EqualTo(1m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryLine.CL_EntryLineUQ, NUnit.Framework.Is.EqualTo("LOT").Using(CustomComparers.TypeComparison));
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine1 = entryHeader.MergedLines[0];
			var entryLine2 = entryHeader.MergedLines[1];
			NUnit.Framework.Assert.That(entryLine1.CL_EntryLineQty, NUnit.Framework.Is.EqualTo(3m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryLine1.CL_EntryLineUQ, NUnit.Framework.Is.EqualTo("ACR").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryLine2.CL_EntryLineQty, NUnit.Framework.Is.EqualTo(4m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryLine2.CL_EntryLineUQ, NUnit.Framework.Is.EqualTo("AML").Using(CustomComparers.TypeComparison));
		}

		[TestDate(2016, 01, 01)]
		[ExpectNoExceptions]
		public void TestCL_EntryLineUnitPrice()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 30.56m, new ZDateTime(2016, 01, 01), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.EuropeanUnion, 34m, new ZDateTime(2016, 01, 01), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 30m, new ZDateTime(2016, 01, 01), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.EuropeanUnion, 33m, new ZDateTime(2016, 01, 01), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = MergeByCodeList.Codes.CondensedDeclaration;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 30000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_IncoTerm = "FOB";
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 20000m;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_PartNo = "PARTNO1";
			invoiceLine1.JI_InvoiceQuantity = 4;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			var invoiceChargeCollection = invoice.Charges;
			var inviceLineChargeCollection = invoiceLine1.Charges;
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 10000m;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_PartNo = "PARTNO1";
			invoiceLine2.JI_InvoiceQuantity = 2;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			var invoiceCharge = invoiceChargeCollection.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceCharge.J7_IsIncludedInITOT = true;
			invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
			invoiceCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			invoiceCharge = invoiceChargeCollection.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 200m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceCharge.J7_IsIncludedInITOT = true;
			invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			invoiceCharge = invoiceChargeCollection.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 300m, Core.Constants.CurrencyCodes.EuropeanUnion);
			invoiceCharge.J7_IsIncludedInITOT = false;
			invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			invoiceCharge = invoiceChargeCollection.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 900m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceCharge.J7_IsIncludedInITOT = false;
			invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
			invoiceCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			invoiceCharge = invoiceChargeCollection.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 600m, Core.Constants.CurrencyCodes.EuropeanUnion);
			invoiceCharge.J7_IsIncludedInITOT = false;
			invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
			invoiceCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			var invoiceLineCharge = inviceLineChargeCollection.AddNew(CustomsChargeTypeList.Codes.Commission, 120m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLineCharge.J7_IsIncludedInITOT = false;
			invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
			invoiceLineCharge = inviceLineChargeCollection.AddNew(CustomsChargeTypeList.Codes.PackingCost, 700m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLineCharge.J7_IsIncludedInITOT = false;
			invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmount = false;
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.AllEntryLines[0];
			NUnit.Framework.Assert.That(entryLine.GetType(), CustomConstraints.HasCustomAttribute<DecimalPlacesAttribute>("CL_EntryLineUnitPrice", true, attrib => attrib.DecimalPlaces == 6));
			NUnit.Framework.Assert.That(entryLine.CL_EntryLineQty, NUnit.Framework.Is.EqualTo(1m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryLine.CL_EntryLineUnitPrice, NUnit.Framework.Is.EqualTo(30000m).Using(CustomComparers.TypeComparison));
			invoiceLine1.JI_EnteredUnitPrice = 0;
			invoiceLine1.JI_InvoiceQuantity = 333;
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			NUnit.Framework.Assert.That(entryLine.CL_EntryLineQty, NUnit.Framework.Is.EqualTo(1M).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryLine.CL_EntryLineUnitPrice, NUnit.Framework.Is.EqualTo(30000m).Using(CustomComparers.TypeComparison));
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Factory.Save();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryLine.CL_EntryLineQty, NUnit.Framework.Is.EqualTo(1M).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryLine.CL_EntryLineUnitPrice, NUnit.Framework.Is.EqualTo(30000m).Using(CustomComparers.TypeComparison));
			}

			);
			invoiceChargeCollection.RemoveAndDeleteAll();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoice.JZ_InvoiceAmount = 1.07m;
			invoiceLine1.JI_LinePrice = 1.07m;
			invoiceLine1.JI_EnteredUnitPrice = 0m;
			invoiceLine1.JI_InvoiceQuantity = 8m;
			invoiceLine2.JI_LinePrice = 0m;
			invoiceLine2.JI_InvoiceQuantity = 0m;
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			NUnit.Framework.Assert.That(entryLine.CL_EntryLineUnitPrice, NUnit.Framework.Is.EqualTo(1.07m).Using(CustomComparers.TypeComparison));
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			invoiceLine1.JI_EnteredUnitPrice = 1000;
			invoiceLine1.JI_InvoiceQuantity = 4;
			invoiceLine1.JI_LinePrice = 20000m;
			invoiceLine2.JI_EnteredUnitPrice = 1000;
			invoiceLine2.JI_LinePrice = 10000m;
			invoiceLine2.JI_EnteredUnitPrice = 0m;
			invoiceLine2.JI_InvoiceQuantity = 2;
			NUnit.Framework.Assert.That(invoiceLine2.JI_EnteredUnitPrice, NUnit.Framework.Is.EqualTo(5000m).Using(CustomComparers.TypeComparison));
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			NUnit.Framework.Assert.That(entryLine.CL_EntryLineQty, NUnit.Framework.Is.EqualTo(4m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryLine.CL_EntryLineUnitPrice, NUnit.Framework.Is.EqualTo(1000m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCustomsQuantityInKG()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CustomsQuantity = 1000m;
			invoiceLine1.JI_CustomsUnitQty = "KGM";
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.AllEntryLines[0];
			NUnit.Framework.Assert.That(entryLine.CustomsQuantityInKG, NUnit.Framework.Is.EqualTo(1000m).Using(CustomComparers.TypeComparison));
			invoiceLine1.JI_CustomsQuantity = 2M;
			invoiceLine1.JI_CustomsUnitQty = "TNE";
			NUnit.Framework.Assert.That(entryLine.CustomsQuantityInKG, NUnit.Framework.Is.EqualTo(2000m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_NetWeightInKG()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_NetWeight = 200m;
			invoiceLine1.JI_NetWeightUQ = "KG";

			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_NetWeight = 100m;
			invoiceLine2.JI_NetWeightUQ = "KG";
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.AllEntryLines[0];
			NUnit.Framework.Assert.That(entryLine.CL_Calc_NetWeightInKG, NUnit.Framework.Is.EqualTo(300m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_CustomsSecondQuantity()
		{
			var entryLine = CreateEntryLine();
			NUnit.Framework.Assert.That(entryLine.CL_Calc_CustomsSecondQuantity, NUnit.Framework.Is.EqualTo(2M).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_CustomsSecondUnitQty()
		{
			var entryLine = CreateEntryLine();
			NUnit.Framework.Assert.That(entryLine.CL_CustomsSecondUnitQty, NUnit.Framework.Is.EqualTo("TNE").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_BondedGoodsCode()
		{
			var entryLine = CreateEntryLine();
			NUnit.Framework.Assert.That(entryLine.CL_BondedGoodsCode, NUnit.Framework.Is.EqualTo(BondedGoodsCodeList.Codes.NB).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCL_EnvironmentalProtectionCode()
		{
			var entryLine = CreateEntryLine();
			NUnit.Framework.Assert.That(entryLine.CL_EnvironmentalProtectionCode, NUnit.Framework.Is.EqualTo("Z00").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCL_CustomsValueForDutyCalculation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoice.JZ_InvoiceAmount = 100m;
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Procedure = Constants.ProcedureCodes._37;
			invoiceLine1.JI_InvoiceQuantity = 2;
			invoiceLine1.JI_EnteredUnitPrice = 50;
			invoiceLine1.JI_UseOneTenthCV = ZBool.False;
			invoiceLine1.JI_RAPPrice = 1000m;
			invoiceLine1.JI_RAPCurr = "TWD";
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.AllEntryLines[0];
			entryLine.CL_CustomsValue = 100m;
			NUnit.Framework.Assert.That(entryLine.CL_CustomsValueForDutyCalculation, NUnit.Framework.Is.EqualTo(1000m).Using(CustomComparers.TypeComparison), "Use JI_RAPPrice");
			NUnit.Framework.Assert.That(entryLine.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(1000m).Using(CustomComparers.TypeComparison), "Use JI_RAPPrice");
			invoiceLine1.JI_RAPPrice = 1000.123m;
			NUnit.Framework.Assert.That(entryLine.CL_CustomsValueForDutyCalculation, NUnit.Framework.Is.EqualTo(1000m).Using(CustomComparers.TypeComparison), "Value should be rounded");
			NUnit.Framework.Assert.That(entryLine.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(1000m).Using(CustomComparers.TypeComparison), "Value should be rounded");
			invoiceLine1.JI_UseOneTenthCV = ZBool.True;
			NUnit.Framework.Assert.That(entryLine.CL_CustomsValueForDutyCalculation, NUnit.Framework.Is.EqualTo(10m).Using(CustomComparers.TypeComparison), "Use 10% * CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(10m).Using(CustomComparers.TypeComparison), "Use 10% * CL_CustomsValue");
			invoiceLine1.JI_UseOneTenthCV = ZBool.False;
			invoiceLine1.JI_Procedure = Constants.ProcedureCodes._31;
			NUnit.Framework.Assert.That(entryLine.CL_CustomsValueForDutyCalculation, NUnit.Framework.Is.EqualTo(100m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryLine.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCL_CustomsValueForCustomsValuation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoice.JZ_InvoiceAmount = 100m;
			invoice.JZ_IncoTerm = "C&I";
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Procedure = Constants.ProcedureCodes._37;
			invoiceLine1.JI_InvoiceQuantity = 2;
			invoiceLine1.JI_EnteredUnitPrice = 50;
			invoiceLine1.JI_UseOneTenthCV = ZBool.False;
			invoiceLine1.JI_RAPPrice = 1000m;
			invoiceLine1.JI_RAPCurr = "TWD";
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.AllEntryLines[0];
			NUnit.Framework.Assert.That(entryLine.CL_CustomsValueForCustomsValuation, NUnit.Framework.Is.EqualTo(100m).Using(CustomComparers.TypeComparison));

			invoiceLine1.JI_InvoiceQuantity = 3m;
			invoice.JZ_InvoiceAmount = 150m;
			NUnit.Framework.Assert.That(entryLine.CL_CustomsValueForCustomsValuation, NUnit.Framework.Is.EqualTo(150m).Using(CustomComparers.TypeComparison));

			invoiceLine1.JI_EnteredUnitPrice = 60m;
			invoice.JZ_InvoiceAmount = 180m;
			NUnit.Framework.Assert.That(entryLine.CL_CustomsValueForCustomsValuation, NUnit.Framework.Is.EqualTo(180m).Using(CustomComparers.TypeComparison));

			invoiceLine1.JI_EnteredUnitPrice = 60.012m;
			NUnit.Framework.Assert.That(entryLine.CL_CustomsValueForCustomsValuation, NUnit.Framework.Is.EqualTo(180m).Using(CustomComparers.TypeComparison), "Value should be rounded");

			var charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = "ONS";
			charge.J7_Amount = 10m;
			charge.J7_IsIncludedInITOT = true;
			NUnit.Framework.Assert.That(entryLine.CL_CustomsValueForCustomsValuation, NUnit.Framework.Is.EqualTo(180m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCL_CustomsValueForCustomsValuationWhenValueIsZero()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = Constants.ProcedureCodes._37;
			invoiceLine.JI_InvoiceQuantity = 2;
			invoiceLine.JI_EnteredUnitPrice = 0;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.AllEntryLines[0];
			NUnit.Framework.Assert.That(entryLine.CL_CustomsValueForCustomsValuation, NUnit.Framework.Is.EqualTo(1m).Using(CustomComparers.TypeComparison));

			invoiceLine.JI_EnteredUnitPrice = 0.3;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			entryHeader = declaration.CustomsEntryHeaders[0];
			entryLine = entryHeader.AllEntryLines[0];
			NUnit.Framework.Assert.That(entryLine.CL_CustomsValueForCustomsValuation, NUnit.Framework.Is.EqualTo(1m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestRoundCustomsValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Procedure = Constants.ProcedureCodes._37;
			invoiceLine1.JI_InvoiceQuantity = 10;
			invoiceLine1.JI_UseOneTenthCV = ZBool.False;
			invoiceLine1.JI_RAPPrice = 1000m;
			invoiceLine1.JI_RAPCurr = "TWD";
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.AllEntryLines[0];
			entryLine.CL_CustomsValue = 100.123m;
			entryLine.RoundCustomsValue();
			NUnit.Framework.Assert.That(entryLine.CL_CustomsValue, NUnit.Framework.Is.EqualTo(100m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPreReconciledCustomsValue()
		{
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.CL_CustomsValue = 685m;
			NUnit.Framework.Assert.That(entryLine.PreReconciledCustomsValue, NUnit.Framework.Is.EqualTo(685m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestRorActualCases1()
		{
			var entryHeader = CusEntryLineTestHelperForTest.GetImportRorEntryHeaderTestCase1(Factory);
			var allEntryLines = entryHeader.AllEntryLines;
			var entryLine1 = allEntryLines[0];
			NUnit.Framework.Assert.That(entryLine1.CL_CustomsValue, NUnit.Framework.Is.EqualTo(54367m).Using(CustomComparers.TypeComparison), "entryLine1.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine1.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(2718m).Using(CustomComparers.TypeComparison), "entryLine1.CL_RAPOrRORCustomsValue");
			var entryLine2 = allEntryLines[1];
			NUnit.Framework.Assert.That(entryLine2.CL_CustomsValue, NUnit.Framework.Is.EqualTo(18444m).Using(CustomComparers.TypeComparison), "entryLine2.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine2.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(922m).Using(CustomComparers.TypeComparison), "entryLine2.CL_RAPOrRORCustomsValue");
			var entryLine3 = allEntryLines[2];
			NUnit.Framework.Assert.That(entryLine3.CL_CustomsValue, NUnit.Framework.Is.EqualTo(25434m).Using(CustomComparers.TypeComparison), "entryLine3.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine3.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(1272m).Using(CustomComparers.TypeComparison), "entryLine3.CL_RAPOrRORCustomsValue");
			var entryLine4 = allEntryLines[3];
			NUnit.Framework.Assert.That(entryLine4.CL_CustomsValue, NUnit.Framework.Is.EqualTo(27352m).Using(CustomComparers.TypeComparison), "entryLine4.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine4.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(1368m).Using(CustomComparers.TypeComparison), "entryLine4.CL_RAPOrRORCustomsValue");
			var entryLine5 = allEntryLines[4];
			NUnit.Framework.Assert.That(entryLine5.CL_CustomsValue, NUnit.Framework.Is.EqualTo(18360m).Using(CustomComparers.TypeComparison), "entryLine5.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine5.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(918m).Using(CustomComparers.TypeComparison), "entryLine5.CL_RAPOrRORCustomsValue");
			var entryLine6 = allEntryLines[5];
			NUnit.Framework.Assert.That(entryLine6.CL_CustomsValue, NUnit.Framework.Is.EqualTo(18360m).Using(CustomComparers.TypeComparison), "entryLine6.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine6.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(918m).Using(CustomComparers.TypeComparison), "entryLine6.CL_RAPOrRORCustomsValue");
			var entryLine7 = allEntryLines[6];
			NUnit.Framework.Assert.That(entryLine7.CL_CustomsValue, NUnit.Framework.Is.EqualTo(5586m).Using(CustomComparers.TypeComparison), "entryLine7.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine7.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(279m).Using(CustomComparers.TypeComparison), "entryLine7.CL_RAPOrRORCustomsValue");
			var entryLine8 = allEntryLines[7];
			NUnit.Framework.Assert.That(entryLine8.CL_CustomsValue, NUnit.Framework.Is.EqualTo(6719m).Using(CustomComparers.TypeComparison), "entryLine8.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine8.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(336m).Using(CustomComparers.TypeComparison), "entryLine8.CL_RAPOrRORCustomsValue");
			var entryLine9 = allEntryLines[8];
			NUnit.Framework.Assert.That(entryLine9.CL_CustomsValue, NUnit.Framework.Is.EqualTo(73616m).Using(CustomComparers.TypeComparison), "entryLine9.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine9.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(3681m).Using(CustomComparers.TypeComparison), "entryLine9.CL_RAPOrRORCustomsValue");
			var entryLine10 = allEntryLines[9];
			NUnit.Framework.Assert.That(entryLine10.CL_CustomsValue, NUnit.Framework.Is.EqualTo(26867m).Using(CustomComparers.TypeComparison), "entryLine10.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine10.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(1343m).Using(CustomComparers.TypeComparison), "entryLine10.CL_RAPOrRORCustomsValue");
			var entryLine11 = allEntryLines[10];
			NUnit.Framework.Assert.That(entryLine11.CL_CustomsValue, NUnit.Framework.Is.EqualTo(6673m).Using(CustomComparers.TypeComparison), "entryLine11.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine11.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(334m).Using(CustomComparers.TypeComparison), "entryLine11.CL_RAPOrRORCustomsValue");
			var entryLine12 = allEntryLines[11];
			NUnit.Framework.Assert.That(entryLine12.CL_CustomsValue, NUnit.Framework.Is.EqualTo(5055m).Using(CustomComparers.TypeComparison), "entryLine12.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine12.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(253m).Using(CustomComparers.TypeComparison), "entryLine12.CL_RAPOrRORCustomsValue");

			var dutyTaxFeeCharges = entryHeader.DutyTaxFeeCharges.Cast<DutyTaxFeeCharge>();
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(5));
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Any(x => x.ChargeType == "A10" && x.MethodOfPayment == "CAS" && x.ChargeAmount == 662m), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Any(x => x.ChargeType == "A19" && x.MethodOfPayment == "DEF" && x.ChargeAmount == 12579m), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Any(x => x.ChargeType == "B40" && x.MethodOfPayment == "CAS" && x.ChargeAmount == 750m), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Any(x => x.ChargeType == "B49" && x.MethodOfPayment == "DEF" && x.ChargeAmount == 14253m), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Any(x => x.ChargeType == "B59" && x.MethodOfPayment == "DEF" && x.ChargeAmount == 114m), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(entryHeader.BusinessTaxBaseAmount, NUnit.Framework.Is.EqualTo(15004m).Using(CustomComparers.TypeComparison), "BusinessTaxBaseAmount");
		}

		[ExpectNoExceptions]
		public void TestRorActualCases2()
		{
			var entryHeader = CusEntryLineTestHelperForTest.GetImportRorEntryHeaderTestCase2(Factory);
			var entryLine1 = entryHeader.MergedLines[0];
			NUnit.Framework.Assert.That(entryLine1.CL_CustomsValue, NUnit.Framework.Is.EqualTo(360578m).Using(CustomComparers.TypeComparison), "entryLine1.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine1.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "entryLine1.CL_RAPOrRORCustomsValue");

			var dutyTaxFeeCharges = entryHeader.DutyTaxFeeCharges.Cast<DutyTaxFeeCharge>();
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Any(x => x.ChargeType == "B49" && x.MethodOfPayment == "DEF" && x.ChargeAmount == 18028m), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Any(x => x.ChargeType == "B59" && x.MethodOfPayment == "DEF" && x.ChargeAmount == 144m), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(entryHeader.BusinessTaxBaseAmount, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "BusinessTaxBaseAmount");
		}

		[ExpectNoExceptions]
		public void TestRorActualCases3()
		{
			var entryHeader = CusEntryLineTestHelperForTest.GetImportRorEntryHeaderTestCase3(Factory);
			var entryLine1 = entryHeader.MergedLines[0];
			NUnit.Framework.Assert.That(entryLine1.CL_CustomsValue, NUnit.Framework.Is.EqualTo(79961m).Using(CustomComparers.TypeComparison), "entryLine1.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine1.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(7996m).Using(CustomComparers.TypeComparison), "entryLine1.CL_RAPOrRORCustomsValue");

			var dutyTaxFeeCharges = entryHeader.DutyTaxFeeCharges.Cast<DutyTaxFeeCharge>();
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(4));
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Any(x => x.ChargeType == "A10" && x.MethodOfPayment == "CAS" && x.ChargeAmount == 799m), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Any(x => x.ChargeType == "A19" && x.MethodOfPayment == "DEF" && x.ChargeAmount == 7196m), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Any(x => x.ChargeType == "B40" && x.MethodOfPayment == "CAS" && x.ChargeAmount == 439m), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Any(x => x.ChargeType == "B49" && x.MethodOfPayment == "DEF" && x.ChargeAmount == 3958m), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(entryHeader.BusinessTaxBaseAmount, NUnit.Framework.Is.EqualTo(8795m).Using(CustomComparers.TypeComparison), "BusinessTaxBaseAmount");
		}

		[ExpectNoExceptions]
		public void TestRorActualCases4()
		{
			var entryHeader = CusEntryLineTestHelperForTest.GetImportRorEntryHeaderTestCase4(Factory);
			var entryLine1 = entryHeader.MergedLines[0];
			NUnit.Framework.Assert.That(entryLine1.CL_CustomsValue, NUnit.Framework.Is.EqualTo(6220200m).Using(CustomComparers.TypeComparison), "entryLine1.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine1.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(622020m).Using(CustomComparers.TypeComparison), "entryLine1.CL_RAPOrRORCustomsValue");
		}

		[ExpectNoExceptions]
		public void TestRorActualCases5()
		{
			var entryHeader = CusEntryLineTestHelperForTest.GetImportRorEntryHeaderTestCase5(Factory);
			var allEntryLines = entryHeader.AllEntryLines;
			var entryLine1 = allEntryLines[0];
			NUnit.Framework.Assert.That(entryLine1.CL_CustomsValue, NUnit.Framework.Is.EqualTo(8768m).Using(CustomComparers.TypeComparison), "entryLine1.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine1.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "entryLine1.CL_RAPOrRORCustomsValue");
			var entryLine2 = allEntryLines[1];
			NUnit.Framework.Assert.That(entryLine2.CL_CustomsValue, NUnit.Framework.Is.EqualTo(8768m).Using(CustomComparers.TypeComparison), "entryLine2.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine2.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "entryLine2.CL_RAPOrRORCustomsValue");
			var entryLine3 = allEntryLines[2];
			NUnit.Framework.Assert.That(entryLine3.CL_CustomsValue, NUnit.Framework.Is.EqualTo(74383m).Using(CustomComparers.TypeComparison), "entryLine3.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine3.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "entryLine3.CL_RAPOrRORCustomsValue");
			var entryLine4 = allEntryLines[3];
			NUnit.Framework.Assert.That(entryLine4.CL_CustomsValue, NUnit.Framework.Is.EqualTo(74874m).Using(CustomComparers.TypeComparison), "entryLine4.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine4.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "entryLine4.CL_RAPOrRORCustomsValue");
			var entryLine5 = allEntryLines[4];
			NUnit.Framework.Assert.That(entryLine5.CL_CustomsValue, NUnit.Framework.Is.EqualTo(32405m).Using(CustomComparers.TypeComparison), "entryLine5.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine5.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "entryLine5.CL_RAPOrRORCustomsValue");
			var entryLine6 = allEntryLines[5];
			NUnit.Framework.Assert.That(entryLine6.CL_CustomsValue, NUnit.Framework.Is.EqualTo(109208m).Using(CustomComparers.TypeComparison), "entryLine6.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine6.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "entryLine6.CL_RAPOrRORCustomsValue");
			var entryLine7 = allEntryLines[6];
			NUnit.Framework.Assert.That(entryLine7.CL_CustomsValue, NUnit.Framework.Is.EqualTo(154308m).Using(CustomComparers.TypeComparison), "entryLine7.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine7.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "entryLine7.CL_RAPOrRORCustomsValue");
			var entryLine8 = allEntryLines[7];
			NUnit.Framework.Assert.That(entryLine8.CL_CustomsValue, NUnit.Framework.Is.EqualTo(20306m).Using(CustomComparers.TypeComparison), "entryLine8.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine8.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "entryLine8.CL_RAPOrRORCustomsValue");
			var entryLine9 = allEntryLines[8];
			NUnit.Framework.Assert.That(entryLine9.CL_CustomsValue, NUnit.Framework.Is.EqualTo(8627m).Using(CustomComparers.TypeComparison), "entryLine9.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine9.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "entryLine9.CL_RAPOrRORCustomsValue");
			var entryLine10 = allEntryLines[9];
			NUnit.Framework.Assert.That(entryLine10.CL_CustomsValue, NUnit.Framework.Is.EqualTo(6137m).Using(CustomComparers.TypeComparison), "entryLine10.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine10.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "entryLine10.CL_RAPOrRORCustomsValue");
			var entryLine11 = allEntryLines[10];
			NUnit.Framework.Assert.That(entryLine11.CL_CustomsValue, NUnit.Framework.Is.EqualTo(9609m).Using(CustomComparers.TypeComparison), "entryLine11.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine11.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "entryLine11.CL_RAPOrRORCustomsValue");
			var entryLine12 = allEntryLines[11];
			NUnit.Framework.Assert.That(entryLine12.CL_CustomsValue, NUnit.Framework.Is.EqualTo(248997m).Using(CustomComparers.TypeComparison), "entryLine12.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine12.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "entryLine12.CL_RAPOrRORCustomsValue");
			var entryLine13 = allEntryLines[12];
			NUnit.Framework.Assert.That(entryLine13.CL_CustomsValue, NUnit.Framework.Is.EqualTo(6766m).Using(CustomComparers.TypeComparison), "entryLine13.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine13.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "entryLine13.CL_RAPOrRORCustomsValue");
			var entryLine14 = allEntryLines[13];
			NUnit.Framework.Assert.That(entryLine14.CL_CustomsValue, NUnit.Framework.Is.EqualTo(483966m).Using(CustomComparers.TypeComparison), "entryLine14.CL_CustomsValue");
			NUnit.Framework.Assert.That(entryLine14.CL_RAPOrRORCustomsValue, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "entryLine14.CL_RAPOrRORCustomsValue");

			var dutyTaxFeeCharges = entryHeader.DutyTaxFeeCharges.Cast<DutyTaxFeeCharge>();
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(5));
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Any(x => x.ChargeType == "A19" && x.MethodOfPayment == "DEF" && x.ChargeAmount == 9375m), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Any(x => x.ChargeType == "B40" && x.MethodOfPayment == "CAS" && x.ChargeAmount == 24198m), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Any(x => x.ChargeType == "B49" && x.MethodOfPayment == "DEF" && x.ChargeAmount == 38626m), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Any(x => x.ChargeType == "B51" && x.MethodOfPayment == "CAS" && x.ChargeAmount == 193m), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Any(x => x.ChargeType == "B59" && x.MethodOfPayment == "DEF" && x.ChargeAmount == 305m), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(entryHeader.BusinessTaxBaseAmount, NUnit.Framework.Is.EqualTo(483966m).Using(CustomComparers.TypeComparison), "BusinessTaxBaseAmount");
		}
		
		[ExpectNoExceptions]
		public void TestCalculateForRAPandROR()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_IncoTerm = "CFR";
			invoiceHeader.JZ_RX_NKInvoice_Currency = "TWD";
			invoiceHeader.JZ_InvoiceAmount = 10000m;
			invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 51m, declaration.LocalCurrencyCode);

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_Procedure = Constants.ProcedureCodes._37;
			invoiceLine.JI_InvoiceQuantity = 1m;
			invoiceLine.JI_EnteredUnitPrice = 4000m;
			invoiceLine.JI_RAPCurr = "TWD";
			invoiceLine.JI_RAPPrice = 2000m;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryLine.CL_CustomsValue = 1020m;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryLine.IsRAPOrROR, NUnit.Framework.Is.EqualTo(ZBool.True));
				NUnit.Framework.Assert.That(entryLine.CL_Calc_RAPRORUnitCurr, NUnit.Framework.Is.EqualTo("TWD").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryLine.CL_Calc_RAPRORPriceLocalAmount, NUnit.Framework.Is.EqualTo(2000m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryLine.CL_Calc_RAPRORUnitPrice, NUnit.Framework.Is.EqualTo(2000m).Using(CustomComparers.TypeComparison));

				invoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
				invoiceLine.JI_UseOneTenthCV = false;
				invoiceLine.JI_RAPCurr = "TWD";
				invoiceLine.JI_RAPPrice = 2000m;
				NUnit.Framework.Assert.That(entryLine.IsRAPOrROR, NUnit.Framework.Is.EqualTo(ZBool.True));
				NUnit.Framework.Assert.That(entryLine.CL_Calc_RAPRORUnitCurr, NUnit.Framework.Is.EqualTo("TWD").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryLine.CL_Calc_RAPRORPriceLocalAmount, NUnit.Framework.Is.EqualTo(2000m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryLine.CL_Calc_RAPRORUnitPrice, NUnit.Framework.Is.EqualTo(2000m).Using(CustomComparers.TypeComparison));

				invoiceLine.JI_Procedure = Constants.ProcedureCodes._98;
				NUnit.Framework.Assert.That(entryLine.IsRAPOrROR, NUnit.Framework.Is.EqualTo(ZBool.False));
				NUnit.Framework.Assert.That(entryLine.CL_Calc_RAPRORUnitCurr, NUnit.Framework.Is.EqualTo(ZString.Empty));
				NUnit.Framework.Assert.That(entryLine.CL_Calc_RAPRORPriceLocalAmount, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryLine.CL_Calc_RAPRORUnitPrice, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison));

				invoiceLine.JI_Procedure = Constants.ProcedureCodes._37;
				invoiceLine.JI_UseOneTenthCV = ZBool.True;
				invoiceLine.JI_RAPCurr = "USD";
				NUnit.Framework.Assert.That(entryLine.IsRAPOrROR, NUnit.Framework.Is.EqualTo(ZBool.True));
				NUnit.Framework.Assert.That(entryLine.CL_Calc_RAPRORUnitCurr, NUnit.Framework.Is.EqualTo("USD").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryLine.CL_Calc_RAPRORPriceLocalAmount, NUnit.Framework.Is.EqualTo(102m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryLine.CL_Calc_RAPRORUnitPrice, NUnit.Framework.Is.EqualTo(102m).Using(CustomComparers.TypeComparison));
			});

			CombineAssertions("UseOneTenthCV is true and CL_Calc_RAPRORUnitCurr is not local currency", () =>
			{
				invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
				invoiceHeader.IsJZ_InvoiceCurrExRateUserEnterable = true;
				invoiceHeader.JZ_InvoiceCurrExRate = 30m;
				NUnit.Framework.Assert.That(entryLine.IsRAPOrROR, NUnit.Framework.Is.EqualTo(ZBool.True));
				NUnit.Framework.Assert.That(entryLine.CL_Calc_RAPRORUnitCurr, NUnit.Framework.Is.EqualTo("USD").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryLine.CL_Calc_RAPRORPriceLocalAmount, NUnit.Framework.Is.EqualTo(102m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryLine.CL_Calc_RAPRORUnitPrice, NUnit.Framework.Is.EqualTo(3.4m).Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestIsRAP()
		{
			CombineAssertions(() =>
			{
				var entryLine = Factory.New<CusEntryLine>();
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_Procedure = Constants.ProcedureCodes._37;
				NUnit.Framework.Assert.That(entryLine.IsRAP, NUnit.Framework.Is.EqualTo(ZBool.True), "code 37 is RAP");
				invoiceLine.JI_Procedure = Constants.ProcedureCodes._98;
				NUnit.Framework.Assert.That(entryLine.IsRAP, NUnit.Framework.Is.EqualTo(ZBool.False), "code 98 is not RAP");

				entryLine = Factory.New<CusEntryLine>();
				NUnit.Framework.Assert.That(entryLine.IsRAP, NUnit.Framework.Is.EqualTo(ZBool.False), "IsRAP returns false when invoiceline is null");
			});
		}

		[ExpectNoExceptions]
		public void TestIsROR()
		{
			CombineAssertions(() =>
			{
				var entryLine = Factory.New<CusEntryLine>();
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
				NUnit.Framework.Assert.That(entryLine.IsROR, NUnit.Framework.Is.EqualTo(ZBool.True), "code 38 is IsROR");
				invoiceLine.JI_Procedure = Constants.ProcedureCodes._98;
				NUnit.Framework.Assert.That(entryLine.IsROR, NUnit.Framework.Is.EqualTo(ZBool.False), "code 98 is not ROR");

				entryLine = Factory.New<CusEntryLine>();
				NUnit.Framework.Assert.That(entryLine.IsROR, NUnit.Framework.Is.EqualTo(ZBool.False), "IsROR returns false when invoiceline is null");
			});
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_GroupingsAndGoodsDescriptions()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Default;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = (JobComInvoiceLine)invoiceHeader.JobComInvoiceLines.AddNew();
			line1.JI_CEI = instruction.PK;
			line1.JI_Tariff = "0000.00.00.00Y";
			line1.JI_Description = ZString.Empty;
			line1.JI_Group = "Car Parts";
			line1.JI_DeclarationGoodsDescription = "Wheels";
			var line2 = (JobComInvoiceLine)invoiceHeader.JobComInvoiceLines.AddNew();
			line2.JI_CEI = instruction.PK;
			line2.JI_Tariff = "0000.00.00.00Y";
			line2.JI_Description = ZString.Empty;
			line2.JI_Group = "Car Parts";
			line2.JI_DeclarationGoodsDescription = "Wheels";
			var line3 = (JobComInvoiceLine)invoiceHeader.JobComInvoiceLines.AddNew();
			line3.JI_CEI = instruction.PK;
			line3.JI_Tariff = "0000.00.00.00Y";
			line3.JI_Description = ZString.Empty;
			line3.JI_Group = "Bicycle Parts";
			line3.JI_DeclarationGoodsDescription = "Wheels";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var entryLine1 = (CusEntryLine)declaration.CustomsEntryHeaders[0].MergedLines[0];
			entryLine1.CL_Description = "Doors and Wheels";
			NUnit.Framework.Assert.That(entryLine1.CL_Calc_EntryLineGroup, NUnit.Framework.Is.EqualTo("Car Parts\r\nBicycle Parts").Using(CustomComparers.TypeComparison), "EntryLine EntryLineGroup");
			NUnit.Framework.Assert.That(entryLine1.CL_Calc_GoodsDescription, NUnit.Framework.Is.EqualTo("Car Parts\r\nBicycle Parts\r\n\r\nDoors and Wheels").Using(CustomComparers.TypeComparison), "EntryLine GoodsDescription");
			NUnit.Framework.Assert.That(entryLine1.CL_Calc_GoodsDescriptionWithoutGrouping, NUnit.Framework.Is.EqualTo("Doors and Wheels").Using(CustomComparers.TypeComparison), "EntryLine GoodsDescriptionWithoutGrouping");
			entryLine1.CL_Description = ZString.Empty;
			NUnit.Framework.Assert.That(entryLine1.CL_Calc_EntryLineGroup, NUnit.Framework.Is.EqualTo("Car Parts\r\nBicycle Parts").Using(CustomComparers.TypeComparison), "EntryLine EntryLineGroup");
			NUnit.Framework.Assert.That(entryLine1.CL_Calc_GoodsDescription, NUnit.Framework.Is.EqualTo("Car Parts\r\nBicycle Parts\r\n\r\nWheels").Using(CustomComparers.TypeComparison), "EntryLine GoodsDescription");
			NUnit.Framework.Assert.That(entryLine1.CL_Calc_GoodsDescriptionWithoutGrouping, NUnit.Framework.Is.EqualTo("Wheels").Using(CustomComparers.TypeComparison), "EntryLine GoodsDescriptionWithoutGrouping");
			line1.JI_Group = "";
			line1.JI_DeclarationGoodsDescription = "";
			entryLine1.CL_Description = "Doors and Wheels";
			NUnit.Framework.Assert.That(entryLine1.CL_Calc_EntryLineGroup, NUnit.Framework.Is.EqualTo("Car Parts\r\nBicycle Parts").Using(CustomComparers.TypeComparison), "EntryLine EntryLineGroup");
			NUnit.Framework.Assert.That(entryLine1.CL_Calc_GoodsDescription, NUnit.Framework.Is.EqualTo("Car Parts\r\nBicycle Parts\r\n\r\nDoors and Wheels").Using(CustomComparers.TypeComparison), "EntryLine GoodsDescription");
			NUnit.Framework.Assert.That(entryLine1.CL_Calc_GoodsDescriptionWithoutGrouping, NUnit.Framework.Is.EqualTo("Doors and Wheels").Using(CustomComparers.TypeComparison), "EntryLine GoodsDescriptionWithoutGrouping");
		}

		[ExpectNoExceptions]
		public void TestCL_EntryLineQty_Caption()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(entryLine.CL_EntryLineQtyInfo);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(resourceStringData.Caption, NUnit.Framework.Is.EqualTo("Quantity"), "Caption");
			});
		}

		[ExpectNoExceptions]
		public void TestCL_EntryLineUQDescription_Caption()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(entryLine.CL_EntryLineUQDescriptionInfo);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(resourceStringData.Caption, NUnit.Framework.Is.EqualTo("UQ Description"), "Caption");
				NUnit.Framework.Assert.That(resourceStringData.ShortCaption, NUnit.Framework.Is.EqualTo("UQ Desc."), "Short Caption");
			});
		}

		[ExpectNoExceptions]
		public void TestCL_CustomsValue_Caption()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(entryLine.CL_CustomsValueInfo);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(resourceStringData.Caption, NUnit.Framework.Is.EqualTo("Customs Value"), "Caption");
			});
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_NetWeightInKG_Caption()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(entryLine.CL_Calc_NetWeightInKGInfo);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(resourceStringData.Caption, NUnit.Framework.Is.EqualTo("Net Weight (KG)"), "Caption");
				NUnit.Framework.Assert.That(resourceStringData.ShortCaption, NUnit.Framework.Is.EqualTo("Net Wgt. (KG)"), "Short Caption");
			});
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_CustomsSecondUnitQty_Caption()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(entryLine.CL_CustomsSecondUnitQtyInfo);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(resourceStringData.Caption, NUnit.Framework.Is.EqualTo("Statistical Quantity Unit"), "Caption");
				NUnit.Framework.Assert.That(resourceStringData.ShortCaption, NUnit.Framework.Is.EqualTo("UQ"), "Short Caption");
			});
		}

		[ExpectNoExceptions]
		public void TestCL_Calc_CustomsSecondQuantity_Caption()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(entryLine.CL_Calc_CustomsSecondQuantityInfo);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(resourceStringData.Caption, NUnit.Framework.Is.EqualTo("Statistical Quantity"), "Caption");
				NUnit.Framework.Assert.That(resourceStringData.ShortCaption, NUnit.Framework.Is.EqualTo("Stats. Qty"), "Short Caption");
			});
		}

		protected override void DoMerge(BaseJobDeclaration declaration)
		{
			SetupDataEligibleForMerging(declaration);
			base.DoMerge(declaration);
		}

		protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection);

		void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		}

		protected override ZString ExpectedFallbackEntrylineDescription => "LINE";
		CusEntryLine CreateEntryLine()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var comRateType = helper.CreateCusRateType("TW", "COM");
			comRateType.ZZR_CustomsValueFormula = "CV + DTA + DTS";
			helper.LoadOrCreateNewCusRateCode(Factory, "CTA", comRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "CTS", comRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "TAT", comRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "HWS", comRateType.PK);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			var lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = "CTA";
			lineFee.CF_ChargeAmount = 100M;
			lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = "TAT";
			lineFee.CF_ChargeAmount = 200M;
			lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = "AA";
			lineFee.CF_ChargeAmount = 300M;
			var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = entryHeader.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = SharedJobMessageTypeList.Codes.Import;
			cusNum1.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "NO1";
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CL = entryLine.PK;
			var invoiceLine1 = Factory.New<JobComInvoiceLine>();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine.JI_Group = "group";
			invoiceLine.JI_Description = "description";
			invoiceLine.JI_ModelYear = 2015;
			invoiceLine.JI_CarType = "A1";
			invoiceLine.JI_Transmission = "B";
			invoiceLine.JI_Gears = 2;
			invoiceLine.JI_NumberOfDoor = 5;
			invoiceLine.JI_BrandName = "brandname";
			invoiceLine.JI_Model = "model";
			invoiceLine.JI_Cylinders = 3;
			invoiceLine.JI_Displacement = "500";
			invoiceLine.JI_LHD = "Y";
			invoiceLine.JI_EngineType = "CG";
			invoiceLine.JI_Seats = 5;
			invoiceLine.JI_CarCondition = "1";
			invoiceLine.JI_Transmission = "A";
			invoiceLine.JI_BondedGoodsCode = BondedGoodsCodeList.Codes.NB;
			var chassis1 = invoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
			chassis1.JG_ReferenceNumber = "123456";
			var chassis2 = invoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
			chassis2.JG_ReferenceNumber = "1234567";
			var chassis3 = invoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
			chassis3.JG_ReferenceNumber = "1234567";
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsSecondUnitQty = "TNE";
			invoiceLine.JI_CustomsSecondQuantity = 2m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_EPTDigit1 = ContainerMaterialList.Codes.Z;
			invoiceLine.JI_EPTDigit2 = ContainerCapacityList.Codes._0;
			invoiceLine.JI_EPTDigit3 = ContainerMaterialNumberList.Codes._0;
			invoiceLine1.JI_NetWeight = 2000m;
			invoiceLine1.JI_NetWeightUQ = "G";
			invoiceLine1.JI_InvoiceQuantity = 100m;
			invoiceLine.JI_InvoiceQuantity = 200m;
			invoiceLine1.JI_InvoiceUQ = "AAA";
			invoiceLine.JI_InvoiceUQ = "AAA";
			return entryLine;
		}
	}
}
