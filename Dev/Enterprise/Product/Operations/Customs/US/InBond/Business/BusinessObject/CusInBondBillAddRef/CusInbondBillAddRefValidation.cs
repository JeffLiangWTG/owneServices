using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInbondBillAddRefValidation : Customs.Business.CusInbondBillAddRefValidation
	{
		public CusInbondBillAddRefValidation(CusInbondBillAddRef parent)
			: base(parent)
		{
		}

		protected SendingMessageValidationHelper Helper => new SendingMessageValidationHelper(Header, Parent);

		public override void ValidateAll()
		{
			var helper = Helper;
			if (!helper.IsArrivalValidationMode && !helper.IsExportationValidationMode)
			{
				base.ValidateAll();
			}
		}

		protected override void CheckBR_Qualifier()
		{
			if (!IsAirValidationModes)
			{
				base.CheckBR_Qualifier();
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BR_QualifierInfo);
				ValidateBR_ReferenceNum();
			}
		}

		protected override void CheckBR_ReferenceNum()
		{
			if (!IsAirValidationModes)
			{
				base.CheckBR_ReferenceNum();
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BR_ReferenceNumInfo);
			}
		}

		protected new CusInbondBillAddRef Parent
		{
			get { return (CusInbondBillAddRef)base.Parent; }
		}

		protected CusInBondHeader Header => Parent.Header;

		bool IsAirValidationModes
		{
			get
			{
				var header = Header;
				return header != null && header.IsAir;
			}
		}
	}
}
