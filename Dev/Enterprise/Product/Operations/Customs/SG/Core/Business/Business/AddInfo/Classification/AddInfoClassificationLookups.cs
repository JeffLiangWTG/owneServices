
namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoClassificationLookups : SGAddInfoLookups
	{
		public AddInfoClassificationLookups(AddInfoClassification parent)
			: base(parent)
		{
		}

		public new AddInfoClassification Parent
		{
			get { return (AddInfoClassification)base.Parent; }
		}
	}
}
