using System.Globalization;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class InBondNumberGeneratorTest : TestCaseWithFactory
	{
		[UseSnapshotProtection]
		public void TestTryGetNextInBondNumber()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var branchFountain = Env.NumberFountains.USInBondNumberFountain(GlbBranch.CurrentBranch.PK.ToGuid());
			branchFountain.SetNext(Factory, 10021211);

			InBondNumberGenerator.TryGetNextInBondNumber(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), out var inBondNumber);
			AssertEquals("10021211" + InBondNumberCheckDigitCalculator.GetCheckDigit("10021211"), inBondNumber);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var inBondHeader = declaration.CustomsEntryHeaders.AddNew();
			inBondHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			inBondHeader.EntryNumber = "10021212" + InBondNumberCheckDigitCalculator.GetCheckDigit("10021212");
			inBondHeader.CusEntryNumber.CE_SystemCreateTimeUtc = ZDateTime.UtcNow.AddYears(-4);

			InBondNumberGenerator.TryGetNextInBondNumber(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), out inBondNumber);
			AssertEquals("10021212" + InBondNumberCheckDigitCalculator.GetCheckDigit("10021212"), inBondNumber);

			inBondHeader.EntryNumber = "10021213" + InBondNumberCheckDigitCalculator.GetCheckDigit("10021213");
			inBondHeader.CusEntryNumber.CE_SystemCreateTimeUtc = ZDateTime.UtcNow.AddYears(-2);
			InBondNumberGenerator.TryGetNextInBondNumber(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), out inBondNumber);
			AssertEquals("10021214" + InBondNumberCheckDigitCalculator.GetCheckDigit("10021214"), inBondNumber);
		}

		[UseSnapshotProtection]
		public void TestTryGetNextInBondNumberFromCompanyRange()
		{
			DeclarationTestHelper.SetupCompanySpecificInBondNumberRange();
			var currentCompany = Factory.Load<GlbCompany>(GlbBranch.CurrentBranch.GB_GC);
			var connection = ((IDbConnected)Factory).Connection;
			var companyFountain = Env.NumberFountains.USInBondNumberFountain(currentCompany.PK.ToGuid());
			var nextNumber = companyFountain.PeekPreliminary(connection);

			InBondNumberGenerator.TryGetNextInBondNumber(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), out var inBondNumber);
			AssertEquals(nextNumber.ToString("D8", CultureInfo.InvariantCulture) + InBondNumberCheckDigitCalculator.GetCheckDigit(nextNumber.ToString(CultureInfo.InvariantCulture)), inBondNumber);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var inBondHeader = declaration.CustomsEntryHeaders.AddNew();
			inBondHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			inBondHeader.EntryNumber = (nextNumber + 1).ToString("D8", CultureInfo.InvariantCulture) + InBondNumberCheckDigitCalculator.GetCheckDigit((nextNumber + 1).ToString(CultureInfo.InvariantCulture));
			inBondHeader.CusEntryNumber.CE_SystemCreateTimeUtc = ZDateTime.UtcNow;

			InBondNumberGenerator.TryGetNextInBondNumber(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), out inBondNumber);
			AssertEquals((nextNumber + 2).ToString("D8", CultureInfo.InvariantCulture) + InBondNumberCheckDigitCalculator.GetCheckDigit((nextNumber + 2).ToString(CultureInfo.InvariantCulture)), inBondNumber);
		}

		[UseSnapshotProtection]
		public void TestGetNextAvailableInBondNumberAndCheckReusable()
		{
			DeclarationTestHelper.SetupCompanySpecificInBondNumberRange();
			var currentCompany = Factory.Load<GlbCompany>(GlbBranch.CurrentBranch.GB_GC);
			var inBondNumberSetting = InBondNumberSetting.New(currentCompany);

			var connection = ((IDbConnected)Factory).Connection;
			var companyFountain = Env.NumberFountains.USInBondNumberFountain(currentCompany.PK.ToGuid());
			var nextNumber = companyFountain.PeekPreliminary(connection);

			var result = InBondNumberGenerator.GetNextAvailableInBondNumberAndCheckReusable(inBondNumberSetting, out var reusableInformation);
			AssertEquals((ZDecimal)nextNumber, result);
			AssertEquals("", reusableInformation);

			var header = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			var moveHeader = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			moveHeader.InBondNumber = "693000140";
			moveHeader.BM_BH = header.PK;

			var entryNum = Factory.New<CusEntryNumber>();
			entryNum.CE_EntryType = CusEntryHeaderMessageTypeList.Codes.InBond;
			entryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			entryNum.CE_EntryNum = inBondNumberSetting.GetNumberWithCheckDigit(nextNumber);
			entryNum.CE_ParentTable = CusInBondMoveHeaderSchema.Constants.TableName;
			entryNum.CE_ParentID = moveHeader.PK;
			entryNum.CE_SystemCreateTimeUtc = ZDateTime.UtcNow;

			entryNum = Factory.New<CusEntryNumber>();
			entryNum.CE_EntryType = CusEntryHeaderMessageTypeList.Codes.InBond;
			entryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			entryNum.CE_EntryNum = inBondNumberSetting.GetNumberWithCheckDigit(++nextNumber);
			entryNum.CE_ParentTable = CusInBondMoveHeaderSchema.Constants.TableName;
			entryNum.CE_SystemCreateTimeUtc = ZDateTime.UtcNow.AddYears(-4);
			Factory.Save();

			result = InBondNumberGenerator.GetNextAvailableInBondNumberAndCheckReusable(inBondNumberSetting, out reusableInformation);
			AssertEquals((ZDecimal)(nextNumber), result);
			AssertEquals($"In-bond number '{inBondNumberSetting.GetNumberWithCheckDigit(nextNumber)}' was previously under on unknown object but was re-issued by Customs.\r\nOK to proceed with re-using this number?", reusableInformation);

			entryNum.CE_ParentID = moveHeader.PK;
			Factory.Save();

			result = InBondNumberGenerator.GetNextAvailableInBondNumberAndCheckReusable(inBondNumberSetting, out reusableInformation);
			AssertEquals((ZDecimal)(nextNumber), result);
			AssertEquals($"In-bond number '{inBondNumberSetting.GetNumberWithCheckDigit(nextNumber)}' was previously under on {((IJobNumber)moveHeader).JobNumber} but was re-issued by Customs.\r\nOK to proceed with re-using this number?", reusableInformation);

			entryNum.CE_EntryNum = inBondNumberSetting.GetNumberWithCheckDigit(inBondNumberSetting.LastNumber);
			entryNum.CE_SystemCreateTimeUtc = ZDateTime.UtcNow.AddYears(-2);
			Factory.Save();

			companyFountain.SetNext(connection, (long)inBondNumberSetting.LastNumber);
			result = InBondNumberGenerator.GetNextAvailableInBondNumberAndCheckReusable(inBondNumberSetting, out reusableInformation);
			AssertEquals(ZDecimal.Zero, result);
			AssertEquals("All numbers in In-Bond number range have been allocated in past 3 year.", reusableInformation);

			companyFountain.SetNext(connection, inBondNumberSetting.MaximumAllowForNumberFountain + 1);
			result = InBondNumberGenerator.GetNextAvailableInBondNumberAndCheckReusable(inBondNumberSetting, out reusableInformation);
			AssertEquals(ZDecimal.Zero, result);
			AssertEquals("No available numbers in In-Bond number range.", reusableInformation);

			inBondNumberSetting.BranchPK = ZGuid.NewZGuid();
			inBondNumberSetting.CompanyPK = ZGuid.NewZGuid();
			result = InBondNumberGenerator.GetNextAvailableInBondNumberAndCheckReusable(inBondNumberSetting, out reusableInformation);
			AssertEquals(ZDecimal.Zero, result);
			AssertEquals("Invalid In-Bond number range.", reusableInformation);
		}
	}
}
