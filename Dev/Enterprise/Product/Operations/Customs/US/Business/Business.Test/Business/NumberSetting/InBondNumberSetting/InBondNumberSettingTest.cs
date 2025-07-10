using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(InBondNumberSetting))]
	sealed class InBondNumberSettingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAvailableNumbersIsZeroFountainRolloverAndLastNumberIsAlreadyUsed()
		{
			DbConnection connection = ((IDbConnected)Factory).Connection;
			connection.BeginTransaction();
			try
			{
				var numberFountain = GetNumberFountain();
				numberFountain.SetNext(Factory, (long)(numberSetting.LastNumber - Enterprise.NumberFountain.FountainUtils.CacheSize - 1));
				while (numberFountain.GetNext(Factory) != numberSetting.LastNumber)
				{
				}

				AssertEquals(numberSetting.LastNumber + 1, numberSetting.CurrentNextNumber);
				AssertEquals(0L, (long)numberSetting.AvailableNumbers);
			}
			finally
			{
				connection.RollbackTransaction();
			}
		}

		public void TestMaximumNextNumberRestriction()
		{
			NumberSetting numberSetting = GetNewNumberSettingForMaximumNextNumberRestrictionTest();
			string message = "Because of a restriction in the System, the Next Number must be greater than or equal to " + numberSetting.StartNumber + " and less than or equal to " + numberSetting.MaximumNextNumberRestriction + ".";
			numberSetting.NextNumber = numberSetting.MaximumNextNumberRestriction;
			AssertNoError(numberSetting.NextNumberInfo, message);
			numberSetting.PostNextNumber();
			AssertEquals(numberSetting.MaximumNextNumberRestriction, (long)numberSetting.CurrentNextNumber);
			numberSetting.NextNumber = numberSetting.MaximumNextNumberRestriction + 1;
			AssertHasError(numberSetting.NextNumberInfo, message);
			numberSetting.PostNextNumber();
			AssertEquals(numberSetting.MaximumNextNumberRestriction, (long)numberSetting.CurrentNextNumber);
		}

		public void TestPostNextNumber()
		{
			var numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var branchNumberFountain = Env.NumberFountains.USInBondNumberFountain(GlbBranch.CurrentBranch.PK.ToGuid());
			long nextBranchNumber = (long)numberRange.StartNumber + 10;
			SetNext(branchNumberFountain, nextBranchNumber, GlbBranch.CurrentBranch.PK.ToGuid());
			InBondNumberSetting.NextNumber = numberRange.StartNumber + 300;
			InBondNumberSetting.PostNextNumber();
			AssertEquals("branchNumberFountain's next number", (long)(numberRange.StartNumber + 300), branchNumberFountain.PeekPreliminary(Factory));
			AssertEquals("NextNumber", 301L, (long)InBondNumberSetting.NextNumber);
		}

		public void TestGenerateCurrentNextNumberWithCheckDigit()
		{
			DbConnection connection = ((IDbConnected)Factory).Connection;
			connection.BeginTransaction();
			try
			{
				ZDecimal number = numberSetting.CurrentNextNumber;
				ZString inbondNumber = number.ToString().PadLeft(8, '0');
				inbondNumber += InBondNumberCheckDigitCalculator.GetCheckDigit(inbondNumber);
				AssertEquals(inbondNumber, numberSetting.GenerateCurrentNextNumberWithCheckDigit());
				AssertEquals(number + 1, numberSetting.CurrentNextNumber);
			}
			finally
			{
				connection.RollbackTransaction();
			}
		}

		public void TestGetCusEntryNumberMatching_Company()
		{
			var number = "42315632" + InBondNumberCheckDigitCalculator.GetCheckDigit("42315632");
			AssertNull(InBondNumberSetting.GetCusEntryNumberMatching(number));

			var companyPK = InBondNumberSetting.CompanyPK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EnableINB = true;
			var inBondHeader = declaration.CustomsEntryHeaders.AddNew();
			inBondHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			inBondHeader.EntryNumber = number;

			((IRegistryItemInternals)USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange).DeleteValue(companyPK.ToGuid(), Guid.Empty, Guid.Empty);
			foreach (GlbBranch branch in InBondNumberSetting.Company.Branches)
			{
				((IRegistryItemInternals)USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange).DeleteValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty);
			}
			var numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(companyPK.ToGuid(), Guid.Empty, Guid.Empty);
			numberRange.StartNumber = NumberFountains.USMinimumInBondNumber;
			numberRange.LastNumber = NumberFountains.USMaximumInBondNumber;
			numberRange.RunOutWarningLimitNumber = 60000;
			USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.SetValue(companyPK.ToGuid(), Guid.Empty, Guid.Empty, numberRange);
			AssertEquals(inBondHeader.CusEntryNumber, InBondNumberSetting.GetCusEntryNumberMatching(number));
		}

		public void TestGetCusEntryNumberMatching_Branch()
		{
			var number = "42315632" + InBondNumberCheckDigitCalculator.GetCheckDigit("42315632");
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EnableINB = true;
			declaration.JE_GB = InBondNumberSetting.BranchPK;
			var inBondHeader = declaration.CustomsEntryHeaders.AddNew();
			inBondHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			inBondHeader.EntryNumber = number;
			AssertEquals(inBondHeader.CusEntryNumber, InBondNumberSetting.GetCusEntryNumberMatching(number));

			declaration.JE_GB = ZGuid.Empty;
			AssertNull(InBondNumberSetting.GetCusEntryNumberMatching(number));
		}

		public void TestGetCusEntryNumberMatching_LinkObject()
		{
			var number = "42315632" + InBondNumberCheckDigitCalculator.GetCheckDigit("42315632");
			AssertNull(InBondNumberSetting.GetCusEntryNumberMatching(number));

			var header = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header.BH_GB = InBondNumberSetting.BranchPK;
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			var moveHeader = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			moveHeader.InBondNumber = "693000140";
			moveHeader.BM_BH = header.PK;

			var cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_ParentTable = CusInBondMoveHeaderSchema.Constants.TableName;
			cusEntryNumber.CE_ParentID = moveHeader.PK;
			cusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			cusEntryNumber.CE_EntryType = CusEntryHeaderMessageTypeList.Codes.InBond;
			cusEntryNumber.CE_EntryNum = number;

			AssertEquals(cusEntryNumber, InBondNumberSetting.GetCusEntryNumberMatching(number));
		}

		public void TestValidateNextNumber()
		{
			DbConnection connection = ((IDbConnected)Factory).Connection;
			connection.BeginTransaction();
			try
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_DeclarationReference = "B000234234";
				declaration.US_EnableINB = true;
				var inBondHeader = declaration.CustomsEntryHeaders.AddNew();
				inBondHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
				InBondNumberRange numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
				var branchNumberFountain = Env.NumberFountains.USInBondNumberFountain(GlbBranch.CurrentBranch.PK.ToGuid());
				long nextBranchNumber = (long)numberRange.StartNumber + 100;
				branchNumberFountain.SetNext(Factory, nextBranchNumber);
				string errorMessage = "The next In-Bond Number must be greater than or equal to 1 and less than or equal to 33333334.";
				InBondNumberSetting.NextNumber = 33333335;
				AssertHasError(InBondNumberSetting.NextNumberInfo, errorMessage);
				InBondNumberSetting.NextNumber = 0;
				AssertHasError(InBondNumberSetting.NextNumberInfo, errorMessage);
				InBondNumberSetting.NextNumber = 33333334;
				AssertNoError(InBondNumberSetting.NextNumberInfo, errorMessage);
				InBondNumberSetting.NextNumber = 1;
				AssertNoError(InBondNumberSetting.NextNumberInfo, errorMessage);
				GlbBranch newBranch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
				InBondNumberSetting.BranchPK = ZGuid.Empty;
				string invalidBranchError = "Please select a valid Branch. A Branch is needed for setting the next In-Bond Number.";
				string invalidRegistry = "Cannot use this Company or Branch range as the In-Bond Number Range setup in the registry 'Admin -> System -> Registry -> Customs -> Country or Region Specific -> United States of America -> Import -> ABI -> In-Bond Number -> Number Range' has errors.";
				AssertHasError(InBondNumberSetting.NextNumberInfo, invalidBranchError);
				AssertNoError(InBondNumberSetting.NextNumberInfo, invalidRegistry);
				InBondNumberSetting.BranchPK = ZGuid.NewZGuid();
				AssertHasError(InBondNumberSetting.NextNumberInfo, invalidBranchError);
				AssertNoError(InBondNumberSetting.NextNumberInfo, invalidRegistry);
				InBondNumberSetting.BranchPK = newBranch.PK;
				AssertNoError(InBondNumberSetting.NextNumberInfo, invalidBranchError);
				AssertHasError(InBondNumberSetting.NextNumberInfo, invalidRegistry);
			}
			finally
			{
				connection.RollbackTransaction();
			}
		}

		public void TestHasReachedLimit()
		{
			InBondNumberRange range = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			InBondNumberSetting.NextNumber = InBondNumberSetting.LastNumber - 60000;
			InBondNumberSetting.PostNextNumber();
			AssertEquals("HasReachedLimit", false, InBondNumberSetting.HasReachedLimit);
			InBondNumberSetting.NextNumber = InBondNumberSetting.LastNumber - 59999;
			InBondNumberSetting.PostNextNumber();
			AssertEquals("HasReachedLimit", true, InBondNumberSetting.HasReachedLimit);
		}

		public void TestWarningLimitMark()
		{
			AssertEquals("WarningLimitMark - default value", 60000L, (long)InBondNumberSetting.WarningLimitMark);
			InBondNumberSetting.BranchPK = ZGuid.Empty;
			AssertEquals("WarningLimitMark", 0L, (long)InBondNumberSetting.WarningLimitMark);
		}

		public void TestProperties()
		{
			InBondNumberRange range = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			AssertEquals("BranchPK", GlbBranch.CurrentBranch.PK, InBondNumberSetting.BranchPK);
			AssertEquals("StartNumber", range.StartNumber, InBondNumberSetting.StartNumber);
			AssertEquals("LastNumber", range.LastNumber, InBondNumberSetting.LastNumber);
			AssertEquals("CurrentNextNumber", Env.NumberFountains.USInBondNumberFountain(GlbBranch.CurrentBranch.PK.ToGuid()).PeekPreliminary(Factory), (long)InBondNumberSetting.CurrentNextNumber);
			InBondNumberSetting.NextNumber = 10;
			AssertEquals("NextNumber", 10L, (long)InBondNumberSetting.NextNumber);
			AssertEquals("AvailableNumbers", 33333334L, (long)InBondNumberSetting.AvailableNumbers);
			InBondNumberSetting.BranchPK = ZGuid.Empty;
			AssertEquals("StartNumber", 0L, (long)InBondNumberSetting.StartNumber);
			AssertEquals("LastNumber", 0L, (long)InBondNumberSetting.LastNumber);
			AssertEquals("CurrentNextNumber", 0L, (long)InBondNumberSetting.CurrentNextNumber);
			AssertEquals("NextNumber", 0L, (long)InBondNumberSetting.NextNumber);
			AssertEquals("AvailableNumbers", 0L, (long)InBondNumberSetting.AvailableNumbers);
		}

		public void TestEnsureCurrentNextNumberIsCorrect()
		{
			var numberFountain = Env.NumberFountains.USInBondNumberFountain(GlbCompany.CurrentCompany.Branches[1].PK.ToGuid());
			var setting = InBondNumberSetting.New(Factory.Load<GlbBranch>(GlbCompany.CurrentCompany.Branches[1].PK));
			setting.BranchPK = GlbCompany.CurrentCompany.Branches[1].PK;
			AssertEquals(true, setting.StartNumber > 0);
			long number = (long)setting.StartNumber - 1;
			SetNext(numberFountain, number, GlbCompany.CurrentCompany.Branches[1].PK.ToGuid());
			AssertEquals(number, (long)setting.CurrentNextNumber);
			setting.EnsureCurrentNextNumberIsCorrect();
			AssertEquals((long)setting.StartNumber, (long)setting.CurrentNextNumber);
			setting.EnsureCurrentNextNumberIsCorrect();
			AssertEquals((long)setting.StartNumber, (long)setting.CurrentNextNumber);
			number = (long)setting.StartNumber + 10;
			SetNext(numberFountain, number, GlbCompany.CurrentCompany.Branches[1].PK.ToGuid());
			AssertEquals(number, (long)setting.CurrentNextNumber);
			setting.EnsureCurrentNextNumberIsCorrect();
			AssertEquals(number, (long)setting.CurrentNextNumber);
		}

		NumberSetting numberSetting;
		protected override void SetUp()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			base.SetUp();
			numberSetting = InBondNumberSetting.New(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK));
		}

		protected override BusinessObject GetNewBusinessObject() => InBondNumberSetting.New(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK));

		INumberFountainProxy GetNumberFountain()
		{
			InBondNumberRange numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			return Env.NumberFountains.USInBondNumberFountain(GlbBranch.CurrentBranch.PK.ToGuid());
		}

		NumberSetting GetNewNumberSettingForMaximumNextNumberRestrictionTest()
		{
			var result = InBondNumberSetting.New(Factory.Load<GlbBranch>(GlbCompany.CurrentCompany.Branches[GlbCompany.CurrentCompany.Branches.Count - 1].PK));
			AssertEquals("Expected last InBondNumberSetting", result.MaximumAllowForNumberFountain, (long)result.LastNumber);
			return result;
		}

		void SetNext(INumberFountainProxy numberFountain, long number, Guid owner)
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			using (var transactionManager = connection.BeginTransactionWithManager())
			{
				numberFountain.SetNext(connection, number);
				transactionManager.CommitTransaction();
				Enterprise.ZArchitecture.Core.Testing.FountainTestListener.AddFountainAccess(NumberFountain.NumberFountains.USInBondNumber, owner);
			}
		}

		InBondNumberSetting InBondNumberSetting => (InBondNumberSetting)numberSetting;
	}
}
