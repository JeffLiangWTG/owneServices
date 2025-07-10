using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NO.Business
{
	public class CusClassification : Customs.Business.BaseCusClassification
	{
		public CusClassification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusClassification Clone() => (CusClassification)base.Clone();

		public new CusClassificationLookups Lookups => (CusClassificationLookups)base.Lookups;

		public new CusClassificationValidation Validation => (CusClassificationValidation)base.Validation;

		protected override Customs.Business.CusClassificationLookups GetNewLookups() => new CusClassificationLookups(this);

		protected override Customs.Business.CusClassificationValidation GetNewValidation() => new CusClassificationValidation(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CC_ClassificationType = CusClassification.ClassificationType.Both;
		}
	}
}
