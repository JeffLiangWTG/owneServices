using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISCommodityLineValidation : AutoDISCommodityLineValidation
	{
		public DISCommodityLineValidation(AutoDISCommodityLine bizObj)
			: base(bizObj)
		{
		}

		new DISCommodityLine Parent
		{
			get { return (DISCommodityLine)base.Parent; }
		}

		protected override void CheckInvoiceNumber()
		{
			base.CheckInvoiceNumber();

			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.InvoiceNumberInfo);
		}

		protected override void CheckInvoiceLineNumber()
		{
			base.CheckInvoiceLineNumber();

			if (Parent.InvoiceLineNumber <= 0)
			{
				Parent.InvoiceLineNumberInfo.AddMessageError(PleaseEnterInvoiceLineNumber);
			}
			else if (!Parent.InvoiceNumber.IsEmpty && Parent.VNELineNumber == 0)
			{
				var lines = Parent.CommodityLines;

				if (lines.IsNullOrEmpty())
				{
					Parent.InvoiceLineNumberInfo.AddMessageError(string.Format(NoLineMatchingTheseNumbers, Parent.InvoiceLineNumber, Parent.InvoiceNumber));
				}
			}
		}

		internal const string PleaseEnterInvoiceLineNumber = "Please enter an invoice line number.";
		internal const string NoLineMatchingTheseNumbers = "Invoice Line # '{0}' does not exist on Invoice # '{1}'";

		protected override void CheckInvoiceLineTo()
		{
			base.CheckInvoiceLineTo();

			if (Parent.InvoiceLineTo < 0)
			{
				Parent.InvoiceLineToInfo.AddMessageError(PleaseEnterValidInvoiceLineNumber);
			}
			else if (Parent.InvoiceLineTo > 0)
			{
				if (Parent.InvoiceLineTo < Parent.InvoiceLineNumber)
				{
					Parent.InvoiceLineToInfo.AddMessageError(InvalidInvoiceLineTo);
				}
				else if (Parent.InvoiceLineTo > Parent.InvoiceLineNumber)
				{
					var lines = Parent.disDocument.HostWrapper.GetInvoiceLines(Parent.InvoiceNumber, Parent.InvoiceLineTo, Parent.InvoiceLineTo);

					if (lines.IsNullOrEmpty())
					{
						Parent.InvoiceLineToInfo.AddMessageError(string.Format(NoInvoiceLineWithThisLineNumber, Parent.InvoiceLineTo, Parent.InvoiceNumber));
					}
				}
			}
		}

		internal const string PleaseEnterValidInvoiceLineNumber = "Please enter a valid invoice line # which is greater than zero.";
		internal const string InvalidInvoiceLineTo = "Invoice Line # To should be greater than or equal to Invoice Line # From.";
		internal const string NoInvoiceLineWithThisLineNumber = "Invoice Line # '{0}' does not exist on Invoice # '{1}'";

		protected override void CheckVNELineNumber()
		{
			base.CheckVNELineNumber();

			if (Parent.VNELineNumber < 0)
			{
				Parent.VNELineNumberInfo.AddMessageError(InvalidVehicleLineNumber);
			}
			else if (!Parent.InvoiceNumber.IsEmpty && Parent.InvoiceLineNumber > 0 && Parent.VNELineNumber > 0)
			{
				var lines = Parent.CommodityLines;

				if (lines.IsNullOrEmpty())
				{
					Parent.VNELineNumberInfo.AddMessageError(string.Format(NoVehicleLineMatchingTheseNumbers, Parent.VNELineNumber, Parent.InvoiceNumber, Parent.InvoiceLineNumber));
				}
			}
		}

		internal const string InvalidVehicleLineNumber = "Please enter a valid vehicle line #.";
		internal const string NoVehicleLineMatchingTheseNumbers = "Vehicle (VNE) Line # '{0}' does not exist on Invoice # '{1}' and Invoice Line # '{2}'.";

		protected override void CheckVNELineNumberTo()
		{
			base.CheckVNELineNumberTo();

			if (Parent.VNELineNumberTo < 0)
			{
				Parent.VNELineNumberToInfo.AddMessageError(InvalidVehicleLineNumber);
			}
			else if (Parent.VNELineNumberTo > 0)
			{
				if (Parent.VNELineNumberTo < Parent.VNELineNumber)
				{
					Parent.VNELineNumberToInfo.AddMessageError(InvalidVNELineTo);
				}
				else if (Parent.VNELineNumberTo > Parent.VNELineNumber)
				{
					var lines = Parent.disDocument.HostWrapper.GetCommodityLines(Parent.InvoiceNumber, Parent.InvoiceLineNumber, Parent.VNELineNumberTo);

					if (lines.IsNullOrEmpty())
					{
						Parent.VNELineNumberToInfo.AddMessageError(string.Format(NoVNELineWithThisLineNumber, Parent.VNELineNumberTo, Parent.InvoiceNumber, Parent.InvoiceLineNumber));
					}
				}
			}
		}

		internal const string InvalidVNELineTo = "Vehicle (VNE) Line # To should be greater than or equal to VNE Line # From.";
		internal const string NoVNELineWithThisLineNumber = "Vehicle (VNE) Line # '{0}' does not exist on Invoice # '{1}' and Invoice Line # '{2}'.";
	}
}
