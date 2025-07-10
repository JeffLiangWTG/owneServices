using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public class RefCusProfileQuestionAnswerList : AutoRefCusProfileQuestionAnswerList
	{
		public RefCusProfileQuestionAnswerList(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(Question))]
		public override ZGuid XQ4_XQ2_Question { get => base.XQ4_XQ2_Question; set => base.XQ4_XQ2_Question = value; }

		public RefCusProfileQuestion Question => Factory.Load<RefCusProfileQuestion>(XQ4_XQ2_Question);
	}
}
