using System.Collections;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Interfaces;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusUnderbondThatLinksToDummyBizo : CusUnderbond
	{
		public CusUnderbondThatLinksToDummyBizo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ICusUnderbondDependentCollectionParent[] GetAllPossibleCollectionProviders()
		{
			return (ICusUnderbondDependentCollectionParent[])allPossibleCollectionProviders.ToArray(typeof(ICusUnderbondDependentCollectionParent));
		}

		public void AddCollectionProvider(ICusUnderbondDependentCollectionParent collectionProvider)
		{
			allPossibleCollectionProviders.Add(collectionProvider);
		}

		protected override TypeLoaderCollection GetParentLoaders()
		{
			TypeLoaderCollection result = base.GetParentLoaders();
			result.Add(new TypeLoader(typeof(DummyBizoWithUnderbondCollection)));
			return result;
		}

		readonly ArrayList allPossibleCollectionProviders = new ArrayList();

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			C4_ApplicationCode = "T4T";
		}
	}
}
