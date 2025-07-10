using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class CreditControlledBizo : DummyEnterpriseBusinessObject, ICreditControlledDocumentDelivery, ICreditControlledNotificationTextProvider
	{
		public CreditControlledBizo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public OrgHeader[] OrganisationsForCreditChecksForTest = Array.Empty<OrgHeader>();

		OrgHeader[] ICreditControlledDocumentDelivery.OrganisationsForCreditChecks
		{
			get { return OrganisationsForCreditChecksForTest; }
		}

		public bool IsDPSFreightMovementRestrictedForTest;

		bool ICreditControlledDocumentDelivery.IsDPSFreightMovementRestricted
		{
			get { return IsDPSFreightMovementRestrictedForTest; }
		}

		public bool IsAviationSecurityFreightMovementRestrictedTest;

		bool ICreditControlledDocumentDelivery.IsAviationSecurityFreightMovementRestricted => IsAviationSecurityFreightMovementRestrictedTest;

		public ScreeningParty[] GetScreeningParties()
		{
			return DPSPartiesForTest;
		}

		public ScreeningParty[] DPSPartiesForTest = Array.Empty<ScreeningParty>();

		void ICreditControlledDocumentDelivery.RaiseOnGetDocumentLogin(SecurityLoginEventArgs e)
		{
			if (getDocumentLogin != null)
			{
				getDocumentLogin(this, e);
			}
		}

		event EventHandler<SecurityLoginEventArgs> ICreditControlledDocumentDelivery.GetDocumentLogin
		{
			add { getDocumentLogin += value; }
			remove { getDocumentLogin -= value; }
		}
		event EventHandler<SecurityLoginEventArgs> getDocumentLogin;

		CustomMessageBoxCallback ICreditControlledDocumentDelivery.DocumentLoginMessageBoxCallback
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		string[] IRelatedJobNumber.JobNumber
		{
			get { return new string[] { "123" }; }
		}

		string ICreditControlledDocumentDelivery.DescriptionOfOrganisationBeingCheckedForCredit
		{
			get { return "Consignee, Consignor or Local Client for Billing"; }
		}

		#region Test helper methods

		public static void SetupCreditControllerOverrideThreshold(ZString authorisationRequirement)
		{
			var collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			collection.Add(new AmountOrPercentageBasedThreeLevelAuthorisationRequirement() { Amount = 0, Percentage = 0, Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AuthorisationRequirement = authorisationRequirement });
			AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
		}

		public static void ClearCreditControllerOverrideThreshold()
		{
			var collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
		}

		public static OrgHeader CreateOrganization(BusinessObjectFactory factory, string orgCode = null)
		{
			var newFactory = new BusinessObjectFactory();
			var organisation = newFactory.NewWithValidTestData<OrgHeader>();
			organisation.OH_IsDebtor = true;
			if (orgCode != null)
			{
				organisation.OH_Code = orgCode;
			}

			newFactory.Save();

			return factory.Load<OrgHeader>(organisation.PK);
		}

		public static void SetOrganizationCreditOnHold(OrgHeader organisation, ZBool isCreditOnHold)
		{
			var newFactory = new BusinessObjectFactory();
			newFactory.Load<OrgHeader>(organisation.PK).CompanyData.OB_AROnCreditHold = isCreditOnHold;

			newFactory.Save();

			organisation.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
		}

		public static void SetOrganizationCredit(OrgHeader organisation, ZDecimal creditAmount, decimal creditLimit = -1)
		{
			var newFactory = new BusinessObjectFactory();
			if (creditLimit != -1)
			{
				newFactory.Load<OrgHeader>(organisation.PK).CompanyData.OB_ARCreditLimit = creditLimit;
			}

			var transaction = newFactory.New<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.Payment;
			transaction.AH_InvoiceDate = ZDateTime.Today;
			transaction.AH_InvoiceAmount = transaction.AH_OutstandingAmount = creditAmount;
			transaction.AH_GC = GlbCompany.CurrentCompany.PK;
			transaction.AH_GB = GlbBranch.CurrentBranch.PK;
			transaction.AH_GE = GlbDepartment.CurrentDepartment.PK;
			transaction.AH_OH = organisation.PK;
			transaction.AH_TransactionNum = "VALUEFORTEST";

			newFactory.Save();

			organisation.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
		}

		#endregion

		#region ICreditControlledNotificationTextProvider Members

		public bool UseICreditControlledNotificationTextProvider;

		MultilingualString ICreditControlledNotificationTextProvider.NotificationHeaderText
		{
			get
			{
				if (UseICreditControlledNotificationTextProvider)
				{
					return (NoResString)"Test Start Words";
				}
				return null;
			}
		}

		MultilingualString ICreditControlledNotificationTextProvider.NotificationConfirmationText
		{
			get
			{
				if (UseICreditControlledNotificationTextProvider)
				{
					return (NoResString)"Test Contact Message";
				}
				return null;
			}
		}

		#endregion
	}
}
