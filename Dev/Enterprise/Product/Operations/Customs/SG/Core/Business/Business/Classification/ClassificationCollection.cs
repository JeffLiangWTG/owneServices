using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business
{
	public class ClassificationCollection : Customs.Business.BaseClassificationCollection<Classification>
	{
		public ClassificationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery result = base.CreateAdditionalFilter();
			result.AddToFilter(CusClassificationSchema.CC_ClassificationType, SQLComparisonOperator.Equal, Classification.DefaultClassificationType);
			return result;
		}
	}
}
