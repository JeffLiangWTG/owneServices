using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RefCusProfileQuestionAnswerListCollection : ActiveBusinessObjectCollection<RefCusProfileQuestionAnswerList>
	{
		public RefCusProfileQuestionAnswerListCollection(RefCusProfileQuestion refCusProfileQuestion)
			: base(refCusProfileQuestion.Factory, refCusProfileQuestion, new ZQuery(), RefCusProfileQuestionAnswerListSchema.XQ4_XQ2_Question)
		{
		}
	}
}
