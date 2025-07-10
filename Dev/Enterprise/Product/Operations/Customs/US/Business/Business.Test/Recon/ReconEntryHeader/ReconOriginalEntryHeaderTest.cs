using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ReconOriginalEntryHeader))]
	sealed class ReconOriginalEntryHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public override void TestSettingValueCallsRefreshBinding()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USFTARECONIND, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				base.TestSettingValueCallsRefreshBinding();
			}
		}

		public void TestUS_NAFTAReconIndicator()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			var recon = ReconDeclaration.Get(declaration);
			var originalEntry = recon.OriginalEntries.AddNew();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USFTARECONIND, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				Assert("default value", !originalEntry.US_NAFTAReconIndicator);
				originalEntry.US_NAFTAReconIndicator = true;
				Assert("US_NAFTAReconIndicator should not be assigned when FUNCS USFTAReconInd is invalid.", !originalEntry.US_NAFTAReconIndicator);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USFTARECONIND, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				Assert("default value", !originalEntry.US_NAFTAReconIndicator);
				originalEntry.US_NAFTAReconIndicator = true;
				Assert("US_NAFTAReconIndicator should be assigned when FUNCS USFTAReconInd is valid.", originalEntry.US_NAFTAReconIndicator);
			}
		}

		public void TestEnsureThatAddInfoDataIsSaved()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			var recon = ReconDeclaration.Get(declaration);
			var originalEntry = recon.OriginalEntries.AddNew();
			originalEntry.US_R_GoodsDescription = "TEST";
			originalEntry.US_R_OwnerRef = "XYZ";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			recon = ReconDeclaration.Get(declaration);
			originalEntry = recon.OriginalEntries[0];
			AssertEquals("US_R_GoodsDescription", "TEST", originalEntry.US_R_GoodsDescription);
			AssertEquals("US_R_OwnerRef", "XYZ", originalEntry.US_R_OwnerRef);
		}

		public void TestReconChargesInAscendingCodeOrder()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDeclaration = new ReconDeclaration(declaration);
			ReconOriginalEntryHeader reconOriginalEntry = reconDeclaration.OriginalEntries.AddNew();

			CusEntryHeaderCharges recon1 = reconOriginalEntry.ReconCharges.AddNew();
			recon1.C1_ChargeType = "GHI";
			recon1.C1_ChargeAmount = 123m;
			CusEntryHeaderCharges recon2 = reconOriginalEntry.ReconCharges.AddNew();
			recon2.C1_ChargeType = "ABC";
			recon2.C1_ChargeAmount = 456m;
			CusEntryHeaderCharges recon3 = reconOriginalEntry.ReconCharges.AddNew();
			recon3.C1_ChargeType = "DEF";
			recon3.C1_ChargeAmount = 222m;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			reconDeclaration = new ReconDeclaration(newFactory.Load<JobDeclaration>(declaration.PK));

			AssertEquals("Code 1", "ABC", reconDeclaration.OriginalEntries[0].ReconCharges[0].C1_ChargeType);
			AssertEquals("Code 2", "DEF", reconDeclaration.OriginalEntries[0].ReconCharges[1].C1_ChargeType);
			AssertEquals("Code 3", "GHI", reconDeclaration.OriginalEntries[0].ReconCharges[2].C1_ChargeType);
		}

		public void TestAddAggregateFeesIfNecessary()
		{
			var declaration = Factory.New<JobDeclaration>();
			var reconDeclaration = new ReconDeclaration(declaration);
			var reconOriginalEntry = reconDeclaration.OriginalEntries.AddNew();
			var recon1 = reconOriginalEntry.ReconCharges.AddNew();
			recon1.C1_ChargeType = Core.Constants.USCustoms.FeeCodes.Beef;
			recon1.C1_ChargeAmount = 123m;
			var recon2 = reconOriginalEntry.ReconCharges.AddNew();
			recon2.C1_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			recon2.C1_ChargeAmount = 456m;
			var recon3 = reconOriginalEntry.ReconCharges.AddNew();
			recon3.C1_ChargeType = Core.Constants.USCustoms.FeeCodes.Duty;
			recon3.C1_ChargeAmount = 222m;

			reconOriginalEntry = reconDeclaration.OriginalEntries.AddNew();
			recon1 = reconOriginalEntry.ReconCharges.AddNew();
			recon1.C1_ChargeType = Core.Constants.USCustoms.FeeCodes.Beef;
			recon1.C1_ChargeAmount = 123m;
			recon2 = reconOriginalEntry.ReconCharges.AddNew();
			recon2.C1_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			recon2.C1_ChargeAmount = 456m;
			recon3 = reconOriginalEntry.ReconCharges.AddNew();
			recon3.C1_ChargeType = Core.Constants.USCustoms.FeeCodes.Duty;
			recon3.C1_ChargeAmount = 222m;
			var recon4 = reconOriginalEntry.ReconCharges.AddNew();
			recon4.C1_ChargeType = Core.Constants.USCustoms.FeeCodes.ReconciliationInterest;
			recon4.C1_ChargeAmount = 225m;

			reconDeclaration.US_IsAggregate = true;
			reconDeclaration.US_R_IsNoChangeAgg = true;

			AssertEquals(2, reconDeclaration.AggregateRefundedFees.Count);
			AssertNotNull(reconDeclaration.AggregateRefundedFees.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Beef));
			AssertNotNull(reconDeclaration.AggregateRefundedFees.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		public void TestOriginalChargesInAscendingCodeOrder()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDeclaration = new ReconDeclaration(declaration);
			ReconOriginalEntryHeader reconOriginalEntry = reconDeclaration.OriginalEntries.AddNew();

			ReconEntryOriginalCharge charge1 = reconOriginalEntry.OriginalCharges.AddNew();
			charge1.CY_Code = "GHI";
			charge1.CY_Amount = 123m;
			ReconEntryOriginalCharge charge2 = reconOriginalEntry.OriginalCharges.AddNew();
			charge2.CY_Code = "ABC";
			charge2.CY_Amount = 456m;
			ReconEntryOriginalCharge charge3 = reconOriginalEntry.OriginalCharges.AddNew();
			charge3.CY_Code = "DEF";
			charge3.CY_Amount = 222m;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			reconDeclaration = new ReconDeclaration(newFactory.Load<JobDeclaration>(declaration.PK));

			AssertEquals("Code 1", "ABC", reconDeclaration.OriginalEntries[0].OriginalCharges[0].CY_Code);
			AssertEquals("Code 2", "DEF", reconDeclaration.OriginalEntries[0].OriginalCharges[1].CY_Code);
			AssertEquals("Code 3", "GHI", reconDeclaration.OriginalEntries[0].OriginalCharges[2].CY_Code);
		}

		public void TestSettingCH_OrigEntryReferenceUpdatesCH_CH_OriginalEntry()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration1.US_EnableENS = true;
			declaration1.US_BondProducerAccNo = "12";
			declaration1.Invoices.AddNew();
			declaration1.InvoiceLines.AddNew();
			declaration1.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			JobDeclaration declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration2.US_EnableENS = true;
			declaration2.Invoices.AddNew();
			declaration2.InvoiceLines.AddNew();
			declaration2.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			CusEntryHeader ensEntry1 = declaration1.CustomsEntryHeaders[0];
			CusEntryHeader ensEntry2 = declaration2.CustomsEntryHeaders[0];

			JobDeclaration reconInnerDec = Factory.New<JobDeclaration>();
			ReconDeclaration reconDeclaration = new ReconDeclaration(reconInnerDec);
			ReconOriginalEntryHeader reconOriginalEntry = reconDeclaration.OriginalEntries.AddNew();
			Assert(!reconOriginalEntry.CH_OrigEntryReferenceInfo.ReadOnly);
			reconOriginalEntry.CH_OrigEntryReference = "XJ5" + ensEntry1.EntryNumber;
			AssertEquals("CH_CH_OrigEntry is updated", ensEntry1.PK, reconOriginalEntry.CH_CH_OriginalEntry);
			AssertEquals("CH_OriginalDeclarationReference", declaration1.JE_DeclarationReference, reconOriginalEntry.CH_OriginalDeclarationReference);

			reconOriginalEntry.CH_OrigEntryReference = "~~~~";
			AssertEquals("CH_CH_OrigEntry is updated", ZGuid.Empty, reconOriginalEntry.CH_CH_OriginalEntry);
			AssertEquals("CH_OriginalDeclarationReference", ZString.Empty, reconOriginalEntry.CH_OriginalDeclarationReference);

			reconOriginalEntry.CH_OrigEntryReference = "XJ5" + ensEntry1.EntryNumber;
			Factory.Save();

			BusinessObjectFactory factory = new BusinessObjectFactory();
			ReconDeclaration reconDecLoaded = new ReconDeclaration(factory.Load<JobDeclaration>(reconInnerDec.PK));
			AssertEquals("CH_OrigEntryReference is kept", "XJ5" + ensEntry1.EntryNumber, reconDecLoaded.OriginalEntries[0].CH_OrigEntryReference);
			AssertEquals(reconOriginalEntry.DeclarationBondNo, "12");
		}

		public void TestSettingCH_OrigEntryReferenceRetrievesEntryDetails()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_DateOfArrival = new ZDateTime(2010, 7, 17);
			declaration.US_PriorDisclosure = true;
			declaration.US_NAFTAClaimStat = true;
			declaration.US_ProtestStat = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			CusEntryHeader ensEntry = declaration.CustomsEntryHeaders[0];

			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader reconOriginalEntry = reconDeclaration.OriginalEntries.AddNew();
			reconOriginalEntry.CH_OrigEntryReference = "XJ5" + ensEntry.EntryNumber;
			AssertEquals(new ZDateTime(2010, 7, 17), reconOriginalEntry.US_ImportDate);
			Assert(reconOriginalEntry.US_PriorDisclosure);
			Assert(reconOriginalEntry.US_NAFTAClaimStat);
			Assert(reconOriginalEntry.US_ProtestStat);
		}

		public void TestRegisterEditableChildForEntry()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDeclaration = new ReconDeclaration(declaration);

			ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries.AddNew();
			Factory.Save();

			AssertEquals("HasChnages", false, reconDeclaration.HasChanges);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration declaration2 = factory2.Load<JobDeclaration>(declaration.PK);
			ReconDeclaration reconDeclaration2 = new ReconDeclaration(declaration2);

			ReconOriginalEntryHeader originalEntry2 = reconDeclaration2.OriginalEntries[0];
			originalEntry2.US_SchDEntry = "~~";
			factory2.Save();

			JobDeclaration declaration3 = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			ReconDeclaration reconDeclaration3 = new ReconDeclaration(declaration3);
			AssertEquals("Change made to US_EntryType should have been saved as an editable child", "~~", reconDeclaration3.OriginalEntries[0].US_SchDEntry);
		}

		public void TestIdentifier()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDeclaration = new ReconDeclaration(declaration);

			ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries.AddNew();
			Factory.Save();

			AssertEquals(originalEntry.CH_PK, ((IIdentified)originalEntry).Identifier);
		}

		public void TestTotalAmountPayableAndExciseTax()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 15000m;

			invoiceLine.JI_Tariff = "2402103030";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsSecondQuantity = 1360m;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();//To serialise JI_AddInfo which will be cloned

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });
			var reconEntry = reconDeclaration.OriginalEntries[0];
			reconEntry.US_R_DutyRateDate = ZDateTime.Today;
			reconEntry.US_R_CalcOrigDuty = true;
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			AssertEquals("OriginalTax", 1828m, reconEntry.OriginalTax);
			AssertEquals("OriginalDuty", 3275.4m, reconEntry.OriginalDuty);

			AssertEquals("ReconTax", 1828m, reconEntry.ReconTax);
			AssertEquals("ReconDuty", 3275.4m, reconEntry.ReconDuty);
		}

		public void TestICodeDescription()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());

			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();

			AssertEquals("ICodeDescription, US_CH_ReconEntry is bound to a GuidDropEdit and Guid mapped to be set to JobComInvoiceHeader.US_CH_ReconEntry should be a real one, not the non-persistent one", entry.CH_PK, ((ICodeDescription)entry).PK);
		}

		public void TestShouldDutiesFeesBeCalculated()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());

			ReconOriginalEntryHeader entry = reconDec.OriginalEntries.AddNew();
			ReconEntryOriginalCharge charge = entry.OriginalCharges.AddNew();

			AssertEquals(false, entry.ShouldDutiesFeesBeCalculated);

			entry.Invoice.InvoiceLines.AddNew();
			AssertEquals(true, entry.ShouldDutiesFeesBeCalculated);
			AssertEquals("CY_Override", false, charge.CY_IsOverridden);

			entry.Invoice.InvoiceLines.RemoveAndDeleteAll();
			AssertEquals(false, entry.ShouldDutiesFeesBeCalculated);

			entry.US_R_NoLineDetails = true;
			AssertEquals(false, entry.ShouldDutiesFeesBeCalculated);
			AssertEquals("CY_Override", true, charge.CY_IsOverridden);

			entry.US_R_NoLineDetails = false;
			entry.Invoice.InvoiceLines.AddNew();
			Assert(entry.ShouldDutiesFeesBeCalculated);
			Factory.Save();

			BusinessObjectFactory factory = new BusinessObjectFactory();
			var decLoaded = factory.Load<JobDeclaration>(reconDec.JE_PK);
			AssertNotNull("PreCondition", decLoaded);
			var reconDecLoaded = new ReconDeclaration(decLoaded);

			Assert(reconDecLoaded.OriginalEntries[0].ShouldDutiesFeesBeCalculated);
		}

		public void TestFeesAndCharges()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDec = new ReconDeclaration(declaration);

			ReconOriginalEntryHeader entry1 = reconDec.OriginalEntries.AddNew();

			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, "OriginalMPF", "ReconMPF", 0.5m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.HMF, "OriginalHMF", "ReconHMF", 0.2m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.Avocado, "OriginalAvocado", "ReconAvocado", 1m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.Beef, "OriginalBeef", "ReconBeef", 2m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.Blueberry, "OriginalBlueberry", "ReconBlueberry", 3m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.Cotton, "OriginalCotton", "ReconCotton", 4m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.DistilledSpirits, "OriginalSpirits", "ReconSpirits", 5m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.DutiableMail, "OriginalDutiableMail", "ReconDutiableMail", 6m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.FreshLimes, "OriginalLimes", "ReconLimes", 7m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.Honey, "OriginalHoney", "ReconHoney", 8m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.Mango, "OriginalMango", "ReconMango", 9m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.MerchandiseInformal, "OriginalInformal", "ReconInformal", 10m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge, "OriginalSurcharge", "ReconSurcharge", 11m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.Mushroom, "OriginalMushroom", "ReconMushroom", 12m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.OtherExcise, "OriginalOtherExcise", "ReconOtherExcise", 13m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.Raspberry, "OriginalRaspberry", "ReconRaspberry", 14m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.Pork, "OriginalPork", "ReconPork", 15m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.Potato, "OriginalPotato", "ReconPotato", 16m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.SoftwoodLumber, "OriginalSoftwoodLumber", "ReconSoftwoodLumber", 17m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.Sugar, "OriginalSugar", "ReconSugar", 18m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.Tobacco, "OriginalTobacco", "ReconTobacco", 19m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.Watermelon, "OriginalWatermelon", "ReconWatermelon", 20m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.Wines, "OriginalWines", "ReconWines", 21m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.OtherAgencies, "OriginalOtherAgencies", "ReconOtherAgencies", 22m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.Sorghum, "OriginalSorghum", "ReconSorghum", 23m);
			AssertChargeAndFee(entry1, Core.Constants.USCustoms.FeeCodes.DairyFee, "OriginalDairy", "ReconDairy", 24m);
		}

		public void TestResetReconChargesIfNecessary()
		{
			ReconEntry.US_R_NoLineDetails = true;
			ReconEntry.ReconCharges.AddNew("AAA", 10m);
			ReconEntry.ReconCharges.AddNew("BBB", 10m);

			ReconEntry.ResetReconChargesIfNecessary();

			AssertEquals("two recon charges remain as there are no invoices attached", 10m, ReconEntry.ReconCharges[0].C1_ChargeAmount);
			AssertEquals("two recon charges remain as there are no invoices attached", 10m, ReconEntry.ReconCharges[1].C1_ChargeAmount);

			ReconEntry.US_R_NoLineDetails = false;
			ReconEntry.ResetReconChargesIfNecessary();
			AssertEquals("two recon charges remain as there are no invoices attached", 10m, ReconEntry.ReconCharges[0].C1_ChargeAmount);
			AssertEquals("two recon charges remain as there are no invoices attached", 10m, ReconEntry.ReconCharges[1].C1_ChargeAmount);

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 15000m;

			invoiceLine.JI_Tariff = "2402103030";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsSecondQuantity = 1360m;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });
			reconDeclaration.OriginalEntries[0].US_R_DutyRateDate = ZDateTime.Today;
			reconDeclaration.CalculateDutyFeesForChangedEntries();

			var reconEntry = reconDeclaration.OriginalEntries[0];
			var reconInvoiceLine = reconEntry.Invoice.InvoiceLines.AddNew();
			reconInvoiceLine.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.Honey);
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			AssertEquals("No HoneyFee tax because amount is 0 for both recon and original line charges", null, reconEntry.ReconCharges.GetChargeWithThisCode(Core.Constants.USCustoms.FeeCodes.Honey));
			AssertEquals("No HoneyFee tax because amount is 0 for both recon and original line charges", null, reconEntry.OriginalCharges.GetCharge(Core.Constants.USCustoms.FeeCodes.Honey));
		}

		public void TestResetReconChargesIfNecessaryForMonthlyFiling()
		{
			DeclarationTestHelper.SetEntryFilerCode("SV9");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.FixedTransportInstallations;
			declaration.US_MonthlyFiling = true;
			declaration.US_EnableENS = true;
			declaration.US_PayableMPF = 500m;

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 15000m;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });
			reconDeclaration.OriginalEntries[0].US_R_DutyRateDate = ZDateTime.Today;
			reconDeclaration.CalculateDutyFeesForChangedEntries();

			var reconEntry = reconDeclaration.OriginalEntries[0];
			var reconInvoiceLine = reconEntry.Invoice.InvoiceLines.AddNew();
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			AssertEquals(500m, reconEntry.ReconCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(500m, reconEntry.OriginalCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			reconEntry.ReconCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 400m);
			reconEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 600m);
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			AssertEquals(400m, reconEntry.ReconCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(600m, reconEntry.OriginalCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		public void TestReconChargesReadOnly()
		{
			var charge = ReconEntry.OriginalCharges.AddNew();
			charge.CY_IsOverridden = true;
			ReconEntry.Invoice.InvoiceLines.AddNew();
			ReconEntry.UpdateChargesReadOnlyState();
			AssertEquals("charge.CY_IsOverridden", false, charge.CY_IsOverridden);

			ReconEntry.US_R_NoLineDetails = true;
			ReconEntry.UpdateChargesReadOnlyState();
			AssertEquals("charge.CY_IsOverridden", true, charge.CY_IsOverridden);

			ReconEntry.US_R_ChangedLinesOnly = true;
			AssertEquals("charge.CY_IsOverridden", true, charge.CY_IsOverridden);

			ReconEntry.US_R_ChangedLinesOnly = false;
			AssertEquals("charge.CY_IsOverridden", false, charge.CY_IsOverridden);
		}

		public void TestSettingUS_R_NoLineDetailsClearUS_R_IsHMFApplicable()
		{
			ReconOriginalEntryHeader reconEntry = ReconDec.OriginalEntries.AddNew();
			reconEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			reconEntry.US_R_NoLineDetails = ZBool.True;
			AssertEquals(ZString.Empty, reconEntry.US_R_IsHMFApplicable);
			reconEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			reconEntry.US_R_NoLineDetails = ZBool.False;
			AssertEquals(YesNoDefaultList.Codes.Yes, reconEntry.US_R_IsHMFApplicable);
		}

		public void TestSchDEntry()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4901", "Test PR Port", startDate, endDate);
			newFactory.Save();

			ReconEntry.US_SchDEntry = "4901";
			AssertEquals("4901", ReconEntry.SchDEntry.ZZD_Code);
		}

		public void TestDelete()
		{
			JobComInvoiceHeader invoice = ReconEntry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			ReconEntry.Delete();

			AssertEquals("invoice is deleted", true, invoice.IsDeleted);
			AssertEquals("invoiceLine is deleted", true, invoiceLine.IsDeleted);
		}

		public void TestTotalFields()
		{
			ReconEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty, 1m);
			ReconEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 2m);
			ReconEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Wines, 4m);
			ReconEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Avocado, 8m);

			ReconEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.CountervailingDuty, 16m);
			ReconEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 32m);
			ReconEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.OtherExcise, 64m);
			ReconEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Honey, 128m);
			ReconEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest, 256m);

			AssertEquals("Original Duty", 2m, ReconEntry.OriginalDuty);
			AssertEquals("Original Tax", 4m, ReconEntry.OriginalTax);
			AssertEquals("Original Fee", 8m, ReconEntry.OriginalFee);

			AssertEquals("Recon Duty", 32m, ReconEntry.ReconDuty);
			AssertEquals("Recon Tax", 64m, ReconEntry.ReconTax);
			AssertEquals("Recon Fee", 128m, ReconEntry.ReconFee);
			AssertEquals("Recon Interest", 256m, ReconEntry.ReconInterest);
		}

		public void TestIsACE()
		{
			ReconEntry.US_R_MsgMode = JobApplicationCodeList.Codes.ACE;
			Assert(ReconEntry.IsACE);

			ReconEntry.US_R_MsgMode = JobApplicationCodeList.Codes.ACS;
			Assert(!ReconEntry.IsACE);
		}

		public void TestLogsAndNotes()
		{
			StmALog log = Entry.Logs.AddNew();
			StmNote note = Entry.Notes.AddNew();

			AssertEquals(true, ReconEntry.GetLogs().GetAllLogs().Contains(log));
			AssertEquals(true, ReconEntry.GetNotes().GetAllNotes().Contains(note));
		}

		public void TestSaveAndDelete()
		{
			AssertEquals(false, Entry.IsInDatabase);
			AssertEquals(false, ReconEntry.IsInDatabase);

			Factory.Save();
			AssertEquals(true, Entry.IsInDatabase);
			AssertEquals(true, ReconEntry.IsInDatabase);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			ReconDeclaration reconDecLoaded = new ReconDeclaration(factory2.Load<JobDeclaration>(ReconDec.JE_PK));
			AssertEquals(1, reconDecLoaded.OriginalEntries.Count);
			ReconOriginalEntryHeader reconEntryLoaded = reconDecLoaded.OriginalEntries[0];
			CusEntryHeader entryLoaded = factory2.Load<CusEntryHeader>(reconEntryLoaded.CH_PK);
			AssertEquals(true, entryLoaded.IsInDatabase);
			AssertEquals(true, reconEntryLoaded.IsInDatabase);

			reconEntryLoaded.Delete();
			AssertEquals(true, entryLoaded.IsDeleted);
			AssertEquals(true, reconEntryLoaded.IsDeleted);
		}

		public void TestWraps()
		{
			AssertEquals(true, ReconEntry.Wraps(Entry));
			AssertEquals(false, ReconEntry.Wraps(Factory.New<CusEntryHeader>()));
		}

		public void TestDefaultDutyDateIfRequired()
		{
			ReconEntry.US_R_DutyRateDate = ZDateTime.Empty;
			ReconEntry.US_PaymentDate = new ZDateTime(2008, 1, 2);
			AssertEquals("DutyDate is defaulted from payment date", new ZDateTime(2008, 1, 2), ReconEntry.US_R_DutyRateDate);

			ReconEntry.US_R_DutyRateDate = ZDateTime.Empty;
			ReconEntry.US_R_ReleaseDate = new ZDateTime(2008, 1, 3);
			AssertEquals("DutyDate is defaulted from entry date", new ZDateTime(2008, 1, 3), ReconEntry.US_R_DutyRateDate);
		}

		public void TestHasBeenShortPaid()
		{
			ReconOriginalEntryHeader reconEntry = ReconDec.OriginalEntries.AddNew();

			reconEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 1m);
			reconEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Avocado, 2m);
			reconEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Tobacco, 3m);

			reconEntry.ReconCharges.UpdateOrAddCharge(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 0m);
			reconEntry.ReconCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Avocado, 2m);
			reconEntry.ReconCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Tobacco, 5m);
			AssertEquals("HasBeenShortPaid", true, reconEntry.HasBeenShortPaid);

			reconEntry.ReconCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Tobacco, 0m);
			AssertEquals("HasBeenShortPaid", false, reconEntry.HasBeenShortPaid);
		}

		public void TestImportEntrySource()
		{
			var reconEntry = ReconDec.OriginalEntries.AddNew();

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var port1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4901", "Test PR Port", startDate, endDate);
			var attributeNameState = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.State, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeListAttribute(port1.PK, attributeNameState.ZXE_Name, USStateList.Codes.PuertoRico);
			newFactory.Save();

			reconEntry.US_SchDEntry = "4901";
			AssertEquals(ReconciliationImportEntrySourceList.Codes.PuertoRico, reconEntry.ImportSourceIndicator);

			var port2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "5101", "Test VI Port", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeListAttribute(port2.PK, attributeNameState.ZXE_Name, USStateList.Codes.VirginIslands);
			newFactory.Save();

			reconEntry.US_SchDEntry = "5101";
			AssertEquals(ReconciliationImportEntrySourceList.Codes.VirginIslands, reconEntry.ImportSourceIndicator);

			var port3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3901", "Test IL Port", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeListAttribute(port3.PK, attributeNameState.ZXE_Name, USStateList.Codes.Illinois);
			newFactory.Save();

			reconEntry.US_SchDEntry = "3901";
			AssertEquals(ReconciliationImportEntrySourceList.Codes.FiftyStates, reconEntry.ImportSourceIndicator);
		}

		public void TestRefreshEntryWithTotalOriginalCustomsFees()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDec.OriginalEntries.AddNew();
			var invoiceLine0 = originalEntry.Invoice.JobComInvoiceLines.AddNew();
			invoiceLine0.US_R_OrigMPFAmount = 60m;
			invoiceLine0.US_R_OrigHMFAmount = 80m;
			invoiceLine0.US_R_OrigDuty = 100m;
			invoiceLine0.US_R_OrigSupDuty = 20m;

			originalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 0);
			originalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.HMF, 0);
			originalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 0);

			AssertEquals(60m, originalEntry.GetTotalOriginalCustomsFeesFromFeeCode(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(80m, originalEntry.GetTotalOriginalCustomsFeesFromFeeCode(Core.Constants.USCustoms.FeeCodes.HMF));
			AssertEquals(120m, originalEntry.GetTotalOriginalCustomsFeesFromFeeCode(Core.Constants.USCustoms.FeeCodes.Duty));

			invoiceLine0.US_R_OrigMPFAmount = 65m;
			AssertEquals(65m, originalEntry.GetTotalOriginalCustomsFeesFromFeeCode(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(80m, originalEntry.GetTotalOriginalCustomsFeesFromFeeCode(Core.Constants.USCustoms.FeeCodes.HMF));

			invoiceLine0.US_R_OrigHMFAmount = 90m;
			AssertEquals(65m, originalEntry.GetTotalOriginalCustomsFeesFromFeeCode(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(90m, originalEntry.GetTotalOriginalCustomsFeesFromFeeCode(Core.Constants.USCustoms.FeeCodes.HMF));
		}

		public void TestRefreshReconFeesOnLineLevel()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDec.OriginalEntries.AddNew();
			var invoiceLine = originalEntry.Invoice.JobComInvoiceLines.AddNew();
			invoiceLine.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.Cotton);
			originalEntry.ResetReconChargesIfNecessary();
			AssertEquals("Cotton fee should be removed", 0, invoiceLine.FeeCusCodes.Count);

			invoiceLine.ReconOriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Cotton);
			invoiceLine.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.Cotton);
			originalEntry.ResetReconChargesIfNecessary();
			AssertEquals("Cotton fee should NOT be removed", 1, invoiceLine.FeeCusCodes.Count);
			AssertEquals("Cotton fee should NOT be removed", Core.Constants.USCustoms.FeeCodes.Cotton, invoiceLine.FeeCusCodes[0].CY_Code);
		}

		public void TestCheckTotalDutyAmountMatchesSumOfAllLines()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDec.OriginalEntries.AddNew();
			var invoiceLine0 = originalEntry.Invoice.JobComInvoiceLines.AddNew();
			invoiceLine0.US_R_OrigDuty = 60m;
			var invoiceLine1 = originalEntry.Invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.US_R_OrigDuty = 80m;

			var originalEntryDutyAmount = originalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 100m);
			originalEntryDutyAmount.Validation.ValidateCY_Amount();
			AssertHasMessageErrorContaining(originalEntryDutyAmount.CY_AmountInfo, ReconEntryOriginalChargeValidation.TotalFeeDoesNotMatch);

			invoiceLine1.US_R_OrigDuty = 40m;
			originalEntryDutyAmount.Validation.ValidateCY_Amount();
			AssertNoMessageErrorContaining(originalEntryDutyAmount.CY_AmountInfo, ReconEntryOriginalChargeValidation.TotalFeeDoesNotMatch);
		}

		public void TestMessageModeDefault()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			CusEntryHeader entry = Factory.New<CusEntryHeader>();
			entry.CH_JE = reconDec.PK;
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry;
			var originalEntry = new ReconOriginalEntryHeader(entry, reconDec);
			AssertEquals(JobApplicationCodeList.Codes.ACE, originalEntry.US_R_MsgMode);
		}

		public void TestUS_R_CottonFeeMandatory()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.US_R_MsgMode = JobApplicationCodeList.Codes.ACS;
			AssertEquals(true, originalEntry.US_R_CottonFeeMandatory_ReadOnly);
			AssertEquals(false, originalEntry.US_R_CottonFeeMandatory);

			originalEntry.US_R_MsgMode = JobApplicationCodeList.Codes.ACE;
			AssertEquals(false, originalEntry.US_R_CottonFeeMandatory_ReadOnly);
			AssertEquals(false, originalEntry.US_R_CottonFeeMandatory);
			originalEntry.US_R_CottonFeeMandatory = true;
			AssertEquals(true, originalEntry.US_R_CottonFeeMandatory);
			originalEntry.US_R_MsgMode = JobApplicationCodeList.Codes.ACS;
			AssertEquals(true, originalEntry.US_R_CottonFeeMandatory_ReadOnly);
			AssertEquals(false, originalEntry.US_R_CottonFeeMandatory);
		}

		public void TestUS_R_ChangedLinesOnly()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.US_R_CalcOrigDuty = true;
			originalEntry.US_R_MonthlyFiling = true;
			originalEntry.US_R_NoLineDetails = true;
			AssertEquals("PreCondition", false, originalEntry.US_R_CalcOrigDuty);
			AssertEquals("PreCondition", true, originalEntry.US_R_MonthlyFiling);
			AssertEquals("PreCondition", true, originalEntry.US_R_NoLineDetails);
			AssertEquals("PreCondition", true, originalEntry.US_R_CalcOrigDuty_ReadOnly);
			AssertEquals("PreCondition", false, originalEntry.US_R_MonthlyFiling_ReadOnly);
			AssertEquals("PreCondition", false, originalEntry.US_R_NoLineDetails_ReadOnly);
			AssertEquals("PreCondition", true, originalEntry.MPC_ReadOnly);
			AssertNull(originalEntry.OriginalCharges.GetCharge(Core.Constants.USCustoms.FeeCodes.MPC));

			originalEntry.US_R_ChangedLinesOnly = true;
			AssertEquals(false, originalEntry.US_R_CalcOrigDuty);
			AssertEquals(false, originalEntry.US_R_MonthlyFiling);
			AssertEquals(false, originalEntry.US_R_NoLineDetails);
			AssertEquals(true, originalEntry.US_R_CalcOrigDuty_ReadOnly);
			AssertEquals(true, originalEntry.US_R_MonthlyFiling_ReadOnly);
			AssertEquals(true, originalEntry.US_R_NoLineDetails_ReadOnly);
			AssertEquals(false, originalEntry.MPC_ReadOnly);
			AssertNotNull(originalEntry.OriginalCharges.GetCharge(Core.Constants.USCustoms.FeeCodes.MPC));
		}

		public void TestUS_R_OrigCV_ReadOnly()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDec.OriginalEntries.AddNew();
			AssertEquals(true, originalEntry.US_R_OrigCV_ReadOnly);

			originalEntry.US_R_ChangedLinesOnly = true;
			AssertEquals(true, originalEntry.US_R_OrigCV_ReadOnly);

			originalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			AssertEquals(false, originalEntry.US_R_OrigCV_ReadOnly);

			originalEntry.US_R_ChangedLinesOnly = false;
			AssertEquals(true, originalEntry.US_R_OrigCV_ReadOnly);
		}

		public void TestNoExceptionThrownWhenOriginalEntryHasMultipleFeesWithSameType()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 100m);
			originalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 50m);

			var invoiceLine = originalEntry.Invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_R_OrigDuty = 100m;

			AssertNoExceptionThrown(() =>
			{
				var totalAmount = originalEntry.GetTotalOriginalCustomsFeesFromFeeCode(Core.Constants.USCustoms.FeeCodes.Duty);
				AssertEquals(100m, totalAmount);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return ReconDec.OriginalEntries.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}

		CusEntryHeader entry;
		CusEntryHeader Entry => entry ?? (entry = Factory.Load<CusEntryHeader>(ReconEntry.CH_PK));

		ReconOriginalEntryHeader reconEntry;
		ReconOriginalEntryHeader ReconEntry => reconEntry ?? (reconEntry = ReconDec.OriginalEntries.AddNew());

		ReconDeclaration reconDec;
		ReconDeclaration ReconDec => reconDec ?? (reconDec = new ReconDeclaration(Factory.New<JobDeclaration>()));

		void AssertChargeAndFee(ReconOriginalEntryHeader entry, string codeType, string originalAmountFieldName, string reconAmountFieldName, decimal amount)
		{
			entry.OriginalCharges.AddNew(codeType, amount);
			entry.ReconCharges.AddNew(codeType, amount + 2);

			AssertEquals("total original amount " + codeType, amount, entry[originalAmountFieldName]);
			AssertEquals("total recon amount " + codeType, amount + 2, entry[reconAmountFieldName]);
		}
	}
}
