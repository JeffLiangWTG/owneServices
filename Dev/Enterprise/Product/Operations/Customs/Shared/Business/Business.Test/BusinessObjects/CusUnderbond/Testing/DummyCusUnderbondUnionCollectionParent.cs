using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Interfaces;

namespace Enterprise.Customs.Business.Testing
{
	public class DummyCusUnderbondUnionCollectionParent : DummyBusinessObject, ICusUnderbondUnionCollectionParent
	{
		public DummyCusUnderbondUnionCollectionParent(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		CusUnderbondUnionCollection underbonds;
		public CusUnderbondUnionCollection AllUnderbonds
		{
			get
			{
				if (underbonds == null)
				{
					underbonds = new CusUnderbondUnionCollectionWithProvider(this);
					underbonds.Load();
				}
				return underbonds;
			}
		}

		public ICusUnderbondDependentCollectionParent[] GetAllPossibleCollectionProviders()
		{
			return AllPossibleCollectionProviders;
		}

		public ICusUnderbondDependentCollectionParent[] AllPossibleCollectionProviders = Array.Empty<ICusUnderbondDependentCollectionParent>();

		public bool IsForAirCargo
		{
			get { return IsForAirCargoCore(); }
		}

		protected virtual bool IsForAirCargoCore()
		{
			return false;
		}
	}
}
