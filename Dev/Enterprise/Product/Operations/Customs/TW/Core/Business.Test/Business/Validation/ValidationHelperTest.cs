using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ValidationHelperTest : TestCaseWithFactory
	{
		public void TestCheckNoOfPacksBalance()
		{
			var declaration = Factory.New<JobDeclarationForTest>();
			declaration.JE_TotalNoOfPacksPackType = "CTN";
			declaration.JE_TotalNoOfPacks = 10;
			var invoice = declaration.Invoices.AddNew();
			var noOfPacksInfo = declaration.JE_MessageTypeInfo;
			declaration.JE_MessageType = "IMP";

			AssertNoWarningContaining(noOfPacksInfo, "The sum of all invoice header package numbers");
			AssertNoWarningContaining(noOfPacksInfo, "does not balance with the declaration total package number");
			invoice.JZ_NoOfPacks = 5;

			declaration.JE_MessageType = "EXP";
			AssertHasWarningContaining(noOfPacksInfo, "The sum of all invoice header package numbers");
			AssertHasWarningContaining(noOfPacksInfo, "does not balance with the declaration total package number");
			Assert(noOfPacksInfo.HasWarning("The sum of all invoice header package numbers {5 CTN} does not balance with the declaration total package number {10 CTN}."));
		}

		public void TestCheckOrganisationExistVAT()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			int testValue = 0;
			ValidationHelper.CheckOrganisationExistVAT(null, () => testValue += 1);
			AssertEquals(0, testValue);

			ValidationHelper.CheckOrganisationExistVAT(orgHeader, () => testValue += 1);
			AssertEquals(1, testValue);

			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("VAT", "123465788", "TW");
			ValidationHelper.CheckOrganisationExistVAT(orgHeader, () => testValue += 1);
			AssertEquals(1, testValue);
		}

		public void TestGetOriginInvalidCodeMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D8;
			AssertEquals(JobDeclarationValidation.MessageErrorPortCodeInvalid, ValidationHelper.GetOriginInvalidCodeMessage(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G1;
			AssertEquals(ValidationConstants.Declaration.EnterForeignPortCodeMessage, ValidationHelper.GetOriginInvalidCodeMessage(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G7;
			AssertEquals(ValidationConstants.Declaration.EnterForeignPortCodeMessage, ValidationHelper.GetOriginInvalidCodeMessage(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F1;
			AssertEquals(ValidationConstants.Declaration.EnterForeignPortCodeMessage, ValidationHelper.GetOriginInvalidCodeMessage(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G2;
			AssertEquals(ValidationConstants.Declaration.EnterTWPortCodeMessage, ValidationHelper.GetOriginInvalidCodeMessage(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D2;
			AssertEquals(ValidationConstants.Declaration.EnterTWPortCodeMessage, ValidationHelper.GetOriginInvalidCodeMessage(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D7;
			AssertEquals(ValidationConstants.Declaration.EnterTWPortCodeMessage, ValidationHelper.GetOriginInvalidCodeMessage(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F2;
			AssertEquals(ValidationConstants.Declaration.EnterTWPortCodeMessage, ValidationHelper.GetOriginInvalidCodeMessage(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F3;
			AssertEquals(ValidationConstants.Declaration.EnterTWPortCodeMessage, ValidationHelper.GetOriginInvalidCodeMessage(declaration));

			declaration.JE_MessageType = "EXP";
			AssertEquals(JobDeclarationValidation.MessageErrorPortCodeInvalid, ValidationHelper.GetOriginInvalidCodeMessage(declaration));
		}

		public void TestGetFinalDestinationInvalidCodeMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D5;
			AssertEquals(JobDeclarationValidation.MessageErrorPortCodeInvalid, ValidationHelper.GetFinalDestinationInvalidCodeMessage(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B8;
			AssertEquals(JobDeclarationValidation.MessageErrorPortCodeInvalid, ValidationHelper.GetFinalDestinationInvalidCodeMessage(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B9;
			AssertEquals(JobDeclarationValidation.MessageErrorPortCodeInvalid, ValidationHelper.GetFinalDestinationInvalidCodeMessage(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.G3;
			AssertEquals(ValidationConstants.Declaration.EnterForeignPortCodeMessage, ValidationHelper.GetFinalDestinationInvalidCodeMessage(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.G5;
			AssertEquals(ValidationConstants.Declaration.EnterForeignPortCodeMessage, ValidationHelper.GetFinalDestinationInvalidCodeMessage(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F5;
			AssertEquals(ValidationConstants.Declaration.EnterForeignPortCodeMessage, ValidationHelper.GetFinalDestinationInvalidCodeMessage(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D1;
			AssertEquals(ValidationConstants.Declaration.EnterTWPortCodeMessage, ValidationHelper.GetFinalDestinationInvalidCodeMessage(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B1;
			AssertEquals(ValidationConstants.Declaration.EnterTWPortCodeMessage, ValidationHelper.GetFinalDestinationInvalidCodeMessage(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B2;
			AssertEquals(ValidationConstants.Declaration.EnterTWPortCodeMessage, ValidationHelper.GetFinalDestinationInvalidCodeMessage(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F4;
			AssertEquals(ValidationConstants.Declaration.EnterTWPortCodeMessage, ValidationHelper.GetFinalDestinationInvalidCodeMessage(declaration));

			declaration.JE_MessageType = "IMP";
			AssertEquals(JobDeclarationValidation.MessageErrorPortCodeInvalid, ValidationHelper.GetFinalDestinationInvalidCodeMessage(declaration));
		}

		public void TestIsImportWarehouse2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.CusEntryInstruction.CEI_OA_Warehouse2 = ZGuid.NewZGuid();
			Assert(!ValidationHelper.IsImportWarehouse2(declaration));

			declaration.JE_MessageType = "IMP";
			Assert(ValidationHelper.IsImportWarehouse2(declaration));

			declaration.CusEntryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
			Assert(!ValidationHelper.IsImportWarehouse2(declaration));
		}

		public void TestIsExportWarehouse()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.CusEntryInstruction.CEI_OA_Warehouse = ZGuid.NewZGuid();
			Assert(!ValidationHelper.IsExportWarehouse(declaration));

			declaration.JE_MessageType = "EXP";
			Assert(ValidationHelper.IsExportWarehouse(declaration));

			declaration.CusEntryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
			Assert(!ValidationHelper.IsExportWarehouse(declaration));
		}

		public void TestIsImportTransportMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_TransportMode = "AIR";
			Assert(!ValidationHelper.IsImportTransportMode(declaration));

			declaration.JE_MessageType = "IMP";
			Assert(ValidationHelper.IsImportTransportMode(declaration));

			declaration.JE_TransportMode = ZString.Empty;
			Assert(!ValidationHelper.IsImportTransportMode(declaration));
		}

		public void TestIsOriginWithForeignPortIsRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G1;
			Assert(!ValidationHelper.IsOriginWithForeignPortIsRequired(declaration));

			declaration.JE_MessageType = "IMP";
			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G1;
			Assert(ValidationHelper.IsOriginWithForeignPortIsRequired(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G7;
			Assert(ValidationHelper.IsOriginWithForeignPortIsRequired(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F1;
			Assert(ValidationHelper.IsOriginWithForeignPortIsRequired(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.B6;
			Assert(!ValidationHelper.IsOriginWithForeignPortIsRequired(declaration));
		}

		public void TestIsOriginWithTWPortCodeIsRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G2;
			Assert(!ValidationHelper.IsOriginWithTWPortCodeIsRequired(declaration));

			declaration.JE_MessageType = "IMP";
			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G2;
			Assert(ValidationHelper.IsOriginWithTWPortCodeIsRequired(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D2;
			Assert(ValidationHelper.IsOriginWithTWPortCodeIsRequired(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D7;
			Assert(ValidationHelper.IsOriginWithTWPortCodeIsRequired(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F2;
			Assert(ValidationHelper.IsOriginWithTWPortCodeIsRequired(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F3;
			Assert(ValidationHelper.IsOriginWithTWPortCodeIsRequired(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G1;
			Assert(!ValidationHelper.IsOriginWithTWPortCodeIsRequired(declaration));
		}

		public void TestIsOriginWithAllPortCodeIsRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D8;
			Assert(!ValidationHelper.IsOriginWithAllPortCodeIsRequired(declaration));

			declaration.JE_MessageType = "IMP";
			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D8;
			Assert(ValidationHelper.IsOriginWithAllPortCodeIsRequired(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G1;
			Assert(!ValidationHelper.IsOriginWithAllPortCodeIsRequired(declaration));
		}

		public void TestIsFinalDestinationWithForeignPortIsRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.G3;
			Assert(!ValidationHelper.IsFinalDestinationWithForeignPortIsRequired(declaration));

			declaration.JE_MessageType = "EXP";
			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.G3;
			Assert(ValidationHelper.IsFinalDestinationWithForeignPortIsRequired(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.G5;
			Assert(ValidationHelper.IsFinalDestinationWithForeignPortIsRequired(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F5;
			Assert(ValidationHelper.IsFinalDestinationWithForeignPortIsRequired(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B1;
			Assert(!ValidationHelper.IsFinalDestinationWithForeignPortIsRequired(declaration));
		}

		public void TestIsFinalDestinationWithTWPortCodeIsRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D1;
			Assert(!ValidationHelper.IsFinalDestinationWithTWPortCodeIsRequired(declaration));

			declaration.JE_MessageType = "EXP";
			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D1;
			Assert(ValidationHelper.IsFinalDestinationWithTWPortCodeIsRequired(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B1;
			Assert(ValidationHelper.IsFinalDestinationWithTWPortCodeIsRequired(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B2;
			Assert(ValidationHelper.IsFinalDestinationWithTWPortCodeIsRequired(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F4;
			Assert(ValidationHelper.IsFinalDestinationWithTWPortCodeIsRequired(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B8;
			Assert(!ValidationHelper.IsFinalDestinationWithTWPortCodeIsRequired(declaration));
		}

		public void TestIsFinalDestinationWithAllPortCodeIsRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D5;
			Assert(!ValidationHelper.IsFinalDestinationWithAllPortCodeIsRequired(declaration));

			declaration.JE_MessageType = "EXP";
			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D5;
			Assert(ValidationHelper.IsFinalDestinationWithAllPortCodeIsRequired(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B8;
			Assert(ValidationHelper.IsFinalDestinationWithAllPortCodeIsRequired(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B9;
			Assert(ValidationHelper.IsFinalDestinationWithAllPortCodeIsRequired(declaration));

			declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F4;
			Assert(!ValidationHelper.IsFinalDestinationWithAllPortCodeIsRequired(declaration));
		}

		class JobDeclarationValidationForTest : AutoTWJobDeclarationValidation
		{
			public JobDeclarationValidationForTest(JobDeclarationForTest parent)
				: base(parent)
			{
			}

			protected override void CheckJE_MessageType()
			{
				var declaration = Parent as JobDeclaration;
				ValidationHelper.CheckNoOfPacksBalance(declaration, Parent.JE_MessageTypeInfo);
			}
		}

		class JobDeclarationForTest : JobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			protected override Customs.Business.JobDeclarationValidation GetNewValidation()
			{
				return new JobDeclarationValidationForTest(this);
			}
		}
	}
}
