using System;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(StocktakeAssignAllLinesToUserApplicator))]
	class StocktakeAssignAllLinesToUserApplicatorTest : LinesAssignerActionMethodApplicatorTest<StocktakeAssignAllLinesToUserApplicator, WhsStocktake>
	{
		protected override StocktakeAssignAllLinesToUserApplicator GetNewApplicator()
		{
			return new StocktakeAssignAllLinesToUserApplicator(Factory);
		}

		protected override Type ExpectedApplicatorValidationType => typeof(AssignAllLinesApplicatorValidation<WhsStocktake>);

		#region Test Operational Action

		#region TestAction Count1

		public void TestAction_Count1()
		{
			AssertStocktakeOperationalAction(1);
		}

		#endregion

		#region TestAction Count2

		public void TestAction_Count2()
		{
			AssertStocktakeOperationalAction(2);
		}

		#endregion

		#region TestAction Count3

		public void TestAction_Count3()
		{
			AssertStocktakeOperationalAction(3);
		}

		#endregion

		void AssertStocktakeOperationalAction(ZByte columnNumber)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var newStocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.New);
			var finalisedStocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Finalised);
			var loadedStocktakeWithNoEmptyDateVerfiedLines = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var loadedStocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);

			var openLineWithNoDateVerifiedByInLoadedStocktake = Helper.CreateWhsStocktakeLine(loadedStocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1m, ZDateTime.Empty, columnNumber, StocktakeLineStatus.Codes.Open);
			var openLineWithDateVerifiedByInLoadedStocktake = Helper.CreateWhsStocktakeLine(loadedStocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1m, ZDateTime.Now, columnNumber, StocktakeLineStatus.Codes.Open);
			var closedLineWithNoDateVerifiedByInLoadedStocktake = Helper.CreateWhsStocktakeLine(loadedStocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1m, ZDateTime.Empty, columnNumber, StocktakeLineStatus.Codes.Closed);
			var closedLineWithDateVerifiedByInLoadedStocktake = Helper.CreateWhsStocktakeLine(loadedStocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1m, ZDateTime.Now, columnNumber, StocktakeLineStatus.Codes.Closed);
			var closedLineWithNoDateVerifiedByInFinalisedStocktake = Helper.CreateWhsStocktakeLine(loadedStocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1m, ZDateTime.Empty, columnNumber, StocktakeLineStatus.Codes.Closed);
			var closedLineWithDateVerifiedByInFinalisedStocktake = Helper.CreateWhsStocktakeLine(loadedStocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1m, ZDateTime.Now, columnNumber, StocktakeLineStatus.Codes.Closed);
			var openLineInLoadedStocktakeWithNoEmptyDateVerfiedLines = Helper.CreateWhsStocktakeLine(loadedStocktakeWithNoEmptyDateVerfiedLines, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1m, ZDateTime.Now, columnNumber, StocktakeLineStatus.Codes.Open);

			var selectedUser = SelectedUser;

			Factory.Save();

			var errorMessage = string.Format("WARNING: Stocktake [HL {0}] - No Lines were assigned because the Stocktake is not Loaded.\n\r" +
											 "WARNING: Stocktake [HL {1}] - No Lines were assigned because the Stocktake is Finalized.\n\r" +
											 "INFO: Stocktake [HL {2}] - No Lines were assigned because there are no unassigned lines.\n\r" +
											 "INFO: Stocktake [HL {3}] - All lines have been assigned successfully.", newStocktake.WS_StocktakeNumber, finalisedStocktake.WS_StocktakeNumber, loadedStocktakeWithNoEmptyDateVerfiedLines.WS_StocktakeNumber, loadedStocktake.WS_StocktakeNumber);

			ApplyApplicator(new WhsStocktake[] { newStocktake, finalisedStocktake, loadedStocktakeWithNoEmptyDateVerfiedLines, loadedStocktake }, errorMessage);
			AssertEquals(selectedUser.GS_Code, openLineWithNoDateVerifiedByInLoadedStocktake.CurrentCountVerifiedBy);
			AssertEquals(ZString.Empty, openLineWithDateVerifiedByInLoadedStocktake.CurrentCountVerifiedBy);
			AssertEquals(ZString.Empty, closedLineWithNoDateVerifiedByInLoadedStocktake.CurrentCountVerifiedBy);
			AssertEquals(ZString.Empty, closedLineWithDateVerifiedByInLoadedStocktake.CurrentCountVerifiedBy);
			AssertEquals(ZString.Empty, closedLineWithNoDateVerifiedByInFinalisedStocktake.CurrentCountVerifiedBy);
			AssertEquals(ZString.Empty, closedLineWithDateVerifiedByInFinalisedStocktake.CurrentCountVerifiedBy);
		}

		#endregion
	}
}
