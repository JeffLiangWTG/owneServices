using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class ManifestToOpenPackValidation : CusTRPreviousDocumentItemValidation
	{
		public ManifestToOpenPackValidation(AutoCusTRPreviousDocumentItem parent) : base(parent)
		{
		}

		new ManifestToOpenPack Parent => (ManifestToOpenPack)base.Parent;

		protected override void CheckTPI_LineNumber()
		{
			base.CheckTPI_LineNumber();

			if (!Parent.BillIncludeAllItems)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.TPI_LineNumberInfo);
			}
		}

		protected override void CheckTPI_Quantity()
		{
			base.CheckTPI_Quantity();

			if (!Parent.BillIncludeAllItems && !Parent.TPI_LineNumber.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.TPI_QuantityInfo);
			}
		}

		protected override void CheckTPI_WarehouseCode()
		{
			base.CheckTPI_WarehouseCode();

			if (Parent.BillIsInWarehouse && !Parent.TPI_LineNumber.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.TPI_WarehouseCodeInfo);
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.TPI_WarehouseCodeInfo);
		}
	}
}
