using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	internal class USInBondMoveHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBranches()
		{
			AssertEquals(moveHeaderView.Header.Lookups.Branches.GetType(), moveHeaderView.Lookups.Branches.GetType());
			AssertEquals(moveHeaderView.Header.Lookups.Branches.Count, moveHeaderView.Lookups.Branches.Count);
		}

		public void TestImporters()
		{
			AssertEquals(moveHeaderView.Header.Lookups.Importers.GetType(), moveHeaderView.Lookups.Importers.GetType());
			AssertEquals(moveHeaderView.Header.Lookups.Importers.Count, moveHeaderView.Lookups.Importers.Count);
		}

		public void TestRegionDistrictPorts()
		{
			AssertEquals(moveHeaderView.MoveHeader.Lookups.RegionDistrictPorts.GetType(), moveHeaderView.Lookups.RegionDistrictPorts.GetType());
			AssertEquals(moveHeaderView.MoveHeader.Lookups.RegionDistrictPorts.Count, moveHeaderView.Lookups.RegionDistrictPorts.Count);
		}

		public void TestForeignPorts()
		{
			AssertEquals(moveHeaderView.MoveHeader.Lookups.ForeignPorts.GetType(), moveHeaderView.Lookups.ForeignPorts.GetType());
			AssertEquals(moveHeaderView.MoveHeader.Lookups.ForeignPorts.Count, moveHeaderView.Lookups.ForeignPorts.Count);
		}
		public void TestInbondTransactionStatusList()
		{
			AssertEquals(moveHeaderView.MoveHeader.Lookups.InbondQPMessageStatusList.GetType(), moveHeaderView.Lookups.InbondQPMessageStatusList.GetType());
			AssertEquals(moveHeaderView.MoveHeader.Lookups.InbondQPMessageStatusList.Count, moveHeaderView.Lookups.InbondQPMessageStatusList.Count);
		}

		public void TestInbondUpdateTransferofLiabilityStatusList()
		{
			AssertEquals(moveHeaderView.MoveHeader.Lookups.InbondWPMessageStatusList.GetType(), moveHeaderView.Lookups.InbondWPMessageStatusList.GetType());
			AssertEquals(moveHeaderView.MoveHeader.Lookups.InbondWPMessageStatusList.Count, moveHeaderView.Lookups.InbondWPMessageStatusList.Count);
		}

		public void TestCountries()
		{
			AssertEquals(moveHeaderView.Header.Lookups.Countries.GetType(), moveHeaderView.Lookups.Countries.GetType());
			AssertEquals(moveHeaderView.Header.Lookups.Countries.Count, moveHeaderView.Lookups.Countries.Count);
		}

		public void TestImportingConveyanceList()
		{
			AssertEquals(moveHeaderView.Header.Lookups.ImportingConveyanceList.GetType(), moveHeaderView.Lookups.ImportingConveyanceList.GetType());
			AssertEquals(moveHeaderView.Header.Lookups.ImportingConveyanceList.Count, moveHeaderView.Lookups.ImportingConveyanceList.Count);
		}

		public void TestScheduleKCodes()
		{
			AssertEquals(moveHeaderView.Header.Lookups.ScheduleKCodes.GetType(), moveHeaderView.Lookups.ScheduleKCodes.GetType());
			AssertEquals(moveHeaderView.Header.Lookups.ScheduleKCodes.Count, moveHeaderView.Lookups.ScheduleKCodes.Count);
		}

		public void TestTransportModeCodes()
		{
			AssertEquals(moveHeaderView.Header.Lookups.TransportModeCodes.GetType(), moveHeaderView.Lookups.TransportModeCodes.GetType());
			AssertEquals(moveHeaderView.Header.Lookups.TransportModeCodes.Count, moveHeaderView.Lookups.TransportModeCodes.Count);
		}

		public void TestEntryTypeList()
		{
			AssertEquals(moveHeaderView.MoveHeader.Lookups.EntryTypeList.GetType(), moveHeaderView.Lookups.EntryTypeList.GetType());
			AssertEquals(moveHeaderView.MoveHeader.Lookups.EntryTypeList.Count, moveHeaderView.Lookups.EntryTypeList.Count);
		}

		public void TestInBondCarriers()
		{
			AssertEquals(moveHeaderView.MoveHeader.Lookups.InBondCarriers.GetType(), moveHeaderView.Lookups.InBondCarriers.GetType());
			AssertEquals(moveHeaderView.MoveHeader.Lookups.InBondCarriers.Count, moveHeaderView.Lookups.InBondCarriers.Count);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			Factory.Save();

			moveHeaderView = Factory.Load<USInBondMoveHeader>(moveHeader.PK);
		}

		USInBondMoveHeader moveHeaderView;

		#endregion
	}
}
