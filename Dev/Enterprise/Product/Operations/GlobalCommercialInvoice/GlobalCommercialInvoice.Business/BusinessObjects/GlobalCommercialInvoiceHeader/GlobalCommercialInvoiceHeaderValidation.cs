using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.GlobalCommercialInvoice.Business
{
	/// <summary>
	/// Validation for the <see cref="GlobalCommercialInvoiceHeader"/> class.
	/// </summary>
	/// <param name="parent">Parent header.</param>
	public sealed class GlobalCommercialInvoiceHeaderValidation(AutoGlobalCommercialInvoiceHeader parent)
		: AutoGlobalCommercialInvoiceHeaderValidation(parent)
	{
		/// <summary>
		/// GIH_InvoiceNumber validation routine.
		/// </summary>
		protected override void CheckGIH_InvoiceNumber()
		{
			base.CheckGIH_InvoiceNumber();
			MandatoryValidation.CheckEntered(Parent.GIH_InvoiceNumberInfo);

			if (Parent.GIH_InvoiceNumber.Length > 0)
			{
				var hasDuplicateInvoiceNumber = ((GlobalCommercialInvoiceHeader)Parent).Headers
					.Any((x) => !x.IsDeleted && x != Parent && x.GIH_InvoiceNumber == Parent.GIH_InvoiceNumber);
				if (hasDuplicateInvoiceNumber)
				{
					Parent.GIH_InvoiceNumberInfo.AddWarning(ResString.GetMultilingualString("82042494-9e92-466b-a5d6-5e911f07987b",
						"This invoice number already exists in this job"));
				}
			}
		}

		/// <summary>
		/// GIH_InvoiceDate validation routine.
		/// </summary>
		protected override void CheckGIH_InvoiceDate()
		{
			base.CheckGIH_InvoiceDate();
			MandatoryValidation.CheckEntered(Parent.GIH_InvoiceDateInfo);
		}
	}
}
