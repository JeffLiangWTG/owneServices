using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusPackageCusPackableItemRelationLookups : ZLookups
	{
		public CusPackageCusPackableItemRelationLookups(CusPackageCusPackableItemRelation parent) : base(parent)
		{
		}

		protected new CusPackageCusPackableItemRelation Parent => (CusPackageCusPackableItemRelation)base.Parent;

		public CodeDescriptionPairList PackTypes => Parent.PackableItem?.Lookups?.PackTypes ?? Factory.GetCachedValue<CodeDescriptionPairList>();

		public CodeDescriptionPairList WeightUQs => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);
	}
}
