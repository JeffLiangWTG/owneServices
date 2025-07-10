using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class QuotaPermitNumberCusSupportingValidation : Customs.Business.CusSupportingInfoValidation
	{
		public QuotaPermitNumberCusSupportingValidation(QuotaPermitNumberCusSupporting parent) : base(parent)
		{
		}
		public new QuotaPermitNumberCusSupporting Parent => (QuotaPermitNumberCusSupporting)base.Parent;

		JobComInvoiceLine InvoiceLine => Parent.Parent;

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			var parent = Parent;
			CheckIfNotEnteredWhenIsTariffQuota(parent.CSI_ReferenceNumber, parent.CSI_ReferenceNumberInfo, Res.GetString("fae0599e-3520-4ee8-b4f6-7215aa2ca1ea", "The invoice line is applying for a tariff quota. Please enter a 'Quota Permit Number'."));
		}

		protected override void CheckCSI_LineNo()
		{
			base.CheckCSI_LineNo();
			var parent = Parent;
			CheckIfNotEnteredWhenIsTariffQuota(parent.CSI_LineNo, parent.CSI_LineNoInfo, Res.GetString("8e3d4cac-fe86-4cad-8c83-de3af983ed4a", "The invoice line is applying for a tariff quota. Please enter a 'Quota Permit Line Number'."));
		}

		void CheckIfNotEnteredWhenIsTariffQuota(IZType value, ZPropertyInfo targetInfo, string message)
		{
			if (value.IsEmpty && (InvoiceLine?.IsTariffQuota ?? false))
			{
				targetInfo.AddMessageError(message);
			}
		}
	}
}
