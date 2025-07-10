using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.TW.Business
{
	public class DocCusPackageCusPackableItemRelationCollection : DocumentWrapperCollection
	{
		public DocCusPackageCusPackableItemRelationCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public DocCusPackageCusPackableItemRelationCollection(IEnumerable<CusPackageCusPackableItemRelation> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocCusPackageCusPackableItemRelation this[int index] => (DocCusPackageCusPackableItemRelation)Elements[index];
	}
}
