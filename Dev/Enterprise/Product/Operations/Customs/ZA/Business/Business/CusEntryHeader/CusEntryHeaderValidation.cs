using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public partial class CusEntryHeaderValidation : AutoZACusEntryHeaderValidation
	{
		public CusEntryHeaderValidation(CusEntryHeader parent)
			: base(parent)
		{
		}

		protected new CusEntryHeader Parent
		{
			get { return (CusEntryHeader)base.Parent; }
		}

		JobDeclaration Declaration => Parent.Declaration;

		protected override void CheckPackagesCount()
		{
			base.CheckPackagesCount();
			if (Parent.Declaration.IsExport && Parent.PackagesCount == 0)
			{
				Parent.PackagesCountInfo.AddMessageError("Number of Packages should not be zero when exporting");
			}
		}

		protected override void CheckCH_BGMReference()
		{
			base.CheckCH_BGMReference();

			if (!Parent.CH_BGMReference.IsEmpty)
			{
				var duplicateEntry = FindDuplicateEntryByLRN(Parent);
				if (duplicateEntry != null)
				{
					Parent.CH_BGMReferenceInfo.AddMessageError(FormattableString.Invariant($"LRN {Parent.CH_BGMReference} is already in use on {duplicateEntry.Declaration.JE_DeclarationReference}"));
				}
			}
		}

		protected override void CheckCH_BondValidToDateIsValidZDateRange()
		{
			base.CheckCH_BondValidToDateIsValidZDateRange();
			if (Parent.CH_EntryReleaseDate.IsValid && Parent.CalculateDefaultCH_BondValidToDate_OnDefaultRegistryValue() != Parent.CH_BondValidToDate)
			{
				Parent.CH_BondValidToDateInfo.AddWarning(@"The Date you have entered is not the same as the default date that is set in Registry
Acquit By Date is managed and and maintained in the following Registry Setting: Registry > Customs > South Africa > CPC Acquit By Date");
			}
		}

		CusEntryHeader FindDuplicateEntryByLRN(CusEntryHeader originalEntry)
		{
			var query = new ZQuery(CusEntryHeaderSchema.CH_BGMReference, originalEntry.CH_BGMReference);
			query.AddToFilter(CusEntryHeaderSchema.PK, SQLComparisonOperator.NotEqual, originalEntry.PK);
			return Parent.Factory.Load<CusEntryHeader>(query).FirstOrDefault();
		}

		protected override void CheckCH_PaymentMethod()
		{
			base.CheckCH_PaymentMethod();
			var paymentMethodList = Parent.Lookups.CH_PaymentMethodCodeList;
			if (paymentMethodList != null)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CH_PaymentMethodInfo, paymentMethodList);
			}

			if (Parent.CH_PaymentMethod != PaymentMethodCodeList.Codes.Free)
			{
				if (Parent.GetFinancialAccountNumber().IsEmpty)
				{
					Parent.CH_PaymentMethodInfo.AddMessageError(ValidationConstants.EntryHeader.FinancialAccountNumberNotSetup(Parent.CustomsOffice));
				}
			}

			if ((Parent.Declaration.JE_MessageType == ZAJobMessageTypeList.Codes.Import || Parent.Declaration.JE_MessageType == ZAJobMessageTypeList.Codes.ExBond) && Parent.CH_PaymentMethod != PaymentMethodCodeList.Codes.Cash)
			{
				if (Parent.ProvisionalPaymentAmountDifference > 0 || Parent.PenaltyAmountAfter > 0)
				{
					Parent.CH_PaymentMethodInfo.AddMessageError("Payment Code: should be C - CASH when ePP Amount Due to Customs is greater than zero");
				}
			}
		}

		protected override void CheckCH_Packages()
		{
			base.CheckCH_Packages();
			if (Declaration != null && (Declaration.IsImport && !Declaration.IsExWarehouse || Declaration.IsExport))
			{
				MandatoryValidation.MessageErrorIfIsNegative(Parent.CH_PackagesInfo);
				MandatoryValidation.MessageErrorIfIsZero(Parent.CH_PackagesInfo);
			}
		}

		protected override void CheckCH_EntryNumber()
		{
			base.CheckCH_EntryNumber();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.CH_EntryNumberInfo);
			MandatoryValidation.MessageErrorIfIsZero(Parent.CH_EntryNumberInfo);

			if (Parent.CH_EntryNumber > Parent.CH_TotalEntries)
			{
				Parent.CH_EntryNumberInfo.AddMessageError("Entry Number should not be greater than Total Entries.");
			}
		}

		protected override void CheckCH_TotalEntries()
		{
			base.CheckCH_TotalEntries();
			if (Declaration != null)
			{
				MandatoryValidation.MessageErrorIfIsNegative(Parent.CH_TotalEntriesInfo);
				MandatoryValidation.MessageErrorIfIsZero(Parent.CH_TotalEntriesInfo);

				if (!Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().AllSame(x => x.CH_TotalEntries))
				{
					Parent.CH_TotalEntriesInfo.AddMessageError("Total Entries value is not equal to other entries.");
				}
			}
		}
	}
}
