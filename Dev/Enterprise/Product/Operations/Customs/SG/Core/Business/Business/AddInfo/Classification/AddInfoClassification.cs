using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoClassification : AddInfo
	{
		public AddInfoClassification(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public new AddInfoClassificationLookups Lookups
		{
			get { return (AddInfoClassificationLookups)base.Lookups; }
		}

		public new AddInfoClassificationValidation Validation
		{
			get { return (AddInfoClassificationValidation)base.Validation; }
		}

		protected override SGAddInfoLookups GetNewLookups()
		{
			return new AddInfoClassificationLookups(this);
		}

		protected override SGAddInfoValidation GetNewValidation()
		{
			return new AddInfoClassificationValidation(this);
		}
	}
}
