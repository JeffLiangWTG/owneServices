using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AllocateInBondNumber))]
	sealed class AllocateInBondNumberTest : NonPersistentBusinessObjectTestCase
	{
		[UseSnapshotProtection]
		public void TestAI_InBondNumber()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var branch = GlbBranch.CurrentBranch;
			var entrySetting = InBondNumberSetting.New(branch);
			var branchFountain = Env.NumberFountains.USInBondNumberFountain(branch.PK.ToGuid());
			branchFountain.SetNext(Factory, 100999);
			ZString number = entrySetting.CurrentNextNumber.ToString().PadLeft(8, '0');
			number += InBondNumberCheckDigitCalculator.GetCheckDigit(number);
			AllocateInBondNumber.AI_InBondNumber = number;
			AssertNoError(AllocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.MustBeNumeric);
			AssertNoError(AllocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.LengthShouldBeNine);
			AssertNoError(AllocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.AlreadyExists);
			AllocateInBondNumber.AI_InBondNumber = "XYX";
			AssertHasError(AllocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.MustBeNumeric);
			AllocateInBondNumber.AI_InBondNumber = "00000000";
			AssertHasError(AllocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.LengthShouldBeNine);

			AllocateInBondNumber.AI_InBondNumber = "901002122";
			AssertHasError(AllocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidCheckDigit("6"));
			AllocateInBondNumber.AI_InBondNumber = "901002126";
			AssertNoError(AllocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidCheckDigit("6"));
			AssertNoErrors(AllocateInBondNumber.AI_InBondNumberInfo);
			var moveHeader = (CusInBondMoveHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			var cusInBondNumber = CusEntryNumber.LoadOrCreate(moveHeader, CusEntryHeaderMessageTypeList.Codes.InBond, Core.Constants.CountryCodes.UnitedStates);
			cusInBondNumber.CE_EntryNum = "001002993";
			cusInBondNumber.CE_SystemCreateTimeUtc = ZDateTime.UtcNow.AddYears(-4);
			var branchErrorMessage = InBondNumberAvailabilityChecker.InBondNumberRangeNotSetup(((IRegistryItemInternals)USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange).Location);
			var notEnoughAvailableInBondNumbersMessageError = InBondNumberAvailabilityChecker.NotEnoughAvailableInBondNumbersForBranchForJob(branch.GB_Code, branch.GB_BranchName);
			AllocateInBondNumber.AI_InBondNumber = "001002993";
			AssertNoError(AllocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.AlreadyExists);
			AssertNoError(AllocateInBondNumber.AI_InBondNumberInfo, notEnoughAvailableInBondNumbersMessageError);
			AssertNoError(AllocateInBondNumber.AI_InBondNumberInfo, branchErrorMessage);

			AllocateInBondNumber.AI_InBondNumber = "";
			cusInBondNumber.CE_SystemCreateTimeUtc = ZDateTime.UtcNow;
			AllocateInBondNumber.AI_InBondNumber = "001002993";
			AssertHasError(AllocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.AlreadyExists);
			AssertNoError(AllocateInBondNumber.AI_InBondNumberInfo, notEnoughAvailableInBondNumbersMessageError);
			AssertNoError(AllocateInBondNumber.AI_InBondNumberInfo, branchErrorMessage);

			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var otherBranch = currentCompany.Branches.AddNew();
			otherBranch.GB_Code = "~Z~";
			var numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(Guid.Empty, otherBranch.PK.ToGuid(), Guid.Empty);
			numberRange.StartNumber = 0;
			AssertEquals(false, numberRange.IsRangeValidForNumberFountain);
			var other = new AllocateInBondNumber(otherBranch);
			other.AI_InBondNumber = ZString.Empty;
			AssertHasError(other.AI_InBondNumberInfo, branchErrorMessage);
			numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var branchNumberFountain = Env.NumberFountains.USInBondNumberFountain(Env.CurrentBranch.PK);
			branchNumberFountain.SetNext(Factory, (long)(numberRange.LastNumber - 101));
			while (branchNumberFountain.GetNext(Factory) != numberRange.LastNumber)
			{
			}

			AllocateInBondNumber.AI_InBondNumber = ZString.Empty;
			AssertHasError(AllocateInBondNumber.AI_InBondNumberInfo, notEnoughAvailableInBondNumbersMessageError);
			branchNumberFountain.SetNext(Factory, (long)(numberRange.LastNumber - 101));
			var numberToCheck = (long)(numberRange.LastNumber - 1);
			while (numberToCheck != branchNumberFountain.PeekPreliminary(Factory))
			{
				branchNumberFountain.GetNext(Factory);
			}

			AssertEquals("", InBondNumberAvailabilityChecker.Check(GlbBranch.CurrentBranch));
		}

		public void TestAI_InBondNumberForPostDepartureOnly()
		{
			var allocateInBondNumber = new AllocateInBondNumber(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), true);
			allocateInBondNumber.AI_InBondNumber = "";
			AssertHasError(allocateInBondNumber.AI_InBondNumberInfo, "In-Bond Number cannot be empty.");
			allocateInBondNumber.AI_InBondNumber = "XYX";
			AssertHasError(allocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.MustBeNumeric);
			allocateInBondNumber.AI_InBondNumber = "00000000";
			AssertNoError(allocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.MustBeNumeric);
			AssertHasError(allocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.InBondNumberLengthShouldBeNineOrEleven);
			var moveHeader = (CusInBondMoveHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			var cusInBondNumber = CusEntryNumber.LoadOrCreate(moveHeader, CusEntryHeaderMessageTypeList.Codes.InBond, Core.Constants.CountryCodes.UnitedStates);
			cusInBondNumber.CE_EntryNum = "001002993";
			cusInBondNumber.CE_SystemCreateTimeUtc = ZDateTime.UtcNow;
			allocateInBondNumber.AI_InBondNumber = "001002993";
			AssertNoError(allocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.InBondNumberLengthShouldBeNineOrEleven);
			AssertHasError(allocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.AlreadyExists);
			allocateInBondNumber.AI_InBondNumber = "001002994";
			AssertNoError(allocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.AlreadyExists);
		}

		[UseSnapshotProtection]
		public void TestAllocateInBondNumberFromCompanyRegistry()
		{
			DeclarationTestHelper.SetupCompanySpecificInBondNumberRange();
			var company = GlbBranch.CurrentBranch.Company;
			var entrySetting = InBondNumberSetting.New(company);
			var companyFountain = Env.NumberFountains.USInBondNumberFountain(company.PK.ToGuid());
			companyFountain.SetNext(Factory, 100999);
			ZString number = entrySetting.CurrentNextNumber.ToString().PadLeft(8, '0');
			number += InBondNumberCheckDigitCalculator.GetCheckDigit(number);
			AllocateInBondNumber.AI_InBondNumber = number;
			AssertNoError(AllocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.MustBeNumeric);
			AssertNoError(AllocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.LengthShouldBeNine);
			AssertNoError(AllocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.AlreadyExists);
			AllocateInBondNumber.AI_InBondNumber = "XYX";
			AssertHasError(AllocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.MustBeNumeric);
			AllocateInBondNumber.AI_InBondNumber = "00000000";
			AssertHasError(AllocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.LengthShouldBeNine);
			
			AllocateInBondNumber.AI_InBondNumber = "901002122";
			AssertHasError(AllocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidCheckDigit("6"));
			AllocateInBondNumber.AI_InBondNumber = "901002126";
			AssertNoError(AllocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidCheckDigit("6"));
			AssertNoErrors(AllocateInBondNumber.AI_InBondNumberInfo);
			var moveHeader = (CusInBondMoveHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			var cusInBondNumber = CusEntryNumber.LoadOrCreate(moveHeader, CusEntryHeaderMessageTypeList.Codes.InBond, Core.Constants.CountryCodes.UnitedStates);
			cusInBondNumber.CE_EntryNum = "001002993";
			cusInBondNumber.CE_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var companyErrorMessage = InBondNumberAvailabilityChecker.InBondNumberRangeNotSetup(((IRegistryItemInternals)USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange).Location);
			var notEnoughAvailableInBondNumbersMessageError = InBondNumberAvailabilityChecker.NotEnoughAvailableInBondNumbersForCompanyForJob(company.GC_Code, company.GC_Name);
			AllocateInBondNumber.AI_InBondNumber = "001002993";
			AssertHasError(AllocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.AlreadyExists);
			AssertNoError(AllocateInBondNumber.AI_InBondNumberInfo, notEnoughAvailableInBondNumbersMessageError);
			AssertNoError(AllocateInBondNumber.AI_InBondNumberInfo, companyErrorMessage);
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var otherBranch = currentCompany.Branches.AddNew();
			otherBranch.GB_Code = "~Z~";
			var numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(Guid.Empty, otherBranch.PK.ToGuid(), Guid.Empty);
			numberRange.StartNumber = 0;
			AssertEquals(false, numberRange.IsRangeValidForNumberFountain);
			numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var branchNumberFountain = Env.NumberFountains.USInBondNumberFountain(company.PK.ToGuid());
			branchNumberFountain.SetNext(Factory, (long)(numberRange.LastNumber - 101));
			while (branchNumberFountain.GetNext(Factory) != numberRange.LastNumber)
			{
			}

			AllocateInBondNumber.AI_InBondNumber = ZString.Empty;
			AssertHasError(AllocateInBondNumber.AI_InBondNumberInfo, notEnoughAvailableInBondNumbersMessageError);
			branchNumberFountain.SetNext(Factory, (long)(numberRange.LastNumber - 101));
			var numberToCheck = (long)(numberRange.LastNumber - 1);
			while (numberToCheck != branchNumberFountain.PeekPreliminary(Factory))
			{
				branchNumberFountain.GetNext(Factory);
			}

			AssertEquals("", InBondNumberAvailabilityChecker.Check(GlbBranch.CurrentBranch));
			((IRegistryItemInternals)USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange).DeleteValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AllocateInBondNumber.AI_InBondNumber = ZString.Empty;
			AssertHasError(AllocateInBondNumber.AI_InBondNumberInfo, companyErrorMessage);
		}

		public void TestPaperlessNumber()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			AllocateInBondNumber.AllowPaperlessNumber = false;
			AssertEquals(9, AllocateInBondNumber.AI_InBondNumberInfo.MaxLength);
			AllocateInBondNumber.AllowPaperlessNumber = true;
			AssertEquals(11, AllocateInBondNumber.AI_InBondNumberInfo.MaxLength);
			AllocateInBondNumber.AI_InBondNumber = "V234";
			AssertHasError(AllocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			AssertNoError(AllocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidCheckDigit("3"));
			AllocateInBondNumber.AI_InBondNumber = "V1A23849632";
			AssertNoError(AllocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			AssertHasError(AllocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidCheckDigit("3"));
			AllocateInBondNumber.AI_InBondNumber = "V1A23849633";
			AssertNoError(AllocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			AssertNoError(AllocateInBondNumber.AI_InBondNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidCheckDigit("3"));
		}

		public void TestGetNextAvailableInBondNumberAndCheckReusable()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			var inBondNumberSetting = InBondNumberSetting.New(branch);
			var allocateInBondNumber = new AllocateInBondNumber(branch);

			var connection = ((IDbConnected)Factory).Connection;
			var companyFountain = Env.NumberFountains.USInBondNumberFountain(branch.PK.ToGuid());
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

			result = allocateInBondNumber.GetNextAvailableInBondNumberAndCheckReusable(out reusableInformation);
			AssertEquals((ZDecimal)(nextNumber), result);
			AssertEquals($"In-bond number '{inBondNumberSetting.GetNumberWithCheckDigit(nextNumber)}' was previously under on unknown object but was re-issued by Customs.\r\nOK to proceed with re-using this number?", reusableInformation);

			entryNum.CE_ParentID = moveHeader.PK;
			Factory.Save();

			result = allocateInBondNumber.GetNextAvailableInBondNumberAndCheckReusable(out reusableInformation);
			AssertEquals((ZDecimal)(nextNumber), result);
			AssertEquals($"In-bond number '{inBondNumberSetting.GetNumberWithCheckDigit(nextNumber)}' was previously under on {((IJobNumber)moveHeader).JobNumber} but was re-issued by Customs.\r\nOK to proceed with re-using this number?", reusableInformation);
		}

		[UseSnapshotProtection]
		public void TestPostNext()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			var allocateInBondNumber = new AllocateInBondNumber(branch);

			var connection = ((IDbConnected)Factory).Connection;
			var companyFountain = Env.NumberFountains.USInBondNumberFountain(branch.PK.ToGuid());
			var nextNumber = companyFountain.PeekPreliminary(connection);

			var result = allocateInBondNumber.GetNextAvailableInBondNumberAndCheckReusable(out var reusableInformation);
			AssertEquals((ZDecimal)nextNumber, result);

			Assert(allocateInBondNumber.PostNextNumber(++result));
			nextNumber = companyFountain.PeekPreliminary(connection);
			AssertEquals((ZDecimal)nextNumber, result);
		}

		protected override BusinessObject GetNewBusinessObject() => AllocateInBondNumber;

		AllocateInBondNumber allocateInBondNumber;
		AllocateInBondNumber AllocateInBondNumber => allocateInBondNumber ?? (allocateInBondNumber = new AllocateInBondNumber(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK)));
	}
}
