using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class FormalDeclarationEntryNumberSupporterTest : TestCaseWithFactory
	{
		public void TestGetAllocateEntryNumberSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			IAllocateNumberSupporter supporter = new FormalDeclarationEntryNumberSupporter(declaration);
			AssertEquals("Entry Number", supporter.NumberType);
			supporter.DoAllocate("12345678");
			AssertEquals("Allocated", "12345678", declaration.EntryNumber);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			supporter.DoAllocate("12345678");
			AssertEquals("12345678", declaration.EntryNumber);
			AssertEquals("12345678", supporter.GetExistingNumber());
			declaration = Declaration;
			supporter = new FormalDeclarationEntryNumberSupporter(declaration);
			AssertEquals("", supporter.GetReasonToStopProceeding());
			var company = declaration.Company;
			var provider = company.CustomsNumberProvider;
			provider.CustomsNumbers.DeleteAll();
			provider.CustomsNumberWrappers.RemoveAndDeleteAll();
			declaration.CusEntryInstruction.CEI_Style = "G1";
			var stmNums = provider.CustomsNumbers.AddNew();
			var wrapper = (TWCustomsNumberViewStmNumsWrapper)stmNums.Wrapper;
			wrapper.MessageType = "EXP";
			wrapper.RangeType = "A";
			wrapper.EndNumber = "E0005";
			wrapper.CurrentValue = "E0005";
			provider.CustomsNumberWrappers.Add(wrapper);
			AssertEquals("All sequential numbers for the current entry number range have been used up. Please visit Maintain > User Admin > Companies > EDI - Eagle Datamation International > Number Ranges to maintain the number ranges.", supporter.GetReasonToStopProceeding());
			wrapper.CurrentValue = "E0001";
			AssertEquals("", supporter.GetReasonToStopProceeding());
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			entryHeader.CH_Status = JobDeclarationMessageStatusList.Codes.AWC;
			AssertNullOrEmpty(supporter.GetReasonToStopProceeding());
		}

		public void TestGetConfirmMessagesWhenIsWaitingForResponseOrHasBeenLodgedAtCustoms()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			IAllocateNumberSupporter supporter = new FormalDeclarationEntryNumberSupporter(declaration);
			AssertEquals("The job is still waiting for a response or has been acknowledged by the customs. Allocating a new entry number means that this declaration job will be treated as a new entry in the customs’ system. Do you want to proceed?", supporter.GetConfirmMessagesWhenIsWaitingForResponseOrHasBeenLodgedAtCustoms());
		}

		[TestDate(2020, 8, 5)]
		public void TestAllocationUseMutex()
		{
			var declaration = Declaration;
			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var declarationInDiffFactory = newFactory.Load<JobDeclaration>(declaration.PK);
			IAllocateNumberSupporter supporterInDiffFactory = new FormalDeclarationEntryNumberSupporter(declarationInDiffFactory);
			AssertEquals(true, supporterInDiffFactory.LockNumberAllocationMutex);
			supporterInDiffFactory.DoAllocate("BB  0912300005");
			AssertEquals("Allocated", "BB  0912300005", declarationInDiffFactory.EntryNumber);
			var entryHeader = declarationInDiffFactory.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = declarationInDiffFactory.CusEntryInstruction.PK;
			entryHeader.CH_Status = JobDeclarationMessageStatusList.Codes.AWC;
			supporterInDiffFactory.DoAllocate("BB  0912300005");
			AssertEquals("Allocated", "BB  0912300005", declarationInDiffFactory.EntryNumber);
			IAllocateNumberSupporter supporter = new FormalDeclarationEntryNumberSupporter(declaration);
			AssertEquals(false, supporter.LockNumberAllocationMutex);
			supporter.UnlockNumberAllocationMutex();
			AssertEquals(false, supporter.LockNumberAllocationMutex);
			AssertEquals(true, supporterInDiffFactory.LockNumberAllocationMutex);
			supporterInDiffFactory.UnlockNumberAllocationMutex();
		}

		public void TestGetNewAllocateNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			IAllocateNumberSupporter allocateNumberSupporter = new FormalDeclarationEntryNumberSupporter(declaration);
			AssertType<JobDeclarationAllocateNumber>(allocateNumberSupporter.GetNewAllocateNumber());
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					var orgHeader = Factory.New<OrgHeader>();
					orgHeader.OH_Code = "Buyer TW";
					orgHeader.OH_FullName = "Buyer TW";
					var orgCusCode = orgHeader.CustomsCodes.AddNew();
					orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
					orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
					orgCusCode.OK_OH = orgHeader.PK;
					orgCusCode.OK_CustomsRegNo = "1245";
					var staff = Factory.NewWithValidTestData<GlbStaff>();
					staff.GS_Code = "TT";
					var company = GlbCompany.CurrentCompany;
					var extPassword1 = Factory.New<GlbExternalPassword>();
					extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
					extPassword1.GP_GC = company.PK;
					extPassword1.GP_MailBoxID = "123-3";
					extPassword1.GP_UserID = "001";
					extPassword1.GP_GS = staff.PK;
					extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
					declaration = Factory.NewWithValidTestData<JobDeclaration>();
					declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
					declaration.JE_GS_NKCusAgent = "TT";
					declaration.JE_CustomsProfile = "123-3";
					declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
					var entryInstruction = declaration.CusEntryInstruction;
					entryInstruction.CEI_CustomsOffice = "BB";
					entryInstruction.CEI_Style = "G1";
					entryInstruction.CEI_BoxNumber = "123";
					var invoice = declaration.Invoices.AddNew();
					invoice.JZ_OH_Supplier = orgHeader.PK;
					Factory.Save();
				}

				return declaration;
			}
		}

		JobDeclaration declaration;
	}
}
