using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ConsolTransportCollection))]
	sealed class ConsolTransportCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		#region TestOverrides

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Collection.AddNew();
			Collection.HasChanges = false;
			Assert("Precondition: Collection does not have changes", !Collection.HasChanges);

			IBindingList bindingList = Collection;

			if (bindingList.AllowNew)
			{
				BusinessObject @new = (BusinessObject)bindingList.AddNew();

				AssertEquals("New element added to collection", 2, Collection.Count);
				Assert("New element added to collection", bindingList.Contains(@new));

				((ICancelAddNew)Collection).CancelNew(Collection.Count - 1);

				AssertEquals("New element removed from the collection", 1, Collection.Count);
				AssertEquals("Has changes should be set to false.", false, Collection.HasChanges);
			}
		}

		#endregion

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CommonConsol parent = Factory.New<CommonConsol>();
			return new ConsolTransportCollection(parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<Transport>();
		}

		#endregion
	}
}
