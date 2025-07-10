using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RefCusProfileQuestionAttributeCollection : ActiveBusinessObjectCollection<RefCusProfileQuestionAttribute>
	{
		public RefCusProfileQuestionAttributeCollection(RefCusProfileQuestion refCusProfileQuestion)
			: base(refCusProfileQuestion.Factory, refCusProfileQuestion, new ZQuery(), RefCusProfileQuestionAttributeSchema.XQ3_XQ2_Question)
		{
		}
	}
}
