using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ValueQuantityBoundValidatorTest : TestCaseWithFactory
	{
		public void TestConsiderCustomsValueIncludingSecondaryLines()
		{
			invoiceLine.US_SupTariff = "99119651";
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.US_SupQty1 = 15000m;
			invoiceLine.JI_Tariff = "2003900010";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 15000m;

			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			IDutyData cusEntryLine = invoiceLine.CusEntryLine;
			AssertHasMessageError(invoiceLine.JI_LinePriceInfo, string.Format(ValueQuantityBoundValidator.UnitPriceOutOfBound, ValueQuantityBoundValidator.FirstQuantity, "supplementary tariff", " < 0.3475", ((ZDecimal)(invoiceLine.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity1)).Round(4)));

			invoiceLine.US_SupQty1 = 30000m;
			invoiceLine.JI_LinePrice = 10000m;
			cusEntryLine = invoiceLine.CusEntryLine;
			AssertNoMessageError(invoiceLine.JI_LinePriceInfo, string.Format(ValueQuantityBoundValidator.UnitPriceOutOfBound, ValueQuantityBoundValidator.FirstQuantity, "supplementary tariff", " < 0.3475", ((ZDecimal)(invoiceLine.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity1)).Round(4)));
		}

		public void TestValidateWithThirdPosition1()
		{
			var tariffValue = tariff.GetOrCreateNewTariffValue();
			tariffValue.UA_ValueEditCode = "111";//first quantity
			tariffValue.UA_ValueLowBounds = 10m;
			tariffValue.UA_ValueHighBounds = 20m;

			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_LinePrice = 2100m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			IDutyData cusEntryLine = invoiceLine.CusEntryLine;
			var unitPrice = ((ZDecimal)(invoiceLine.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity1)).Round(4);
			AssertHasMessageError(invoiceLine.JI_LinePriceInfo, string.Format(ValueQuantityBoundValidator.UnitPriceOutOfBound, ValueQuantityBoundValidator.FirstQuantity, "tariff", " > 10.0000 and <= 20.0000", unitPrice));

			invoiceLine.JI_LinePrice = 2000m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			cusEntryLine = invoiceLine.CusEntryLine;
			unitPrice = ((ZDecimal)(invoiceLine.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity1)).Round(4);
			AssertNoMessageErrorContaining(invoiceLine.JI_LinePriceInfo, string.Format(tariffMessageError, ValueQuantityBoundValidator.FirstQuantity, "tariff", " > 10.0000 and <= 20.0000"));

			invoiceLine.JI_CustomsQuantity = 200m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			cusEntryLine = invoiceLine.CusEntryLine;
			unitPrice = ((ZDecimal)(invoiceLine.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity1)).Round(4);
			AssertHasMessageError(invoiceLine.JI_LinePriceInfo, string.Format(ValueQuantityBoundValidator.UnitPriceOutOfBound, ValueQuantityBoundValidator.FirstQuantity, "tariff", " > 10.0000 and <= 20.0000", unitPrice));

			invoiceLine.JI_CustomsQuantity = 150m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			cusEntryLine = invoiceLine.CusEntryLine;
			unitPrice = ((ZDecimal)(invoiceLine.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity1)).Round(4);
			AssertNoMessageErrorContaining(invoiceLine.JI_LinePriceInfo, string.Format(tariffMessageError, ValueQuantityBoundValidator.FirstQuantity, "tariff", " > 10.0000 and <= 20.0000", unitPrice));

			invoiceLine.JI_CustomsQuantity = 200m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, string.Format(tariffMessageError, ValueQuantityBoundValidator.FirstQuantity, "tariff", " > 10.0000 and <= 20.0000"));

			//quantity validation tests
			var tariffQuantity = tariff.GetOrCreateNewTariffQuantity();
			tariffQuantity.UQ_QuantityEditCode = "121";//first quantity/second quantity
			tariffQuantity.UQ_LowerBound = 10m;
			tariffQuantity.UQ_UpperBound = 20m;

			invoiceLine.JI_CustomsQuantity = 2000m;
			invoiceLine.JI_CustomsSecondQuantity = 200m;
			invoiceLine.Validation.ValidateJI_CustomsQuantity();
			AssertHasMessageError(invoiceLine.JI_CustomsQuantityInfo, string.Format(ValueQuantityBoundValidator.QuantityRatioOutOfBound, ValueQuantityBoundValidator.FirstQuantity, ValueQuantityBoundValidator.SecondQuantity, " > 10.0000 and <= 20.0000"));

			invoiceLine.JI_CustomsSecondQuantity = 100m;
			invoiceLine.Validation.ValidateJI_CustomsQuantity();
			AssertNoMessageError(invoiceLine.JI_CustomsQuantityInfo, string.Format(ValueQuantityBoundValidator.QuantityRatioOutOfBound, ValueQuantityBoundValidator.FirstQuantity, ValueQuantityBoundValidator.SecondQuantity, " > 10.0000 and <= 20.0000"));

			invoiceLine.JI_CustomsSecondQuantity = 90m;
			invoiceLine.Validation.ValidateJI_CustomsQuantity();
			AssertHasMessageError(invoiceLine.JI_CustomsQuantityInfo, string.Format(ValueQuantityBoundValidator.QuantityRatioOutOfBound, ValueQuantityBoundValidator.FirstQuantity, ValueQuantityBoundValidator.SecondQuantity, " > 10.0000 and <= 20.0000"));

			invoiceLine.JI_CustomsSecondQuantity = 150m;
			invoiceLine.Validation.ValidateJI_CustomsQuantity();
			AssertNoMessageError(invoiceLine.JI_CustomsQuantityInfo, string.Format(ValueQuantityBoundValidator.QuantityRatioOutOfBound, ValueQuantityBoundValidator.FirstQuantity, ValueQuantityBoundValidator.SecondQuantity, " > 10.0000 and <= 20.0000"));
		}

		public void TestValidateWithThirdPosition2()
		{
			var tariffValue = tariff.GetOrCreateNewTariffValue();
			tariffValue.UA_ValueEditCode = "112";//first quantity
			tariffValue.UA_ValueLowBounds = 10m;
			tariffValue.UA_ValueHighBounds = 20m;

			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_LinePrice = 2000m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			IDutyData cusEntryLine = invoiceLine.CusEntryLine;
			var unitPrice = ((ZDecimal)(invoiceLine.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity1)).Round(4);
			AssertHasMessageError(invoiceLine.JI_LinePriceInfo, string.Format(ValueQuantityBoundValidator.UnitPriceOutOfBound, ValueQuantityBoundValidator.FirstQuantity, "tariff", " >= 10.0000 and < 20.0000", unitPrice));

			invoiceLine.JI_LinePrice = 1900m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNoMessageErrorContaining(invoiceLine.JI_LinePriceInfo, string.Format(tariffMessageError, ValueQuantityBoundValidator.FirstQuantity, "tariff", " >= 10.0000 and < 20.0000"));

			invoiceLine.JI_CustomsQuantity = 210m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			cusEntryLine = invoiceLine.CusEntryLine;
			unitPrice = ((ZDecimal)(invoiceLine.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity1)).Round(4);
			AssertHasMessageError(invoiceLine.JI_LinePriceInfo, string.Format(ValueQuantityBoundValidator.UnitPriceOutOfBound, ValueQuantityBoundValidator.FirstQuantity, "tariff", " >= 10.0000 and < 20.0000", unitPrice));

			invoiceLine.JI_CustomsQuantity = 190m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNoMessageError(invoiceLine.JI_LinePriceInfo, string.Format(tariffMessageError, ValueQuantityBoundValidator.FirstQuantity, "tariff", " >= 10.0000 and < 20.0000"));

			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_LinePrice = 2000m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, string.Format(tariffMessageError, ValueQuantityBoundValidator.FirstQuantity, "tariff", " >= 10.0000 and < 20.0000"));

			//quantity validation tests
			var tariffQuantity = tariff.GetOrCreateNewTariffQuantity();
			tariffQuantity.UQ_QuantityEditCode = "212";//second quantity/first quantity
			tariffQuantity.UQ_LowerBound = 10m;
			tariffQuantity.UQ_UpperBound = 20m;

			invoiceLine.JI_CustomsSecondQuantity = 2000m;
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.Validation.ValidateJI_CustomsSecondQuantity();
			AssertHasMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, string.Format(ValueQuantityBoundValidator.QuantityRatioOutOfBound, ValueQuantityBoundValidator.SecondQuantity, ValueQuantityBoundValidator.FirstQuantity, " >= 10.0000 and < 20.0000"));

			invoiceLine.JI_CustomsQuantity = 150m;
			invoiceLine.Validation.ValidateJI_CustomsSecondQuantity();
			AssertNoMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, string.Format(ValueQuantityBoundValidator.QuantityRatioOutOfBound, ValueQuantityBoundValidator.SecondQuantity, ValueQuantityBoundValidator.FirstQuantity, " >= 10.0000 and < 20.0000"));

			invoiceLine.JI_CustomsQuantity = 210m;
			invoiceLine.Validation.ValidateJI_CustomsSecondQuantity();
			AssertHasMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, string.Format(ValueQuantityBoundValidator.QuantityRatioOutOfBound, ValueQuantityBoundValidator.SecondQuantity, ValueQuantityBoundValidator.FirstQuantity, " >= 10.0000 and < 20.0000"));

			invoiceLine.JI_CustomsQuantity = 200m;
			invoiceLine.Validation.ValidateJI_CustomsSecondQuantity();
			AssertNoMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, string.Format(ValueQuantityBoundValidator.QuantityRatioOutOfBound, ValueQuantityBoundValidator.SecondQuantity, ValueQuantityBoundValidator.FirstQuantity, " >= 10.0000 and < 20.0000"));
		}

		public void TestValidateWithThirdPosition3()
		{
			var tariffValue = tariff.GetOrCreateNewTariffValue();
			tariffValue.UA_ValueEditCode = "123";//second quantity
			tariffValue.UA_ValueLowBounds = 10m;

			invoiceLine.JI_CustomsSecondQuantity = 100m;
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			IDutyData cusEntryLine = invoiceLine.CusEntryLine;
			var unitPrice = ((ZDecimal)(invoiceLine.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity2)).Round(4);
			AssertHasMessageError(invoiceLine.JI_LinePriceInfo, string.Format(ValueQuantityBoundValidator.UnitPriceOutOfBound, ValueQuantityBoundValidator.SecondQuantity, "tariff", " > 10.0000", unitPrice));
			AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, string.Format(tariffMessageError, ValueQuantityBoundValidator.SecondQuantity, "tariff", " > 10.0000"));

			invoiceLine.JI_LinePrice = 1100m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNoMessageError(invoiceLine.JI_LinePriceInfo, string.Format(tariffMessageError, ValueQuantityBoundValidator.SecondQuantity, "tariff", " > 10.0000"));

			invoiceLine.JI_CustomsSecondQuantity = 110m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			cusEntryLine = invoiceLine.CusEntryLine;
			unitPrice = ((ZDecimal)(invoiceLine.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity2)).Round(4);
			AssertHasMessageError(invoiceLine.JI_LinePriceInfo, string.Format(ValueQuantityBoundValidator.UnitPriceOutOfBound, ValueQuantityBoundValidator.SecondQuantity, "tariff", " > 10.0000", unitPrice));

			invoiceLine.JI_CustomsSecondQuantity = 90m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNoMessageError(invoiceLine.JI_LinePriceInfo, string.Format(tariffMessageError, ValueQuantityBoundValidator.SecondQuantity, "tariff", " > 10.0000"));

			//quantity validation tests
			var tariffQuantity = tariff.GetOrCreateNewTariffQuantity();
			tariffQuantity.UQ_QuantityEditCode = "133";//First quantity/Third quantity
			tariffQuantity.UQ_LowerBound = 10m;

			invoiceLine.JI_CustomsThirdQuantity = 200m;
			invoiceLine.JI_CustomsQuantity = 2000m;
			AssertHasMessageError(invoiceLine.JI_CustomsQuantityInfo, string.Format(ValueQuantityBoundValidator.QuantityRatioOutOfBound, ValueQuantityBoundValidator.FirstQuantity, ValueQuantityBoundValidator.ThirdQuantity, " > 10.0000"));

			invoiceLine.JI_CustomsQuantity = 2100m;
			AssertNoMessageError(invoiceLine.JI_CustomsQuantityInfo, string.Format(ValueQuantityBoundValidator.QuantityRatioOutOfBound, ValueQuantityBoundValidator.FirstQuantity, ValueQuantityBoundValidator.ThirdQuantity, " > 10.0000"));
		}

		public void TestValidateWithThirdPosition4()
		{
			var tariffValue = tariff.GetOrCreateNewTariffValue();
			tariffValue.UA_ValueEditCode = "124";//second quantity
			tariffValue.UA_ValueLowBounds = 10m;

			invoiceLine.JI_CustomsSecondQuantity = 100m;
			invoiceLine.JI_LinePrice = 900m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			IDutyData cusEntryLine = invoiceLine.CusEntryLine;
			var unitPrice = ((ZDecimal)(invoiceLine.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity2)).Round(4);
			AssertHasMessageError(invoiceLine.JI_LinePriceInfo, string.Format(ValueQuantityBoundValidator.UnitPriceOutOfBound, ValueQuantityBoundValidator.SecondQuantity, "tariff", " >= 10.0000", unitPrice));
			AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, string.Format(tariffMessageError, ValueQuantityBoundValidator.SecondQuantity, "tariff", " >= 10.0000"));

			invoiceLine.JI_LinePrice = 1200m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNoMessageError(invoiceLine.JI_LinePriceInfo, string.Format(tariffMessageError, ValueQuantityBoundValidator.SecondQuantity, "tariff", " >= 10.0000"));

			invoiceLine.JI_CustomsSecondQuantity = 130m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			cusEntryLine = invoiceLine.CusEntryLine;
			unitPrice = ((ZDecimal)(invoiceLine.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity2)).Round(4);
			AssertHasMessageError(invoiceLine.JI_LinePriceInfo, string.Format(ValueQuantityBoundValidator.UnitPriceOutOfBound, ValueQuantityBoundValidator.SecondQuantity, "tariff", " >= 10.0000", unitPrice));

			invoiceLine.JI_CustomsSecondQuantity = 120m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNoMessageError(invoiceLine.JI_LinePriceInfo, string.Format(tariffMessageError, ValueQuantityBoundValidator.SecondQuantity, "tariff", " >= 10.0000"));

			//quantity validation tests
			var tariffQuantity = tariff.GetOrCreateNewTariffQuantity();
			tariffQuantity.UQ_QuantityEditCode = "314";//Third quantity/First quantity
			tariffQuantity.UQ_LowerBound = 10m;

			invoiceLine.JI_CustomsThirdQuantity = 2000m;
			invoiceLine.JI_CustomsQuantity = 210m;
			invoiceLine.Validation.ValidateJI_CustomsThirdQuantity();
			AssertHasMessageError(invoiceLine.JI_CustomsThirdQuantityInfo, string.Format(ValueQuantityBoundValidator.QuantityRatioOutOfBound, ValueQuantityBoundValidator.ThirdQuantity, ValueQuantityBoundValidator.FirstQuantity, " >= 10.0000"));

			invoiceLine.JI_CustomsQuantity = 200m;
			invoiceLine.Validation.ValidateJI_CustomsThirdQuantity();
			AssertNoMessageError(invoiceLine.JI_CustomsThirdQuantityInfo, string.Format(ValueQuantityBoundValidator.QuantityRatioOutOfBound, ValueQuantityBoundValidator.ThirdQuantity, ValueQuantityBoundValidator.FirstQuantity, " >= 10.0000"));
		}

		public void TestValidateWithThirdPosition5()
		{
			var tariffValue = tariff.GetOrCreateNewTariffValue();
			tariffValue.UA_ValueEditCode = "135";//third quantity
			tariffValue.UA_ValueHighBounds = 20m;

			invoiceLine.JI_CustomsThirdQuantity = 100m;
			invoiceLine.JI_LinePrice = 2100m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			IDutyData cusEntryLine = invoiceLine.CusEntryLine;
			var unitPrice = ((ZDecimal)(invoiceLine.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity3)).Round(4);
			invoiceLine.Validation.ValidateJI_LinePrice();
			AssertHasMessageError(invoiceLine.JI_LinePriceInfo, string.Format(ValueQuantityBoundValidator.UnitPriceOutOfBound, ValueQuantityBoundValidator.ThirdQuantity, "tariff", " <= 20.0000", unitPrice));
			AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, string.Format(tariffMessageError, ValueQuantityBoundValidator.ThirdQuantity, "tariff", " <= 20.0000"));

			invoiceLine.JI_LinePrice = 2000m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			invoiceLine.Validation.ValidateJI_LinePrice();
			AssertNoMessageError(invoiceLine.JI_LinePriceInfo, string.Format(tariffMessageError, ValueQuantityBoundValidator.ThirdQuantity, "tariff", " <= 20.0000"));

			invoiceLine.JI_CustomsThirdQuantity = 90m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			cusEntryLine = invoiceLine.CusEntryLine;
			unitPrice = ((ZDecimal)(invoiceLine.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity3)).Round(4);
			invoiceLine.Validation.ValidateJI_LinePrice();
			AssertHasMessageError(invoiceLine.JI_LinePriceInfo, string.Format(ValueQuantityBoundValidator.UnitPriceOutOfBound, ValueQuantityBoundValidator.ThirdQuantity, "tariff", " <= 20.0000", unitPrice));

			invoiceLine.JI_CustomsThirdQuantity = 120m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			invoiceLine.Validation.ValidateJI_LinePrice();
			AssertNoMessageError(invoiceLine.JI_LinePriceInfo, string.Format(tariffMessageError, ValueQuantityBoundValidator.ThirdQuantity, "tariff", " <= 20.0000"));

			//quantity validation tests
			var tariffQuantity = tariff.GetOrCreateNewTariffQuantity();
			tariffQuantity.UQ_QuantityEditCode = "235";//Second quantity/Third quantity
			tariffQuantity.UQ_UpperBound = 20m;

			invoiceLine.JI_CustomsThirdQuantity = 90m;
			invoiceLine.JI_CustomsSecondQuantity = 2000m;
			invoiceLine.Validation.ValidateJI_CustomsSecondQuantity();
			AssertHasMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, string.Format(ValueQuantityBoundValidator.QuantityRatioOutOfBound, ValueQuantityBoundValidator.SecondQuantity, ValueQuantityBoundValidator.ThirdQuantity, " <= 20.0000"));

			invoiceLine.JI_CustomsThirdQuantity = 100m;
			invoiceLine.Validation.ValidateJI_CustomsSecondQuantity();
			AssertNoMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, string.Format(ValueQuantityBoundValidator.QuantityRatioOutOfBound, ValueQuantityBoundValidator.SecondQuantity, ValueQuantityBoundValidator.ThirdQuantity, " <= 20.0000"));
		}

		public void TestValidateWithThirdPosition6()
		{
			var tariffValue = tariff.GetOrCreateNewTariffValue();
			tariffValue.UA_ValueEditCode = "136";//third quantity
			tariffValue.UA_ValueHighBounds = 20m;

			invoiceLine.JI_CustomsThirdQuantity = 100m;
			invoiceLine.JI_LinePrice = 2000m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			IDutyData cusEntryLine = invoiceLine.CusEntryLine;
			var unitPrice = ((ZDecimal)(invoiceLine.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity3)).Round(4);
			AssertHasMessageError(invoiceLine.JI_LinePriceInfo, string.Format(ValueQuantityBoundValidator.UnitPriceOutOfBound, ValueQuantityBoundValidator.ThirdQuantity, "tariff", " < 20.0000", unitPrice));
			AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, string.Format(tariffMessageError, ValueQuantityBoundValidator.ThirdQuantity, "tariff", " < 20.0000", unitPrice));

			invoiceLine.JI_LinePrice = 1900m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNoMessageError(invoiceLine.JI_LinePriceInfo, string.Format(tariffMessageError, ValueQuantityBoundValidator.ThirdQuantity, "tariff", " < 20.0000"));

			invoiceLine.JI_CustomsThirdQuantity = 90m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			cusEntryLine = invoiceLine.CusEntryLine;
			unitPrice = ((ZDecimal)(invoiceLine.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity3)).Round(4);
			AssertHasMessageError(invoiceLine.JI_LinePriceInfo, string.Format(ValueQuantityBoundValidator.UnitPriceOutOfBound, ValueQuantityBoundValidator.ThirdQuantity, "tariff", " < 20.0000", unitPrice));

			invoiceLine.JI_CustomsThirdQuantity = 120m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNoMessageError(invoiceLine.JI_LinePriceInfo, string.Format(tariffMessageError, ValueQuantityBoundValidator.ThirdQuantity, "tariff", " < 20.0000"));

			//quantity validation tests
			var tariffQuantity = tariff.GetOrCreateNewTariffQuantity();
			tariffQuantity.UQ_QuantityEditCode = "326";//Third quantity/Second quantity
			tariffQuantity.UQ_UpperBound = 20m;

			invoiceLine.JI_CustomsThirdQuantity = 2000m;
			invoiceLine.JI_CustomsSecondQuantity = 100m;
			invoiceLine.Validation.ValidateJI_CustomsThirdQuantity();
			AssertHasMessageError(invoiceLine.JI_CustomsThirdQuantityInfo, string.Format(ValueQuantityBoundValidator.QuantityRatioOutOfBound, ValueQuantityBoundValidator.ThirdQuantity, ValueQuantityBoundValidator.SecondQuantity, " < 20.0000"));

			invoiceLine.JI_CustomsThirdQuantity = 110m;
			invoiceLine.Validation.ValidateJI_CustomsThirdQuantity();
			AssertNoMessageError(invoiceLine.JI_CustomsThirdQuantityInfo, string.Format(ValueQuantityBoundValidator.QuantityRatioOutOfBound, ValueQuantityBoundValidator.ThirdQuantity, ValueQuantityBoundValidator.SecondQuantity, " < 20.0000"));
		}

		public void TestWhenRequiredQuantityNotEntered()
		{
			var tariffValue = tariff.GetOrCreateNewTariffValue();
			tariffValue.UA_ValueEditCode = "135";//third quantity
			tariffValue.UA_ValueHighBounds = 20m;

			invoiceLine.JI_CustomsThirdQuantity = 0m;
			AssertNoExceptionThrown(() => invoiceLine.JI_LinePrice = 2000m);

			var tariffQuantity = tariff.GetOrCreateNewTariffQuantity();
			tariffQuantity.UQ_QuantityEditCode = "323";//Third quantity/Second quantity
			tariffQuantity.UQ_UpperBound = 20m;
			AssertNoExceptionThrown(() => invoiceLine.JI_CustomsSecondQuantity = 2000m);
		}

		public void TestValidateWithXAndVWith9802()
		{
			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "9603302000";
			tariff.UE_Unit1 = "NO";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-1);
			tariff.UE_DateTo = ZDateTime.Today;
			var tariffValue = tariff.GetOrCreateNewTariffValue();
			tariffValue.UA_ValueEditCode = "115";
			tariffValue.UA_ValueHighBounds = 0.05000m;

			invoiceLine.JI_Tariff = "6912.00.4400";
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			var vLine1 = invoice.JobComInvoiceLines.AddNew();
			vLine1.JI_Tariff = "6912.00.4400";
			vLine1.JI_LinePrice = 8870.04m;
			vLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			var vLine2 = invoice.JobComInvoiceLines.AddNew();
			vLine2.JI_Tariff = "9603302000";
			vLine2.JI_LinePrice = 275.04m;
			vLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			vLine2.JI_CustomsQuantity = 13752m;
			vLine2.US_SupTariff = "9802005030";
			vLine2.US_98GoodsValue = 10000m;

			var vLine3 = invoice.JobComInvoiceLines.AddNew();
			vLine3.JI_Tariff = "3213100000";
			vLine3.JI_LinePrice = 825.2m;
			vLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			vLine2.Validation.ValidateJI_LinePrice();
			AssertHasMessageErrorContaining(vLine2.JI_LinePriceInfo, "calculated with the customs value divided by the first quantity of entry lines is outside of the range the selected tariff allows,");

			vLine2.US_SupTariff = "";
			vLine2.US_98GoodsValue = 0m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			vLine2.Validation.ValidateJI_LinePrice();
			AssertNoMessageErrorContaining(vLine2.JI_LinePriceInfo, "calculated with the customs value divided by the first quantity of entry lines is outside of the range the selected tariff allows,");
		}

		USCTariff tariff;
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		const string tariffMessageError = @"the customs value divided by the {0} of entry lines is outside of the range the selected {1} allows, {2}. Please review all the invoice lines merged to this entry line. If you have modified line prices or quantities of any invoice line, please save or click Brokerage > Merge to refresh this validation.";

		protected override void SetUp()
		{
			base.SetUp();
			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000111122";
			tariff.UE_Unit1 = "KG";
			tariff.UE_Unit2 = "NO";
			tariff.UE_Unit3 = "PCK";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000111122";
		}
	}
}
