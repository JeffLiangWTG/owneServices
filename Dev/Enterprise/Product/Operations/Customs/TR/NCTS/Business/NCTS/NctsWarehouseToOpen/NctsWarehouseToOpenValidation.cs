using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsWarehouseToOpenValidation : Customs.Business.CusSupportingInfoValidation
	{
		public NctsWarehouseToOpenValidation(NctsWarehouseToOpen parent) : base(parent)
		{
		}

		new NctsWarehouseToOpen Parent => (NctsWarehouseToOpen)base.Parent;
		
		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateIncoterm();
		}

		public void ValidateIncoterm()
		{
			ValidateCalculatedProperty(Parent.IncotermInfo);
		}

		protected void CheckIncoterm()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.IncotermInfo);
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
		}
		protected override void CheckCSI_UnitOfQuantity()
		{
			base.CheckCSI_UnitOfQuantity();
			if (Parent.CSI_Quantity > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_UnitOfQuantityInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_UnitOfQuantityInfo);
		}

		protected override void CheckCSI_ItemNumber()
		{
			base.CheckCSI_ItemNumber();
			if (Parent.CSI_ItemNumber == ZInt.Zero)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.CSI_ItemNumberInfo);
			}
		}
		protected override void CheckCSI_Procedure()
		{
			base.CheckCSI_Procedure();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_ProcedureInfo);
		}

		protected override void CheckCSI_RN_NKCountryCode()
		{
			base.CheckCSI_RN_NKCountryCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_RN_NKCountryCodeInfo);
		}

		protected override void CheckCSI_SubType()
		{
			base.CheckCSI_SubType();
			if (Parent.CSI_SubType.IsEmpty)
			{
				Parent.CSI_SubTypeInfo.AddMessageError(SubTypeEmpty);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_SubTypeInfo, Parent.Lookups.SubTypeList);
		}
		protected override void CheckCSI_RX_NKCurrency()
		{
			base.CheckCSI_RX_NKCurrency();
			if (Parent.CSI_Value > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_RX_NKCurrencyInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_RX_NKCurrencyInfo);
		}
		public string SubTypeEmpty => Res.GetString("1BE7A926-39DE-4C61-B4D5-1B584B2AD2C7", "You have not entered a Payment Type.");
	}
}
