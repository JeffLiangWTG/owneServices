using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobContainerPackPivot))]
	sealed class JobContainerPackPivotTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CommonShipment shipment = factory.New<CommonShipment>();
			CommonConsol consol = shipment.Consols.AddNew();
			CommonContainer container = consol.Containers.AddNew();
			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container);

			return packLine.Containers.GetRelationshipBusinessObject(container);
		}

		public void TestIsSavedByFactory()
		{
			var shipment = Factory.New<CommonShipment>();

			CommonContainer container = Factory.NewWithValidTestData<CommonContainer>();
			PackLine pack = shipment.OuterPackLines.AddNew();

			CommonContainer otherContainer = Factory.NewWithValidTestData<CommonContainer>();
			PackLine otherPack = shipment.OuterPackLines.AddNew();

			JobContainerPackPivot pivot = Factory.New<JobContainerPackPivot>();
			pivot.J6_JC = container.PK;
			pivot.J6_JL = pack.PK;
			AssertEquals("no duplicate in db", true, pivot.IsSavedByFactory);

			Factory.Save();
			AssertEquals("no duplicate in db", true, pivot.IsSavedByFactory);

			JobContainerPackPivot pivotNew = Factory.New<JobContainerPackPivot>();
			pivotNew.J6_JC = pivot.J6_JC;
			pivotNew.J6_JL = pivot.J6_JL;
			AssertEquals("duplicate of other pivot", false, pivotNew.IsSavedByFactory);

			Factory.Save(); // should not be able to save PivotNew
			AssertEquals("duplicate of other pivot, so don't save", false, pivotNew.IsInDatabase);

			pivotNew.J6_JC = otherContainer.PK;
			pivotNew.J6_JL = pivot.J6_JL;
			AssertEquals("should not be a duplicate of other pivot", true, pivotNew.IsSavedByFactory);

			pivotNew.J6_JC = pivot.J6_JC;
			pivotNew.J6_JL = otherPack.PK;
			AssertEquals("should not be a duplicate of other pivot", true, pivotNew.IsSavedByFactory);

			pivotNew.J6_JC = otherContainer.PK;
			pivotNew.J6_JL = otherPack.PK;
			AssertEquals("should not be a duplicate of other pivot", true, pivotNew.IsSavedByFactory);

			pivotNew.J6_JL = pivot.J6_JC;
			pivotNew.J6_JL = pivot.J6_JL;
			Factory.Save(); // should be able to save
		}

		[DeveloperOnlyTest]
		public override void TestFetchForLoad()
		{
			Assert("There should never be a case in production where a pivot object is loaded but the foreign key objects are not loaded, so fetch hints on such tables will never be unused.", true);
		}
	}
}
