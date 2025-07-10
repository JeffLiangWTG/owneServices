using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.CountryCompliance.Portugal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Testing.Accounting.Helpers
{
	sealed class PortugalValidatorHelperTest : TestCaseWithFactory
	{
		public void TestChargeCodeDescriptionComplianceRequirements()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				chargeCode.AC_Desc = "A";
				AssertHasError(chargeCode.AC_DescInfo, "Length is invalid, it must be at least 2 characters long. Please ensure you are entering the correct description for this Charge code.");

				var exception = AssertExceptionThrown<MaxLengthExceededException>(() => chargeCode.AC_Desc = new string('A', chargeCode.AC_DescInfo.MaxLength + 1));
				AssertContains("The maximum length of this property is 80 characters, but 81 were entered. New value: AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA. Old value: A", exception.Message);
				ErrorReporter.Clear();

				chargeCode.AC_Desc = new string('A', chargeCode.AC_DescInfo.MaxLength);
				AssertNoErrors(chargeCode.AC_DescInfo);

				chargeCode.AC_Desc = "1";
				AssertHasError(chargeCode.AC_DescInfo, "Length is invalid, it must be at least 2 characters long. Please ensure you are entering the correct description for this Charge code.");

				chargeCode.AC_Desc = "陆";
				AssertHasError(chargeCode.AC_DescInfo, "Description only accepts Western European languages characters.");

				chargeCode.AC_Desc = "á";
				AssertHasError(chargeCode.AC_DescInfo, "Length is invalid, it must be at least 2 characters long. Please ensure you are entering the correct description for this Charge code.");

				chargeCode.AC_Desc = "áá";
				AssertNoErrors(chargeCode.AC_DescInfo);

				chargeCode.AC_Desc = "A1";
				AssertHasError(chargeCode.AC_DescInfo, "Value is invalid. Numbers and special characters only are allowed if description is longer than 2 characters and cannot be only special characters or numbers. Please ensure you are entering the correct description for this Charge code.");

				chargeCode.AC_Desc = "A12";
				AssertNoErrors(chargeCode.AC_DescInfo);

				chargeCode.AC_Desc = "12";
				AssertHasError(chargeCode.AC_DescInfo, "Value is invalid. Numbers and special characters only are allowed if description is longer than 2 characters and cannot be only special characters or numbers. Please ensure you are entering the correct description for this Charge code.");

				chargeCode.AC_Desc = "12m";
				AssertNoErrors(chargeCode.AC_DescInfo);

				chargeCode.AC_Desc = "123";
				AssertHasError(chargeCode.AC_DescInfo, "Value is invalid. Numbers and special characters only are allowed if description is longer than 2 characters and cannot be only special characters or numbers. Please ensure you are entering the correct description for this Charge code.");

				chargeCode.AC_Desc = "";
				AssertHasError(chargeCode.AC_DescInfo, "Please enter a Description.");

				chargeCode.AC_Desc = "C.";
				AssertHasError(chargeCode.AC_DescInfo, "Value is invalid. Numbers and special characters only are allowed if description is longer than 2 characters and cannot be only special characters or numbers. Please ensure you are entering the correct description for this Charge code.");

				chargeCode.AC_Desc = "C.D";
				AssertNoErrors(chargeCode.AC_DescInfo);

				chargeCode.AC_Desc = "1.";
				AssertHasError(chargeCode.AC_DescInfo, "Value is invalid. Numbers and special characters only are allowed if description is longer than 2 characters and cannot be only special characters or numbers. Please ensure you are entering the correct description for this Charge code.");

				chargeCode.AC_Desc = "1&3";
				AssertHasError(chargeCode.AC_DescInfo, "Value is invalid. Numbers and special characters only are allowed if description is longer than 2 characters and cannot be only special characters or numbers. Please ensure you are entering the correct description for this Charge code.");

				chargeCode.AC_Desc = "1&3á";
				AssertNoErrors(chargeCode.AC_DescInfo);

				chargeCode.AC_Desc = "&$#";
				AssertHasError(chargeCode.AC_DescInfo, "Value is invalid. Numbers and special characters only are allowed if description is longer than 2 characters and cannot be only special characters or numbers. Please ensure you are entering the correct description for this Charge code.");

				chargeCode.AC_IsActive = false;
				chargeCode.AC_Desc = "???";
				AssertNoErrors("validation does not apply to inactive charge codes", chargeCode.AC_DescInfo);
			}

			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_Desc = "A";
			AssertNoErrors("validation does not apply outside of Portugal", chargeCode2.AC_DescInfo);
		}

		public void TestChargeCodeDescriptionComplianceRequirements_NotCheckedWhenDescriptionHasNoChanges()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				chargeCode.AC_Desc = "?";
				AssertEquals("pre-condition", true, chargeCode.HasChanges);
				AssertHasError(chargeCode.AC_DescInfo, "Length is invalid, it must be at least 2 characters long. Please ensure you are entering the correct description for this Charge code.");

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var reloadedChargeCode = newFactory.Load<AccChargeCode>(chargeCode.PK);
				reloadedChargeCode.Validation.ValidateAC_Desc();

				AssertEquals("pre-condition", false, reloadedChargeCode.HasChanges);
				AssertNoErrors(reloadedChargeCode.AC_DescInfo);
			}
		}

		public void TestChargeCodeDescriptionComplianceRequirements_NotCheckedWithPostedTransaction()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				var arInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
				arInvoiceLine.AL_AC = chargeCode.PK;
				Factory.Save();

				chargeCode.AC_Desc = "1";
				AssertHasError(chargeCode.AC_DescInfo, "You cannot edit this description. At least one transaction has been posted in a Portugal Login Company in this database using this Charge Code.");

				chargeCode.AC_Desc = "???";
				AssertHasError(chargeCode.AC_DescInfo, "You cannot edit this description. At least one transaction has been posted in a Portugal Login Company in this database using this Charge Code.");
			}
		}

		public void TestShouldNotChangeOrgDetailsIfHasPostedTransactions()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var code = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.IVA, "123456789", Core.Constants.CountryCodes.Portugal);

				var adjustmentNote = Factory.NewWithValidTestData<AccTransactionHeader>();
				adjustmentNote.AH_Ledger = LedgerTypes.AccountsReceivable;
				adjustmentNote.AH_TransactionType = TransactionTypes.AdjustmentNote;
				adjustmentNote.AH_OH = org.PK;
				Factory.Save();
				Assert(!PortugalValidatorHelper.IsThereAnyTransactionForOrgWithRestrictedCusCode(code));

				var shipment = (IJobHeaderParent)Factory.New<IForwardingShipment>();
				var shipmentJob = new JobHeader.Loader(shipment).TryCreate();
				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				var wipLine = Factory.NewWithValidTestData<AccTransactionLines>();
				wipLine.AL_AC = chargeCode.PK;
				wipLine.AL_OH = org.PK;
				wipLine.AL_LineType = TransactionLineTypes.WIP;
				wipLine.AL_JH = shipmentJob.PK;
				Factory.Save();
				org.Factory.ClearCachedValue<bool>(org.PK.ToStringKey());
				Assert(!PortugalValidatorHelper.IsThereAnyTransactionForOrgWithRestrictedCusCode(code));

				var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
				transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
				transaction.AH_TransactionType = TransactionTypes.Invoice;
				transaction.AH_OH = org.PK;
				Factory.Save();
				org.Factory.ClearCachedValue<bool>(org.PK.ToStringKey());
				Assert(PortugalValidatorHelper.IsThereAnyTransactionForOrgWithRestrictedCusCode(code));

				transaction.AH_Desc = "Description";
				Factory.Save();
				org.Factory.ClearCachedValue<bool>(org.PK.ToStringKey());
				Assert(PortugalValidatorHelper.IsThereAnyTransactionForOrgWithRestrictedCusCode(code));
			}
		}

		public void TestIsRestrictedCusCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var code = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.IVA, AccountingCountrySpecificValidationHelper.EmptyPortugalIVA, Core.Constants.CountryCodes.Portugal);

				var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
				transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
				transaction.AH_TransactionType = TransactionTypes.Invoice;
				transaction.AH_OH = org.PK;
				Factory.Save();
				Assert(!PortugalValidatorHelper.IsThereAnyTransactionForOrgWithRestrictedCusCode(code));

				code.OK_CustomsRegNo = Core.Constants.CountryCodes.Portugal + AccountingCountrySpecificValidationHelper.EmptyPortugalIVA;
				Factory.Save();
				Assert(!PortugalValidatorHelper.IsThereAnyTransactionForOrgWithRestrictedCusCode(code));

				code.OK_CustomsRegNo = "PT123456789";
				Factory.Save();
				Assert(PortugalValidatorHelper.IsThereAnyTransactionForOrgWithRestrictedCusCode(code));
			}
		}
	}
}
