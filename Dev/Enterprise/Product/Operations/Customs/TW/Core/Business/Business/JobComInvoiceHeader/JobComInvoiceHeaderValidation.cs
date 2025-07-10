using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class JobComInvoiceHeaderValidation : Customs.Business.InvoiceHeaderValidation
	{
		public JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		public new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		public override void ValidateAll()
		{
			Parent.LoadBeforeValationAll();
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateTW_MarksAndNumbers();
			}
		}

		protected override void CheckJZ_InvoiceAmount()
		{
			base.CheckJZ_InvoiceAmount();
			CompareValidation.MessageErrorIfLessThanOrEqualToZero(Parent.JZ_InvoiceAmountInfo);
		}

		public void ValidateTW_MarksAndNumbers()
		{
			((IValidationInternals)this).Validate(Parent.TW_MarksAndNumbersInfo, () => { CheckTW_MarksAndNumbers(); });
		}

		protected override void CheckJZ_Calc_CIFAmount()
		{
			//CIF field was been hidden, so didn't show any messageError and Warnings.
		}

		protected override void CheckJZ_RelatedIndicator()
		{
			base.CheckJZ_RelatedIndicator();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JZ_RelatedIndicatorInfo);
		}

		protected override TypeOfValidationForMissingMandatoryChargesForIncoterm ValidationForMissingMandatoryCharges
		{
			get { return TypeOfValidationForMissingMandatoryChargesForIncoterm.Warning; }
		}

		protected override void CheckJZ_IncoTerm()
		{
			base.CheckJZ_IncoTerm();

			var parent = Parent;
			if (IncoTermRequired)
			{
				ValidateRecommendedChargesForIncoTerm();
			}
			if (parent.JobDeclaration?.Invoices?.Any(x => x.PK != parent.PK && x.JZ_IncoTerm != parent.JZ_IncoTerm) ?? ZBool.False)
			{
				parent.JZ_IncoTermInfo.AddMessageError(ValidationConstants.InvoiceHeader.HavingMultipleINCOTermsOnSingleJob);
			}

			if (parent.IsImport && !parent.HasInternationFreightAmount)
			{
				parent.JZ_IncoTermInfo.AddMessageError(Res.GetString("6488D14B-BB13-415E-B9AA-DC0063C41D2B", "International Freight is required."));
			}
		}

		protected override void CheckJZ_NoOfPacks()
		{
			base.CheckJZ_NoOfPacks();
			var invoice = Parent;
			ValidationHelper.CheckNoOfPacksBalance(invoice.JobDeclaration, invoice.JZ_NoOfPacksInfo);
		}

		protected override void CheckJZ_NoOfPacksIsValidZDecimal()
		{
			TypeValidation.CheckValidDecimal(Parent.JZ_NoOfPacksInfo, 8, 0);
		}

		protected void ValidateRecommendedChargesForIncoTerm()
		{
			string missingRecommendedCharges = GetMissingRecommendedCharges();
			if (missingRecommendedCharges.Length > 0)
			{
				Parent.JZ_IncoTermInfo.AddWarning(Res.GetString("12ca6efb-c053-fc8d-4256-a29a3ad26fa6", "The following charges are probably required for Incoterm {0}:\r\n{1}", Parent.JZ_IncoTerm, missingRecommendedCharges.Trim(',')));
			}
		}

		protected string GetMissingRecommendedCharges()
		{
			string result = string.Empty;
			var incoTerm = Parent.IncoTerm;
			if (!incoTerm.IsEmpty)
			{
				var incoTermAndChargeFactory = Parent.IncoTermAndChargeFactory;
				var missingCharges = incoTermAndChargeFactory.MissingRecommendedCharges(incoTerm, Parent);
				var charges = new StringBuilder();
				foreach (var chargeCode in missingCharges)
				{
					if (!incoTermAndChargeFactory.IsThisChargeMandatory(incoTerm, chargeCode.Code))
					{
						charges.Append(chargeCode.Description + ",");
					}
				}
				result = charges.ToString();
			}
			return result;
		}

		protected override void CheckJZ_NetWeight()
		{
			base.CheckJZ_NetWeight();
			var netWeight = Parent.JZ_NetWeight;
			if (!netWeight.IsEmpty)
			{
				var invoiceHeaderNetWeightUQ = Parent.JZ_NetWeightUQ;
				var totalInvoiceLineNetWeightInHeaderNetWeightUQ = ((ZDecimal)Parent.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => Core.Constants.Weight.ConvertSafe(x.JI_NetWeight, x.JI_NetWeightUQ, invoiceHeaderNetWeightUQ))).Round(3).Normalize();
				if (netWeight != totalInvoiceLineNetWeightInHeaderNetWeightUQ)
				{
					Parent.JZ_NetWeightInfo.AddWarning(Res.GetString("263b1e37-dbf6-4aa9-849a-19bd51cde52b", "The sum of all Invoice Line Net Weight {0} {1} does not balance with the Invoice Header total Net Weight {2} {1}.", totalInvoiceLineNetWeightInHeaderNetWeightUQ, invoiceHeaderNetWeightUQ, netWeight.Normalize()));
				}
			}
		}

		protected override void CheckJZ_Weight()
		{
			base.CheckJZ_Weight();
			var weight = Parent.JZ_Weight;
			if (!weight.IsEmpty)
			{
				var weightUQ = Parent.JZ_WeightUQ;
				var totalInvoiceLineNetWeightInHeaderGrossWeightUQ = ((ZDecimal)Parent.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => Core.Constants.Weight.ConvertSafe(x.JI_Weight, x.JI_WeightUQ, weightUQ))).Round(3).Normalize();
				if (weight != totalInvoiceLineNetWeightInHeaderGrossWeightUQ)
				{
					Parent.JZ_WeightInfo.AddWarning(Res.GetString("15b2436a-82ea-454f-9ec1-7c5e3fe7d321", "The sum of all Invoice Line Gross Weight {0} {1} does not balance with the Invoice Header total Gross Weight {2} {1}.", totalInvoiceLineNetWeightInHeaderGrossWeightUQ, weightUQ, weight.Normalize()));
				}
			}
		}

		protected override void CheckJZ_RX_NKInvoice_Currency()
		{
			base.CheckJZ_RX_NKInvoice_Currency();

			if (Parent.JobDeclaration != null && Parent.JobDeclaration.Invoices.Any(x => !x.JZ_RX_NKInvoice_Currency.IsEmpty && x.JZ_RX_NKInvoice_Currency != Parent.JZ_RX_NKInvoice_Currency))
			{
				Parent.JZ_RX_NKInvoice_CurrencyInfo.AddMessageError(ValidationConstants.InvoiceHeader.MultipleCurrencyError);
			}
		}

		protected void CheckTW_MarksAndNumbers()
		{
			var maxlength = 512;
			if (Parent.TW_MarksAndNumbers.Length > maxlength)
			{
				Parent.TW_MarksAndNumbersInfo.AddWarning(ValidationConstants.TWJobDocAddressValidationMessages.MaxCharactersLength(maxlength));
			}
		}

		protected override void CheckJZ_OH_Supplier()
		{
			//JZ_OH_Supplier field is not used, do not show any notifications.
		}

		protected override void CheckJZ_OH_Buyer()
		{
			//JZ_OH_Buyer field is not used, do not show any notifications.
		}

		protected override void CheckJZ_InvoiceDate()
		{
			base.CheckJZ_InvoiceDate();
			var parent = Parent;
			var invoiceDate = parent.JZ_InvoiceDate;
			if (invoiceDate.IsEmpty)
			{
				foreach (JobComInvoiceLine invoiceLine in parent.InvoiceLines)
				{
					if (invoiceLine.IsInvoiceDateAndNumberRequired)
					{
						parent.JZ_InvoiceDateInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Res.GetString("484B5D8B-5BA0-4C73-AB1E-92880831F21F", "Invoice Date")));
						break;
					}
				}
			}
			else if (invoiceDate > parent.JobDeclaration?.JE_EntrySubmittedDate)
			{
				foreach (JobComInvoiceLine invoiceLine in parent.InvoiceLines)
				{
					if (invoiceLine.IsLinkedNX101WithCertificate15)
					{
						parent.JZ_InvoiceDateInfo.AddMessageError(Res.GetString("11F34FEE-36D6-4F60-AD10-D97D8B8D6D33", "Invoice Date should before Submission Date"));
						break;
					}
				}
			}
		}

		protected override void CheckJZ_InvoiceNumber()
		{
			base.CheckJZ_InvoiceNumber();
			var parent = Parent;
			var invoiceNumber = Parent.JZ_InvoiceNumber;
			if (invoiceNumber.IsEmpty)
			{
				foreach (JobComInvoiceLine invoiceLine in parent.InvoiceLines)
				{
					if (invoiceLine.IsInvoiceDateAndNumberRequired)
					{
						parent.JZ_InvoiceNumberInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Res.GetString("520F6649-BFED-4A5C-861C-0C4F4519CC20", "Invoice No")));
						break;
					}
				}
			}
		}
	}
}
