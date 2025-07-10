using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	abstract class RebateCalculatorAbstractTest<T> : TestCaseWithFactory where T : CertificateCusCodeData
	{
		public void TestCalculateRebatedValueForDutyUsingAllFirstPermitAndSomeOfSecondWithAdjustmentFactor()
		{
			SetupRefData();
			var orgHeader = Factory.New<OrgHeader>();
			var entryInstruction = CreateEntryInstruction(orgHeader);
			SetupPermit(orgHeader, 100, "12345", PermitSubTypeList.Codes.MHV);
			SetupPermit(orgHeader, 200, "67890", PermitSubTypeList.Codes.LVE);
			CreateCertificate(entryInstruction, "12345", 1);
			CreateCertificate(entryInstruction, "67890", 2);
			var onePoneValue = 180m;
			var mockCusEntryLine = Factory.New<DummyCusEntryLine_RebateCalculatorAbstractTest>();
			mockCusEntryLine.EntryInstructionCoreReturn = entryInstruction;
			mockCusEntryLine.IsSpecifiedMotorVehicleCoreReturn = false;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MergedLines.Add(mockCusEntryLine.Object);
			var rebatedCalculator = GetRebateCalculator(declaration);
			ZShort index = 1;
			rebatedCalculator.CalculateRebatedValueForDuty(onePoneValue, mockCusEntryLine.Object).ForEach(x => x.Invoke(index++));
			AssertEquals("Rebated value should be 180", 180m, rebatedCalculator.AmountToRebate);
			AssertEquals("Entry line should have 4 additional information documents (2 pairs)", 4, mockCusEntryLine.Object.AdditionalInformationCodes.Count);
			AssertEquals("PRV", mockCusEntryLine.Object.AdditionalInformationCodes[0].CY_Code);
			AssertEquals(GetExpectedPRVValue("100"), mockCusEntryLine.Object.AdditionalInformationCodes[0].CY_Data);
			AssertEquals("1", mockCusEntryLine.Object.AdditionalInformationCodes[0].Grouping);
			AssertEquals("PRC", mockCusEntryLine.Object.AdditionalInformationCodes[1].CY_Code);
			AssertEquals("MHV12345", mockCusEntryLine.Object.AdditionalInformationCodes[1].CY_Data);
			AssertEquals("1", mockCusEntryLine.Object.AdditionalInformationCodes[1].Grouping);
			AssertEquals("PRV", mockCusEntryLine.Object.AdditionalInformationCodes[2].CY_Code);
			AssertEquals(GetExpectedPRVValue("80"), mockCusEntryLine.Object.AdditionalInformationCodes[2].CY_Data);
			AssertEquals("2", mockCusEntryLine.Object.AdditionalInformationCodes[2].Grouping);
			AssertEquals("PRC", mockCusEntryLine.Object.AdditionalInformationCodes[3].CY_Code);
			AssertEquals("LVE67890", mockCusEntryLine.Object.AdditionalInformationCodes[3].CY_Data);
			AssertEquals("2", mockCusEntryLine.Object.AdditionalInformationCodes[3].Grouping);
		}

		public void TestCalculateRebatedValueForDutyUsingAllFirstPermitAndSomeOfSecondWithoutAdjustment()
		{
			SetupRefData();
			var orgHeader = Factory.New<OrgHeader>();
			var entryInstruction = CreateEntryInstruction(orgHeader);
			SetupPermit(orgHeader, 100, "12345", PermitSubTypeList.Codes.MHV);
			SetupPermit(orgHeader, 200, "67890", PermitSubTypeList.Codes.LVE);
			CreateCertificate(entryInstruction, "12345", 1);
			CreateCertificate(entryInstruction, "67890", 2);
			var onePoneValue = 225m;
			var mockCusEntryLine = Factory.New<DummyCusEntryLine_RebateCalculatorAbstractTest>();
			mockCusEntryLine.EntryInstructionCoreReturn = entryInstruction;
			mockCusEntryLine.IsSpecifiedMotorVehicleCoreReturn = false;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MergedLines.Add(mockCusEntryLine.Object);
			var rebatedCalculator = GetRebateCalculator(declaration);
			ZShort index = 1;
			rebatedCalculator.CalculateRebatedValueForDuty(onePoneValue, mockCusEntryLine.Object).ForEach(x => x.Invoke(index++));
			AssertEquals("Rebated value should be 225", 225m, rebatedCalculator.AmountToRebate);
			AssertEquals("Entry line should have 4 additional information documents (2 pairs)", 4, mockCusEntryLine.Object.AdditionalInformationCodes.Count);
			AssertEquals("PRV", mockCusEntryLine.Object.AdditionalInformationCodes[0].CY_Code);
			AssertEquals(GetExpectedPRVValue("100"), mockCusEntryLine.Object.AdditionalInformationCodes[0].CY_Data);
			AssertEquals("1", mockCusEntryLine.Object.AdditionalInformationCodes[0].Grouping);
			AssertEquals("PRC", mockCusEntryLine.Object.AdditionalInformationCodes[1].CY_Code);
			AssertEquals("MHV12345", mockCusEntryLine.Object.AdditionalInformationCodes[1].CY_Data);
			AssertEquals("1", mockCusEntryLine.Object.AdditionalInformationCodes[1].Grouping);
			AssertEquals("PRV", mockCusEntryLine.Object.AdditionalInformationCodes[2].CY_Code);
			AssertEquals(GetExpectedPRVValue("125"), mockCusEntryLine.Object.AdditionalInformationCodes[2].CY_Data);
			AssertEquals("2", mockCusEntryLine.Object.AdditionalInformationCodes[2].Grouping);
			AssertEquals("PRC", mockCusEntryLine.Object.AdditionalInformationCodes[3].CY_Code);
			AssertEquals("LVE67890", mockCusEntryLine.Object.AdditionalInformationCodes[3].CY_Data);
			AssertEquals("2", mockCusEntryLine.Object.AdditionalInformationCodes[3].Grouping);
		}

		public void TestCalculateRebatedValueForDutyUsingJustFirstPermit()
		{
			SetupRefData();
			var orgHeader = Factory.New<OrgHeader>();
			var entryInstruction = CreateEntryInstruction(orgHeader);
			SetupPermit(orgHeader, 100, "12345", PermitSubTypeList.Codes.MHV);
			SetupPermit(orgHeader, 200, "67890", PermitSubTypeList.Codes.LVE);
			CreateCertificate(entryInstruction, "12345", 1);
			CreateCertificate(entryInstruction, "67890", 2);
			var onePoneValue = 80m;
			var mockCusEntryLine = Factory.New<DummyCusEntryLine_RebateCalculatorAbstractTest>();
			mockCusEntryLine.EntryInstructionCoreReturn = entryInstruction;
			mockCusEntryLine.IsSpecifiedMotorVehicleCoreReturn = false;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MergedLines.Add(mockCusEntryLine.Object);
			var rebatedCalculator = GetRebateCalculator(declaration);
			ZShort index = 1;
			rebatedCalculator.CalculateRebatedValueForDuty(onePoneValue, mockCusEntryLine.Object).ForEach(x => x.Invoke(index++));
			AssertEquals("Rebated value should be 80", 80m, rebatedCalculator.AmountToRebate);
			AssertEquals("Entry line should have 2 additional information documents (1 pair)", 2, mockCusEntryLine.Object.AdditionalInformationCodes.Count);
			AssertEquals("PRV", mockCusEntryLine.Object.AdditionalInformationCodes[0].CY_Code);
			AssertEquals(GetExpectedPRVValue("80"), mockCusEntryLine.Object.AdditionalInformationCodes[0].CY_Data);
			AssertEquals("", mockCusEntryLine.Object.AdditionalInformationCodes[0].Grouping);
			AssertEquals("PRC", mockCusEntryLine.Object.AdditionalInformationCodes[1].CY_Code);
			AssertEquals("MHV12345", mockCusEntryLine.Object.AdditionalInformationCodes[1].CY_Data);
			AssertEquals("", mockCusEntryLine.Object.AdditionalInformationCodes[1].Grouping);
		}

		public void TestCalculateRebatedValueForDutyUsingOnePermitTakesCurrentTransactionsIntoAccount()
		{
			SetupRefData();
			var orgHeader = Factory.New<OrgHeader>();
			var entryInstruction = CreateEntryInstruction(orgHeader);
			var permit1 = SetupPermit(orgHeader, 100, "12345", PermitSubTypeList.Codes.MHV);
			CreateCertificate(entryInstruction, "12345", 1);
			var onePoneValue = 80m;
			var mockCusEntryLine = Factory.New<DummyCusEntryLine_RebateCalculatorAbstractTest>();
			mockCusEntryLine.EntryInstructionCoreReturn = entryInstruction;
			mockCusEntryLine.IsSpecifiedMotorVehicleCoreReturn = false;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "Ref1";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.MergedLines.Add(mockCusEntryLine.Object);
			var rebatedCalculator = GetRebateCalculator(declaration);
			ZShort index = 1;
			rebatedCalculator.CalculateRebatedValueForDuty(onePoneValue, mockCusEntryLine.Object).ForEach(x => x.Invoke(index++));
			AssertEquals("Rebated value should be 80", 80m, rebatedCalculator.AmountToRebate);
			AssertEquals("Entry line should have 2 additional information documents (1 pair)", 2, mockCusEntryLine.Object.AdditionalInformationCodes.Count);
			AssertEquals("PRV", mockCusEntryLine.Object.AdditionalInformationCodes[0].CY_Code);
			AssertEquals(GetExpectedPRVValue("80"), mockCusEntryLine.Object.AdditionalInformationCodes[0].CY_Data);
			AssertEquals("", mockCusEntryLine.Object.AdditionalInformationCodes[0].Grouping);
			AssertEquals("PRC", mockCusEntryLine.Object.AdditionalInformationCodes[1].CY_Code);
			AssertEquals("MHV12345", mockCusEntryLine.Object.AdditionalInformationCodes[1].CY_Data);
			AssertEquals("", mockCusEntryLine.Object.AdditionalInformationCodes[1].Grouping);
			// Manually add transaction to simulate sending of a message
			permit1.AddTransaction("Ref1", "", "1", "", -80m, 0m, "CON");
			onePoneValue = 90m;
			index = 1;
			mockCusEntryLine.Object.AdditionalInformationCodes.RemoveAndDeleteAll();
			rebatedCalculator = GetRebateCalculator(declaration);
			rebatedCalculator.CalculateRebatedValueForDuty(onePoneValue, mockCusEntryLine.Object).ForEach(x => x.Invoke(index++));
			AssertEquals("Rebated value should be 90", 90m, rebatedCalculator.AmountToRebate);
			AssertEquals("Entry line should have 2 additional information documents (1 pair)", 2, mockCusEntryLine.Object.AdditionalInformationCodes.Count);
			AssertEquals("PRV", mockCusEntryLine.Object.AdditionalInformationCodes[0].CY_Code);
			AssertEquals(GetExpectedPRVValue("90"), mockCusEntryLine.Object.AdditionalInformationCodes[0].CY_Data);
			AssertEquals("", mockCusEntryLine.Object.AdditionalInformationCodes[0].Grouping);
			AssertEquals("PRC", mockCusEntryLine.Object.AdditionalInformationCodes[1].CY_Code);
			AssertEquals("MHV12345", mockCusEntryLine.Object.AdditionalInformationCodes[1].CY_Data);
			AssertEquals("", mockCusEntryLine.Object.AdditionalInformationCodes[1].Grouping);
		}

		public void TestCalculateRebatedValueForDutyUsingTwoEntryLinesUsingTheSamePermit()
		{
			SetupRefData();
			var orgHeader = Factory.New<OrgHeader>();
			var entryInstruction = CreateEntryInstruction(orgHeader);
			SetupPermit(orgHeader, 100, "12345", PermitSubTypeList.Codes.MHV);
			CreateCertificate(entryInstruction, "12345", 1);
			var onePoneValue = 80m;
			var mockCusEntryLine = Factory.New<DummyCusEntryLine_RebateCalculatorAbstractTest>();
			mockCusEntryLine.EntryInstructionCoreReturn = entryInstruction;
			mockCusEntryLine.IsSpecifiedMotorVehicleCoreReturn = false;
			var mockCusEntryLine2 = Factory.New<DummyCusEntryLine_RebateCalculatorAbstractTest>();
			mockCusEntryLine2.EntryInstructionCoreReturn = entryInstruction;
			mockCusEntryLine2.IsSpecifiedMotorVehicleCoreReturn = false;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MergedLines.Add(mockCusEntryLine.Object);
			entryHeader.MergedLines.Add(mockCusEntryLine2.Object);
			var rebatedCalculator = GetRebateCalculator(declaration);
			ZShort index = 1;
			rebatedCalculator.CalculateRebatedValueForDuty(onePoneValue, mockCusEntryLine.Object).ForEach(x => x.Invoke(index++));
			AssertEquals("Rebated value should be 80", 80m, rebatedCalculator.AmountToRebate);
			AssertEquals("Entry line should have 2 additional information documents (1 pair)", 2, mockCusEntryLine.Object.AdditionalInformationCodes.Count);
			AssertEquals("PRV", mockCusEntryLine.Object.AdditionalInformationCodes[0].CY_Code);
			AssertEquals(GetExpectedPRVValue("80"), mockCusEntryLine.Object.AdditionalInformationCodes[0].CY_Data);
			AssertEquals("", mockCusEntryLine.Object.AdditionalInformationCodes[0].Grouping);
			AssertEquals("PRC", mockCusEntryLine.Object.AdditionalInformationCodes[1].CY_Code);
			AssertEquals("MHV12345", mockCusEntryLine.Object.AdditionalInformationCodes[1].CY_Data);
			AssertEquals("", mockCusEntryLine.Object.AdditionalInformationCodes[1].Grouping);
			index = 1;
			rebatedCalculator.CalculateRebatedValueForDuty(onePoneValue, mockCusEntryLine2.Object).ForEach(x => x.Invoke(index++));
			AssertEquals("Rebated value should be 20", 20m, rebatedCalculator.AmountToRebate);
			AssertEquals("Entry line should have 2 additional information documents (1 pair)", 2, mockCusEntryLine2.Object.AdditionalInformationCodes.Count);
			AssertEquals("PRV", mockCusEntryLine2.Object.AdditionalInformationCodes[0].CY_Code);
			AssertEquals(GetExpectedPRVValue("20"), mockCusEntryLine2.Object.AdditionalInformationCodes[0].CY_Data);
			AssertEquals("", mockCusEntryLine2.Object.AdditionalInformationCodes[0].Grouping);
			AssertEquals("PRC", mockCusEntryLine2.Object.AdditionalInformationCodes[1].CY_Code);
			AssertEquals("MHV12345", mockCusEntryLine2.Object.AdditionalInformationCodes[1].CY_Data);
			AssertEquals("", mockCusEntryLine2.Object.AdditionalInformationCodes[1].Grouping);
		}

		protected CusEntryInstruction CreateEntryInstruction(OrgHeader orgHeader)
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			declaration.JE_OH_Importer = orgHeader.PK;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			return entryInstruction;
		}

		protected CusPermitHeader SetupPermit(OrgHeader orgHeader, ZDecimal value, ZString number, ZString subType)
		{
			var permit1 = Factory.New<CusPermitHeader>();
			permit1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			permit1.CPH_Type = PermitTypeCore;
			permit1.CPH_SubType = subType;
			permit1.CPH_Number = number;
			permit1.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.VAL;
			permit1.CPH_OH_PermitHolder = orgHeader.PK;
			permit1.CPH_StartDate = ZDate.Today.AddDays(-10);
			permit1.CPH_EndDate = ZDate.Today.AddDays(10);
			var tran1 = permit1.CusPermitLineTransactions.AddNew();
			tran1.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			tran1.CPL_TranValue = value;
			return permit1;
		}

		protected void SetupRefData()
		{
			var universalHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			universalHelper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.ProductionRebateCertificate);
			universalHelper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.ProductionRebateValue);
			universalHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "12", "00", "13A,13B,13C,13D,4", "", "IMP");
			universalHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "12", "", "", "", "IMP");
			var tariffType1P1 = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType4P1 = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "4P1");
			Factory.Save();
			var tariff = universalHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1010102030", ZDateTime.Today, ZDateTime.Today);
			universalHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.SpecifiedMotorVehicle, "true", tariff);
			var tariff4P1 = universalHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType4P1.PK, "4100101010", ZDateTime.Today, ZDateTime.Today);
			universalHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.PRCC, "true", tariff4P1);
		}

		ZString GetExpectedPRVValue(ZString value)
		{
			var result = value;
			if (PRVValueUnit == PRVValueUnits.CENTS)
			{
				result = (ZInt.ParseSafe(value, 0) * 100).ToString();
			}

			return result;
		}

		protected abstract void CreateCertificate(CusEntryInstruction entryInstruction, ZString code, ZShort order);
		protected abstract RebateCalculator<T> GetRebateCalculator(JobDeclaration declaration);
		protected abstract ZString PermitTypeCore { get; }

		protected abstract PRVValueUnits PRVValueUnit { get; }

		protected JobDeclaration declaration;
		protected enum PRVValueUnits
		{
			RANDS,
			CENTS
		}
	}

	public class DummyCusEntryLine_RebateCalculatorAbstractTest : CusEntryLine
	{
		public DummyCusEntryLine_RebateCalculatorAbstractTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			Object = this;
		}

		public DummyCusEntryLine_RebateCalculatorAbstractTest Object { get; private set; }

		public ZBool IsSpecifiedMotorVehicleCoreReturn { get; set; }
		public CusEntryInstruction EntryInstructionCoreReturn { get; set; }

		protected override ZBool IsSpecifiedMotorVehicleCore => IsSpecifiedMotorVehicleCoreReturn;

		protected override CusEntryInstruction EntryInstructionCore => EntryInstructionCoreReturn;
	}
}
