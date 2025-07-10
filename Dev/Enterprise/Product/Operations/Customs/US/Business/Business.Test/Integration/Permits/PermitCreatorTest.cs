using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Moq;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PermitCreatorTest : TestCaseWithFactory
	{
		public void TestCreatePermitsByCombination()
		{
			var declaration = new PermitEntryLineGroupingTest().CreateEntryForPermitTest();
			var mock = new Mock<PermitEntryLineGrouping>(declaration);
			mock.Setup(m => m.IsDetailedTrackingEnabled).Returns(true);

			var permitGrouping = mock.Object;
			var lineGroups = permitGrouping.GetEntryLineGroups();
			var creator = new PermitCreator(declaration);

			creator.CreatePermits(lineGroups);
			declaration.Factory.Save();

			CusPermitHeaderCollection coll = new CusPermitHeaderCollection(declaration.Factory);
			AssertEquals("2 Permits should be created", 2, coll.Count);
			AssertEquals("Permit 1: StartDate", new ZDate(2017, 7, 7), coll[0].CPH_StartDate);
			AssertEquals("Permit 1: EndDate", new ZDate(2017, 7, 13), coll[0].CPH_EndDate);
			AssertEquals("Permit 2: StartDate", new ZDate(2017, 7, 7), coll[1].CPH_StartDate);
			AssertEquals("Permit 2: EndDate", new ZDate(2017, 7, 13), coll[1].CPH_EndDate);
			AssertEquals("Permit 1: ApplicationCode", "PER", coll[1].CPH_ApplicationCode);
		}

		public void TestCreatePermitsByTariff()
		{
			var declaration = new PermitEntryLineGroupingTest().CreateEntryForPermitTest();
			var mock = new Mock<PermitEntryLineGrouping>(declaration);
			mock.Setup(m => m.IsDetailedTrackingEnabled).Returns(false);

			var permitGrouping = mock.Object;
			var lineGroups = permitGrouping.GetEntryLineGroups();
			var creator = new PermitCreator(declaration);

			creator.CreatePermits(lineGroups);
			declaration.Factory.Save();

			CusPermitHeaderCollection coll = new CusPermitHeaderCollection(declaration.Factory);
			AssertEquals("1 Permits should be created", 1, coll.Count);
			AssertEquals("Permit 1: StartDate", new ZDate(2017, 7, 7), coll[0].CPH_StartDate);
			AssertEquals("Permit 1: EndDate", new ZDate(2017, 7, 13), coll[0].CPH_EndDate);
		}
	}
}
