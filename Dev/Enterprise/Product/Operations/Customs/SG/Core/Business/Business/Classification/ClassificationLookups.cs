namespace Enterprise.Customs.SG.V4.Business
{
	public class ClassificationLookups : Customs.Business.CusClassificationLookups
	{
		public ClassificationLookups(Classification parent)
			: base(parent)
		{
		}

		protected new Classification Parent
		{
			get { return (Classification)base.Parent; }
		}
	}
}
