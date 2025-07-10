using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusEntryCPDecLookups : Customs.Business.CusEntryCPDecLookups
	{
		public CusEntryCPDecLookups(AutoCusEntryCPDec parent) : base(parent)
		{
		}

		public CodeDescriptionPairList AnswerCodeList => Factory.GetCachedValue<Universal.CodeDescriptionPairLists.YesNoList>();

		public CodeDescriptionPairList QuestionTypeList => Factory.GetCachedValue<QuestionTypeList>();
	}
}
