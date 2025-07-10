//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSWHSPackLineAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSWHSPackLineAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USWHSPackLineAddInfoValidation : AutoUSWHSPackLineAddInfoValidation
	{
		public USWHSPackLineAddInfoValidation(AutoUSWHSPackLineAddInfo parent)
			: base(parent)
		{
			if (!(parent is USWHSPackLineAddInfo))
			{
				throw new ArgumentException("Parent should be USWHSPackLineAddInfo");
			}
		}

		protected new USWHSPackLineAddInfo Parent
		{
			get { return (USWHSPackLineAddInfo)base.Parent; }
		}

		protected override void CheckUS_JI_InvoiceLineIsValidZGuid()
		{
		}

		protected override void CheckUS_JI_InvoiceLine()
		{
			base.CheckUS_JI_InvoiceLine();
			if (IsInwardBondedWarehousingEnabled)
			{
				MandatoryValidation.CheckEntered(Parent.US_JI_InvoiceLineInfo);
				ListValidation.ErrorIfInvalidPK(Parent.US_JI_InvoiceLineInfo);
			}
			ValidateUS_PackedQty();
			ValidateUS_B7_WHSPack();
		}

		protected override void CheckUS_B7_WHSPackIsValidZGuid()
		{
		}

		protected override void CheckUS_B7_WHSPack()
		{
			base.CheckUS_B7_WHSPack();
			if (IsInwardBondedWarehousingEnabled)
			{
				MandatoryValidation.CheckEntered(Parent.US_B7_WHSPackInfo);
				ListValidation.ErrorIfInvalidPK(Parent.US_B7_WHSPackInfo);
				CheckDuplication();
			}
			ValidateUS_PackedQty();
		}

		void CheckDuplication()
		{
			var whsPackPK = Parent.US_B7_WHSPack;
			var invoiceLinePK = Parent.US_JI_InvoiceLine;
			var whsPackLine = Parent.Parent;
			var declaration = whsPackLine == null ? null : whsPackLine.Parent;
			if (declaration != null)
			{
				var whsPackLinePK = whsPackLine.PK;
				if (declaration.WHSPackLines.OfType<WHSPackLine>().Any(x => x.PK != whsPackLinePK && x.US_B7_WHSPack == whsPackPK && x.US_JI_InvoiceLine == invoiceLinePK))
				{
					Parent.US_B7_WHSPackInfo.AddError(ValidationConstants.WHSPackLine.DuplicatePackageReferenceAndInvoiceLineReference);
				}
			}
		}

		protected override void CheckUS_PackedQty()
		{
			base.CheckUS_PackedQty();
			if (IsInwardBondedWarehousingEnabled)
			{
				if (Parent.US_PackedQty <= ZDecimal.Zero)
				{
					Parent.US_PackedQtyInfo.AddMessageError(ValidationConstants.WHSPackLine.PackedQtyIsRequired);
				}
				else
				{
					var invoiceLine = Parent.InvoiceLine;
					if (invoiceLine != null && invoiceLine.JI_InvoiceQuantity < Parent.US_PackedQty)
					{
						Parent.US_PackedQtyInfo.AddMessageError(ValidationConstants.WHSPackLine.PackedQtyCannotBeGreaterThanInvoiceQuantity);
					}
				}
			}
		}

		bool IsInwardBondedWarehousingEnabled
		{
			get
			{
				var packLine = Parent.Parent;
				return packLine != null && packLine.IsInwardBondedWarehousingEnabled;
			}
		}
	}
}
