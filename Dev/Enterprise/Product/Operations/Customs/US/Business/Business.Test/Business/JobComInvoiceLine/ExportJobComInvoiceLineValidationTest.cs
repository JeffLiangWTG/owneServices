using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ExportJobComInvoiceLineValidationTest : JobComInvoiceLineValidationTest
	{
		public void TestCheckJI_CustomsSecondQuantity_PHC()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
			invoiceLine.JI_CustomsSecondUnitQty = AESUnitOfMeasureList.Codes.Kilograms;
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.JI_Weight = 0m;
			AssertNoMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, ExportJobComInvoiceLineValidation.QtyCannotExceedShipmentWeight);

			declaration.JE_TransportMode = TransportTypeList.Codes.Pedestrian;
			invoiceLine.JI_CustomsSecondUnitQty = AESUnitOfMeasureList.Codes.Kilograms;
			invoiceLine.JI_CustomsSecondQuantity = 2m;
			invoiceLine.JI_Weight = 0m;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsSecondQuantityInfo, ExportJobComInvoiceLineValidation.QtyCannotExceedShipmentWeight);
		}

		public void TestCheckJI_CustomsSecondQuantity()
		{
			invoiceLine.JI_CustomsSecondUnitQty = "";
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			AssertEquals("NeedsSecondCustomsQuantity", false, invoiceLine.NeedsSecondCustomsQuantity);
			AssertNoMessageErrorContaining(invoiceLine.JI_CustomsSecondQuantityInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, ExportJobComInvoiceLineValidation.NoSecondQtyRequired);

			invoiceLine.JI_CustomsSecondUnitQty = AESUnitOfMeasureList.Codes.Number;
			invoiceLine.JI_CustomsSecondQuantity = 0m;
			AssertEquals("NeedsSecondCustomsQuantity", true, invoiceLine.NeedsSecondCustomsQuantity);
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsSecondQuantityInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, ExportJobComInvoiceLineValidation.NoSecondQtyRequired);

			invoiceLine.JI_CustomsSecondQuantity = 100000000m;
			AssertHasMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, ExportJobComInvoiceLineValidation.MaxValueExceeded);

			invoiceLine.JI_CustomsSecondQuantity = 99999999m;
			AssertNoMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, ExportJobComInvoiceLineValidation.MaxValueExceeded);

			invoiceLine.JI_CustomsSecondUnitQty = AESUnitOfMeasureList.Codes.Kilograms;
			invoiceLine.JI_Weight = 500m;
			invoiceLine.JI_CustomsSecondQuantity = 515m;
			AssertHasMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, ExportJobComInvoiceLineValidation.QtyCannotExceedShipmentWeight);

			invoiceLine.JI_CustomsSecondQuantity = 500m;
			AssertNoMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, ExportJobComInvoiceLineValidation.QtyCannotExceedShipmentWeight);
		}

		public void TestCheckJI_CustomsSecondUnitQty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusTariffTypeSB = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			var cusTariffHTS = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.Export);
			Factory.Save();

			var scheduleB = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, "2401208010", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var scheduleB2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, "1401208010", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddDays(-20));
			var scheduleHTS = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffHTS.PK, "2401208010", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			Factory.Save();

			AssertNotNull("EXP tariff do not affect to STB tariff validation", scheduleHTS);
			helper.CreateTariffUOM(scheduleHTS, "CU1", "KG");

			scheduleB.ZZ1_Description = "Short description";
			helper.CreateTariffUOM(scheduleB, "CU1", "KG");
			helper.CreateTariffUOM(scheduleB, "CU2", "CKG");

			scheduleB2.ZZ1_Description = "Short description";
			helper.CreateTariffUOM(scheduleB2, "CU1", "KG");
			helper.CreateTariffUOM(scheduleB2, "CU2", "CKG");

			invoiceLine.US_TariffType = TariffTypeList.Codes.ScheduleB;
			invoiceLine.JI_Tariff = "0000000000";
			invoiceLine.JI_CustomsSecondUnitQty = "X";
			AssertNoMessageError(invoiceLine.JI_CustomsSecondUnitQtyInfo, ExportJobComInvoiceLineValidation.SecondUQDoesNotMatchTariffUQ);

			invoiceLine.JI_Tariff = scheduleB.ZZ1_TariffCode;
			invoiceLine.JI_CustomsSecondUnitQty = "X";
			AssertHasMessageError(invoiceLine.JI_CustomsSecondUnitQtyInfo, ExportJobComInvoiceLineValidation.SecondUQDoesNotMatchTariffUQ);

			invoiceLine.JI_CustomsSecondUnitQty = "CKG";
			AssertNoMessageError(invoiceLine.JI_CustomsSecondUnitQtyInfo, ExportJobComInvoiceLineValidation.SecondUQDoesNotMatchTariffUQ);

			invoiceLine.US_TariffType = TariffTypeList.Codes.HTS;
			invoiceLine.JI_CustomsSecondUnitQty = "X";
			AssertNoMessageError(invoiceLine.JI_CustomsSecondUnitQtyInfo, ExportJobComInvoiceLineValidation.SecondUQDoesNotMatchTariffUQ);

			invoiceLine.US_TariffType = TariffTypeList.Codes.ScheduleB;
			invoiceLine.JI_Tariff = scheduleB2.ZZ1_TariffCode;
			invoiceLine.JI_CustomsSecondUnitQty = "X";
			AssertHasMessageError(invoiceLine.JI_CustomsSecondUnitQtyInfo, ExportJobComInvoiceLineValidation.SecondUQDoesNotMatchTariffUQ);

			invoiceLine.JI_CustomsSecondUnitQty = "CKG";
			AssertNoMessageError(invoiceLine.JI_CustomsSecondUnitQtyInfo, ExportJobComInvoiceLineValidation.SecondUQDoesNotMatchTariffUQ);
		}

		public void TestUS_ExportCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusTariffTypeSB = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();

			var scheduleB = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, "0000000000", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTariffUOM(scheduleB, "CU1", "LB");
			helper.CreateTariffUOM(scheduleB, "CU2", "C3");

			invoiceLine.US_ExportCode = "";
			invoiceLine.JI_Tariff = "";
			invoiceLine.JI_CustomsQuantity = 0m;
			invoiceLine.JI_CustomsSecondQuantity = 0m;
			AssertEquals("NeedsCustomsQuantity", false, invoiceLine.NeedsCustomsQuantity);
			AssertEquals("NeedsSecondCustomsQuantity", false, invoiceLine.NeedsSecondCustomsQuantity);
			AssertHasMessageErrors(invoiceLine.JI_TariffInfo);
			AssertNoMessageErrors(invoiceLine.JI_CustomsQuantityInfo);
			AssertNoMessageErrors(invoiceLine.JI_CustomsSecondQuantityInfo);

			invoiceLine.US_ExportCode = LimitedReportingExportInformationCodeList.Codes.TE;
			AssertNoMessageErrors(invoiceLine.JI_TariffInfo);
			AssertNoMessageErrors(invoiceLine.JI_CustomsQuantityInfo);
			AssertNoMessageErrors(invoiceLine.JI_CustomsSecondQuantityInfo);

			invoiceLine.US_ExportCode = ExportInformationCodeList.Codes.ZD;
			AssertHasMessageErrors(invoiceLine.JI_TariffInfo);
			AssertNoMessageErrors(invoiceLine.JI_CustomsQuantityInfo);
			AssertNoMessageErrors(invoiceLine.JI_CustomsSecondQuantityInfo);

			invoiceLine.JI_Tariff = scheduleB.ZZ1_TariffCode;
			AssertEquals("NeedsCustomsQuantity", true, invoiceLine.NeedsCustomsQuantity);
			AssertEquals("NeedsSecondCustomsQuantity", true, invoiceLine.NeedsSecondCustomsQuantity);
			invoiceLine.US_ExportCode = LimitedReportingExportInformationCodeList.Codes.TE;
			AssertNoMessageErrors(invoiceLine.JI_TariffInfo);
			AssertNoMessageErrors(invoiceLine.JI_CustomsQuantityInfo);
			AssertNoMessageErrors(invoiceLine.JI_CustomsSecondQuantityInfo);

			invoiceLine.US_ExportCode = ExportInformationCodeList.Codes.ZD;
			AssertNoMessageErrors(invoiceLine.JI_TariffInfo);
			AssertHasMessageErrors(invoiceLine.JI_CustomsQuantityInfo);
			AssertHasMessageErrors(invoiceLine.JI_CustomsSecondQuantityInfo);
		}

		public void TestWarnIfTariffIsOutDatedForExportSHB()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.Export);
			var shBTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();
			var expTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, expTariffType.PK, "0000000000", new ZDateTime(2009, 1, 1), new ZDateTime(2009, 6, 30));
			var shbTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, shBTariffType.PK, "0000000001", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_TariffType = TariffTypeList.Codes.HTS;
			declaration.Invoices.AddNew();

			var todayAdd2 = ZDateTime.Today.AddDays(2);
			var invoiceLine = declaration.InvoiceLines.AddNew();

			invoiceLine.US_DateOfExport = todayAdd2;
			invoiceLine.US_TariffType = TariffTypeList.Codes.ScheduleB;
			invoiceLine.JI_Tariff = "0000000001";

			AssertNull(invoiceLine.ScheduleBTariff);
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff was found but is not valid for " + todayAdd2.ToShortDateString());
			AssertHasWarningContaining(invoiceLine.JI_TariffInfo, TariffValidator.TariffExpiredButCanBeUsedWithin30Days);
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, ListValidation.InvalidCodeMessageError.ToString());

			invoiceLine.US_DateOfExport = ZDateTime.Empty;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasWarningContaining(invoiceLine.JI_TariffInfo, TariffValidator.TariffExpiredButCanBeUsedWithin30Days);
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, ListValidation.InvalidCodeMessageError.ToString());

			var todayAdd35 = ZDateTime.Today.AddDays(35);
			invoiceLine.US_DateOfExport = todayAdd35;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff was found but is not valid for " + todayAdd35.ToShortDateString());
			AssertNoWarningContaining(invoiceLine.JI_TariffInfo, TariffValidator.TariffExpiredButCanBeUsedWithin30Days);
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, ListValidation.InvalidCodeMessageError.ToString());

			invoiceLine.US_TariffType = TariffTypeList.Codes.HTS;
			invoiceLine.US_DateOfExport = todayAdd2;
			invoiceLine.JI_Tariff = "0000000000";

			AssertNull(invoiceLine.ExportTariff);
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff was found but is not valid for " + todayAdd2.ToShortDateString());
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, ListValidation.InvalidCodeMessageError.ToString());
		}

		public void TestCheckJI_TariffTariffExpirationDateWinin30DaysHTS()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.Export);
			Factory.Save();
			var htsTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, expTariffType.PK, "0000000000", new ZDateTime(2009, 1, 1), new ZDateTime(2023, 12, 01));
			var expired5daysagoTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, expTariffType.PK, "0000000001", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(-5));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_TariffType = TariffTypeList.Codes.HTS;
			declaration.Invoices.AddNew();

			var todayAdd2 = ZDateTime.Today.AddDays(2);
			var invoiceLine = declaration.InvoiceLines.AddNew();

			invoiceLine.US_TariffType = TariffTypeList.Codes.HTS;
			invoiceLine.US_DateOfExport = todayAdd2;
			invoiceLine.JI_Tariff = "0000000000";

			AssertNull(invoiceLine.ExportTariff);
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff was found but is not valid for " + todayAdd2.ToShortDateString());
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, ListValidation.InvalidCodeMessageError.ToString());

			invoiceLine.US_DateOfExport = todayAdd2;
			invoiceLine.US_TariffType = TariffTypeList.Codes.HTS;
			invoiceLine.JI_Tariff = "0000000001";

			AssertNull(invoiceLine.ExportTariff);
			AssertHasWarningContaining(invoiceLine.JI_TariffInfo, TariffValidator.TariffExpiredButCanBeUsedWithin30Days);
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, ListValidation.InvalidCodeMessageError.ToString());
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff was found but is not valid for " + todayAdd2.ToShortDateString());

			invoiceLine.US_DateOfExport = ZDateTime.Empty;
			invoiceLine.Validation.ValidateJI_Tariff();

			var todayAdd35 = ZDateTime.Today.AddDays(35);
			invoiceLine.US_DateOfExport = todayAdd35;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff was found but is not valid for " + todayAdd35.ToShortDateString());
			AssertNoWarningContaining(invoiceLine.JI_TariffInfo, TariffValidator.TariffExpiredButCanBeUsedWithin30Days);
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, ListValidation.InvalidCodeMessageError.ToString());
		}

		public void TestCheckJI_CustomsQuantity()
		{
			invoiceLine.JI_CustomsQuantity = 100000000m;
			AssertHasMessageError(invoiceLine.JI_CustomsQuantityInfo, ExportJobComInvoiceLineValidation.MaxValueExceeded);

			invoiceLine.JI_CustomsQuantity = 99999999m;
			AssertNoMessageError(invoiceLine.JI_CustomsQuantityInfo, ExportJobComInvoiceLineValidation.MaxValueExceeded);

			invoiceLine.US_IsUsedVehicle = true;
			invoiceLine.JI_CustomsQuantity = 2m;
			AssertHasMessageError(invoiceLine.JI_CustomsQuantityInfo, ExportJobComInvoiceLineValidation.UsedVehicleQtyExceeded);

			invoiceLine.JI_CustomsQuantity = 1m;
			AssertNoMessageError(invoiceLine.JI_CustomsQuantityInfo, ExportJobComInvoiceLineValidation.UsedVehicleQtyExceeded);
		}

		public void TestCheckJI_CustomsQuantityForStandaloneInvoice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffType.PK, "7202300000", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTariffUOM(tariff, "CU1", "KG");
			Factory.Save();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "7202300000";
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			invoiceLine.JI_CustomsQuantity = 1250m;
			invoiceLine.JI_CustomsUnitQty = invoiceLine.JI_WeightUQ;
			AssertHasMessageError(invoiceLine.JI_CustomsQuantityInfo, ExportJobComInvoiceLineValidation.ExportWeightImbalance);

			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsUnitQty = invoiceLine.JI_WeightUQ;
			AssertNoMessageError(invoiceLine.JI_CustomsQuantityInfo, ExportJobComInvoiceLineValidation.ExportWeightImbalance);
		}

		public void TestCheckCustomsQtyWeightImbalance()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffType.PK, "7202300000", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTariffUOM(tariff, "CU1", "KG");
			Factory.Save();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			invoiceLine.JI_Tariff = "7202300000";
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertNoMessageError(invoiceLine.JI_CustomsQuantityInfo, ExportJobComInvoiceLineValidation.ExportWeightImbalance);

			invoiceLine.JI_CustomsQuantity = 1250m;
			invoiceLine.JI_CustomsUnitQty = invoiceLine.JI_WeightUQ;
			AssertHasMessageError(invoiceLine.JI_CustomsQuantityInfo, ExportJobComInvoiceLineValidation.ExportWeightImbalance);

			invoiceLine.JI_CustomsQuantity = 950m;
			AssertNoMessageError(invoiceLine.JI_CustomsQuantityInfo, ExportJobComInvoiceLineValidation.ExportWeightImbalance);

			declaration.US_CommodityFilingOption = AESCommodityFilingOptionList.Codes._4Postdeparture;
			invoiceLine.JI_CustomsQuantity = 1250m;
			AssertHasMessageError(invoiceLine.JI_CustomsQuantityInfo, ExportJobComInvoiceLineValidation.ExportWeightImbalance);
		}

		public void TestCheckJI_InvoiceUQ()
		{
			invoiceLine.JI_InvoiceUQ = "~";
			AssertHasMessageError(invoiceLine.JI_InvoiceUQInfo, ExportJobComInvoiceLineValidation.InvoiceUQShouldBeInList);
			invoiceLine.JI_InvoiceUQ = invoiceLine.Lookups.CustomsUQList[0].Code;
			AssertNoMessageError(invoiceLine.JI_InvoiceUQInfo, ExportJobComInvoiceLineValidation.InvoiceUQShouldBeInList);
		}

		public void TestCheckJI_NetWeightUQ()
		{
			invoiceLine.JI_NetWeightUQ = "~";
			AssertHasMessageError(invoiceLine.JI_NetWeightUQInfo, ExportJobComInvoiceLineValidation.NetWeightUQShouldBeInList);
			invoiceLine.JI_NetWeightUQ = invoiceLine.Lookups.WeightUQList[0].Code;
			AssertNoMessageError(invoiceLine.JI_NetWeightUQInfo, ExportJobComInvoiceLineValidation.NetWeightUQShouldBeInList);
		}

		public void TestCheckJI_Tariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.US_ExportCode = LimitedReportingExportInformationCodeList.Codes.DD;
			AssertEquals("IsExportDeclaration", false, line.IsExport);
			AssertEquals("IsLimitedReportingExportCode", true, line.IsLimitedReportingExportCode);

			string errorMessage = "Tariff may not be empty";
			line.JI_Tariff = "";
			AssertHasMessageError(line.JI_TariffInfo, errorMessage);

			line.JI_Tariff = "0146456789";
			AssertNoMessageError(line.JI_TariffInfo, errorMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("IsExportDeclaration", true, line.IsExport);
			AssertEquals("IsLimitedReportingExportCode", true, line.IsLimitedReportingExportCode);
			line.JI_Tariff = "";
			AssertNoMessageError(line.JI_TariffInfo, errorMessage);

			line.US_ExportCode = "";
			AssertEquals("IsLimitedReportingExportCode", false, line.IsLimitedReportingExportCode);
			line.JI_Tariff = "";
			AssertHasMessageError(line.JI_TariffInfo, errorMessage);

			line.JI_Tariff = "0146456789";
			AssertNoMessageError(line.JI_TariffInfo, errorMessage);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.Export);
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffType.PK, "0146456789", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

			line.US_TariffType = TariffTypeList.Codes.HTS;
			line.JI_Tariff = "0146456789";
			AssertNoMessageErrors(line.JI_TariffInfo);
		}

		public void TestCheckJI_TariffIsValidWhenItIsNotEmpty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.Export);
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffType.PK, "8703230000", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(-40));
			helper.CreateTariffUOM(tariff, "CU1", "L");
			helper.CreateTariffUOM(tariff, "CU2", "M");
			helper.CreateTariffUOM(tariff, "CU3", "N");
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffType.PK, "8703230001", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(20));
			helper.CreateTariffUOM(tariff2, "CU1", "P");
			helper.CreateTariffUOM(tariff2, "CU2", "Q");
			helper.CreateTariffUOM(tariff2, "CU3", "R");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_TariffType = TariffTypeList.Codes.HTS;

			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "BOOZE";
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, TariffValidator.GetTariffFoundButNotValid("Tariff", ZDateTime.Today.ToShortDateString()));
			AssertNullOrEmpty(invoiceLine.JI_CustomsUnitQty);
			AssertNullOrEmpty(invoiceLine.JI_CustomsSecondUnitQty);
			AssertNullOrEmpty(invoiceLine.JI_CustomsThirdUnitQty);

			invoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
			AssertNoMessageErrors(invoiceLine.JI_TariffInfo);
			AssertNoWarnings(invoiceLine.JI_TariffInfo);
			AssertEquals(invoiceLine.JI_CustomsUnitQty, "P");
			AssertEquals(invoiceLine.JI_CustomsSecondUnitQty, "Q");
			AssertEquals(invoiceLine.JI_CustomsThirdUnitQty, "R");

			declaration.US_TariffType = TariffTypeList.Codes.ScheduleB;
			invoiceLine.JI_Tariff = "00002020";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, "The code you have selected is not in the list.");
			AssertNoWarnings(invoiceLine.JI_TariffInfo);
			CombineAssertions("Defaulted values should not be emptied when selecting an invalid tariff.", () =>
			{
				AssertEquals(invoiceLine.JI_CustomsUnitQty, "P");
				AssertEquals(invoiceLine.JI_CustomsSecondUnitQty, "Q");
				AssertEquals(invoiceLine.JI_CustomsThirdUnitQty, "R");
			});
		}

		public void TestCheckJI_LinePrice()
		{
			invoiceLine.JI_LinePrice = 0m;
			AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.JI_LinePrice = 0m;
			AssertNoMessageErrorContaining(invoiceLine.JI_LinePriceInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_LinePrice = 1m;
			AssertNoMessageErrorContaining(invoiceLine.JI_LinePriceInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_Tariff = "8802110045";
			invoiceLine.JI_LinePrice = 1000000000m;
			AssertHasMessageError("All commodities should give message error when over $999,999,999", invoiceLine.JI_LinePriceInfo, ExportJobComInvoiceLineValidation.ValueExceedsLimitChapter88Goods);

			invoiceLine.JI_Tariff = USCTariff.CottonFeeApplicable;
			invoiceLine.JI_LinePrice = 500000000m;
			AssertHasMessageError("All Non Chapter 88 commodities should give message error when over $499,999,999", invoiceLine.JI_LinePriceInfo, ExportJobComInvoiceLineValidation.ValueExceedsLimitNonChapter88Goods);

			invoiceLine.JI_Tariff = "8802110045";
			invoiceLine.JI_LinePrice = 999000000m;
			AssertNoMessageError(invoiceLine.JI_LinePriceInfo, ExportJobComInvoiceLineValidation.ValueExceedsLimitNonChapter88Goods);
			AssertNoMessageError(invoiceLine.JI_LinePriceInfo, ExportJobComInvoiceLineValidation.ValueExceedsLimitChapter88Goods);

			invoiceLine.JI_LinePrice = -100m;
			AssertHasErrors(invoiceLine.JI_LinePriceInfo);
		}

		public void TestJI_CustomsQuantityAndWeightUQWhenDeclarationIsPHC()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();

			line.JI_WeightUQ = "KG";
			line.JI_CustomsUnitQty = "KG";
			AssertNoMessageError(line.JI_CustomsQuantityInfo, ExportJobComInvoiceLineValidation.ExportWeightImbalance);
			AssertNoMessageError(line.JI_WeightInfo, JobDeclaration.Constants.MessageErrorOrWarningGrossWeightNotAllowedWhenMOTIsPHC);

			line.JI_CustomsQuantity = 11;
			AssertNoMessageError(line.JI_CustomsQuantityInfo, ExportJobComInvoiceLineValidation.ExportWeightImbalance);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			line.JI_CustomsQuantity = 111;
			AssertHasMessageError(line.JI_CustomsQuantityInfo, ExportJobComInvoiceLineValidation.ExportWeightImbalance);

			declaration.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
			line.JI_Weight = 20;
			AssertHasMessageError(line.JI_WeightInfo, JobDeclaration.Constants.MessageErrorOrWarningGrossWeightNotAllowedWhenMOTIsPHC);
		}

		public void TestJI_WeightRequirement()
		{
			var list = new TransportTypeList();
			foreach (var transportMode in new[] { TransportTypeList.Codes.Air, TransportTypeList.Codes.BorderWaterBorne, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Truck, TransportTypeList.Codes.Sea })
			{
				list.RemoveCode(transportMode);
				declaration.JE_TransportMode = transportMode;
				invoiceLine.US_ExportCode = ExportInformationCodeList.Codes.OS;
				AssertEquals(true, invoiceLine.IsWeightRequired());
				invoiceLine.JI_Weight = ZDecimal.Zero;
				AssertHasMessageError(invoiceLine.JI_WeightInfo, ExportJobComInvoiceLineValidation.WeightRequired);
			}

			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			invoiceLine.US_ExportCode = ExportInformationCodeList.Codes.OS;
			AssertEquals(false, invoiceLine.IsWeightRequired());
			invoiceLine.JI_Weight = ZDecimal.Zero;
			AssertNoMessageError(invoiceLine.JI_WeightInfo, ExportJobComInvoiceLineValidation.WeightRequired);

			invoiceLine.US_ExportCode = ExportInformationCodeList.Codes.HH;
			AssertEquals(false, invoiceLine.IsWeightRequired());
			invoiceLine.JI_Weight = ZDecimal.Zero;
			AssertNoMessageError(invoiceLine.JI_WeightInfo, ExportJobComInvoiceLineValidation.WeightRequired);

			list.RemoveCode(TransportTypeList.Codes.FixedTransportInstallations);

			foreach (ICodeDescription pair in list)
			{
				declaration.JE_TransportMode = pair.Code;
				invoiceLine.US_ExportCode = ExportInformationCodeList.Codes.OS;
				AssertEquals(false, invoiceLine.IsWeightRequired());
				invoiceLine.JI_Weight = ZDecimal.Zero;
				AssertNoMessageError(invoiceLine.JI_WeightInfo, ExportJobComInvoiceLineValidation.WeightRequired);

				invoiceLine.US_ExportCode = ExportInformationCodeList.Codes.HH;
				AssertEquals(true, invoiceLine.IsWeightRequired());
				invoiceLine.JI_Weight = ZDecimal.Zero;
				if (!declaration.IsHandCarry)
				{
					AssertHasMessageError(invoiceLine.JI_WeightInfo, ExportJobComInvoiceLineValidation.WeightRequired);
				}
			}
		}

		public void TestCheckJI_Weight()
		{
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_Weight = ZDecimal.Zero;
			invoiceLine.JI_Weight = ZDecimal.Zero;
			AssertHasMessageError(invoiceLine.JI_WeightInfo, ExportJobComInvoiceLineValidation.WeightRequired);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.JI_Weight = ZDecimal.Zero;
			AssertNoMessageError(invoiceLine.JI_WeightInfo, ExportJobComInvoiceLineValidation.WeightRequired);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_Weight = 1m;
			AssertNoMessageError(invoiceLine.JI_WeightInfo, ExportJobComInvoiceLineValidation.WeightRequired);

			invoiceLine.JI_Weight = ZDecimal.Zero;
			AssertHasMessageError(invoiceLine.JI_WeightInfo, ExportJobComInvoiceLineValidation.WeightRequired);

			invoice.JZ_Weight = 1m;
			invoiceLine.JI_LinePrice = 1m;
			invoiceLine.JI_Weight = ZDecimal.Zero;
			AssertNoMessageError(invoiceLine.JI_WeightInfo, ExportJobComInvoiceLineValidation.WeightRequired);

			declaration.RunPreSaveValidation();
			AssertNoMessageError(invoiceLine.JI_WeightInfo, ExportJobComInvoiceLineValidation.WeightRequired);

			invoice.JZ_Weight = ZDecimal.Zero;
			declaration.RunPreSaveValidation();
			AssertHasMessageError(invoiceLine.JI_WeightInfo, ExportJobComInvoiceLineValidation.WeightRequired);
		}

		public void TestCheckJI_WeightThresholds()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			invoiceLine.JI_Weight = 200000.5;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			AssertHasMessageError(invoiceLine.JI_WeightInfo, ExportJobComInvoiceLineValidation.VesselWeightThresholdExceeded);

			invoiceLine.JI_Weight = 200000;
			AssertNoMessageError(invoiceLine.JI_WeightInfo, ExportJobComInvoiceLineValidation.VesselWeightThresholdExceeded);

			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			invoiceLine.JI_Weight = 25001;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			AssertHasMessageError(invoiceLine.JI_WeightInfo, ExportJobComInvoiceLineValidation.TruckWeightThresholdExceeded);

			invoiceLine.JI_Weight = 25000;
			AssertNoMessageError(invoiceLine.JI_WeightInfo, ExportJobComInvoiceLineValidation.TruckWeightThresholdExceeded);

			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			invoiceLine.JI_Weight = 30001;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			AssertHasMessageError(invoiceLine.JI_WeightInfo, ExportJobComInvoiceLineValidation.RailWeightThresholdExceeded);

			invoiceLine.JI_Weight = 30000;
			AssertNoMessageError(invoiceLine.JI_WeightInfo, ExportJobComInvoiceLineValidation.RailWeightThresholdExceeded);
		}

		public void TestCheckJI_CustomsUnitQty()
		{
			declaration.US_TariffType = TariffTypeList.Codes.ScheduleB;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusTariffTypeSB = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			var cusTariffTypeHTS = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.Export);
			Factory.Save();

			var scheduleB = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, "2401208010", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var scheduleB2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, "1401208010", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddDays(-20));
			Factory.Save();

			scheduleB.ZZ1_Description = "Short description";
			helper.CreateTariffUOM(scheduleB, "CU1", "KG");
			helper.CreateTariffUOM(scheduleB, "CU2", "CKG");

			scheduleB2.ZZ1_Description = "Short description";
			helper.CreateTariffUOM(scheduleB2, "CU1", "KG");
			helper.CreateTariffUOM(scheduleB2, "CU2", "CKG");

			invoiceLine.US_TariffType = Universal.Constants.TariffTypes.ScheduleB;
			invoiceLine.JI_Tariff = "0000000000";
			invoiceLine.JI_CustomsUnitQty = "X";
			AssertNoMessageError(invoiceLine.JI_CustomsUnitQtyInfo, ExportJobComInvoiceLineValidation.CustomsUnitQtyDoesNotMatchTariffUQ);

			invoiceLine.JI_Tariff = scheduleB.ZZ1_TariffCode;
			invoiceLine.JI_CustomsUnitQty = "X";
			AssertHasMessageError(invoiceLine.JI_CustomsUnitQtyInfo, ExportJobComInvoiceLineValidation.CustomsUnitQtyDoesNotMatchTariffUQ);

			invoiceLine.JI_CustomsUnitQty = "KG";
			AssertNoMessageError(invoiceLine.JI_CustomsUnitQtyInfo, ExportJobComInvoiceLineValidation.CustomsUnitQtyDoesNotMatchTariffUQ);

			invoiceLine.US_TariffType = Universal.Constants.TariffTypes.Export;
			invoiceLine.JI_Tariff = scheduleB.ZZ1_TariffCode;
			invoiceLine.JI_CustomsUnitQty = "X";
			AssertNoMessageError(invoiceLine.JI_CustomsUnitQtyInfo, ExportJobComInvoiceLineValidation.CustomsUnitQtyDoesNotMatchTariffUQ);

			invoiceLine.JI_CustomsUnitQty = "KG";
			AssertNoMessageError(invoiceLine.JI_CustomsUnitQtyInfo, ExportJobComInvoiceLineValidation.CustomsUnitQtyDoesNotMatchTariffUQ);

			invoiceLine.US_TariffType = Universal.Constants.TariffTypes.ScheduleB;
			invoiceLine.JI_Tariff = scheduleB2.ZZ1_TariffCode;
			invoiceLine.JI_CustomsUnitQty = "X";
			AssertHasMessageError(invoiceLine.JI_CustomsUnitQtyInfo, ExportJobComInvoiceLineValidation.CustomsUnitQtyDoesNotMatchTariffUQ);
		}

		public void TestCheckJI_Description()
		{
			invoiceLine.Validation.ValidateJI_Description();
			AssertNoMessageErrorContaining(invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.Validation.ValidateJI_Description();
			AssertHasMessageErrorContaining(invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.US_NMFSHMSInd = ZString.Empty;
			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.Validation.ValidateJI_Description();
			AssertHasMessageErrorContaining(invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.US_ATFInd = ZString.Empty;
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.Validation.ValidateJI_Description();
			AssertHasMessageErrorContaining(invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestValidateJI_Description_LicenseTypeECCNChanged()
		{
			invoiceLine.Validation.ValidateJI_Description();
			AssertNoWarningContaining(string.Format("LicenseType: {0}, ECCN: {1}", invoiceLine.US_LicenseType, invoiceLine.US_ECCN), invoiceLine.JI_DescriptionInfo, ExportJobComInvoiceLineValidation.InvalidBeginningForGoodDescription);

			invoiceLine.JI_Description = "123";
			invoiceLine.US_LicenseType = "C67";
			AseertSpecialConditions(true);
			invoiceLine.US_LicenseType = "C68";
			AseertSpecialConditions(true);
			invoiceLine.US_LicenseType = "C69";
			AseertSpecialConditions(false);

			invoiceLine.JI_Description = ".Z 123";
			invoiceLine.US_LicenseType = "C67";
			AseertSpecialConditions(false);
			invoiceLine.US_LicenseType = "C68";
			AseertSpecialConditions(false);
			invoiceLine.US_LicenseType = "C69";
			AseertSpecialConditions(false);

			void AseertSpecialConditions(bool hasWarning)
			{
				ZString[] eccns = { "3A001", "3A002", "4A003", "4A004", "4A005", "5A002", "5A004", "5A992", "5D002", "5D992" };
				foreach (var eccn in eccns)
				{
					invoiceLine.US_ECCN = eccn;
					if (eccn == "3A002")
					{
						AssertNoWarningContaining(string.Format("LicenseType: {0}, ECCN: {1}", invoiceLine.US_LicenseType, invoiceLine.US_ECCN), invoiceLine.JI_DescriptionInfo, ExportJobComInvoiceLineValidation.InvalidBeginningForGoodDescription);
						continue;
					}

					if (hasWarning)
					{
						AssertHasWarningContaining(string.Format("LicenseType: {0}, ECCN: {1}", invoiceLine.US_LicenseType, invoiceLine.US_ECCN), invoiceLine.JI_DescriptionInfo, ExportJobComInvoiceLineValidation.InvalidBeginningForGoodDescription);
					}
					else
					{
						AssertNoWarningContaining(string.Format("LicenseType: {0}, ECCN: {1}", invoiceLine.US_LicenseType, invoiceLine.US_ECCN), invoiceLine.JI_DescriptionInfo, ExportJobComInvoiceLineValidation.InvalidBeginningForGoodDescription);
					}
				}
			}
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}
	}
}
