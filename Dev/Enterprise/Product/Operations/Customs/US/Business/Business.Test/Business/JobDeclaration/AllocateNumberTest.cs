using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AllocateNumber))]
	public class AllocateNumberTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAE_Number()
		{
			var entrySetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XXX");
			AllocateNumber.AE_Number = "00000000";
			AssertNoErrorContaining(AllocateNumber.AE_NumberInfo, AllocateNumber.Constants.MustBeNumeric);
			AssertNoErrorContaining(AllocateNumber.AE_NumberInfo, AllocateNumberExtraValidation.Constants.FormalEntryNumber.AlreadyExists);

			AllocateNumber.AE_Number = "XYX";
			AssertHasError(AllocateNumber.AE_NumberInfo, AllocateNumber.Constants.MustBeNumeric);

			var companyStmNums = DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XXX");
			var wholeCompanyFountain = companyStmNums.TryGetNumberFountain();
			wholeCompanyFountain.SetNext(Factory, 100212);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XXX";

			CusEntryNumber cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_ParentID = declaration.PK;
			cusEntryNumber.CE_ParentTable = JobDeclarationSchema.Constants.TableName;
			cusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			cusEntryNumber.CE_EntryType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			cusEntryNumber.CE_EntryNum = "00100299";

			AllocateNumber.AE_Number = "00100299";
			AssertHasError(AllocateNumber.AE_NumberInfo, AllocateNumberExtraValidation.Constants.FormalEntryNumber.AlreadyExists);
		}

		AllocateNumber AllocateNumber
		{
			get
			{
				if (allocateNumber == null)
				{
					var args = new AllocateNumberArgs();
					args.Branch = GlbBranch.CurrentBranch;
					args.EntryFilerCode = "XXX";
					args.Factory = Factory;
					args.MaxLength = 8;
					args.ValidateNumber = (info, validator) => validator.ValidateFormalEntryNumber(info);
					allocateNumber = new AllocateNumber(args);
				}

				return allocateNumber;
			}
		}
		AllocateNumber allocateNumber;

		protected override BusinessObject GetNewBusinessObject()
		{
			return AllocateNumber;
		}
	}
}
