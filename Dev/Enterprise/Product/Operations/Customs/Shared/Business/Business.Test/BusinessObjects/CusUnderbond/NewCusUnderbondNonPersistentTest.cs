using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Interfaces;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(NewCusUnderbondNonPersistent))]
	sealed class NewCusUnderbondNonPersistentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUnderbondForList()
		{
			NewCusUnderbondNonPersistent bizo = (NewCusUnderbondNonPersistent)GetNewBusinessObject();
			AssertEquals("Count", 1, bizo.UnderbondForList.Count);
			AssertEquals("Code", "Dummy Underbond Biz Obj", bizo.UnderbondForList[0].Code);
			AssertEquals("Description", "Dummy Underbond Biz Obj", bizo.UnderbondForList[0].Description);
		}

		public void TestSelectedParent()
		{
			NewCusUnderbondNonPersistent bizo = (NewCusUnderbondNonPersistent)GetNewBusinessObject();
			AssertEquals("SelectedParentType", Dummy, bizo.SelectedParent);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NewCusUnderbondNonPersistent(Parent.GetAllPossibleCollectionProviders());
		}

		ICusUnderbondUnionCollectionParent parent;
		ICusUnderbondUnionCollectionParent Parent
		{
			get
			{
				if (parent == null)
				{
					parent = (ICusUnderbondUnionCollectionParent)Factory.New(typeof(DummyCusUnderbondUnionCollectionParent));
					((DummyCusUnderbondUnionCollectionParent)parent).AllPossibleCollectionProviders = DependentCollectionParents;
				}
				return parent;
			}
		}

		ICusUnderbondDependentCollectionParent[] dependentCollectionParents;
		ICusUnderbondDependentCollectionParent[] DependentCollectionParents
		{
			get
			{
				if (dependentCollectionParents == null)
				{
					dependentCollectionParents = new ICusUnderbondDependentCollectionParent[] { Dummy };
				}
				return dependentCollectionParents;
			}
		}

		DummyBizoWithUnderbondCollection dummy;
		DummyBizoWithUnderbondCollection Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<DummyBizoWithUnderbondCollection>();
				}
				return dummy;
			}
		}

		#endregion

	}
}
