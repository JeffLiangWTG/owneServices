using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class RelatedBusinessDataLookups
	{
		public RelatedBusinessDataLookups(RelatedBusinessData relatedBusinessData)
		{
			this.factory = relatedBusinessData.Factory;
		}
		readonly BusinessObjectFactory factory;

		public ImporterRelatedBusinessTypeList RelatedBusinessTypes
		{
			get { return factory.GetCachedValue<ImporterRelatedBusinessTypeList>(); }
		}
	}
}
