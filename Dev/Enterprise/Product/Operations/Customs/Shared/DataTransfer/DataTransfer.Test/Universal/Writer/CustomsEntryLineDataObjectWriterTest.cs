using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectWriterTest
	{
		public void TestCustomsEntryLineMappings()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeaderMock = CreateCusEntryHeaderMock(declaration);
			var entryHeader = entryHeaderMock.Object;
			var entryLine = SetupCusEntryLine(entryHeader.MergedLines.AddNew());

			var writer = new CustomsEntryLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, entryLine)), CurrentCompanyHelper);
			var entryLineData = writer.GetDataObject(entryLine);
			AssertContents(entryLineData);
		}

		public void TestCustomsEntryLineConfirmedFees()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeaderMock = CreateCusEntryHeaderMock(declaration);
			var entryHeader = entryHeaderMock.Object;
			var entryLine = SetupCusEntryLineOnly(entryHeader.MergedLines.AddNew(), "1010101010", 340.23m, 3, "HELLO WORLD", 391.53m, "KG", 72.23m, "ACT");
			SetupCusEntryLineFee(entryLine.Fees.AddNew(), 100m, Core.Constants.USCustoms.FeeCodes.Duty);
			SetupCusEntryLineFee(entryLine.ConfirmedFees.AddNew());
			SetupCusEntryLineFee2(entryLine.ConfirmedFees.AddNew());

			var writer = new CustomsEntryLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, entryLine)), CurrentCompanyHelper);
			var entryLineData = writer.GetDataObject(entryLine);
			AssertContents(entryLineData);
		}

		CusEntryLine SetupCusEntryLineOnly(CusEntryLine entryLine, ZString tariff, ZDecimal customsValue, ZShort lineNumber, ZString description, ZDecimal flatAmount, ZString flatAmountUQ, ZDecimal dutyPercent, ZString postedStatus)
		{
			entryLine.CL_AdValoremTariff = tariff;
			entryLine.CL_CustomsValue = customsValue;
			entryLine.CL_LineNumber = lineNumber;
			entryLine.CL_Description = description;
			entryLine.CL_FlatAmount = flatAmount;
			entryLine.CL_FlatAmountUQ = flatAmountUQ;
			entryLine.CL_DutyPercent = dutyPercent;
			entryLine.CL_CustomsPostedStatus = postedStatus;
			entryLine.CL_AddInfo = AddInfoCollectionCreatorTest.AddInfoStringForTesting;
			return entryLine;
		}

		CusEntryLine SetupCusEntryLine(CusEntryLine entryLine, ZString tariff, ZDecimal customsValue, ZShort lineNumber, ZString description, ZDecimal flatAmount, ZString flatAmountUQ, ZDecimal dutyPercent, CodeDescriptionPair postedStatus)
		{
			entryLine = SetupCusEntryLineOnly(entryLine, tariff, customsValue, lineNumber, description, flatAmount, flatAmountUQ, dutyPercent, postedStatus.Code ?? ZString.Empty);
			SetupCusEntryLineFee(entryLine.Fees.AddNew());
			SetupCusEntryLineFee2(entryLine.Fees.AddNew());
			return entryLine;
		}

		CusEntryLine SetupCusEntryLine(CusEntryLine entryLine)
		{
			return SetupCusEntryLine(entryLine, "1010101010", 340.23m, 3, "HELLO WORLD", 391.53m, "KG", 72.23m, new CodeDescriptionPair { Code = "ACT", Description = "ACTIVE" });
		}

		void SetupCusEntryLineFee(CusEntryLineFee entryLineFee, ZDecimal chargeAmount, ZString chargeType)
		{
			entryLineFee.CF_ChargeAmount = chargeAmount;
			entryLineFee.CF_ChargeType = chargeType;
			entryLineFee.CF_BaseValue = 135.62m;
			entryLineFee.CF_Rate = 0.68m;
			entryLineFee.CF_RateOverrideReasonCode = "OTH";
			entryLineFee.CF_MethodOfPayment = "PPD";
			entryLineFee.CF_MethodOfCalculation = "SUMFUN";
			entryLineFee.CF_Source = "CW1";
		}

		void SetupCusEntryLineFee(CusEntryLineFee entryLineFee)
		{
			SetupCusEntryLineFee(entryLineFee, 102.23m, Core.Constants.USCustoms.FeeCodes.Duty);
		}

		void SetupCusEntryLineFee2(CusEntryLineFee entryLineFee)
		{
			SetupCusEntryLineFee(entryLineFee, 689.35m, Core.Constants.USCustoms.FeeCodes.Mango);
		}

		void AssertContentsWithLookingAtChildren(UniversalCustoms.EntryLine entryLineData, ZString tariff, ZDecimal customsValue, ZShort lineNumber, ZString description, ZDecimal dutyRateFlatAmount, ICodeDescription dutyRateFlatAmountUnit, ZDecimal dutyRatePercent, ICodeDescription postedStatus)
		{
			AssertNotNull("Precondition: entryLineData", entryLineData);

			CombineAssertions(delegate
			{
				AssertEquals("entryLineData.DutyRateFlatAmountUnit.HarmonisedCode", tariff, entryLineData.HarmonisedCode);
				AssertEquals("entryLineData.DutyRateFlatAmountUnit.CustomsValue", customsValue, entryLineData.CustomsValue);
				AssertEquals("entryLineData.DutyRateFlatAmountUnit.LineNumber", lineNumber, entryLineData.LineNumber);
				AssertEquals("entryLineData.DutyRateFlatAmountUnit.Description", description, entryLineData.Description);
				AssertEquals("entryLineData.DutyRateFlatAmountUnit.DutyRateFlatAmount", dutyRateFlatAmount, entryLineData.DutyRateFlatAmount);
				AssertNotNull("entryLineData.DutyRateFlatAmountUnit", entryLineData.DutyRateFlatAmountUnit);
				AssertEquals("entryLineData.DutyRateFlatAmountUnit.Code", dutyRateFlatAmountUnit.Code, entryLineData.DutyRateFlatAmountUnit.Code);
				AssertEquals("entryLineData.DutyRateFlatAmountUnit.Description", dutyRateFlatAmountUnit.Description, entryLineData.DutyRateFlatAmountUnit.Description);
				AssertEquals("entryLineData.DutyRateFlatAmountUnit.DutyRatePercent", dutyRatePercent, entryLineData.DutyRatePercent);
				AssertEquals("entryLineData.CustomsPostedStatus.Code", postedStatus.Code, entryLineData.CustomsStatus.Code);
				AssertEquals("entryLineData.CustomsPostedStatus.Description", postedStatus.Description, entryLineData.CustomsStatus.Description);

				AssertNotNull("Precondition: entryLineData.AddInfoCollection", entryLineData.AddInfoCollection);
				AddInfoCollectionCreatorTest.AssertContents(entryLineData.AddInfoCollection);
			});
		}

		void AssertContents(UniversalCustoms.EntryLine entryLineData, ZString tariff, ZDecimal customsValue, ZShort lineNumber, ZString description, ZDecimal dutyRateFlatAmount, ICodeDescription dutyRateFlatAmountUnit, ZDecimal dutyRatePercent, ICodeDescription postedStatus)
		{
			AssertContentsWithLookingAtChildren(entryLineData, tariff, customsValue, lineNumber, description, dutyRateFlatAmount, dutyRateFlatAmountUnit, dutyRatePercent, postedStatus);
			AssertNotNull("Precondition: entryLineData.EntryLineChargeCollection", entryLineData.EntryLineChargeCollection);
			AssertEquals("entryLineData.EntryLineChargeCollection.Count", 2, entryLineData.EntryLineChargeCollection.Count);
			AssertContents(entryLineData.EntryLineChargeCollection[0]);
			AssertContents2(entryLineData.EntryLineChargeCollection[1]);
		}

		void AssertContents(UniversalCustoms.EntryLine entryLineData)
		{
			AssertContents(entryLineData, "1010101010", 340.23m, 3, "HELLO WORLD", 391.53m, GetCodeDescriptionPair(Core.Constants.Weight.Kilograms, "Kilograms"), 72.23m, GetCodeDescriptionPair(Enterprise.Customs.Business.EntryLineStatusList.Codes.Active, Enterprise.Customs.Business.EntryLineStatusList.Descriptions.Active));
		}

		void AssertContents(UniversalCustoms.EntryLineCharge chargeData, ZDecimal amount, ICodeDescription type)
		{
			AssertNotNull("Precondition: chargeData", chargeData);
			CombineAssertions(delegate
			{
				AssertEquals("chargeData.Amount", amount, chargeData.Amount);
				AssertNotNull("chargeData.Type", chargeData.Type);
				AssertEquals("chargeData.Type.Code", type.Code, chargeData.Type.Code);
				AssertEquals("chargeData.Type.Description", type.Description, chargeData.Type.Description);
				AssertEquals("chargeData.Source", "CW1", chargeData.Source);

				AssertEquals("chargeData.BaseValue", 135.62m, chargeData.BaseValue);
				AssertEquals("chargeData.Rate", 0.68m, chargeData.Rate);
				AssertNotNull("chargeData.RateOverrideReason", chargeData.RateOverrideReason);
				AssertEquals("chargeData.RateOverrideReason.Code", "OTH", chargeData.RateOverrideReason.Code);
				AssertNull("chargeData.RateOverrideReason.Description", chargeData.RateOverrideReason.Description);

				AssertNotNull("chargeData.MethodOfPayment", chargeData.MethodOfPayment);
				AssertEquals("chargeData.MethodOfPayment.Code", "PPD", chargeData.MethodOfPayment.Code);
				AssertNull("chargeData.MethodOfPayment.Description", chargeData.MethodOfPayment.Description);

				AssertNotNull("chargeData.MethodOfCalculation", chargeData.MethodOfCalculation);
				AssertEquals("chargeData.MethodOfCalculation.Code", "SUMFUN", chargeData.MethodOfCalculation.Code);
				AssertNull("chargeData.MethodOfCalculation.Description", chargeData.MethodOfCalculation.Description);
			});
		}

		void AssertContents(UniversalCustoms.EntryLineCharge chargeData)
		{
			AssertContents(chargeData, 102.23m, GetCodeDescriptionPair(Core.Constants.USCustoms.FeeCodes.Duty, "Duty"));
		}

		void AssertContents2(UniversalCustoms.EntryLineCharge chargeData)
		{
			AssertContents(chargeData, 689.35m, GetCodeDescriptionPair(Core.Constants.USCustoms.FeeCodes.Mango, "108 Desc from DB"));
		}
	}
}
