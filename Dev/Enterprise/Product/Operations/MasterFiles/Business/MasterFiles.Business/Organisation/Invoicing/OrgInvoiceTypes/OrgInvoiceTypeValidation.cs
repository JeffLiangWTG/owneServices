using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgInvoiceTypeValidation : AutoOrgInvoiceTypeValidation
	{
		public OrgInvoiceTypeValidation(AutoOrgInvoiceType parent) : base(parent)
		{
		}

		new OrgInvoiceType Parent
		{
			get { return (OrgInvoiceType)base.Parent; }
		}

		public void ValidatePI_Calc_IsInclude()
		{
			ValidateCalculatedProperty(Parent.PI_Calc_IsIncludeInfo);
		}

		protected void CheckPI_Calc_IsInclude()
		{
			if (Parent.PI_Calc_IsInclude != InvoiceTypeChargeInclusionTypeList.Codes.ALL && Parent.DeferredCharges.Count == 0)
			{
				Parent.PI_Calc_IsIncludeInfo.AddError(Res.GetString("e6e6df5b-338b-4370-8c5d-438779664ed7", "Deferred Charges should be entered"));
			}
		}

		protected override void CheckPI_Module()
		{
			Parent.OrgInvoiceTypeLookupHelper.ValidateJobType();
		}

		protected override void CheckPI_Interval()
		{
			MandatoryValidation.CheckEntered(Parent.PI_IntervalInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PI_IntervalInfo);
		}

		protected override void CheckPI_Type()
		{
			MandatoryValidation.CheckEntered(Parent.PI_TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PI_TypeInfo);
			if (!Parent.PI_TypeInfo.HasErrors() &&
				Parent.PI_Module == InvoiceTypeModuleList.Codes.MSC &&
				Parent.PI_Type != InvoiceTypeLayoutList.Codes.CHG)
			{
				Parent.PI_TypeInfo.AddError(Res.GetString("DD7CEBFA-DDAA-4206-8E5E-18995D82A0D7", "The Layout must be '{0}' when the Module is '{1}'",
					InvoiceTypeLayoutList.Codes.CHG, InvoiceTypeModuleList.Codes.MSC));
			}
		}

		protected override void CheckPI_StartDay()
		{
			MandatoryValidation.CheckEntered(Parent.PI_StartDayInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PI_StartDayInfo);
		}

		protected override void CheckPI_TransportMode()
		{
			Parent.OrgInvoiceTypeLookupHelper.ValidateTransportMode();
		}

		protected override void CheckPI_ServiceDirection()
		{
			Parent.OrgInvoiceTypeLookupHelper.ValidateServiceDirection();
		}

		protected override void CheckPI_RS_NKServiceLevel()
		{
			Parent.OrgInvoiceTypeLookupHelper.ValidateServiceLevel();
		}

		protected override void CheckPI_SecondaryType()
		{
			MandatoryValidation.CheckEntered(Parent.PI_SecondaryTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PI_SecondaryTypeInfo);

			if (!Parent.PI_SecondaryTypeInfo.HasErrors() &&
				Parent.PI_Module == InvoiceTypeModuleList.Codes.MSC &&
				Parent.PI_SecondaryType != InvoiceTypeLayoutList.Codes.CHG)
			{
				Parent.PI_SecondaryTypeInfo.AddError(Res.GetString("7ba697bb-8bb4-42fc-9c71-1b26fcb4ef33", "The Secondary Layout must be '{0}' when the Module is '{1}'",
					InvoiceTypeLayoutList.Codes.CHG, InvoiceTypeModuleList.Codes.MSC));
			}
		}
	}
}
