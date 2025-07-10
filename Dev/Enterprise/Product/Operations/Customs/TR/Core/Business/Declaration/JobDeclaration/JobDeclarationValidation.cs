using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public partial class JobDeclarationValidation : AutoTRJobDeclarationValidation
	{
		public JobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateDischargeOffice();
			ValidateDischargePlace();
			ValidateEntryOffice();
			ValidateEntrySubStyle();
		}

		public void ValidateDischargeOffice()
		{
			ValidateCalculatedProperty(Parent.DischargeOfficeInfo);
		}

		protected void CheckDischargeOffice()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.DischargeOfficeInfo);
		}

		public void ValidateDischargePlace()
		{
			ValidateCalculatedProperty(Parent.DischargePlaceInfo);
		}

		protected void CheckDischargePlace()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.DischargePlaceInfo);
		}

		protected override void CheckJE_PaymentMethod()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_PaymentMethodInfo);
		}

		public void ValidateEntryOffice()
		{
			ValidateCalculatedProperty(Parent.EntryOfficeInfo);
		}

		protected void CheckEntryOffice()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.EntryOfficeInfo);
		}

		protected override void CheckJE_TransportModeInland()
		{
			base.CheckJE_TransportModeInland();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_TransportModeInlandInfo);
		}

		protected override void CheckJE_SubLocationOfGoods()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_SubLocationOfGoodsInfo);
		}

		public void ValidateBondedWarehouseCode()
		{
			ValidateCalculatedProperty(Parent.BondedWarehouseCodeInfo);
		}

		protected void CheckBondedWarehouseCode()
		{
			if (!Parent.BondedWarehouseCode.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.BondedWarehouseCodeInfo);
			}
		}

		protected override void CheckJE_ExportGoodsType()
		{
			base.CheckJE_ExportGoodsType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_ExportGoodsTypeInfo);
		}

		public void ValidateEntrySubStyle()
		{
			ValidateCalculatedProperty(Parent.JE_EntrySubStyleInfo);
		}

		protected void CheckJE_EntrySubStyle()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_EntrySubStyleInfo);
		}

		protected override void CheckJE_TransportMeans()
		{
			base.CheckJE_TransportMeans();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_TransportMeansInfo);
		}
	}
}
