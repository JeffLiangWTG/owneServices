using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconEntryFlatFileDataTransferProcessorTest : TestCaseWithFactory
	{
		public void TestExportData()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USFTARECONIND, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				ReconOriginalEntryHeader entry = ReconDeclaration.OriginalEntries.AddNew();
				entry.CH_OrigEntryReference = "XJ5123456";
				entry.US_R_OwnerRef = "OWNERREF";
				entry.US_SchDEntry = "2904";
				entry.US_ImportDate = new ZDateTime(2008, 9, 10);
				entry.US_PaymentDate = new ZDateTime(2008, 9, 20);
				entry.US_R_ReleaseDate = new ZDateTime(2008, 9, 11);
				entry.US_R_NoLineDetails = ZBool.False;
				entry.US_R_GoodsDescription = "HELLO WORLD";
				entry.US_R_MsgMode = JobApplicationCodeList.Codes.ACE;
				entry.US_R_ChangedLinesOnly = true;
				entry.US_R_MonthlyFiling = true;
				entry.US_R_OrigCV = 2300m;
				entry.MPC = 540m;
				entry.US_NAFTAReconIndicator = true;
				Assert("Pre-condition", !entry.CH_PK.IsEmpty);
				AddAllChargesToEntry(entry);
				ReconFlattenedDataLineCollection collection = new ReconFlattenedDataLineCollection(Factory);
				ReconEntryFlatFileDataTransferProcessor processor = new ReconEntryFlatFileDataTransferProcessor();
				processor.ExportReconDataToCollection(collection, ReconDeclaration);
				ReconFlattenedDataLine dataLine = collection[0];
				AssertEquals("XJ5123456", dataLine.EntryNumber);
				AssertEquals("2904", dataLine.EntryPort);
				AssertEquals("OWNERREF", dataLine.OwnerReferenceNumber);
				AssertEquals(new ZDateTime(2008, 9, 10), dataLine.ImportationDate);
				AssertEquals(new ZDateTime(2008, 9, 20), dataLine.PaymentDate);
				AssertEquals(new ZDateTime(2008, 9, 11), dataLine.EntryDate);
				AssertEquals(YesNoDefaultList.Codes.No, dataLine.HasNoLineDetails);
				AssertEquals("HELLO WORLD", dataLine.GoodsDescription);
				AssertEquals(JobApplicationCodeList.Codes.ACE, dataLine.MessageMode);
				AssertEquals(true, dataLine.ChangedLinesOnly);
				AssertEquals(true, dataLine.MonthlyFiling);
				AssertEquals(2300m, dataLine.OriginalCustomsValue);
				AssertEquals(540m, dataLine.MPC);
				AssertEquals(true, dataLine.FTAReconFiled);
				AssertAllChargesExported(dataLine);
				entry.US_R_NoLineDetails = ZBool.True;
				collection = new ReconFlattenedDataLineCollection(Factory);
				processor.ExportReconDataToCollection(collection, ReconDeclaration);
				dataLine = collection[0];
				AssertEquals(YesNoDefaultList.Codes.Yes, dataLine.HasNoLineDetails);
			}
		}

		public void TestImportData_Update()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USFTARECONIND, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				ReconOriginalEntryHeader entry = ReconDeclaration.OriginalEntries.AddNew();
				entry.CH_OrigEntryReference = "XJ512345678";
				entry.US_R_OwnerRef = "OWNERREF";
				entry.US_R_GoodsDescription = "GOODSDESC";
				entry.US_SchDEntry = "2904";
				entry.US_ImportDate = new ZDateTime(2008, 9, 10);
				entry.US_PaymentDate = new ZDateTime(2008, 9, 20);
				entry.US_R_MsgMode = JobApplicationCodeList.Codes.ACE;
				entry.US_R_ChangedLinesOnly = true;
				entry.US_R_MonthlyFiling = true;
				entry.US_R_OrigCV = 122m;
				entry.MPC = 323m;
				Assert("Pre-condition", !entry.CH_PK.IsEmpty);
				ReconFlattenedDataLineCollection collection = new ReconFlattenedDataLineCollection(Factory);
				ReconFlattenedDataLine dataLine = collection.AddNew();
				dataLine.EntryNumber = "XJ5-1234567-8";
				dataLine.OwnerReferenceNumber = "CHANGEDOWNERREF";
				dataLine.GoodsDescription = "CHANGEDGOODSDESC";
				dataLine.EntryPort = "1101";
				dataLine.ImportationDate = new ZDateTime(2008, 9, 11);
				dataLine.PaymentDate = new ZDateTime(2008, 9, 21);
				dataLine.OriginalDuty = 90.123;
				dataLine.ReconDuty = 100.123;
				dataLine.MessageMode = JobApplicationCodeList.Codes.ACS;
				dataLine.ChangedLinesOnly = false;
				dataLine.MonthlyFiling = false;
				dataLine.OriginalCustomsValue = 12220m;
				dataLine.MPC = 32300m;
				dataLine.FTAReconFiled = true;
				ReconEntryFlatFileDataTransferProcessor processor = new ReconEntryFlatFileDataTransferProcessor();
				processor.ImportReconDataFromCollection(collection, ReconDeclaration);
				AssertEquals("CHANGEDOWNERREF", entry.US_R_OwnerRef);
				AssertEquals("CHANGEDGOODSDESC", entry.US_R_GoodsDescription);
				AssertEquals("1101", entry.US_SchDEntry);
				AssertEquals(new ZDateTime(2008, 9, 11), entry.US_ImportDate);
				AssertEquals(new ZDateTime(2008, 9, 21), entry.US_PaymentDate);
				AssertEquals(90.12m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Duty));
				AssertEquals(100.12m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Duty));
				AssertEquals(true, entry.US_R_NoLineDetails);
				AssertEquals(JobApplicationCodeList.Codes.ACS, entry.US_R_MsgMode);
				AssertEquals(false, entry.US_R_ChangedLinesOnly);
				AssertEquals(false, entry.US_R_MonthlyFiling);
				AssertEquals(12220m, entry.US_R_OrigCV);
				AssertEquals(32300m, entry.MPC);
				AssertEquals(true, entry.US_NAFTAReconIndicator);
				dataLine.MessageMode = "";
				processor.ImportReconDataFromCollection(collection, ReconDeclaration);
				AssertEquals(JobApplicationCodeList.Codes.ACS, entry.US_R_MsgMode);
			}
		}

		public void TestImportData()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USFTARECONIND, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				ReconFlattenedDataLineCollection collection = new ReconFlattenedDataLineCollection(Factory);
				ReconFlattenedDataLine dataLine = collection.AddNew();
				dataLine.EntryNumber = "XJ5-1234567-8";
				dataLine.EntryPort = "1101";
				dataLine.ImportationDate = new ZDateTime(2008, 9, 11);
				dataLine.PaymentDate = new ZDateTime(2008, 9, 21);
				dataLine.EntryDate = new ZDateTime(2008, 9, 13);
				dataLine.OriginalDuty = 90.999;
				dataLine.ReconDuty = 100.988;
				dataLine.OriginalMPF = 1.123;
				dataLine.ReconMPF = 2.123;
				dataLine.OriginalHMF = 3.124m;
				dataLine.ReconHMF = 4.124m;
				dataLine.OriginalAVO = 1.125m;
				dataLine.ReconAVO = 2.125m;
				dataLine.OriginalBeef = 1.126m;
				dataLine.ReconBeef = 2.126m;
				dataLine.OriginalBlueberry = 1.127m;
				dataLine.ReconBlueberry = 2.127m;
				dataLine.OriginalCotton = 1.128m;
				dataLine.ReconCotton = 2.128m;
				dataLine.OriginalDairy = 1.129m;
				dataLine.ReconDairy = 2.129m;
				dataLine.OriginalDistilledSpirits = 1.133m;
				dataLine.ReconDistilledSpirits = 2.133m;
				dataLine.OriginalMailFee = 1.143m;
				dataLine.ReconMailFee = 2.143m;
				dataLine.OriginalFreshLimes = 1.153m;
				dataLine.ReconFreshLimes = 2.153m;
				dataLine.OriginalHoney = 1.163m;
				dataLine.ReconHoney = 2.163m;
				dataLine.OriginalMango = 1.173m;
				dataLine.ReconMango = 2.173m;
				dataLine.OriginalMerchandiseInformal = 1.183m;
				dataLine.ReconMerchandiseInformal = 2.183m;
				dataLine.OriginalMerchandiseSurcharge = 1.193m;
				dataLine.ReconMerchandiseSurcharge = 2.193m;
				dataLine.OriginalMushroom = 1.223m;
				dataLine.ReconMushroom = 2.223m;
				dataLine.OriginalOtherAgencies = 1.323m;
				dataLine.ReconOtherAgencies = 2.323m;
				dataLine.OriginalRaspberry = 1.423m;
				dataLine.ReconRaspberry = 2.423m;
				dataLine.OriginalPork = 1.523m;
				dataLine.ReconPork = 2.523m;
				dataLine.OriginalPotato = 1.623m;
				dataLine.ReconPotato = 2.623m;
				dataLine.OriginalSoftwoodLumber = 1.723m;
				dataLine.ReconSoftwoodLumber = 2.723m;
				dataLine.OriginalTobacco = 1.823m;
				dataLine.ReconTobacco = 2.823m;
				dataLine.OriginalWatermelon = 1.923m;
				dataLine.ReconWatermelon = 2.923m;
				dataLine.OriginalWines = 3.123m;
				dataLine.ReconWines = 4.123m;
				dataLine.GoodsDescription = "HELLO WORLD";
				dataLine.HasNoLineDetails = "N";
				dataLine.MonthlyFiling = true;
				dataLine.FTAReconFiled = true;
				ReconEntryFlatFileDataTransferProcessor processor = new ReconEntryFlatFileDataTransferProcessor();
				processor.ImportReconDataFromCollection(collection, ReconDeclaration);
				ReconOriginalEntryHeader entry = ReconDeclaration.OriginalEntries[0];
				AssertEquals("XJ512345678", entry.CH_OrigEntryReference);
				AssertEquals("1101", entry.US_SchDEntry);
				AssertEquals("HELLO WORLD", entry.US_R_GoodsDescription);
				AssertEquals(ZBool.False, entry.US_R_NoLineDetails);
				AssertEquals(YesNoDefaultList.Codes.Yes, entry.US_R_IsHMFApplicable);
				AssertEquals(new ZDateTime(2008, 9, 11), entry.US_ImportDate);
				AssertEquals(new ZDateTime(2008, 9, 21), entry.US_PaymentDate);
				AssertEquals(new ZDateTime(2008, 9, 13), entry.US_R_ReleaseDate);
				AssertEquals(91m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Duty));
				AssertEquals(100.99m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Duty));
				AssertEquals(1.12m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
				AssertEquals(2.12m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
				AssertEquals(3.12m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF));
				AssertEquals(4.12m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF));
				AssertEquals(1.13m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Avocado));
				AssertEquals(2.13m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Avocado));
				AssertEquals(1.13m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Beef));
				AssertEquals(2.13m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Beef));
				AssertEquals(1.13m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Blueberry));
				AssertEquals(2.13m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Blueberry));
				AssertEquals(1.13m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton));
				AssertEquals(2.13m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton));
				AssertEquals(1.13m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.DairyFee));
				AssertEquals(2.13m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.DairyFee));
				AssertEquals(1.13m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.DistilledSpirits));
				AssertEquals(2.13m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.DistilledSpirits));
				AssertEquals(1.14m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.DutiableMail));
				AssertEquals(2.14m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.DutiableMail));
				AssertEquals(1.15m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.FreshLimes));
				AssertEquals(2.15m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.FreshLimes));
				AssertEquals(1.16m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Honey));
				AssertEquals(2.16m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Honey));
				AssertEquals(1.17m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Mango));
				AssertEquals(2.17m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Mango));
				AssertEquals(1.18m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal));
				AssertEquals(2.18m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal));
				AssertEquals(1.19m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge));
				AssertEquals(2.19m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge));
				AssertEquals(1.22m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Mushroom));
				AssertEquals(2.22m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Mushroom));
				AssertEquals(1.32m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherAgencies));
				AssertEquals(2.32m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherAgencies));
				AssertEquals(1.42m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Raspberry));
				AssertEquals(2.42m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Raspberry));
				AssertEquals(1.52m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Pork));
				AssertEquals(2.52m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Pork));
				AssertEquals(1.62m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Potato));
				AssertEquals(2.62m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Potato));
				AssertEquals(1.72m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber));
				AssertEquals(2.72m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber));
				AssertEquals(1.82m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Tobacco));
				AssertEquals(2.82m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Tobacco));
				AssertEquals(1.92m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Watermelon));
				AssertEquals(2.92m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Watermelon));
				AssertEquals(3.12m, entry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Wines));
				AssertEquals(4.12m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Wines));
				AssertEquals(true, entry.US_R_MonthlyFiling);
				AssertEquals(true, entry.US_NAFTAReconIndicator);
				dataLine.OriginalHMF = 0m;
				dataLine.GoodsDescription = ZString.Empty;
				processor.ImportReconDataFromCollection(collection, ReconDeclaration);
				entry = ReconDeclaration.OriginalEntries[0];
				AssertEquals("HELLO WORLD", entry.US_R_GoodsDescription);
				AssertEquals(ZBool.False, entry.US_R_NoLineDetails);
				AssertEquals(YesNoDefaultList.Codes.No, entry.US_R_IsHMFApplicable);
				dataLine.HasNoLineDetails = "";
				processor.ImportReconDataFromCollection(collection, ReconDeclaration);
				dataLine.OriginalHMF = 50m;
				AssertEquals("", entry.US_R_IsHMFApplicable);
				AssertEquals(ZBool.True, entry.US_R_NoLineDetails);
			}
		}

		void AddAllChargesToEntry(ReconOriginalEntryHeader entry)
		{
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 10);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 20);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 101);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 201);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.HMF, 102);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.HMF, 202);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Avocado, 103);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Avocado, 203);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Beef, 104);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Beef, 204);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Blueberry, 105);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Blueberry, 205);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Cotton, 106);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Cotton, 206);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.DairyFee, 6);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.DairyFee, 7);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, 107);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, 207);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.DutiableMail, 108);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.DutiableMail, 208);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.FreshLimes, 190);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.FreshLimes, 209);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Honey, 100);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Honey, 200);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Mango, 110);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Mango, 120);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal, 210);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal, 220);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge, 310);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge, 320);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Mushroom, 410);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Mushroom, 420);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.OtherAgencies, 510);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.OtherAgencies, 520);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Raspberry, 610);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Raspberry, 620);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Pork, 710);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Pork, 720);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Potato, 810);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Potato, 820);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber, 910);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber, 920);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Tobacco, 10);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Tobacco, 20);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Watermelon, 130);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Watermelon, 230);
			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Wines, 140);
			entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Wines, 240);
		}

		void AssertAllChargesExported(ReconFlattenedDataLine dataLine)
		{
			AssertEquals(10m, dataLine.OriginalDuty);
			AssertEquals(20m, dataLine.ReconDuty);
			AssertEquals(101m, dataLine.OriginalMPF);
			AssertEquals(201m, dataLine.ReconMPF);
			AssertEquals(102m, dataLine.OriginalHMF);
			AssertEquals(202m, dataLine.ReconHMF);
			AssertEquals(103m, dataLine.OriginalAVO);
			AssertEquals(203m, dataLine.ReconAVO);
			AssertEquals(104m, dataLine.OriginalBeef);
			AssertEquals(204m, dataLine.ReconBeef);
			AssertEquals(105m, dataLine.OriginalBlueberry);
			AssertEquals(205m, dataLine.ReconBlueberry);
			AssertEquals(106m, dataLine.OriginalCotton);
			AssertEquals(206m, dataLine.ReconCotton);
			AssertEquals(6m, dataLine.OriginalDairy);
			AssertEquals(7m, dataLine.ReconDairy);
			AssertEquals(107m, dataLine.OriginalDistilledSpirits);
			AssertEquals(207m, dataLine.ReconDistilledSpirits);
			AssertEquals(108m, dataLine.OriginalMailFee);
			AssertEquals(208m, dataLine.ReconMailFee);
			AssertEquals(190m, dataLine.OriginalFreshLimes);
			AssertEquals(209m, dataLine.ReconFreshLimes);
			AssertEquals(100m, dataLine.OriginalHoney);
			AssertEquals(200m, dataLine.ReconHoney);
			AssertEquals(110m, dataLine.OriginalMango);
			AssertEquals(120m, dataLine.ReconMango);
			AssertEquals(210m, dataLine.OriginalMerchandiseInformal);
			AssertEquals(220m, dataLine.ReconMerchandiseInformal);
			AssertEquals(310m, dataLine.OriginalMerchandiseSurcharge);
			AssertEquals(320m, dataLine.ReconMerchandiseSurcharge);
			AssertEquals(410m, dataLine.OriginalMushroom);
			AssertEquals(420m, dataLine.ReconMushroom);
			AssertEquals(510m, dataLine.OriginalOtherAgencies);
			AssertEquals(520m, dataLine.ReconOtherAgencies);
			AssertEquals(610m, dataLine.OriginalRaspberry);
			AssertEquals(620m, dataLine.ReconRaspberry);
			AssertEquals(710m, dataLine.OriginalPork);
			AssertEquals(720m, dataLine.ReconPork);
			AssertEquals(810m, dataLine.OriginalPotato);
			AssertEquals(820m, dataLine.ReconPotato);
			AssertEquals(910m, dataLine.OriginalSoftwoodLumber);
			AssertEquals(920m, dataLine.ReconSoftwoodLumber);
			AssertEquals(10m, dataLine.OriginalTobacco);
			AssertEquals(20m, dataLine.ReconTobacco);
			AssertEquals(130m, dataLine.OriginalWatermelon);
			AssertEquals(230m, dataLine.ReconWatermelon);
			AssertEquals(140m, dataLine.OriginalWines);
			AssertEquals(240m, dataLine.ReconWines);
		}

		ReconDeclaration reconDeclaration;
		ReconDeclaration ReconDeclaration
		{
			get
			{
				if (reconDeclaration == null)
				{
					reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
					reconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.ValueRecon;
				}

				return reconDeclaration;
			}
		}
	}
}
