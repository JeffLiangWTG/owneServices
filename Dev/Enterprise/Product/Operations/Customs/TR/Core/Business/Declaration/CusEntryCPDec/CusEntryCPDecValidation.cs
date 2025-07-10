using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusEntryCPDecValidation : Customs.Business.CusEntryCPDecValidation
	{
		public CusEntryCPDecValidation(AutoCusEntryCPDec parent) : base(parent)
		{
		}

		public new CusEntryCPDec Parent => (CusEntryCPDec)base.Parent;

		protected override void CheckON_QuestionType()
		{
			base.CheckON_QuestionType();
			if (!Parent.ON_AnswerCode.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ON_QuestionTypeInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.ON_QuestionTypeInfo);
		}

		protected override void CheckON_CPDecNum()
		{
			base.CheckON_CPDecNum();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ON_CPDecNumInfo);
			MandatoryValidation.MessageErrorIfIsNegative(Parent.ON_CPDecNumInfo);
		}

		protected override void CheckON_AnswerCode()
		{
			base.CheckON_AnswerCode();
			if (Parent.ON_QuestionType == QuestionTypeList.Codes.Q)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.ON_AnswerCodeInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.ON_AnswerCodeInfo);
		}
	}
}
