using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USImportEntryLine))]
	sealed class USImportEntryLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIDrawbackEntryLineMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(-1);
			declaration.US_SchDEntry = "4009";

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_PartNo = "PART2";
			invoiceLine1.JI_Tariff = "10000000";
			invoiceLine1.JI_InvoiceUQ = "N5";
			invoiceLine1.JI_InvoiceQuantity = 50m;
			invoiceLine1.JI_CustomsUnitQty = "N1";
			invoiceLine1.JI_CustomsQuantity = 10m;
			invoiceLine1.JI_CustomsSecondUnitQty = "N2";
			invoiceLine1.JI_CustomsSecondQuantity = 20m;
			invoiceLine1.JI_CustomsThirdUnitQty = "N3";
			invoiceLine1.JI_CustomsThirdQuantity = 30m;
			invoiceLine1.US_SupTariff = "10000001";
			invoiceLine1.US_SupUQ1 = "S1";
			invoiceLine1.US_SupQty1 = 11m;
			invoiceLine1.US_SupUQ2 = "S2";
			invoiceLine1.US_SupQty2 = 22m;
			invoiceLine1.US_SupUQ3 = "S3";
			invoiceLine1.US_SupQty3 = 33m;
			invoiceLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.F;
			invoiceLine1.US_SPI = SpecialProgramList.Codes.AU;
			invoiceLine1.US_UC_NKCountryOfOrigin = "TT";
			invoiceLine1.US_PayableMPF = 40000m;
			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = "PART1";
			invoiceLine2.JI_Tariff = "10000000";
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.G;
			invoiceLine2.US_SPI = PrimarySpecProgramIndicatorList.Codes.D;
			invoiceLine2.US_UC_NKCountryOfOrigin = "EK";
			invoiceLine2.US_PayableMPF = 4000m;
			var invoiceLine3 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_ParentID = invoiceLine1.PK;
			invoiceLine3.JI_PartNo = "PART3";
			invoiceLine3.JI_Tariff = "30000000";
			invoiceLine3.JI_InvoiceUQ = "N6";
			invoiceLine3.JI_InvoiceQuantity = 60m;
			invoiceLine3.JI_CustomsUnitQty = "N7";
			invoiceLine3.JI_CustomsQuantity = 70m;
			invoiceLine3.JI_CustomsSecondUnitQty = "N8";
			invoiceLine3.JI_CustomsSecondQuantity = 80m;
			invoiceLine3.JI_CustomsThirdUnitQty = "N9";
			invoiceLine3.JI_CustomsThirdQuantity = 90m;
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.M;
			invoiceLine3.US_SPI = PrimarySpecProgramIndicatorList.Codes.A;
			invoiceLine3.US_UC_NKCountryOfOrigin = "ID";
			invoiceLine3.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			var dateForDutyRate = ZDate.Today.AddDays(-1);
			var privilegedStatusDate = dateForDutyRate.AddDays(-1);
			invoiceLine3.US_PrivilegedStatusDate = privilegedStatusDate;
			invoiceLine3.US_PayableMPF = 400m;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV2";
			var invoice2Line1 = invoice1.JobComInvoiceLines.AddNew();
			invoice2Line1.JI_Tariff = "60000000";
			invoice2Line1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.F;
			invoice2Line1.US_SPI = SpecialProgramList.Codes.AU;
			invoice2Line1.US_UC_NKCountryOfOrigin = "TT";
			invoice2Line1.US_PayableMPF = 40m;

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.US_DutyCalcDate = dateForDutyRate;
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_AdValoremTariff = "10000000";
			entryLine1.US_DutyRateDesc = "DUTY DESC";
			entryLine1.CL_CustomsValue = 50000m;
			entryLine1.US_HasMPF = ZBool.True;
			entryLine1.CL_Description = "DESC 1";
			var entrySupLine1 = entry.MergedLines.AddNew();
			entrySupLine1.CL_LineNumber = 2;
			entrySupLine1.CL_AdValoremTariff = "10000001";
			entrySupLine1.US_SupLine = ZBool.True;
			entrySupLine1.CL_CustomsValue = 5000m;
			entrySupLine1.CL_Description = "DESC SUP 1";
			entrySupLine1.US_CL_ParentLine = entryLine1.PK;
			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 3;
			entryLine2.CL_AdValoremTariff = "30000000";
			entryLine2.CL_CustomsValue = 500m;
			entryLine2.CL_Description = "DESC 2";
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entrySupLine1);
			invoiceLine2.JI_CL = entryLine1.PK;
			invoiceLine3.JI_CL = entryLine2.PK;
			invoice2Line1.JI_CL = entryLine1.PK;
			declaration.CalculateTotalEnteredValue();

			var entryCharge1 = entry.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 100m);
			var entryCharge2 = entry.Charges.AddNew();
			entryCharge2.C1_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			entryCharge2.C1_ChargeAmount = 10m;
			var entryCharge3 = entry.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.HMF, 200m);
			var entryCharge4 = entry.Charges.AddNew();
			entryCharge4.C1_ChargeType = Core.Constants.USCustoms.FeeCodes.HMF;
			entryCharge4.C1_ChargeAmount = 20m;

			var entryLine1Fee1 = entryLine1.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.OtherExcise, 1m);
			var entryLine1Fee2 = entryLine1.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, 10m);
			var entryLine1Fee3 = entryLine1.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Tobacco, 100m);
			var entryLine1Fee4 = entryLine1.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Wines, 1000m);
			var entryLine1Fee5 = entryLine1.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Avocado, 10000m);
			var entryLine1Fee6 = entryLine1.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 100000m);
			var entryLine1Fee7 = entryLine1.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.HMF, 1000000m);
			var entryLine1Fee8 = entryLine1.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 10000000m);

			var entrySupLine1Fee1 = entrySupLine1.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Wines, 3m, ZBool.True);
			var entrySupLine1Fee2 = entrySupLine1.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Avocado, 30m, ZBool.True);
			var entrySupLine1Fee3 = entrySupLine1.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 300m, ZBool.True);
			var entrySupLine1Fee4 = entrySupLine1.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.HMF, 3000m, ZBool.True);
			var entrySupLine1Fee5 = entrySupLine1.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 30000m, ZBool.True);

			var entryLine2Fee1 = entryLine2.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 2m);
			var entryLine2Fee2 = entryLine2.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 200m);
			var entryLine2Fee3 = entryLine2.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.OtherExcise, 2000m);
			var entryLine2Fee4 = entryLine2.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Beef, 20000m);

			Factory.Save();
			AssertIDrawbackEntryLine(entryLine1.PK, entrySupLine1.PK, entryLine2.PK, entryLine1, entrySupLine1, entryLine2,
				new IFee[]
				{
					entryLine1Fee1, entryLine1Fee2, entryLine1Fee3, entryLine1Fee4,
					entryLine1Fee5, entryLine1Fee6, entryLine1Fee7, entryLine1Fee8
				},
				new IFee[]
				{
					entrySupLine1Fee1, entrySupLine1Fee2, entrySupLine1Fee3, entrySupLine1Fee4, entrySupLine1Fee5
				},
				new IFee[]
				{
					entryLine2Fee1, entryLine2Fee2, entryLine2Fee3, entryLine2Fee4
				}, invoiceLine1, invoiceLine2, invoiceLine3, invoice2Line1);

			var factory = new BusinessObjectFactory();
			var importEntryLine1 = factory.Load<USImportEntryLine>(entryLine1.PK);
			var importEntrySupLine1 = factory.Load<USImportEntryLine>(entrySupLine1.PK);
			var importEntryLine2 = factory.Load<USImportEntryLine>(entryLine2.PK);

			var importEntryLine1Fee1 = factory.Load<USImportEntryLineFee>(entryLine1Fee1.PK);
			var importEntryLine1Fee2 = factory.Load<USImportEntryLineFee>(entryLine1Fee2.PK);
			var importEntryLine1Fee3 = factory.Load<USImportEntryLineFee>(entryLine1Fee3.PK);
			var importEntryLine1Fee4 = factory.Load<USImportEntryLineFee>(entryLine1Fee4.PK);
			var importEntryLine1Fee5 = factory.Load<USImportEntryLineFee>(entryLine1Fee5.PK);
			var importEntryLine1Fee6 = factory.Load<USImportEntryLineFee>(entryLine1Fee6.PK);
			var importEntryLine1Fee7 = factory.Load<USImportEntryLineFee>(entryLine1Fee7.PK);
			var importEntryLine1Fee8 = factory.Load<USImportEntryLineFee>(entryLine1Fee8.PK);

			var importEntrySupLine1Fee1 = factory.Load<USImportEntryLineFee>(entrySupLine1Fee1.PK);
			var importEntrySupLine1Fee2 = factory.Load<USImportEntryLineFee>(entrySupLine1Fee2.PK);
			var importEntrySupLine1Fee3 = factory.Load<USImportEntryLineFee>(entrySupLine1Fee3.PK);
			var importEntrySupLine1Fee4 = factory.Load<USImportEntryLineFee>(entrySupLine1Fee4.PK);
			var importEntrySupLine1Fee5 = factory.Load<USImportEntryLineFee>(entrySupLine1Fee5.PK);

			var importEntryLine2Fee1 = factory.Load<USImportEntryLineFee>(entryLine2Fee1.PK);
			var importEntryLine2Fee2 = factory.Load<USImportEntryLineFee>(entryLine2Fee2.PK);
			var importEntryLine2Fee3 = factory.Load<USImportEntryLineFee>(entryLine2Fee3.PK);
			var importEntryLine2Fee4 = factory.Load<USImportEntryLineFee>(entryLine2Fee4.PK);

			var importEntryInvoiceLine1 = factory.Load<USImportEntryInvoiceLine>(invoiceLine1.PK);
			var importEntryInvoiceLine2 = factory.Load<USImportEntryInvoiceLine>(invoiceLine2.PK);
			var importEntryInvoiceLine3 = factory.Load<USImportEntryInvoiceLine>(invoiceLine3.PK);
			var importEntryInvoice2Line1 = factory.Load<USImportEntryInvoiceLine>(invoice2Line1.PK);
			AssertIDrawbackEntryLine(entryLine1.PK, entrySupLine1.PK, entryLine2.PK, importEntryLine1, importEntrySupLine1, importEntryLine2,
				new IFee[]
				{
					importEntryLine1Fee1, importEntryLine1Fee2, importEntryLine1Fee3, importEntryLine1Fee4,
					importEntryLine1Fee5, importEntryLine1Fee6, importEntryLine1Fee7, importEntryLine1Fee8
				},
				new IFee[]
				{
					importEntrySupLine1Fee1, importEntrySupLine1Fee2, importEntrySupLine1Fee3, importEntrySupLine1Fee4, importEntrySupLine1Fee5
				},
				new IFee[]
				{
					importEntryLine2Fee1, importEntryLine2Fee2, importEntryLine2Fee3, importEntryLine2Fee4
				}, importEntryInvoiceLine1, importEntryInvoiceLine2, importEntryInvoiceLine3, importEntryInvoice2Line1);
		}

		void AssertIDrawbackEntryLine(ZGuid entryLine1PK, ZGuid entrySupLine1PK, ZGuid entryLine2PK, IDrawbackEntryLine drawbackEntryLine1, IDrawbackEntryLine drawbackEntrySupLine1, IDrawbackEntryLine drawbackEntryLine2, IFee[] drawbackEntryLine1Fees, IFee[] drawbackEntrySupLine1Fees, IFee[] drawbackEntryLine2Fees, IInvoiceLine importEntryInvoiceLine1, IInvoiceLine importEntryInvoiceLine2, IInvoiceLine importEntryInvoiceLine3, IInvoiceLine importEntryInvoice2Line1)
		{
			var isCusEntryLine = drawbackEntryLine1 is CusEntryLine;
			var allDrawbackEntryLines = new IDrawbackEntryLine[] { drawbackEntryLine1, drawbackEntrySupLine1, drawbackEntryLine2 };
			var drawbackEntryLines = allDrawbackEntryLines.ToList();
			CombineAssertions("IDrawbackEntryLine", () =>
			{
				if (isCusEntryLine)
				{
					AssertArrayEqualsByElements("drawbackEntryLine2.AllDrawbackEntryLines", allDrawbackEntryLines, drawbackEntryLine2.AllDrawbackEntryLines);
					AssertArrayEqualsByElements("drawbackEntryLine1.AllDrawbackEntryLines", allDrawbackEntryLines, drawbackEntryLine1.AllDrawbackEntryLines);
				}
				else
				{
					AssertSame("AllDrawbackEntryLines is cached", drawbackEntryLine1.AllDrawbackEntryLines, drawbackEntryLine2.AllDrawbackEntryLines);
					AssertSame("AllDrawbackEntryLines is cached", drawbackEntryLine1.AllDrawbackEntryLines, drawbackEntrySupLine1.AllDrawbackEntryLines);
				}
				AssertArrayEqualsByElements("AllDrawbackEntryLines", allDrawbackEntryLines, drawbackEntryLine1.AllDrawbackEntryLines);
				var assertVoid = new Action<IDrawbackEntryLine>((drawbackEntryLine) =>
				{
					var lineNumber = drawbackEntryLine.CL_LineNumber.ToString();
					AssertEquals($"{lineNumber}-EntryDate", ZDateTime.Today.AddDays(-1), drawbackEntryLine.EntryDate);
					AssertEquals($"{lineNumber}-EntryPort", "4009", drawbackEntryLine.EntryPort);
					AssertEquals($"{lineNumber}-InvoiceNumber", "INV1", drawbackEntryLine.InvoiceNumber);
					AssertEquals($"{lineNumber}-PartNo", "PART2", drawbackEntryLine.PartNo);
					AssertEquals($"{lineNumber}-TotalEnteredValueForEntry", 55500m, drawbackEntryLine.TotalEnteredValueForEntry);
					AssertEquals($"{lineNumber}-MPFAmountForEntry", 100m, drawbackEntryLine.MPFAmountForEntry);
					AssertEquals($"{lineNumber}-HMFAmountForEntry", 200m, drawbackEntryLine.HMFAmountForEntry);
				});
				drawbackEntryLines.ForEach(assertVoid);

				AssertEquals("drawbackEntryLine1.DutyRateDescription", "DUTY DESC", drawbackEntryLine1.DutyRateDescription);
				AssertEquals("drawbackEntryLine1.SecondCustomsUnitQty", "N2", drawbackEntryLine1.SecondCustomsUnitQty);
				AssertEquals("drawbackEntryLine1.ThirdCustomsUnitQty", "N3", drawbackEntryLine1.ThirdCustomsUnitQty);
				AssertEquals("drawbackEntryLine1.ExciseTax", 1111m, drawbackEntryLine1.ExciseTax);
				AssertEquals("drawbackEntryLine1.HMFAmount", 1000000m, drawbackEntryLine1.HMFAmount);
				AssertEquals("drawbackEntryLine1.US_HasMPF", ZBool.True, drawbackEntryLine1.US_HasMPF);
				AssertEquals("drawbackEntryLine1.MPFAmount", 10000000m, drawbackEntryLine1.MPFAmount);
				AssertEquals("drawbackEntryLine1.PayableMPFAmount", 44040m, drawbackEntryLine1.PayableMPFAmount);
				AssertEquals("drawbackEntryLine1.TotalFeeAmount", 11111111m, drawbackEntryLine1.TotalFeeAmount);
				AssertEquals("drawbackEntryLine1.SecondCustomsQuantity", 20m, drawbackEntryLine1.SecondCustomsQuantity);
				AssertEquals("drawbackEntryLine1.ThirdCustomsQuantity", 30m, drawbackEntryLine1.ThirdCustomsQuantity);
				AssertEquals("drawbackEntryLine1.IsSecondaryTariffLine", ZBool.False, drawbackEntryLine1.IsSecondaryTariffLine);
				AssertNull("drawbackEntryLine1.ParentLine", drawbackEntryLine1.ParentLine);
				if (!isCusEntryLine)
				{
					AssertSame("drawbackEntryLine1.Fees is cached", drawbackEntryLine1.Fees, drawbackEntryLine1.Fees);
				}
				Assert("drawbackEntryLine1.Fees is not same as drawbackEntryLine2.Fees", !object.ReferenceEquals(drawbackEntryLine1.Fees, drawbackEntryLine2.Fees));
				Assert("drawbackEntryLine1.Fees is not same as drawbackEntrySupLine1.Fees", !object.ReferenceEquals(drawbackEntryLine1.Fees, drawbackEntrySupLine1.Fees));
				if (isCusEntryLine)
				{
					AssertContainsExactElementsInAnyOrder("drawbackEntryLine1.Fees", drawbackEntryLine1Fees.OrderBy(x => x.Code).ToArray(), drawbackEntryLine1.Fees.ToArray());
				}
				else
				{
					AssertArrayEqualsByElements("drawbackEntryLine1.Fees", drawbackEntryLine1Fees.OrderBy(x => x.Code).ToArray(), drawbackEntryLine1.Fees.ToArray());
				}
				AssertSame("drawbackEntryLine1.ChildSecondaryEntryLines.Single()", drawbackEntrySupLine1, drawbackEntryLine1.ChildSecondaryEntryLines.Single());
				AssertEquals("drawbackEntryLine1.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise)", 1m, drawbackEntryLine1.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise));
				AssertEquals("drawbackEntryLine1.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.DistilledSpirits)", 10m, drawbackEntryLine1.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.DistilledSpirits));
				AssertEquals("drawbackEntryLine1.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.Tobacco)", 100m, drawbackEntryLine1.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.Tobacco));
				AssertEquals("drawbackEntryLine1.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.Wines)", 1000m, drawbackEntryLine1.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.Wines));
				AssertEquals("drawbackEntryLine1.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.Avocado)", 10000m, drawbackEntryLine1.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.Avocado));
				AssertEquals("drawbackEntryLine1.GetFeeAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount)", 100000m, drawbackEntryLine1.GetFeeAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount));
				AssertEquals("drawbackEntryLine1.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.HMF)", 1000000m, drawbackEntryLine1.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.HMF));
				AssertEquals("drawbackEntryLine1.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing)", 10000000m, drawbackEntryLine1.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

				AssertEquals("drawbackEntrySupLine1.DutyRateDescription", DutyResult.DutyFreeString, drawbackEntrySupLine1.DutyRateDescription);
				AssertEquals("drawbackEntrySupLine1.SecondCustomsUnitQty", "S2", drawbackEntrySupLine1.SecondCustomsUnitQty);
				AssertEquals("drawbackEntrySupLine1.ThirdCustomsUnitQty", "S3", drawbackEntrySupLine1.ThirdCustomsUnitQty);
				AssertEquals("drawbackEntrySupLine1.ExciseTax", 3m, drawbackEntrySupLine1.ExciseTax);
				AssertEquals("drawbackEntrySupLine1.HMFAmount", 0m, drawbackEntrySupLine1.HMFAmount);
				AssertEquals("drawbackEntrySupLine1.US_HasMPF", ZBool.False, drawbackEntrySupLine1.US_HasMPF);
				AssertEquals("drawbackEntrySupLine1.MPFAmount", 0m, drawbackEntrySupLine1.MPFAmount);
				AssertEquals("drawbackEntrySupLine1.PayableMPFAmount", 40000m, drawbackEntrySupLine1.PayableMPFAmount);
				AssertEquals("drawbackEntrySupLine1.TotalFeeAmount", 33333m, drawbackEntrySupLine1.TotalFeeAmount);
				AssertEquals("drawbackEntrySupLine1.SecondCustomsQuantity", 22m, drawbackEntrySupLine1.SecondCustomsQuantity);
				AssertEquals("drawbackEntrySupLine1.ThirdCustomsQuantity", 33m, drawbackEntrySupLine1.ThirdCustomsQuantity);
				AssertEquals("drawbackEntrySupLine1.IsSecondaryTariffLine", ZBool.True, drawbackEntrySupLine1.IsSecondaryTariffLine);
				AssertEquals("drawbackEntrySupLine1.ParentLine", drawbackEntryLine1, drawbackEntrySupLine1.ParentLine);
				if (isCusEntryLine)
				{
					AssertContainsExactElementsInAnyOrder("drawbackEntrySupLine1.Fees", drawbackEntrySupLine1Fees.OrderBy(x => x.Code).ToArray(), drawbackEntrySupLine1.Fees.ToArray());
				}
				else
				{
					AssertSame("drawbackEntrySupLine1.Fees is cached", drawbackEntrySupLine1.Fees, drawbackEntrySupLine1.Fees);
					AssertArrayEqualsByElements("drawbackEntrySupLine1.Fees", drawbackEntrySupLine1Fees.OrderBy(x => x.Code).ToArray(), drawbackEntrySupLine1.Fees.ToArray());
				}
				AssertEquals("drawbackEntrySupLine1.ChildSecondaryEntryLines.Any()", false, drawbackEntrySupLine1.ChildSecondaryEntryLines.Any());
				AssertEquals("drawbackEntrySupLine1.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.Wines)", 0m, drawbackEntrySupLine1.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.Wines));
				AssertEquals("drawbackEntrySupLine1.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.Avocado)", 0m, drawbackEntrySupLine1.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.Avocado));
				AssertEquals("drawbackEntrySupLine1.GetFeeAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount)", 0m, drawbackEntrySupLine1.GetFeeAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount));
				AssertEquals("drawbackEntrySupLine1.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.HMF)", 0m, drawbackEntrySupLine1.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.HMF));
				AssertEquals("drawbackEntrySupLine1.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing)", 0m, drawbackEntrySupLine1.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

				AssertEquals("drawbackEntryLine2.DutyRateDescription", "", drawbackEntryLine2.DutyRateDescription);
				AssertEquals("drawbackEntryLine2.SecondCustomsUnitQty", "N8", drawbackEntryLine2.SecondCustomsUnitQty);
				AssertEquals("drawbackEntryLine2.ThirdCustomsUnitQty", "N9", drawbackEntryLine2.ThirdCustomsUnitQty);
				AssertEquals("drawbackEntryLine2.ExciseTax", 2000m, drawbackEntryLine2.ExciseTax);
				AssertEquals("drawbackEntryLine2.HMFAmount", 0m, drawbackEntryLine2.HMFAmount);
				AssertEquals("drawbackEntryLine2.US_HasMPF", ZBool.False, drawbackEntryLine2.US_HasMPF);
				AssertEquals("drawbackEntryLine2.MPFAmount", 200m, drawbackEntryLine2.MPFAmount);
				AssertEquals("drawbackEntryLine2.PayableMPFAmount", 400m, drawbackEntryLine2.PayableMPFAmount);
				AssertEquals("drawbackEntryLine2.TotalFeeAmount", 22202m, drawbackEntryLine2.TotalFeeAmount);
				AssertEquals("drawbackEntryLine2.SecondCustomsQuantity", 80m, drawbackEntryLine2.SecondCustomsQuantity);
				AssertEquals("drawbackEntryLine2.ThirdCustomsQuantity", 90m, drawbackEntryLine2.ThirdCustomsQuantity);
				AssertEquals("drawbackEntryLine2.IsSecondaryTariffLine", ZBool.False, drawbackEntryLine2.IsSecondaryTariffLine);
				AssertNull("drawbackEntryLine2.ParentLine", drawbackEntryLine2.ParentLine);
				if (isCusEntryLine)
				{
					AssertContainsExactElementsInAnyOrder("drawbackEntryLine2.Fees", drawbackEntryLine2Fees.OrderBy(x => x.Code).ToArray(), drawbackEntryLine2.Fees.ToArray());
				}
				else
				{
					AssertSame("drawbackEntryLine2.Fees is cached", drawbackEntryLine2.Fees, drawbackEntryLine2.Fees);
				}
				AssertEquals("drawbackEntryLine2.ChildSecondaryEntryLines.Any()", false, drawbackEntryLine2.ChildSecondaryEntryLines.Any());
				AssertEquals("drawbackEntryLine2.GetFeeAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount)", 2m, drawbackEntryLine2.GetFeeAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount));
				AssertEquals("drawbackEntryLine2.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing)", 200m, drawbackEntryLine2.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
				AssertEquals("drawbackEntryLine2.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise)", 2000m, drawbackEntryLine2.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise));
				AssertEquals("drawbackEntryLine2.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.Beef)", 20000m, drawbackEntryLine2.GetFeeAmount(Core.Constants.USCustoms.FeeCodes.Beef));
			});
			CombineAssertions("IDrawbackEntryLine", () =>
			{
				AssertIBaseEntryLine(drawbackEntryLine1, entryLine1PK, 1, "10000000", 50000m, 100000m);
				AssertIBaseEntryLine(drawbackEntrySupLine1, entrySupLine1PK, 2, "10000001", 5000m, 0m);
				AssertIBaseEntryLine(drawbackEntryLine2, entryLine2PK, 3, "30000000", 500m, 2);
			});
			CombineAssertions("IBaseDrawbackEntryLine", () =>
			{
				AssertIBaseDrawbackEntryLine(drawbackEntryLine1, "DESC 1", 50m, "N5", 10m, "N1");
				AssertIBaseDrawbackEntryLine(drawbackEntrySupLine1, "DESC SUP 1", 50m, "N5", 11m, "S1");
				AssertIBaseDrawbackEntryLine(drawbackEntryLine2, "DESC 2", 60m, "N6", 70m, "N7");
			});

			IEntryLine iEntryLine1 = drawbackEntryLine1;
			IEntryLine iEntrySupLine1 = drawbackEntrySupLine1;
			IEntryLine iEntryLine2 = drawbackEntryLine2;
			CombineAssertions("IEntryLine", () =>
			{
				var entryLines = new IEntryLine[] { iEntryLine1, iEntrySupLine1, iEntryLine2 };

				if (isCusEntryLine)
				{
					AssertArrayEqualsByElements("iEntryLine2.AllRelatedEntryLines", entryLines, iEntryLine2.AllRelatedEntryLines.ToArray());
					AssertArrayEqualsByElements("iEntrySupLine1.AllRelatedEntryLines", entryLines, iEntrySupLine1.AllRelatedEntryLines.ToArray());
				}
				else
				{
					AssertSame("AllRelatedEntryLines is cached", iEntryLine1.AllRelatedEntryLines, iEntryLine2.AllRelatedEntryLines);
					AssertSame("AllRelatedEntryLines is cached", iEntryLine1.AllRelatedEntryLines, iEntrySupLine1.AllRelatedEntryLines);
				}
				AssertArrayEqualsByElements("AllRelatedEntryLines", entryLines, iEntryLine1.AllRelatedEntryLines.ToArray());
				var assertVoid = new Action<IEntryLine>((entryLine) =>
				{
					var lineNumber = entryLine.CL_LineNumber.ToString();
					AssertEquals($"{lineNumber}-IsCustomsChargeToBeCalculated", true, entryLine.IsCustomsChargeToBeCalculated);
					AssertEquals($"{lineNumber}-IsACE", true, entryLine.IsACE);
					AssertEquals($"{lineNumber}-IsConsumptionFTZ", false, entryLine.IsConsumptionFTZ);
					AssertEquals($"{lineNumber}-IsFTZAdmission", false, entryLine.IsFTZAdmission);
				});
				drawbackEntryLines.ForEach(assertVoid);

				var importEntryInvoiceLines = new[] { importEntryInvoiceLine1, importEntryInvoiceLine2, importEntryInvoice2Line1 };
				AssertEquals("iEntryLine1.BaseDutyRateDesc", "DUTY DESC", iEntryLine1.BaseDutyRateDesc);
				AssertEquals("iEntryLine1.NoDutyRateExists", false, iEntryLine1.NoDutyRateExists);
				AssertEquals("iEntryLine1.US_SupLine", false, iEntryLine1.US_SupLine);
				AssertEquals("iEntryLine1.US_CL_ParentLine", ZGuid.Empty, iEntryLine1.US_CL_ParentLine);
				AssertEquals("iEntryLine1.IsSetXLine", false, iEntryLine1.IsSetXLine);
				AssertEquals("iEntryLine1.IsSetVLine", false, iEntryLine1.IsSetVLine);
				AssertEquals("iEntryLine1.RoundedCustomsValue", 50000m, iEntryLine1.RoundedCustomsValue);
				AssertNull("iEntryLine1.ParentLine", iEntryLine1.ParentLine);
				AssertEquals($"iEntryLine1.FirstInvoiceLineAfterSortedOnInvoiceLineNo ({importEntryInvoiceLine1.JI_LineNo}, {iEntryLine1.FirstInvoiceLineAfterSortedOnInvoiceLineNo.JI_LineNo})", importEntryInvoiceLine1, iEntryLine1.FirstInvoiceLineAfterSortedOnInvoiceLineNo);
				AssertEquals("iEntryLine1.RandomLine", importEntryInvoiceLine1, iEntryLine1.RandomLine);
				if (!isCusEntryLine)
				{
					AssertSame("iEntryLine1.InvoiceLines is cached.", iEntryLine1.InvoiceLines, iEntryLine1.InvoiceLines);
				}
				AssertArrayEqualsByElements("iEntryLine1.InvoiceLines", importEntryInvoiceLines, iEntryLine1.InvoiceLines.ToArray());
				AssertEquals("iEntryLine1.ChildSecondaryEntryLines.Single()", iEntrySupLine1, iEntryLine1.ChildSecondaryEntryLines.Single());

				AssertEquals("iEntrySupLine1.BaseDutyRateDesc", "", iEntrySupLine1.BaseDutyRateDesc);
				AssertEquals("iEntrySupLine1.NoDutyRateExists", false, iEntrySupLine1.NoDutyRateExists);
				AssertEquals("iEntrySupLine1.US_SupLine", true, iEntrySupLine1.US_SupLine);
				AssertEquals("iEntrySupLine1.US_CL_ParentLine", entryLine1PK, iEntrySupLine1.US_CL_ParentLine);
				AssertEquals("iEntrySupLine1.IsSetXLine", false, iEntrySupLine1.IsSetXLine);
				AssertEquals("iEntrySupLine1.IsSetVLine", false, iEntrySupLine1.IsSetVLine);
				AssertEquals("iEntrySupLine1.RoundedCustomsValue", 5000m, iEntrySupLine1.RoundedCustomsValue);
				AssertSame("iEntrySupLine1.ParentLine", iEntryLine1, iEntrySupLine1.ParentLine);
				AssertEquals($"iEntrySupLine1.FirstInvoiceLineAfterSortedOnInvoiceLineNo ({importEntryInvoiceLine1.JI_LineNo}, {iEntrySupLine1.FirstInvoiceLineAfterSortedOnInvoiceLineNo.JI_LineNo})", importEntryInvoiceLine1, iEntrySupLine1.FirstInvoiceLineAfterSortedOnInvoiceLineNo);
				AssertEquals("iEntrySupLine1.RandomLine", importEntryInvoiceLine1, iEntrySupLine1.RandomLine);
				if (!isCusEntryLine)
				{
					AssertSame("iEntrySupLine1.InvoiceLines is cached.", iEntrySupLine1.InvoiceLines, iEntrySupLine1.InvoiceLines);
				}
				AssertArrayEqualsByElements("iEntrySupLine1.InvoiceLines", new[] { importEntryInvoiceLine1 }, iEntrySupLine1.InvoiceLines.ToArray());
				AssertEquals("iEntrySupLine1.ChildSecondaryEntryLines.Any()", false, iEntrySupLine1.ChildSecondaryEntryLines.Any());

				AssertEquals("iEntryLine2.BaseDutyRateDesc", "", iEntryLine2.BaseDutyRateDesc);
				AssertEquals("iEntryLine2.NoDutyRateExists", false, iEntryLine2.NoDutyRateExists);
				AssertEquals("iEntryLine2.US_SupLine", false, iEntryLine2.US_SupLine);
				AssertEquals("iEntryLine2.US_CL_ParentLine", ZGuid.Empty, iEntryLine2.US_CL_ParentLine);
				AssertEquals("iEntryLine2.IsSetXLine", false, iEntryLine2.IsSetXLine);
				AssertEquals("iEntryLine2.IsSetVLine", false, iEntryLine2.IsSetVLine);
				AssertEquals("iEntryLine2.RoundedCustomsValue", 500m, iEntryLine2.RoundedCustomsValue);
				AssertNull("iEntryLine2.ParentLine", iEntryLine2.ParentLine);
				AssertEquals($"iEntryLine2.FirstInvoiceLineAfterSortedOnInvoiceLineNo ({importEntryInvoiceLine3.JI_LineNo}, {iEntryLine2.FirstInvoiceLineAfterSortedOnInvoiceLineNo.JI_LineNo})", importEntryInvoiceLine3, iEntryLine2.FirstInvoiceLineAfterSortedOnInvoiceLineNo);
				AssertEquals("iEntryLine2.RandomLine", importEntryInvoiceLine3, iEntryLine2.RandomLine);
				if (!isCusEntryLine)
				{
					AssertSame("iEntryLine2.InvoiceLines is cached.", iEntryLine2.InvoiceLines, iEntryLine2.InvoiceLines);
				}
				AssertArrayEqualsByElements("iEntryLine2.InvoiceLines", new IInvoiceLine[] { importEntryInvoiceLine3 }, iEntryLine2.InvoiceLines.ToArray());
				AssertEquals("iEntryLine2.ChildSecondaryEntryLines.Any()", false, iEntryLine2.ChildSecondaryEntryLines.Any());
			});
		}

		void AssertIBaseDrawbackEntryLine(Customs.Business.IBaseDrawbackEntryLine baseDrawbackEntryLine, ZString description, ZDecimal invoiceQuantity, ZString invoiceUQ, ZDecimal customsQuantity, ZString customsUnitQty)
		{
			var key = baseDrawbackEntryLine.CL_LineNumber.ToString();
			AssertEquals($"{key} - CL_Description", description, baseDrawbackEntryLine.CL_Description);
			AssertEquals($"{key} - InvoiceQuantity", invoiceQuantity, baseDrawbackEntryLine.InvoiceQuantity);
			AssertEquals($"{key} - InvoiceUQ", invoiceUQ, baseDrawbackEntryLine.InvoiceUQ);
			AssertEquals($"{key} - CustomsQuantity", customsQuantity, baseDrawbackEntryLine.CustomsQuantity);
			AssertEquals($"{key} - CustomsUnitQty", customsUnitQty, baseDrawbackEntryLine.CustomsUnitQty);
		}

		void AssertIBaseEntryLine(Customs.Business.IBaseEntryLine baseEntryLine, ZGuid entryLinePK, ZShort lineNumber, ZString adValoremTariff, ZDecimal customsValue, ZDecimal dutyAmount)
		{
			var key = baseEntryLine.CL_LineNumber.ToString();
			AssertEquals($"{key} - PK", entryLinePK, baseEntryLine.PK);
			AssertEquals($"{key} - CL_LineNumber", lineNumber, baseEntryLine.CL_LineNumber);
			AssertEquals($"{key} - CL_AdValoremTariff", adValoremTariff, baseEntryLine.CL_AdValoremTariff);
			AssertEquals($"{key} - CL_CustomsValue", customsValue, baseEntryLine.CL_CustomsValue);
			AssertEquals($"{key} - DutyAmount", dutyAmount, baseEntryLine.DutyAmount);
		}

		public void TestIDutyDataMembers()
		{
			var universalReferenceTestHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USExpiredSPI, "Expired Special Program Indicator");
			universalReferenceTestHelper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USExpiredSPI, PrimarySpecProgramIndicatorList.Codes.A, "A", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1), Universal.RefCusCodeListAttributeTypes.Codes.USSPIException, PrimarySpecProgramIndicatorList.Codes.D);
			universalReferenceTestHelper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USExpiredSPI, SpecialProgramList.Codes.MA, "MA", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1), Universal.RefCusCodeListAttributeTypes.Codes.USSPIException, SpecialProgramList.Codes.PA);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "40000000";
			invoiceLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.F;
			invoiceLine1.US_SPI = SpecialProgramList.Codes.AU;
			invoiceLine1.US_UC_NKCountryOfOrigin = "TT";
			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "20000000";
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.G;
			invoiceLine2.US_SPI = PrimarySpecProgramIndicatorList.Codes.D;
			invoiceLine2.US_UC_NKCountryOfOrigin = "EK";
			var invoiceLine3 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "50000000";
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.M;
			invoiceLine3.US_SPI = PrimarySpecProgramIndicatorList.Codes.A;
			invoiceLine3.US_UC_NKCountryOfOrigin = "ID";
			invoiceLine3.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			var dateForDutyRate = ZDate.Today.AddDays(-1);
			var privilegedStatusDate = dateForDutyRate.AddDays(-1);
			invoiceLine3.US_PrivilegedStatusDate = privilegedStatusDate;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.US_DutyCalcDate = dateForDutyRate;
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_AdValoremTariff = "40000000";
			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_AdValoremTariff = "50000000";
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine1.PK;
			invoiceLine3.JI_CL = entryLine2.PK;
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var importEntryLine1 = factory.Load<USImportEntryLine>(entryLine1.PK);
			var importEntryLine2 = factory.Load<USImportEntryLine>(entryLine2.PK);
			IDutyData dutyData1 = importEntryLine1;
			IDutyData dutyData2 = importEntryLine2;
			AssertIDutyDataNotSupportedMembers(dutyData1);
			AssertIDutyDataSupportedMembers(dutyData1, "40000000", dateForDutyRate, "TT", ZString.Empty, SpecialProgramList.Codes.AU, SecondarySpecProgIndicatorList.Codes.F);
			AssertIDutyDataSupportedMembers(dutyData2, "50000000", privilegedStatusDate, "ID", ZString.Empty, ZString.Empty, SecondarySpecProgIndicatorList.Codes.M);
		}

		void AssertIDutyDataSupportedMembers(IDutyData data, ZString tariff, ZDate dateForDutyCalculation, ZString countryOfOrigin, ZString specialProgramsIndicatorPrimary, ZString specialProgramsIndicatorCountry, ZString specialProgramsIndicatorSecondary)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Tariff", tariff, data.Tariff);
				AssertEquals("DateForDutyCalculation", dateForDutyCalculation, data.DateForDutyCalculation);
				AssertEquals("CountryOfOrigin", countryOfOrigin, data.CountryOfOrigin);
				AssertEquals("SpecialProgramsIndicatorPrimary", specialProgramsIndicatorPrimary, data.SpecialProgramsIndicatorPrimary);
				AssertEquals("SpecialProgramsIndicatorCountry", specialProgramsIndicatorCountry, data.SpecialProgramsIndicatorCountry);
				AssertEquals("SpecialProgramsIndicatorSecondary", specialProgramsIndicatorSecondary, data.SpecialProgramsIndicatorSecondary);
			});
		}

		void AssertIDutyDataNotSupportedMembers(IDutyData data)
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<NotSupportedException>("Quantity1", () => _ = data.Quantity1);
				AssertExceptionThrown<NotSupportedException>("UQ1", () => _ = data.UQ1);
				AssertExceptionThrown<NotSupportedException>("Quantity2", () => _ = data.Quantity2);
				AssertExceptionThrown<NotSupportedException>("UQ2", () => _ = data.UQ2);
				AssertExceptionThrown<NotSupportedException>("Quantity3", () => _ = data.Quantity3);
				AssertExceptionThrown<NotSupportedException>("UQ3", () => _ = data.UQ3);
				AssertExceptionThrown<NotSupportedException>("CustomsValue", () => _ = data.CustomsValue);
				AssertExceptionThrown<NotSupportedException>("SelectedRateType", () => _ = data.SelectedRateType);
				AssertExceptionThrown<NotSupportedException>("ValueForADD", () => _ = data.ValueForADD);
				AssertExceptionThrown<NotSupportedException>("ADDDepositRate", () => _ = data.ADDDepositRate);
				AssertExceptionThrown<NotSupportedException>("ADDCaseRateTypeQualifier", () => _ = data.ADDCaseRateTypeQualifier);
				AssertExceptionThrown<NotSupportedException>("ADDQuantity", () => _ = data.ADDQuantity);
				AssertExceptionThrown<NotSupportedException>("ADDutyManual", () => _ = data.ADDutyManual);
				AssertExceptionThrown<NotSupportedException>("ValueForCVD", () => _ = data.ValueForCVD);
				AssertExceptionThrown<NotSupportedException>("CVDDepositRate", () => _ = data.CVDDepositRate);
				AssertExceptionThrown<NotSupportedException>("CVDCaseRateTypeQualifier", () => _ = data.CVDCaseRateTypeQualifier);
				AssertExceptionThrown<NotSupportedException>("CVDQuantity", () => _ = data.CVDQuantity);
				AssertExceptionThrown<NotSupportedException>("CVDutyManual", () => _ = data.CVDutyManual);
				AssertExceptionThrown<NotSupportedException>("ParentTariffLine", () => _ = data.ParentTariffLine);
				AssertExceptionThrown<NotSupportedException>("IsCottonFeeExemptIndicated", () => _ = data.IsCottonFeeExemptIndicated);
				AssertExceptionThrown<NotSupportedException>("HasCottonCertificate", () => _ = data.HasCottonCertificate);
				AssertExceptionThrown<NotSupportedException>("IsSetXLine", () => _ = data.IsSetXLine);
				AssertExceptionThrown<NotSupportedException>("IsSetVLine", () => _ = data.IsSetVLine);
				AssertExceptionThrown<NotSupportedException>("IsAMSFeeExempt", () => _ = data.IsAMSFeeExempt);
				AssertExceptionThrown<NotSupportedException>("IsRaspberryFeeExempt", () => _ = data.IsRaspberryFeeExempt);
				AssertExceptionThrown<NotSupportedException>("EntryType", () => _ = data.EntryType);
				AssertExceptionThrown<NotSupportedException>("IsClearedInPR", () => _ = data.IsClearedInPR);
				AssertExceptionThrown<NotSupportedException>("IsSecondaryTariffLine", () => _ = data.IsSecondaryTariffLine);
				AssertExceptionThrown<NotSupportedException>("IsDomesticMerchandise", () => _ = data.IsDomesticMerchandise);
				AssertExceptionThrown<NotSupportedException>("HasTextileCategoryNo", () => _ = data.HasTextileCategoryNo);
				AssertExceptionThrown<NotSupportedException>("IsCombineSecondaryTariffLine", () => _ = data.IsCombineSecondaryTariffLine);
				AssertExceptionThrown<NotSupportedException>("CombineChildLines", () => _ = data.CombineChildLines);
				AssertExceptionThrown<NotSupportedException>("SupTariffs", () => _ = data.SupTariffs);
				AssertExceptionThrown<NotSupportedException>("CombineParentLine", () => _ = data.CombineParentLine);
				AssertExceptionThrown<NotSupportedException>("CombineAllLines", () => _ = data.CombineAllLines);
			});
		}

		protected override bool IsDeleteSupported() => false;
	}
}
