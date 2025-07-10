using CargoWise.EntityFramework;

namespace Enterprise.GlobalCommercialInvoice.Business
{
	/// <summary>
	/// Validation for the <see cref="GlobalCommercialInvoiceLine"/> class.
	/// </summary>
	/// <param name="parent">Parent line.</param>
	public sealed class GlobalCommercialInvoiceLineValidation(AutoGlobalCommercialInvoiceLine parent)
		: AutoGlobalCommercialInvoiceLineValidation(parent)
	{
		protected override void CheckGIL_GIH_Header()
		{
			base.CheckGIL_GIH_Header();
			MandatoryValidation.CheckEntered(Parent.GIL_GIH_HeaderInfo);
		}

		protected override void CheckGIL_LineNo()
		{
			base.CheckGIL_LineNo();
			MandatoryValidation.CheckEntered(Parent.GIL_LineNoInfo);
			MandatoryValidation.CheckNotZero(Parent.GIL_LineNoInfo);
			MandatoryValidation.CheckNotNegative(Parent.GIL_LineNoInfo);
		}

		protected override void CheckGIL_GrossWeightUQ()
		{
			base.CheckGIL_GrossWeightUQ();
			ListValidation.ErrorIfInvalidCode(Parent.GIL_GrossWeightUQInfo, Parent.Lookups.GrossWeightUQList);

			if (Parent.GIL_GrossWeight != 0)
			{
				MandatoryValidation.CheckEntered(Parent.GIL_GrossWeightUQInfo);
			}
		}

		protected override void CheckGIL_GrossWeight()
		{
			base.CheckGIL_GrossWeight();
			CompareValidation.CheckNumberNotNegative(Parent.GIL_GrossWeightInfo);
		}

		protected override void CheckGIL_NetWeightUQ()
		{
			base.CheckGIL_NetWeightUQ();
			ListValidation.ErrorIfInvalidCode(Parent.GIL_NetWeightUQInfo, Parent.Lookups.NetWeightUQList);

			if (Parent.GIL_NetWeight != 0)
			{
				MandatoryValidation.CheckEntered(Parent.GIL_NetWeightUQInfo);
			}
		}

		protected override void CheckGIL_NetWeight()
		{
			base.CheckGIL_NetWeight();
			CompareValidation.CheckNumberNotNegative(Parent.GIL_NetWeightInfo);
		}

		protected override void CheckGIL_VolumeUQ()
		{
			base.CheckGIL_VolumeUQ();
			ListValidation.ErrorIfInvalidCode(Parent.GIL_VolumeUQInfo, Parent.Lookups.VolumeUQList);

			if (Parent.GIL_Volume != 0)
			{
				MandatoryValidation.CheckEntered(Parent.GIL_VolumeUQInfo);
			}
		}

		protected override void CheckGIL_Volume()
		{
			base.CheckGIL_Volume();
			CompareValidation.CheckNumberNotNegative(Parent.GIL_VolumeInfo);
		}
	}
}
