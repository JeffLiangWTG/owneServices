using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public class RefCusProfileQuestionAttribute : AutoRefCusProfileQuestionAttribute
	{
		public RefCusProfileQuestionAttribute(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(Question))]
		public override ZGuid XQ3_XQ2_Question { get => base.XQ3_XQ2_Question; set => base.XQ3_XQ2_Question = value; }

		public RefCusProfileQuestion Question => Factory.Load<RefCusProfileQuestion>(XQ3_XQ2_Question);
	}
}
