using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class JobTWComInvoiceLineValidation : AutoJobTWComInvoiceLineValidation
	{
		public JobTWComInvoiceLineValidation(AutoJobTWComInvoiceLine parent) : base(parent)
		{
		}

		protected new JobTWComInvoiceLine Parent => (JobTWComInvoiceLine)base.Parent;

		protected override void CheckTWL_AircraftPartsCategory()
		{
			base.CheckTWL_AircraftPartsCategory();
			var parent = Parent;
			if (!parent.TWL_AircraftPartsCode.IsEmpty || !parent.TWL_AircraftIPC.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.TWL_AircraftPartsCategoryInfo);
			}
		}

		protected override void CheckTWL_AircraftPartsCode()
		{
			base.CheckTWL_AircraftPartsCode();
			var parent = Parent;
			if (!parent.TWL_AircraftPartsCategory.IsEmpty || !parent.TWL_AircraftIPC.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.TWL_AircraftPartsCodeInfo);
			}
		}

		protected override void CheckTWL_AircraftIPC()
		{
			base.CheckTWL_AircraftIPC();
			var parent = Parent;
			if (!parent.TWL_AircraftPartsCategory.IsEmpty || !parent.TWL_AircraftPartsCode.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.TWL_AircraftIPCInfo);
			}
		}

		protected override void CheckTWL_DocumentaryQty()
		{
			base.CheckTWL_DocumentaryQty();
			if (Parent.TWL_DocumentaryQty <= 0)
			{
				var tragetInfo = Parent.TWL_DocumentaryQtyInfo;
				tragetInfo.AddWarning(Res.GetString("F1DF0924-45BF-4E78-9922-FE5C7A799B17", "{0} must be greater than 0.", tragetInfo.HumanReadableName));
			}
		}

		protected override void CheckTWL_DocumentaryUQ()
		{
			base.CheckTWL_DocumentaryUQ();
			var tragetInfo = Parent.TWL_DocumentaryUQInfo;
			ListValidation.WarnIfInvalidCode(tragetInfo);
			MandatoryValidation.WarnIfNotEntered(tragetInfo);
		}

		protected override void CheckTWL_DocumentaryUnitPrice()
		{
			base.CheckTWL_DocumentaryUnitPrice();
			var targetInfo = Parent.TWL_DocumentaryUnitPriceInfo;
			if (Parent.TWL_DocumentaryUnitPrice <= 0)
			{
				targetInfo.AddWarning(Res.GetString("CAC96799-6369-4904-8D62-D5AEA81D2E73", "{0} must be greater than 0.", targetInfo.HumanReadableName));
			}

			var invoiceLine = Parent.InvoiceLine;
			var linePrice = invoiceLine?.JI_LinePrice ?? ZDecimal.Zero;
			var invoiceQuantity = Parent.TWL_DocumentaryQty;
			var unitPrice = Parent.TWL_DocumentaryUnitPrice;
			var unitPriceDecimalPlaces = Math.Max(unitPrice.DecimalPlaces, 2);
			var linePriceDecimalPlaces = invoiceLine?.InvoiceHeader?.Invoice_Currency?.Decimals ?? 2;
			var calculatedLinePrice = invoiceQuantity * unitPrice;

			if (Utilities.Round(calculatedLinePrice, 2) != linePrice
				&& Utilities.Round(calculatedLinePrice, linePriceDecimalPlaces) != linePrice
				&& (invoiceQuantity <= 0 || Utilities.Round(linePrice / invoiceQuantity, unitPriceDecimalPlaces) != unitPrice))
			{
				targetInfo.AddWarning(Res.GetString("98770442-CBF5-4E48-8A2D-73BB075DAC07", "{0} * {1} must equal to Line Price.", targetInfo.HumanReadableName, Parent.TWL_DocumentaryQtyInfo.HumanReadableName));
			}
		}
	}
}
