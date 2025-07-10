using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccComplianceSequenceForSplitValidationTest : AccComplianceSequenceValidationTest
	{
		public void TestXD_NewSequenceStartNumber()
		{
			newSequenceForSplit.Validation.ValidateAll();
			AssertEquals("Precondition", false, newSequenceForSplit.HasErrors);
			AssertEquals(true, newSequenceForSplit.IsNewBook);

			newSequenceForSplit.XD_StartNumber = existSequenceForSplit.XD_EndNumber - 1;
			newSequenceForSplit.Validation.ValidateXD_StartNumber();
			AssertEquals("Number series is not unique", true, newSequenceForSplit.XD_StartNumberInfo.HasError("Number series and prefix is not unique for the compliance sub type"));

			newSequenceForSplit.XD_Prefix = "XXX";
			AssertNotEquals(existSequenceForSplit.XD_Prefix, newSequenceForSplit.XD_Prefix);
			newSequenceForSplit.Validation.ValidateXD_StartNumber();
			AssertEquals("newSequence.XD_StartNumber must be bigger than existSequence.XD_EndNumber", true, newSequenceForSplit.XD_StartNumberInfo.HasError("Start number of new book must be bigger than end number of exist book."));

			newSequenceForSplit.IsNewBook = false;

			newSequenceForSplit.Validation.ValidateXD_StartNumber();
			AssertEquals(false, newSequenceForSplit.XD_StartNumberInfo.HasError("Start number of new book must be bigger than end number of exist book."));
		}

		public void TestXD_NewSequenceStartDate()
		{
			newSequenceForSplit.Validation.ValidateAll();
			AssertEquals("Precondition", false, newSequenceForSplit.HasErrors);
			AssertEquals(true, newSequenceForSplit.IsNewBook);

			newSequenceForSplit.XD_StartDate = existSequenceForSplit.XD_ExpiryDate.AddDays(-1).Date;
			newSequenceForSplit.Validation.ValidateXD_StartDate();
			AssertEquals("Sequence books overlapped", true, newSequenceForSplit.XD_StartDateInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));

			newSequenceForSplit.XD_AllocationLevel = ComplianceBookAllocationLevel.Branch;
			AssertNotEquals(existSequenceForSplit.XD_AllocationLevel, newSequenceForSplit.XD_AllocationLevel);
			newSequenceForSplit.XD_GB_BranchOwner = testBranch.PK;
			newSequenceForSplit.Validation.ValidateXD_StartDate();
			AssertEquals("newSequence.XD_StartDate must be bigger than existSequence.XD_ExpiryDate", true, newSequenceForSplit.XD_StartDateInfo.HasError("Start date of new book must be bigger than end date of exist book."));

			newSequenceForSplit.IsNewBook = false;

			newSequenceForSplit.Validation.ValidateXD_StartDate();
			AssertEquals(false, newSequenceForSplit.XD_StartDateInfo.HasError("Start date of new book must be bigger than end date of exist book."));
		}

		public void TestXD_EndNumberBoundry()
		{
			newSequenceForSplit.Validation.ValidateAll();
			AssertEquals("Precondition", false, newSequenceForSplit.HasErrors);
			AssertEquals(true, newSequenceForSplit.IsNewBook);

			newSequenceForSplit.XD_EndNumber = newSequenceForSplit.OriginalEndNumber + 1;
			newSequenceForSplit.Validation.ValidateXD_EndNumber();
			AssertEquals("newSequence.XD_EndNumber cannot be bigger than origianl XD_EndNumber", true, newSequenceForSplit.XD_EndNumberInfo.HasError("End number of new book must not be bigger than original end number."));

			newSequenceForSplit.XD_EndNumber = newSequenceForSplit.OriginalEndNumber;
			newSequenceForSplit.Validation.ValidateXD_EndNumber();
			AssertEquals(false, newSequenceForSplit.HasErrors);

			existSequenceForSplit.Validation.ValidateAll();
			AssertEquals("Precondition", false, existSequenceForSplit.HasErrors);
			AssertEquals(false, existSequenceForSplit.IsNewBook);

			existSequenceForSplit.XD_EndNumber = existSequenceForSplit.OriginalEndNumber + 1;
			existSequenceForSplit.Validation.ValidateXD_EndNumber();
			AssertEquals("existSequence.XD_EndNumber should be smaller than origianl XD_EndNumber", true, existSequenceForSplit.XD_EndNumberInfo.HasError("End number of exist book must be smaller than original end number."));

			existSequenceForSplit.XD_EndNumber = existSequenceForSplit.OriginalEndNumber;
			existSequenceForSplit.Validation.ValidateXD_EndNumber();
			AssertEquals(true, existSequenceForSplit.XD_EndNumberInfo.HasError("End number of exist book must be smaller than original end number."));

			existSequenceForSplit.XD_EndNumber = existSequenceForSplit.OriginalEndNumber - 1;
			existSequenceForSplit.Validation.ValidateXD_EndNumber();
			AssertEquals(false, existSequenceForSplit.HasErrors);
		}

		public void TestXD_ExpiryDateBoundry()
		{
			newSequenceForSplit.Validation.ValidateAll();
			AssertEquals("Precondition", false, newSequenceForSplit.HasErrors);
			AssertEquals(true, newSequenceForSplit.IsNewBook);

			newSequenceForSplit.XD_ExpiryDate = newSequenceForSplit.OriginalExpiryDate.AddDays(1);
			newSequenceForSplit.Validation.ValidateXD_ExpiryDate();
			AssertEquals("newSequence.XD_ExpiryDate cannot be bigger than origianl XD_ExpiryDate", true, newSequenceForSplit.XD_ExpiryDateInfo.HasError("Expiry date of new book must not be bigger than original expiry date."));

			newSequenceForSplit.XD_ExpiryDate = newSequenceForSplit.OriginalExpiryDate;
			newSequenceForSplit.Validation.ValidateXD_ExpiryDate();
			AssertEquals(false, newSequenceForSplit.HasErrors);

			existSequenceForSplit.Validation.ValidateAll();
			AssertEquals("Precondition", false, existSequenceForSplit.HasErrors);
			AssertEquals(false, existSequenceForSplit.IsNewBook);

			existSequenceForSplit.XD_ExpiryDate = existSequenceForSplit.OriginalExpiryDate.AddDays(1);
			existSequenceForSplit.Validation.ValidateXD_ExpiryDate();
			AssertEquals("newSequence.XD_ExpiryDate should be smaller than origianl XD_ExpiryDate", true, existSequenceForSplit.XD_ExpiryDateInfo.HasError("Expiry date of exist book must be smaller than original expiry date."));

			existSequenceForSplit.XD_ExpiryDate = existSequenceForSplit.OriginalExpiryDate;
			existSequenceForSplit.Validation.ValidateXD_ExpiryDate();
			AssertEquals(true, existSequenceForSplit.XD_ExpiryDateInfo.HasError("Expiry date of exist book must be smaller than original expiry date."));

			existSequenceForSplit.XD_ExpiryDate = existSequenceForSplit.OriginalExpiryDate.AddDays(-1);
			existSequenceForSplit.Validation.ValidateXD_ExpiryDate();
			AssertEquals(false, existSequenceForSplit.HasErrors);

			existSequenceForSplit.XD_AllocationLevel = ComplianceBookAllocationLevel.Company;
			existSequenceForSplit.XD_StartDate = ZDate.Empty;
			existSequenceForSplit.XD_ExpiryDate = ZDate.Empty;
			newSequenceForSplit.XD_AllocationLevel = ComplianceBookAllocationLevel.Branch;
			newSequenceForSplit.XD_GB_BranchOwner = testBranch.PK;
			newSequenceForSplit.XD_StartDate = ZDate.Empty;
			newSequenceForSplit.XD_ExpiryDate = ZDate.Empty;
			existSequenceForSplit.Validation.ValidateXD_StartDate();
			existSequenceForSplit.Validation.ValidateXD_ExpiryDate();
			newSequenceForSplit.Validation.ValidateXD_StartDate();
			newSequenceForSplit.Validation.ValidateXD_ExpiryDate();
			AssertEquals("Allow exist / new sequences have empty start date / expiry date.", false, existSequenceForSplit.HasErrors);
		}

		public void TestCheckXD_StartNumber()
		{
			newSequenceForSplit.Validation.ValidateAll();
			AssertEquals("Precondition", false, newSequenceForSplit.HasErrors);
			AssertEquals(true, newSequenceForSplit.IsNewBook);

			newSequenceForSplit.XD_StartNumber = existSequenceForSplit.XD_NextNumber - 1;
			newSequenceForSplit.Validation.ValidateXD_StartNumber();
			AssertEquals("Number series is not unique", true, newSequenceForSplit.XD_StartNumberInfo.HasError("Number series and prefix is not unique for the compliance sub type"));

			newSequenceForSplit.XD_Prefix = "TXY";
			AssertNotEquals(existSequenceForSplit.XD_Prefix, newSequenceForSplit.XD_Prefix);
			newSequenceForSplit.Validation.ValidateXD_StartNumber();
			AssertEquals("newSequence.XD_StartNumber must not be smaller than existSequence.XD_NextNumber", true, newSequenceForSplit.XD_StartNumberInfo.HasError("Start number of new book must not be smaller than next number of exist book."));

			newSequenceForSplit.IsNewBook = false;

			newSequenceForSplit.Validation.ValidateXD_StartNumber();
			AssertEquals(false, newSequenceForSplit.XD_StartNumberInfo.HasError("Start number of new book must not be smaller than next number of exist book."));
		}

		#region Overrides

		protected override void SetUp()
		{
			base.SetUp();

			// Create test data in a different company. So that it would not conflict with those in base setup.
			testCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK) { OrderBy = GlbCompanySchema.GC_Code.Name });
			testBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, testCompany.PK) { OrderBy = GlbBranchSchema.GB_Code.Name });

			var controller = new DummyComplianceSequencSplitController();
			var tmpFactory = Factory.CreateNewFactory();

			var existedSequence = tmpFactory.NewWithValidTestData<AccComplianceSequence>();
			existedSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Company;
			existedSequence.XD_SequenceClass = "TXI";
			existedSequence.XD_Code = "LM7";
			existedSequence.XD_StartNumber = 100;
			existedSequence.XD_EndNumber = 300;
			existedSequence.XD_MaximumNumberDigits = 6;
			existedSequence.XD_GC_Company = testCompany.PK;
			existedSequence.XD_Description = "LM7 DESC";
			existedSequence.XD_ExpiryDate = new ZDateTime(2018, 3, 20);
			existedSequence.XD_NextNumber = existedSequence.XD_StartNumber + 1;
			existedSequence.XD_Prefix = "X";

			tmpFactory.Save();

			existSequenceForSplit = Factory.Load<AccComplianceSequenceForSplit>(existedSequence.PK);
			existSequenceForSplit.IsNewBook = false;
			existSequenceForSplit.OriginalEndNumber = existSequenceForSplit.XD_EndNumber + 1;
			existSequenceForSplit.OriginalExpiryDate = existSequenceForSplit.XD_ExpiryDate.Date.AddDays(1);
			existSequenceForSplit.SplitController = controller;

			var newSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			newSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Company;
			newSequence.XD_SequenceClass = "TXI";
			newSequence.XD_Code = "LM6";
			newSequence.XD_StartNumber = 400;
			newSequence.XD_EndNumber = 600;
			newSequence.XD_MaximumNumberDigits = 6;
			newSequence.XD_GC_Company = testCompany.PK;
			newSequence.XD_Description = "LM6 DESC";
			newSequence.XD_StartDate = new ZDate(2018, 4, 20);
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 8, 20);
			newSequence.XD_NextNumber = newSequence.XD_StartNumber + 1;
			newSequence.XD_Prefix = "X";

			newSequenceForSplit = Factory.Load<AccComplianceSequenceForSplit>(newSequence.PK);
			newSequenceForSplit.IsNewBook = true;
			newSequenceForSplit.OriginalEndNumber = newSequence.XD_EndNumber;
			newSequenceForSplit.OriginalExpiryDate = newSequence.XD_ExpiryDate.Date;
			newSequenceForSplit.SplitController = controller;

			controller.ExistSequence = existSequenceForSplit;
			controller.NewSequence = newSequenceForSplit;
		}

		#endregion

		AccComplianceSequenceForSplit existSequenceForSplit, newSequenceForSplit;
		GlbCompany testCompany;
		GlbBranch testBranch;

		class DummyComplianceSequencSplitController : ISplitComplianceSequenceController
		{
			public AccComplianceSequenceForSplit ExistSequence { get; set; }

			public AccComplianceSequenceForSplit NewSequence { get; set; }

			public void CreateOnSavingEvent(bool isNewbook)
			{
			}

			public void UpdateNewSequenceStartDate()
			{
			}

			public void UpdateNewSequenceStartNumber()
			{
			}
		}
	}
}
