using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting.EPayment;
using static Enterprise.MasterFiles.Business.AccEPaymentStaffTokenLookups;

namespace Enterprise.MasterFiles.Business.Testing
{
	class AccEPaymentStaffTokenValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTK_AB()
		{
			staffToken.TK_AB = ZGuid.Empty;
			AssertHasError(staffToken.TK_ABInfo, "Please enter a value.");
			staffToken.TK_AB = ZGuid.NewZGuid();
			AssertHasError(staffToken.TK_ABInfo, "Token must be used for a valid bank account.");
			var bankAccount = Factory.New<AccBankAccount>();
			staffToken.TK_AB = bankAccount.PK;
			AssertNoErrors(staffToken.TK_ABInfo);
		}

		public void TestCheckTK_GC()
		{
			staffToken.TK_GC = ZGuid.Empty;
			AssertHasError(staffToken.TK_GCInfo, "Please enter a value.");
			staffToken.TK_GC = ZGuid.NewZGuid();
			AssertHasError(staffToken.TK_GCInfo, "Token must specify a valid Company.");
			var company = Factory.New<GlbCompany>();
			staffToken.TK_GC = company.PK;
			AssertNoErrors(staffToken.TK_GCInfo);
		}

		public void TestCheckTK_Status()
		{
			staffToken.TK_Status = ZString.Empty;
			AssertHasError(staffToken.TK_StatusInfo, "Please enter a Status.");
			staffToken.TK_Status = "AAA";
			AssertHasError(staffToken.TK_StatusInfo, "Enter a valid Status.");
			AssertWithStatusIndicatingIsAuthorised(() => AssertNoErrors(staffToken.TK_StatusInfo));
			AssertWithStatusIndicatingNotAuthorised(() => AssertNoErrors(staffToken.TK_StatusInfo));
		}

		public void TestCheckTK_ExpiryUtc()
		{
			AssertWithStatusIndicatingIsAuthorised(() =>
			{
				staffToken.TK_ExpiryUtc = ZDateTime.Empty;
				AssertHasError(staffToken.TK_ExpiryUtcInfo, "Expiry date/Time must be recorded if the token is in authorized state.");
				staffToken.TK_RequestedUtc = ZDateTime.Today;
				staffToken.TK_ExpiryUtc = ZDateTime.Today.AddDays(-1);
				AssertHasError(staffToken.TK_ExpiryUtcInfo, "Expiry date/Time cannot be earlier than the token requested date/time.");

				staffToken.TK_ExpiryUtc = ZDateTime.Today;
				AssertNoErrors(staffToken.TK_ExpiryUtcInfo);
				staffToken.TK_ExpiryUtc = ZDateTime.Today.AddDays(1);
				AssertNoErrors(staffToken.TK_ExpiryUtcInfo);
			});

			AssertWithStatusIndicatingNotAuthorised(() =>
			{
				staffToken.TK_ExpiryUtc = ZDateTime.Today.AddDays(1);
				AssertHasError(staffToken.TK_ExpiryUtcInfo, "Expiry date/Time can only be set if the token is in authorized state.");
				staffToken.TK_ExpiryUtc = ZDateTime.Empty;
				AssertNoErrors(staffToken.TK_ExpiryUtcInfo);
			});
		}

		public void TestCheckTK_ErrorDescription()
		{
			foreach (var code in staffToken.Lookups.StatusCodeList.GetAllCodes())
			{
				staffToken.TK_Status = code;
				staffToken.TK_ErrorDescription = ZString.Empty;
				AssertNoErrors(staffToken.TK_ErrorDescriptionInfo);

				staffToken.TK_ErrorDescription = "Heyo an error happened";
				if (code == StatusCodes.Error)
				{
					AssertNoErrors(staffToken.TK_ErrorDescriptionInfo);
				}
				else
				{
					AssertHasError(staffToken.TK_ErrorDescriptionInfo, "Error Description should only be recorded if the status is ERR.");
				}
			}
		}

		public void TestCheckTK_GS_NKStaffCode()
		{
			var bankAccountData = Factory.NewWithValidTestData<AccBankAccount>();
			var collection = new AccEPaymentStaffTokenDependentCollection(bankAccountData);
			var staffToken = collection.AddNew();
			staffToken.TK_AB = bankAccountData.PK;
			staffToken.TK_GS_NKStaffCode = "ZZZ";
			AssertHasError(staffToken.TK_GS_NKStaffCodeInfo, "Enter a valid Staff Code.");

			staffToken.TK_GS_NKStaffCode = "";
			AssertHasError(staffToken.TK_GS_NKStaffCodeInfo, "Please enter a Staff Code.");

			staffToken.TK_GS_NKStaffCode = Factory.LoadTop1<GlbStaff>(new ZQuery()).GS_Code;
			AssertNoErrors(staffToken.TK_GS_NKStaffCodeInfo);

			var staffToken2 = collection.AddNew();
			staffToken2.TK_AB = bankAccountData.PK;
			staffToken2.TK_GS_NKStaffCode = staffToken.TK_GS_NKStaffCode;
			AssertHasError(staffToken2.TK_GS_NKStaffCodeInfo, "Authorization record with the same CW1 staff code already exists.");

			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			staffToken2.TK_GS_NKStaffCode = newStaff.GS_Code;
			AssertNoErrors(staffToken.TK_GS_NKStaffCodeInfo);
		}

		void AssertWithStatusIndicatingIsAuthorised(Action testToRun)
		{
			foreach (var code in (new[] { StatusCodes.Authorised }))
			{
				staffToken.TK_Status = code;
				staffToken.Validation.ValidateAll();
				testToRun();
			}
		}

		void AssertWithStatusIndicatingNotAuthorised(Action testToRun)
		{
			foreach (var code in (new[] { StatusCodes.NotAuthorised, StatusCodes.Pending, StatusCodes.Error }))
			{
				staffToken.TK_Status = code;
				staffToken.Validation.ValidateAll();
				testToRun();
			}
		}

		protected AccEPaymentStaffToken staffToken;

		protected virtual AccEPaymentStaffToken GetNewStaffToken()
		{
			return Factory.New<AccEPaymentStaffToken>();
		}

		protected override void SetUp()
		{
			base.SetUp();
			staffToken = GetNewStaffToken();
		}
	}
}
