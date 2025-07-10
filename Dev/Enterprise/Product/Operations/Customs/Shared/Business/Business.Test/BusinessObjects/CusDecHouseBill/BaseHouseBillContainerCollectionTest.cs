using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseBillContainerCollection))]
	public class BaseHouseBillContainerCollectionTest : BusinessObjectCollectionTestCase
	{
		[ExpectNoExceptions]
		public void TestAddNewGetsCorrectType()
		{
			BaseCusContainer container = Containers.AddNew();
		}

		public void TestTypedIndexerAndCount()
		{
			BaseCusContainer container = Containers.AddNew();
			AssertEquals("Collection Element Count", 1, Containers.Count);
			Assert("Indexer Returns Correct Type", typeof(BaseCusContainer).IsAssignableFrom(Containers[0].GetType()));
			AssertEquals("Typed Indexer Returns Element Just Added", container, Containers[0]);
		}

		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new BaseBillContainerCollection(HouseBill);
		}

		BaseBillContainerCollection fContainers;
		protected BaseBillContainerCollection Containers
		{
			get
			{
				if (fContainers == null)
				{
					fContainers = CreateContainers();
				}
				return fContainers;
			}
		}

		protected virtual BaseBillContainerCollection CreateContainers()
		{
			return new BaseBillContainerCollection(HouseBill);
		}

		Bill fHouseBill;
		protected Bill HouseBill
		{
			get
			{
				if (fHouseBill == null)
				{
					BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
					fHouseBill = declaration.Bills.AddNew();
				}
				return fHouseBill;
			}
		}
		#endregion
	}
}
