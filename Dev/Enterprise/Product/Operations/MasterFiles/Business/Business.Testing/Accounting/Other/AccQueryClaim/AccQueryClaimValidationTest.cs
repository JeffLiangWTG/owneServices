using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AccQueryClaimValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAY_QueryClaimNextFollowUp()
		{
			QueryClaim.AY_QueryClaimNextFollowUp = ZDateTime.Today.AddDays(1);
			Assert("Next Follow Up should not have errors!", !QueryClaim.AY_QueryClaimNextFollowUpInfo.HasErrors());

			QueryClaim.AY_QueryClaimNextFollowUp = ZDateTime.Invalid;
			Assert("Next Follow Up is invalid!", QueryClaim.AY_QueryClaimNextFollowUpInfo.HasErrors());
		}

		public void TestCheckAY_QueryClaimReasonCode()
		{
			QueryClaim.AY_QueryClaimReasonCode = "xxx";
			Assert("Invalid Claim Reason Code!", QueryClaim.AY_QueryClaimReasonCodeInfo.HasErrors());

			QueryClaim.AY_QueryClaimReasonCode = QueryClaim.Lookups.ClaimReason[0].Code;
			Assert("Claim Reason Code should not have errors", !QueryClaim.AY_QueryClaimReasonCodeInfo.HasErrors());

			QueryClaim.AY_QueryClaimReasonCode = ZString.Empty;
			Assert("Invalid Claim Reason Code!", QueryClaim.AY_QueryClaimReasonCodeInfo.HasErrors());
		}

		public void TestCheckAY_QueryClaimStatus()
		{
			QueryClaim.AY_QueryClaimStatus = "xxx";
			Assert("Invalid Claim Status !", QueryClaim.AY_QueryClaimStatusInfo.HasErrors());

			QueryClaim.AY_QueryClaimStatus = QueryClaim.Lookups.ClaimStatus[0].Code;
			Assert("Claim Status should not have errors", !QueryClaim.AY_QueryClaimStatusInfo.HasErrors());

			QueryClaim.AY_QueryClaimStatus = ZString.Empty;
			Assert("Invalid Claim Status !", QueryClaim.AY_QueryClaimStatusInfo.HasErrors());
		}

		public void TestCheckAY_QueryClaimType()
		{
			QueryClaim.AY_QueryClaimType = "xxx";
			Assert("Invalid Claim Type!", QueryClaim.AY_QueryClaimTypeInfo.HasErrors());

			QueryClaim.AY_QueryClaimType = QueryClaim.Lookups.ClaimType[0].Code;
			Assert("Claim Type should not have errors", !QueryClaim.AY_QueryClaimTypeInfo.HasErrors());

			QueryClaim.AY_QueryClaimType = ZString.Empty;
			Assert("Invalid Claim Type!", QueryClaim.AY_QueryClaimTypeInfo.HasErrors());
		}

		public void TestCheckAY_ShortDescriptionOfClaim()
		{
			QueryClaim.AY_ShortDescriptionOfClaim = ZString.Empty;
			AssertNotNull("Short Description should be not null", QueryClaim.AY_ShortDescriptionOfClaimInfo.HasErrors());

			QueryClaim.AY_ShortDescriptionOfClaim = "xxx";
			AssertNotNull("Short Description not null", !QueryClaim.AY_ShortDescriptionOfClaimInfo.HasErrors());
		}

		public void TestCheckAY_GB()
		{
			ZQuery branchesFilter = new ZQuery(GlbBranchSchema.GB_GC, Env.CurrentCompany.PK);
			QueryClaim.AY_GB = Factory.LoadTop1(typeof(GlbBranch), branchesFilter).PK;
			Assert("Branch should not have errors!", !QueryClaim.AY_GBInfo.HasErrors());

			QueryClaim.AY_GB = ZGuid.NewZGuid();
			Assert("Invalid branch code!", QueryClaim.AY_GBInfo.HasErrors());
		}

		public void TestCheckAY_OC()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = Factory.New<OrgContact>();
			contact.OC_OH = org.PK;
			Assert("Contact should not have errors!", !QueryClaim.AY_OCInfo.HasErrors());

			QueryClaim.AY_OC = ZGuid.NewZGuid();
			Assert("Invalid contact code!", QueryClaim.AY_OCInfo.HasErrors());
		}

		public void TestCheckAY_GS_NKStaffAssignedTo()
		{
			QueryClaim.AY_GS_NKStaffAssignedTo = "ABC";
			Assert("Invalid staff member code", QueryClaim.AY_GS_NKStaffAssignedToInfo.HasErrors());

			QueryClaim.AY_GS_NKStaffAssignedTo = (Factory.LoadTop1<GlbStaff>(new ZQuery())).GS_Code;
			Assert("Staff member should not have errors", !QueryClaim.AY_GS_NKStaffAssignedToInfo.HasErrors());
		}

		public void TestCheckAY_AH()
		{
			ZQuery branchesFilter = new ZQuery(GlbBranchSchema.GB_GC, Env.CurrentCompany.PK);
			ZGuid branchPK = Factory.LoadTop1(typeof(GlbBranch), branchesFilter).PK;

			OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery());

			CheckAY_AHFiltering(QueryClaim.Ledger == LedgerTypes.AccountsPayable ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable, TransactionTypes.Invoice, branchPK, org.PK);
			Assert("Invalid Invoice - Invalid Ledger Type", QueryClaim.AY_AHInfo.HasErrors());

			CheckAY_AHFiltering(QueryClaim.Ledger, TransactionTypes.CreditNote, branchPK, org.PK);
			Assert("Invalid Invoice - Invalid Transaction Type", QueryClaim.AY_AHInfo.HasErrors());

			QueryClaim.AY_AH = ZGuid.NewZGuid();
			Assert("Invalid invoice number", QueryClaim.AY_AHInfo.HasErrors());

			CheckAY_AHFiltering(QueryClaim.Ledger, TransactionTypes.Invoice, branchPK, org.PK);
			Assert("Invoice number should not have errors", !QueryClaim.AY_AHInfo.HasErrors());

			AssertNotNull("Transaction Headers not null", QueryClaim.Lookups.TransactionHeaders);
		}

		public void TestCheckAY_AH_InvoiceNumberReferenceUnique()
		{
			var branchPK = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, Env.CurrentCompany.PK)).PK;
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var transactionInDB = CreateTransactionHeader(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, branchPK, org.PK);
			var queryClaimInDB = (AccQueryClaim)Factory.New<Enterprise.Integration.Accounting.IARAccQueryClaim>();
			queryClaimInDB.FillWithValidTestData();
			queryClaimInDB.AY_AH = transactionInDB.PK;
			Factory.Save();
			ReleaseFactory();

			var queryClaim = (AccQueryClaim)Factory.New<Enterprise.Integration.Accounting.IARAccQueryClaim>();
			queryClaim.FillWithValidTestData();
			queryClaim.AY_AH = transactionInDB.PK;
			AssertHasError(queryClaim.AY_AHInfo, "Each transaction can only be associated with one claim.  This transaction cannot be chosen because it is already linked to a Claim.");

			var transaction = CreateTransactionHeader(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, branchPK, org.PK);
			AssertEquals("Precondition: transaction is not in db", false, transaction.IsInDatabase);
			queryClaim.AY_AH = transaction.PK;
			AssertNoErrors(queryClaim.AY_AHInfo);
			AssertNoErrors(queryClaimInDB.AY_AHInfo);

			var queryClaimInDBInCurrentFactory = (AccQueryClaim)Factory.Load(queryClaimInDB.GetType(), queryClaimInDB.PK);
			queryClaimInDBInCurrentFactory.AY_AH = transaction.PK;
			AssertHasError(queryClaimInDBInCurrentFactory.AY_AHInfo, "Each transaction can only be associated with one claim.  This transaction cannot be chosen because it is already linked to a Claim.");

			queryClaim.Validation.ValidateAY_AH();
			AssertHasError(queryClaim.AY_AHInfo, "Each transaction can only be associated with one claim.  This transaction cannot be chosen because it is already linked to a Claim.");
		}

		public void TestCheckAY_AHMandatory()
		{
			ZQuery branchesFilter = new ZQuery(GlbBranchSchema.GB_GC, Env.CurrentCompany.PK);
			ZGuid branchPK = Factory.LoadTop1(typeof(GlbBranch), branchesFilter).PK;

			OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery());

			QueryClaim.Validation.ValidateAY_AH();
			AssertHasErrors("Should be mandatory validation error on invoice", QueryClaim.AY_AHInfo);
			QueryClaim.AY_OH_Debtor = Factory.NewWithValidTestData<OrgHeader>().PK;
			QueryClaim.AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;

			QueryClaim.Factory.Save();
			QueryClaim.Validation.ValidateAY_AH();
			AssertNoErrors("Mandatory validation should not validate because the original value is also empty", QueryClaim.AY_AHInfo);
		}

		void CheckAY_AHFiltering(ZGuid branch, ZGuid debtor, AccQueryClaim claim, AccTransactionHeader transactionHeader)
		{
			claim.AY_GB = branch;
			claim.AY_OH_Debtor = debtor;
			claim.AY_AH = transactionHeader.PK;
		}

		void CheckAY_AHFiltering(ZString ledgerType, ZString transactionType, ZGuid branch, ZGuid debtor)
		{
			CheckAY_AHFiltering(branch, debtor, QueryClaim, CreateTransactionHeader(ledgerType, transactionType, branch, debtor));
			QueryClaim.AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;
			Factory.Save();
			QueryClaim.Lookups.TransactionHeaders.Load();
			QueryClaim.Validation.ValidateAY_AH();
		}

		AccTransactionHeader CreateTransactionHeader(ZString ledgerType, ZString transactionType, ZGuid branch, ZGuid debtor)
		{
			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = ledgerType;
			header.AH_TransactionType = transactionType;
			header.AH_GB = branch;
			header.AH_OH = debtor;

			return header;
		}

		public void TestCheckAY_AH_ClosedClaim()
		{
			var branchPK = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, Env.CurrentCompany.PK)).PK;
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var contact = Factory.NewWithValidTestData<OrgContact>();

			var transactionInDB = CreateTransactionHeader(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, branchPK, org.PK);
			var queryClaimInDB1 = (AccQueryClaim)Factory.New<Enterprise.Integration.Accounting.IARAccQueryClaim>();
			queryClaimInDB1.FillWithValidTestData();
			queryClaimInDB1.AY_OH_Debtor = org.PK;
			queryClaimInDB1.AY_OC = contact.PK;
			queryClaimInDB1.AY_QueryClaimStatus = QueryClaimStatusCodeList.Codes.QCStatus6CancelledAndClosed;
			queryClaimInDB1.AY_AH = transactionInDB.PK;
			var queryClaimInDB2 = (AccQueryClaim)Factory.New<Enterprise.Integration.Accounting.IARAccQueryClaim>();
			queryClaimInDB2.FillWithValidTestData();
			queryClaimInDB2.AY_OH_Debtor = org.PK;
			queryClaimInDB2.AY_OC = contact.PK;
			queryClaimInDB2.AY_AH = transactionInDB.PK;
			Factory.Save();
			ReleaseFactory();

			queryClaimInDB1.Validation.ValidateAY_AH();
			AssertNoErrors("Shouldn't have any error, because this closed claim is no need to validate.", queryClaimInDB1.AY_AHInfo);

			queryClaimInDB2.Validation.ValidateAY_AH();
			AssertNoErrors("Shouldn't have any error, because other claims are closed.", queryClaimInDB2.AY_AHInfo);
		}

		public void TestCheckAY_OH_Debtor()
		{
			OrgHeaderCollection organisations = GetOrganisationsCollection();
			organisations.Load();
			QueryClaim.AY_OH_Debtor = organisations[0].PK;
			Assert("Should not have errors", !QueryClaim.AY_OH_DebtorInfo.HasErrors());

			QueryClaim.AY_OH_Debtor = ZGuid.Empty;
			Assert("Empty debtor code", QueryClaim.AY_OH_DebtorInfo.HasErrors());

			QueryClaim.AY_OH_Debtor = ZGuid.NewZGuid();
			Assert("Invalid debtor code", QueryClaim.AY_OH_DebtorInfo.HasErrors());
		}

		public void TestCheckAY_QueryClaimAmount()
		{
			QueryClaim.AY_QueryClaimAmount = 0;
			AssertHasErrors(QueryClaim.AY_QueryClaimAmountInfo);

			QueryClaim.AY_QueryClaimAmount = 10;
			AssertNoErrors(QueryClaim.AY_QueryClaimAmountInfo);

			QueryClaim.FillWithValidTestData();
			QueryClaim.AY_QueryClaimAmount = 0;
			QueryClaim.Factory.Save();

			QueryClaim.RunPreSaveValidation();
			AssertNoErrors("Old claims shouldn't be validated for Amount as this will prevent the organisation from being saved.", QueryClaim.AY_QueryClaimAmountInfo);
		}

		public void TestCheckAY_HoldOption()
		{
			QueryClaim.FillWithValidTestData();
			QueryClaim.RunPreSaveValidation();
			AssertNoNotifications(QueryClaim.AY_HoldOptionInfo);

			QueryClaim.AY_HoldOption = "xxx";
			QueryClaim.RunPreSaveValidation();
			AssertHasError(QueryClaim.AY_HoldOptionInfo, "Enter a valid Invoice Hold Option.");

			QueryClaim.AY_HoldOption = null;
			QueryClaim.RunPreSaveValidation();
			AssertHasError(QueryClaim.AY_HoldOptionInfo, "Please enter an Invoice Hold Option.");
		}

		#region Implementation

		AccQueryClaim fQueryClaim;
		AccQueryClaim QueryClaim
		{
			get
			{
				if (fQueryClaim == null)
				{
					fQueryClaim = (AccQueryClaim)Factory.New(GetExpectedParentType());
				}
				return fQueryClaim;
			}
		}

		protected virtual Type GetExpectedParentType()
		{
			return ObjectFactory.GetType<Enterprise.Integration.Accounting.IARAccQueryClaim>();
		}

		protected virtual OrgHeaderCollection GetOrganisationsCollection()
		{
			return QueryClaim.Lookups.Debtors;
		}

		#endregion
	}
}
