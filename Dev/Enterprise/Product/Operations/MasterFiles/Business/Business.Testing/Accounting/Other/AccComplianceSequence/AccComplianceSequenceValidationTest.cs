using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	class AccComplianceSequenceValidationTest : BusinessObjectValidationTestCase
	{
		protected BusinessObjectFactory TestFactory;
		protected AccComplianceSequence newSequence, existedSequence;

		protected override void SetUp()
		{
			base.SetUp();
			TestFactory = new BusinessObjectFactory();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Peru);

			existedSequence = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			existedSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Branch;
			existedSequence.XD_SequenceClass = "TXI";
			existedSequence.XD_Code = "LM5";
			existedSequence.XD_StartNumber = 200;
			existedSequence.XD_EndNumber = 300;
			existedSequence.XD_MaximumNumberDigits = 6;
			existedSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			existedSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			TestFactory.Save();

			newSequence = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			newSequence.XD_MaximumNumberDigits = 4;
		}

		public void TestSQLQueriesOfOverlapSeriesCheckMethodsAreNotRunWhenParentHasErrors()
		{
			newSequence.XD_StartNumber = 999999999999m;
			AssertHasError(newSequence.XD_StartNumberInfo, "The number 999,999,999,999 is too large, the maximum value allowed for Start Number is 999,999,999.");

			newSequence.XD_SequenceClass = "TXI";
			Assert(newSequence.HasErrors);
			Assert(!newSequence.XD_SequenceClass.IsEmpty);
			Assert(!newSequence.XD_GB_BranchOwner.IsEmpty);

			var dbHitsBefore = TestFactory.GetTableHitCount(AccComplianceSequenceSchema.Constants.TableName);
			AssertNoExceptionThrown(() => newSequence.XD_Code = "LM5");
			var dbHitsAfter = TestFactory.GetTableHitCount(AccComplianceSequenceSchema.Constants.TableName);
			AssertEquals("Should not be running SQL queries when parent has errors", 0, dbHitsAfter - dbHitsBefore);
		}

		public void TestCheckXD_Prefix_Portugal()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				Assert(!newSequence.IsAllocated);
				AssertPrefixHasError("abc/");
				AssertPrefixHasError("abcde-");
				AssertPrefixHasError("abcde/1");
				AssertPrefixHasError(" abcd");
				AssertPrefixHasError("abc de");

				AssertPrefixHasNoError("aBc2dE1");
				AssertPrefixHasNoError("123");
				AssertPrefixHasNoError("abcde");
				AssertPrefixHasNoError("ABCDE");

				newSequence.XD_StartNumber = 1;
				newSequence.XD_NextNumber = 20;
				newSequence.Factory.Save();

				Assert(newSequence.IsAllocated);
				AssertPrefixHasNoError("abc/");
			}

			newSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			Assert(!newSequence.IsAllocated);
			AssertPrefixHasNoError("abc/");

			void AssertPrefixHasError(string prefix)
			{
				newSequence.XD_Prefix = prefix;
				AssertHasError(newSequence.XD_PrefixInfo, "Series Prefix must contain alphanumeric characters only and no spaces");
			}

			void AssertPrefixHasNoError(string prefix)
			{
				newSequence.XD_Prefix = prefix;
				AssertNoErrors(newSequence.XD_PrefixInfo);
			}
		}

		public void TestValidateXD_SequenceClass()
		{
			newSequence.XD_SequenceClass = "";
			newSequence.Validation.ValidateXD_SequenceClass();
			Assert("compliance sequence must have a XD_SequenceClass", newSequence.XD_SequenceClassInfo.HasErrors());

			newSequence.XD_SequenceClass = "XXX";
			newSequence.Validation.ValidateXD_SequenceClass();
			Assert("compliance sequence must have a valid XD_SequenceClass", newSequence.XD_SequenceClassInfo.HasErrors());

			newSequence.XD_SequenceClass = "TXI";
			newSequence.XD_GB_BranchOwner = Factory.NewWithValidTestData<GlbBranch>().PK;
			newSequence.Validation.ValidateXD_SequenceClass();
			Assert(!newSequence.XD_SequenceClassInfo.HasErrors());

			newSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			newSequence.Validation.ValidateXD_SequenceClass();
			Assert(newSequence.XD_SequenceClassInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));

			ComplianceSubTypeDependencyConfigurationCollection collection = new ComplianceSubTypeDependencyConfigurationCollection();
			ComplianceSubTypeDependencyConfiguration item = collection.AddNew();
			item.Country = "PE";
			item.ChildSubType = "CAE";
			item.ParentSubType = "DSB";
			item = collection.AddNew();
			item.Country = "PE";
			item.ChildSubType = "DSB";
			item.ParentSubType = "HON";
			item = collection.AddNew();
			item.Country = "PE";
			item.ChildSubType = "HON";
			item.ParentSubType = "TBO";
			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeDependencyConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
			newSequence.XD_SequenceClass = "CAE";
			newSequence.Validation.ValidateXD_SequenceClass();
			Assert(newSequence.XD_SequenceClassInfo.HasError(@"Compliance Sequence Book cannot be created for sub type CAE. 
Transactions with sub type CAE will be assigned Compliance numbers from the TBO Compliance Invoice Book"));
			newSequence.XD_SequenceClass = "DSB";
			newSequence.Validation.ValidateXD_SequenceClass();
			Assert(newSequence.XD_SequenceClassInfo.HasError(@"Compliance Sequence Book cannot be created for sub type DSB. 
Transactions with sub type DSB will be assigned Compliance numbers from the TBO Compliance Invoice Book"));
			newSequence.XD_SequenceClass = "TBO";
			newSequence.Validation.ValidateXD_SequenceClass();
			Assert(!newSequence.XD_SequenceClassInfo.HasErrors());
		}

		public void TestValidateXD_Code()
		{
			newSequence.XD_Code = "";
			newSequence.Validation.ValidateXD_Code();
			Assert("compliance sequence must have a XD_Code", newSequence.XD_CodeInfo.HasErrors());

			newSequence.XD_Code = "LM1";
			newSequence.Validation.ValidateXD_Code();
			Assert(!newSequence.XD_CodeInfo.HasErrors());

			existedSequence.XD_Code = "LM1";
			newSequence.Validation.ValidateXD_Code();
			AssertEquals("Duplicate code is not allowed.", true, newSequence.XD_CodeInfo.GetErrors().ToList<INotification>().Exists(x => x.Message == "A book already has the same code in this company."));
		}

		public void TestValidateXD_Code_DoesNotCheckDuplicateWithinDifferentCompany()
		{
			newSequence.XD_Code = "LM1";
			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;

			existedSequence.XD_Code = "LM1";
			existedSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			existedSequence.Validation.ValidateXD_Code();
			AssertEquals("Duplicate code is not allowed.", true, existedSequence.XD_CodeInfo.GetErrors().ToList<INotification>().Exists(x => x.Message == "A book already has the same code in this company."));

			existedSequence.XD_GC_Company = ZGuid.NewZGuid();

			existedSequence.Validation.ValidateXD_Code();
			Assert("should not give errors as codes are belong to different companies", !existedSequence.XD_CodeInfo.HasErrors());
		}

		public void TestValidateXD_Description()
		{
			newSequence.XD_Description = "";
			newSequence.Validation.ValidateXD_Description();
			Assert("compliance sequence must have a XD_Description", newSequence.XD_DescriptionInfo.HasErrors());

			newSequence.XD_Description = "this is a test";
			newSequence.Validation.ValidateXD_Description();
			Assert(!newSequence.XD_DescriptionInfo.HasErrors());
		}

		public void TestValidateXD_NumberFormat()
		{
			var configurationCollection = ComplianceNumberSequenceConfigurationCollectionTest.GetConfigurationCollectionForTest(Factory);
			AccountingMasterFilesRegistry.Instance.ComplianceNumberSequenceConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurationCollection);
			Factory.Save();

			newSequence.XD_NumberFormat = ZString.Empty;
			newSequence.Validation.ValidateXD_NumberFormat();
			AssertHasError(newSequence.XD_NumberFormatInfo, "Please enter a value.");

			newSequence.XD_NumberFormat = "XX";
			newSequence.Validation.ValidateXD_NumberFormat();
			AssertHasError(newSequence.XD_NumberFormatInfo, "Enter a valid selection.");
			TestFactory.Save();

			newSequence.XD_NumberFormat = "DEF";
			newSequence.XD_StartNumber = 1;
			newSequence.XD_NextNumber = 20;
			newSequence.Validation.ValidateXD_NumberFormat();
			AssertHasError(newSequence.XD_NumberFormatInfo, "The Number Format cannot be changed as number sequence has been allocated from this compliance sequence book.");

			newSequence.XD_NextNumber = 1;
			newSequence.XD_Prefix = "123456";
			newSequence.XD_MaximumNumberDigits = 8;
			newSequence.XD_NumberFormat = "AAA";
			newSequence.Validation.ValidateXD_NumberFormat();
			AssertHasError(newSequence.XD_NumberFormatInfo, "This compliance invoice book cannot use the selected number format as the length of the compliance sequence number will exceed the total length of 20 characters.");

			newSequence.XD_SequenceClass = "TXI";
			newSequence.XD_NextNumber = 1;
			newSequence.XD_Prefix = "TEST";
			newSequence.XD_MaximumNumberDigits = 9;
			newSequence.XD_NumberFormat = "CCC";
			newSequence.Validation.ValidateXD_NumberFormat();
			AssertHasError(newSequence.XD_NumberFormatInfo, "This compliance invoice book cannot use the selected number format as the length of the compliance sequence number will exceed the total length of 20 characters.");

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Portugal);
			newSequence.XD_NextNumber = 1;
			newSequence.XD_Prefix = "AB";
			newSequence.XD_MaximumNumberDigits = 5;
			newSequence.XD_NumberFormat = "AAA";
			newSequence.Validation.ValidateXD_NumberFormat();
			AssertHasError(newSequence.XD_NumberFormatInfo, "The book must use the default mandatory number format: MPC");
		}

		public void TestValidateXD_AllocationLevel()
		{
			newSequence.XD_AllocationLevel = "SDG";
			newSequence.Validation.ValidateXD_AllocationLevel();
			Assert("invalid Allocation Level", newSequence.XD_AllocationLevelInfo.HasErrors());

			newSequence.XD_AllocationLevel = "";
			newSequence.Validation.ValidateXD_AllocationLevel();
			Assert("blank Allocation Level", newSequence.XD_AllocationLevelInfo.HasErrors());

			newSequence.XD_AllocationLevel = "COM";
			newSequence.Validation.ValidateXD_AllocationLevel();
			Assert(!newSequence.XD_AllocationLevelInfo.HasErrors());

			newSequence.XD_AllocationLevel = "BRN";
			newSequence.Validation.ValidateXD_AllocationLevel();
			Assert(!newSequence.XD_AllocationLevelInfo.HasErrors());

			newSequence.XD_AllocationLevel = "BDP";
			newSequence.Validation.ValidateXD_AllocationLevel();
			Assert(!newSequence.XD_AllocationLevelInfo.HasErrors());

			newSequence.XD_AllocationLevel = "CTR";
			newSequence.Validation.ValidateXD_AllocationLevel();
			Assert(!newSequence.XD_AllocationLevelInfo.HasErrors());
		}

		public void TestValidateXD_Prefix()
		{
			newSequence.XD_Prefix = ZString.Empty;
			newSequence.Validation.ValidateXD_Prefix();
			Assert(!newSequence.XD_PrefixInfo.HasErrors());

			newSequence.XD_Prefix = "abc";
			newSequence.Validation.ValidateXD_Prefix();
			Assert(!newSequence.XD_PrefixInfo.HasErrors());

			newSequence.XD_MaximumNumberDigits = 9;
			newSequence.XD_Prefix = "123456789012";
			newSequence.Validation.ValidateXD_Prefix();
			Assert(newSequence.XD_PrefixInfo.HasError("Length of prefix plus max number of digits should not exceeds the length of transaction reference"));

			newSequence.XD_MaximumNumberDigits = 8;
			newSequence.Validation.ValidateXD_Prefix();
			Assert(!newSequence.XD_PrefixInfo.HasErrors());
		}

		public void TestValidateEmptyXD_Prefix_EnableEInvoicingFunctionalityReceivables()
		{
			var countriesHaveSpecialRules = new[] { Core.Constants.CountryCodes.Argentina };
			var countriesShouldHaveErrorsWhenEnable = new[] { Core.Constants.CountryCodes.VietNam };
			var countriesShouldNotHaveErrors = Core.Constants.CountryCodes.GetAll().Except(countriesShouldHaveErrorsWhenEnable).Except(countriesHaveSpecialRules).ToArray();

			newSequence.XD_Prefix = ZString.Empty;

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				AssertXD_PrefixValidation(countriesShouldNotHaveErrors, hasError: false);
				AssertXD_PrefixValidation(countriesShouldHaveErrorsWhenEnable, hasError: false);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				AssertXD_PrefixValidation(countriesShouldNotHaveErrors, hasError: false);
				AssertXD_PrefixValidation(countriesShouldHaveErrorsWhenEnable, hasError: true);
			}

			void AssertXD_PrefixValidation(string[] countries, bool hasError)
			{
				foreach (var country in countries)
				{
					var refCountry = RefCountry.LoadFromCountryCode(Factory, country);
					if (refCountry == null)
					{
						var tempCountry = Factory.NewWithValidTestData<RefCountry>();
						tempCountry.RN_Code = country;
						Factory.Save();
					}
					GlbCompany.CurrentCompany.SetCountry(country);
					
					newSequence.Validation.ValidateXD_Prefix();
					if (hasError)
					{
						AssertHasError(newSequence.XD_PrefixInfo, "Please enter a Series Prefix");
					}
					else
					{
						AssertNoErrors(newSequence.XD_PrefixInfo);
						AssertHasWarning(newSequence.XD_PrefixInfo, "Prefix is blank");
					}
				}
			}
		}

		public void TestValidateXD_StartNumber()
		{
			newSequence.XD_StartNumber = 1;
			newSequence.XD_EndNumber = 100;
			newSequence.XD_NextNumber = 20;

			newSequence.XD_StartNumber = 101;
			newSequence.Validation.ValidateXD_StartNumber();
			Assert(newSequence.XD_StartNumberInfo.HasError("Start number cannot be greater than end number"));

			newSequence.XD_StartNumber = 21;
			newSequence.Validation.ValidateXD_StartNumber();
			Assert(newSequence.XD_StartNumberInfo.HasError("Start number cannot be greater than next number"));

			newSequence.XD_StartNumber = 10000;
			newSequence.Validation.ValidateXD_StartNumber();
			Assert(newSequence.XD_StartNumberInfo.HasError("The start number exceeds the number of digits (4 digits) set for the sequence"));

			newSequence.XD_StartNumber = 10;
			newSequence.Validation.ValidateXD_StartNumber();
			Assert(!newSequence.XD_StartNumberInfo.HasErrors());
		}

		public void TestValidateXD_PrintingAuthorizationNumber()
		{
			var expectedErrorMessage = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Account -> Compliance Sequences -> Edit -> Printing Authorization Number";

			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			Factory.Save();

			Env.Security.ComplianceSequencesModifyPrintingAuthNo.IsAllowed = true;
			sequence.XD_PrintingAuthorizationNumber = "auth123";
			AssertNoError(sequence.XD_PrintingAuthorizationNumberInfo, expectedErrorMessage);

			Factory.Save();

			Env.Security.ComplianceSequencesModifyPrintingAuthNo.IsAllowed = false;
			sequence.XD_PrintingAuthorizationNumber = "auth456";
			AssertHasError(sequence.XD_PrintingAuthorizationNumberInfo, expectedErrorMessage);
		}

		public void TestValidateXD_EndNumber()
		{
			newSequence.XD_StartNumber = 10;
			newSequence.XD_EndNumber = 100;
			newSequence.XD_NextNumber = 20;

			newSequence.XD_EndNumber = 9;
			newSequence.Validation.ValidateXD_EndNumber();
			Assert(newSequence.XD_EndNumberInfo.HasError("End number must be greater than start number"));

			newSequence.XD_EndNumber = 19;
			newSequence.Validation.ValidateXD_EndNumber();
			Assert(newSequence.XD_EndNumberInfo.HasError("End number must be greater than or equal to next number"));

			newSequence.XD_EndNumber = 10000;
			newSequence.Validation.ValidateXD_EndNumber();
			Assert(newSequence.XD_EndNumberInfo.HasError("The end number exceeds the number of digits (4 digits) set for the sequence"));

			newSequence.XD_EndNumber = 200;
			newSequence.Validation.ValidateXD_EndNumber();
			Assert(!newSequence.XD_EndNumberInfo.HasErrors());
		}

		public void TestValidateXD_NextNumber()
		{
			newSequence.XD_StartNumber = 10;
			newSequence.XD_EndNumber = 100;
			newSequence.XD_NextNumber = 20;

			newSequence.XD_NextNumber = 9;
			newSequence.Validation.ValidateXD_NextNumber();
			Assert(newSequence.XD_NextNumberInfo.HasError("Next number must be between start number and end number"));

			newSequence.XD_NextNumber = 101;
			newSequence.Validation.ValidateXD_NextNumber();
			Assert(newSequence.XD_NextNumberInfo.HasError("Next number must be between start number and end number"));

			newSequence.XD_NextNumber = 21;
			newSequence.Validation.ValidateXD_NextNumber();
			Assert(!newSequence.XD_NextNumberInfo.HasErrors());
		}

		public void TestNumberSeriesMustNotOverlapWithBranchBook()
		{
			AccComplianceSequence existedBranchBook = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			existedBranchBook.XD_SequenceClass = "TXI";
			existedBranchBook.XD_Code = "AAA";
			existedBranchBook.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Branch;
			existedBranchBook.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			existedBranchBook.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			existedBranchBook.XD_Prefix = "ABC";
			existedBranchBook.XD_MaximumNumberDigits = 5;
			existedBranchBook.XD_StartNumber = 10;
			existedBranchBook.XD_EndNumber = 30;
			existedBranchBook.XD_NextNumber = 31;
			existedBranchBook.XD_IsActive = true;
			TestFactory.Save();

			newSequence = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			newSequence.XD_SequenceClass = "TXI";
			newSequence.XD_Code = "BBB";
			newSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Branch;
			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			newSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			newSequence.XD_Prefix = "ABC";
			newSequence.XD_MaximumNumberDigits = 5;
			newSequence.XD_StartNumber = 5;
			newSequence.XD_EndNumber = 25;
			newSequence.XD_NextNumber = 6;
			newSequence.XD_IsActive = true;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Expect overlapping error due to new range overlaps start number", newSequence);

			newSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			newSequence.XD_StartNumber = 15;
			newSequence.XD_EndNumber = 35;
			newSequence.XD_NextNumber = 16;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Expect overlapping error due to new range overlaps end number", newSequence);

			newSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			newSequence.XD_StartNumber = 15;
			newSequence.XD_EndNumber = 25;
			newSequence.XD_NextNumber = 16;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Expect overlapping error due to new range is a subset", newSequence);

			newSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			newSequence.XD_StartNumber = 5;
			newSequence.XD_EndNumber = 35;
			newSequence.XD_NextNumber = 6;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Expect overlapping error due to new range is a superset", newSequence);

			newSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			newSequence.XD_StartNumber = 35;
			newSequence.XD_EndNumber = 45;
			newSequence.XD_NextNumber = 36;
			newSequence.Validation.ValidateXD_Prefix();
			AssertNotOverlap("Expect no overlapping errors", newSequence);

			newSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			newSequence.XD_StartNumber = 15;
			newSequence.XD_EndNumber = 25;
			newSequence.XD_NextNumber = 16;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Expect overlapping error due to new range is a subset", newSequence);
			newSequence.XD_IsActive = false;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Setting inactive does not have any impact on series validation", newSequence);
		}

		public void TestNumberSeriesMustNotOverlapWithCompanyBook()
		{
			AccComplianceSequence existedCompanyBook = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			existedCompanyBook.XD_SequenceClass = "TXI";
			existedCompanyBook.XD_Code = "AAA";
			existedCompanyBook.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Company;
			existedCompanyBook.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			existedCompanyBook.XD_Prefix = "ABC";
			existedCompanyBook.XD_MaximumNumberDigits = 5;
			existedCompanyBook.XD_StartNumber = 10;
			existedCompanyBook.XD_EndNumber = 30;
			existedCompanyBook.XD_NextNumber = 31;
			existedCompanyBook.XD_IsActive = true;
			TestFactory.Save();

			newSequence = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			newSequence.XD_SequenceClass = "TXI";
			newSequence.XD_Code = "BBB";
			newSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Company;
			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			newSequence.XD_Prefix = "ABC";
			newSequence.XD_MaximumNumberDigits = 5;
			newSequence.XD_StartNumber = 5;
			newSequence.XD_EndNumber = 25;
			newSequence.XD_NextNumber = 5;
			newSequence.XD_IsActive = true;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Expect overlapping error due to new range overlaps start number", newSequence);
			newSequence.XD_GC_Company = ZGuid.Empty;
			newSequence.Validation.ValidateXD_Prefix();
			AssertNotOverlap("", newSequence);

			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			newSequence.XD_StartNumber = 15;
			newSequence.XD_EndNumber = 35;
			newSequence.XD_NextNumber = 15;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Expect overlapping error due to new range overlaps end number", newSequence);
			newSequence.XD_GC_Company = ZGuid.Empty;
			newSequence.Validation.ValidateXD_Prefix();
			AssertNotOverlap("", newSequence);

			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			newSequence.XD_StartNumber = 15;
			newSequence.XD_EndNumber = 25;
			newSequence.XD_NextNumber = 15;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Expect overlapping error due to new range is a subset", newSequence);
			newSequence.XD_GC_Company = ZGuid.Empty;
			newSequence.Validation.ValidateXD_Prefix();
			AssertNotOverlap("", newSequence);

			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			newSequence.XD_StartNumber = 5;
			newSequence.XD_EndNumber = 35;
			newSequence.XD_NextNumber = 5;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Expect overlapping error due to new range is a superset", newSequence);
			newSequence.XD_GC_Company = ZGuid.Empty;
			newSequence.Validation.ValidateXD_Prefix();
			AssertNotOverlap("", newSequence);

			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			newSequence.XD_StartNumber = 35;
			newSequence.XD_EndNumber = 45;
			newSequence.XD_NextNumber = 35;
			newSequence.Validation.ValidateXD_Prefix();
			AssertNotOverlap("Expect no overlapping errors", newSequence);
			newSequence.XD_GC_Company = ZGuid.Empty;
			newSequence.Validation.ValidateXD_Prefix();
			AssertNotOverlap("", newSequence);

			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			newSequence.XD_StartNumber = 15;
			newSequence.XD_EndNumber = 25;
			newSequence.XD_NextNumber = 15;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Expect overlapping error due to new range is a subset", newSequence);
			newSequence.XD_IsActive = false;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Setting inactive does not have any impact on series validation", newSequence);
		}

		public void TestNumberSeriesMustNotOverlapWithBDPBook()
		{
			var existedCompanyBook = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			existedCompanyBook.XD_SequenceClass = "TXI";
			existedCompanyBook.XD_Code = "AAA";
			existedCompanyBook.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.BranchDepartment;
			existedCompanyBook.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			existedCompanyBook.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			existedCompanyBook.XD_GE_Department = GlbDepartment.CurrentDepartment.PK;
			existedCompanyBook.XD_Prefix = "ABC";
			existedCompanyBook.XD_MaximumNumberDigits = 5;
			existedCompanyBook.XD_StartNumber = 10;
			existedCompanyBook.XD_EndNumber = 30;
			existedCompanyBook.XD_NextNumber = 31;
			existedCompanyBook.XD_IsActive = true;
			TestFactory.Save();

			newSequence = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			newSequence.XD_SequenceClass = "TXI";
			newSequence.XD_Code = "BBB";
			newSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.BranchDepartment;
			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			newSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			newSequence.XD_GE_Department = GlbDepartment.CurrentDepartment.PK;
			newSequence.XD_Prefix = "ABC";
			newSequence.XD_MaximumNumberDigits = 5;
			newSequence.XD_StartNumber = 5;
			newSequence.XD_EndNumber = 25;
			newSequence.XD_NextNumber = 5;
			newSequence.XD_IsActive = true;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Expect overlapping error due to new range overlaps start number", newSequence);

			newSequence.XD_GE_Department = GlbDepartment.CurrentDepartment.PK;
			newSequence.XD_StartNumber = 15;
			newSequence.XD_EndNumber = 35;
			newSequence.XD_NextNumber = 15;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Expect overlapping error due to new range overlaps end number", newSequence);

			newSequence.XD_GE_Department = GlbDepartment.CurrentDepartment.PK;
			newSequence.XD_StartNumber = 15;
			newSequence.XD_EndNumber = 25;
			newSequence.XD_NextNumber = 15;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Expect overlapping error due to new range is a subset", newSequence);

			newSequence.XD_GE_Department = GlbDepartment.CurrentDepartment.PK;
			newSequence.XD_StartNumber = 5;
			newSequence.XD_EndNumber = 35;
			newSequence.XD_NextNumber = 5;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Expect overlapping error due to new range is a superset", newSequence);

			newSequence.XD_GE_Department = GlbDepartment.CurrentDepartment.PK;
			newSequence.XD_StartNumber = 35;
			newSequence.XD_EndNumber = 45;
			newSequence.XD_NextNumber = 35;
			newSequence.Validation.ValidateXD_Prefix();
			AssertNotOverlap("Expect no overlapping errors", newSequence);

			newSequence.XD_GE_Department = GlbDepartment.CurrentDepartment.PK;
			newSequence.XD_StartNumber = 15;
			newSequence.XD_EndNumber = 25;
			newSequence.XD_NextNumber = 15;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Expect overlapping error due to new range is a subset", newSequence);
			newSequence.XD_IsActive = false;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Setting inactive does not have any impact on series validation", newSequence);
		}

		public void TestNumberSeriesMustNotOverlapWithCTRBook()
		{
			AccComplianceSequence existedCompanyBook = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			existedCompanyBook.XD_SequenceClass = "TXI";
			existedCompanyBook.XD_Code = "AAA";
			existedCompanyBook.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Counter;
			existedCompanyBook.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			existedCompanyBook.XD_Prefix = "ABC";
			existedCompanyBook.XD_MaximumNumberDigits = 5;
			existedCompanyBook.XD_StartNumber = 10;
			existedCompanyBook.XD_EndNumber = 30;
			existedCompanyBook.XD_NextNumber = 31;
			existedCompanyBook.XD_IsActive = true;
			TestFactory.Save();

			newSequence = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			newSequence.XD_SequenceClass = "TXI";
			newSequence.XD_Code = "BBB";
			newSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Counter;
			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			newSequence.XD_Prefix = "ABC";
			newSequence.XD_MaximumNumberDigits = 5;
			newSequence.XD_StartNumber = 5;
			newSequence.XD_EndNumber = 25;
			newSequence.XD_NextNumber = 5;
			newSequence.XD_IsActive = true;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Expect overlapping error due to new range overlaps start number", newSequence);
			newSequence.XD_GC_Company = ZGuid.Empty;
			newSequence.Validation.ValidateXD_Prefix();
			AssertNotOverlap("", newSequence);

			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			newSequence.XD_StartNumber = 15;
			newSequence.XD_EndNumber = 35;
			newSequence.XD_NextNumber = 15;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Expect overlapping error due to new range overlaps end number", newSequence);
			newSequence.XD_GC_Company = ZGuid.Empty;
			newSequence.Validation.ValidateXD_Prefix();
			AssertNotOverlap("", newSequence);

			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			newSequence.XD_StartNumber = 15;
			newSequence.XD_EndNumber = 25;
			newSequence.XD_NextNumber = 15;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Expect overlapping error due to new range is a subset", newSequence);
			newSequence.XD_GC_Company = ZGuid.Empty;
			newSequence.Validation.ValidateXD_Prefix();
			AssertNotOverlap("", newSequence);

			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			newSequence.XD_StartNumber = 5;
			newSequence.XD_EndNumber = 35;
			newSequence.XD_NextNumber = 5;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Expect overlapping error due to new range is a superset", newSequence);
			newSequence.XD_GC_Company = ZGuid.Empty;
			newSequence.Validation.ValidateXD_Prefix();
			AssertNotOverlap("", newSequence);

			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			newSequence.XD_StartNumber = 35;
			newSequence.XD_EndNumber = 45;
			newSequence.XD_NextNumber = 35;
			newSequence.Validation.ValidateXD_Prefix();
			AssertNotOverlap("Expect no overlapping errors", newSequence);
			newSequence.XD_GC_Company = ZGuid.Empty;
			newSequence.Validation.ValidateXD_Prefix();
			AssertNotOverlap("", newSequence);

			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			newSequence.XD_StartNumber = 15;
			newSequence.XD_EndNumber = 25;
			newSequence.XD_NextNumber = 15;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Expect overlapping error due to new range is a subset", newSequence);
			newSequence.XD_IsActive = false;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Setting inactive does not have any impact on series validation", newSequence);
		}

		public void TestNumberSeriesMustNotOverlapWithActive()
		{
			AccComplianceSequence existedCompanyBook = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			existedCompanyBook.XD_SequenceClass = "TXI";
			existedCompanyBook.XD_Code = "AAA";
			existedCompanyBook.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Counter;
			existedCompanyBook.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			existedCompanyBook.XD_Prefix = "ABC";
			existedCompanyBook.XD_MaximumNumberDigits = 5;
			existedCompanyBook.XD_StartNumber = 10;
			existedCompanyBook.XD_EndNumber = 30;
			existedCompanyBook.XD_NextNumber = 31;
			existedCompanyBook.XD_IsActive = true;
			TestFactory.Save();

			newSequence = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			newSequence.XD_SequenceClass = "TXI";
			newSequence.XD_Code = "BBB";
			newSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Counter;
			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			newSequence.XD_Prefix = "ABC";
			newSequence.XD_MaximumNumberDigits = 5;
			newSequence.XD_StartNumber = 5;
			newSequence.XD_EndNumber = 25;
			newSequence.XD_NextNumber = 5;
			newSequence.XD_IsActive = true;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Expect overlapping error due to new range overlaps start number", newSequence);
			newSequence.XD_IsActive = false;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Setting inactive does not have any impact on series validation", newSequence);

			existedCompanyBook.XD_IsActive = false;
			TestFactory.Save();

			newSequence.XD_IsActive = false;
			newSequence.Validation.ValidateXD_Prefix();
			AssertOverlap("Expect overlapping error due to new range overlaps start number", newSequence);
		}

		public void TestValidateComplianceSeriesNumberPlusPrefixNotUnique()
		{
			AccComplianceSequence sequence1 = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			sequence1.XD_SequenceClass = "TXI";
			sequence1.XD_Code = "AAA";
			sequence1.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence1.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence1.XD_Prefix = "ABC";
			sequence1.XD_MaximumNumberDigits = 5;
			sequence1.XD_StartNumber = 10;
			sequence1.XD_EndNumber = 30;
			sequence1.XD_NextNumber = 31;
			sequence1.XD_IsActive = false;
			TestFactory.Save();

			newSequence = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			newSequence.XD_SequenceClass = "TXI";
			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			newSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			newSequence.XD_Prefix = "ABC00";
			newSequence.XD_MaximumNumberDigits = 3;
			newSequence.XD_StartNumber = 15;
			newSequence.XD_EndNumber = 35;
			newSequence.XD_NextNumber = 15;
			newSequence.XD_IsActive = true;
			newSequence.Validation.ValidateXD_Prefix();
			Assert("Expect overlapping error due to new range overlaps end number", newSequence.XD_PrefixInfo.HasErrors());

			newSequence.XD_Prefix = "ABC00";
			newSequence.XD_MaximumNumberDigits = 4;
			newSequence.Validation.ValidateXD_Prefix();
			Assert("Expect no overlapping errors", !newSequence.XD_PrefixInfo.HasErrors());

			newSequence.XD_Prefix = "ABC000";
			newSequence.XD_MaximumNumberDigits = 3;
			newSequence.Validation.ValidateXD_Prefix();
			Assert("Expect no overlapping errors", !newSequence.XD_PrefixInfo.HasErrors());
		}

		public void TestValidateXD_RollupBehaviourWhenMaxExceeded()
		{
			newSequence.XD_SU_MenuItem = Factory.NewWithValidTestData<StmMenuItem>().PK;
			newSequence.XD_RollupBehaviourWhenMaxExceeded = "";
			newSequence.Validation.ValidateXD_RollupBehaviourWhenMaxExceeded();
			Assert("compliance sequence must have a XD_RollupBehaviourWhenMaxExceeded", newSequence.XD_RollupBehaviourWhenMaxExceededInfo.HasErrors());

			newSequence.XD_RollupBehaviourWhenMaxExceeded = "XXX";
			newSequence.Validation.ValidateXD_RollupBehaviourWhenMaxExceeded();
			Assert("compliance sequence must have a valid XD_RollupBehaviourWhenMaxExceeded", newSequence.XD_RollupBehaviourWhenMaxExceededInfo.HasErrors());

			newSequence.XD_RollupBehaviourWhenMaxExceeded = "SSM";
			Assert(!newSequence.XD_RollupBehaviourWhenMaxExceededInfo.HasErrors());
		}

		public void TestValidateXD_SO_ComplianceMenu()
		{
			newSequence.XD_SU_MenuItem = ZGuid.Empty;
			newSequence.Validation.ValidateXD_SU_MenuItem();
			Assert("compliance sequence can have a blank XD_SU_MenuItem", !newSequence.XD_SU_MenuItemInfo.HasErrors());

			ZGuid menuPK = TestFactory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_BusinessContext, nameof(BusinessContext.Shipment))).PK;
			Factory.Save();
			newSequence.XD_SU_MenuItem = menuPK;
			newSequence.Validation.ValidateXD_SU_MenuItem();
			Assert("invalid menu selected", newSequence.XD_SU_MenuItemInfo.HasErrors());

			menuPK = TestFactory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_BusinessContext, nameof(BusinessContext.ARInvoice))).PK;
			newSequence.XD_SU_MenuItem = menuPK;
			newSequence.Validation.ValidateXD_SU_MenuItem();
			Assert(!newSequence.XD_SU_MenuItemInfo.HasErrors());
		}

		public void TestCheckXD_IsActive()
		{
			var complianceSequance1 = Factory.NewWithValidTestData<AccComplianceSequence>();
			var complianceSequance2 = Factory.NewWithValidTestData<AccComplianceSequence>();

			var mockProvier = new Mock<IComplianceSequencePresentationProvider>();
			mockProvier.Setup(x => x.CanReactivate(It.Is<AccComplianceSequence>(o => o == complianceSequance1))).Returns(ZString.Empty);
			mockProvier.Setup(x => x.CanReactivate(It.Is<AccComplianceSequence>(o => o == complianceSequance2))).Returns("Can not reactive");

			var mockFactory = new Mock<IAccountingMasterFilesDependencyFactory>();
			mockFactory.Setup(x => x.GetComplianceSequencePresentationProvider()).Returns(mockProvier.Object);

			using (ObjectFactory.Substitute(mockFactory.Object))
			{
				Assert(complianceSequance1.XD_IsActive);
				complianceSequance1.XD_IsActive = false;
				AssertNoErrors(complianceSequance1.XD_IsActiveInfo);
				Factory.Save();

				complianceSequance1.XD_IsActive = true;
				AssertNoErrors(complianceSequance1.XD_IsActiveInfo);
				Factory.Save();

				Assert(complianceSequance2.XD_IsActive);
				complianceSequance2.XD_IsActive = false;
				AssertNoErrors(complianceSequance2.XD_IsActiveInfo);
				Factory.Save();

				complianceSequance2.XD_IsActive = true;
				AssertHasError(complianceSequance2.XD_IsActiveInfo, "Can not reactive");

				mockFactory.Verify(x => x.GetComplianceSequencePresentationProvider(), Times.Exactly(2));
				mockProvier.Verify(x => x.CanReactivate(complianceSequance1), Times.Once);
				mockProvier.Verify(x => x.CanReactivate(complianceSequance2), Times.Once);
			}
		}

		public void TestXD_MaximumNumberDigits()
		{
			AssertEquals("XD_StartNumber max length and XD_EndNumber max length are not the same. Please review the max length check for XD_MaximumNumberDigits in CheckXD_MaximumNumberDigits method", AccComplianceSequenceSchema.XD_StartNumber.Precision, AccComplianceSequenceSchema.XD_EndNumber.Precision);

			newSequence.XD_MaximumNumberDigits = 10;
			newSequence.Validation.ValidateXD_MaximumNumberDigits();
			Assert(newSequence.XD_MaximumNumberDigitsInfo.HasError("Max Number Digits exceeds the maximum length of 9"));

			newSequence.XD_MaximumNumberDigits = 9;
			newSequence.Validation.ValidateXD_MaximumNumberDigits();
			Assert(!newSequence.XD_MaximumNumberDigitsInfo.HasErrors());

			newSequence.XD_Prefix = "123456789012";
			newSequence.XD_MaximumNumberDigits = 9;
			newSequence.Validation.ValidateXD_MaximumNumberDigits();
			Assert(newSequence.XD_MaximumNumberDigitsInfo.HasError("Length of prefix plus max number of digits should not exceeds the length of transaction reference"));

			newSequence.XD_MaximumNumberDigits = 8;
			newSequence.Validation.ValidateXD_MaximumNumberDigits();
			Assert(!newSequence.XD_MaximumNumberDigitsInfo.HasErrors());
		}

		public void TestXD_GB_BranchOwner()
		{
			AssertEquals("TXI", existedSequence.XD_SequenceClass);
			AssertEquals(Core.Constants.ComplianceBookAllocationLevel.Branch, existedSequence.XD_AllocationLevel);

			newSequence.XD_SequenceClass = "TXI";
			newSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			newSequence.Validation.ValidateXD_GB_BranchOwner();
			AssertEquals(Core.Constants.ComplianceBookAllocationLevel.Branch, newSequence.XD_AllocationLevel);
			AssertEquals(true, newSequence.XD_GB_BranchOwnerInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));

			existedSequence.XD_IsActive = false;
			TestFactory.Save();
			newSequence.Validation.ValidateXD_GB_BranchOwner();
			Assert(!newSequence.XD_GB_BranchOwnerInfo.HasErrors());

			existedSequence.XD_IsActive = true;
			TestFactory.Save();
			newSequence.XD_SequenceClass = "TCR";
			newSequence.Validation.ValidateXD_GB_BranchOwner();
			Assert(!newSequence.XD_GB_BranchOwnerInfo.HasErrors());

			AssertEquals("TXI", existedSequence.XD_SequenceClass);
			AssertEquals(Core.Constants.ComplianceBookAllocationLevel.Branch, existedSequence.XD_AllocationLevel);
			newSequence.XD_SequenceClass = "TXI";
			newSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Branch;
			newSequence.Validation.ValidateXD_GB_BranchOwner();
			Assert("Sub Type / Branch should be unique for BRN", newSequence.XD_GB_BranchOwnerInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));
			newSequence.XD_SequenceClass = "TCR";
			newSequence.Validation.ValidateXD_GB_BranchOwner();
			Assert(!newSequence.XD_GB_BranchOwnerInfo.HasErrors());

			newSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.BranchDepartment;
			newSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			newSequence.XD_GE_Department = ZGuid.Empty;
			newSequence.Validation.ValidateXD_GB_BranchOwner();
			AssertEquals("Should not be empty for BDP.", false, newSequence.XD_GB_BranchOwnerInfo.HasErrors());
			newSequence.XD_GB_BranchOwner = ZGuid.Empty;
			newSequence.Validation.ValidateXD_GB_BranchOwner();
			AssertEquals(true, newSequence.XD_GB_BranchOwnerInfo.HasErrors());
		}

		public void TestXD_GE_Department()
		{
			newSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Company;
			newSequence.XD_GE_Department = ZGuid.Empty;
			newSequence.Validation.ValidateXD_GE_Department();
			AssertEquals("Should not affect COM.", false, newSequence.XD_GE_DepartmentInfo.HasErrors());

			newSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.BranchDepartment;
			newSequence.XD_GE_Department = ZGuid.Empty;
			newSequence.Validation.ValidateXD_GE_Department();
			var errors = newSequence.XD_GE_DepartmentInfo.GetErrors();
			AssertEquals(1, errors.Count());
			AssertEquals("Should not be empty for BDP.", "Please enter a value.", errors.GetFirst().Message);
			newSequence.XD_GE_Department = GlbDepartment.CurrentDepartment.PK;
			newSequence.Validation.ValidateXD_GE_Department();
			AssertEquals(false, newSequence.XD_GE_DepartmentInfo.HasErrors());

			AssertEquals("TXI", existedSequence.XD_SequenceClass);
			existedSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			existedSequence.XD_GE_Department = GlbDepartment.CurrentDepartment.PK;
			existedSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.BranchDepartment;
			TestFactory.Save();
			newSequence.XD_SequenceClass = "TXI";
			newSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			newSequence.XD_GE_Department = GlbDepartment.CurrentDepartment.PK;
			newSequence.Validation.ValidateXD_GE_Department();
			AssertEquals("Sub Type / Branch / Department must be unique for BDP.", true, newSequence.XD_GE_DepartmentInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));
			newSequence.XD_GB_BranchOwner = ZGuid.Empty;
			newSequence.Validation.ValidateXD_GE_Department();
			AssertEquals(false, newSequence.XD_GE_DepartmentInfo.HasErrors());
		}

		public void TestValidatXD_StartDate()
		{
			newSequence.XD_StartDate = new ZDate(2018, 06, 03);
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 06, 02);
			newSequence.Validation.ValidateXD_StartDate();
			Assert("Valid date should be smaller than expire date", newSequence.XD_StartDateInfo.HasError("Valid date should be smaller than expire date"));

			newSequence.XD_StartDate = new ZDate(2018, 06, 01);
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 06, 02);
			newSequence.Validation.ValidateXD_StartDate();
			Assert(!newSequence.XD_StartDateInfo.HasError("Valid date should be smaller than expire date"));

			var existedSequenceWithinTheValidtyPeriod = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			existedSequenceWithinTheValidtyPeriod.XD_SequenceClass = "SSP";
			existedSequenceWithinTheValidtyPeriod.XD_Code = "LM5";
			existedSequenceWithinTheValidtyPeriod.XD_StartNumber = 200;
			existedSequenceWithinTheValidtyPeriod.XD_EndNumber = 300;
			existedSequenceWithinTheValidtyPeriod.XD_MaximumNumberDigits = 6;
			existedSequenceWithinTheValidtyPeriod.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			existedSequenceWithinTheValidtyPeriod.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			existedSequenceWithinTheValidtyPeriod.XD_StartDate = new ZDate(2018, 07, 01);
			existedSequenceWithinTheValidtyPeriod.XD_ExpiryDate = new ZDateTime(2018, 07, 31);
			existedSequenceWithinTheValidtyPeriod.XD_IsActive = true;
			TestFactory.Save();

			newSequence.XD_StartDate = new ZDate(2018, 07, 15);
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 08, 31);
			newSequence.XD_SequenceClass = "SSP";
			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			newSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			newSequence.Validation.ValidateXD_StartDate();
			Assert(newSequence.XD_StartDateInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));

			newSequence.XD_StartDate = new ZDate(2018, 06, 30);
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 08, 31);
			newSequence.Validation.ValidateXD_StartDate();
			Assert(newSequence.XD_StartDateInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));

			newSequence.XD_StartDate = new ZDate(2018, 06, 30);
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 07, 15);
			newSequence.Validation.ValidateXD_StartDate();
			Assert(newSequence.XD_StartDateInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));

			newSequence.XD_StartDate = new ZDate(2018, 08, 01);
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 08, 31);
			newSequence.Validation.ValidateXD_StartDate();
			Assert(!newSequence.XD_StartDateInfo.HasErrors());

			newSequence.XD_StartDate = new ZDate(2018, 06, 01);
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 06, 30);
			newSequence.Validation.ValidateXD_StartDate();
			Assert(!newSequence.XD_StartDateInfo.HasErrors());

			newSequence.XD_StartDate = ZDate.Empty;
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 08, 31);
			newSequence.Validation.ValidateXD_StartDate();
			Assert(newSequence.XD_StartDateInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));

			newSequence.XD_StartDate = ZDate.Empty;
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 07, 15);
			newSequence.Validation.ValidateXD_StartDate();
			Assert(newSequence.XD_StartDateInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));

			newSequence.XD_StartDate = ZDate.Empty;
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 06, 30);
			newSequence.Validation.ValidateXD_StartDate();
			Assert(!newSequence.XD_StartDateInfo.HasErrors());

			newSequence.XD_StartDate = new ZDate(2018, 07, 15);
			newSequence.XD_ExpiryDate = ZDateTime.Empty;
			newSequence.Validation.ValidateXD_StartDate();
			Assert(newSequence.XD_StartDateInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));

			newSequence.XD_StartDate = new ZDate(2018, 06, 30);
			newSequence.XD_ExpiryDate = ZDateTime.Empty;
			newSequence.Validation.ValidateXD_StartDate();
			Assert(newSequence.XD_StartDateInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));

			newSequence.XD_StartDate = new ZDate(2018, 08, 01);
			newSequence.XD_ExpiryDate = ZDateTime.Empty;
			newSequence.Validation.ValidateXD_StartDate();
			Assert(!newSequence.XD_StartDateInfo.HasErrors());

			newSequence.XD_StartDate = ZDate.Empty;
			newSequence.XD_ExpiryDate = ZDateTime.Empty;
			newSequence.Validation.ValidateXD_StartDate();
			Assert(newSequence.XD_StartDateInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));
		}

		public void TestValidatXD_ExpiryDate()
		{
			newSequence.XD_StartDate = new ZDate(2018, 06, 03);
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 06, 02);
			newSequence.Validation.ValidateXD_ExpiryDate();
			Assert("Valid date should be smaller than expire date", newSequence.XD_ExpiryDateInfo.HasError("Valid date should be smaller than expire date"));

			newSequence.XD_StartDate = new ZDate(2018, 06, 01);
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 06, 02);
			newSequence.Validation.ValidateXD_ExpiryDate();
			Assert(!newSequence.XD_ExpiryDateInfo.HasError("Valid date should be smaller than expire date"));

			var existedSequenceWithinTheValidtyPeriod = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			existedSequenceWithinTheValidtyPeriod.XD_SequenceClass = "NCR";
			existedSequenceWithinTheValidtyPeriod.XD_Code = "LM5";
			existedSequenceWithinTheValidtyPeriod.XD_StartNumber = 200;
			existedSequenceWithinTheValidtyPeriod.XD_EndNumber = 300;
			existedSequenceWithinTheValidtyPeriod.XD_MaximumNumberDigits = 6;
			existedSequenceWithinTheValidtyPeriod.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			existedSequenceWithinTheValidtyPeriod.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			existedSequenceWithinTheValidtyPeriod.XD_StartDate = new ZDate(2018, 07, 01);
			existedSequenceWithinTheValidtyPeriod.XD_ExpiryDate = new ZDateTime(2018, 07, 31);
			existedSequenceWithinTheValidtyPeriod.XD_IsActive = true;
			TestFactory.Save();

			newSequence.XD_StartDate = new ZDate(2018, 06, 01);
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 07, 15);
			newSequence.XD_SequenceClass = "NCR";
			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			newSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			newSequence.Validation.ValidateXD_ExpiryDate();
			Assert(newSequence.XD_ExpiryDateInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));

			newSequence.XD_StartDate = new ZDate(2018, 06, 30);
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 08, 31);
			newSequence.Validation.ValidateXD_ExpiryDate();
			Assert(newSequence.XD_ExpiryDateInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));

			newSequence.XD_StartDate = new ZDate(2018, 06, 30);
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 07, 15);
			newSequence.Validation.ValidateXD_ExpiryDate();
			Assert(newSequence.XD_ExpiryDateInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));

			newSequence.XD_StartDate = new ZDate(2018, 08, 01);
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 08, 31);
			newSequence.Validation.ValidateXD_ExpiryDate();
			Assert(!newSequence.XD_ExpiryDateInfo.HasErrors());

			newSequence.XD_StartDate = new ZDate(2018, 06, 01);
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 06, 30);
			newSequence.Validation.ValidateXD_ExpiryDate();
			Assert(!newSequence.XD_ExpiryDateInfo.HasErrors());

			newSequence.XD_StartDate = ZDate.Empty;
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 08, 31);
			newSequence.Validation.ValidateXD_ExpiryDate();
			Assert(newSequence.XD_ExpiryDateInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));

			newSequence.XD_StartDate = ZDate.Empty;
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 07, 15);
			newSequence.Validation.ValidateXD_ExpiryDate();
			Assert(newSequence.XD_ExpiryDateInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));

			newSequence.XD_StartDate = ZDate.Empty;
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 06, 30);
			newSequence.Validation.ValidateXD_ExpiryDate();
			Assert(!newSequence.XD_ExpiryDateInfo.HasErrors());

			newSequence.XD_StartDate = new ZDate(2018, 07, 15);
			newSequence.XD_ExpiryDate = ZDateTime.Empty;
			newSequence.Validation.ValidateXD_ExpiryDate();
			Assert(newSequence.XD_ExpiryDateInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));

			newSequence.XD_StartDate = new ZDate(2018, 06, 30);
			newSequence.XD_ExpiryDate = ZDateTime.Empty;
			newSequence.Validation.ValidateXD_ExpiryDate();
			Assert(newSequence.XD_ExpiryDateInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));

			newSequence.XD_StartDate = new ZDate(2018, 08, 01);
			newSequence.XD_ExpiryDate = ZDateTime.Empty;
			newSequence.Validation.ValidateXD_ExpiryDate();
			Assert(!newSequence.XD_ExpiryDateInfo.HasErrors());

			newSequence.XD_StartDate = ZDate.Empty;
			newSequence.XD_ExpiryDate = ZDateTime.Empty;
			newSequence.Validation.ValidateXD_ExpiryDate();
			Assert(newSequence.XD_ExpiryDateInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));
		}

		public void AssertIsExpiredMustNotOverlap(AccComplianceSequence sequence, ZDate startDate, ZDateTime expiryDate)
		{
			sequence.XD_StartDate = startDate;
			sequence.XD_ExpiryDate = expiryDate;
			sequence.Validation.ValidateXD_ExpiryDate();
			Assert(sequence.XD_ExpiryDateInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));
			sequence.Validation.ValidateXD_StartDate();
			Assert(sequence.XD_StartDateInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));
		}

		public void AssertNotIsExpiredMustNotOverlap(AccComplianceSequence sequence)
		{
			sequence.Validation.ValidateXD_ExpiryDate();
			Assert(!sequence.XD_ExpiryDateInfo.HasErrors());
			sequence.Validation.ValidateXD_StartDate();
			Assert(!sequence.XD_StartDateInfo.HasErrors());
		}

		void SetBranchOrCompanyOrDepartment(string allocationLevel, bool isClearValue)
		{
			switch (allocationLevel)
			{
				case Core.Constants.ComplianceBookAllocationLevel.Company:
					newSequence.XD_GC_Company = isClearValue ? ZGuid.Empty : GlbCompany.CurrentCompany.PK;
					break;
				case Core.Constants.ComplianceBookAllocationLevel.Branch:
					newSequence.XD_GB_BranchOwner = isClearValue ? ZGuid.Empty : GlbBranch.CurrentBranch.PK;
					break;
				case Core.Constants.ComplianceBookAllocationLevel.BranchDepartment:
					newSequence.XD_GE_Department = isClearValue ? ZGuid.Empty : GlbDepartment.CurrentDepartment.PK;
					break;
				default:
					break;
			}
		}

		void IsExpiredMustNotOverlapWith(string allocationLevel)
		{
			SetBranchOrCompanyOrDepartment(allocationLevel, false);
			AssertIsExpiredMustNotOverlap(newSequence, new ZDate(2018, 06, 01), new ZDateTime(2018, 07, 15));

			SetBranchOrCompanyOrDepartment(allocationLevel, true);
			AssertNotIsExpiredMustNotOverlap(newSequence);

			SetBranchOrCompanyOrDepartment(allocationLevel, false);
			AssertIsExpiredMustNotOverlap(newSequence, new ZDate(2018, 06, 30), new ZDateTime(2018, 08, 31));

			SetBranchOrCompanyOrDepartment(allocationLevel, true);
			AssertNotIsExpiredMustNotOverlap(newSequence);

			SetBranchOrCompanyOrDepartment(allocationLevel, false);
			AssertIsExpiredMustNotOverlap(newSequence, new ZDate(2018, 06, 30), new ZDateTime(2018, 07, 15));

			SetBranchOrCompanyOrDepartment(allocationLevel, true);
			AssertNotIsExpiredMustNotOverlap(newSequence);

			SetBranchOrCompanyOrDepartment(allocationLevel, false);
			newSequence.XD_StartDate = new ZDate(2018, 08, 01);
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 08, 31);
			AssertNotIsExpiredMustNotOverlap(newSequence);

			SetBranchOrCompanyOrDepartment(allocationLevel, true);
			AssertNotIsExpiredMustNotOverlap(newSequence);

			SetBranchOrCompanyOrDepartment(allocationLevel, false);
			newSequence.XD_StartDate = new ZDate(2018, 06, 01);
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 06, 30);
			AssertNotIsExpiredMustNotOverlap(newSequence);

			SetBranchOrCompanyOrDepartment(allocationLevel, true);
			AssertNotIsExpiredMustNotOverlap(newSequence);

			SetBranchOrCompanyOrDepartment(allocationLevel, false);
			AssertIsExpiredMustNotOverlap(newSequence, ZDate.Empty, new ZDateTime(2018, 08, 31));

			SetBranchOrCompanyOrDepartment(allocationLevel, true);
			AssertNotIsExpiredMustNotOverlap(newSequence);

			SetBranchOrCompanyOrDepartment(allocationLevel, false);
			AssertIsExpiredMustNotOverlap(newSequence, ZDate.Empty, new ZDateTime(2018, 07, 15));

			SetBranchOrCompanyOrDepartment(allocationLevel, true);
			AssertNotIsExpiredMustNotOverlap(newSequence);

			SetBranchOrCompanyOrDepartment(allocationLevel, false);
			newSequence.XD_StartDate = ZDate.Empty;
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 06, 30);
			AssertNotIsExpiredMustNotOverlap(newSequence);

			SetBranchOrCompanyOrDepartment(allocationLevel, true);
			AssertNotIsExpiredMustNotOverlap(newSequence);

			SetBranchOrCompanyOrDepartment(allocationLevel, false);
			AssertIsExpiredMustNotOverlap(newSequence, new ZDate(2018, 07, 15), ZDateTime.Empty);

			SetBranchOrCompanyOrDepartment(allocationLevel, true);
			AssertNotIsExpiredMustNotOverlap(newSequence);

			SetBranchOrCompanyOrDepartment(allocationLevel, false);
			AssertIsExpiredMustNotOverlap(newSequence, new ZDate(2018, 06, 30), ZDateTime.Empty);

			SetBranchOrCompanyOrDepartment(allocationLevel, true);
			AssertNotIsExpiredMustNotOverlap(newSequence);

			SetBranchOrCompanyOrDepartment(allocationLevel, false);
			newSequence.XD_StartDate = new ZDate(2018, 08, 01);
			newSequence.XD_ExpiryDate = ZDateTime.Empty;
			AssertNotIsExpiredMustNotOverlap(newSequence);

			SetBranchOrCompanyOrDepartment(allocationLevel, true);
			AssertNotIsExpiredMustNotOverlap(newSequence);

			SetBranchOrCompanyOrDepartment(allocationLevel, false);
			AssertIsExpiredMustNotOverlap(newSequence, ZDate.Empty, ZDateTime.Empty);

			SetBranchOrCompanyOrDepartment(allocationLevel, true);
			AssertNotIsExpiredMustNotOverlap(newSequence);

			SetBranchOrCompanyOrDepartment(allocationLevel, false);
			newSequence.XD_IsActive = true;
			AssertIsExpiredMustNotOverlap(newSequence, new ZDate(2018, 06, 30), new ZDateTime(2018, 08, 31));

			newSequence.XD_IsActive = false;
			AssertNotIsExpiredMustNotOverlap(newSequence);
		}

		public void TestIsExpiredMustNotOverlapWithBranchBook()
		{
			var existedSequenceWithinTheValidtyPeriod = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			existedSequenceWithinTheValidtyPeriod.XD_SequenceClass = "NCR";
			existedSequenceWithinTheValidtyPeriod.XD_Code = "LM5";
			existedSequenceWithinTheValidtyPeriod.XD_StartNumber = 200;
			existedSequenceWithinTheValidtyPeriod.XD_EndNumber = 300;
			existedSequenceWithinTheValidtyPeriod.XD_MaximumNumberDigits = 6;
			existedSequenceWithinTheValidtyPeriod.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Branch;
			existedSequenceWithinTheValidtyPeriod.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			existedSequenceWithinTheValidtyPeriod.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			existedSequenceWithinTheValidtyPeriod.XD_StartDate = new ZDate(2018, 07, 01);
			existedSequenceWithinTheValidtyPeriod.XD_ExpiryDate = new ZDateTime(2018, 07, 31);
			existedSequenceWithinTheValidtyPeriod.XD_IsActive = true;
			TestFactory.Save();

			newSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Branch;
			newSequence.XD_SequenceClass = "NCR";
			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			IsExpiredMustNotOverlapWith(Core.Constants.ComplianceBookAllocationLevel.Branch);
		}

		public void TestIsExpiredMustNotOverlapWithCompanyBook()
		{
			var existedSequenceWithinTheValidtyPeriod = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			existedSequenceWithinTheValidtyPeriod.XD_SequenceClass = "NCR";
			existedSequenceWithinTheValidtyPeriod.XD_Code = "LM5";
			existedSequenceWithinTheValidtyPeriod.XD_StartNumber = 200;
			existedSequenceWithinTheValidtyPeriod.XD_EndNumber = 300;
			existedSequenceWithinTheValidtyPeriod.XD_MaximumNumberDigits = 6;
			existedSequenceWithinTheValidtyPeriod.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Company;
			existedSequenceWithinTheValidtyPeriod.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			existedSequenceWithinTheValidtyPeriod.XD_StartDate = new ZDate(2018, 07, 01);
			existedSequenceWithinTheValidtyPeriod.XD_ExpiryDate = new ZDateTime(2018, 07, 31);
			existedSequenceWithinTheValidtyPeriod.XD_IsActive = true;
			TestFactory.Save();

			newSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Company;
			newSequence.XD_SequenceClass = "NCR";
			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			IsExpiredMustNotOverlapWith(Core.Constants.ComplianceBookAllocationLevel.Company);
		}

		public void TestIsExpiredMustNotOverlapWithBDPBook()
		{
			var existedSequenceWithinTheValidtyPeriod = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			existedSequenceWithinTheValidtyPeriod.XD_SequenceClass = "NCR";
			existedSequenceWithinTheValidtyPeriod.XD_Code = "LM5";
			existedSequenceWithinTheValidtyPeriod.XD_StartNumber = 200;
			existedSequenceWithinTheValidtyPeriod.XD_EndNumber = 300;
			existedSequenceWithinTheValidtyPeriod.XD_MaximumNumberDigits = 6;
			existedSequenceWithinTheValidtyPeriod.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.BranchDepartment;
			existedSequenceWithinTheValidtyPeriod.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			existedSequenceWithinTheValidtyPeriod.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			existedSequenceWithinTheValidtyPeriod.XD_GE_Department = GlbDepartment.CurrentDepartment.PK;
			existedSequenceWithinTheValidtyPeriod.XD_StartDate = new ZDate(2018, 07, 01);
			existedSequenceWithinTheValidtyPeriod.XD_ExpiryDate = new ZDateTime(2018, 07, 31);
			existedSequenceWithinTheValidtyPeriod.XD_IsActive = true;
			TestFactory.Save();

			newSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.BranchDepartment;
			newSequence.XD_SequenceClass = "NCR";
			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			newSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			IsExpiredMustNotOverlapWith(Core.Constants.ComplianceBookAllocationLevel.BranchDepartment);
		}

		public void TestIsExpiredMustNotOverlapWithCounterBook()
		{
			var existedSequenceWithinTheValidtyPeriod = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			existedSequenceWithinTheValidtyPeriod.XD_SequenceClass = "NCR";
			existedSequenceWithinTheValidtyPeriod.XD_Code = "LM5";
			existedSequenceWithinTheValidtyPeriod.XD_StartNumber = 200;
			existedSequenceWithinTheValidtyPeriod.XD_EndNumber = 300;
			existedSequenceWithinTheValidtyPeriod.XD_MaximumNumberDigits = 6;
			existedSequenceWithinTheValidtyPeriod.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			existedSequenceWithinTheValidtyPeriod.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Counter;
			existedSequenceWithinTheValidtyPeriod.XD_StartDate = new ZDate(2018, 07, 01);
			existedSequenceWithinTheValidtyPeriod.XD_ExpiryDate = new ZDateTime(2018, 07, 31);
			existedSequenceWithinTheValidtyPeriod.XD_IsActive = true;
			TestFactory.Save();

			newSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Counter;
			newSequence.XD_StartDate = new ZDate(2018, 06, 01);
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 07, 15);
			newSequence.XD_SequenceClass = "NCR";
			newSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			AssertNotIsExpiredMustNotOverlap(newSequence);

			newSequence.XD_StartDate = new ZDate(2018, 06, 30);
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 08, 31);
			AssertNotIsExpiredMustNotOverlap(newSequence);

			newSequence.XD_StartDate = new ZDate(2018, 06, 30);
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 07, 15);
			AssertNotIsExpiredMustNotOverlap(newSequence);

			newSequence.XD_StartDate = new ZDate(2018, 08, 01);
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 08, 31);
			AssertNotIsExpiredMustNotOverlap(newSequence);

			newSequence.XD_StartDate = new ZDate(2018, 06, 01);
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 06, 30);
			AssertNotIsExpiredMustNotOverlap(newSequence);

			newSequence.XD_StartDate = ZDate.Empty;
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 08, 31);
			AssertNotIsExpiredMustNotOverlap(newSequence);

			newSequence.XD_StartDate = ZDate.Empty;
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 07, 15);
			AssertNotIsExpiredMustNotOverlap(newSequence);

			newSequence.XD_StartDate = ZDate.Empty;
			newSequence.XD_ExpiryDate = new ZDateTime(2018, 06, 30);
			AssertNotIsExpiredMustNotOverlap(newSequence);

			newSequence.XD_StartDate = new ZDate(2018, 07, 15);
			newSequence.XD_ExpiryDate = ZDateTime.Empty;
			AssertNotIsExpiredMustNotOverlap(newSequence);

			newSequence.XD_StartDate = new ZDate(2018, 06, 30);
			newSequence.XD_ExpiryDate = ZDateTime.Empty;
			AssertNotIsExpiredMustNotOverlap(newSequence);

			newSequence.XD_StartDate = new ZDate(2018, 08, 01);
			newSequence.XD_ExpiryDate = ZDateTime.Empty;
			AssertNotIsExpiredMustNotOverlap(newSequence);

			newSequence.XD_StartDate = ZDate.Empty;
			newSequence.XD_ExpiryDate = ZDateTime.Empty;
			AssertNotIsExpiredMustNotOverlap(newSequence);

			newSequence.XD_IsActive = false;
			newSequence.XD_StartDate = ZDate.Empty;
			newSequence.XD_ExpiryDate = ZDateTime.Empty;
			AssertNotIsExpiredMustNotOverlap(newSequence);
		}

		public void TestIsExpiredMustNotOverlapWithActive()
		{
			var existedSequenceWithinTheValidtyPeriod = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			existedSequenceWithinTheValidtyPeriod.XD_SequenceClass = "NCR";
			existedSequenceWithinTheValidtyPeriod.XD_Code = "LM5";
			existedSequenceWithinTheValidtyPeriod.XD_StartNumber = 200;
			existedSequenceWithinTheValidtyPeriod.XD_EndNumber = 300;
			existedSequenceWithinTheValidtyPeriod.XD_MaximumNumberDigits = 6;
			existedSequenceWithinTheValidtyPeriod.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.BranchDepartment;
			existedSequenceWithinTheValidtyPeriod.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			existedSequenceWithinTheValidtyPeriod.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			existedSequenceWithinTheValidtyPeriod.XD_GE_Department = GlbDepartment.CurrentDepartment.PK;
			existedSequenceWithinTheValidtyPeriod.XD_StartDate = new ZDate(2018, 07, 01);
			existedSequenceWithinTheValidtyPeriod.XD_ExpiryDate = new ZDateTime(2018, 07, 31);
			existedSequenceWithinTheValidtyPeriod.XD_IsActive = true;
			TestFactory.Save();

			var existedSequenceWithinTheValidtyPeriod2 = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			existedSequenceWithinTheValidtyPeriod2.XD_SequenceClass = "NCR";
			existedSequenceWithinTheValidtyPeriod2.XD_Code = "LM5";
			existedSequenceWithinTheValidtyPeriod2.XD_StartNumber = 200;
			existedSequenceWithinTheValidtyPeriod2.XD_EndNumber = 300;
			existedSequenceWithinTheValidtyPeriod2.XD_MaximumNumberDigits = 6;
			existedSequenceWithinTheValidtyPeriod2.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.BranchDepartment;
			existedSequenceWithinTheValidtyPeriod2.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			existedSequenceWithinTheValidtyPeriod2.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			existedSequenceWithinTheValidtyPeriod2.XD_GE_Department = GlbDepartment.CurrentDepartment.PK;
			existedSequenceWithinTheValidtyPeriod2.XD_StartDate = new ZDate(2018, 07, 01);
			existedSequenceWithinTheValidtyPeriod2.XD_ExpiryDate = new ZDateTime(2018, 07, 31);
			existedSequenceWithinTheValidtyPeriod2.XD_IsActive = true;

			existedSequenceWithinTheValidtyPeriod2.Validation.ValidateXD_ExpiryDate();
			Assert(existedSequenceWithinTheValidtyPeriod2.XD_ExpiryDateInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));
			existedSequenceWithinTheValidtyPeriod2.Validation.ValidateXD_StartDate();
			Assert(existedSequenceWithinTheValidtyPeriod2.XD_StartDateInfo.HasError("An active book already exists for selected branch/department or company within the validity period with the subtype"));

			existedSequenceWithinTheValidtyPeriod2.XD_IsActive = false;
			existedSequenceWithinTheValidtyPeriod2.Validation.ValidateXD_ExpiryDate();
			Assert(!existedSequenceWithinTheValidtyPeriod2.XD_ExpiryDateInfo.HasErrors());
			existedSequenceWithinTheValidtyPeriod2.Validation.ValidateXD_StartDate();
			Assert(!existedSequenceWithinTheValidtyPeriod2.XD_StartDateInfo.HasErrors());

			existedSequenceWithinTheValidtyPeriod.XD_IsActive = false;
			TestFactory.Save();

			existedSequenceWithinTheValidtyPeriod2.XD_IsActive = false;
			existedSequenceWithinTheValidtyPeriod2.Validation.ValidateXD_ExpiryDate();
			Assert(!existedSequenceWithinTheValidtyPeriod2.XD_ExpiryDateInfo.HasErrors());
			existedSequenceWithinTheValidtyPeriod2.Validation.ValidateXD_StartDate();
			Assert(!existedSequenceWithinTheValidtyPeriod2.XD_StartDateInfo.HasErrors());
		}

		public void TestValidComplianceSequenceDateRangeForPortugalIsOneYearMinimum()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var validComplianceSequenceDateRangeForPortugalIsOneYearMinimum = @"The validity of this compliance book is less than one year.
Compliance books must be valid for at least one year to comply with Portugal tax regulations.";

				newSequence.XD_StartDate = new ZDate(2020, 01, 01);
				newSequence.XD_ExpiryDate = new ZDate(2020, 12, 30);
				newSequence.Validation.ValidateXD_StartDate();
				newSequence.Validation.ValidateXD_ExpiryDate();
				AssertEquals("The total days is less than 365 days in leap year", 364, (newSequence.XD_ExpiryDate - newSequence.XD_StartDate).Days);
				AssertHasError("Country Portugal (PT) must report error if Compliance Sequence date range is less than 365 days in leap year", newSequence.XD_StartDateInfo, validComplianceSequenceDateRangeForPortugalIsOneYearMinimum);
				AssertHasError("Country Portugal (PT) must report error if Compliance Sequence date range is less than 365 days in leap year", newSequence.XD_ExpiryDateInfo, validComplianceSequenceDateRangeForPortugalIsOneYearMinimum);

				newSequence.XD_StartDate = new ZDate(2019, 01, 01);
				newSequence.XD_ExpiryDate = new ZDate(2019, 12, 30);
				newSequence.Validation.ValidateXD_StartDate();
				newSequence.Validation.ValidateXD_ExpiryDate();
				AssertEquals("The total days is less than 364 days in non-leap year", 363, (newSequence.XD_ExpiryDate - newSequence.XD_StartDate).Days);
				AssertHasError("Country Portugal (PT) must report error if Compliance Sequence date range is less than 364 days in non-leap year", newSequence.XD_StartDateInfo, validComplianceSequenceDateRangeForPortugalIsOneYearMinimum);
				AssertHasError("Country Portugal (PT) must report error if Compliance Sequence date range is less than 364 days in non-leap year", newSequence.XD_ExpiryDateInfo, validComplianceSequenceDateRangeForPortugalIsOneYearMinimum);

				newSequence.XD_StartDate = new ZDate(2020, 01, 01);
				newSequence.XD_ExpiryDate = new ZDate(2020, 12, 31);
				newSequence.Validation.ValidateXD_StartDate();
				newSequence.Validation.ValidateXD_ExpiryDate();
				AssertEquals("The total days is equal to 365 days in leap year", 365, (newSequence.XD_ExpiryDate - newSequence.XD_StartDate).Days);
				AssertNoError("Country Portugal (PT) NOT report error if Compliance Sequence date range is equal to 365 days in leap year", newSequence.XD_StartDateInfo, validComplianceSequenceDateRangeForPortugalIsOneYearMinimum);
				AssertNoError("Country Portugal (PT) NOT report error if Compliance Sequence date range is equal to 365 days in leap year", newSequence.XD_ExpiryDateInfo, validComplianceSequenceDateRangeForPortugalIsOneYearMinimum);

				newSequence.XD_StartDate = new ZDate(2019, 01, 01);
				newSequence.XD_ExpiryDate = new ZDate(2019, 12, 31);
				newSequence.Validation.ValidateXD_StartDate();
				newSequence.Validation.ValidateXD_ExpiryDate();
				AssertEquals("The total days is equal to 364 days in non-leap year", 364, (newSequence.XD_ExpiryDate - newSequence.XD_StartDate).Days);
				AssertNoError("Country Portugal (PT) NOT report error if Compliance Sequence date range is equal to 364 days in non-leap year", newSequence.XD_StartDateInfo, validComplianceSequenceDateRangeForPortugalIsOneYearMinimum);
				AssertNoError("Country Portugal (PT) NOT report error if Compliance Sequence date range is equal to 364 days in non-leap year", newSequence.XD_ExpiryDateInfo, validComplianceSequenceDateRangeForPortugalIsOneYearMinimum);

				newSequence.XD_StartDate = new ZDate(2020, 01, 01);
				newSequence.XD_ExpiryDate = new ZDate(2021, 01, 01);
				newSequence.Validation.ValidateXD_StartDate();
				newSequence.Validation.ValidateXD_ExpiryDate();
				AssertEquals("The total days is equal to 366 days in leap year", 366, (newSequence.XD_ExpiryDate - newSequence.XD_StartDate).Days);
				AssertNoError("Country Portugal (PT) NOT report error if Compliance Sequence date range is greater than 365 days in leap year", newSequence.XD_StartDateInfo, validComplianceSequenceDateRangeForPortugalIsOneYearMinimum);
				AssertNoError("Country Portugal (PT) NOT report error if Compliance Sequence date range is greater than days in leap year", newSequence.XD_ExpiryDateInfo, validComplianceSequenceDateRangeForPortugalIsOneYearMinimum);

				newSequence.XD_StartDate = new ZDate(2019, 01, 01);
				newSequence.XD_ExpiryDate = new ZDate(2020, 01, 01);
				newSequence.Validation.ValidateXD_StartDate();
				newSequence.Validation.ValidateXD_ExpiryDate();
				AssertEquals("The total days is equal to 365 days in non-leap year", 365, (newSequence.XD_ExpiryDate - newSequence.XD_StartDate).Days);
				AssertNoError("Country Portugal (PT) NOT report error if Compliance Sequence date range is greater than 364 days in non-leap year", newSequence.XD_StartDateInfo, validComplianceSequenceDateRangeForPortugalIsOneYearMinimum);
				AssertNoError("Country Portugal (PT) NOT report error if Compliance Sequence date range is greater than 364 days in non-leap year", newSequence.XD_ExpiryDateInfo, validComplianceSequenceDateRangeForPortugalIsOneYearMinimum);
			}
		}

		public void TestDatesMandatoryInPortugal_AR()
		{
			AssertDatesMandatoryInPortugal(LedgerTypes.AccountsReceivable);
		}

		public void TestDatesMandatoryInPortugal_AP()
		{
			AssertDatesMandatoryInPortugal(LedgerTypes.AccountsPayable);
		}

		void AssertDatesMandatoryInPortugal(string ledger)
		{
			var subType = ledger == LedgerTypes.AccountsPayable ? PortugalComplianceInfo.ComplianceSubTypeCodes.SBI : PortugalComplianceInfo.ComplianceSubTypeCodes.TXI;
			var registryARValue = ledger == LedgerTypes.AccountsPayable ? AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code : AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code;
			var registryAPValue = ledger == LedgerTypes.AccountsPayable ? AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code : AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryARValue))
			using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AP.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryAPValue))
			{
				newSequence.XD_SequenceClass = subType;
				newSequence.XD_StartDate = ZDate.Empty;
				newSequence.XD_ExpiryDate = ZDate.Empty;

				AssertNullOrEmpty(newSequence.XD_StartDate.ToString());
				AssertNullOrEmpty(newSequence.XD_ExpiryDate.ToString());

				newSequence.Validation.ValidateXD_StartDate();
				AssertHasError(newSequence.XD_StartDateInfo, "Please enter a value.");

				newSequence.Validation.ValidateXD_ExpiryDate();
				AssertHasError(newSequence.XD_ExpiryDateInfo, "Please enter an Expiry Date.");
			}
		}

		public void TestCheckXD_Prefix_Argentina()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Argentina))
			{
				AssertResult(string.Empty, true);
				AssertResult("", true);
				AssertResult("0", true);
				AssertResult("00", true);
				AssertResult("000", true);
				AssertResult("0000", true);
				AssertResult("00000", true);
				AssertResult("000001", true);
				AssertResult("99999", true);
				AssertResult("999999", true);
				AssertResult("100110", true);
				AssertResult("00 00", true);
				AssertResult("-1", true);
				AssertResult("121 AA", true);
				AssertResult("arg", true);
				AssertResult("  1", true);
				AssertResult(" 455 ", true);
				AssertResult(" 01", true);
				AssertResult(" 001 ", true);
				AssertResult("@.12/", true);
				AssertResult("00013-", true);
				AssertResult("000010", true);
				AssertResult("000100", true);
				AssertResult("001000", true);
				AssertResult("010000", true);
				AssertResult("00011111", true);

				var seriesPrefix = new HashSet<string>();
				for (var i = 1; i <= 99998; i++)
				{
					var istring = i.ToString();
					for (var j = istring.Length; j < 6; j++)
					{
						seriesPrefix.Add(istring.PadLeft(j, '0'));
					}
				}

				seriesPrefix.ToList<String>().ForEach(seriePrefix => AssertResult(seriePrefix, false));
			}

			void AssertResult(string prefix, bool expectedError)
			{
				newSequence.XD_Prefix = prefix;
				if (expectedError)
				{
					AssertHasError(newSequence.XD_PrefixInfo, @"Please enter a number between 1 and 99998");
				}
				else
				{
					AssertNoErrors(newSequence.XD_PrefixInfo);
				}
			}
		}

		[TestDate(2023, 1, 1)]
		public void TestCheckXD_PrintingAuthorizationNumber_ForPortugalCountry_WhenCurrentYearLargerThan2022()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				CheckXD_PrintingAuthorizationNumber(true);
			}
		}

		[TestDate(2022, 12, 30)]
		public void TestCheckXD_PrintingAuthorizationNumber_ForPortugalCountry_WhenCurrentYearIsStill2022()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				CheckXD_PrintingAuthorizationNumber(false);
			}
		}

		[TestDate(2023, 1, 1)]
		public void TestCheckXD_PrintingAuthorizationNumber_ForNonPortugalCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Indonesia))
			{
				CheckXD_PrintingAuthorizationNumber(false);
			}
		}
		public void TestCheckXD_PrintingAuthorizationNumberValidation()
		{
			const string invalidLengthErrorMessage = "Length is invalid, it must be at least 8 characters long. Please ensure you are entering the correct Series Validation Code issued by the AT for this book.";
			const string invalidFormatErrorMessage = "Value is invalid. Only capital consonant letters and numbers from 2 to 9 are allowed. No spaces allowed. Please ensure you are entering the correct Series Validation Code issued by the AT for this book.";

			var mockICountryComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockICountryComplianceInfoBase = new Mock<ICountryComplianceInfoBase>();
			var mockITransactionAuthorizationNumber = new Mock<ITransactionAuthorizationNumber>();

			using (ObjectFactory.Substitute(mockICountryComplianceFactory.Object))
			{
				mockICountryComplianceFactory.Setup(x => x.GetITransactionAuthorizationNumber(It.IsAny<ZString>())).Returns(mockITransactionAuthorizationNumber.Object);
				mockITransactionAuthorizationNumber.Setup(x => x.IsTransactionAuthorizationNumberEnabled(It.IsAny<ZGuid>(), It.IsAny<ZDateTime>())).Returns(true);

				// Both Invalid, contains only the Length Validation error message
				mockICountryComplianceInfoBase.As<IComplianceSequenceValidationProvider>()
				.Setup(x => x.IsPrintingAuthorizationNumberLengthValid(It.IsAny<string>())).Returns(false);
				mockICountryComplianceInfoBase.As<IComplianceSequenceValidationProvider>()
							.Setup(x => x.IsPrintingAuthorizationNumberFormatValid(It.IsAny<string>())).Returns(false);
				mockICountryComplianceFactory.Setup(x => x.GetICountryComplianceInfoBase(It.IsAny<ZString>()))
							.Returns(mockICountryComplianceInfoBase.Object);

				var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				sequence.XD_PrintingAuthorizationNumber = "anyValue";
				sequence.RunPreSaveValidation();
				AssertEquals(expected: true, sequence.XD_PrintingAuthorizationNumberInfo.HasError(invalidLengthErrorMessage));
				AssertEquals(expected: false, sequence.XD_PrintingAuthorizationNumberInfo.HasError(invalidFormatErrorMessage));

				// Both Valid, constains no Validation error
				mockICountryComplianceInfoBase.As<IComplianceSequenceValidationProvider>()
							.Setup(x => x.IsPrintingAuthorizationNumberLengthValid(It.IsAny<string>())).Returns(true);
				mockICountryComplianceInfoBase.As<IComplianceSequenceValidationProvider>()
							.Setup(x => x.IsPrintingAuthorizationNumberFormatValid(It.IsAny<string>())).Returns(true);

				sequence.RunPreSaveValidation();
				AssertEquals(expected: false, sequence.XD_PrintingAuthorizationNumberInfo.HasError(invalidLengthErrorMessage));
				AssertEquals(expected: false, sequence.XD_PrintingAuthorizationNumberInfo.HasError(invalidFormatErrorMessage));

				//Invalid length only, contains only the Length Validation error message
				mockICountryComplianceInfoBase.As<IComplianceSequenceValidationProvider>()
							.Setup(x => x.IsPrintingAuthorizationNumberLengthValid(It.IsAny<string>())).Returns(false);
				mockICountryComplianceInfoBase.As<IComplianceSequenceValidationProvider>()
							.Setup(x => x.IsPrintingAuthorizationNumberFormatValid(It.IsAny<string>())).Returns(true);

				sequence.RunPreSaveValidation();
				AssertEquals(expected: true, sequence.XD_PrintingAuthorizationNumberInfo.HasError(invalidLengthErrorMessage));
				AssertEquals(expected: false, sequence.XD_PrintingAuthorizationNumberInfo.HasError(invalidFormatErrorMessage));

				//Invalid format only, contains only the Format Validation error message
				mockICountryComplianceInfoBase.As<IComplianceSequenceValidationProvider>()
							.Setup(x => x.IsPrintingAuthorizationNumberLengthValid(It.IsAny<string>())).Returns(true);
				mockICountryComplianceInfoBase.As<IComplianceSequenceValidationProvider>()
							.Setup(x => x.IsPrintingAuthorizationNumberFormatValid(It.IsAny<string>())).Returns(false);

				sequence.RunPreSaveValidation();
				AssertEquals(expected: false, sequence.XD_PrintingAuthorizationNumberInfo.HasError(invalidLengthErrorMessage));
				AssertEquals(expected: true, sequence.XD_PrintingAuthorizationNumberInfo.HasError(invalidFormatErrorMessage));
			}
		}

		void CheckXD_PrintingAuthorizationNumber(bool shouldContainError)
		{
			var complianceBookHasBeenUsedErrorMessage = "Editing or deleting the ATCUD code is no longer possible after the compliance book has been used";
			var complianceBookWithEmptyPrintingAuthorizationNumberErrorMessage = "The ATCUD code is mandatory in the compliance books, fill it in the 'Print authorization number' field";

			//Not In DB
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.RunPreSaveValidation();
			AssertNullOrEmpty("Percondition", sequence.XD_PrintingAuthorizationNumber);
			AssertEquals(shouldContainError, sequence.XD_PrintingAuthorizationNumberInfo.HasError(complianceBookWithEmptyPrintingAuthorizationNumberErrorMessage));
			sequence.XD_PrintingAuthorizationNumber = "Test";
			AssertEquals(false, sequence.XD_PrintingAuthorizationNumberInfo.HasError(complianceBookWithEmptyPrintingAuthorizationNumberErrorMessage));

			//In DB & XD_PrintingAuthorizationNumberInfo.HasChanges
			Factory.Save();
			AssertEquals("Percondition", true, sequence.IsInDatabase);
			AssertEquals("Percondition", false, sequence.PrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference);
			sequence.XD_PrintingAuthorizationNumber = ZString.Empty;
			AssertEquals(true, sequence.XD_PrintingAuthorizationNumberInfo.HasChanges);
			AssertEquals(shouldContainError, sequence.XD_PrintingAuthorizationNumberInfo.HasError(complianceBookWithEmptyPrintingAuthorizationNumberErrorMessage));

			//Used In AccTransactionHeader, and headerReference is ATH
			var newFactory = new BusinessObjectFactory();
			var arInvoice1 = newFactory.NewWithValidTestData<AccTransactionHeader>();
			arInvoice1.AH_Ledger = LedgerTypes.AccountsReceivable;
			arInvoice1.AH_TransactionType = TransactionTypes.Invoice;
			arInvoice1.AH_XD_ComplianceBook = sequence.PK;
			var headerReference1 = newFactory.NewWithValidTestData<AccTransactionHeaderReference>();
			headerReference1.AH1_AH = arInvoice1.PK;
			headerReference1.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ATH;
			headerReference1.AH1_Reference = "Test001";
			newFactory.Save();
			AssertEquals("Percondition", true, sequence.PrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference);
			sequence.RunPreSaveValidation();
			AssertEquals(shouldContainError, sequence.XD_PrintingAuthorizationNumberInfo.HasError(complianceBookHasBeenUsedErrorMessage));

			//Not Used In AccTransactionHeader
			var sequence2 = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence2.XD_PrintingAuthorizationNumber = "SequenceNotUsed";
			Factory.Save();
			AssertEquals("Percondition", true, sequence2.IsInDatabase);
			AssertEquals("Percondition", false, sequence2.PrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference);
			sequence2.XD_PrintingAuthorizationNumber = ZString.Empty;
			AssertEquals(false, sequence2.XD_PrintingAuthorizationNumberInfo.HasError(complianceBookHasBeenUsedErrorMessage));
			AssertEquals(shouldContainError, sequence2.XD_PrintingAuthorizationNumberInfo.HasError(complianceBookWithEmptyPrintingAuthorizationNumberErrorMessage));

			//Used In AccTransactionHeader, but Transaction has no headerReference
			var arInvoice2 = newFactory.NewWithValidTestData<AccTransactionHeader>();
			arInvoice2.AH_Ledger = LedgerTypes.AccountsReceivable;
			arInvoice2.AH_TransactionType = TransactionTypes.Invoice;
			arInvoice2.AH_XD_ComplianceBook = sequence2.PK;
			newFactory.Save();
			AssertEquals("Percondition", false, sequence2.PrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference);
			sequence2.RunPreSaveValidation();
			AssertEquals(false, sequence2.XD_PrintingAuthorizationNumberInfo.HasError(complianceBookHasBeenUsedErrorMessage));
			AssertEquals(shouldContainError, sequence2.XD_PrintingAuthorizationNumberInfo.HasError(complianceBookWithEmptyPrintingAuthorizationNumberErrorMessage));

			//Used In AccTransactionHeader, but headerReference is not ATH
			var headerReference2 = newFactory.NewWithValidTestData<AccTransactionHeaderReference>();
			headerReference2.AH1_AH = arInvoice2.PK;
			headerReference2.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.IRR;
			headerReference2.AH1_Reference = "Test001";
			newFactory.Save();
			AssertEquals("Percondition", false, sequence2.PrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference);
			sequence2.RunPreSaveValidation();
			AssertEquals(false, sequence2.XD_PrintingAuthorizationNumberInfo.HasError(complianceBookHasBeenUsedErrorMessage));
			AssertEquals(shouldContainError, sequence2.XD_PrintingAuthorizationNumberInfo.HasError(complianceBookWithEmptyPrintingAuthorizationNumberErrorMessage));

			headerReference2.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ATH;
			newFactory.Save();
			AssertEquals("Percondition", true, sequence2.PrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference);
			sequence2.RunPreSaveValidation();
			AssertEquals(shouldContainError, sequence2.XD_PrintingAuthorizationNumberInfo.HasError(complianceBookHasBeenUsedErrorMessage));
			AssertEquals(false, sequence2.XD_PrintingAuthorizationNumberInfo.HasError(complianceBookWithEmptyPrintingAuthorizationNumberErrorMessage));
		}

		#region Implementation

		void AssertOverlap(string message, AccComplianceSequence sequence)
		{
			AssertCollectionContains(message, "Number series and prefix is not unique for the compliance sub type", newSequence.XD_PrefixInfo.GetErrors().Select(x => x.Message));
		}

		void AssertNotOverlap(string message, AccComplianceSequence sequence)
		{
			AssertCollectionNotContains(message, "Number series and prefix is not unique for the compliance sub type", newSequence.XD_PrefixInfo.GetErrors().Select(x => x.Message));
		}

		#endregion
	}
}
