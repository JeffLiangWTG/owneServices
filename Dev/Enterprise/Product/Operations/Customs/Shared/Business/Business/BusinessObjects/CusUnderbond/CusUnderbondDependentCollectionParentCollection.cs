using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;

namespace Enterprise.Customs.Business
{
	public class CusUnderbondUnionCollectionParentCollection : BusinessObjectCollection<BusinessObject>
	{
		public CusUnderbondUnionCollectionParentCollection(BusinessObjectFactory factory, Type typeOfElements) : base(factory)
		{
			fTypeOfElements = typeOfElements;
		}

		public CusUnderbondUnionCollectionParentCollection(BusinessObjectFactory factory) : this(factory, typeof(ICusUnderbondUnionCollectionParent))
		{
		}

		public new ICusUnderbondUnionCollectionParent this[int index]
		{
			get { return (ICusUnderbondUnionCollectionParent)Elements[index]; }
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return fTypeOfElements;
		}

		readonly Type fTypeOfElements;
	}
}
