using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusBondDetailValidation : Enterprise.MasterFiles.Business.CusBondDetailValidation
	{
		public CusBondDetailValidation(AutoCusBondDetail parent) : base(parent)
		{
		}

		protected new CusBondDetail Parent => (CusBondDetail)base.Parent;

		protected override void CheckPW_CPH_Guarantee()
		{
			base.CheckPW_CPH_Guarantee();
			ListValidation.ErrorIfInvalidPK(Parent.PW_CPH_GuaranteeInfo);
		}

		protected override void CheckPW_BondType()
		{
			base.CheckPW_BondType();
			ListValidation.MessageErrorIfInvalidCode(Parent.PW_BondTypeInfo);

			if (!IsDedicatedGuaranteeAmountEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.PW_BondTypeInfo);
			}
		}

		protected override void CheckPW_BondNumber()
		{
			base.CheckPW_BondNumber();

			var bondType = Parent.PW_BondType;
			if (!bondType.IsEmpty && bondType != GuaranteeTypeList.Codes.GLOBAL && bondType != GuaranteeTypeList.Codes.NAKIT)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.PW_BondNumberInfo);
			}
		}

		protected override void CheckPW_BondAmount()
		{
			base.CheckPW_BondAmount();

			if (!IsDedicatedGuaranteeAmountEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.PW_BondAmountInfo);
			}
		}

		bool IsDedicatedGuaranteeAmountEmpty => Parent.EntryInstruction?.ZG_DedicatedGuaranteeAmount.IsEmpty ?? true;
	}
}
