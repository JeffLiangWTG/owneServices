using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.Business
{
	class MandatoryChargesHandler
	{
		public MandatoryChargesHandler(BaseJobComInvoiceHeader invoice)
		{
			this.invoice = invoice;
		}

		readonly BaseJobComInvoiceHeader invoice;

		public void Execute()
		{
			var chargesToDelete = GetZeroAmountChargesToDelete();

			var declaration = !invoice.IncoTerm.IsEmpty ? invoice.JobDeclaration : null;
			if (declaration != null && declaration.AutoCreateChargesBasedOnIncoTerm)
			{
				var missingCharges = invoice.IncoTermAndChargeFactory.MissingRecommendedCharges(invoice.IncoTerm, invoice);

				foreach (var missingCharge in missingCharges)
				{
					var chargeMatched = chargesToDelete.Find(x => x.WithKey(missingCharge.ChargeCodeChargeKey));

					if (chargeMatched != null && IsInARightPlace(chargeMatched))
					{
						chargesToDelete.Remove(chargeMatched);
					}
					else
					{
						FillInMissingMandatoryCharge(missingCharge);
					}
				}
			}

			chargesToDelete.ForEach(x => x.Delete());
		}

		bool IsInARightPlace(JobComInvCharge chargeMatched)
		{
			if (!invoice.IncoTerm.IsEmpty && invoice.IncoTermAndChargeFactory.CanThisIncoTermHaveThisCharge(invoice.IncoTerm, chargeMatched.ChargeCode))
			{
				return invoice.Charges.Contains(chargeMatched);
			}
			else
			{
				return !invoice.Charges.Contains(chargeMatched);
			}
		}

		List<JobComInvCharge> GetZeroAmountChargesToDelete()
		{
			var result = new List<JobComInvCharge>();

			foreach (var charge in invoice.Charges)
			{
				if (charge.IsEmpty)
				{
					result.Add(charge);
				}
			}

			if (invoice.GroupHeader != null)
			{
				foreach (var charge in invoice.GroupHeader.Charges)
				{
					if (charge.IsEmpty && !invoice.GroupHeader.JobComInvoiceHeaders.HasInvoicesWithRecommendedChargeInGroupCharges(charge.ChargeCode))
					{
						result.Add(charge);
					}
				}
			}

			return result;
		}

		void FillInMissingMandatoryCharge(ICustomsChargeCode missingCharge)
		{
			if (!IsRightParentType(missingCharge, ChargeParentTypes.GroupInvoice) && !IsRightParentType(missingCharge, ChargeParentTypes.Invoice))
			{
				ErrorReporter.ReportOnce(string.Format(System.Globalization.CultureInfo.InvariantCulture, "This charge {0} is marked down as recommended for this incoterm {1}, but its ParentType is neither GroupInvoice nor Invoice.", missingCharge.Code, invoice.IncoTerm));
			}
			BaseJobComInvHeaderCharge chargeToAdd = null;
			if (!invoice.IncoTerm.IsEmpty && invoice.IncoTermAndChargeFactory.CanThisIncoTermHaveThisCharge(invoice.IncoTerm, missingCharge) && IsRightParentType(missingCharge, ChargeParentTypes.Invoice))
			{
				if (!invoice.Charges.HasChargeWithThisKey(missingCharge.ChargeCodeChargeKey))
				{
					chargeToAdd = invoice.Charges.AddNew();
				}
			}
			else if (invoice.GroupHeader != null && IsRightParentType(missingCharge, ChargeParentTypes.GroupInvoice))
			{
				if (invoice.GroupHeader.Charges.GetCharge(missingCharge.Code).Length == 0)
				{
					chargeToAdd = invoice.GroupHeader.Charges.AddNew();
				}
			}

			if (chargeToAdd != null)
			{
				using (chargeToAdd.SuspendSettingHasChanges())
				{
					chargeToAdd.J7_ChargeType = missingCharge.Code;
				}
			}
		}

		static bool IsRightParentType(ICustomsChargeCode missingCharge, ChargeParentTypes targetParentType)
		{
			return (missingCharge.ParentTypes & targetParentType) == targetParentType;
		}
	}
}
