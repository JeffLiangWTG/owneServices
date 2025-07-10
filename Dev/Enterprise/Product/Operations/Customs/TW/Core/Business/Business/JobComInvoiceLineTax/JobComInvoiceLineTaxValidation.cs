using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class JobComInvoiceLineTaxValidation : Customs.Business.JobComInvoiceLineTaxValidation
	{
		public JobComInvoiceLineTaxValidation(AutoJobComInvoiceLineTax parent) : base(parent)
		{
		}

		public new JobComInvoiceLineTax Parent => (JobComInvoiceLineTax)base.Parent;

		protected override void CheckJLT_Type()
		{
			base.CheckJLT_Type();
			var parent = Parent;
			var tariffType = parent.JLT_Type;
			var targetInfo = parent.JLT_TypeInfo;
			MandatoryValidation.CheckEntered(targetInfo);

			if (!tariffType.IsEmpty)
			{
				if (parent.UniversalTariffType == null)
				{
					ListValidation.MessageErrorIfInvalidCode(targetInfo);
				}
				else if (parent.InvoiceLine?.Taxes?.Cast<JobComInvoiceLineTax>().Any(x => x.JLT_Type == tariffType && x.PK != parent.PK) ?? false)
				{
					targetInfo.AddMessageError(ValidationConstants.InvoiceLineTax.DuplicateTariffTypeFound(parent.JLT_TypeDescription));
				}
			}
		}

		protected override void CheckJLT_Tariff()
		{
			base.CheckJLT_Tariff();
			var parent = Parent;
			var targetInfo = parent.JLT_TariffInfo;
			var tariff = parent.JLT_Tariff;
			var tariffType = parent.JLT_Type;
			var invoiceLine = parent?.InvoiceLine;
			var mainTariff = invoiceLine?.JI_Tariff ?? ZString.Empty;

			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);

			if (!tariff.IsEmpty)
			{
				if (parent.UniversalTariffType == null)
				{
					ListValidation.MessageErrorIfInvalidCode(targetInfo);
				}
				else if (parent.UniversalTariff == null)
				{
					targetInfo.AddMessageError(ValidationConstants.InvoiceLineTax.TariffDoesNotBelongToType(tariff, tariffType));
				}

				if (!mainTariff.IsEmpty)
				{
					var tariffValidBasedOnMainTariff = invoiceLine?.UniversalTariff?.GetEffectiveChildTariffs(parent.EffectiveAssessmentDate)?.Any(x => string.Equals(x.ZZ1_TariffCode, tariff, System.StringComparison.OrdinalIgnoreCase)) ?? false;
					if (!tariffValidBasedOnMainTariff)
					{
						targetInfo.AddMessageError(ValidationConstants.InvoiceLineTax.ChildTariffDoesNotBelongToMainTariff(tariff, mainTariff));
					}
				}
			}
		}

		protected override void CheckJLT_MethodOfPayment()
		{
			base.CheckJLT_MethodOfPayment();
			ListValidation.MessageErrorIfInvalidCode(Parent.JLT_MethodOfPaymentInfo);
		}
	}
}
