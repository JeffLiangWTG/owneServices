using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public class RatingHeaderProcessTaskValidation : ProcessTaskValidation
	{
		public RatingHeaderProcessTaskValidation(ProcessTask parent)
			: base(parent)
		{ }

		protected override void CheckP9_OA()
		{
			//No need to check task address for quotation
		}
	}
}

