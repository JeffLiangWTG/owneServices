using System;
using Enterprise.Customs.Business.Interfaces;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusUnderbondUnionCollectionWithProvider : CusUnderbondUnionCollection
	{
		public CusUnderbondUnionCollectionWithProvider(ICusUnderbondUnionCollectionParent parent)
			: base(parent)
		{
		}

		public new CusUnderBondWithParentLoader this[int i]
		{
			get { return (CusUnderBondWithParentLoader)Elements[i]; }
		}

		public new CusUnderBondWithParentLoader AddNew()
		{
			return (CusUnderBondWithParentLoader)base.AddNew();
		}

		public new CusUnderBondWithParentLoader AddNew(Type bizOType)
		{
			return (CusUnderBondWithParentLoader)base.AddNew(bizOType);
		}
	}
}
