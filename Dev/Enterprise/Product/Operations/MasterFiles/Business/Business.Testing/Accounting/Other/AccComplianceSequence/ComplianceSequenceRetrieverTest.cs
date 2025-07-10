using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ComplianceSequenceRetrieverTest : TestCaseWithFactory
	{
		[TestDate(2012, 11, 11)]
		public void TestComplianceSequenceFromSubTypeUseParentBook()
		{
			var currCompany = GlbCompany.CurrentCompany.PK;
			var currBranch = GlbBranch.CurrentBranch.PK;

			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = currCompany;
			complianceSequence.XD_GB_BranchOwner = currBranch;
			complianceSequence.XD_Code = "AAA";
			complianceSequence.XD_SequenceClass = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequence.XD_Prefix = "abc-001";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			complianceSequence.XD_IsActive = true;
			complianceSequence.XD_ExpiryDate = ZDate.Today.AddMonths(1).AddDays(1);
			Factory.Save();

			var collection = new ComplianceSubTypeDependencyConfigurationCollection();
			var item = collection.AddNew();
			item.Country = CountryCodes.Peru;
			item.ChildSubType = PeruComplianceInfo.ComplianceSubTypeCodes.CAE;
			item.ParentSubType = PeruComplianceInfo.ComplianceSubTypeCodes.DSB;
			item = collection.AddNew();
			item.Country = CountryCodes.Peru;
			item.ChildSubType = PeruComplianceInfo.ComplianceSubTypeCodes.DSB;
			item.ParentSubType = PeruComplianceInfo.ComplianceSubTypeCodes.HON;
			item = collection.AddNew();
			item.Country = CountryCodes.Peru;
			item.ChildSubType = PeruComplianceInfo.ComplianceSubTypeCodes.HON;
			item.ParentSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;

			using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeDependencyConfiguration.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Peru))
			{
				var header = Factory.NewWithValidTestData<AccTransactionHeader>();
				header.AH_GC = currCompany;
				header.AH_GB = currBranch;
				header.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.CAE;
				header.AH_TransactionReference = "";
				AssertEquals("should find sequence CAE=>DSB=>HON=>TXI", complianceSequence.PK, header.ComplianceSequenceFromSubType.PK);

				header = Factory.NewWithValidTestData<AccTransactionHeader>();
				header.AH_GC = currCompany;
				header.AH_GB = currBranch;
				header.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.DSB;
				header.AH_TransactionReference = "";
				AssertEquals("should find sequence DSB=>HON=>TXI", complianceSequence.PK, header.ComplianceSequenceFromSubType.PK);

				header = Factory.NewWithValidTestData<AccTransactionHeader>();
				header.AH_GC = currCompany;
				header.AH_GB = currBranch;
				header.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.HON;
				header.AH_TransactionReference = "";
				AssertEquals("should find sequence HON=>TXI", complianceSequence.PK, header.ComplianceSequenceFromSubType.PK);
			}
		}

		[TestDate(2012, 11, 11)]
		public void TestComplianceSequenceFromSubType()
		{
			var currCompany = GlbCompany.CurrentCompany.PK;
			var currBranch = GlbBranch.CurrentBranch.PK;

			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = currCompany;
			complianceSequence.XD_GB_BranchOwner = currBranch;
			complianceSequence.XD_Code = "AAA";
			complianceSequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequence.XD_Prefix = "abc-001";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			complianceSequence.XD_IsActive = true;
			complianceSequence.XD_ExpiryDate = ZDate.Today.AddMonths(1).AddDays(1);
			Factory.Save();

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_GC = currCompany;
			header.AH_GB = currBranch;
			header.AH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			header.AH_TransactionReference = "";
			AssertEquals("should find sequence", complianceSequence.PK, header.ComplianceSequenceFromSubType.PK);

			header.AH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR;
			AssertEquals("should NOT find sequence", null, header.ComplianceSequenceFromSubType);

			header.AH_ComplianceSubType = "TXI";
			var complianceSequence2 = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence2.XD_GC_Company = currCompany;
			complianceSequence2.XD_GB_BranchOwner = currBranch;
			complianceSequence2.XD_Code = "BBB";
			complianceSequence2.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequence2.XD_Prefix = "abc-001";
			complianceSequence2.XD_StartNumber = 100;
			complianceSequence2.XD_EndNumber = 200;
			complianceSequence2.XD_MaximumNumberDigits = 4;
			complianceSequence2.XD_NextNumber = 25;
			complianceSequence2.XD_IsActive = true;
			complianceSequence2.XD_ExpiryDate = complianceSequence.XD_ExpiryDate.AddYears(1);
			Factory.Save();

			try
			{
				var x = header.ComplianceSequenceFromSubType;
				Fail("Should not reach this line as exception should be thrown");
			}
			catch (MultipleComplianceSequenceFoundException ex)
			{
				AssertEquals(@$"Please check your Compliance Invoice Book Setups. 
 {Core.Constants.ProductName} was unable to determine which Compliance Book should to use when allocating numbers. 
 There should only be one Active Compliance Book per Sub Type and Branch at any one time.", ex.UserFriendlyMessage);
			}
		}

		[TestDate(2012, 11, 11)]
		public void TestComplianceSequenceFromSubTypeWithPostDateInItaly()
		{
			var todayNextMonth = ZDate.Today.AddMonths(1);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code))
			{
				var currCompany = GlbCompany.CurrentCompany.PK;
				var currBranch = GlbBranch.CurrentBranch.PK;

				var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				complianceSequence.XD_GC_Company = currCompany;
				complianceSequence.XD_GB_BranchOwner = currBranch;
				complianceSequence.XD_Code = "AAA";
				complianceSequence.XD_SequenceClass = ItalyComplianceInfo.ComplianceSubTypeCodes.ARI;
				complianceSequence.XD_Prefix = "abc-001";
				complianceSequence.XD_StartNumber = 1;
				complianceSequence.XD_EndNumber = 99;
				complianceSequence.XD_MaximumNumberDigits = 4;
				complianceSequence.XD_NextNumber = 25;
				complianceSequence.XD_IsActive = true;
				complianceSequence.XD_ExpiryDate = todayNextMonth.AddDays(1);
				Factory.Save();

				var header = Factory.NewWithValidTestData<AccTransactionHeader>();
				header.AH_GC = currCompany;
				header.AH_GB = currBranch;
				header.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.ARI;
				header.AH_TransactionReference = "";
				header.AH_PostDate = todayNextMonth;
				AssertEquals("should find sequence", complianceSequence.PK, header.ComplianceSequenceFromSubType.PK);

				header.AH_PostDate = todayNextMonth.AddDays(2);
				AssertEquals("should NOT find sequence", null, header.ComplianceSequenceFromSubType);

				ZDateTime wrongPostDate;
				ZDateTime.TryParseExact("1213212", out wrongPostDate, "DDMMYYYY");
				Assert(!wrongPostDate.IsValid);
				header.AH_PostDate = wrongPostDate;
				AssertEquals("should find sequence even with invalid post date", complianceSequence.PK, header.ComplianceSequenceFromSubType.PK);
			}
		}

		[TestDate(2018, 07, 03)]
		public void TestComplianceSequenceFromSubTypeAddValidDateAndAllocationLevel()
		{
			var today = ZDate.Today;
			var firstDay = new ZDate(today.Year, 1, 1);
			var lastDay = new ZDate(today.Year, 12, 31);

			var currCompany = GlbCompany.CurrentCompany.PK;
			var currBranch = GlbBranch.CurrentBranch.PK;
			var currDept = GlbDepartment.CurrentDepartment.PK;

			var complianceSequenceCOM = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequenceCOM.XD_AllocationLevel = ComplianceBookAllocationLevel.Company;
			complianceSequenceCOM.XD_GC_Company = currCompany;
			complianceSequenceCOM.XD_Code = "COM";
			complianceSequenceCOM.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequenceCOM.XD_Prefix = "abc-001";
			complianceSequenceCOM.XD_StartNumber = 1;
			complianceSequenceCOM.XD_EndNumber = 99;
			complianceSequenceCOM.XD_MaximumNumberDigits = 4;
			complianceSequenceCOM.XD_NextNumber = 25;
			complianceSequenceCOM.XD_IsActive = true;
			complianceSequenceCOM.XD_StartDate = firstDay;
			complianceSequenceCOM.XD_ExpiryDate = lastDay;

			var complianceSequenceBRN = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequenceBRN.XD_AllocationLevel = ComplianceBookAllocationLevel.Branch;
			complianceSequenceBRN.XD_GC_Company = currCompany;
			complianceSequenceBRN.XD_GB_BranchOwner = currBranch;
			complianceSequenceBRN.XD_Code = "BRN";
			complianceSequenceBRN.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequenceBRN.XD_Prefix = "abc-001";
			complianceSequenceBRN.XD_StartNumber = 100;
			complianceSequenceBRN.XD_EndNumber = 199;
			complianceSequenceBRN.XD_MaximumNumberDigits = 4;
			complianceSequenceBRN.XD_NextNumber = 25;
			complianceSequenceBRN.XD_IsActive = true;
			complianceSequenceBRN.XD_StartDate = firstDay;
			complianceSequenceBRN.XD_ExpiryDate = lastDay;

			var complianceSequenceBDP = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequenceBDP.XD_AllocationLevel = ComplianceBookAllocationLevel.BranchDepartment;
			complianceSequenceBDP.XD_GC_Company = currCompany;
			complianceSequenceBDP.XD_GB_BranchOwner = currBranch;
			complianceSequenceBDP.XD_GE_Department = currDept;
			complianceSequenceBDP.XD_Code = "BDP";
			complianceSequenceBDP.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequenceBDP.XD_Prefix = "abc-001";
			complianceSequenceBDP.XD_StartNumber = 200;
			complianceSequenceBDP.XD_EndNumber = 299;
			complianceSequenceBDP.XD_MaximumNumberDigits = 4;
			complianceSequenceBDP.XD_NextNumber = 25;
			complianceSequenceBDP.XD_IsActive = true;
			complianceSequenceBDP.XD_StartDate = firstDay;
			complianceSequenceBDP.XD_ExpiryDate = lastDay;

			var complianceSequenceCTR = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequenceCTR.XD_AllocationLevel = ComplianceBookAllocationLevel.Counter;
			complianceSequenceCTR.XD_GC_Company = currCompany;
			complianceSequenceCTR.XD_LockBy = GlbStaff.CurrentUser.PK;
			complianceSequenceCTR.XD_Code = "CTR";
			complianceSequenceCTR.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequenceCTR.XD_Prefix = "abc-001";
			complianceSequenceCTR.XD_StartNumber = 300;
			complianceSequenceCTR.XD_EndNumber = 399;
			complianceSequenceCTR.XD_MaximumNumberDigits = 4;
			complianceSequenceCTR.XD_NextNumber = 25;
			complianceSequenceCTR.XD_IsActive = true;
			complianceSequenceCTR.XD_StartDate = firstDay;
			complianceSequenceCTR.XD_ExpiryDate = lastDay;
			Factory.Save();

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_GC = currCompany;
			header.AH_GB = currBranch;
			header.AH_GE = currDept;
			header.AH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			header.AH_TransactionReference = "";
			AssertEquals("should find CTR book", complianceSequenceCTR.PK, header.ComplianceSequenceFromSubType.PK);

			complianceSequenceCTR.XD_LockBy = ZGuid.Empty;
			Factory.Save();
			AssertEquals("should find BDP book", complianceSequenceBDP.PK, header.ComplianceSequenceFromSubType.PK);

			complianceSequenceBDP.XD_GE_Department = Factory.NewWithValidTestData<GlbDepartment>().PK;
			Factory.Save();
			AssertEquals("should find BRN book", complianceSequenceBRN.PK, header.ComplianceSequenceFromSubType.PK);

			complianceSequenceBRN.XD_GB_BranchOwner = Factory.NewWithValidTestData<GlbBranch>().PK;
			Factory.Save();
			AssertEquals("should find COM book", complianceSequenceCOM.PK, header.ComplianceSequenceFromSubType.PK);

			complianceSequenceCOM.XD_GC_Company = Factory.NewWithValidTestData<GlbCompany>().PK;
			Factory.Save();

			AssertEquals("should NOT find book", null, header.ComplianceSequenceFromSubType);

			complianceSequenceCOM.XD_GC_Company = currCompany;
			complianceSequenceCOM.XD_StartDate = today.AddDays(1);
			complianceSequenceCOM.XD_ExpiryDate = lastDay;
			Factory.Save();
			AssertEquals("should NOT find book", null, header.ComplianceSequenceFromSubType);
		}

		[TestDate(2018, 07, 03)]
		public void TestComplianceSequenceFromSubTypeFallbackToAllocationLevel()
		{
			var today = ZDate.Today;
			var firstDay = new ZDate(today.Year, 1, 1);
			var lastDay = new ZDate(today.Year, 12, 31);

			var currCompany = GlbCompany.CurrentCompany.PK;
			var currBranch = GlbBranch.CurrentBranch.PK;
			var currDept = GlbDepartment.CurrentDepartment.PK;

			var complianceSequenceCOM = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequenceCOM.XD_AllocationLevel = ComplianceBookAllocationLevel.Company;
			complianceSequenceCOM.XD_GC_Company = currCompany;
			complianceSequenceCOM.XD_Code = "COM";
			complianceSequenceCOM.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequenceCOM.XD_Prefix = "abc-001";
			complianceSequenceCOM.XD_StartNumber = 1;
			complianceSequenceCOM.XD_EndNumber = 99;
			complianceSequenceCOM.XD_MaximumNumberDigits = 4;
			complianceSequenceCOM.XD_NextNumber = 25;
			complianceSequenceCOM.XD_IsActive = true;
			complianceSequenceCOM.XD_StartDate = firstDay;
			complianceSequenceCOM.XD_ExpiryDate = lastDay;

			var complianceSequenceBRN = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequenceBRN.XD_AllocationLevel = ComplianceBookAllocationLevel.Branch;
			complianceSequenceBRN.XD_GC_Company = currCompany;
			complianceSequenceBRN.XD_GB_BranchOwner = currBranch;
			complianceSequenceBRN.XD_Code = "BRN";
			complianceSequenceBRN.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequenceBRN.XD_Prefix = "abc-001";
			complianceSequenceBRN.XD_StartNumber = 100;
			complianceSequenceBRN.XD_EndNumber = 199;
			complianceSequenceBRN.XD_MaximumNumberDigits = 4;
			complianceSequenceBRN.XD_NextNumber = 25;
			complianceSequenceBRN.XD_IsActive = true;
			complianceSequenceBRN.XD_StartDate = firstDay;
			complianceSequenceBRN.XD_ExpiryDate = lastDay;

			var complianceSequenceBDP = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequenceBDP.XD_AllocationLevel = ComplianceBookAllocationLevel.BranchDepartment;
			complianceSequenceBDP.XD_GC_Company = currCompany;
			complianceSequenceBDP.XD_GB_BranchOwner = currBranch;
			complianceSequenceBDP.XD_GE_Department = currDept;
			complianceSequenceBDP.XD_Code = "BDP";
			complianceSequenceBDP.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequenceBDP.XD_Prefix = "abc-001";
			complianceSequenceBDP.XD_StartNumber = 200;
			complianceSequenceBDP.XD_EndNumber = 299;
			complianceSequenceBDP.XD_MaximumNumberDigits = 4;
			complianceSequenceBDP.XD_NextNumber = 25;
			complianceSequenceBDP.XD_IsActive = true;
			complianceSequenceBDP.XD_StartDate = firstDay;
			complianceSequenceBDP.XD_ExpiryDate = lastDay;

			var complianceSequenceCTR = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequenceCTR.XD_AllocationLevel = ComplianceBookAllocationLevel.Counter;
			complianceSequenceCTR.XD_GC_Company = currCompany;
			complianceSequenceCTR.XD_LockBy = GlbStaff.CurrentUser.PK;
			complianceSequenceCTR.XD_Code = "CTR";
			complianceSequenceCTR.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequenceCTR.XD_Prefix = "abc-001";
			complianceSequenceCTR.XD_StartNumber = 300;
			complianceSequenceCTR.XD_EndNumber = 399;
			complianceSequenceCTR.XD_MaximumNumberDigits = 4;
			complianceSequenceCTR.XD_NextNumber = 25;
			complianceSequenceCTR.XD_IsActive = true;
			complianceSequenceCTR.XD_StartDate = firstDay;
			complianceSequenceCTR.XD_ExpiryDate = lastDay;
			Factory.Save();

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_GC = currCompany;
			header.AH_GB = currBranch;
			header.AH_GE = currDept;
			header.AH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			header.AH_TransactionReference = "";
			AssertEquals("should find CTR book", complianceSequenceCTR.PK, header.ComplianceSequenceFromSubType.PK);

			complianceSequenceCTR.XD_IsActive = false;
			Factory.Save();
			AssertEquals("should find BDP book", complianceSequenceBDP.PK, header.ComplianceSequenceFromSubType.PK);

			complianceSequenceBDP.XD_IsActive = false;
			Factory.Save();
			AssertEquals("should find BRN book", complianceSequenceBRN.PK, header.ComplianceSequenceFromSubType.PK);

			complianceSequenceBRN.XD_IsActive = false;
			Factory.Save();
			AssertEquals("should find COM book", complianceSequenceCOM.PK, header.ComplianceSequenceFromSubType.PK);

			complianceSequenceCOM.XD_IsActive = false;
			Factory.Save();
			AssertEquals("should NOT find book", null, header.ComplianceSequenceFromSubType);

			complianceSequenceCOM.XD_GC_Company = currCompany;
			complianceSequenceCOM.XD_StartDate = today.AddDays(1);
			complianceSequenceCOM.XD_ExpiryDate = lastDay;
			complianceSequenceCOM.XD_IsActive = true;
			Factory.Save();
			AssertEquals("should NOT find book", null, header.ComplianceSequenceFromSubType);
		}

		[TestDate(2012, 11, 11)]
		public void TestComplianceSequenceFromSubTypeFallbackToCompanyBook()
		{
			var today = ZDate.Today;

			var currCompany = GlbCompany.CurrentCompany.PK;
			var currBranch = GlbBranch.CurrentBranch.PK;

			var branchBook = Factory.NewWithValidTestData<AccComplianceSequence>();
			branchBook.XD_GC_Company = currCompany;
			branchBook.XD_GB_BranchOwner = currBranch;
			branchBook.XD_Code = "AAA";
			branchBook.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			branchBook.XD_Prefix = "abc-001";
			branchBook.XD_StartNumber = 1;
			branchBook.XD_EndNumber = 99;
			branchBook.XD_MaximumNumberDigits = 4;
			branchBook.XD_NextNumber = 25;
			branchBook.XD_IsActive = true;
			branchBook.XD_ExpiryDate = today.AddMonths(1).AddDays(1);
			Factory.Save();

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_GC = currCompany;
			header.AH_GB = currBranch;
			header.AH_ComplianceSubType = "TXI";
			header.AH_TransactionReference = "";
			AssertEquals("should find branch book", branchBook.PK, header.ComplianceSequenceFromSubType.PK);

			var companyBook = Factory.NewWithValidTestData<AccComplianceSequence>();
			companyBook.XD_AllocationLevel = ComplianceBookAllocationLevel.Company;
			companyBook.XD_GC_Company = currCompany;
			companyBook.XD_GB_BranchOwner = ZGuid.Empty;
			companyBook.XD_Code = "BBB";
			companyBook.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			companyBook.XD_Prefix = "XYZ-123";
			companyBook.XD_StartNumber = 1;
			companyBook.XD_EndNumber = 99;
			companyBook.XD_MaximumNumberDigits = 4;
			companyBook.XD_NextNumber = 25;
			companyBook.XD_IsActive = true;
			companyBook.XD_ExpiryDate = today.AddYears(2).AddMonths(1).AddDays(1);
			Factory.Save();

			AssertEquals("should find branch book", branchBook.PK, header.ComplianceSequenceFromSubType.PK);

			branchBook.XD_IsActive = false;
			Factory.Save();
			AssertEquals("should find company book", companyBook.PK, header.ComplianceSequenceFromSubType.PK);
		}

		[TestDate(2018, 07, 03)]
		public void TestComplianceSequenceFromSubType_ComplianceDocumentNumberAllocationRule_Receivables()
		{
			var today = ZDate.Today;
			var firstDay = new ZDate(today.Year, 1, 1);
			var lastDay = new ZDate(today.Year, 12, 31);

			var currCompany = GlbCompany.CurrentCompany.PK;
			var currBranch = GlbBranch.CurrentBranch.PK;
			var currDept = GlbDepartment.CurrentDepartment.PK;

			var nonCurrentBranchFilter = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
			nonCurrentBranchFilter.AddToFilter(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, currBranch);
			var nonCurrentBranch = Factory.LoadTop1<GlbBranch>(nonCurrentBranchFilter);

			var nonCurrentDepartmentFilter = new ZQuery(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, currDept);
			nonCurrentDepartmentFilter.AddToFilter(GlbDepartmentSchema.GE_IsValid, true);
			nonCurrentDepartmentFilter.AddToFilter(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, currDept);
			var nonCurrentDepartment = Factory.LoadTop1<GlbDepartment>(nonCurrentDepartmentFilter);

			var complianceSequenceCOM = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequenceCOM.XD_AllocationLevel = ComplianceBookAllocationLevel.Company;
			complianceSequenceCOM.XD_GC_Company = currCompany;
			complianceSequenceCOM.XD_GB_BranchOwner = currBranch;
			complianceSequenceCOM.XD_GE_Department = currDept;
			complianceSequenceCOM.XD_Code = "COM";
			complianceSequenceCOM.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequenceCOM.XD_Prefix = "abc-001";
			complianceSequenceCOM.XD_StartNumber = 1;
			complianceSequenceCOM.XD_EndNumber = 99;
			complianceSequenceCOM.XD_MaximumNumberDigits = 4;
			complianceSequenceCOM.XD_NextNumber = 25;
			complianceSequenceCOM.XD_IsActive = true;
			complianceSequenceCOM.XD_StartDate = firstDay;
			complianceSequenceCOM.XD_ExpiryDate = lastDay;

			var complianceSequenceBRN = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequenceBRN.XD_AllocationLevel = ComplianceBookAllocationLevel.Branch;
			complianceSequenceBRN.XD_GC_Company = currCompany;
			complianceSequenceBRN.XD_GB_BranchOwner = nonCurrentBranch.PK;
			complianceSequenceBRN.XD_GE_Department = nonCurrentDepartment.PK;
			complianceSequenceBRN.XD_Code = "BRN";
			complianceSequenceBRN.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequenceBRN.XD_Prefix = "abc-001";
			complianceSequenceBRN.XD_StartNumber = 100;
			complianceSequenceBRN.XD_EndNumber = 199;
			complianceSequenceBRN.XD_MaximumNumberDigits = 4;
			complianceSequenceBRN.XD_NextNumber = 25;
			complianceSequenceBRN.XD_IsActive = true;
			complianceSequenceBRN.XD_StartDate = firstDay;
			complianceSequenceBRN.XD_ExpiryDate = lastDay;
			Factory.Save();

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_GC = currCompany;
			header.AH_GB = nonCurrentBranch.PK;
			header.AH_GE = nonCurrentDepartment.PK;
			header.AH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			header.AH_TransactionReference = "";
			AssertEquals("should find COM book", complianceSequenceCOM.PK, header.ComplianceSequenceFromSubType.PK);

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(currCompany.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationRuleReceivables.SetTemporaryValue(currCompany.ToGuid(), Guid.Empty, Guid.Empty, ComplianceDocumentNumberAllocationRuleTypes.HBD.Code))
				{
					AssertEquals("should find BRN book", complianceSequenceBRN.PK, header.ComplianceSequenceFromSubType.PK);
				}

				using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationRuleReceivables.SetTemporaryValue(currCompany.ToGuid(), Guid.Empty, Guid.Empty, ComplianceDocumentNumberAllocationRuleTypes.LBD.Code))
				{
					AssertEquals("should find COM book", complianceSequenceCOM.PK, header.ComplianceSequenceFromSubType.PK);
				}
			}

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(currCompany.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationRuleReceivables.SetTemporaryValue(currCompany.ToGuid(), Guid.Empty, Guid.Empty, ComplianceDocumentNumberAllocationRuleTypes.HBD.Code))
			{
				AssertEquals("should find COM book", complianceSequenceCOM.PK, header.ComplianceSequenceFromSubType.PK);
			}
		}

		[TestDate(2018, 07, 03)]
		public void TestComplianceSequenceFromSubType_ComplianceDocumentNumberAllocationRule_Payables()
		{
			var today = ZDate.Today;
			var firstDay = new ZDate(today.Year, 1, 1);
			var lastDay = new ZDate(today.Year, 12, 31);

			var currCompany = GlbCompany.CurrentCompany.PK;
			var currBranch = GlbBranch.CurrentBranch.PK;
			var currDept = GlbDepartment.CurrentDepartment.PK;

			var nonCurrentBranchFilter = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
			nonCurrentBranchFilter.AddToFilter(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, currBranch);
			var nonCurrentBranch = Factory.LoadTop1<GlbBranch>(nonCurrentBranchFilter);

			var nonCurrentDepartmentFilter = new ZQuery(GlbDepartmentSchema.GE_IsActive, true);
			nonCurrentDepartmentFilter.AddToFilter(GlbDepartmentSchema.GE_IsValid, true);
			nonCurrentDepartmentFilter.AddToFilter(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, currDept);
			var nonCurrentDepartment = Factory.LoadTop1<GlbDepartment>(nonCurrentDepartmentFilter);

			var complianceSequenceCOM = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequenceCOM.XD_AllocationLevel = ComplianceBookAllocationLevel.Company;
			complianceSequenceCOM.XD_GC_Company = currCompany;
			complianceSequenceCOM.XD_GB_BranchOwner = currBranch;
			complianceSequenceCOM.XD_GE_Department = currDept;
			complianceSequenceCOM.XD_Code = "COM";
			complianceSequenceCOM.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequenceCOM.XD_Prefix = "abc-001";
			complianceSequenceCOM.XD_StartNumber = 1;
			complianceSequenceCOM.XD_EndNumber = 99;
			complianceSequenceCOM.XD_MaximumNumberDigits = 4;
			complianceSequenceCOM.XD_NextNumber = 25;
			complianceSequenceCOM.XD_IsActive = true;
			complianceSequenceCOM.XD_StartDate = firstDay;
			complianceSequenceCOM.XD_ExpiryDate = lastDay;

			var complianceSequenceBRN = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequenceBRN.XD_AllocationLevel = ComplianceBookAllocationLevel.Branch;
			complianceSequenceBRN.XD_GC_Company = currCompany;
			complianceSequenceBRN.XD_GB_BranchOwner = nonCurrentBranch.PK;
			complianceSequenceBRN.XD_GE_Department = nonCurrentDepartment.PK;
			complianceSequenceBRN.XD_Code = "BRN";
			complianceSequenceBRN.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequenceBRN.XD_Prefix = "abc-001";
			complianceSequenceBRN.XD_StartNumber = 100;
			complianceSequenceBRN.XD_EndNumber = 199;
			complianceSequenceBRN.XD_MaximumNumberDigits = 4;
			complianceSequenceBRN.XD_NextNumber = 25;
			complianceSequenceBRN.XD_IsActive = true;
			complianceSequenceBRN.XD_StartDate = firstDay;
			complianceSequenceBRN.XD_ExpiryDate = lastDay;
			Factory.Save();

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_GC = currCompany;
			header.AH_GB = nonCurrentBranch.PK;
			header.AH_GE = nonCurrentDepartment.PK;
			header.AH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			header.AH_TransactionReference = "";
			AssertEquals("should find COM book", complianceSequenceCOM.PK, header.ComplianceSequenceFromSubType.PK);

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(currCompany.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationRulePayables.SetTemporaryValue(currCompany.ToGuid(), Guid.Empty, Guid.Empty, ComplianceDocumentNumberAllocationRuleTypes.HBD.Code))
				{
					AssertEquals("should find BRN book", complianceSequenceBRN.PK, header.ComplianceSequenceFromSubType.PK);
				}

				using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationRulePayables.SetTemporaryValue(currCompany.ToGuid(), Guid.Empty, Guid.Empty, ComplianceDocumentNumberAllocationRuleTypes.LBD.Code))
				{
					AssertEquals("should find COM book", complianceSequenceCOM.PK, header.ComplianceSequenceFromSubType.PK);
				}
			}

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(currCompany.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationRulePayables.SetTemporaryValue(currCompany.ToGuid(), Guid.Empty, Guid.Empty, ComplianceDocumentNumberAllocationRuleTypes.HBD.Code))
			{
				AssertEquals("should find COM book", complianceSequenceCOM.PK, header.ComplianceSequenceFromSubType.PK);
			}
		}

		public void TestComplianceSequenceFromSubType_IsTaxBranchApplicable()
		{
			var today = ZDate.Today;
			var firstDay = new ZDate(today.Year, 1, 1);
			var lastDay = new ZDate(today.Year, 12, 31);

			var currCompany = GlbCompany.CurrentCompany.PK;
			var currBranch = GlbBranch.CurrentBranch.PK;
			var currDept = GlbDepartment.CurrentDepartment.PK;

			var nonCurrentBranchFilter = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
			nonCurrentBranchFilter.AddToFilter(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, currBranch);
			var nonCurrentBranch = Factory.LoadTop1<GlbBranch>(nonCurrentBranchFilter);

			var nonCurrentDepartmentFilter = new ZQuery(GlbDepartmentSchema.GE_IsActive, true);
			nonCurrentDepartmentFilter.AddToFilter(GlbDepartmentSchema.GE_IsValid, true);
			nonCurrentDepartmentFilter.AddToFilter(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, currDept);
			var nonCurrentDepartment = Factory.LoadTop1<GlbDepartment>(nonCurrentDepartmentFilter);

			var taxBranch = Factory.NewWithValidTestData<GlbBranch>();
			taxBranch.GB_GC = currCompany;

			var complianceSequenceCOM = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequenceCOM.XD_AllocationLevel = ComplianceBookAllocationLevel.Company;
			complianceSequenceCOM.XD_GC_Company = currCompany;
			complianceSequenceCOM.XD_GB_BranchOwner = currBranch;
			complianceSequenceCOM.XD_GE_Department = currDept;
			complianceSequenceCOM.XD_Code = "COM";
			complianceSequenceCOM.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequenceCOM.XD_Prefix = "abc-001";
			complianceSequenceCOM.XD_StartNumber = 1;
			complianceSequenceCOM.XD_EndNumber = 99;
			complianceSequenceCOM.XD_MaximumNumberDigits = 4;
			complianceSequenceCOM.XD_NextNumber = 25;
			complianceSequenceCOM.XD_IsActive = true;
			complianceSequenceCOM.XD_StartDate = firstDay;
			complianceSequenceCOM.XD_ExpiryDate = lastDay;

			var complianceSequenceBRN = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequenceBRN.XD_AllocationLevel = ComplianceBookAllocationLevel.Branch;
			complianceSequenceBRN.XD_GC_Company = currCompany;
			complianceSequenceBRN.XD_GB_BranchOwner = nonCurrentBranch.PK;
			complianceSequenceBRN.XD_GE_Department = nonCurrentDepartment.PK;
			complianceSequenceBRN.XD_Code = "BRN";
			complianceSequenceBRN.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequenceBRN.XD_Prefix = "abc-001";
			complianceSequenceBRN.XD_StartNumber = 100;
			complianceSequenceBRN.XD_EndNumber = 199;
			complianceSequenceBRN.XD_MaximumNumberDigits = 4;
			complianceSequenceBRN.XD_NextNumber = 25;
			complianceSequenceBRN.XD_IsActive = true;
			complianceSequenceBRN.XD_StartDate = firstDay;
			complianceSequenceBRN.XD_ExpiryDate = lastDay;

			var complianceSequenceTST = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequenceTST.XD_AllocationLevel = ComplianceBookAllocationLevel.Branch;
			complianceSequenceTST.XD_GC_Company = currCompany;
			complianceSequenceTST.XD_GB_BranchOwner = taxBranch.PK;
			complianceSequenceTST.XD_Code = "TST";
			complianceSequenceTST.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequenceTST.XD_Prefix = "abc-001";
			complianceSequenceTST.XD_StartNumber = 200;
			complianceSequenceTST.XD_EndNumber = 299;
			complianceSequenceTST.XD_MaximumNumberDigits = 4;
			complianceSequenceTST.XD_NextNumber = 25;
			complianceSequenceTST.XD_IsActive = true;
			complianceSequenceTST.XD_StartDate = firstDay;
			complianceSequenceTST.XD_ExpiryDate = lastDay;
			Factory.Save();

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			header.AH_OH = org.PK;
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_GC = currCompany;
			header.AH_GB = nonCurrentBranch.PK;
			header.AH_GE = nonCurrentDepartment.PK;
			header.AH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			header.AH_TransactionReference = "";
			header.AH_GB_TaxBranch = taxBranch.PK;
			AssertEquals("should find COM book", complianceSequenceCOM.PK, header.ComplianceSequenceFromSubType.PK);

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(currCompany.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationRuleReceivables.SetTemporaryValue(currCompany.ToGuid(), Guid.Empty, Guid.Empty, ComplianceDocumentNumberAllocationRuleTypes.HBD.Code))
				{
					AssertEquals("This AccTransactionHeader is not TaxBranchApplicable", false, AccountingMasterFilesUtils.IsTaxBranchApplicableForTransaction(header));
					AssertEquals("When AccTransactionHeader is not TaxBranchApplicable, we should find BRN book", complianceSequenceBRN.PK, header.ComplianceSequenceFromSubType.PK);

					using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(currCompany.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
						header.Header.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;

						AssertEquals("This AccTransactionHeader is TaxBranchApplicable", true, AccountingMasterFilesUtils.IsTaxBranchApplicableForTransaction(header));
						AssertEquals("When AccTransactionHeader is TaxBranchApplicable, we should find TST book", complianceSequenceTST.PK, header.ComplianceSequenceFromSubType.PK);
					}
				}

				using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationRuleReceivables.SetTemporaryValue(currCompany.ToGuid(), Guid.Empty, Guid.Empty, ComplianceDocumentNumberAllocationRuleTypes.LBD.Code))
				{
					AssertEquals("ComplianceDocumentNumberAllocationRuleReceivables is LBD", ComplianceDocumentNumberAllocationRuleTypes.LBD.Code, AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationRuleReceivables.Value);
					AssertEquals("When ComplianceDocumentNumberAllocationRuleTypes is LBD, we should find COM book", complianceSequenceCOM.PK, header.ComplianceSequenceFromSubType.PK);
				}
			}

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(currCompany.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationRuleReceivables.SetTemporaryValue(currCompany.ToGuid(), Guid.Empty, Guid.Empty, ComplianceDocumentNumberAllocationRuleTypes.HBD.Code))
			{
				AssertEquals("EnableComplianceDocumentModule is true", true, AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value);
				AssertEquals("When EnableComplianceDocumentModule is true, we should find COM book", complianceSequenceCOM.PK, header.ComplianceSequenceFromSubType.PK);
			}
		}

		public void TestComplianceSequenceFromTransactionReferenceFindParentBook()
		{
			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence.XD_SequenceClass = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequence.XD_Prefix = "abc-001";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			Factory.Save();

			var collection = new ComplianceSubTypeDependencyConfigurationCollection();
			var item = collection.AddNew();
			item.Country = CountryCodes.Peru;
			item.ChildSubType = PeruComplianceInfo.ComplianceSubTypeCodes.CAE;
			item.ParentSubType = PeruComplianceInfo.ComplianceSubTypeCodes.DSB;
			item = collection.AddNew();
			item.Country = CountryCodes.Peru;
			item.ChildSubType = PeruComplianceInfo.ComplianceSubTypeCodes.DSB;
			item.ParentSubType = PeruComplianceInfo.ComplianceSubTypeCodes.HON;
			item = collection.AddNew();
			item.Country = CountryCodes.Peru;
			item.ChildSubType = PeruComplianceInfo.ComplianceSubTypeCodes.HON;
			item.ParentSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;

			using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeDependencyConfiguration.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Peru))
			{
				var header = Factory.NewWithValidTestData<AccTransactionHeader>();
				header.AH_GC = GlbCompany.CurrentCompany.PK;
				header.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.CAE;
				header.AH_TransactionReference = "abc-0010001";
				AssertEquals("should find sequence", complianceSequence, header.ComplianceSequenceFromTransactionReference);

				header.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.DSB;
				header.AH_TransactionReference = "abc-0010099";
				AssertEquals("should find sequence", complianceSequence, header.ComplianceSequenceFromTransactionReference);

				header.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.HON;
				header.AH_TransactionReference = "abc-0010098";
				AssertEquals("should find sequence", complianceSequence, header.ComplianceSequenceFromTransactionReference);
			}
		}

		[TestDate(2024, 5, 11)]
		public void TestComplianceSequenceFromSubType_ComplianceNumberAllocationDate()
		{
			var currentCompany = GlbCompany.CurrentCompany;

			var sequenceForInvoiceDate = CreateComplianceSequence("abc-001", 1 , 99, new ZDate(2024, 4, 1), new ZDate(2024, 4, 30));
			var sequenceForTodayDate = CreateComplianceSequence("abc-002", 100 , 199, new ZDate(2024, 5, 1), new ZDate(2024, 5, 31));
			var sequenceForPostDate = CreateComplianceSequence("abc-003", 200 , 299, new ZDate(2024, 6, 1), new ZDate(2024, 6, 30));
			Factory.Save();

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_GC = currentCompany.PK;
			header.AH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			header.AH_TransactionReference = "";
			header.AH_PostDate = new ZDate(2024, 6, 12);
			header.AH_InvoiceDate = new ZDate(2024, 4, 13);

			AssertResult("NOT registry should use Today's date", LedgerTypes.AccountsReceivable, ComplianceNumberAllocationDateOptions.NoControl.Code, ComplianceNumberAllocationDateOptions.PostDate.Code, sequenceForTodayDate.PK);
			AssertResult("NOT registry should use Today's date", LedgerTypes.AccountsPayable, ComplianceNumberAllocationDateOptions.PostDate.Code, ComplianceNumberAllocationDateOptions.NoControl.Code, sequenceForTodayDate.PK);
			AssertResult("when header ledger is AR it should use AR registry to find correct compliance sequence", LedgerTypes.AccountsReceivable, ComplianceNumberAllocationDateOptions.InvoiceDate.Code, ComplianceNumberAllocationDateOptions.PostDate.Code, sequenceForInvoiceDate.PK);
			AssertResult("when header ledger is AP it should use AP registry to find correct compliance sequence", LedgerTypes.AccountsPayable, ComplianceNumberAllocationDateOptions.InvoiceDate.Code, ComplianceNumberAllocationDateOptions.PostDate.Code, sequenceForPostDate.PK);

			void AssertResult(string message, string ledger, string registryARValue, string registryAPValue, ZGuid expectedComplianceSequencePK)
			{
				header.AH_Ledger = ledger;

				using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryARValue))
				using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AP.SetTemporaryValue(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryAPValue))
				{
					AssertEquals(message, expectedComplianceSequencePK, header.ComplianceSequenceFromSubType.PK);
				}
			}

			AccComplianceSequence CreateComplianceSequence(string prefix, int startNumber, int endNumber, ZDate startDate, ZDate expiryDate)
			{
				var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				complianceSequence.XD_GC_Company = currentCompany.PK;
				complianceSequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
				complianceSequence.XD_Prefix = prefix;
				complianceSequence.XD_StartNumber = startNumber;
				complianceSequence.XD_EndNumber = endNumber;
				complianceSequence.XD_MaximumNumberDigits = 4;
				complianceSequence.XD_NextNumber = 25;
				complianceSequence.XD_StartDate = startDate;
				complianceSequence.XD_ExpiryDate = expiryDate;

				return complianceSequence;
			}
		}

		public void TestComplianceSequenceFromTransactionReference()
		{
			var currCompany = GlbCompany.CurrentCompany.PK;

			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = currCompany;
			complianceSequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequence.XD_Prefix = "abc-001";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			Factory.Save();

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_GC = currCompany;
			header.AH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			header.AH_TransactionReference = "abc-0010001";
			AssertEquals("should find sequence", complianceSequence, header.ComplianceSequenceFromTransactionReference);

			header.AH_TransactionReference = "abc-0010099";
			AssertEquals("should find sequence", complianceSequence, header.ComplianceSequenceFromTransactionReference);

			header.AH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR;
			AssertEquals("should NOT find sequence due to subtype", null, header.ComplianceSequenceFromTransactionReference);
			header.AH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;

			header.AH_TransactionReference = "abc-00100099";
			AssertEquals("should NOT find sequence due to total length", null, header.ComplianceSequenceFromTransactionReference);

			header.AH_TransactionReference = "abc-0010999";
			AssertEquals("should NOT find sequence due to number outside range", null, header.ComplianceSequenceFromTransactionReference);

			header.AH_TransactionReference = "xyz-0010050";
			AssertEquals("should NOT find sequence due to prefix", null, header.ComplianceSequenceFromTransactionReference);

			header.AH_TransactionReference = "abc001xyz001";
			AssertEquals("should NOT find sequence", null, header.ComplianceSequenceFromTransactionReference);

			try
			{
				header.AH_TransactionReference = "abcdefg";
				var sequence = header.ComplianceSequenceFromTransactionReference;
				Fail("Should not reach this line as exception should be thrown already");
			}
			catch (FailedToFindSequenceDueToReferenceNotEndWithNumberException ex)
			{
				AssertEquals(@$"Could not identify the original Compliance Invoice Book.
 {Core.Constants.ProductName} could not identify the Compliance Invoice Book as the transaction reference does not end with a valid number.", ex.UserFriendlyMessage);
			}

			try
			{
				header.AH_TransactionReference = "abc123xyz";
				var sequence = header.ComplianceSequenceFromTransactionReference;
				Fail("Should not reach this line as exception should be thrown already");
			}
			catch (FailedToFindSequenceDueToReferenceNotEndWithNumberException ex)
			{
				AssertEquals(@$"Could not identify the original Compliance Invoice Book.
 {Core.Constants.ProductName} could not identify the Compliance Invoice Book as the transaction reference does not end with a valid number.", ex.UserFriendlyMessage);
			}

			header.AH_TransactionReference = "123456";
			AssertEquals("should NOT find sequence", null, header.ComplianceSequenceFromTransactionReference);
		}

		public void TestComplianceSequenceFromTransactionReferenceWhenPrefixIsBlank()
		{
			var currCompany = GlbCompany.CurrentCompany.PK;

			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = currCompany;
			complianceSequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequence.XD_Prefix = "";
			complianceSequence.XD_StartNumber = 5;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			Factory.Save();

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_GC = currCompany;
			header.AH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			header.AH_TransactionReference = "0010005";
			AssertEquals("should not find sequence due to length", null, header.ComplianceSequenceFromTransactionReference);

			header.AH_TransactionReference = "0005";
			AssertEquals("should find sequence", complianceSequence, header.ComplianceSequenceFromTransactionReference);

			header.AH_TransactionReference = "0099";
			AssertEquals("should find sequence", complianceSequence, header.ComplianceSequenceFromTransactionReference);

			header.AH_TransactionReference = "99";
			AssertEquals("should not find sequence due to length", null, header.ComplianceSequenceFromTransactionReference);

			header.AH_TransactionReference = "0004";
			AssertEquals("should not find sequence due to outside range", null, header.ComplianceSequenceFromTransactionReference);

			header.AH_TransactionReference = "0100";
			AssertEquals("should not find sequence due to outside range", null, header.ComplianceSequenceFromTransactionReference);
		}

		public void TestComplianceSequenceFromTransactionReferenceLength()
		{
			var currCompany = GlbCompany.CurrentCompany.PK;

			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = currCompany;
			complianceSequence.XD_SequenceClass = "TXI";
			complianceSequence.XD_Prefix = "999999999999";
			complianceSequence.XD_StartNumber = 99999990;
			complianceSequence.XD_EndNumber = 99999999;
			complianceSequence.XD_NextNumber = 99999999;
			complianceSequence.XD_MaximumNumberDigits = 8;
			Factory.Save();

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_GC = currCompany;
			header.AH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			header.AH_TransactionReference = "99999999999999999999";
			AssertEquals("Should find sequence", complianceSequence, header.ComplianceSequenceFromTransactionReference);
		}

		[ExpectNoExceptions]
		public void TestComplianceSequenceFromTransactionReferenceNoExceptions()
		{
			var currCompany = GlbCompany.CurrentCompany.PK;

			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = currCompany;
			complianceSequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequence.XD_Prefix = "AAAAAAAAAAAA";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_NextNumber = 25;
			complianceSequence.XD_MaximumNumberDigits = 8;
			Factory.Save();

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_GC = currCompany;
			header.AH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			header.AH_TransactionReference = "AAAAAAAAAAAAAAAAAAA1";
			AssertEquals("Should find sequence", complianceSequence, header.ComplianceSequenceFromTransactionReference);
		}

		public void TestComplianceSequenceFromTransactionReferenceWhenPrefixNumericOnly()
		{
			var currCompany = GlbCompany.CurrentCompany.PK;

			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = currCompany;
			complianceSequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequence.XD_Prefix = "001";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			Factory.Save();

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_GC = currCompany;
			header.AH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			header.AH_TransactionReference = "0010001";
			AssertEquals("should find sequence", complianceSequence, header.ComplianceSequenceFromTransactionReference);

			header.AH_TransactionReference = "0010099";
			AssertEquals("should find sequence", complianceSequence, header.ComplianceSequenceFromTransactionReference);

			header.AH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR;
			AssertEquals("should NOT find sequence due to subtype", null, header.ComplianceSequenceFromTransactionReference);
			header.AH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;

			header.AH_TransactionReference = "00100099";
			AssertEquals("should NOT find sequence due to total length", null, header.ComplianceSequenceFromTransactionReference);

			header.AH_TransactionReference = "0010999";
			AssertEquals("should NOT find sequence due to number outside range", null, header.ComplianceSequenceFromTransactionReference);

			header.AH_TransactionReference = "0001005";
			AssertEquals("should NOT find sequence due to prefix", null, header.ComplianceSequenceFromTransactionReference);

			header.AH_TransactionReference = "0010050";
			AssertEquals("should find sequence", complianceSequence, header.ComplianceSequenceFromTransactionReference);

			header.AH_TransactionReference = "001xyz001";
			AssertEquals("should NOT find sequence", null, header.ComplianceSequenceFromTransactionReference);

			AssertExceptionWhenTransactionReferenceEndsWithLetter(header);
		}

		void AssertExceptionWhenTransactionReferenceEndsWithLetter(AccTransactionHeader header)
		{
			try
			{
				header.AH_TransactionReference = "abcdefg";
				var sequence = header.ComplianceSequenceFromTransactionReference;
				Fail("Should not reach this line as exception should be thrown already");
			}
			catch (FailedToFindSequenceDueToReferenceNotEndWithNumberException ex)
			{
				AssertEquals(@$"Could not identify the original Compliance Invoice Book.
 {Core.Constants.ProductName} could not identify the Compliance Invoice Book as the transaction reference does not end with a valid number.", ex.UserFriendlyMessage);
			}

			try
			{
				header.AH_TransactionReference = "abc123xyz";
				var sequence = header.ComplianceSequenceFromTransactionReference;
				Fail("Should not reach this line as exception should be thrown already");
			}
			catch (FailedToFindSequenceDueToReferenceNotEndWithNumberException ex)
			{
				AssertEquals(@$"Could not identify the original Compliance Invoice Book.
 {Core.Constants.ProductName} could not identify the Compliance Invoice Book as the transaction reference does not end with a valid number.", ex.UserFriendlyMessage);
			}
		}

		[ExpectNoExceptions]
		public void TestGetMockComplianceSequenceFromSubType()
		{
			var complianceSequenceRetrieverMock = new Mock<IComplianceSequenceRetriever>();

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();

			header.SubstituteComplianceSequenceRetriever_ForTestOnly(complianceSequenceRetrieverMock.Object);
			var headerComplianceSequence = header.ComplianceSequenceFromSubType;
			complianceSequenceRetrieverMock.Verify(x => x.GetComplianceSequenceFromSubType(It.IsAny<ZString>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZDateTime>()), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestGetComplianceSequenceFromTransactionReference()
		{
			var complianceSequenceRetrieverMock = new Mock<IComplianceSequenceRetriever>();

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();

			header.SubstituteComplianceSequenceRetriever_ForTestOnly(complianceSequenceRetrieverMock.Object);

			var headerComplianceSequence = header.ComplianceSequenceFromTransactionReference;
			complianceSequenceRetrieverMock.Verify(x => x.GetComplianceSequenceFromTransactionReference(header.AH_ComplianceSubType, header.AH_TransactionReference, header.AH_GC, It.IsAny<BusinessObjectFactory>()), Times.Once);
		}

		public void TestGetComplianceSequenceFromTransactionReference_Portugal()
		{
			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Portugal);

			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence.XD_SequenceClass = PortugalComplianceInfo.ComplianceSubTypeCodes.TXI;
			complianceSequence.XD_Prefix = "001";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			Factory.Save();

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_GC = GlbCompany.CurrentCompany.PK;
			header.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXI;
			header.AH_TransactionReference = "TXI 001/0001";
			AssertEquals("should find sequence", complianceSequence, header.ComplianceSequenceFromTransactionReference);

			header.AH_TransactionReference = "TXI 001/0099";
			AssertEquals("should find sequence", complianceSequence, header.ComplianceSequenceFromTransactionReference);

			header.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TCR;
			AssertEquals("should NOT find sequence due to subtype", null, header.ComplianceSequenceFromTransactionReference);
			header.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXI;

			header.AH_TransactionReference = "TXI 001/00099";
			AssertEquals("should NOT find sequence due to total length", null, header.ComplianceSequenceFromTransactionReference);

			header.AH_TransactionReference = "TXI 001/0999";
			AssertEquals("should NOT find sequence due to number outside range", null, header.ComplianceSequenceFromTransactionReference);

			header.AH_TransactionReference = "TXI 002/005";
			AssertEquals("should NOT find sequence due to prefix", null, header.ComplianceSequenceFromTransactionReference);

			header.AH_TransactionReference = "TXI 001/0050";
			AssertEquals("should find sequence", complianceSequence, header.ComplianceSequenceFromTransactionReference);

			header.AH_TransactionReference = "TXI 001/xyz001";
			AssertEquals("should NOT find sequence", null, header.ComplianceSequenceFromTransactionReference);

			AssertExceptionWhenTransactionReferenceEndsWithLetter(header);
		}

		public void TestGetSequenceNumber()
		{
			IComplianceSequenceRetriever complianceSequenceRetriever = new ComplianceSequenceRetriever();

			AssertEquals("000005", complianceSequenceRetriever.GetSequenceNumber(CountryCodes.Australia, "001000005", "001"));
			AssertEquals("000010", complianceSequenceRetriever.GetSequenceNumber(CountryCodes.Australia, "abc000010", "abc"));
			AssertEquals("100000", complianceSequenceRetriever.GetSequenceNumber(CountryCodes.Australia, "AU0100000", "AU0"));
			AssertEquals("00099", complianceSequenceRetriever.GetSequenceNumber(CountryCodes.Australia, "00AU00099", "00AU"));
			AssertEquals("000005", complianceSequenceRetriever.GetSequenceNumber("", "001000005", "001"));
			AssertEquals("001005", complianceSequenceRetriever.GetSequenceNumber(CountryCodes.Australia, "001005", ""));
			AssertEquals("", complianceSequenceRetriever.GetSequenceNumber(CountryCodes.Australia, "", ""));

			AssertEquals("000005", complianceSequenceRetriever.GetSequenceNumber(CountryCodes.Portugal, "TXI /000005", ""));
			AssertEquals("000085", complianceSequenceRetriever.GetSequenceNumber(CountryCodes.Portugal, "TCR Prefix/000085", ""));
			AssertEquals("000085", complianceSequenceRetriever.GetSequenceNumber(CountryCodes.Portugal, "TCR Prefix/000085", "NotUsed"));
			AssertEquals("990", complianceSequenceRetriever.GetSequenceNumber(CountryCodes.Portugal, "TCR 0258/990", ""));
			AssertEquals("0005230", complianceSequenceRetriever.GetSequenceNumber(CountryCodes.Portugal, "TCD PR8/0005230", ""));
			AssertEquals("", complianceSequenceRetriever.GetSequenceNumber(CountryCodes.Portugal, "", ""));
			AssertEquals("invalid transaction reference Portugal format fallback to default sequence number",
				"000005", complianceSequenceRetriever.GetSequenceNumber(CountryCodes.Portugal, "001000005", "001"));
		}
	}
}
